using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using System.ComponentModel;
using System.Text;
using System.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using Microsoft.Xna.Framework.Net;
using Microsoft.Xna.Framework.Storage;
using Definitions;

namespace Muffin.Components.Renderer
{
    /// <summary>
    /// This is the main type for your game
    /// </summary>
    public class Renderer : DrawableGameComponent
    {
        struct MyOwnVertexFormat
        {
            private Vector3 position;
            private Vector2 texcoord;
            private Vector3 normal;

            public MyOwnVertexFormat(Vector3 position, Vector2 tex, Vector3 normal)
            {
                this.position = position;
                this.texcoord = tex;
                this.normal = normal;
            }

            public static VertexElement[] VertexElements =
             {
                 new VertexElement(0, 0, VertexElementFormat.Vector3, VertexElementMethod.Default, VertexElementUsage.Position, 0),
                 new VertexElement(0, sizeof(float)*3, VertexElementFormat.Vector2, VertexElementMethod.Default, VertexElementUsage.TextureCoordinate, 0),
                 new VertexElement(0, sizeof(float)*5, VertexElementFormat.Vector3, VertexElementMethod.Default, VertexElementUsage.Normal, 0),
             };

            public static int SizeInBytes = sizeof(float) * (3 + 2 + 3);
        }

        public enum LightType { POINT, DIRECTIONAL };

        //light data
        struct Light
        {
            public Matrix LightViewProjectionMatrix;
            public Matrix InvViewMatrix;
            public Vector3 LightPos;
            public Vector3 LightDir;
            public float LightIntensity;

            public float ConeAngle;
            public float ConeDecay;
            public LightType Type;
        }

        public float Ambient;
        public float Specular;
        public float Shininess;

        MuffinGame m_game;
        GraphicsDeviceManager graphics;
        GraphicsDevice device;
        SpriteBatch spriteBatch;

        /// deferred rendering varaibles
        Effect deferredLighting;
        Effect deferredShading;
        Effect deferredCombined;
        Effect deferredShadow;

        VertexPositionTexture[] quadVertices;

        GameCamera camera;
        MouseHandler m_mouse;
        Texture2D spotlightTexture;
        Texture2D tex;

        List<GameObject> m_objects;
        List<Model> m_models;
        List<Texture2D[]> m_model_textures;

        Light[] m_lights;
        Matrix viewProjection;
        Matrix viewProjectionInverse;

        RenderTarget2D[] deferredRenderTarget = new RenderTarget2D[3];
        Texture2D[] deferredRenderMap = new Texture2D[3];
        RenderTarget2D deferredShadingTarget;
        RenderTarget2D deferredShadowTarget;
        Texture2D deferredShadingMap;
        Texture2D deferredShadowMap;
        Texture2D blackTexture;
        DepthStencilBuffer shadowDSB;
        DepthStencilBuffer standardDSB;

        Matrix handle_original;
        bool isLoaded = false;

        public Renderer(MuffinGame game)
            : base(game)
        {
            //renderer settings
            m_game = game;
            graphics = m_game.graphics;
            graphics.IsFullScreen = GameConstants.FULL_SCREEN_ENABLED;
            graphics.PreferredBackBufferHeight = GameConstants.SCREEN_HEIGHT;
            graphics.PreferredBackBufferWidth = GameConstants.SCREEN_WIDTH;
            //enable anti-aliasing
            //graphics.PreferMultiSampling = true;
            //graphics.PreparingDeviceSettings +=
            //    new EventHandler<PreparingDeviceSettingsEventArgs>(graphics_PreparingDeviceSettings);
            //mouse handler
            //m_mouse = new MouseHandler(Mouse.GetState());
            m_models = new List<Model>();
            m_objects = m_game.allObjects;
            m_model_textures = new List<Texture2D[]>();
            game.IsMouseVisible = true;
        }

        void graphics_PreparingDeviceSettings(object sender, PreparingDeviceSettingsEventArgs e)
        {
            int quality = 0;
            GraphicsAdapter adapter = e.GraphicsDeviceInformation.Adapter;
            SurfaceFormat format = adapter.CurrentDisplayMode.Format;
            // Check for 4xAA
            if (adapter.CheckDeviceMultiSampleType(DeviceType.Hardware, format,
                false, MultiSampleType.FourSamples, out quality))
            {
                // even if a greater quality is returned, we only want quality 0
                e.GraphicsDeviceInformation.PresentationParameters.MultiSampleQuality = 0;
                e.GraphicsDeviceInformation.PresentationParameters.MultiSampleType =
                    MultiSampleType.FourSamples;
            }
            // Check for 2xAA
            else if (adapter.CheckDeviceMultiSampleType(DeviceType.Hardware, format,
                false, MultiSampleType.TwoSamples, out quality))
            {
                // even if a greater quality is returned, we only want quality 0
                e.GraphicsDeviceInformation.PresentationParameters.MultiSampleQuality = 0;
                e.GraphicsDeviceInformation.PresentationParameters.MultiSampleType =
                    MultiSampleType.TwoSamples;
            }
            return;

        }

        /// <summary>
        /// Allows the game to perform any initialization it needs to before starting to run.
        /// This is where it can query for any required services and load any non-graphic
        /// related content.  Calling base.Initialize will enumerate through any components
        /// and initialize them as well.
        /// </summary>
        public override void Initialize()
        {
            //load 2D quad to render 3D scene to a texture
            InitQuad();
            base.Initialize();
        }

        private Model LoadModel(string assetName, out Texture2D[] textures)
        {
            Model newModel = m_game.Content.Load<Model>("Models/" + assetName);
            int numTextures = 0;
            foreach (ModelMesh mesh in newModel.Meshes)
                foreach (BasicEffect currentEffect in mesh.Effects)
                    numTextures++;
            textures = new Texture2D[numTextures];
            int i = 0;
            //extract all the textures from the model
            foreach (ModelMesh mesh in newModel.Meshes)
                foreach (BasicEffect currentEffect in mesh.Effects)
                    textures[i++] = currentEffect.Texture;
            //set meshPart.Effect to custom hardware shader
            foreach (ModelMesh mesh in newModel.Meshes)
                foreach (ModelMeshPart meshPart in mesh.MeshParts)
                    meshPart.Effect = deferredLighting.Clone(device);

            return newModel;
        }

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            // Create a new SpriteBatch, which can be used to draw textures.
            device = graphics.GraphicsDevice;
            spriteBatch = new SpriteBatch(device);
            //load effects for rendering
            deferredShadow = m_game.Content.Load<Effect>("Effects/RenderEffect");
            deferredLighting = m_game.Content.Load<Effect>("Effects/DeferredLighting");
            deferredShading = m_game.Content.Load<Effect>("Effects/DeferredShading");
            deferredCombined = m_game.Content.Load<Effect>("Effects/DeferredCombinedEffect");

            //load models
            //DepthStencil Buffer and Corresponding RenderTarget for shadowMap
            PresentationParameters pp = device.PresentationParameters;
            SurfaceFormat format = device.PresentationParameters.BackBufferFormat;
            DepthFormat dFormat = device.DepthStencilBuffer.Format;
            //MultiSampleType type = MultiSampleType.FourSamples;
            int height = graphics.PreferredBackBufferHeight;
            int width = graphics.PreferredBackBufferWidth;
            //int quality = 0;

            //color
            deferredRenderTarget[0] = new RenderTarget2D(device, width, height, 1, SurfaceFormat.Color);
            //normaal
            deferredRenderTarget[1] = new RenderTarget2D(device, width, height, 1, SurfaceFormat.Color);
            //depth
            deferredRenderTarget[2] = new RenderTarget2D(device, width, height, 1, SurfaceFormat.Single);
            //shading (for lights)
            deferredShadingTarget = new RenderTarget2D(device, width, height, 1, SurfaceFormat.Color);
            //shadows
            deferredShadowTarget = new RenderTarget2D(device, width, height, 1, SurfaceFormat.Single);

            blackTexture = new Texture2D(device, width, height, 1, TextureUsage.None, SurfaceFormat.Color);

            shadowDSB = new DepthStencilBuffer(graphics.GraphicsDevice, width, height, dFormat);
            //save pointer to default DSB                                               
            standardDSB = device.DepthStencilBuffer;

            Texture2D[] modelTexture;
            m_models.Add(LoadModel("ai", out modelTexture));
            m_model_textures.Add(modelTexture);
            m_models.Add(LoadModel("box", out modelTexture));
            m_model_textures.Add(modelTexture);
            m_models.Add(LoadModel("coin", out modelTexture));
            m_model_textures.Add(modelTexture);
            m_models.Add(LoadModel("grass", out modelTexture));
            m_model_textures.Add(modelTexture);
            m_models.Add(LoadModel("player", out modelTexture));
            m_model_textures.Add(modelTexture);
            m_models.Add(LoadModel("star", out modelTexture));
            m_model_textures.Add(modelTexture);

            setModels();
            //SetUpCamera();
            SetUpLights();

            isLoaded = true;
        }

        public void setModels()
        {
            foreach (GameObject o in m_objects)
            {
                switch (o.modelName)
                {
                    case ModelName.AI:
                        o.model = m_models.ElementAt((int)ModelName.AI);
                        break;
                    case ModelName.BOX:
                        o.model = m_models.ElementAt((int)ModelName.BOX);
                        break;
                    case ModelName.COIN:
                        o.model = m_models.ElementAt((int)ModelName.COIN);
                        break;
                    case ModelName.GRASS:
                        o.model = m_models.ElementAt((int)ModelName.GRASS);
                        break;
                    case ModelName.PLAYER:
                        o.model = m_models.ElementAt((int)ModelName.PLAYER);
                        break;
                    case ModelName.STAR:
                        o.model = m_models.ElementAt((int)ModelName.STAR);
                        break;
                    case ModelName.NONE:
                    default:
                        break;
                }
            }
        }

        private void InitQuad()
        {
            quadVertices = new VertexPositionTexture[4];
            quadVertices[0] = new VertexPositionTexture(new Vector3(-1, 1, 0), new Vector2(0, 0));
            quadVertices[1] = new VertexPositionTexture(new Vector3(1, 1, 1), new Vector2(1, 0));
            quadVertices[2] = new VertexPositionTexture(new Vector3(-1, -1, 3), new Vector2(0, 1));
            quadVertices[3] = new VertexPositionTexture(new Vector3(1, -1, 2), new Vector2(1, 1));
        }

        public void SetUpCamera(GameCamera c)
        {
            
            camera = c;
            //camera = new GameCamera(100 * new Vector3(-20, 40, -20), new Vector3(1000, 200, 1000), device.Viewport.AspectRatio);
        }

        Vector3 look = new Vector3(0.1f, -6.88f, 3.20f);
        Vector3 lightloc = new Vector3(4910, 12120, -1230);
        float NEAR_CLIP = 9000.0f;
        float FAR_CLIP = 17200.0f;

        private void SetUpLights()
        {
            m_lights = new Light[3];

            m_lights[0].LightPos = lightloc;
            m_lights[1].LightPos = new Vector3(400, 300, 400);
            m_lights[2].LightPos = new Vector3(-30, 50, 0);

            m_lights[0].LightDir = new Vector3(20 + look.X, -20 + look.Y, -20 + look.Z);
            m_lights[1].LightDir = new Vector3(2, -1, 2);
            m_lights[2].LightDir = new Vector3(30 + look.X, -45 + look.Y, -20 + look.Z);

            m_lights[0].LightIntensity = 0.7f;
            m_lights[1].LightIntensity = 1.0f;
            m_lights[2].LightIntensity = 0.0f;

            m_lights[0].ConeAngle = 40.0f;
            m_lights[1].ConeAngle = 60.0f;
            m_lights[2].ConeAngle = 20.0f;

            m_lights[0].ConeDecay = 0.0f;
            m_lights[1].ConeDecay = 0.0f;
            m_lights[2].ConeDecay = 0.6f;

            Ambient = 0.3f;
            Specular = 0.54f;
            Shininess = 18.0f;

            for (int i = 0; i < GameConstants.MaxLights; i++)
            {
                Matrix LightView = Matrix.CreateLookAt(m_lights[i].LightPos, m_lights[i].LightPos + look, Vector3.Up);
                Matrix LightProjection = Matrix.CreatePerspectiveFieldOfView(MathHelper.PiOver4, device.Viewport.AspectRatio, NEAR_CLIP, FAR_CLIP);
                //matrix used in calculating shadow maps
                m_lights[i].LightViewProjectionMatrix = LightView * LightProjection;
            }

            viewProjection = camera.ViewMatrix * camera.ProjectionMatrix;
            viewProjectionInverse = Matrix.Invert(viewProjection);
        }

        /// <summary>
        /// UnloadContent will be called once per game and is the place to unload
        /// all content.
        /// </summary>
        protected override void UnloadContent()
        {
            // TODO: Unload any non ContentManager content here
        }


        private void UpdateLights()
        {
            viewProjection = camera.ViewMatrix * camera.ProjectionMatrix;
            viewProjectionInverse = Matrix.Invert(viewProjection);

            KeyboardState state = Keyboard.GetState();

            bool hit = false;
            bool hit2 = false;
            bool hit3 = false;
            /*
            if (state.IsKeyDown(Keys.R))
            {
                lightloc.X += 10;
                hit = true;
            }
            if (state.IsKeyDown(Keys.T))
            {
                lightloc.Y += 10;
                hit = true;
            }
            if (state.IsKeyDown(Keys.Y))
            {
                lightloc.Z += 10;
                hit = true;
            }
            if (state.IsKeyDown(Keys.F))
            {
                lightloc.X -= 10;
                hit = true;
            }
            if (state.IsKeyDown(Keys.G))
            {
                lightloc.Y -= 10;
                hit = true;
            }
            if (state.IsKeyDown(Keys.H))
            {
                lightloc.Z -= 10;
                hit = true;
            }

            m_lights[0].LightPos = lightloc;

            if (hit)
            {
                Console.WriteLine("Light Position: " + lightloc);
            }


            if (state.IsKeyDown(Keys.U))
            {
                look.X += 0.10f;
                hit2 = true;
            }
            if (state.IsKeyDown(Keys.I))
            {
                look.Y += 0.10f;
                hit2 = true;
            }
            if (state.IsKeyDown(Keys.O))
            {
                look.Z += 0.10f;
                hit2 = true;
            }
            if (state.IsKeyDown(Keys.J))
            {
                look.X -= 0.10f;
                hit2 = true;
            }
            if (state.IsKeyDown(Keys.K))
            {
                look.Y -= 0.10f;
                hit2 = true;
            }
            if (state.IsKeyDown(Keys.L))
            {
                look.Z -= 0.10f;
                hit2 = true;
            }

            if (state.IsKeyDown(Keys.V))
            {
                NEAR_CLIP += 100.0f;
                hit3 = true;
            }

            if (state.IsKeyDown(Keys.B))
            {
                NEAR_CLIP -= 100.0f;
                hit3 = true;
            }

            if (state.IsKeyDown(Keys.N))
            {
                FAR_CLIP += 100.0f;
                hit3 = true;
            }

            if (state.IsKeyDown(Keys.M))
            {
                FAR_CLIP -= 100.0f;
                hit3 = true;
            }

            for (int i = 0; i < GameConstants.MaxLights; i++)
            {
                Matrix LightView = Matrix.CreateLookAt(m_lights[i].LightPos, m_lights[i].LightPos + look, Vector3.Up);
                Matrix LightProjection = Matrix.CreatePerspectiveFieldOfView(MathHelper.PiOver4, device.Viewport.AspectRatio, NEAR_CLIP, FAR_CLIP);
                //matrix used in calculating shadow maps
                m_lights[i].LightViewProjectionMatrix = LightView * LightProjection;
            }

            if (hit2)
            {
                Console.WriteLine("Light Target: " + look);
            }

            if (hit3) 
            {
                Console.WriteLine("NEAR: " + NEAR_CLIP + " FAR: " + FAR_CLIP);
            }
             */
        }

        GamePadState gstate;

        private void UpdateCamera()
        {
            camera.Update();
            //Console.WriteLine(m_game.allPlayer.ElementAt(0).rotation);
        }

        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        public override void Update(GameTime gameTime)
        {
            if (!isLoaded)
            {
                return;
            }

            // TODO: Add your update logic here
            UpdateCamera();
            UpdateLights();

            base.Update(gameTime);
        }

        private void renderDeferredMaps()
        {
            for (int i = 0; i < 3; i++)
            {
                device.SetRenderTarget(i, deferredRenderTarget[i]);
            }

            device.Clear(ClearOptions.Target | ClearOptions.DepthBuffer, Color.Black, 1.0f, 0);
            switchEffect(deferredLighting);

            DrawAll("MultipleTargets");

            for (int i = 0; i < 3; i++)
            {
                device.SetRenderTarget(i, null);
                deferredRenderMap[i] = deferredRenderTarget[i].GetTexture();
            }
        }

        private void renderShadow(Light light)
        {
            device.SetRenderTarget(0, deferredShadowTarget);

            device.Clear(Color.White);

            deferredShadow.CurrentTechnique = deferredShadow.Techniques["ShadowMap"];
            deferredShadow.Parameters["xLightViewProjection"].SetValue(light.LightViewProjectionMatrix);

            switchEffect(deferredShadow);
            DrawAll("ShadowMap");

            device.SetRenderTarget(0, null);
            deferredShadowMap = deferredShadowTarget.GetTexture();
        }

        private void renderLight(Light light)
        {
            device.SetRenderTarget(0, deferredShadingTarget);

            deferredShading.CurrentTechnique = deferredShading.Techniques["DeferredShading"];
            deferredShading.Parameters["xShadingMap"].SetValue(deferredShadingMap);
            deferredShading.Parameters["xNormalMap"].SetValue(deferredRenderMap[1]);
            deferredShading.Parameters["xDepthMap"].SetValue(deferredRenderMap[2]);
            deferredShading.Parameters["xShadowMap"].SetValue(deferredShadowMap);
            deferredShading.Parameters["xLightPos"].SetValue(light.LightPos);
            deferredShading.Parameters["xLightDir"].SetValue(dir);
            deferredShading.Parameters["xLightIntensity"].SetValue(light.LightIntensity);
            deferredShading.Parameters["xConeDirection"].SetValue(light.LightDir);
            deferredShading.Parameters["xConeAngle"].SetValue(light.ConeAngle);
            deferredShading.Parameters["xConeDecay"].SetValue(light.ConeDecay);
            deferredShading.Parameters["xCameraPos"].SetValue(camera.cameraPosition);

            deferredShading.Parameters["xViewProjectionInverse"].SetValue(viewProjectionInverse);
            deferredShading.Parameters["xLightViewProjection"].SetValue(light.LightViewProjectionMatrix);

            deferredShading.Begin();
            foreach (EffectPass pass in deferredShading.CurrentTechnique.Passes)
            {
                pass.Begin();
                device.VertexDeclaration = new VertexDeclaration(device, VertexPositionTexture.VertexElements);
                device.DrawUserPrimitives<VertexPositionTexture>(PrimitiveType.TriangleStrip, quadVertices, 0, 2);
                pass.End();
            }
            deferredShading.End();

            device.SetRenderTarget(0, null);
            deferredShadingMap = deferredShadingTarget.GetTexture();
        }

        private Texture2D getShadingMap()
        {

            deferredShadingMap = blackTexture;
           
            for (int i = 0; i < GameConstants.MaxLights; i++)
            {
               // calculateFrustum(m_lights[i]);
                renderShadow(m_lights[i]);
                renderLight(m_lights[i]);
            }

            

            return deferredShadingTarget.GetTexture();

            //return blackTexture;
        }

        private void renderCombinedEffects()
        {
            deferredCombined.CurrentTechnique = deferredCombined.Techniques["DeferredCombined"];
            deferredCombined.Parameters["xColorMap"].SetValue(deferredRenderMap[0]);
            deferredCombined.Parameters["xNormalMap"].SetValue(deferredRenderMap[1]);
            deferredCombined.Parameters["xShadingMap"].SetValue(deferredShadingMap);
            deferredCombined.Parameters["xAmbient"].SetValue(Ambient);
            
            deferredCombined.Begin();
            foreach (EffectPass pass in deferredCombined.CurrentTechnique.Passes)
            {
                pass.Begin();
                device.VertexDeclaration = new VertexDeclaration(device, VertexPositionTexture.VertexElements);
                device.DrawUserPrimitives<VertexPositionTexture>(PrimitiveType.TriangleStrip, quadVertices, 0, 2);
                pass.End();
            }
            deferredCombined.End();
        }

        private void switchEffect(Effect effect)
        {
            foreach (Model m in m_models)
                foreach (ModelMesh mesh in m.Meshes)
                    foreach (ModelMeshPart meshPart in mesh.MeshParts)
                        meshPart.Effect = effect;
        }

        public override void Draw(GameTime gameTime)
        {
            if (!isLoaded)
            {
                return;
            }

            //Console.WriteLine(camera.cameraPosition);

            //switch effect for models
            //render color map, normal map, and depth map of scene
            renderDeferredMaps();

            PresentationParameters pp = device.PresentationParameters;
            
            //render lighting contributions from lights in the scene
            deferredShadingMap = getShadingMap();
            
            renderCombinedEffects();
            //for debugging
            /*spriteBatch.Begin(SpriteBlendMode.AlphaBlend, SpriteSortMode.Deferred, SaveStateMode.SaveState);
            spriteBatch.Draw(deferredShadowMap, new Rectangle(0, 0, 400, 300), Color.White);
            spriteBatch.End();
            */
            base.Draw(gameTime);
        }

        private void DrawAll(String technique)
        {
            foreach (GameObject o in m_objects)
            {
                Matrix worldMatrix = o.worldMatrix();
                if (o is PlayerObject)
                {
                    DrawPlayer(o.model, m_model_textures.ElementAt((int)o.modelName), worldMatrix, technique);
                }
                else
                {
                    DrawModel(o.model, m_model_textures.ElementAt((int)o.modelName), worldMatrix, technique);
                }
            }
        }
        
        Vector3 dir = new Vector3(1, -1, 1);
        float handleRot = 0.0f;
        
        private void DrawPlayer(Model model, Texture2D[] textures, Matrix wMatrix, string technique)
        {
            Matrix[] modelTransforms = new Matrix[model.Bones.Count];
            Matrix[] origTransforms = new Matrix[model.Bones.Count];
            model.CopyBoneTransformsTo(origTransforms);

            int i = 0;

            //Console.WriteLine("start");
            //for (int j = 0; j < model.Bones.Count; j++)
            //{
                //Console.WriteLine(j + " " + model.Bones[j].Name);
            //}

            handleRot += MathHelper.ToRadians(2.0f);
            Vector3 trans = handle_original.Translation;

            int num = 19;
            if (num <= model.Bones.Count)
            {
                model.Bones[19].Transform = Matrix.CreateFromYawPitchRoll(0, 0, handleRot) * origTransforms[19];
            }

            model.CopyAbsoluteBoneTransformsTo(modelTransforms);

            foreach (ModelMesh mesh in model.Meshes)
            {
                foreach (Effect currentEffect in mesh.Effects)
                {
                    Matrix worldMatrix = modelTransforms[mesh.ParentBone.Index] * wMatrix;
                    Matrix worldMatrixInv = Matrix.Invert(worldMatrix);
                    currentEffect.CurrentTechnique = currentEffect.Techniques[technique];
                    currentEffect.Parameters["xCameraViewProjection"].SetValue(viewProjection);
                    currentEffect.Parameters["xCameraPos"].SetValue(camera.cameraPosition);
                    currentEffect.Parameters["xLightPos"].SetValue(m_lights[0].LightPos);
                    currentEffect.Parameters["xWorld"].SetValue(worldMatrix);
                    currentEffect.Parameters["xTexture"].SetValue(textures[i++]);
                }
                mesh.Draw();
            }
        }

        private void DrawModel(Model model, Texture2D[] textures, Matrix wMatrix, string technique)
        {
            Matrix[] modelTransforms = new Matrix[model.Bones.Count];
            model.CopyAbsoluteBoneTransformsTo(modelTransforms);
            int i = 0;

            
            foreach (ModelMesh mesh in model.Meshes)
            {
                foreach (Effect currentEffect in mesh.Effects)
                {
                    Matrix worldMatrix = modelTransforms[mesh.ParentBone.Index] * wMatrix;
                    Matrix worldMatrixInv = Matrix.Invert(worldMatrix);
                    currentEffect.CurrentTechnique = currentEffect.Techniques[technique];
                    currentEffect.Parameters["xCameraViewProjection"].SetValue(viewProjection);
                    currentEffect.Parameters["xCameraPos"].SetValue(camera.cameraPosition);
                    currentEffect.Parameters["xLightPos"].SetValue(m_lights[0].LightPos);
                    currentEffect.Parameters["xWorld"].SetValue(worldMatrix);
                    currentEffect.Parameters["xTexture"].SetValue(textures[i++]);
                }
                mesh.Draw();
            }
        }
    }
}
