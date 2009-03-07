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

            public MyOwnVertexFormat(Vector3 position, Vector2 tex)
            {
                this.position = position;
                this.texcoord = tex;
            }

            public static VertexElement[] VertexElements =
             {
                 new VertexElement(0, 0, VertexElementFormat.Vector3, VertexElementMethod.Default, VertexElementUsage.Position, 0),
                 new VertexElement(0, sizeof(float)*3, VertexElementFormat.Vector2, VertexElementMethod.Default, VertexElementUsage.TextureCoordinate, 0),
             };

            public static int SizeInBytes = sizeof(float) * (3 + 2);
        }

        GraphicsDeviceManager graphics;
        GraphicsDevice device;
        SpriteBatch spriteBatch;
        Effect effect;
        Matrix viewMatrix;
        Matrix projectionMatrix;
        VertexBuffer vertexBuffer;
        VertexDeclaration vertexDeclaration;

        Texture2D brickTexture;
        Texture2D[] carTexture;
        Texture2D[] lampTexture;
        Texture2D[] shipTexture;
        Texture2D[] forkLiftTexture;

        Model carModel;
        Model lampModel;
        Model shipModel;
        Model forkLiftModel;

        Vector3 cameraPos;
 
        public Renderer()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
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
            textures = new Texture2D[20];
            int i = 0;
            foreach (ModelMesh mesh in newModel.Meshes)
                foreach (BasicEffect currentEffect in mesh.Effects)
                    textures[i++] = currentEffect.Texture;

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

            effect = Content.Load<Effect>("Effects/simpleEffect");
            brickTexture = Content.Load<Texture2D>("Textures/streettexture");
            carModel = LoadModel("car", out carTexture);
            lampModel = LoadModel("lamppost", out lampTexture);
            shipModel = LoadModel("p1_wedge", out shipTexture);
            forkLiftModel = LoadModel("forklift", out forkLiftTexture);

            lampTexture[0] = brickTexture;

            SetUpVertices();
            SetUpCamera();

        }

        private void SetUpVertices()
        {

            MyOwnVertexFormat[] vertices = new MyOwnVertexFormat[12];

            vertices[0] = new MyOwnVertexFormat(new Vector3(-20, 0, 10), new Vector2(-0.25f, 25.0f));
            vertices[1] = new MyOwnVertexFormat(new Vector3(-20, 0, -100), new Vector2(-0.25f, 0.0f));
            vertices[2] = new MyOwnVertexFormat(new Vector3(2, 0, 10), new Vector2(0.25f, 25.0f));
            vertices[3] = new MyOwnVertexFormat(new Vector3(2, 0, -100), new Vector2(0.25f, 0.0f));
            vertices[4] = new MyOwnVertexFormat(new Vector3(2, 1, 10), new Vector2(0.375f, 25.0f));
            vertices[5] = new MyOwnVertexFormat(new Vector3(2, 1, -100), new Vector2(0.375f, 0.0f));
            vertices[6] = new MyOwnVertexFormat(new Vector3(3, 1, 10), new Vector2(0.5f, 25.0f));
            vertices[7] = new MyOwnVertexFormat(new Vector3(3, 1, -100), new Vector2(0.5f, 0.0f));
            vertices[8] = new MyOwnVertexFormat(new Vector3(13, 1, 10), new Vector2(0.75f, 25.0f));
            vertices[9] = new MyOwnVertexFormat(new Vector3(13, 1, -100), new Vector2(0.75f, 0.0f));
            vertices[10] = new MyOwnVertexFormat(new Vector3(13, 21, 10), new Vector2(1.25f, 25.0f));
            vertices[11] = new MyOwnVertexFormat(new Vector3(13, 21, -100), new Vector2(1.25f, 0.0f));

            vertexBuffer = new VertexBuffer(device, vertices.Length * MyOwnVertexFormat.SizeInBytes, BufferUsage.WriteOnly);
            vertexBuffer.SetData(vertices);

            vertexDeclaration = new VertexDeclaration(device, MyOwnVertexFormat.VertexElements);
        }

        private void SetUpCamera()
        {
            cameraPos = new Vector3(-25, 13, 18);
            viewMatrix = Matrix.CreateLookAt(cameraPos, new Vector3(0, 2, -12), new Vector3(0, 1, 0));
            projectionMatrix = Matrix.CreatePerspectiveFieldOfView(MathHelper.PiOver4, device.Viewport.AspectRatio, 1.0f, 200.0f);
        }

        /// <summary>
        /// UnloadContent will be called once per game and is the place to unload
        /// all content.
        /// </summary>
        protected override void UnloadContent()
        {
            // TODO: Unload any non ContentManager content here
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

            // TODO: Add your update logic here

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            device.Clear(ClearOptions.Target | ClearOptions.DepthBuffer, Color.Black, 1.0f, 0);

            effect.CurrentTechnique = effect.Techniques["Simplest"];
            effect.Parameters["xWorldViewProjection"].SetValue(Matrix.Identity * viewMatrix * projectionMatrix);
            effect.Parameters["xTexture"].SetValue(brickTexture);
            effect.Begin();
            foreach (EffectPass pass in effect.CurrentTechnique.Passes)
            {
                pass.Begin();
                device.VertexDeclaration = vertexDeclaration;
                device.Vertices[0].SetSource(vertexBuffer, 0, MyOwnVertexFormat.SizeInBytes);
                device.DrawPrimitives(PrimitiveType.TriangleStrip, 0, 10);
                pass.End();
            }
            effect.End();

            Matrix car1Matrix = Matrix.CreateScale(4f) * Matrix.CreateTranslation(4, 2, -15);
            DrawModel(carModel, carTexture, car1Matrix, "Simplest", false);

            Matrix ship1Matrix = Matrix.CreateScale(.005f) * Matrix.CreateRotationY(MathHelper.Pi) * Matrix.CreateTranslation(-15, 2, -15);
            DrawModel(shipModel, shipTexture, ship1Matrix, "Simplest", false);

            Matrix forklift1Matrix = Matrix.CreateScale(.05f) * Matrix.CreateRotationY(3.0f * MathHelper.Pi/2.0f) * Matrix.CreateTranslation(-5, 0, -15);
            DrawModel(forkLiftModel, forkLiftTexture, forklift1Matrix, "Simplest", false);

            //Matrix lamp1Matrix = Matrix.CreateScale(0.05f) * Matrix.CreateTranslation(4.0f, 1, -35);
            //DrawModel(lampModel, lampTexture, lamp1Matrix, "Simplest", true);

            //Matrix lamp2Matrix = Matrix.CreateScale(0.05f) * Matrix.CreateTranslation(4.0f, 1, -5);
            //DrawModel(lampModel, lampTexture, lamp2Matrix, "Simplest", true);

            base.Draw(gameTime);
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
                    currentEffect.Parameters["xWorldViewProjection"].SetValue(worldMatrix * viewMatrix * projectionMatrix);
                    currentEffect.Parameters["xTexture"].SetValue(textures[i++]);
                    currentEffect.Parameters["xSolidBrown"].SetValue(solidBrown);
                }
                mesh.Draw();
            }
        }
    }
}
