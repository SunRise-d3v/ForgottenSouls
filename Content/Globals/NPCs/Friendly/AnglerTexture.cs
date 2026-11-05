namespace DisasterMod.Content.NPCs;

internal sealed class AnglerTexture : GlobalNPC
{
    public override bool AppliesToEntity(NPC entity, bool lateInstantiation)
    {
        return entity.type == NPCID.Angler || entity.type == NPCID.SleepingAngler;
    }

    public override bool PreDraw(NPC npc, SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
    {
        Asset<Texture2D> asset;

        if (npc.type == NPCID.SleepingAngler)
            asset = AnglerTextureSubstitution.Sleep;
        else if (BirthdayParty.PartyIsUp && AnglerTextureSubstitution.Party != null)
            asset = AnglerTextureSubstitution.Party;
        else
            asset = AnglerTextureSubstitution.Normal;

        if (asset == null || !asset.IsLoaded)
            return true;

        Texture2D texture = asset.Value;

        Rectangle frame = npc.frame;

        if (frame.Width <= 0 || frame.Height <= 0)
            frame = texture.Bounds;

        SpriteEffects effects = npc.spriteDirection == 1 ? SpriteEffects.FlipHorizontally : SpriteEffects.None;
        Vector2 origin = frame.Size() / 2f;

        Vector2 position = new(
            npc.Bottom.X - screenPos.X,
            npc.Bottom.Y - screenPos.Y - frame.Height * npc.scale / 2f + 4f + npc.gfxOffY);

        spriteBatch.Draw(texture, position, frame, npc.GetNPCColorTintedByBuffs(drawColor), npc.rotation, origin, npc.scale, effects, 0f);

        return false;
    }
}

internal sealed class AnglerTextureSubstitution : ModSystem
{
    internal static Asset<Texture2D> Normal;
    internal static Asset<Texture2D> Party;
    internal static Asset<Texture2D> Sleep;

    private static Asset<Texture2D> _origHead;
    private static int _headIndex = -1;

    public override void Load()
    {
        if (Main.dedServ)
            return;

        Normal = Request("Angler_Default");
        Party = Request("Angler_Default");
        Sleep = Request("Angler_Sleep");

        Asset<Texture2D> head = Request("Angler_Head");
        _headIndex = NPC.TypeToDefaultHeadIndex(NPCID.Angler);

        if (head != null && _headIndex >= 0 && _headIndex < TextureAssets.NpcHead.Length)
        {
            _origHead = TextureAssets.NpcHead[_headIndex];
            TextureAssets.NpcHead[_headIndex] = head;
        }
    }

    public override void Unload()
    {
        if (!Main.dedServ && _origHead != null && _headIndex >= 0 && _headIndex < TextureAssets.NpcHead.Length)
            TextureAssets.NpcHead[_headIndex] = _origHead;

        Normal = null;
        Party = null;
        Sleep = null;
        _origHead = null;
    }

    private static Asset<Texture2D> Request(string name)
    {
        try
        {
            string path = Helper.LoadNpcTexture("Angler", name);

            return ModContent.Request<Texture2D>(path, AssetRequestMode.ImmediateLoad);
        }
        catch (Exception)
        {
            return null;
        }
    }
}