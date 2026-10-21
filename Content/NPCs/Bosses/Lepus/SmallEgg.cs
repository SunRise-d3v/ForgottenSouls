namespace ForgottenSouls.Content.Entities;

internal class SmallEgg : ModNPC
{
    public override string Texture => Helper.TextureLoad("NPCs/Lepus", "SmallEgg");
    
    private int timer = 0;

    public override void SetDefaults()
    {
        NPC.friendly = false;
        NPC.width = 22;
        NPC.height = 24;
        NPC.aiStyle = NPCAIStyleID.FaceClosestPlayer;
        NPC.damage = 0;
        NPC.defense = 3;
        NPC.lifeMax = 65;
        NPC.HitSound = SoundID.NPCHit1;
        NPC.DeathSound = SoundID.NPCDeath1;
        NPC.knockBackResist = 0f;
        NPC.noTileCollide = false;
    }

    public override void AI()
    {
        if (Main.netMode != NetmodeID.MultiplayerClient)
        {
            NPC.homeless = false;
            NPC.homeTileX = -1;
            NPC.homeTileY = -1;
            NPC.netUpdate = true;
        }

        NPC.spriteDirection = 0;
        NPC.velocity.X = 0f;
        NPC.velocity.Y = 5f;
        timer++;

        if (timer >= 360)
        {
            Gore.NewGore(NPC.GetSource_FromAI(), NPC.position, NPC.velocity, Mod.Find<ModGore>("EggShell").Type);
            Gore.NewGore(NPC.GetSource_FromAI(), NPC.position, NPC.velocity, Mod.Find<ModGore>("EggShell").Type);
            NPC.Transform(ModContent.NPCType<DisasterBunny>());
        }
    }

    public override void HitEffect(NPC.HitInfo hit)
    {
        if (NPC.life <= 0)
        {
            SoundEngine.PlaySound(SoundID.NPCHit2, NPC.position);
            Gore.NewGore(NPC.GetSource_FromAI(), NPC.position, NPC.velocity, Mod.Find<ModGore>("EggShell").Type, 1f);
            Gore.NewGore(NPC.GetSource_FromAI(), NPC.position, NPC.velocity, Mod.Find<ModGore>("EggShell").Type, 1f);
        }
    }
}