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
        public static ModKeybind Skill { get; private set; }

        public override void Load()
        {
            ChangeSpetoJav = KeybindLoader.RegisterKeybind(Mod, "更改长枪形态", "P");
            Skill = KeybindLoader.RegisterKeybind(Mod, "技能", "X");
        }

        public override void Unload()
        {
            ChangeSpetoJav = null;
            Skill = null;
        }




    }
}