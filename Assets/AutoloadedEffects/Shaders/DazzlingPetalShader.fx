sampler noiseTexture : register(s1);
sampler streakTexture : register(s2);

float fireColorInterpolant;
float globalTime;
matrix uWorldViewProjection;

struct VertexShaderInput
{
    float4 Position : POSITION0;
    float4 Color : COLOR0;
    float3 TextureCoordinates : TEXCOORD0;
};

struct VertexShaderOutput
{
    float4 Position : SV_POSITION;
    float4 Color : COLOR0;
    float3 TextureCoordinates : TEXCOORD0;
};

VertexShaderOutput VertexShaderFunction(in VertexShaderInput input)
{
    VertexShaderOutput output = (VertexShaderOutput) 0;
    float4 pos = mul(input.Position, uWorldViewProjection);
    output.Position = pos;
    
    output.Color = input.Color;
    output.TextureCoordinates = input.TextureCoordinates;

    return output;
}

float4 CalculateFireColor(float2 coords)
{
    float fireNoise = tex2D(noiseTexture, coords * 2 + float2(globalTime * -3.95, 0));
    float horizontalEdgeDistance = distance(coords.y, 0.5);
    float edgeFade = smoothstep(0.5, 0.2, horizontalEdgeDistance);
    float glow = clamp(edgeFade / horizontalEdgeDistance * fireNoise * 0.4, 0, 5) * (1 - coords.x);
    
    float4 fireColor = lerp(float4(1.95, 0.4, 0.06, 1), float4(0.94, 0.85, 0.45, 1), fireNoise) * glow;
    fireColor.a *= smoothstep(0.2, 0.4, coords.x);
    fireColor -= tex2D(streakTexture, coords + float2(globalTime * -4.15, 0)) * fireColor.a / glow * 2;
    
    return fireColor * 2;
}

float4 PixelShaderFunction(VertexShaderOutput input) : COLOR0
{
    float2 coords = input.TextureCoordinates;
    coords.y = (input.TextureCoordinates.y - 0.5) / input.TextureCoordinates.z + 0.5;
    
    float horizontalEdgeDistance = distance(coords.y, 0.5);
    float edgeFade = smoothstep(0.5, 0, horizontalEdgeDistance);
    float glow = saturate(edgeFade / horizontalEdgeDistance * 0.5);
    float4 petalColor = float4(input.Color.rgb, 0) * glow + (1 - coords.x) * edgeFade * 1.3;
    
    return lerp(petalColor, CalculateFireColor(coords), fireColorInterpolant) * input.Color.a;
}

technique Technique1
{
    pass AutoloadPass
    {
        VertexShader = compile vs_3_0 VertexShaderFunction();
        PixelShader = compile ps_3_0 PixelShaderFunction();
    }
}