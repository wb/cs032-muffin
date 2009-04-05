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

        MuffinGame m_game;
        GraphicsDeviceManager graphics;
        GraphicsDevice device;
        SpriteBatch spriteBatch;
        Effect effect;
        VertexBuffer vertexBuffer;
        VertexDeclaration vertexDeclaration;

        GameCamera camera;
        MouseHandler m_mouse;

        Texture2D brickTexture;
        Texture2D spotlightTexture;
        Texture2D[] carTexture;
        Texture2D[] lampTexture;
        Texture2D[] shipTexture;  
        Texture2D[] forkLiftTexture;

        List<GameObject> m_objects;
        List<Model> m_models;
        List<Texture2D[]> m_model_textures;
        XMLParser m_parser;

        Model carModel;
        Model lampModel;
        Model shipModel;
        Model forkLiftModel;
        Matrix forkTransform;
        Boolean forkMoving = false;
        float forkVelocity = 0.0f;
        float forkHeight = 0.0f;

        //light data
        Matrix[] LightViewProjectionMatrix = new Matrix[3];
        Vector3[] LightPos = new Vector3[3];
        float[] LightIntensity = new float[3];
        float Ambient;
        float Specular;
        float Shininess;

        RenderTarget2D[] renderTarget = new RenderTarget2D[3];
        DepthStencilBuffer shadowDSB;
        DepthStencilBuffer standardDSB;
        Texture2D[] shadowMap = new Texture2D[3];

        public Renderer(MuffinGame game) : base(game)
        {
            //renderer settings
            m_game = game;
            graphics = m_game.graphics;
            graphics.IsFullScreen = false;
            //enable anti-aliasing
            graphics.PreferMultiSampling = true;
            graphics.PreparingDeviceSettings +=
                new EventHandler<PreparingDeviceSettingsEventArgs>(graphics_PreparingDeviceSettings);
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
            // TODO: Add your initialization logic here
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
                    meshPart.Effect = effect.Clone(device);

            return newModel;
        }

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            // Create a new SpriteBatch, which can be used to draw textures.
            spriteBatch = m_game.spriteBatch;
            device = graphics.GraphicsDevice;
            //load effect
            effect = m_game.Content.Load<Effect>("Effects/RenderEffect");
            //load textures
            spotlightTexture = m_game.Content.Load<Texture2D>("Textures/spotlight");
            //load models
            carModel = LoadModel("car", out carTexture);
            //DepthStencil Buffer and Corresponding RenderTarget for shadowMap
            PresentationParameters pp = device.PresentationParameters;
            SurfaceFormat format = device.PresentationParameters.BackBufferFormat;
            DepthFormat dFormat = device.DepthStencilBuffer.Format;
            MultiSampleType type = MultiSampleType.FourSamples;
            int height = graphics.PreferredBackBufferHeight;
            int width = graphics.PreferredBackBufferWidth;
            int quality = 0;

            //for multiple shadowmaps
            for(int i = 0; i < GameConstants.MaxLights; i++) {
                renderTarget[i] = new RenderTarget2D(device, width, height, 1, format, type, quality);
            }
            shadowDSB = new DepthStencilBuffer(graphics.GraphicsDevice, width, height, dFormat, type, quality);
            //save pointer to default DSB                                               
            standardDSB = device.DepthStencilBuffer;

            Texture2D[] terrain;
            m_models.Add(LoadModel("flat", out terrain));
            m_model_textures.Add(terrain);
            m_models.Add(LoadModel("wedge", out terrain));
            m_model_textures.Add(terrain);
            m_models.Add(LoadModel("corner", out terrain));
            m_model_textures.Add(terrain);
            m_models.Add(LoadModel("inverted_corner", out terrain));
            m_model_textures.Add(terrain);
            m_models.Add(LoadModel("box", out terrain));
            m_model_textures.Add(terrain);

            foreach(GameObject o in m_objects) {
                switch(o.modelName){
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
                    case ModelName.NONE: default:
                        break;
                }
            }

            SetUpCamera();
            SetUpLights();
        }

        private void SetUpCamera()
        {
            camera = new GameCamera(new Vector3(-40, 25, 35), new Vector3(0, 0, 0), device.Viewport.AspectRatio);
        }

        private void SetUpLights() {
            LightPos[0] = new Vector3(-20, 20, 20);
            LightPos[1] = new Vector3(20, 20, -20);
            LightPos[2] = new Vector3(-30, 45, 0);
            LightIntensity[0] = 2.0f;
            LightIntensity[1] = 2.0f;
            LightIntensity[2] = 2.0f;
            Ambient = 0.2f;
            Specular = 0.54f;
            Shininess = 18.0f;

            for (int i = 0; i < GameConstants.MaxLights; i++)
            {
                Matrix LightView = Matrix.CreateLookAt(LightPos[i], new Vector3(-3, -4, -5), new Vector3(0, 1, 0));
                Matrix LightProjection = Matrix.CreatePerspectiveFieldOfView(MathHelper.PiOver2, device.Viewport.AspectRatio, 2.0f, 2000f);
                //matrix used in calculating shadow maps
                LightViewProjectionMatrix[i] = LightView * LightProjection;
            }
        }

        /// <summary>
        /// UnloadContent will be called once per game and is the place to unload
        /// all content.
        /// </summary>
        protected override void UnloadContent()
        {
            // TODO: Unload any non ContentManager content here
        }

        Vector3 look = new Vector3(15, 15, 15);
        private void UpdateLights()
        {

            for (int i = 0; i < GameConstants.MaxLights; i++)
            {
                Matrix LightView = Matrix.CreateLookAt(LightPos[i], ModelPos, new Vector3(0, 1, 0));
                Matrix LightProjection = Matrix.CreatePerspectiveFieldOfView(MathHelper.PiOver2, device.Viewport.AspectRatio, 2.0f, 2000f);
                //matrix used in calculating shadow maps
                LightViewProjectionMatrix[i] = LightView * LightProjection;
            }
        }

        GamePadState gstate;

        private void UpdateCamera() {
            m_mouse.updateHandler(Mouse.GetState());

            float xRot = MathHelper.ToRadians((float)m_mouse.getNetY()) / 4.0f;
            float yRot = MathHelper.ToRadians((float)m_mouse.getNetX()) / 4.0f;

            Vector3 cameraRotationMouse;
            Vector3 cameraRotationController;

            int scrollValue = m_mouse.getNetScroll();
            camera.zoom(scrollValue);

            if (m_mouse.shouldRotate())
                cameraRotationMouse = new Vector3(xRot, yRot, 0.0f);
            else
                cameraRotationMouse = Vector3.Zero;

            float yRot2 = MathHelper.ToRadians(gstate.ThumbSticks.Right.X * 1.0f);
            float xRot2 = -MathHelper.ToRadians(gstate.ThumbSticks.Right.Y * 1.0f);

            int scrollValue1 = -(int)(gstate.Triggers.Left * 10);
            int scrollValue2 = (int)(gstate.Triggers.Right * 10);

            camera.zoom(scrollValue1);
            camera.zoom(scrollValue2);

            cameraRotationController = new Vector3(xRot2, yRot2, 0.0f);

            camera.setTarget(ModelPos);
            camera.Update(cameraRotationController);
            camera.Update(cameraRotationMouse);
        }

        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        public override void Update(GameTime gameTime)
        {
            
            gstate = GamePad.GetState(PlayerIndex.One);
            if(gstate.IsConnected){

                float netRotY = -gstate.ThumbSticks.Left.X * .02f;
                ModelRot *= Matrix.CreateFromYawPitchRoll(netRotY, 0.0f, 0.0f);
                ModelDir = Vector3.Transform(ModelDir, Matrix.CreateFromYawPitchRoll(netRotY, 0.0f, 0.0f));
                ModelDir.Normalize();

                if(gstate.Buttons.A == ButtonState.Pressed && gstate.Buttons.B == ButtonState.Released){
                    ModelPos += ((ModelDir) *.4f);
                }

                if (gstate.Buttons.B == ButtonState.Pressed && gstate.Buttons.A == ButtonState.Released)
                {
                    ModelPos -= ((ModelDir) * .4f);
                }

                if(gstate.Buttons.X == ButtonState.Pressed) {

                    if(!forkMoving) {
                        forkMoving = true;
                        if(forkHeight == 0.0f) {
                           // Console.WriteLine("Moving up");
                            forkVelocity = 15.0f;
                        } else {
                            forkVelocity = -15.0f;
                           // Console.WriteLine("Moving down");
                        }
                    }
                }

                if(forkMoving) {
                    forkHeight = forkHeight + forkVelocity * gameTime.ElapsedGameTime.Milliseconds / 1000.0f;

                    if(forkHeight >= 100.0f) {
                        forkHeight = 100.0f;
                        forkVelocity = 0.0f;
                        forkMoving = false;
                    } else if(forkHeight <= 0.0f) {
                        forkHeight = 0.0f;
                        forkVelocity = 0.0f;
                        forkMoving = false;
                    }

                    forkLiftModel.Bones["vorken"].Transform = Matrix.CreateTranslation(0.0f, 0.0f, forkHeight) * forkTransform;
                }
                
            }
            // TODO: Add your update logic here
            UpdateCamera();
            UpdateLights();
            
            base.Update(gameTime);
        }

        Vector3 ModelPos = new Vector3(100, 100, 100);
        Vector3 ModelDir = new Vector3(0, 0, 1);
        Matrix ModelRot = Matrix.CreateFromYawPitchRoll(3.0f * MathHelper.Pi / 2.0f, 0.0f, 0.0f);

        public override void Draw (GameTime gameTime) {
            //set DSB to the shadow DSB
            device.DepthStencilBuffer = shadowDSB;

            for(int i = 0; i < GameConstants.MaxLights; i++) {
                device.SetRenderTarget(0, renderTarget[i]);
                device.Clear(ClearOptions.Target | ClearOptions.DepthBuffer, Color.Black, 1.0f, 0);
                DrawAll("ShadowMap", i);
            }

            device.SetRenderTarget(0, null);

            for(int i = 0; i < GameConstants.MaxLights; i++) {
                shadowMap[i] = renderTarget[i].GetTexture();
            }

            //render the scene using shadowMap
            device.DepthStencilBuffer = standardDSB;
            device.Clear(ClearOptions.Target | ClearOptions.DepthBuffer, Color.CornflowerBlue, 1.0f, 0);
            DrawAll("ShadowedScene", 0);

            base.Draw(gameTime);
        }

        private void DrawAll(String technique, int index)
        {
            device.RenderState.DepthBufferEnable = true;
            device.RenderState.CullMode = CullMode.CullCounterClockwiseFace;
            device.RenderState.AlphaBlendEnable = false;

            foreach (GameObject o in m_objects) {
                Matrix worldMatrix = Matrix.CreateFromYawPitchRoll(o.rotation.Y, o.rotation.X, 0.0f) *
                                     Matrix.CreateTranslation(o.position) *
                                     Matrix.CreateScale(0.20f) *
                                     Matrix.CreateTranslation(new Vector3(-45,-4.7f,-45));
                DrawModel(o.model, m_model_textures.ElementAt((int)o.modelName), worldMatrix, technique, index);
            }
            Matrix forklift1Matrix = Matrix.CreateScale(.045f) * ModelRot * Matrix.CreateTranslation(ModelPos);
            DrawModel(m_models.ElementAt((int)ModelName.BOX), forkLiftTexture, forklift1Matrix, technique, index);
        }

        private void DrawModel(Model model, Texture2D[] textures, Matrix wMatrix, string technique, int index)
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
                    currentEffect.Parameters["num_lights"].SetValue(GameConstants.MaxLights);
                    currentEffect.Parameters["current_light"].SetValue(index);
                    currentEffect.Parameters["xCameraViewProjection"].SetValue(camera.ViewMatrix * camera.ProjectionMatrix);
                    currentEffect.Parameters["xLightViewProjection"].SetValue(LightViewProjectionMatrix);
                    currentEffect.Parameters["xWorld"].SetValue(worldMatrix);
                    currentEffect.Parameters["xTexture"].SetValue(textures[i++]);
                    currentEffect.Parameters["xSpotlight"].SetValue(spotlightTexture);
                    currentEffect.Parameters["xShadowMap1"].SetValue(shadowMap[0]);
                    currentEffect.Parameters["xShadowMap2"].SetValue(shadowMap[1]);
                    currentEffect.Parameters["xShadowMap3"].SetValue(shadowMap[2]);
                    currentEffect.Parameters["xCameraPos"].SetValue(camera.cameraPosition);
                    currentEffect.Parameters["xLightPos"].SetValue(LightPos);
                    currentEffect.Parameters["xLightIntensity"].SetValue(LightIntensity);
                    currentEffect.Parameters["xSpecular"].SetValue(Specular);
                    currentEffect.Parameters["xShininess"].SetValue(Shininess);
                    currentEffect.Parameters["xAmbient"].SetValue(Ambient);
                }
                mesh.Draw();
            }
        }
    }
}
