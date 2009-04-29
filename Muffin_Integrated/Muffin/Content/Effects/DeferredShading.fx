float4x4 xCameraViewProjection;
float4x4 xLightViewProjection;
float4x4 xViewProjectionInverse;

Texture xNormalMap;
Texture xDepthMap;
Texture xShadowMap;
Texture xShadingMap;

float3 xCameraPos;
float3 xLightDir;
float3 xLightPos;
float xLightIntensity;
float3 xConeDirection;
float xConeAngle;
float xConeDecay;

float xSpecular;
float xShininess;
float xAmbient;

sampler NormalMapSampler = sampler_state { texture = <xNormalMap> ; magfilter = POINT; minfilter = POINT; mipfilter=LINEAR; AddressU = mirror; AddressV = mirror;};
sampler DepthMapSampler = sampler_state { texture = <xDepthMap> ; magfilter = POINT; minfilter = POINT; mipfilter=none; AddressU = mirror; AddressV = mirror;};
sampler ShadowMapSampler = sampler_state { texture = <xShadowMap> ; magfilter = LINEAR; minfilter = LINEAR; mipfilter=LINEAR; AddressU = CLAMP; AddressV = CLAMP;};
sampler ShadingMapSampler = sampler_state { texture = <xShadingMap> ; magfilter = LINEAR; minfilter = LINEAR; mipfilter=LINEAR; AddressU = mirror; AddressV = mirror;};

struct VertexToPixel {
	float4 Position				  : POSITION;
	float2 TexCoords			  : TEXCOORD0;
};

struct PixelToFrame {
	float4 Color		: COLOR0;
};

// Calculates the shadow term using PCF soft-shadowing
float CalcShadowTermSoftPCF(float fLightDepth, float2 vShadowTexCoord, int iSqrtSamples)
{
	float fShadowTerm = 0.0f;  
		
	float fRadius = (iSqrtSamples - 1.0f) / 2;
	float fWeightAccum = 0.0f;
	
	for (float y = -fRadius; y <= fRadius; y++)
	{
		for (float x = -fRadius; x <= fRadius; x++)
		{
			float2 vOffset = 0;
			vOffset = float2(x, y);				
			vOffset.x /= 1440.0f;
			vOffset.y /= 900.0f;
			float2 vSamplePoint = vShadowTexCoord + vOffset;			
			float fDepth = tex2D(ShadowMapSampler, vSamplePoint).x;
			float fSample = (fLightDepth <= fDepth + 0.01f);
			
			// Edge tap smoothing
			float xWeight = 1;
			float yWeight = 1;
			
			if (x == -fRadius)
				xWeight = 1 - frac(vShadowTexCoord.x * 1440.0f);
			else if (x == fRadius)
				xWeight = frac(vShadowTexCoord.x * 1440.0f);
				
			if (y == -fRadius)
				yWeight = 1 - frac(vShadowTexCoord.y * 900.0f);
			else if (y == fRadius)
				yWeight = frac(vShadowTexCoord.y * 900.0f);
				
			fShadowTerm += fSample * xWeight * yWeight;
			fWeightAccum = xWeight * yWeight;
		}											
	}		
	
	fShadowTerm /= (iSqrtSamples * iSqrtSamples);
	fShadowTerm *= 1.55f;	
	
	return fShadowTerm;
}

VertexToPixel VertexShaderFunction (float4 inPos		: POSITION0,
									float2 inTexCoords  : TEXCOORD0) 
{
	VertexToPixel Output = (VertexToPixel)0;
	
	Output.Position.x = inPos.x;						
	Output.Position.y = inPos.y;
	Output.Position.z = 0.0f;
	Output.Position.w = 1.0f;						
	Output.TexCoords = inTexCoords;
	return Output;
}

PixelToFrame PixelShaderFunction (VertexToPixel inVS) : COLOR0 {
	PixelToFrame Output = (PixelToFrame)0;
	
	float3 normal = tex2D(NormalMapSampler, inVS.TexCoords).rgb;
	normal = normal * 2.0f - 1.0f;
	normal = normalize(normal);
	
	float depth = tex2D(DepthMapSampler, inVS.TexCoords).r;
	
	float4 screenPos;
	screenPos.x = inVS.TexCoords.x * 2.0f - 1.0f;
	screenPos.y = -(inVS.TexCoords.y * 2.0f - 1.0f);
	screenPos.z = depth;
	screenPos.w = 1.0f;
	
	float4 worldPos = mul(screenPos, xViewProjectionInverse);
	worldPos /= worldPos.w;
	
	//find screen position
	float4 lightScreenPos = mul(worldPos, xLightViewProjection);
	lightScreenPos /= lightScreenPos.w;
	
	//convert so that screen position can be looked up in shadow map
	float2 lightSamplePos;
	lightSamplePos.x = (lightScreenPos.x / 2.0f) + 0.5f;
	lightSamplePos.y = (-lightScreenPos.y / 2.0f + 0.5f);
	
	float realDistance = lightScreenPos.z;
	float distanceInShadowMap = tex2D(ShadowMapSampler, lightSamplePos).r;
	
	float shading = 0;
	
	float3 lightDirection = normalize(worldPos - xLightPos);
	shading = dot(normal, -lightDirection);
	shading = shading * xLightIntensity;	

	float3 eyeDir = normalize(xCameraPos - worldPos);
	float3 reflection = reflect(-lightDirection, normal);
	Output.Color.b = saturate(dot(reflection, -eyeDir));
	
	float4 previous = tex2D(ShadingMapSampler, inVS.TexCoords);
	Output.Color.r = shading + previous;
	Output.Color.a = CalcShadowTermSoftPCF(realDistance, lightSamplePos, 4);
	//Output.Color = tex2D(ShadowMapSampler, inVS.TexCoords);
	return Output;
}

technique DeferredShading
{
    pass Pass1
    {
        // TODO: set renderstates here.

        VertexShader = compile vs_3_0 VertexShaderFunction();
        PixelShader = compile ps_3_0 PixelShaderFunction();
    }
}
