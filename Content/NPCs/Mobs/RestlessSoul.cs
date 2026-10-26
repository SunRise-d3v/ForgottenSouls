namespace ForgottenSouls.Content.Entities;

internal sealed class RestlessSoul : ModNPC
{
    public override string Texture => Helper.TextureLoadNPC("RestlessSoul");
#if false

    private const float InitialSpeed = 4f;
    private const float MaxSpeed = 18f;
    private const float Acceleration = 0.15f;
    private const int RiseTime = 20;

    private int _timer;

    public override void SetDefaults()
    {
        NPC.width = 16;
        NPC.height = 26;

        NPC.damage = 40;
        NPC.lifeMax = 1;

        NPC.friendly = false;
        NPC.dontTakeDamageFromHostiles = true;
        NPC.dontTakeDamageFromHostiles = true;
        NPC.dontTakeDamage = true;

        NPC.noGravity = true;
        NPC.noTileCollide = true;
        NPC.aiStyle = -1;
        NPC.knockBackResist = 0f;
    }

    public override void AI()
    {
        if (NPC.ai[2] == 0f &&
            Main.netMode != NetmodeID.MultiplayerClient)
        {
            int target = Player.FindClosest(NPC.Center, 1, 1);

            if (target >= 0 && target < Main.maxPlayers)
            {
                Player player = Main.player[target];

                if (player.active && !player.dead)
                {
                    NPC.ai[0] = player.Center.X;
                    NPC.ai[1] = player.Center.Y;
                    NPC.ai[2] = 1f;
                    NPC.netUpdate = true;
                }
            }
        }

        _timer++;

        if (_timer <= RiseTime)
        {
            NPC.velocity = new Vector2(0f, -InitialSpeed);
        }
        else
        {
            Vector2 targetPosition = new(
                NPC.ai[0],
                NPC.ai[1]);

            Vector2 direction = targetPosition - NPC.Center;

            if (direction.LengthSquared() > 1f)
                direction.Normalize();

            float speed = MathHelper.Clamp(
                NPC.velocity.Length() + Acceleration,
                InitialSpeed,
                MaxSpeed);

            NPC.velocity = direction * speed;
        }

        if (NPC.velocity.LengthSquared() > 0.01f)
            NPC.rotation = NPC.velocity.ToRotation();

        foreach (Player player in Main.player)
        {
            if (player.active &&
                !player.dead &&
                NPC.Hitbox.Intersects(player.Hitbox))
            {
                Explode();
                return;
            }
        }

        if (CollidesWithSolidTiles())
        {
            Explode();
            return;
        }
    }

    public override bool? CanBeHitByItem(
        Player player, Item item)
    {
        return false;
    }

    public override bool? CanBeHitByProjectile(
        Projectile projectile)
    {
        return false;
    }

    public override bool CheckDead()
    {
        return false;
    }

    private void Explode()
    {
        if (!NPC.active)
            return;

        NPC.active = false;
        NPC.netUpdate = true;
    }

    private bool CollidesWithSolidTiles()
    {
        Rectangle hitbox = NPC.Hitbox;

        int left = hitbox.Left / 16;
        int right = (hitbox.Right - 1) / 16;
        int top = hitbox.Top / 16;
        int bottom = (hitbox.Bottom - 1) / 16;

        for (int x = left; x <= right; x++)
        {
            for (int y = top; y <= bottom; y++)
            {
                if (!WorldGen.InWorld(x, y))
                    continue;

                Tile tile = Main.tile[x, y];

                if (tile.HasTile &&
                    Main.tileSolid[tile.TileType] &&
                    !Main.tileSolidTop[tile.TileType] &&
                    !tile.IsActuated)
                {
                    return true;
                }
            }
        }

        return false;
    }
}

internal sealed class RestlessSoulGlobalNPC : GlobalNPC
{
    public override void OnKill(NPC npc)
    {
        if (Main.netMode == NetmodeID.MultiplayerClient)
            return;

        if (npc.type == ModContent.NPCType<RestlessSoul>())
            return;

        if (npc.friendly || npc.townNPC || npc.lifeMax <= 10)
            return;

        if (npc.boss == true)
            return;

        if (Main.rand.NextFloat() >= 0.4f)
            return;

        NPC.NewNPC(
            npc.GetSource_Death(),
            (int)npc.Center.X,
            (int)npc.Center.Y,
            ModContent.NPCType<RestlessSoul>());
    }
#endif
}