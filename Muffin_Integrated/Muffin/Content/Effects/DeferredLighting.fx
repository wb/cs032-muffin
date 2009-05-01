static const int max_lights = 3;
int num_lights;
int current_light;

//float4x4 xLightViewProjection[3];
float4x4 xCameraViewProjection;
float4x4 xWorld;


Texture xTexture;
//Texture xSpotlight;
//Texture xShadowMap1;
//Texture xShadowMap2;
//Texture xShadowMap3;

float3 xCameraPos;
float3 xLightDir;
float3 xLightPos;

bool xEnableLighting;

//float xLightIntensity[3];

//float xSpecular;
//float xShininess;
//float xAmbient;

sampler TextureSampler = sampler_state { texture = <xTexture> ; magfilter = LINEAR; minfilter = LINEAR; mipfilter=LINEAR; AddressU = wrap; AddressV = wrap;};

struct VertexToPixel {
	float4 Position		: POSITION;
	float3 Normal		: TEXCOORD0;
	float4 ScreenPos	: TEXCOORD1;
	float2 TexCoords	: TEXCOORD2;
	float4 ObjectPos	: TEXCOORD3;
};

struct PixelToFrame {
	float4 Color		: COLOR0;
	float4 Normal		: COLOR1;
	float4 Depth		: COLOR2;
};

struct PixelToFrame2 {
	float4 Color		: COLOR0;
};

VertexToPixel VertexShaderFunction (float4 inPos		: POSITION0,
									float3 inNormal		: NORMAL0,
									float2 inTexCoords  : TEXCOORD0) 
{
	VertexToPixel Output = (VertexToPixel)0;
	//CWVP = camera's world view projection
	float4x4 CWVP = mul(xWorld, xCameraViewProjection);
	Output.Position = mul (float4(inPos.xyz, 1.0f), CWVP);
	
	float3x3 normal_world_matrix = (float3x3)xWorld;
	float3 normal_world = mul(inNormal, normal_world_matrix);
	Output.Normal = normalize(normal_world);
	
	Output.ScreenPos = Output.Position;
	Output.TexCoords = inTexCoords;		
	Output.ObjectPos = inPos;							
	
	return Output;
}

PixelToFrame PixelShaderFunction (VertexToPixel inVS) : COLOR0{
	PixelToFrame Output = (PixelToFrame)0;
	
	//diffuse lighting
	Output.Color.rgb = tex2D(TextureSampler, inVS.TexCoords);
	
	//transform form [-1,1] to [0,1]
	Output.Normal.xyz =  inVS.Normal/2.0f + 0.5f;
	
	Output.Depth = inVS.ScreenPos.z/inVS.ScreenPos.w;
	
	if(!xEnableLighting) {
		Output.Depth = -1.0f;   

		float4 topColor = float4(0.1f, 0.5f, 0.9f, 1);    
		float4 bottomColor = 1;    
	    
		float4 baseColor = lerp(bottomColor, topColor, saturate((inVS.ObjectPos.y)/0.7f));
		float4 cloudValue = tex2D(TextureSampler, inVS.TexCoords).r;
	    
		Output.Color = lerp(baseColor,1, cloudValue);       
	}
	
	return Output;
}

technique MultipleTargets
{
    pass Pass1
    {
        // TODO: set renderstates here.

        VertexShader = compile vs_3_0 VertexShaderFunction();
        PixelShader = compile ps_3_0 PixelShaderFunction();
    }
}
