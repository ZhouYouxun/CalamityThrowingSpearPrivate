using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.DataStructures;
using Microsoft.Xna.Framework;
using Terraria.Audio;
using Microsoft.Xna.Framework.Graphics;
using System.Linq;
using Terraria.Localization;


namespace CalamityThrowingSpear.Global
{
    public class KeybindSystem : ModSystem
    {
        public static ModKeybind ChangeSpetoJav { get; private set; }
        public static ModKeybind WeaponSkill { get; private set; }

        public override void Load()
        {
            ChangeSpetoJav = KeybindLoader.RegisterKeybind(Mod, "更改长枪形态", "P");
            WeaponSkill = KeybindLoader.RegisterKeybind(Mod, "武器技能", "X");
        }

        public override void Unload()
        {
            ChangeSpetoJav = null;
            WeaponSkill = null;
        }




    }
}