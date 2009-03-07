using System;
using System.Collections.Generic;
using System.Text;
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
using WindowsGame1;

namespace WindowsGame1
{
    /**
     * GameCharacter represents a movable character in the game.
     * It is generally only used for AI characters, but has the option of adding all characters to it in case this 
     * functionality is needed later.
     * 
     * This operates hand-in-hand with the AI object in order to compute actions for the NPCs in the game
     **/
    class GameCharacter : GameObject
    {
        private AIState m_state;
        private bool m_isEnemy;

        // Constructs a new GameCharacter
        public GameCharacter(bool isEnemy, int startX, int startY) : base(null, isEnemy?ModelType.ENEMY:ModelType.HUMAN, 
            new Vector3((float)startX, (float)startY, (float)0.0), new Vector3())
        {
            m_isEnemy = isEnemy;
            m_state = AIState.INACTIVE;
        }

        /**
         * Performs the main AI processing for the enemy represented by this object
         * The "output" is the setting for the motion vectors to show desired movement
         **/
        public void DoAI(Object AI)
        {
            if (!m_isEnemy)
                return;
        }

    }
}
