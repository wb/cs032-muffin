using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using Microsoft.Xna.Framework.Net;
using Microsoft.Xna.Framework.Storage;

namespace _3D_Renderer
{
    /// <summary>
    /// This is the main type for your game
    /// </summary>
    public class Renderer : Microsoft.Xna.Framework.Game
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
        float[] Ambient = new float[3];
        float[] Specular = new float[3];
        float[] Shininess = new float[3];

        RenderTarget2D renderTarget;
        DepthStencilBuffer shadowDSB;
        DepthStencilBuffer standardDSB;
        Texture2D shadowMap;

        public Renderer()
        {
            //renderer settings
            graphics = new GraphicsDeviceManager(this);
            //enable anti-aliasing
            graphics.PreferMultiSampling = true;
            graphics.PreparingDeviceSettings +=
                new EventHandler<PreparingDeviceSettingsEventArgs>(graphics_PreparingDeviceSettings);
            //mouse handler
            m_mouse = new MouseHandler(Mouse.GetState());
            this.IsMouseVisible = true;
            Content.RootDirectory = "Content";
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
        protected override void Initialize()
        {
            // TODO: Add your initialization logic here
            base.Initialize();
        }

        private Model LoadModel(string assetName, out Texture2D[] textures)
        {
            Model newModel = Content.Load<Model>("Models/" + assetName);
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
            spriteBatch = new SpriteBatch(GraphicsDevice);
            device = GraphicsDevice;
            //load effect
            effect = Content.Load<Effect>("Effects/simpleEffect");
            //load textures
            brickTexture = Content.Load<Texture2D>("Textures/streettexture");
            spotlightTexture = Content.Load<Texture2D>("Textures/spotlight");
            //load models
            carModel = LoadModel("car", out carTexture);
            lampModel = LoadModel("lamppost", out lampTexture);
            shipModel = LoadModel("p1_wedge", out shipTexture);
            forkLiftModel = LoadModel("forklift", out forkLiftTexture);
            forkTransform = forkLiftModel.Bones["vorken"].Transform;

            lampTexture[0] = brickTexture;
            //DepthStencil Buffer and Corresponding RenderTarget for shadowMap
            PresentationParameters pp = device.PresentationParameters;
            SurfaceFormat format = GraphicsDevice.PresentationParameters.BackBufferFormat;
            DepthFormat dFormat = GraphicsDevice.DepthStencilBuffer.Format;
            MultiSampleType type = MultiSampleType.FourSamples;
            int height = graphics.PreferredBackBufferHeight;
            int width = graphics.PreferredBackBufferWidth;
            int quality = 0;

            renderTarget = new RenderTarget2D(device, width, height, 1, format, type, quality);
            shadowDSB = new DepthStencilBuffer(graphics.GraphicsDevice, width, height, dFormat, type, quality);
            //save pointer to default DSB                                               
            standardDSB = device.DepthStencilBuffer;

            SetUpVertices();
            SetUpCamera();
        }

        private void SetUpVertices()
        {
             MyOwnVertexFormat[] vertices = new MyOwnVertexFormat[18];
             
             vertices[0] = new MyOwnVertexFormat(new Vector3(-20, 0, 10), new Vector2(-0.25f, 25.0f), new Vector3(0, 1, 0));
             vertices[1] = new MyOwnVertexFormat(new Vector3(-20, 0, -100), new Vector2(-0.25f, 0.0f), new Vector3(0, 1, 0));
             vertices[2] = new MyOwnVertexFormat(new Vector3(2, 0, 10), new Vector2(0.25f, 25.0f), new Vector3(0, 1, 0));
             vertices[3] = new MyOwnVertexFormat(new Vector3(2, 0, -100), new Vector2(0.25f, 0.0f), new Vector3(0, 1, 0));
             vertices[4] = new MyOwnVertexFormat(new Vector3(2, 0, 10), new Vector2(0.25f, 25.0f), new Vector3(-1, 0, 0));
             vertices[5] = new MyOwnVertexFormat(new Vector3(2, 0, -100), new Vector2(0.25f, 0.0f), new Vector3(-1, 0, 0));
             vertices[6] = new MyOwnVertexFormat(new Vector3(2, 1, 10), new Vector2(0.375f, 25.0f), new Vector3(-1, 0, 0));
             vertices[7] = new MyOwnVertexFormat(new Vector3(2, 1, -100), new Vector2(0.375f, 0.0f), new Vector3(-1, 0, 0));
             vertices[8] = new MyOwnVertexFormat(new Vector3(2, 1, 10), new Vector2(0.375f, 25.0f), new Vector3(0, 1, 0));
             vertices[9] = new MyOwnVertexFormat(new Vector3(2, 1, -100), new Vector2(0.375f, 0.0f), new Vector3(0, 1, 0));
             vertices[10] = new MyOwnVertexFormat(new Vector3(3, 1, 10), new Vector2(0.5f, 25.0f), new Vector3(0, 1, 0));
             vertices[11] = new MyOwnVertexFormat(new Vector3(3, 1, -100), new Vector2(0.5f, 0.0f), new Vector3(0, 1, 0));
             vertices[12] = new MyOwnVertexFormat(new Vector3(13, 1, 10), new Vector2(0.75f, 25.0f), new Vector3(0, 1, 0));
             vertices[13] = new MyOwnVertexFormat(new Vector3(13, 1, -100), new Vector2(0.75f, 0.0f), new Vector3(0, 1, 0));
             vertices[14] = new MyOwnVertexFormat(new Vector3(13, 1, 10), new Vector2(0.75f, 25.0f), new Vector3(-1, 0, 0));
             vertices[15] = new MyOwnVertexFormat(new Vector3(13, 1, -100), new Vector2(0.75f, 0.0f), new Vector3(-1, 0, 0));
             vertices[16] = new MyOwnVertexFormat(new Vector3(13, 21, 10), new Vector2(1.25f, 25.0f), new Vector3(-1, 0, 0));
             vertices[17] = new MyOwnVertexFormat(new Vector3(13, 21, -100), new Vector2(1.25f, 0.0f), new Vector3(-1, 0, 0));

            vertexBuffer = new VertexBuffer(device, vertices.Length * MyOwnVertexFormat.SizeInBytes, BufferUsage.WriteOnly);
            vertexBuffer.SetData(vertices);

            vertexDeclaration = new VertexDeclaration(device, MyOwnVertexFormat.VertexElements);
        }

        private void SetUpCamera()
        {
            camera = new GameCamera(new Vector3(-40, 25, 35), new Vector3(0, 2, -12), device.Viewport.AspectRatio);
        }

        /// <summary>
        /// UnloadContent will be called once per game and is the place to unload
        /// all content.
        /// </summary>
        protected override void UnloadContent()
        {
            // TODO: Unload any non ContentManager content here
        }

        private void UpdateLightData()
        {
            LightPos[0] = new Vector3(-10, 20, 10);
            LightIntensity[0] = 2.0f;
            Ambient[0] = 0.3f;
            Specular[0] = 0.54f;
            Shininess[0] = 45.0f;
            
            Matrix LightView = Matrix.CreateLookAt(LightPos[0], new Vector3(-3, -4, -5), new Vector3(0, 1, 0));
            Matrix LightProjection = Matrix.CreatePerspectiveFieldOfView(MathHelper.PiOver2, device.Viewport.AspectRatio, 5f, 200f);
            //matrix used in calculating shadow maps
            LightViewProjectionMatrix[0] = LightView * LightProjection;
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

            cameraRotationController = new Vector3(0.0f, yRot2, 0.0f);

            camera.setTarget(ModelPos);
            camera.Update(cameraRotationController);
            camera.Update(cameraRotationMouse);
            
        }

        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update(GameTime gameTime)
        {
            // Allows the game to exit
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed)
                this.Exit();
            
            gstate = GamePad.GetState(PlayerIndex.One);
            if(gstate.IsConnected){

                float netRotY = -gstate.ThumbSticks.Left.X * .01f;
                ModelRot *= Matrix.CreateFromYawPitchRoll(netRotY, 0.0f, 0.0f);
                ModelDir = Vector3.Transform(ModelDir, Matrix.CreateFromYawPitchRoll(netRotY, 0.0f, 0.0f));
                ModelDir.Normalize();

                if(gstate.Buttons.A == ButtonState.Pressed && gstate.Buttons.B == ButtonState.Released){
                    ModelPos += ((ModelDir) *.05f);
                }

                if (gstate.Buttons.B == ButtonState.Pressed && gstate.Buttons.A == ButtonState.Released)
                {
                    ModelPos -= ((ModelDir) * .05f);
                }

                if(gstate.Buttons.X == ButtonState.Pressed) {

                    if(!forkMoving) {
                        forkMoving = true;
                        if(forkHeight == 0.0f) {
                            Console.WriteLine("Moving up");
                            forkVelocity = 8.0f;
                        } else {
                            forkVelocity = -8.0f;
                            Console.WriteLine("Moving down");
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
            UpdateLightData();
            UpdateCamera();
            
            base.Update(gameTime);
        }

        Vector3 ModelPos = new Vector3(-6, 0, -5);
        Vector3 ModelDir = new Vector3(0, 0, 1);
        Matrix ModelRot = Matrix.CreateFromYawPitchRoll(3.0f * MathHelper.Pi / 2.0f, 0.0f, 0.0f);

        protected override void Draw (GameTime gameTime) {
            //set DSB to the shadow DSB
            device.DepthStencilBuffer = shadowDSB;
            device.SetRenderTarget(0, renderTarget);
            device.Clear(ClearOptions.Target | ClearOptions.DepthBuffer, Color.Black, 1.0f, 0);
            DrawAll("ShadowMap");
            //render shadowMap and save it
            device.SetRenderTarget(0, null);
            shadowMap = renderTarget.GetTexture();
            //render the scene using shadowMap
            device.DepthStencilBuffer = standardDSB;
            device.Clear(ClearOptions.Target | ClearOptions.DepthBuffer, Color.Black, 1.0f, 0);
            DrawAll("ShadowedScene");

            base.Draw(gameTime);
        }

        private void DrawAll (String technique)
        {
            
            effect.CurrentTechnique = effect.Techniques[technique];
            effect.Parameters["xWorld"].SetValue(Matrix.Identity);
            effect.Parameters["xLightsWorldViewProjection"].SetValue(Matrix.Identity * LightViewProjectionMatrix[0]);
            effect.Parameters["xWorldViewProjection"].SetValue(Matrix.Identity * camera.ViewMatrix * camera.ProjectionMatrix);
            effect.Parameters["xLightPos"].SetValue(LightPos[0]);
            effect.Parameters["xCameraPos"].SetValue(camera.cameraPosition);
            effect.Parameters["xSpecular"].SetValue(Specular[0]);
            effect.Parameters["xShadowMap"].SetValue(shadowMap);
            effect.Parameters["xShininess"].SetValue(Shininess[0]);
            effect.Parameters["xSpotlight"].SetValue(spotlightTexture);
            effect.Parameters["xLightIntensity"].SetValue(LightIntensity[0]);
            effect.Parameters["xAmbient"].SetValue(Ambient[0]);
            effect.Parameters["xTexture"].SetValue(brickTexture);
            effect.Begin();
            foreach (EffectPass pass in effect.CurrentTechnique.Passes)
            {
                pass.Begin();
                device.VertexDeclaration = vertexDeclaration;
                device.Vertices[0].SetSource(vertexBuffer, 0, MyOwnVertexFormat.SizeInBytes);
                device.DrawPrimitives(PrimitiveType.TriangleStrip, 0, 18);
                pass.End();
            }
            effect.End();

            Matrix car1Matrix = Matrix.CreateScale(4f) * Matrix.CreateTranslation(6, 1, -15);
            DrawModel(carModel, carTexture, car1Matrix, technique, false);

            Matrix ship1Matrix = Matrix.CreateScale(.005f) * Matrix.CreateRotationY(MathHelper.Pi) * Matrix.CreateTranslation(-15, 2, -15);
            DrawModel(shipModel, shipTexture, ship1Matrix, technique, false);

            Matrix forklift1Matrix = Matrix.CreateScale(.045f) * ModelRot * Matrix.CreateTranslation(ModelPos);
            DrawModel(forkLiftModel, forkLiftTexture, forklift1Matrix, technique, false);

            Matrix lamp1Matrix = Matrix.CreateScale(0.05f) * Matrix.CreateTranslation(4.0f, 1, -35);
            DrawModel(lampModel, lampTexture, lamp1Matrix, technique, true);

            Matrix lamp2Matrix = Matrix.CreateScale(0.05f) * Matrix.CreateTranslation(4.0f, 1, -55);
            DrawModel(lampModel, lampTexture, lamp2Matrix, technique, true);
        }

        private void DrawModel(Model model, Texture2D[] textures, Matrix wMatrix, string technique, bool solidBrown)
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
                    currentEffect.Parameters["xWorld"].SetValue(worldMatrix);
                    currentEffect.Parameters["xLightPos"].SetValue(LightPos[0]);
                    currentEffect.Parameters["xCameraPos"].SetValue(camera.cameraPosition);
                    currentEffect.Parameters["xSpecular"].SetValue(Specular[0]);
                    currentEffect.Parameters["xShininess"].SetValue(Shininess[0]);
                    currentEffect.Parameters["xShadowMap"].SetValue(shadowMap);
                    currentEffect.Parameters["xSpotlight"].SetValue(spotlightTexture);
                    currentEffect.Parameters["xLightIntensity"].SetValue(LightIntensity[0]);
                    currentEffect.Parameters["xAmbient"].SetValue(Ambient[0]);
                    currentEffect.Parameters["xLightsWorldViewProjection"].SetValue(worldMatrix * LightViewProjectionMatrix[0]);
                    currentEffect.Parameters["xWorldViewProjection"].SetValue(worldMatrix* camera.ViewMatrix * camera.ProjectionMatrix);
                    currentEffect.Parameters["xTexture"].SetValue(textures[i++]);
                    currentEffect.Parameters["xSolidBrown"].SetValue(solidBrown);
                }
                mesh.Draw();
            }
        }
    }
}
