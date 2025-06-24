using Terraria.Graphics.Effects;
using Terraria.Graphics.Shaders;
using Microsoft.Xna.Framework.Graphics;
using Terraria.ModLoader;
using Terraria;
using ReLogic.Content;
using Microsoft.Xna.Framework;
using System.Collections.Generic;

namespace CalamityRangerExpansion.LightingBolts.Shader
{
    internal class ShaderGames : Mod
    {
        // 直接定义所有 Shader
        public static Effect RainbowShader;
        public static Effect EdgeGlowShader;
        public static Effect GlassRefractionShader;
        public static Effect DistortionShader;
        //public static Effect TailMagic;
        //public static Effect TailModern;
        //public static Effect TailTechnology;
        public static Effect EnchantmentShader;
        public static Effect GlitchBlocksShader;
        public static Effect KaleidoscopeShader;
        public static Effect KaleidoscopeScreenShader;
        public static Effect ScanlineShader;
        public static Effect WormShader;
        public static Effect GrayscaleShader;
        public static Effect MagnifyDistortionShader;
        public static Effect CyberNeonGlow;
        public static Effect LiquidFlowShader;
        public static Effect FireBurnShader;
        public static Effect AuroraWaveShader;
        public static Effect PixelationShader;
        public static Effect TailFirst;
        public static Effect TailSecond;
        public static Effect TailMagic;
        public static Effect TailModern;
        public static Effect TailTechnology;
        public static Effect TrailFrostCrystal;
        public static Effect TrailGhostlyPhantom;
        public static Effect TrailBlazingFlame;
        public static Effect TrailWarpDistortion;


        






        public override void Load()
        {
            if (Main.dedServ)
                return;

            // 按顺序加载 Shader
            RainbowShader = AddShader("RainbowShader"); // 横向彩虹
            EdgeGlowShader = AddShader("EdgeGlowShader"); // 边缘发光
            GlassRefractionShader = AddShader("GlassRefractionShader"); // 玻璃反射
            DistortionShader = AddShader("DistortionShader"); // 失真

            EnchantmentShader = AddShader("EnchantmentShader"); // 附魔书
            GlitchBlocksShader = AddShader("GlitchBlocksShader"); // 故障色块
            KaleidoscopeShader = AddShader("KaleidoscopeShader"); // 万花筒
            KaleidoscopeScreenShader = AddShader("KaleidoscopeScreenShader"); // 整个屏幕的万花筒
            ScanlineShader = AddShader("ScanlineShader"); // 扫描线
            WormShader = AddShader("WormShader"); // 蠕虫
            GrayscaleShader = AddShader("GrayscaleShader"); // 灰度渐变
            MagnifyDistortionShader = AddShader("MagnifyDistortionShader"); // 放大镜
            CyberNeonGlow = AddShader("CyberNeonGlow"); // 赛博霓虹
            LiquidFlowShader = AddShader("LiquidFlowShader"); // 流体力学
            FireBurnShader = AddShader("FireBurnShader"); // 火焰灼烧
            AuroraWaveShader = AddShader("AuroraWaveShader"); // 南极极光
            PixelationShader = AddShader("PixelationShader"); // 马赛克









            TailFirst = AddShader("TailFirst"); // 1号拖尾
            RegisterTrailShader(TailFirst, "TrailPass", "TailFirstEffect");

            TailSecond = AddShader("TailSecond"); // 2号拖尾
            RegisterTrailShader(TailSecond, "TrailPass", "TailSecondEffect");

            TailMagic = AddShader("TailMagic"); // 魔法拖尾
            RegisterTrailShader(TailMagic, "TrailPass", "TailMagicEffect");

            TailModern = AddShader("TailModern"); // 现代拖尾
            RegisterTrailShader(TailModern, "TrailPass", "TailModernEffect");

            TailTechnology = AddShader("TailTechnology"); // 科技拖尾
            RegisterTrailShader(TailTechnology, "TrailPass", "TailTechnologyEffect");

            TrailFrostCrystal = AddShader("TrailFrostCrystal"); // 冰霜拖尾
            RegisterTrailShader(TrailFrostCrystal, "TrailPass", "TrailFrostCrystalEffect");

            TrailGhostlyPhantom = AddShader("TrailGhostlyPhantom"); // 鬼魂拖尾
            RegisterTrailShader(TrailGhostlyPhantom, "TrailPass", "TrailGhostlyPhantomEffect");

            TrailBlazingFlame = AddShader("TrailBlazingFlame"); // 烈焰拖尾
            RegisterTrailShader(TrailBlazingFlame, "TrailPass", "TrailBlazingFlameEffect");

            TrailWarpDistortion = AddShader("TrailWarpDistortion"); // 空间扭曲拖尾
            RegisterTrailShader(TrailWarpDistortion, "TrailPass", "TrailWarpDistortionEffect");



            







            //TailMagic = AddShader("TailMagic"); // 魔法拖尾
            //TailModern = AddShader("TailModern"); // 现代拖尾
            //TailTechnology = AddShader("TailTechnology"); // 科技拖尾


            // **注册拖尾 Shader**
            //GameShaders.Misc["ModNamespace:TailMagic"] = new MiscShaderData(ModContent.Request<Effect>("CalamityRangerExpansion/LightingBolts/Shader/TailMagic"), "MagicTrail");
            //GameShaders.Misc["ModNamespace:TailModern"] = new MiscShaderData(ModContent.Request<Effect>("CalamityRangerExpansion/LightingBolts/Shader/TailModern"), "ModernTrail");
            //GameShaders.Misc["ModNamespace:TailTechnology"] = new MiscShaderData(ModContent.Request<Effect>("CalamityRangerExpansion/LightingBolts/Shader/TailTechnology"), "TechTrail");


            // 以后新增 Shader 直接加在这里
        }
        // 调用的时候可以使用这些原版的底图：
        // Images/Extra_
        // 191 192 193 194 196 197
        // 


        // **注册拖尾 Shader**
        private static void RegisterTrailShader(Effect shader, string passName, string registrationName)
        {
            Ref<Effect> shaderPointer = new(shader);
            MiscShaderData passParamRegistration = new(shaderPointer, passName);
            GameShaders.Misc[$"ModNamespace:{registrationName}"] = passParamRegistration;
        }


        // 统一的 Shader 加载方法
        private static Effect AddShader(string name)
        {
            string path = $"CalamityRangerExpansion/LightingBolts/Shader/XNBcoder/Effects/{name}";
            try
            {
                Effect shader = ModContent.Request<Effect>(path, AssetRequestMode.ImmediateLoad).Value;
                Main.NewText($"{name} 加载成功！", Color.Green);
                return shader;
            }
            catch
            {
                Main.NewText($"{name} 加载失败，完蛋", Color.Red);
                return null;
            }
        }
    }
}
