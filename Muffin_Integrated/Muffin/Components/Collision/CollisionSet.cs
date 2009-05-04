using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using System.ComponentModel;
using System.Data;
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
using Muffin.Components.Renderer;
using Muffin.Components.Physics;
using Muffin;
using ModelCollision;

namespace Muffin.Components.Collision
{
    /// <summary>
    /// This is a game component that implements IUpdateable.
    /// </summary>
    public class CollisionSet : Microsoft.Xna.Framework.GameComponent
    {
        private MuffinGame _muffinGame;
        public CollisionSet(Game game)
            : base(game)
        {
            _muffinGame = (MuffinGame)game;
        }

        /// <summary>
        /// Allows the game component to perform any initialization it needs to before starting
        /// to run.  This is where it can query for any required services and load content.
        /// </summary>
        public override void Initialize()
        {
            // TODO: Add your initialization code here
            _muffinGame.grid = new Grid(new Vector3(-400, -400, -400), new Vector3(5000, 5000, 5000));
            foreach (GameObject o in _muffinGame.allObjects)
            {
                _muffinGame.grid.insertElement(o);
            }
            base.Initialize();
        }

        /// <summary>
        /// Allows the game component to update itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        public override void Update(GameTime gameTime)
        {
            // TODO: Add your update code here

            foreach (GameObject o in _muffinGame.allObjects)
            {
                if (o.modelType == ModelType.TERRAIN)
                    continue;
                _muffinGame.grid.moveElement(o);
            }


            base.Update(gameTime);
        }

   }
}