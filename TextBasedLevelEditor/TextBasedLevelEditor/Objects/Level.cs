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

namespace TextBasedLevelEditor
{
    class Level
    {
        private DrawableObject[, ,] _levelObjects;
        private Vector3 _levelSize;

        public Level(int x, int y, int z)
        {
            _levelObjects = new DrawableObject[x, y, z];
            _levelSize = new Vector3(x, y, z);
        }

        /*
         * This method registers an object with the level only if
         * there isn't one there. Returns true if it added,
         * and false if an error occured.
         * 
         * */

        public Boolean registerObject(LevelObject levelObject)
        {
            
            // get the position of this object
            int x = (int)levelObject.position.X / Constants.objectSize;
            int y = (int)(levelObject.position.Y / Constants.objectSize * 2.0f);
            int z = (int)levelObject.position.Z / Constants.objectSize;

            // get the height of the object
            int height = (int) levelObject.size.Y;

            // make sure the object will fit in the level
            if (height == Constants.objectHeight)
            {
                if (y + 1 >= _levelSize.Y)
                {
                    Console.WriteLine("Object is too tall for level!");
                    return false;
                }

            }
            else if (height == Constants.terrainHeight)
            {
                if (y >= _levelSize.Y)
                    return false;
            }

            // if its 30 tall, just add it
            if (height == Constants.terrainHeight)
            {
                if (this.isGridSpaceEmpty(x, y, z))
                {
                    _levelObjects[x, y, z] = levelObject;
                    return true;
                }
                else
                    return false;
            }

            else if (height == Constants.objectHeight)
            {
                if (this.isGridSpaceEmpty(x, y, z) && this.isGridSpaceEmpty(x, y + 1, z))
                {
                    _levelObjects[x, y, z] = levelObject;
                    _levelObjects[x, y + 1, z] = new PlaceholderObject(levelObject);
                    return true;
                }
                else
                    return false;
            }

            return false;
            
        }

        public Boolean removeObject(int x, int y, int z)
        {
            if (_levelObjects[x, y, z] != null)
            {
                DrawableObject toRemove = _levelObjects[x, y, z];

                // if this is a level object
                if (toRemove is LevelObject)
                {
                    // if this is a tall object
                    if (((LevelObject)toRemove).size.Y == Constants.objectHeight)
                    {
                        _levelObjects[x, y, z] = null;
                        _levelObjects[x, y + 1, z] = null;
                    }
                    else
                    {
                        _levelObjects[x, y, z] = null;
                    }

                    return true;
                }
            }

            return false;
        }

        public Boolean removeObjectOfType(int x, int y, int z, String modelName)
        {
            if (_levelObjects[x, y, z] != null)
            {
                DrawableObject toRemove = _levelObjects[x, y, z];

                // if this is a level object and is of the type passed in
                if (toRemove is LevelObject && ((LevelObject)toRemove).modelName == modelName)
                {
                    // if this is a tall object
                    if (((LevelObject)toRemove).size.Y == Constants.objectHeight)
                    {
                        _levelObjects[x, y, z] = null;
                        _levelObjects[x, y + 1, z] = null;
                    }
                    else
                    {
                        _levelObjects[x, y, z] = null;
                    }

                    return true;
                }
            }

            return false;
        }

        /*
         * This method finds the highest object in the current grid square.
         * */

        public LevelObject getTopLevelObject(int x, int z)
        {
            DrawableObject currentObject;

            for (int y = (int) _levelSize.Y - 1; y >= 0; y--)
            {
                currentObject = _levelObjects[x, y, z];

                if (currentObject != null)
                {
                    if (currentObject is PlaceholderObject)
                    {
                        return ((PlaceholderObject)currentObject).getInternalObject();
                    }
                    else if (currentObject is LevelObject)
                    {
                        return currentObject as LevelObject;
                    }
                }
            }

            return null;
        }

        public Boolean isGridSpaceEmpty(int x, int y, int z)
        {
            if (_levelObjects[x, y, z] == null)
                return true;
            else
                return false;
        }

        /*
         * This method draws one layer of the grid.
         * */

        public void drawLayer(int layer, SpriteBatch spriteBatch)
        {
            for (int x = 0; x < _levelSize.X; x++)
            {
                for (int z = 0; z < _levelSize.Z; z++)
                {
                    if(_levelObjects[x,layer,z] != null)
                        _levelObjects[x, layer, z].draw(spriteBatch);
                }
            }
        }

        public DrawableObject[, ,] getInternalArray()
        {
            return (DrawableObject[,,])_levelObjects.Clone();
        }
    }
}
