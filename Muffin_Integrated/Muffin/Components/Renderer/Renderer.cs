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

        //light data
        struct Light
        {
            public Matrix LightViewProjectionMatrix;
            public Vector3 LightPos;
            public Vector3 LightDir;
            public float LightIntensity;

            public float ConeAngle;
            public float ConeDecay;
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
        VertexPositionTexture[] menuVertices;

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

        bool isLoaded = false;

        public Renderer(MuffinGame game)
            : base(game)
        {
            //renderer settings
            m_game = game;
            graphics = m_game.graphics;
            graphics.IsFullScreen = true;
            graphics.PreferredBackBufferHeight = 1200;
            graphics.PreferredBackBufferWidth = 1920;
            //enable anti-aliasing
            //graphics.PreferMultiSampling = true;
            //graphics.PreparingDeviceSettings +=
            //    new EventHandler<PreparingDeviceSettingsEventArgs>(graphics_PreparingDeviceSettings);
            //mouse handler
            m_mouse = new MouseHandler(Mouse.GetState());
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



            //load textures
            tex = m_game.Content.Load<Texture2D>("Textures\\muffin");
            spotlightTexture = m_game.Content.Load<Texture2D>("Textures/spotlight");
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
            deferredShadingTarget = new RenderTarget2D(device, width, height, 1, format);
            //shadows
            deferredShadowTarget = new RenderTarget2D(device, width, height, 1, SurfaceFormat.Single);

            blackTexture = new Texture2D(device, width, height, 1, TextureUsage.None, SurfaceFormat.Color);

            shadowDSB = new DepthStencilBuffer(graphics.GraphicsDevice, width, height, dFormat);
            //save pointer to default DSB                                               
            standardDSB = device.DepthStencilBuffer;

            Texture2D[] modelTexture;
            m_models.Add(LoadModel("flat", out modelTexture));
            m_model_textures.Add(modelTexture);
            m_models.Add(LoadModel("wedge", out modelTexture));
            m_model_textures.Add(modelTexture);
            m_models.Add(LoadModel("corner", out modelTexture));
            m_model_textures.Add(modelTexture);
            m_models.Add(LoadModel("inverted_corner", out modelTexture));
            m_model_textures.Add(modelTexture);
            m_models.Add(LoadModel("box", out modelTexture));
            m_model_textures.Add(modelTexture);
            m_models.Add(LoadModel("player", out modelTexture));
            m_model_textures.Add(modelTexture);

            foreach (GameObject o in m_objects)
            {
                switch (o.modelName)
                {
                    case ModelName.BOX:
                        o.model = m_models.ElementAt((int)ModelName.BOX);
                        break;
                    case ModelName.CORNER:
                        o.model = m_models.ElementAt((int)ModelName.CORNER);
                        break;
                    case ModelName.FLAT:
                        o.model = m_models.ElementAt((int)ModelName.FLAT);
                        break;
                    case ModelName.INVERTED_CORNER:
                        o.model = m_models.ElementAt((int)ModelName.INVERTED_CORNER);
                        break;
                    case ModelName.WEDGE:
                        o.model = m_models.ElementAt((int)ModelName.WEDGE);
                        break;
                    case ModelName.PLAYER:
                        o.model = m_models.ElementAt((int)ModelName.PLAYER);
                        break;
                    case ModelName.NONE:
                    default:
                        break;
                }
            }

            //SetUpCamera();
            SetUpLights();

            isLoaded = true;
        }

        private void InitQuad()
        {
            quadVertices = new VertexPositionTexture[4];
            quadVertices[0] = new VertexPositionTexture(new Vector3(-1, 1, 0.0f), new Vector2(0, 0));
            quadVertices[1] = new VertexPositionTexture(new Vector3(1, 1, 0.0f), new Vector2(1, 0));
            quadVertices[2] = new VertexPositionTexture(new Vector3(-1, -1, 0.0f), new Vector2(0, 1));
            quadVertices[3] = new VertexPositionTexture(new Vector3(1, -1, 0.0f), new Vector2(1, 1));

            menuVertices = new VertexPositionTexture[4];
            menuVertices[0] = new VertexPositionTexture(new Vector3(-0.25f, 0.25f, 0.0f), new Vector2(0, 0));
            menuVertices[1] = new VertexPositionTexture(new Vector3(0.25f, 0.25f, 0.0f), new Vector2(1, 0));
            menuVertices[2] = new VertexPositionTexture(new Vector3(-0.25f, -0.25f, 0.0f), new Vector2(0, 1));
            menuVertices[3] = new VertexPositionTexture(new Vector3(0.25f, -0.25f, 0.0f), new Vector2(1, 1));
        }

        public void SetUpCamera(GameCamera c)
        {
            
            camera = c;
            //camera = new GameCamera(100 * new Vector3(-20, 40, -20), new Vector3(1000, 200, 1000), device.Viewport.AspectRatio);
        }

        Vector3 look = new Vector3(1000, 200, 1000);
        Vector3 lightloc = 20 * new Vector3(-20, 150, -20);

        private void SetUpLights()
        {
            m_lights = new Light[3];

            m_lights[0].LightPos = lightloc;
            m_lights[1].LightPos = new Vector3(400, 300, 400);
            m_lights[2].LightPos = new Vector3(-30, 50, 0);

            m_lights[0].LightDir = new Vector3(20 + look.X, -20 + look.Y, -20 + look.Z);
            m_lights[1].LightDir = new Vector3(2, -1, 2);
            m_lights[2].LightDir = new Vector3(30 + look.X, -45 + look.Y, -20 + look.Z);

            m_lights[0].LightIntensity = 1.0f;
            m_lights[1].LightIntensity = 1.0f;
            m_lights[2].LightIntensity = 0.0f;

            m_lights[0].ConeAngle = 40.0f;
            m_lights[1].ConeAngle = 60.0f;
            m_lights[2].ConeAngle = 20.0f;

            m_lights[0].ConeDecay = 0.0f;
            m_lights[1].ConeDecay = 0.0f;
            m_lights[2].ConeDecay = 0.6f;

            Ambient = 0.2f;
            Specular = 0.54f;
            Shininess = 18.0f;

            for (int i = 0; i < GameConstants.MaxLights; i++)
            {
                Matrix LightView = Matrix.CreateLookAt(m_lights[i].LightPos, look, Vector3.Up);
                Matrix LightProjection = Matrix.CreatePerspectiveFieldOfView(MathHelper.PiOver2, device.Viewport.AspectRatio, GameConstants.NearClip, GameConstants.FarClip);
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
                look.X += 10;
                hit2 = true;
            }
            if (state.IsKeyDown(Keys.I))
            {
                look.Y += 10;
                hit2 = true;
            }
            if (state.IsKeyDown(Keys.O))
            {
                look.Z += 10;
                hit2 = true;
            }
            if (state.IsKeyDown(Keys.J))
            {
                look.X -= 10;
                hit2 = true;
            }
            if (state.IsKeyDown(Keys.K))
            {
                look.Y -= 10;
                hit2 = true;
            }
            if (state.IsKeyDown(Keys.L))
            {
                look.Z -= 10;
                hit2 = true;
            }

            for (int i = 0; i < GameConstants.MaxLights; i++)
            {
                Matrix LightView = Matrix.CreateLookAt(m_lights[i].LightPos, look, Vector3.Up);
                Matrix LightProjection = Matrix.CreatePerspectiveFieldOfView(MathHelper.PiOver2, device.Viewport.AspectRatio, GameConstants.NearClip, GameConstants.FarClip);
                //matrix used in calculating shadow maps
                m_lights[i].LightViewProjectionMatrix = LightView * LightProjection;
            }

            if (hit2)
            {
                Console.WriteLine("Light Target: " + look);
            }
        }

        GamePadState gstate;

        private void UpdateCamera()
        {
            camera.Update(m_game.allPlayer.ElementAt(0).position, m_game.allPlayer.ElementAt(0).orientation);
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
            deferredShading.Parameters["xLightIntensity"].SetValue(light.LightIntensity);
            deferredShading.Parameters["xConeDirection"].SetValue(light.LightDir);
            deferredShading.Parameters["xConeAngle"].SetValue(light.ConeAngle);
            deferredShading.Parameters["xConeDecay"].SetValue(light.ConeDecay);

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
            //switch effect for models
            //render color map, normal map, and depth map of scene
            renderDeferredMaps();

            PresentationParameters pp = device.PresentationParameters;
            
            //spriteBatch.Begin();
            //spriteBatch.Draw(deferredRenderMap[1], new Rectangle(0,0, pp.BackBufferWidth, pp.BackBufferHeight), Color.White);
            //spriteBatch.End();

            //render lighting contributions from lights in the scene
            deferredShadingMap = getShadingMap();
            
            renderCombinedEffects();

            if (m_game.paused)
            {
                deferredCombined.CurrentTechnique = deferredCombined.Techniques["TextureDraw"];
                deferredCombined.Parameters["xTexture"].SetValue(tex);

                deferredCombined.Begin();
                foreach (EffectPass pass in deferredCombined.CurrentTechnique.Passes)
                {
                    pass.Begin();
                    device.VertexDeclaration = new VertexDeclaration(device, VertexPositionTexture.VertexElements);
                    device.DrawUserPrimitives<VertexPositionTexture>(PrimitiveType.TriangleStrip, menuVertices, 0, 2);
                    pass.End();
                }
                deferredCombined.End();
            }
            base.Draw(gameTime);
        }

        private void DrawAll(String technique)
        {
            foreach (GameObject o in m_objects)
            {
                Matrix worldMatrix = o.worldMatrix();
                DrawModel(o.model, m_model_textures.ElementAt((int)o.modelName), worldMatrix, technique);
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
                    currentEffect.CurrentTechnique = currentEffect.Techniques[technique];
                    currentEffect.Parameters["xCameraViewProjection"].SetValue(camera.ViewMatrix * camera.ProjectionMatrix);
                    currentEffect.Parameters["xWorld"].SetValue(worldMatrix);
                    currentEffect.Parameters["xTexture"].SetValue(textures[i++]);
                }
                mesh.Draw();
            }
        }
    }
}
