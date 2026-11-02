sampler uImage0 : register(s0);

float uTime;
float uIntensity;

float4 ChallengePixel(
    float4 color : COLOR0,
    float2 uv : TEXCOORD0
) : COLOR0
{
    float4 source = tex2D(uImage0, uv);

    // Анимация крови.
    float wave1 = sin(uv.x * 85.0 + uv.y * 25.0 - uTime * 3.0);
    float wave2 = sin(uv.x * 130.0 - uv.y * 40.0 - uTime * 4.0);

    float flame = saturate((wave1 + wave2) * 0.25 + 0.5);

    // Кровавые потёки.
    float x1 = abs(uv.x - 0.78);
    float x2 = abs(uv.x - 0.88);
    float x3 = abs(uv.x - 0.97);

    float y = uv.y - 0.48;

    float length1 = 0.12 + sin(uTime * 0.7) * 0.025;
    float length2 = 0.19 + sin(uTime * 0.5 + 1.0) * 0.035;
    float length3 = 0.09 + sin(uTime * 0.9 + 2.0) * 0.02;

    float drip1 =
        (1.0 - smoothstep(0.009, 0.016, x1)) *
        (1.0 - smoothstep(length1, length1 + 0.015, y)) *
        step(0.0, y);

    float drip2 =
        (1.0 - smoothstep(0.008, 0.014, x2)) *
        (1.0 - smoothstep(length2, length2 + 0.015, y)) *
        step(0.0, y);

    float drip3 =
        (1.0 - smoothstep(0.009, 0.015, x3)) *
        (1.0 - smoothstep(length3, length3 + 0.015, y)) *
        step(0.0, y);

    float tip1 = 1.0 - smoothstep(
        0.012, 0.022,
        length(float2(x1, y - length1))
    );

    float tip2 = 1.0 - smoothstep(
        0.011, 0.020,
        length(float2(x2, y - length2))
    );

    float tip3 = 1.0 - smoothstep(
        0.012, 0.021,
        length(float2(x3, y - length3))
    );

    float drips = saturate(
        max(max(drip1, drip2), drip3) +
        max(max(tip1, tip2), tip3)
    );

    // Цвет крови.
    float3 blood = lerp(
        float3(0.10, 0.005, 0.025),
        float3(0.75, 0.04, 0.08),
        flame * 0.5
    );

    // Более яркие кончики капель.
    float3 dropColor = lerp(
        float3(0.30, 0.005, 0.02),
        float3(0.85, 0.08, 0.12),
        flame
    );

    float3 outline = lerp(
        blood,
        dropColor,
        drips
    );

    // Чёрные буквы, красный контур.
    float3 result = lerp(
        outline,
        float3(0.0, 0.0, 0.0),
        source.a
    );

    // Сохраняем альфа-канал текста.
    float alpha = source.a * color.a;

    return float4(result * alpha, alpha);
}

technique Technique1
{
    pass ChallengePass
    {
        PixelShader = compile ps_3_0 ChallengePixel();
    }
}