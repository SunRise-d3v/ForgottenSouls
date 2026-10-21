using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ForgottenSouls.Content.Items;

internal sealed class CookEnchantment : ModItem
{
    public override string Texture => Helper.TextureLoad("Items", "CookEnchantment");
}