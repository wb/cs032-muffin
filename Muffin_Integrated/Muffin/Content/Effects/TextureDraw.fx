Texture xTexture;

sampler MenuTextureSampler = sampler_state { texture = <xTexture> ; magfilter = LINEAR; minfilter = LINEAR; mipfilter=LINEAR; AddressU = mirror; AddressV = mirror;};

struct VertexToPixel {
	float4 Position		: POSITION;
	float2 TexCoords	: TEXCOORD0;
};

struct PixelToFrame {
	float4 Color		: COLOR0;
};


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