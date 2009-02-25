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
using System.Xml;
using System.IO;
using System.Text;

namespace CS032_Level_Editor
{
    /// <summary>
    /// This is the main type for your game
    /// </summary>
    public class Game1 : Microsoft.Xna.Framework.Game
    {
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;
        int mouseXOld, mouseYOld, mouseScrollOld;
        inputParser inputParser;
        //InputBindingAccess input;

        public Game1()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            graphics.PreferredBackBufferHeight = 800;
            graphics.PreferredBackBufferWidth = 1280;
            //graphics.ToggleFullScreen();
            

            mouseXOld = Mouse.GetState().X;
            mouseYOld = Mouse.GetState().Y;

            mouseScrollOld = mouseState.ScrollWheelValue;

            inputParser = new inputParser(this);

        }


        KeyboardState kbState; //Holds the state of the keyboard keys
        Keys[] pressedKeys;     //An array of the currently held down keys on the keyboard
        Grid3D grid3D;

        MouseState mouseState;

        GamePadState gamePadState;

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
            grid3D = new Grid3D(10,10, 10, graphics, Content);

            grid3D.moveX(0);
        }

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        /// 
        // the model to draw
        Model corner, flat, inverted_corner, wedge;

        // the aspect ratio
        float aspectRatio;

        protected override void LoadContent()
        {
            // Create a new SpriteBatch, which can be used to draw textures.
            spriteBatch = new SpriteBatch(GraphicsDevice);

            corner = Content.Load<Model>("Models\\corner");
            flat = Content.Load<Model>("Models\\flat");
            inverted_corner = Content.Load<Model>("Models\\inverted_corner");
            wedge = Content.Load<Model>("Models\\wedge");

            aspectRatio = graphics.GraphicsDevice.Viewport.AspectRatio;

            // TODO: use this.Content to load your game content here
        }

        public void getModel(String name)
        {

        }

        /// <summary>
        /// UnloadContent will be called once per game and is the place to unload
        /// all content.
        /// </summary>
        protected override void UnloadContent()
        {
            // TODO: Unload any non ContentManager content here
        }

        GamePadState oldGamePadState;

        // boolean for editing a tile

        Boolean isEditing = false;
        
        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update(GameTime gameTime)
        {
           
            inputParser.update(gameTime.TotalGameTime.TotalMilliseconds);

            if (inputParser.left())
                this.OnKeyPressed(Keys.Left);
            else if (inputParser.right())
                this.OnKeyPressed(Keys.Right);
            if (inputParser.down())
                this.OnKeyPressed(Keys.Down);
            else if (inputParser.up())
                this.OnKeyPressed(Keys.Up);
            if (inputParser.select())
                this.OnKeyPressed(Keys.Enter);
            if (inputParser.unselect())
                this.OnKeyPressed(Keys.Space);



            // gamepad
            gamePadState = GamePad.GetState(PlayerIndex.One);

            // if a was pressed, enter editing mode
            if (gamePadState.Buttons.A == ButtonState.Pressed && oldGamePadState.Buttons.A == ButtonState.Released)
                this.OnKeyPressed(Keys.Enter);

            // if b was pressed, exit editing mode
            if (gamePadState.Buttons.B == ButtonState.Pressed && oldGamePadState.Buttons.B == ButtonState.Released)
                this.OnKeyPressed(Keys.Space);

            grid3D.rotateCameraXZ(-25.0f * gamePadState.ThumbSticks.Right.X);
            grid3D.rotateCameraY(5.0f * gamePadState.ThumbSticks.Right.Y);

            grid3D.setZoom(10.0f * (gamePadState.Triggers.Left - gamePadState.Triggers.Right));

            oldGamePadState = gamePadState;

          
            // Mouse and Keyboard

            mouseState = Mouse.GetState();

            int scroll = mouseState.ScrollWheelValue;
            int scrollChange = scroll - mouseScrollOld;

            grid3D.setZoom(-scrollChange);

            int mouseX = mouseState.X;
            int mouseY = mouseState.Y;

            int xChange = mouseX - mouseXOld;
            int yChange = mouseY - mouseYOld;


            if (mouseState.LeftButton == ButtonState.Pressed)
            {
                grid3D.rotateCameraXZ(xChange);
                grid3D.rotateCameraY(1.0f * yChange);
            }

            if (mouseState.RightButton == ButtonState.Pressed)
            {
                grid3D.setZoom(yChange);
            }
            
            mouseXOld = mouseX;
            mouseYOld = mouseY;

            mouseScrollOld = scroll;

            kbState = Keyboard.GetState();
            Keys[] newKeys = (Keys[])kbState.GetPressedKeys();

            if (pressedKeys != null)
            {
                foreach (Keys k in newKeys)
                {
                    bool bFound = false;

                    foreach (Keys k2 in pressedKeys)
                    {
                        if (k == k2)
                        {
                            bFound = true;
                            break;
                        }
                    }

                    if (!bFound && k != Keys.Up && k != Keys.Down && k != Keys.Left && k != Keys.Right)
                    {

                        OnKeyPressed(k); //handle this key press
                    }
                }
            }

            pressedKeys = newKeys;

            // Allows the game to exit
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed)
                this.Exit();

            // TODO: Add your update logic here

            base.Update(gameTime);

        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);


            grid3D.draw();

            base.Draw(gameTime);
        }

        public void saveToXML()
        {
            FileStream fs = new FileStream("level.xml", FileMode.Create);

            XmlWriter w = XmlWriter.Create(fs);

            w.WriteStartDocument();
            w.WriteStartElement("LEVEL");

            grid3D.writeXML(w);
           
            /*w.WriteStartElement("OBJECT");
            w.WriteStartElement("MODEL");
            w.WriteAttributeString("NAME", "name");
            w.WriteEndElement();
            w.WriteStartElement("POSITION");
            w.WriteAttributeString("X", "1");
            w.WriteAttributeString("Y", "2");
            w.WriteAttributeString("Z", "3");
            w.WriteEndElement();
            w.WriteStartElement("ROTATION");
            w.WriteAttributeString("ANGLE", "12");
            w.WriteEndElement();
            w.WriteEndElement();

           */

            w.WriteEndElement();
            w.WriteEndDocument();
            w.Flush();
            fs.Close();
        }

        private void OnKeyPressed(Keys k)
        {
            int orientation = grid3D.getOrientation();

            switch (k)
            {
                case Keys.Up:
                    if (!isEditing)
                    {
                        if (orientation == 0)
                            grid3D.moveZ(1);
                        else if (orientation == 1)
                            grid3D.moveX(1);
                        else if (orientation == 2)
                            grid3D.moveZ(-1);
                        else if (orientation == 3)
                            grid3D.moveX(-1);
                    }
                    else
                        grid3D.handleUp();
                    break;

                case Keys.Down:
                    if (!isEditing)
                    {
                        if (orientation == 0)
                            grid3D.moveZ(-1);
                        else if (orientation == 1)
                            grid3D.moveX(-1);
                        else if (orientation == 2)
                            grid3D.moveZ(1);
                        else if (orientation == 3)
                            grid3D.moveX(1);

                    }
                    else
                        grid3D.handleDown();
                    break;

                case Keys.Left:
                    if (!isEditing)
                    {
                        if (orientation == 0)
                            grid3D.moveX(1);
                        else if (orientation == 1)
                            grid3D.moveZ(-1);
                        else if (orientation == 2)
                            grid3D.moveX(-1);
                        else if (orientation == 3)
                            grid3D.moveZ(1);

                    }
                    else
                        grid3D.handleLeft();
                    break;

                case Keys.Right:
                    if (!isEditing)
                    {
                        if (orientation == 0)
                            grid3D.moveX(-1);
                        else if (orientation == 1)
                            grid3D.moveZ(1);
                        else if (orientation == 2)
                            grid3D.moveX(1);
                        else if (orientation == 3)
                            grid3D.moveZ(-1);

                    }
                    else
                        grid3D.handleRight();
                    break;

                case Keys.Enter:
                    // if we are already in edit mode, then advance to the next menu item
                    if (isEditing == true)
                        grid3D.nextEditMenuItem();
                    else
                        grid3D.enterEditMenu();
                    isEditing = true;
                    break;

                case Keys.Space:
                    if (isEditing == true)
                        grid3D.exitEditMenu();

                    isEditing = false;
                    break;

                case Keys.S:
                    this.saveToXML();
                    break;

               

            }

            
        }

    }
}
