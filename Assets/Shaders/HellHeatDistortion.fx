sampler uImage0 : register(s0);

float uTime;
float uIntensity;

float4 HellHeatPixel(float2 coords : TEXCOORD0) : COLOR0
{
    // Вертикальные волны горячего воздуха
    float wave1 = sin(coords.y * 90.0 + uTime * 2.5);
    float wave2 = sin(coords.y * 145.0 - uTime * 1.8);

    // Небольшое горизонтальное искажение
    float distortion = (wave1 + wave2) * 0.0008 * uIntensity;

    float2 uv = coords;
    uv.x += distortion;

    // Дополнительное слабое вертикальное колебание
    uv.y += sin(coords.x * 70.0 + uTime * 2.0)
          * 0.0003 * uIntensity;

    return tex2D(uImage0, uv);
}

technique Technique1
{
    pass HellHeatPass
    {
        PixelShader = compile ps_2_0 HellHeatPixel();
    }
}