sampler2D Sampler; // 原始屏幕

float2 uCenter;    // 黑洞中心 (0~1)
float uRadius;     // 黑洞半径 (0~0.5)
float uStrength;   // 扭曲强度
float uTime;       // 时间

float4 PixelShaderFunction(float2 uv : TEXCOORD0) : COLOR0
{
    float2 delta = uv - uCenter;
    float dist = length(delta);

    //// 中心完全黑色
    //if (dist < uRadius * 0.6)
    //{
    //    return float4(0, 0, 0, 1);
    //}

    // 周围扭曲
    if (dist < uRadius)
    {
        float factor = (uRadius - dist) / uRadius;
        float angle = atan2(delta.y, delta.x);
        float radius = dist * (1 + factor * uStrength * sin(uTime * 4 + dist * 40));
        delta = radius * float2(cos(angle), sin(angle));
        uv = uCenter + delta;
    }

    float4 color = tex2D(Sampler, uv);
    return color;
}

technique BlackHoleDistortion{
    pass Pass1 {
        PixelShader = compile ps_2_0 PixelShaderFunction();
    }
}
