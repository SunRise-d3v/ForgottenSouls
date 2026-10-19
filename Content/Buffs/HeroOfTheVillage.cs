namespace NoCompromises.Content.Buffs;

internal abstract class HeroOfTheVillage : ModBuff
{
    public override string Texture => null;

    public override void SetStaticDefaults()
    {
        Main.debuff[Type] = false;
        Main.buffNoSave[Type] = false;
        Main.buffNoTimeDisplay[Type] = true;
    }

    public override void Update(NPC npc, ref int buffIndex) { }
}

internal class HeroVillage1 : HeroOfTheVillage
{
    public override string Texture => "NoCompromises/Assets/Textures/Buffs/HeroOfTheVillage1";

    public override void SetStaticDefaults()
    {
        base.SetStaticDefaults();
    }
}

internal class HeroVillage2 : HeroOfTheVillage
{
    public override string Texture => "NoCompromises/Assets/Textures/Buffs/HeroOfTheVillage2";

    public override void SetStaticDefaults()
    {
        base.SetStaticDefaults();
    }
}

internal class HeroVillage3 : HeroOfTheVillage
{
    public override string Texture => "NoCompromises/Assets/Textures/Buffs/HeroOfTheVillage3";

    public override void SetStaticDefaults()
    {
        base.SetStaticDefaults();
    }
}

internal class HeroVillage4 : HeroOfTheVillage
{
    public override string Texture => "NoCompromises/Assets/Textures/Buffs/HeroOfTheVillage4";

    public override void SetStaticDefaults()
    {
        base.SetStaticDefaults();
    }
}