using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.ModLoader.Config;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.ModLoader.Config;
using System.ComponentModel;
using Terraria.ModLoader.Config;

namespace CalamityThrowingSpear.Global
{
    public class CTSConfigs : ModConfig
    {
        public override ConfigScope Mode => ConfigScope.ClientSide;


        //[Label("开启全部特效")]
        //[Tooltip("用于开关弹药的所有特效")]


        // 专门启用化龙点睛的独特音效
        [DefaultValue(true)]
        public bool EnableFTSound { get; set; }

        // 专门启用是否受攻速加成影响的开关
        [DefaultValue(true)]
        [ReloadRequired]
        public bool EnableMeleeSpeed { get; set; }


        // 专门开关盗贼武器
        [DefaultValue(false)]
        [ReloadRequired]
        public bool EnableRogue { get; set; }




    }
}