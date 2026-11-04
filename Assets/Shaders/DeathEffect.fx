sampler uImage0 : register(s0);
float uTime;
float uProgress;

float hash(float2 p)
{
    p = frac(p * float2(123.34, 456.21));
    p += dot(p, p + 45.32);
    return frac(p.x * p.y);
}

float vnoise(float2 p)
{
    float2 i = floor(p);
    float2 f = frac(p);
    f = f * f * (3.0 - 2.0 * f);
    float a = hash(i);
    float b = hash(i + float2(1, 0));
    float c = hash(i + float2(0, 1));
    float d = hash(i + float2(1, 1));
    return lerp(lerp(a, b, f.x), lerp(c, d, f.x), f.y);
}

float fbm(float2 p)
{
    float v = 0.0;
    float a = 0.5;
    for (int i = 0; i < 4; i++)
    {
        v += a * vnoise(p);
        p = p * 2.03 + 17.1;
        a *= 0.5;
    }
    return v;
}

float4 DeathEffectPS(float2 uv : TEXCOORD0) : COLOR0
{
    float3 scene = tex2D(uImage0, uv).rgb;
    
    float2 q = uv * float2(7.0, 4.0);
    float2 warp = float2(vnoise(q * 0.8 + 3.7), vnoise(q * 0.8 + 11.3)) - 0.5;
    float n = fbm(q + warp * 1.6 + uTime * 0.05);
    float black = smoothstep(n - 0.04, n + 0.02, uProgress * 1.15);

    return float4(scene * (1.0 - black), 1.0);
}

technique Technique1
{
    pass DeathEffect
    {
        PixelShader = compile ps_3_0 DeathEffectPS();
    }
}