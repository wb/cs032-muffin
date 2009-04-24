float xAmbient;

Texture xTexture;
Texture xColorMap;
Texture xShadingMap;

sampler MenuTextureSampler = sampler_state { texture = <xTexture> ; magfilter = LINEAR; minfilter = LINEAR; mipfilter=LINEAR; AddressU = mirror; AddressV = mirror;};
sampler ColorMapSampler = sampler_state { texture = <xColorMap> ; magfilter = LINEAR; minfilter = LINEAR; mipfilter=LINEAR; AddressU = mirror; AddressV = mirror;};
sampler ShadingMapSampler = sampler_state { texture = <xShadingMap> ; magfilter = LINEAR; minfilter = LINEAR; mipfilter=LINEAR; AddressU = mirror; AddressV = mirror;};

struct VertexToPixel {
	float4 Position		: POSITION;
	float2 TexCoords	: TEXCOORD0;
};

struct PixelToFrame {
	float4 Color		: COLOR0;
};

VertexToPixel VertexShaderFunction (float4 inPos		: POSITION0,
									float2 inTexCoords  : TEXCOORD0) 
{
	VertexToPixel Output = (VertexToPixel)0;
	
	Output.Position = inPos;						
	Output.TexCoords = inTexCoords;
	
	return Output;
}

PixelToFrame PixelShaderFunction (VertexToPixel inVS) : COLOR0 {
	PixelToFrame Output = (PixelToFrame)0;
	
	float4 color = tex2D(ColorMapSampler, inVS.TexCoords);
	float shading = tex2D(ShadingMapSampler, inVS.TexCoords);
	if(xAmbient + shading > 1.0f) {
		Output.Color = color;
	} else {
		Output.Color = color * (xAmbient + shading);
	}
		
	return Output;
}

technique DeferredCombined
{
    pass Pass1
    {
        // TODO: set renderstates here.

        VertexShader = compile vs_3_0 VertexShaderFunction();
        PixelShader = compile ps_3_0 PixelShaderFunction();
    }
}

//////////////////////////////////////////////////

VertexToPixel VertexShaderFunction2 (float4 inPos		: POSITION0,
									float2 inTexCoords  : TEXCOORD0) 
{
	VertexToPixel Output = (VertexToPixel)0;
	
	Output.Position = inPos;						
	Output.TexCoords = inTexCoords;
	
	return Output;
}

PixelToFrame PixelShaderFunction2 (VertexToPixel inVS) : COLOR0 {
	PixelToFrame Output = (PixelToFrame)0;
	
	float4 color = tex2D(MenuTextureSampler, inVS.TexCoords);
	Output.Color = color;
		
	return Output;
}

technique TextureDraw
{
	pass Pass1
	{
		VertexShader = compile vs_3_0 VertexShaderFunction2();
        PixelShader = compile ps_3_0 PixelShaderFunction2();
	}
}