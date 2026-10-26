namespace ForgottenSouls.Content.NPCs;

internal sealed class CrabGrabBuff : ModBuff
{
    public override string Texture => "Terraria/Images/Buff_" + BuffID.Webbed;

    public override void SetStaticDefaults()
    {
        Main.debuff[Type] = true;
        Main.buffNoSave[Type] = true;
        BuffID.Sets.NurseCannotRemoveDebuff[Type] = true;
    }

    public override void ModifyBuffText(ref string buffName, ref string tip, ref int rare)
    {
        buffName = "Схвачен";
        tip = "Краб держит вас за ноги";
    }

    public override void Update(Player player, ref int buffIndex)
    {
        player.velocity.X = 0f;
        if (player.velocity.Y < 0f)
            player.velocity.Y = 0f;

        player.controlLeft = false;
        player.controlRight = false;
        player.controlJump = false;
        player.controlHook = false;
        player.dashDelay = 1;
    }
}

internal sealed class CrabGrabNPC : GlobalNPC
{
    public const int GrabTicks = 120;
    private const int CooldownTicks = 8 * 60;

    public override bool InstancePerEntity => true;

    public override bool AppliesToEntity(NPC entity, bool lateInstantiation)
        => entity.type == NPCID.Crab;

    private int heldPlayer = -1;
    private int holdTicks;
    private uint nextGrabTick;

    public override void OnHitPlayer(NPC npc, Player target, Player.HurtInfo hurtInfo)
    {
        if (target.whoAmI != Main.myPlayer || Main.GameUpdateCount < nextGrabTick || target.HasBuff<CrabGrabBuff>())
            return;

        nextGrabTick = Main.GameUpdateCount + GrabTicks + CooldownTicks;
        target.AddBuff(ModContent.BuffType<CrabGrabBuff>(), GrabTicks);

        if (Main.netMode == NetmodeID.MultiplayerClient)
        {
            ModPacket packet = ModContent.GetInstance<ForgottenSouls>().GetPacket();
            packet.Write((byte)1);
            packet.Write((short)npc.whoAmI);
            packet.Write((byte)target.whoAmI);
            packet.Send();
        }
        else
        {
            StartHold(npc, target.whoAmI);
        }
    }

    public void StartHold(NPC npc, int playerIndex)
    {
        heldPlayer = playerIndex;
        holdTicks = GrabTicks;
        npc.netUpdate = true;
    }

    public override void PostAI(NPC npc)
    {
        if (Main.netMode == NetmodeID.MultiplayerClient || heldPlayer < 0)
            return;

        Player p = Main.player[heldPlayer];
        if (!p.active || p.dead || --holdTicks <= 0)
        {
            heldPlayer = -1;
            return;
        }

        npc.velocity = Vector2.Zero;
        npc.Center = new Vector2(p.Center.X, p.Bottom.Y - npc.height / 2f);

        if (holdTicks % 5 == 0)
            npc.netUpdate = true;
    }
}