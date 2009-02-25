using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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

namespace CS032_Level_Editor
{
    class GameTile
    {
        int xPosition, zPosition, currentModelIndex, maxHeight, tileContourIndex;
        String[] tileContours;
        GameModel[] theModels;
        Vector3 position, cameraPosition;
        float aspectRatio;
        GraphicsDeviceManager graphics;
        ContentManager Content;
        Grid3D grid3d;
        Boolean isSelected;

        public GameTile(int x, int z, int height, GraphicsDeviceManager gdm, ContentManager cm, Grid3D grid)
        {
            xPosition = x;
            zPosition = z;
            currentModelIndex = 0;
            maxHeight = height;
            tileContourIndex = 0;
            position = Vector3.Zero;

      
            graphics = gdm;
            cameraPosition = new Vector3(1000.0f, 500.0f, 5000.0f);
            aspectRatio = graphics.GraphicsDevice.Viewport.AspectRatio;

            grid3d = grid;
           

            // initialize the list of tileContours available
            tileContours = new String[4] { "flat", "wedge", "corner", "inverted_corner" };

            Content = cm;

            theModels = new GameModel[height];

            // initially, the array has one object
            theModels[0] = new GameModel("flat", Content,xPosition, this.getFreeHeight(), zPosition);

        }

        public void selected()
        {
            isSelected = true;
            this.cycle(0);
        }

        public void unselected()
        {
            isSelected = false;
            this.cycle(0);
        }

        public void moveY(int change)
        {

            // check first to see if there are free spots left
            if (currentModelIndex + change < theModels.Length && currentModelIndex + change >= 0)
            {
                // now, the current piece must be nonempty to move up
                if (theModels[currentModelIndex].getModel() != null)
                {

                    // get the rotation from the current one
                    float tempRotation = theModels[currentModelIndex].getRotation();


                    // and now, move up and set this model if it is null

                    currentModelIndex += change;

                    // if we are moving down, keep the rotation
                    if (change < 0)
                        theModels[currentModelIndex].setRotation(theModels[currentModelIndex + 1].getRotation());

                    // now, we know we can move up.  but, before doing so we must make sure each piece below is a flat piece

                    for (int i = 0; i < currentModelIndex; i++)
                    {
                        theModels[i].setModel("flat");
                    }

                    // and make sure each model above it is null

                    for (int i = currentModelIndex + 1; i < theModels.Length; i++)
                        theModels[i] = null;


                    // if we are moving up, keep the rotation and piece of the previous layer
                    if (theModels[currentModelIndex] == null)
                    {
                        theModels[currentModelIndex] = new GameModel("flat", Content, xPosition, this.getFreeHeight(), zPosition);
                        theModels[currentModelIndex].setRotation(tempRotation);
                    }

                   

                }
                

            }

            grid3d.moveX(0);

            //Console.WriteLine("Tile (" + xPosition + "," + zPosition + ") and height " + currentHeightPosition);
        }

        public void cycle(int change)
        {
            tileContourIndex += change;

            if (tileContourIndex < 0)
                tileContourIndex += tileContours.Length;
            else if (tileContourIndex >= tileContours.Length)
                tileContourIndex = 0;

            String temp = tileContours[tileContourIndex];

            if (isSelected)
                temp = temp + "_selected";


  
            //Console.WriteLine("Currently showing tile " + temp);

            if (theModels[currentModelIndex] == null)
                theModels[currentModelIndex] = new GameModel("flat", Content, xPosition, this.getFreeHeight(), zPosition);
            else
                theModels[currentModelIndex].setModel(temp);


        }

        public void draw()
        {
            // draw each model contained by this tile

            foreach (GameModel model in theModels)
            {
                if (model != null && model.getModel() != null)
                {


                    Matrix[] transforms = new Matrix[model.getModel().Bones.Count];
                    model.getModel().CopyAbsoluteBoneTransformsTo(transforms);

                    foreach (ModelMesh mesh in model.getModel().Meshes)
                    {
                        foreach (BasicEffect effect in mesh.Effects)
                        {

                            effect.EnableDefaultLighting();
                            effect.World = transforms[mesh.ParentBone.Index] * Matrix.CreateTranslation(-1.0f * model.getTranslateToCenter()) * Matrix.CreateRotationY(MathHelper.ToRadians(model.getRotation())) * Matrix.CreateTranslation(model.getTranslateToCenter()) * Matrix.CreateTranslation(model.getPosition()) * Matrix.CreateScale(1.0f);
                            effect.View = Matrix.CreateLookAt(grid3d.getCurrentCameraPosition(), grid3d.getCurrentCameraLookPosition(), Vector3.Up);
                            effect.Projection = Matrix.CreatePerspectiveFieldOfView(MathHelper.ToRadians(45.0f), aspectRatio, 1.0f, 10000.0f);
                        }

                        mesh.Draw();
                    }
                }

                // Console.WriteLine("Drawing tile (" + xPosition + "," + zPosition + ")");
            }

        }

        public void rotate(int rotation)
        {
            theModels[currentModelIndex].setRotation(theModels[currentModelIndex].getRotation() + 90.0f * rotation);
        }

        private float getFreeHeight()
        {
            float height = 0;

            foreach (GameModel model in theModels)
            {
                if (model != null)
                    height += model.getHeight();
            }

            return height;
        }

        public float getCurrentHeight()
        {
            float maxY = 0;

            // return the y-position of the tallest tile
            for (int i = 0; i < theModels.Length; i++)
            {
                if (theModels[i] != null)
                {
                    if (maxY < theModels[i].getPosition().Y)
                        maxY = theModels[i].getPosition().Y;

                }
                else
                    break;


            }
            
            return maxY;
        }

        public void writeXML(XmlWriter w)
        {
            foreach (GameModel model in theModels)
            {
                if (model != null)
                {
                    model.writeXML(w);
                }
            }

        }

    }


}
