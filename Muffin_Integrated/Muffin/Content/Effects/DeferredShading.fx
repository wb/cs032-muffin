float4x4 xCameraViewProjection;
float4x4 xLightViewProjection;
float4x4 xViewProjectionInverse;

Texture xNormalMap;
Texture xDepthMap;
Texture xShadowMap;
Texture xShadingMap;

float3 xCameraPos;

float3 xLightPos;
float xLightIntensity;
float3 xConeDirection;
float xConeAngle;
float xConeDecay;

float xSpecular;
float xShininess;
float xAmbient;

sampler NormalMapSampler = sampler_state { texture = <xNormalMap> ; magfilter = LINEAR; minfilter = LINEAR; mipfilter=LINEAR; AddressU = mirror; AddressV = mirror;};
sampler DepthMapSampler = sampler_state { texture = <xDepthMap> ; magfilter = LINEAR; minfilter = LINEAR; mipfilter=LINEAR; AddressU = mirror; AddressV = mirror;};
sampler ShadowMapSampler = sampler_state { texture = <xShadowMap> ; magfilter = LINEAR; minfilter = LINEAR; mipfilter=LINEAR; AddressU = CLAMP; AddressV = CLAMP;};
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
	float distanceInShadowMap = tex2D(ShadowMapSampler, lightSamplePos);
	
	float shading = 0;
	
	if(!(distanceInShadowMap <= realDistance - 1.0f/100.0f)) {
		float3 lightDirection = normalize(worldPos - xLightPos);
		//float coneDot = dot(lightDirection, normalize(xConeDirection));
		
		
		//float attenuation = pow(coneDot, xConeDecay);
		shading = dot(normal, -lightDirection);
		shading = shading * xLightIntensity;	
		
		//if(coneDot >= xConeAngle) {
		//	float attenuation = pow(coneDot, xConeDecay);
			
		//	shading = dot(normal, -lightDirection);
		//	shading = shading * xLightIntensity * attenuation;	
		//}
	} 
	
	float4 previous = tex2D(ShadingMapSampler, inVS.TexCoords);
	Output.Color = shading + previous;
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
