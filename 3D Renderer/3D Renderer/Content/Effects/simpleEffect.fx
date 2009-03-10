float3 xLightPos;
float3 xCameraPos;
float xLightIntensity;
float xAmbient;
float xSpecular;
float xShininess;

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

float getDiffuseIntensity(float3 light3D, float3 pos3D, float3 normal)
{
    float3 lightDir = normalize(pos3D - light3D);
    return dot(-lightDir, normal);    
}

float getSpecularIntensity(float3 light3D, float3 pos3D, float3 normal, float3 los_v)
{
    float3 lightDir = normalize(pos3D - light3D);
    float3 reflection = normalize(2*normal*(dot(-lightDir,normal)) + lightDir);
    return dot(reflection, los_v);    
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
	float diffuseLighting = getDiffuseIntensity(xLightPos, PSIn.Position3D, PSIn.Normal);
	diffuseLighting = saturate(diffuseLighting);
	diffuseLighting = diffuseLighting * xLightIntensity;

    float3 LOS_v = normalize(PSIn.Position3D - xCameraPos);
	float specularLighting = getSpecularIntensity(xLightPos, PSIn.Position3D, PSIn.Normal, LOS_v);
	specularLighting = saturate(specularLighting);
	specularLighting = xSpecular * xLightIntensity * pow(specularLighting, xShininess);

    PixelToFrame Output = (PixelToFrame)0;    
    Output.Color = tex2D(TextureSampler, PSIn.TexCoords);
    
    if (xSolidBrown == true)
         Output.Color = float4(0.25f, 0.21f, 0.20f, 1);
    
	Output.Color = Output.Color * (diffuseLighting + xAmbient + specularLighting);
	//Output.Color = Output.Color * (diffuseLighting + xAmbient);

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

float4x4 xLightsWorldViewProjection;

struct SMapVertexToPixel
{
    float4 Position     : POSITION;
    float4 Position2D    : TEXCOORD0;
};

struct SMapPixelToFrame
{
    float4 Color : COLOR0;
};

SMapVertexToPixel ShadowMapVertexShader( float4 inPos : POSITION)
{
    SMapVertexToPixel Output = (SMapVertexToPixel)0;

    Output.Position = mul(inPos, xLightsWorldViewProjection);
    Output.Position2D = Output.Position;

    return Output;
}

SMapPixelToFrame ShadowMapPixelShader(SMapVertexToPixel PSIn) 
{
	SMapPixelToFrame Output = (SMapPixelToFrame)0;            
    Output.Color = PSIn.Position2D.z/PSIn.Position2D.w;

    return Output;
}

technique ShadowMap
{
    pass Pass0
    {
        VertexShader = compile vs_2_0 ShadowMapVertexShader();
        PixelShader = compile ps_2_0 ShadowMapPixelShader();
    }
}

Texture xSpotlight;
Texture xShadowMap;

sampler SpotlightSampler = sampler_state { texture = <xSpotlight> ; 
										   magfilter = LINEAR; 
										   minfilter = LINEAR; 
										   mipfilter=LINEAR; 
										   AddressU = clamp; 
										   AddressV = clamp;};

sampler ShadowMapSampler = sampler_state { texture = <xShadowMap> ; 
										   magfilter = LINEAR; 
										   minfilter = LINEAR; 
										   mipfilter=LINEAR; 
										   AddressU = clamp; 
										   AddressV = clamp;};

struct SSceneVertexToPixel
{
    float4 Position              : POSITION;
    float4 Pos2DAsSeenByLight    : TEXCOORD0;
    float2 TexCoords             : TEXCOORD1;
	float3 Normal                : TEXCOORD2;
	float4 Position3D            : TEXCOORD3;
};

struct SScenePixelToFrame
{
    float4 Color : COLOR0;
};

SSceneVertexToPixel ShadowedSceneVertexShader( float4 inPos : POSITION, float2 inTex : TEXCOORD0, float3 inNormal : NORMAL)
{
    SSceneVertexToPixel Output = (SSceneVertexToPixel)0;

    Output.Position = mul(inPos, xWorldViewProjection);   
    Output.TexCoords = inTex;
    Output.Normal = normalize(mul(inNormal, (float3x3) xWorld));
    Output.Position3D = mul(inPos, xWorld); 
    Output.Pos2DAsSeenByLight= mul(inPos, xLightsWorldViewProjection);    
    return Output;
}

SScenePixelToFrame ShadowedScenePixelShader(SSceneVertexToPixel PSIn)
{
    SScenePixelToFrame Output = (SScenePixelToFrame)0;    
	float diffuseLighting = 0;
	float specularLighting = 0;
	
	//load shadowMap
    float2 ProjectedTexCoords;
    ProjectedTexCoords[0] = PSIn.Pos2DAsSeenByLight.x/PSIn.Pos2DAsSeenByLight.w/2.0f +0.5f;
    ProjectedTexCoords[1] = -PSIn.Pos2DAsSeenByLight.y/PSIn.Pos2DAsSeenByLight.w/2.0f +0.5f;
	
	if ((saturate(ProjectedTexCoords).x == ProjectedTexCoords.x)
	     && (saturate(ProjectedTexCoords).y == ProjectedTexCoords.y)) {
	     
	      float depthStoredInShadowMap = tex2D(ShadowMapSampler, ProjectedTexCoords).r;
		  float realDistance = PSIn.Pos2DAsSeenByLight.z/PSIn.Pos2DAsSeenByLight.w;
	
		  if ((realDistance - 1.0f/100.0f) <= depthStoredInShadowMap) {
			 //diffuse
			  diffuseLighting = getDiffuseIntensity(xLightPos, PSIn.Position3D, PSIn.Normal);
			  diffuseLighting = saturate(diffuseLighting);
			  diffuseLighting = diffuseLighting * xLightIntensity;
			  
			  //specular
			  float3 LOS_v = normalize(PSIn.Position3D - xCameraPos);
			  specularLighting = getSpecularIntensity(xLightPos, PSIn.Position3D, PSIn.Normal, LOS_v);
			  specularLighting = saturate(specularLighting);
			  specularLighting = xSpecular * xLightIntensity * pow(specularLighting, xShininess);
		  }
	}

	//load textureMap
	float4 displayColor = tex2D(TextureSampler, PSIn.TexCoords);
	
	if (xSolidBrown == true)
         displayColor = float4(0.25f, 0.21f, 0.20f, 1);

	float spotlightIntensity = tex2D(SpotlightSampler, ProjectedTexCoords).b;

    Output.Color = spotlightIntensity * (displayColor * (diffuseLighting + xAmbient + specularLighting));

    return Output;
}

technique ShadowedScene
{
    pass Pass0
    {
        VertexShader = compile vs_3_0 ShadowedSceneVertexShader();
        PixelShader = compile ps_3_0 ShadowedScenePixelShader();
    }
}
