static const int max_lights = 3;
int num_lights;
int current_light;

float4x4 xLightViewProjection[3];
float4x4 xCameraViewProjection;
float4x4 xWorld;

Texture xTexture;
Texture xSpotlight;
Texture xShadowMap1;
Texture xShadowMap2;
Texture xShadowMap3;

float3 xCameraPos;

float3 xLightPos[3];
float xLightIntensity[3];

float xSpecular;
float xShininess;
float xAmbient;

struct SMapVertexToPixel
{
    float4 Position      : POSITION;
    float4 Position2D    : TEXCOORD0;
};

struct SMapPixelToFrame
{
    float4 Color : COLOR0;
};

SMapVertexToPixel ShadowMapVertexShader( float4 inPos : POSITION)
{
    SMapVertexToPixel Output = (SMapVertexToPixel)0;
    float4x4 xLight = mul(xWorld, xLightViewProjection[current_light]);

	Output.Position = mul(inPos, xLight);
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
        VertexShader = compile vs_3_0 ShadowMapVertexShader();
        PixelShader = compile ps_3_0 ShadowMapPixelShader();
    }
}

///////////////////////////////////////////////////////////////
///////////////////////////////////////////////////////////////
///////////////////////////////////////////////////////////////
///////////////////////////////////////////////////////////////
///////////////////////////////////////////////////////////////

sampler TextureSampler = sampler_state { texture = <xTexture> ; magfilter = LINEAR; minfilter = LINEAR; mipfilter=LINEAR; AddressU = mirror; AddressV = mirror;};

sampler SpotlightSampler = sampler_state { texture = <xSpotlight> ; magfilter = LINEAR; minfilter = LINEAR;  mipfilter=LINEAR; AddressU = clamp;  AddressV = clamp;};

sampler ShadowMapSampler1 = sampler_state { texture = <xShadowMap1> ;  magfilter = LINEAR;  minfilter = LINEAR;  mipfilter=LINEAR;  AddressU = clamp; AddressV = clamp;};
sampler ShadowMapSampler2 = sampler_state { texture = <xShadowMap2> ;  magfilter = LINEAR;  minfilter = LINEAR;  mipfilter=LINEAR;  AddressU = clamp; AddressV = clamp;};
sampler ShadowMapSampler3 = sampler_state { texture = <xShadowMap3> ;  magfilter = LINEAR;  minfilter = LINEAR;  mipfilter=LINEAR;  AddressU = clamp; AddressV = clamp;};

//Structs

struct SSceneVertexToPixel
{
    float4 Position              : POSITION;
    float4 Pos2DAsSeenByLight1   : TEXCOORD0;
    float4 Pos2DAsSeenByLight2   : TEXCOORD1;
    float4 Pos2DAsSeenByLight3   : TEXCOORD2;
    float2 TexCoords             : TEXCOORD3;
	float3 Normal                : TEXCOORD4;
	float4 Position3D            : TEXCOORD5;
};

struct SScenePixelToFrame
{
    float4 Color : COLOR0;
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

SSceneVertexToPixel ShadowedSceneVertexShader( float4 inPos : POSITION, float2 inTex : TEXCOORD0, float3 inNormal : NORMAL)
{
    SSceneVertexToPixel Output = (SSceneVertexToPixel)0;

	float4x4 xWorldViewProjection = mul(xWorld, xCameraViewProjection);
	float4x4 xLight[max_lights];
	
	for(int i = 0; i < num_lights; i++) {
		xLight[i] = mul(xWorld, xLightViewProjection[i]);
	}
	
    Output.Position = mul(inPos, xWorldViewProjection);   
    Output.TexCoords = inTex;
    Output.Normal = normalize(mul(inNormal, (float3x3) xWorld));
    Output.Position3D = mul(inPos, xWorld); 
    
    if(num_lights >= 1) {
		Output.Pos2DAsSeenByLight1 = mul(inPos, xLight[0]);     
    } 
    
    if (num_lights >= 2) {
		Output.Pos2DAsSeenByLight2 = mul(inPos, xLight[1]);  
    } 
    
    if (num_lights >= 3) {
		Output.Pos2DAsSeenByLight3 = mul(inPos, xLight[2]);  
    }

    return Output;
}

SScenePixelToFrame ShadowedScenePixelShader(SSceneVertexToPixel PSIn)
{
    SScenePixelToFrame Output = (SScenePixelToFrame)0;    
	float diffuseLighting[max_lights];
	float specularLighting[max_lights];
	float spotlightIntensity[max_lights];
	
	//load shadowMap
    float2 ProjectedTexCoords[max_lights];
	
	if(num_lights >= 1) {
		ProjectedTexCoords[0].x =  PSIn.Pos2DAsSeenByLight1.x/PSIn.Pos2DAsSeenByLight1.w/2.0f +0.5f;
		ProjectedTexCoords[0].y = -PSIn.Pos2DAsSeenByLight1.y/PSIn.Pos2DAsSeenByLight1.w/2.0f +0.5f;  
		spotlightIntensity[0]   = tex2D(SpotlightSampler, ProjectedTexCoords[0]).b;
    } 
    
    if (num_lights >= 2) {
		ProjectedTexCoords[1].x =  PSIn.Pos2DAsSeenByLight2.x/PSIn.Pos2DAsSeenByLight2.w/2.0f +0.5f;
		ProjectedTexCoords[1].y = -PSIn.Pos2DAsSeenByLight2.y/PSIn.Pos2DAsSeenByLight2.w/2.0f +0.5f;  
		spotlightIntensity[1]   = tex2D(SpotlightSampler, ProjectedTexCoords[1]).b;
    } 
    
    if (num_lights == 3) {
		ProjectedTexCoords[2].x =  PSIn.Pos2DAsSeenByLight3.x/PSIn.Pos2DAsSeenByLight3.w/2.0f +0.5f;
		ProjectedTexCoords[2].y = -PSIn.Pos2DAsSeenByLight3.y/PSIn.Pos2DAsSeenByLight3.w/2.0f +0.5f; 
		spotlightIntensity[2]   = tex2D(SpotlightSampler, ProjectedTexCoords[2]).b; 
    }
	
	float depthStoredInShadowMap;
	float realDistance;
	
	for(int i = 0; i < num_lights; i++) {
		
		if ((saturate(ProjectedTexCoords[i]).x == ProjectedTexCoords[i].x)
			 && (saturate(ProjectedTexCoords[i]).y == ProjectedTexCoords[i].y)) {
		     
		     if(i == 0) {
				depthStoredInShadowMap = tex2D(ShadowMapSampler1, ProjectedTexCoords[i]).r;
				realDistance = PSIn.Pos2DAsSeenByLight1.z/PSIn.Pos2DAsSeenByLight1.w;
		     } else if (i == 1) {
				depthStoredInShadowMap = tex2D(ShadowMapSampler2, ProjectedTexCoords[i]).r;
				realDistance = PSIn.Pos2DAsSeenByLight2.z/PSIn.Pos2DAsSeenByLight2.w;
		     } else if (i == 2) {
				depthStoredInShadowMap = tex2D(ShadowMapSampler3, ProjectedTexCoords[i]).r;
				realDistance = PSIn.Pos2DAsSeenByLight3.z/PSIn.Pos2DAsSeenByLight3.w;
		     }
		
			  if ((realDistance - 1.0f/100.0f) <= depthStoredInShadowMap) {
				 //diffuse
				  diffuseLighting[i] = getDiffuseIntensity(xLightPos[i], PSIn.Position3D, PSIn.Normal);
				  diffuseLighting[i] = saturate(diffuseLighting[i]);
				  diffuseLighting[i] = diffuseLighting[i] * xLightIntensity[i];
				  
				  //specular
				  float3 LOS_v = normalize(PSIn.Position3D - xCameraPos);
				  specularLighting[i] = getSpecularIntensity(xLightPos[i], PSIn.Position3D, PSIn.Normal, LOS_v);
				  specularLighting[i] = saturate(specularLighting[i]);
				  specularLighting[i] = xSpecular * xLightIntensity[i] * pow(specularLighting[i], xShininess);
			  }
		}
	}
	
	//load textureMap
	float4 displayColor = tex2D(TextureSampler, PSIn.TexCoords);
	
	for(int i = 0; i < num_lights; i++) {
		//Output.Color += (displayColor * (diffuseLighting[i] + xAmbient + specularLighting[i]));
		Output.Color += spotlightIntensity[i] * (displayColor * (diffuseLighting[i] + xAmbient + specularLighting[i]));
	}

    Output.Color /= num_lights;

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

///////////////////////////////////////////////////////////////
///////////////////////////////////////////////////////////////
///////////////////////////////////////////////////////////////
///////////////////////////////////////////////////////////////
///////////////////////////////////////////////////////////////

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

VertexToPixel SimplestVertexShader( float4 inPos : POSITION, float2 inTexCoords : TEXCOORD0, float3 inNormal: NORMAL0)
{
    VertexToPixel Output = (VertexToPixel)0;
    float4x4 xWVP = mul(xWorld, xCameraViewProjection);
    Output.Position =mul(inPos, xWVP);
    Output.Position3D = mul(inPos, xWorld);
    Output.Normal = normalize(mul(inNormal, (float3x3)xWorld));
    Output.TexCoords = inTexCoords;
    return Output;
}

PixelToFrame OurFirstPixelShader(VertexToPixel PSIn)
{
	float diffuseLighting = getDiffuseIntensity(xLightPos[0], PSIn.Position3D, PSIn.Normal);
	diffuseLighting = saturate(diffuseLighting);
	diffuseLighting = diffuseLighting * xLightIntensity[0];

    float3 LOS_v = normalize(PSIn.Position3D - xCameraPos);
	float specularLighting = getSpecularIntensity(xLightPos[0], PSIn.Position3D, PSIn.Normal, LOS_v);
	specularLighting = saturate(specularLighting);
	specularLighting = xSpecular * xLightIntensity[0] * pow(specularLighting, xShininess);

    PixelToFrame Output = (PixelToFrame)0;    
    Output.Color = tex2D(TextureSampler, PSIn.TexCoords);
    
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