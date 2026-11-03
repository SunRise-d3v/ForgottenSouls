sampler uImage0 : register(s0);

float uTime;
float uIntensity;

float4 ChallengePixel(
    float4 color : COLOR0,
    float2 uv : TEXCOORD0
) : COLOR0
{
    float4 source = tex2D(uImage0, uv);

    // Переход от металла к бордовому.
    float gradient = smoothstep(0.15, 0.9, uv.x);

    // Холодная металлическая поверхность.
    float metalNoise = sin(uv.y * 35.0 + uv.x * 8.0) * 0.5 + 0.5;

    float3 metal = lerp(
        float3(0.22, 0.25, 0.30),
        float3(0.85, 0.88, 0.92),
        metalNoise
    );

    // Глубокий бордовый оттенок крови.
    float bloodWave = sin(uv.y * 18.0 - uTime * 0.8) * 0.5 + 0.5;

    float3 blood = lerp(
        float3(0.12, 0.005, 0.025),
        float3(0.38, 0.015, 0.055),
        bloodWave * 0.35
    );

    float3 blade = lerp(metal, blood, gradient);

    // Металлический блик.
    float shine = pow(
        saturate(
            1.0 - abs(
                uv.y - 0.5 - sin(uv.x * 8.0) * 0.04
            ) * 18.0
        ),
        5.0
    );

    blade = lerp(
        blade,
        float3(1.0, 0.92, 0.88),
        shine * (1.0 - gradient) * 0.65
    );

    // Лёгкое движение кровавого оттенка.
    float pulse = sin(uTime * 1.5 + uv.x * 12.0) * 0.025;

    blade = lerp(
        blade,
        float3(0.48, 0.015, 0.065),
        saturate(gradient + pulse) * 0.2
    );

    float alpha = source.a * color.a;

    return float4(blade * color.rgb * alpha, alpha);
}

technique Technique1
{
    pass ChallengePass
    {
        PixelShader = compile ps_3_0 ChallengePixel();
    }
}