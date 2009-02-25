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
    class Grid3D
    {
        int xMax, yMax, zMax, xPosition, zPosition, menuItem;
        GameTile[,] gameTiles;
        float totalRotation, cameraX, cameraY, cameraZ, totalRotationY, zoomLevel, smallBound, largeBound;
        Vector3 oldCameraLookPosition, oldCameraPosition, currentCameraPosition, currentCameraLookPosition;
        Boolean showMenu = false;
        String[] menu;
        //SpriteFont font;

        public Grid3D(int x, int z, int y, GraphicsDeviceManager gdm, ContentManager cm)
        {
            xMax = x; 
            zMax = z;
            yMax = y; // maximum height of each game tile

            xPosition = 0;
            zPosition = 0;

            gameTiles = new GameTile[x, z];

            zoomLevel = 0.8f;

            // initial the array with new, blank game tiles
            this.initializeGameTiles(gdm, cm);

            totalRotation = 270.0f;

            this.rotateCameraXZ(0);

            totalRotationY = 45.0f;

            this.rotateCameraY(0);

            //menu = new String[3] { "Place Tile", "Change Height", "Place Item" };
            menu = new String[2] { "Set Height", "Choose Tile" };
            smallBound = 0.25f / ((xMax + yMax) / 20);
            largeBound = smallBound * 12;

            // load the font
            //font = cm.Load<SpriteFont>("arial");

        }

        public void enterEditMenu()
        {
            menuItem = 0;
            showMenu = true;
            Console.WriteLine("Entering Menu");
            Console.WriteLine(menu[menuItem]);

            //spriteBatch.DrawString(font, string.Format("Down, value: {0}", input.GetValue("down")), new Vector2(50, 180), Color.Black);
        }

        public void exitEditMenu()
        {
            showMenu = false;
            Console.WriteLine("Exiting Menu");
        }

        public void nextEditMenuItem()
        {
            menuItem += 1;
            menuItem = menuItem % menu.Length;

            Console.WriteLine(menu[menuItem]);
        }

        public void moveX(int change)
        {
            // save the old position
            int oldXPosition = xPosition;

            // add on the change in position
            xPosition = xPosition + change;

            // if we have moved outside of the range, undo the last move
            if (xPosition < 0 || xPosition >= xMax)
                xPosition = xPosition - change;
            else
            {
                // if we successfully moved, unselect the previous game tile
                gameTiles[oldXPosition, zPosition].unselected();

                // and select the new tile
                gameTiles[xPosition, zPosition].selected();
            }
            
        }

        public void moveZ(int change)
        {
            // save the old position
            int oldZPosition = zPosition;

            // add on the change in position
            zPosition = zPosition + change;

            // if we have moved outside of the range, undo the last move
            if (zPosition < 0 || zPosition >= zMax)
                zPosition = zPosition - change;
            else
            {
                // if we successfully moved, unselect the previous game tile
                gameTiles[xPosition, oldZPosition].unselected();

                // and select the new tile
                gameTiles[xPosition, zPosition].selected();
            }


        }

        public void moveY(int change)
        {
            this.selectedSquare().moveY(change);
        }

        public void cycle(int change)
        {
            this.selectedSquare().cycle(change);
        }

        public void writeXML(XmlWriter w)
        {

            foreach (GameTile tile in gameTiles)
            {
                tile.writeXML(w);
            }

        }

        public void draw()
        {
            // update the camera position and look position for this round of drawing
            this.calculateCameraLookPosition();
            this.calculateCameraPosition();

            foreach (GameTile tile in gameTiles)
            {
                tile.draw();
            }
        }

        private GameTile selectedSquare()
        {
            return gameTiles[xPosition, zPosition];
        }

        private void initializeGameTiles(GraphicsDeviceManager gdm, ContentManager cm)
        {
            for (int x = 0; x < xMax; x++)
            {
                for (int z = 0; z < zMax; z++)
                {
                    gameTiles[x, z] = new GameTile(x, z, yMax,gdm, cm, this);
                }
            }
        }

        public void rotateCameraXZ(float rotation)
        {
            totalRotation += (rotation / 10.0f);

            totalRotation = totalRotation % 360;

            cameraX = (float)((60 * xMax * Math.Cos(MathHelper.ToRadians(totalRotation)))) + 60 * (xPosition) + 30;
            cameraZ = (float)((60 * zMax * Math.Sin(MathHelper.ToRadians(totalRotation)))) + 60 * (zPosition - 1) + 30;

        }

        public void rotateCameraY(float rotation)
        {
            totalRotationY += rotation / 5.0f;

            if (totalRotationY < 5)
                totalRotationY = 5;
            else if (totalRotationY > 90)
                totalRotationY = 90;

            cameraY = (float)(60 * xMax * Math.Sin(MathHelper.ToRadians(totalRotationY)));

        }

        public Vector3 getCurrentCameraPosition()
        {
            return currentCameraPosition;
        }

        public Vector3 getCurrentCameraLookPosition()
        {
            return currentCameraLookPosition;
        }


        public void calculateCameraPosition()
        {
           
            // find vector between cameraPosition and cameraLookPosition
            Vector3 cameraPosition = new Vector3(cameraX, cameraY, cameraZ);
            Vector3 viewVector = cameraPosition - this.getCurrentCameraLookPosition();

            // now, scale this vector by zoom level
            viewVector = viewVector * zoomLevel;

            Vector3 newCameraPosition =  this.getCurrentCameraLookPosition() + viewVector;
            
            // slowly move the camera to this new position (removes jerkyness)
            if(oldCameraPosition != null)
                newCameraPosition = ((newCameraPosition - oldCameraPosition) * 0.05f + oldCameraPosition);

            oldCameraPosition = newCameraPosition;

            currentCameraPosition = newCameraPosition;

        }

        public void calculateCameraLookPosition()
        {
            
            Vector3 newCameraLookPosition = new Vector3((xPosition) * 60.0f + 30, 70.0f + this.selectedSquare().getCurrentHeight(), (zPosition - 1) * 60.0f + 30);

            if (oldCameraLookPosition != null)
                newCameraLookPosition = ((newCameraLookPosition - oldCameraLookPosition) * 0.05f + oldCameraLookPosition);

            oldCameraLookPosition = newCameraLookPosition;
            currentCameraLookPosition =  newCameraLookPosition;
        }

        public void rotateTile(int rotation)
        {
            this.selectedSquare().rotate(rotation);
        }

        public void setZoom(float change)
        {
            

            zoomLevel += change / 500.0f;

            if (zoomLevel < smallBound)
                zoomLevel = smallBound;
            else if (zoomLevel > largeBound)
                zoomLevel = largeBound;

        }

        public int getOrientation()
        {
            // just in case
            totalRotation = totalRotation% 360;

            if (totalRotation < 0)
                totalRotation += 360;

            if (totalRotation >= 225 && totalRotation < 315)
                return 0;
            else if (totalRotation >= 135 && totalRotation < 225)
                return 1;
            else if (totalRotation >= 45 && totalRotation < 135)
                return 2;
            else
                return 3;
        }

        public void handleRight()
        {
            switch (menuItem)
            {

                case 0:
                    break;

                case 1:
                    this.cycle(1);
                    break;

                

                case 2:
                    break;
            }
        }

        public void handleLeft()
        {
            switch (menuItem)
            {

                case 0:
                    break;

                case 1:
                    this.cycle(-1);
                    break;

                case 2:
                    break;

            }

        }

        public void handleUp()
        {


            switch (menuItem)
            {

                case 0:
                    this.moveY(1);
                    break;


                case 1:
                    this.rotateTile(-1);
                    break;

                case 2:
                    break;

            }
        }

        public void handleDown()
        {
            switch (menuItem)
            {

                case 0:
                    this.moveY(-1);
                    break;

                case 1:
                    this.rotateTile(1);
                    break;

                case 2:
                    break;

            }
        }


    }
}
