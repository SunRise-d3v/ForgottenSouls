sampler uImage0 : register(s0);

float uTime;
float uIntensity;
float uOpacity;

float4 SolarStrikePixel(float2 coords : TEXCOORD0) : COLOR0
{
    float wave = sin(coords.y * 60.0 + uTime * 7.0);

    float2 uv = coords;
    uv.x += wave * 0.0025 * uIntensity;

    float2 shift = (uv - 0.5) * 0.012 * uIntensity;

    float red = tex2D(uImage0, uv + shift).r;
    float green = tex2D(uImage0, uv).g;
    float blue = tex2D(uImage0, uv - shift).b;

    float3 color = float3(red, green, blue);

    float luma = dot(color, float3(0.299, 0.587, 0.114));

    // Обесцвечивание
    color = lerp(
        color,
        float3(luma, luma, luma),
        0.35 * uIntensity
    );

    // Тёплый оттенок
    color *= float3(1.15, 0.85, 0.55);

    // Bloom
    float bloom = (luma - 0.5) * 0.5;

    if (bloom < 0.0)
        bloom = 0.0;

    color += float3(1.0, 0.75, 0.35) * bloom * uIntensity;

    // Виньетка
    float2 centered = coords - 0.5;
    float dist = length(centered);

    float vignette = (dist - 0.2) / 0.65;

    if (vignette < 0.0)
        vignette = 0.0;

    if (vignette > 1.0)
        vignette = 1.0;

    color = lerp(
        color,
        float3(1.0, 0.55, 0.15),
        vignette * 0.5 * uIntensity
    );

    // Пульсирующая вспышка
    float pulse = (sin(uTime * 3.0) + 1.0) * 0.5;

    float flash = pulse * uIntensity * 0.15;

    color = lerp(
        color,
        float3(1.0, 0.85, 0.55),
        flash
    );

    return float4(color, 1.0);
}

technique Technique1
{
    pass SolarStrikePass
    {
        PixelShader = compile ps_2_0 SolarStrikePixel();
    }
}