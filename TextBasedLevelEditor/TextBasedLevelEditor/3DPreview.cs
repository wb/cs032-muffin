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
    class _3DPreview
    {
        Dictionary<String, Texture2D> _textures;
        int _xPosition, _yPosition;

        public _3DPreview(Game game, int xPos, int yPos)
        {
            _xPosition = xPos;
            _yPosition = yPos;

            _textures = new Dictionary<string, Texture2D>();
            _textures.Add("ai", game.Content.Load<Texture2D>("Pixels\\ai"));
            _textures.Add("box", game.Content.Load<Texture2D>("Pixels\\box"));
            _textures.Add("coin", game.Content.Load<Texture2D>("Pixels\\coin"));
            _textures.Add("grass", game.Content.Load<Texture2D>("Pixels\\grass"));
            _textures.Add("player", game.Content.Load<Texture2D>("Pixels\\player"));
            _textures.Add("star", game.Content.Load<Texture2D>("Pixels\\star"));
        }

        public void draw(Level level, SpriteBatch spriteBatch)
        {
            this.draw(level, spriteBatch, Constants.gridSizeY);

        }
        public void draw(Level level, SpriteBatch spriteBatch, int maxLevel)
        {
            

            DrawableObject[,,] tiles = level.getInternalArray();

            for (int z = 0; z < Constants.gridSizeZ; z++)
            {
                for (int y = 0; y < maxLevel; y++)
                {
                    for (int x = 0; x < Constants.gridSizeX; x++)
                    {

                        // get the grid contents
                        DrawableObject contents = tiles[x, y, z];

                        // draw if this isn't null
                        if (contents != null)
                        {
                            
                            LevelObject tile = null;
                            int xPos = x, yPos = 0, zPos = z;

                            if (contents is LevelObject)
                            {
                                tile = contents as LevelObject;
                                yPos = y;
                            }
                            else if (contents is PlaceholderObject)
                            {
                                tile = (contents as PlaceholderObject).getInternalObject();
                                yPos = y - 1;
                            }

                            // if we get the texture
                            Texture2D texture;
                            if (_textures.TryGetValue(tile.modelName, out texture))
                            {
                                // then draw it

                                
                                spriteBatch.Draw(texture, this.rectangle(xPos, yPos, zPos), Color.White);
                            }
                        }
                    }
                }
            }
        }

        private Rectangle rectangle(int x, int y, int z)
        {
            
            int xCoordinate = _xPosition + ((x - z) * 6);
            int yCoordinate = _yPosition + ((x + z) * 3) - (3 * y);
            return new Rectangle(xCoordinate, yCoordinate, 14, 13);
        }
    }
}
