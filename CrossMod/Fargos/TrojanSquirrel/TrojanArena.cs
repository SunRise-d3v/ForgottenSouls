namespace CrossMod;

internal class TrojanArenaTrigger : GlobalNPC
{
    public override bool InstancePerEntity => true;

    public override bool AppliesToEntity(NPC entity, bool lateInstantiation)
        => entity.type == ModContent.NPCType<TrojanSquirrel>();

    private bool hasArea;
    private Rectangle area;

    public override void OnSpawn(NPC npc, IEntitySource source)
    {
        if (Main.netMode == NetmodeID.MultiplayerClient)
            return;

        Player player = Main.player[Player.FindClosest(npc.Center, 0, 0)];

        if (!player.active || player.dead)
            return;

        area = TrojanArenaSystem.Build(player.Center.ToTileCoordinates());
        hasArea = true;
        npc.netUpdate = true;
    }

    public override void SendExtraAI(NPC npc, BitWriter bitWriter, BinaryWriter binaryWriter)
    {
        binaryWriter.Write(hasArea);

        if (!hasArea)
            return;

        binaryWriter.Write(area.X);
        binaryWriter.Write(area.Y);
        binaryWriter.Write(area.Width);
        binaryWriter.Write(area.Height);
    }

    public override void ReceiveExtraAI(NPC npc, BitReader bitReader, BinaryReader binaryReader)
    {
        hasArea = binaryReader.ReadBoolean();

        if (!hasArea)
            return;

        area = new Rectangle(binaryReader.ReadInt32(), binaryReader.ReadInt32(), binaryReader.ReadInt32(), binaryReader.ReadInt32());
        TrojanArenaSystem.SetArea(area);
    }
}

internal class TrojanArenaSystem : ModSystem
{
    private const int Size = 125;       
    private const int Thickness = 2;   
    private const int WorldMargin = 50;   
    private const int AbsentTicks = 60; 

    private int _absent;

    public static Rectangle? Area { get; private set; }

    public static Rectangle? InnerPixels
        => Area is Rectangle a
            ? new Rectangle((a.X + Thickness) * 16, (a.Y + Thickness) * 16, (a.Width - Thickness * 2) * 16, (a.Height - Thickness * 2) * 16)
            : null;

    public static Rectangle? OuterPixels
        => Area is Rectangle a ? new Rectangle(a.X * 16, a.Y * 16, a.Width * 16, a.Height * 16) : null;

    public static bool Contains(Vector2 worldPosition)
        => Area is Rectangle area && area.Contains(worldPosition.ToTileCoordinates());

    internal static Rectangle Build(Point center)
    {
        if (Area is Rectangle existing)
            return existing;

        int left = Math.Clamp(center.X - Size / 2, WorldMargin, Main.maxTilesX - Size - WorldMargin);
        int top = Math.Clamp(center.Y - Size / 2, WorldMargin, Main.maxTilesY - Size - WorldMargin);

        Area = new Rectangle(left, top, Size, Size);
        return Area.Value;
    }

    internal static void SetArea(Rectangle area) => Area = area;

    public override void PostUpdateWorld()
    {
        if (Area == null)
            return;

        if (NPC.AnyNPCs(ModContent.NPCType<TrojanSquirrel>()))
        {
            _absent = 0;
            return;
        }

        if (++_absent >= AbsentTicks)
        {
            Area = null;
            _absent = 0;
        }
    }

    public override void ClearWorld()
    {
        Area = null;
        _absent = 0;
    }

    public override void PostDrawTiles()
    {
        if (Area is not Rectangle area)
            return;

        Rectangle screen = new(
            (int)(Main.screenPosition.X / 16f) - 1,
            (int)(Main.screenPosition.Y / 16f) - 1,
            Main.screenWidth / 16 + 3,
            Main.screenHeight / 16 + 3);

        Rectangle visible = Rectangle.Intersect(area, screen);

        if (visible.IsEmpty)
            return;

        Main.instance.LoadTiles(TileID.WoodBlock);
        Texture2D texture = TextureAssets.Tile[TileID.WoodBlock].Value;
        Rectangle frame = new(18, 18, 16, 16);

        Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);

        for (int x = visible.Left; x < visible.Right; x++)
        {
            for (int y = visible.Top; y < visible.Bottom; y++)
            {
                bool border = x < area.Left + Thickness || x >= area.Right - Thickness
                    || y < area.Top + Thickness || y >= area.Bottom - Thickness;

                if (!border)
                    continue;

                Vector2 position = new Vector2(x * 16f, y * 16f) - Main.screenPosition;
                Main.spriteBatch.Draw(texture, position, frame, Lighting.GetColor(x, y));
            }
        }

        Main.spriteBatch.End();
    }
}

internal class TrojanArenaPlayer : ModPlayer
{
    public bool Shocked;

    private bool _inside;
    private bool _hadArena;

    public override void PostUpdate()
    {
        if (Player.whoAmI != Main.myPlayer)
            return;

        Rectangle? area = TrojanArenaSystem.Area;

        Shocked = area != null && TrojanArenaSystem.Contains(Player.Center);

        if (Shocked && !Player.dead)
            Player.AddBuff(BuffID.NoBuilding, 2);

        if (area == null || Player.dead)
        {
            _hadArena = false;
            _inside = false;
            return;
        }

        if (!_hadArena)
        {
            _hadArena = true;
            _inside = Shocked;
        }

        Rectangle inner = TrojanArenaSystem.InnerPixels.Value;

        if (_inside)
            KeepInside(inner);
        else
            KeepOutside(TrojanArenaSystem.OuterPixels.Value);
    }

    private void KeepInside(Rectangle inner)
    {
        if (Player.position.X < inner.Left)
        {
            Player.position.X = inner.Left;
            Player.velocity.X = Math.Max(0f, Player.velocity.X);
        }
        else if (Player.position.X + Player.width > inner.Right)
        {
            Player.position.X = inner.Right - Player.width;
            Player.velocity.X = Math.Min(0f, Player.velocity.X);
        }

        if (Player.position.Y < inner.Top)
        {
            Player.position.Y = inner.Top;
            Player.velocity.Y = Math.Max(0f, Player.velocity.Y);
        }
        else if (Player.position.Y + Player.height > inner.Bottom)
        {
            Player.position.Y = inner.Bottom - Player.height;
            Player.velocity.Y = Math.Min(0f, Player.velocity.Y);
        }
    }

    private void KeepOutside(Rectangle outer)
    {
        if (!Player.Hitbox.Intersects(outer))
            return;

        float toLeft = Player.Right.X - outer.Left;
        float toRight = outer.Right - Player.Left.X;
        float toTop = Player.Bottom.Y - outer.Top;
        float toBottom = outer.Bottom - Player.Top.Y;

        float min = Math.Min(Math.Min(toLeft, toRight), Math.Min(toTop, toBottom));

        if (min == toLeft)
        {
            Player.position.X = outer.Left - Player.width;
            Player.velocity.X = Math.Min(0f, Player.velocity.X);
        }
        else if (min == toRight)
        {
            Player.position.X = outer.Right;
            Player.velocity.X = Math.Max(0f, Player.velocity.X);
        }
        else if (min == toTop)
        {
            Player.position.Y = outer.Top - Player.height;
            Player.velocity.Y = Math.Min(0f, Player.velocity.Y);
        }
        else
        {
            Player.position.Y = outer.Bottom;
            Player.velocity.Y = Math.Max(0f, Player.velocity.Y);
        }
    }
}

internal class HideCreativeShock : GlobalBuff
{
    public override bool PreDraw(SpriteBatch spriteBatch, int type, int buffIndex, ref BuffDrawParams drawParams)
        => !(type == BuffID.NoBuilding && Main.LocalPlayer.GetModPlayer<TrojanArenaPlayer>().Shocked);
}