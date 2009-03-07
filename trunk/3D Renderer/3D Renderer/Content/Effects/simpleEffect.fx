float3 xLightPos;
float xLightIntensity;
float xAmbient;

float4x4 xWorld;
float4x4 xWorldViewProjection;
bool xSolidBrown;


Texture xTexture;

sampler TextureSampler = sampler_state { texture = <xTexture> ; magfilter = LINEAR; minfilter = LINEAR; mipfilter=LINEAR; AddressU = mirror; AddressV = mirror;};
struct VertexToPixel
{
    float4 Position     : POSITION;    
    float2 TexCoords    : TEXCOORD0;
    float3 Normal       : TEXCOORD1;
    float3 Position3D   : TEXCOORD2;
};

struct PixelToFrame
{
    float4 Color        : COLOR0;
};

float DotProduct(float3 light3D, float3 pos3D, float3 normal)
{
    float3 lightDir = normalize(pos3D - light3D);
    return dot(-lightDir, normal);    
}

VertexToPixel SimplestVertexShader( float4 inPos : POSITION, float2 inTexCoords : TEXCOORD0, float3 inNormal: NORMAL0)
{
    VertexToPixel Output = (VertexToPixel)0;
    
    Output.Position =mul(inPos, xWorldViewProjection);
    Output.Position3D = mul(inPos, xWorld);
    Output.Normal = normalize(mul(inNormal, (float3x3)xWorld));
    Output.TexCoords = inTexCoords;
    return Output;
}

PixelToFrame OurFirstPixelShader(VertexToPixel PSIn)
{
	float diffuseLighting = DotProduct(xLightPos, PSIn.Position3D, PSIn.Normal);
    PixelToFrame Output = (PixelToFrame)0;    

	diffuseLighting = saturate(diffuseLighting);
	diffuseLighting = diffuseLighting * xLightIntensity;

    Output.Color = tex2D(TextureSampler, PSIn.TexCoords);
    
    if (xSolidBrown == true)
         Output.Color = float4(0.25f, 0.21f, 0.20f, 1);
    
	Output.Color = Output.Color * (diffuseLighting + xAmbient);

    return Output;
}

technique Simplest
{
    pass Pass0
    {
        VertexShader = compile vs_2_0 SimplestVertexShader();
        PixelShader = compile ps_2_0 OurFirstPixelShader();
    }
}