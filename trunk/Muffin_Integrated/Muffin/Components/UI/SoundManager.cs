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
using Muffin.Components.UI;
using Muffin.Components.AI;
using Muffin.Objects;
using System.Collections;

namespace Muffin.Components.UI
{
    public class SoundManager
    {
        private Dictionary<String, SoundEffect> _soundclips;
        private MuffinGame _game;

        public SoundManager(MuffinGame game)
        {
            _soundclips = new Dictionary<string, SoundEffect>();
            _game = game;
        }

        /*
         * Registers a sound clip--just pass in the name
         * of the clip found in the Audio folder (without .wav).
         * */
        public void registerSoundClip(String name, String clipname)
        {
            if (!_soundclips.ContainsKey(name))
            {
                SoundEffect clip = _game.Content.Load<SoundEffect>("Audio\\" + clipname);
                _soundclips.Add(name, clip);
            }
        }
        public SoundEffectInstance playSound(String name)
        {
            return this.playSound(name, 0.5f);
        }

        public SoundEffectInstance playSound(String name, float volume)
        {
            if (_soundclips.ContainsKey(name))
            {
                SoundEffect audioclip;
                if (_soundclips.TryGetValue(name, out audioclip))
                {
                    SoundEffectInstance instance = audioclip.Play();
                    instance.Volume = volume;
                    return instance;
                }
                
            }
            return null;
        }
    }
}
