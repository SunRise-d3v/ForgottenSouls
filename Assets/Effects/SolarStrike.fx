// Экранный шейдер солнечного удара: тепловое марево, пересвет, выцветание, раскалённое виньетирование.
// Файл: Assets/Effects/SolarStrike.fx

sampler uImage0 : register(s0);

float uTime;
float uIntensity;        // 0..1, сила эффекта (задаётся из кода)
float uOpacity;          // общая непрозрачность фильтра (ставит tModLoader)
float3 uColor;           // тёплый оттенок
float3 uSecondaryColor;  // цвет «белого каления»
float2 uScreenResolution;

float4 SolarStrikePixel(float4 sampleColor : COLOR0, float2 coords : TEXCOORD0) : COLOR0
{
    float strength = saturate(uIntensity) * uOpacity;
    float2 centered = coords - 0.5;
    float dist = length(centered);

    // 1. Тепловое марево: кадр волнами «плывёт», сильнее к краям
    float wave = sin(coords.y * 60.0 + uTime * 7.0) + sin(coords.y * 23.0 - uTime * 4.5);
    float2 uv = coords;
    uv.x += wave * 0.0025 * strength * (0.4 + dist);
    uv.y += sin(coords.x * 45.0 + uTime * 5.0) * 0.0015 * strength;

    // 2. Пульсирующее «дыхание» кадра (лёгкое приближение в такт)
    float pulse = 0.5 + 0.5 * sin(uTime * 3.0);
    uv = 0.5 + (uv - 0.5) * (1.0 - 0.025 * strength * pulse);

    // 3. Хроматическая аберрация, растёт к краям экрана
    float2 shift = centered * 0.012 * strength;
    float3 color;
    color.r = tex2D(uImage0, uv + shift).r;
    color.g = tex2D(uImage0, uv).g;
    color.b = tex2D(uImage0, uv - shift).b;

    // 4. Выцветание и тёплый оттенок
    float luma = dot(color, float3(0.299, 0.587, 0.114));
    color = lerp(color, luma.xxx, 0.35 * strength);
    color = lerp(color, color * uColor * 1.35, 0.55 * strength);

    // 5. Пересвет ярких областей (небо, свет)
    float bloom = smoothstep(0.5, 1.0, luma);
    color += uSecondaryColor * bloom * 0.55 * strength;

    // 6. Белое раскалённое виньетирование по краям
    float vignette = smoothstep(0.2, 0.85, dist);
    color = lerp(color, uSecondaryColor, vignette * 0.75 * strength);

    // 7. Короткая ослепляющая вспышка при самой сильной фазе (момент удара)
    float flash = smoothstep(0.8, 1.0, strength);
    color = lerp(color, uSecondaryColor, flash * 0.35 * (1.0 - dist));

    return float4(color, 1.0);
}

technique Technique1
{
    pass SolarStrikePass
    {
        PixelShader = compile ps_3_0 SolarStrikePixel();
    }
}
