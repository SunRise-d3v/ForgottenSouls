namespace ForgottenSouls.Content;

internal enum ChosenClass { None, Melee, Ranged, Magic, Summoner }
internal class ClassChoicePlayer : ModPlayer
{
    private const float BonusDamage = 0.20f;  
    private const float BonusCrit = 8f;       
    private const float PenaltyDamage = 0.10f; 
    private const float PenaltyCrit = 4f;   

    private static readonly (ChosenClass Class, DamageClass Damage)[] Classes =
    [
        (ChosenClass.Melee, DamageClass.Melee),
        (ChosenClass.Ranged, DamageClass.Ranged),
        (ChosenClass.Magic, DamageClass.Magic),
        (ChosenClass.Summoner, DamageClass.Summon),
    ];

    public ChosenClass Chosen;

    public override void SaveData(TagCompound tag)
    {
        if (Chosen != ChosenClass.None)
            tag["ChosenClass"] = (int)Chosen;
    }

    public override void LoadData(TagCompound tag)
    {
        Chosen = (ChosenClass)tag.GetInt("ChosenClass");
    }

    public override void PostUpdateEquips()
    {
        if (Chosen == ChosenClass.None)
            return;

        foreach ((ChosenClass chosenClass, DamageClass damage) in Classes)
        {
            if (chosenClass == Chosen)
            {
                Player.GetDamage(damage) += BonusDamage;
                Player.GetCritChance(damage) += BonusCrit;
            }
            else
            {
                Player.GetDamage(damage) -= PenaltyDamage;
                Player.GetCritChance(damage) -= PenaltyCrit;
            }
        }

        switch (Chosen)
        {
            case ChosenClass.Melee:
                Player.GetAttackSpeed(DamageClass.Melee) += 0.10f;
                break;

            case ChosenClass.Ranged:
                Player.GetAttackSpeed(DamageClass.Ranged) += 0.10f;
                break;

            case ChosenClass.Magic:
                Player.statManaMax2 += 60;
                break;

            case ChosenClass.Summoner:
                Player.maxMinions += 1;
                break;
        }
    }
}

internal class ClassChoiceItem : ModItem
{
    public override string Texture => "Terraria/Images/Item_" + ItemID.AvengerEmblem;

    public override void SetDefaults()
    {
        Item.width = 28;
        Item.height = 28;
        Item.maxStack = 9999;
        Item.rare = ItemRarityID.Expert;
        Item.useStyle = ItemUseStyleID.HoldUp;
        Item.useTime = 20;
        Item.useAnimation = 20;
        Item.consumable = false; 
        Item.UseSound = SoundID.Item4;
    }

    public override bool? UseItem(Player player)
    {
        if (player.whoAmI == Main.myPlayer)
            ClassChoiceSystem.Toggle();

        return true;
    }
}

internal class ClassChoiceDrop : GlobalNPC
{
    public override void ModifyNPCLoot(NPC npc, NPCLoot npcLoot)
    {
        if (npc.type == NPCID.MoonLordCore)
            npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<ClassChoiceItem>()));
    }
}

internal class ClassChoiceSystem : ModSystem
{
    private static UserInterface _interface;
    private static ClassChoiceUI _state;
    private static GameTime _lastGameTime;

    public override void Load()
    {
        if (Main.dedServ)
            return;

        _state = new ClassChoiceUI();
        _state.Activate();
        _interface = new UserInterface();
    }

    public override void Unload()
    {
        _interface = null;
        _state = null;
    }

    internal static void Toggle()
    {
        if (_interface != null)
            _interface.SetState(_interface.CurrentState == null ? _state : null);
    }

    internal static void Close() => _interface?.SetState(null);

    public override void OnWorldUnload() => Close();

    public override void UpdateUI(GameTime gameTime)
    {
        _lastGameTime = gameTime;

        if (_interface?.CurrentState != null)
            _interface.Update(gameTime);
    }

    public override void ModifyInterfaceLayers(List<GameInterfaceLayer> layers)
    {
        int index = layers.FindIndex(layer => layer.Name == "Vanilla: Mouse Text");

        if (index < 0)
            return;

        layers.Insert(index, new LegacyGameInterfaceLayer(
            "ForgottenSouls: Class Choice",
            () =>
            {
                if (_interface?.CurrentState != null && _lastGameTime != null)
                    _interface.Draw(Main.spriteBatch, _lastGameTime);

                return true;
            },
            InterfaceScaleType.UI));
    }
}

internal class ClassChoiceUI : UIState
{
    private UIPanel _panel;

    private const float PanelWidth = 340f;
    private const float PanelHeight = 150f;
    private const float Gap = 18f;

    public override void OnInitialize()
    {
        _panel = new UIPanel();
        _panel.Width.Set(PanelWidth, 0f);
        _panel.Height.Set(PanelHeight, 0f);
        _panel.HAlign = 0.5f;
        _panel.VAlign = 0.5f;
        _panel.BackgroundColor = new Color(29, 33, 70) * 0.85f;
        Append(_panel);

        UIText title = new("Select a class", 0.9f, true)
        {
            HAlign = 0.5f,
            Top = { Pixels = 6f },
        };
        _panel.Append(title);

        float step = ClassButton.Size + Gap;

        AddOption(ChosenClass.Melee, ItemID.WarriorEmblem, "Воин", "Ближний бой: +10% скорости атаки", new Vector2(-1.5f * step, 0f));
        AddOption(ChosenClass.Ranged, ItemID.RangerEmblem, "Стрелок", "Дальний бой: +10% скорости атаки", new Vector2(-0.5f * step, 0f));
        AddOption(ChosenClass.Magic, ItemID.SorcererEmblem, "Маг", "Магия: +60 к запасу маны", new Vector2(0.5f * step, 0f));
        AddOption(ChosenClass.Summoner, ItemID.SummonerEmblem, "Призыватель", "Призыв: +1 слот прислужников", new Vector2(1.5f * step, 0f));
    }

    private void AddOption(ChosenClass chosenClass, int emblem, string name, string bonus, Vector2 offset)
    {
        ClassButton button = new(chosenClass, emblem, name, bonus);

        button.Left.Set(offset.X - button.Width.Pixels / 2f, 0.5f);
        button.Top.Set(offset.Y - button.Height.Pixels / 2f + 18f, 0.5f);
        _panel.Append(button);
    }

    public override void Update(GameTime gameTime)
    {
        base.Update(gameTime);

        if (_panel.ContainsPoint(Main.MouseScreen))
            Main.LocalPlayer.mouseInterface = true;

        Player player = Main.LocalPlayer;

        if (player.dead || !player.HasItem(ModContent.ItemType<ClassChoiceItem>()))
            ClassChoiceSystem.Close();
    }

    internal static void Select(ChosenClass chosenClass)
    {
        Player player = Main.LocalPlayer;
        int itemType = ModContent.ItemType<ClassChoiceItem>();

        if (!player.HasItem(itemType))
        {
            ClassChoiceSystem.Close();
            return;
        }

        player.GetModPlayer<ClassChoicePlayer>().Chosen = chosenClass;
        player.ConsumeItem(itemType);

        SoundEngine.PlaySound(SoundID.Item4, player.Center);
        CombatText.NewText(player.Hitbox, Color.Gold, "Класс выбран!");

        ClassChoiceSystem.Close();
    }
}

internal class ClassButton : UIElement
{
    internal const float Size = 56f;

    private readonly ChosenClass _class;
    private readonly int _emblem;
    private readonly string _name;
    private readonly string _bonus;

    public ClassButton(ChosenClass chosenClass, int emblem, string name, string bonus)
    {
        _class = chosenClass;
        _emblem = emblem;
        _name = name;
        _bonus = bonus;

        Width.Set(Size, 0f);
        Height.Set(Size, 0f);
    }

    public override void LeftClick(UIMouseEvent evt)
    {
        base.LeftClick(evt);
        ClassChoiceUI.Select(_class);
    }

    protected override void DrawSelf(SpriteBatch spriteBatch)
    {
        Rectangle area = GetDimensions().ToRectangle();
        bool hover = ContainsPoint(Main.MouseScreen);

        Utils.DrawInvBG(spriteBatch, area, (hover ? new Color(100, 120, 200) : new Color(63, 82, 151)) * 0.85f);

        Main.instance.LoadItem(_emblem);
        Texture2D texture = TextureAssets.Item[_emblem].Value;

        float scale = Math.Min(1f, 32f / Math.Max(texture.Width, texture.Height)) * (hover ? 1.25f : 1f);
        spriteBatch.Draw(texture, area.Center.ToVector2(), null, Color.White, 0f, texture.Size() / 2f, scale, SpriteEffects.None, 0f);

        if (hover)
        {
            Main.LocalPlayer.mouseInterface = true;
            Main.instance.MouseText($"{_name}\n{_bonus}\n+20% урона и +8% к крит. шансу, остальным классам -10% урона");
        }
    }
}