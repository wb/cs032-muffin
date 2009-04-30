float4x4 xLightViewProjection;
float4x4 xCameraViewProjection;
float4x4 xWorld;
float4x4 xWorldInv;

float3 xCameraPos;
float3 xLightPos;

bool xEnableLighting;
Texture xTexture;

struct SMapVertexToPixel
{
    float4 Position      : POSITION;
    float4 ScreenPos	 : TEXCOORD1;
};

struct SMapPixelToFrame
{
    float4 Color : COLOR0;
};

SMapVertexToPixel ShadowMapVertexShader( float4 inPos : POSITION0,
										 float3 inNormal : NORMAL0)
{
    SMapVertexToPixel Output = (SMapVertexToPixel)0;
    float4x4 xLight = mul(xWorld, xLightViewProjection);

	Output.Position = mul(inPos, xLight);
    Output.ScreenPos = Output.Position;
    
    return Output;
}

SMapPixelToFrame ShadowMapPixelShader(SMapVertexToPixel PSIn) : COLOR0
{
	SMapPixelToFrame Output = (SMapPixelToFrame)0;            
    Output.Color.r = PSIn.ScreenPos.z/PSIn.ScreenPos.w;

    return Output;
}

technique ShadowMap
{
    pass Pass0
    {
        VertexShader = compile vs_3_0 ShadowMapVertexShader();
        PixelShader = compile ps_3_0 ShadowMapPixelShader();
    }
}

///////////////////////////////////////////////////////////////
///////////////////////////////////////////////////////////////
///////////////////////////////////////////////////////////////
///////////////////////////////////////////////////////////////
///////////////////////////////////////////////////////////////