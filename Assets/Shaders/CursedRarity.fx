sampler uImage0 : register(s0);

float uTime;
float uIntensity;

float4 CursedPixel(
    float4 color : COLOR0,
    float2 coords : TEXCOORD0
) : COLOR0
{
    float4 textureColor = tex2D(uImage0, coords);

    // Пульсация яркости.
    float pulse = 0.85 + sin(uTime * 2.5) * 0.15;

    // Движущаяся полоса свечения.
    float shimmer = sin(coords.x * 120.0 - uTime * 5.0);
    shimmer = max(shimmer, 0.0) * 0.2;

    float brightness = pulse + shimmer * uIntensity;

    float3 result = textureColor.rgb * color.rgb * brightness;

    return float4(result, textureColor.a * color.a);
}

technique Technique1
{
    pass CursedPass
    {
        PixelShader = compile ps_2_0 CursedPixel();
    }
}