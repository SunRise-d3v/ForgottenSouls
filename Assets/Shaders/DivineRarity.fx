sampler uImage0 : register(s0);

float uTime;
float uIntensity;

float4 DivinePixel(
    float4 color : COLOR0,
    float2 uv : TEXCOORD0
) : COLOR0
{
    float4 source = tex2D(uImage0, uv);

    float wave1 = sin(uv.x * 85.0 + uv.y * 25.0 - uTime * 5.0);
    float wave2 = sin(uv.x * 130.0 - uv.y * 40.0 - uTime * 7.0);

    float flame = saturate((wave1 + wave2) * 0.25 + 0.5);
    flame = saturate(flame * uIntensity);

    float3 fireColor = lerp(
        float3(1.0, 0.12, 0.0),
        float3(1.0, 0.85, 0.1),
        flame
    );

    float alpha = source.a * color.a;
    
    float3 result = lerp(
        fireColor,
        float3(0.0, 0.0, 0.0),
        source.a
    );

    return float4(result * alpha, alpha);
}

technique Technique1
{
    pass DivinePass
    {
        PixelShader = compile ps_2_0 DivinePixel();
    }
}