namespace ForgottenSouls.Content.Recipes;

internal sealed record CustomRecipe(int Result)
{
    public int ResultStack { get; init; } = 1;
    public (int Type, int Stack)[] Items { get; init; } = [];
    public (string Group, int Stack)[] Groups { get; init; } = [];
    public int[] Tiles { get; init; } = [];
}
internal sealed partial class CustomRecipes : ModSystem
{
    public override void AddRecipes()
    {
        foreach (CustomRecipe def in Recipes)
        {
            Recipe recipe = Recipe.Create(def.Result, def.ResultStack);

            foreach (var (type, stack) in def.Items)
                recipe.AddIngredient(type, stack);

            foreach (var (group, stack) in def.Groups)
                recipe.AddRecipeGroup(group, stack);

            foreach (int tile in def.Tiles)
                recipe.AddTile(tile);

            recipe.Register();
        }
    }

    public override void PostAddRecipes()
    {
        foreach (Recipe recipe in Main.recipe)
        {
            if (recipe.Mod != Mod && RecipeRemovals.Results.Contains(recipe.createItem.type))
                recipe.DisableRecipe();
        }
    }

    public override void PostSetupContent()
    {
        // Swords
        DrawingPlayer.RequireDrawing(ItemID.Gladius);
        DrawingPlayer.RequireDrawing(ModContent.ItemType<TreeSword>());
        DrawingPlayer.RequireDrawing(ItemID.Flymeal);
        DrawingPlayer.RequireDrawing(ItemID.EnchantedSword);
        DrawingPlayer.RequireDrawing(ItemID.LightsBane);
        DrawingPlayer.RequireDrawing(ItemID.BloodButcherer);
        DrawingPlayer.RequireDrawing(ItemID.IceBlade);
        DrawingPlayer.RequireDrawing(ItemID.Terragrim);
        DrawingPlayer.RequireDrawing(ItemID.BoneSword);
        DrawingPlayer.RequireDrawing(ItemID.Muramasa);
        DrawingPlayer.RequireDrawing(ItemID.Starfury);
        DrawingPlayer.RequireDrawing(ItemID.GreenPhaseblade);
        DrawingPlayer.RequireDrawing(ItemID.BluePhaseblade);
        DrawingPlayer.RequireDrawing(ItemID.PurplePhaseblade);
        DrawingPlayer.RequireDrawing(ItemID.PurplePhasesaber);
        DrawingPlayer.RequireDrawing(ItemID.OrangePhaseblade);
        DrawingPlayer.RequireDrawing(ItemID.YellowPhaseblade);
        DrawingPlayer.RequireDrawing(ItemID.RedPhaseblade);
        DrawingPlayer.RequireDrawing(ItemID.BluePhasesaber);
        DrawingPlayer.RequireDrawing(ItemID.GreenPhasesaber);
        DrawingPlayer.RequireDrawing(ItemID.RedPhasesaber);
        DrawingPlayer.RequireDrawing(ItemID.OrangePhasesaber);
        DrawingPlayer.RequireDrawing(ItemID.YellowPhasesaber);
        DrawingPlayer.RequireDrawing(ItemID.WhitePhasesaber);
        DrawingPlayer.RequireDrawing(ItemID.WhitePhaseblade);
        DrawingPlayer.RequireDrawing(ItemID.FieryGreatsword);
        DrawingPlayer.RequireDrawing(ItemID.NightsEdge);
        DrawingPlayer.RequireDrawing(ItemID.Frostbrand);
        DrawingPlayer.RequireDrawing(ItemID.ChlorophyteSaber);
        DrawingPlayer.RequireDrawing(ItemID.ChlorophyteClaymore);
        DrawingPlayer.RequireDrawing(ItemID.BeamSword);
        DrawingPlayer.RequireDrawing(ItemID.Cutlass);
        DrawingPlayer.RequireDrawing(ItemID.TrueNightsEdge);
        DrawingPlayer.RequireDrawing(ItemID.Excalibur);
        DrawingPlayer.RequireDrawing(ItemID.TrueExcalibur);
        DrawingPlayer.RequireDrawing(ItemID.PsychoKnife);
        DrawingPlayer.RequireDrawing(ItemID.TerraBlade);
        DrawingPlayer.RequireDrawing(ItemID.ChristmasTreeSword);
        DrawingPlayer.RequireDrawing(ItemID.DD2SquireDemonSword);
        DrawingPlayer.RequireDrawing(ItemID.InfluxWaver);
        DrawingPlayer.RequireDrawing(ItemID.Keybrand);
        DrawingPlayer.RequireDrawing(ItemID.TheHorsemansBlade);
        DrawingPlayer.RequireDrawing(ItemID.Zenith);
        DrawingPlayer.RequireDrawing(ModContent.ItemType<MissDrakovisFishingPole>());
        DrawingPlayer.RequireDrawing(ModContent.ItemType<SlimeSlingingSlasher>());
        DrawingPlayer.RequireDrawing(ModContent.ItemType<Penetrator>());
        DrawingPlayer.RequireDrawing(ModContent.ItemType<SlimeRain>());

        // Spears
        DrawingPlayer.RequireDrawing(ItemID.Trident);
        DrawingPlayer.RequireDrawing(ItemID.ThunderSpear);
        DrawingPlayer.RequireDrawing(ItemID.TheRottedFork);
        DrawingPlayer.RequireDrawing(ModContent.ItemType<TheLightningRod>());
        DrawingPlayer.RequireDrawing(ItemID.PalladiumPike);
        DrawingPlayer.RequireDrawing(ItemID.CobaltNaginata);
        DrawingPlayer.RequireDrawing(ItemID.MonkStaffT2);
        DrawingPlayer.RequireDrawing(ItemID.MythrilHalberd);
        DrawingPlayer.RequireDrawing(ItemID.OrichalcumHalberd);
        DrawingPlayer.RequireDrawing(ItemID.TitaniumTrident);
        DrawingPlayer.RequireDrawing(ItemID.AdamantiteGlaive);
        DrawingPlayer.RequireDrawing(ItemID.MonkStaffT1);
        DrawingPlayer.RequireDrawing(ItemID.Bladetongue);
        DrawingPlayer.RequireDrawing(ItemID.FetidBaghnakhs);
        DrawingPlayer.RequireDrawing(ItemID.Gungnir);
        DrawingPlayer.RequireDrawing(ItemID.NorthPole);
        DrawingPlayer.RequireDrawing(ItemID.DayBreak);
        DrawingPlayer.RequireDrawing(ModContent.ItemType<UmbraRegalia>());

        // Flails
        DrawingPlayer.RequireDrawing(ItemID.BallOHurt);
        DrawingPlayer.RequireDrawing(ItemID.TheMeatball);
        DrawingPlayer.RequireDrawing(ItemID.BlueMoon);
        DrawingPlayer.RequireDrawing(ItemID.Sunfury);
        DrawingPlayer.RequireDrawing(ItemID.DaoofPow);
        DrawingPlayer.RequireDrawing(ItemID.ChainGuillotines);
        DrawingPlayer.RequireDrawing(ItemID.Anchor);
        DrawingPlayer.RequireDrawing(ItemID.SolarEruption);
        DrawingPlayer.RequireDrawing(ModContent.ItemType<LeashOfCthulhu>());
        DrawingPlayer.RequireDrawing(ModContent.ItemType<PhantasmalLeashOfCthulhu>());

        // Yo-yos
        DrawingPlayer.RequireDrawing(ItemID.CorruptYoyo);
        DrawingPlayer.RequireDrawing(ItemID.CrimsonYoyo);
        DrawingPlayer.RequireDrawing(ItemID.HiveFive);
        DrawingPlayer.RequireDrawing(ItemID.JungleYoyo);
        DrawingPlayer.RequireDrawing(ItemID.Cascade);
        DrawingPlayer.RequireDrawing(ItemID.Valor);
        DrawingPlayer.RequireDrawing(ItemID.Chik);
        DrawingPlayer.RequireDrawing(ItemID.Code2);
        DrawingPlayer.RequireDrawing(ItemID.Yelets);
        DrawingPlayer.RequireDrawing(ItemID.Kraken);
        DrawingPlayer.RequireDrawing(ItemID.TheEyeOfCthulhu);
        DrawingPlayer.RequireDrawing(ModContent.ItemType<Blender>());

        // Boomerangs
        DrawingPlayer.RequireDrawing(ItemID.EnchantedBoomerang);
        DrawingPlayer.RequireDrawing(ItemID.IceBoomerang);
        DrawingPlayer.RequireDrawing(ItemID.Shroomerang);
        DrawingPlayer.RequireDrawing(ItemID.Flamarang);
        DrawingPlayer.RequireDrawing(ItemID.Trimarang);
        DrawingPlayer.RequireDrawing(ItemID.LightDisc);
        DrawingPlayer.RequireDrawing(ItemID.BouncingShield);
        DrawingPlayer.RequireDrawing(ItemID.PaladinsHammer);
        DrawingPlayer.RequireDrawing(ModContent.ItemType<GeminiGlaives>());
        DrawingPlayer.RequireDrawing(ModContent.ItemType<SparklingLove>());

        // Scythes
        DrawingPlayer.RequireDrawing(ItemID.IceSickle);
        DrawingPlayer.RequireDrawing(ItemID.DeathSickle);
        DrawingPlayer.RequireDrawing(ModContent.ItemType<StyxGazer>());

        // Lances
        DrawingPlayer.RequireDrawing(ItemID.ShadowJoustingLance);
    }
}