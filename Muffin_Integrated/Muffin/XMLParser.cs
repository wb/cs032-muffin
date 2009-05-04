using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
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

namespace Muffin
{
    class XMLParser
    {
        XmlDocument m_doc;
        XmlElement m_root;
        MuffinGame _muffinGame;

        public XMLParser(XmlDocument doc, MuffinGame game)
        {
            m_doc = doc;
            m_root = doc.DocumentElement;
            _muffinGame = game;
        }

        public void loadLevel(List<GameObject> m_objects, List<Model> m_models)
        {
            //Console.WriteLine("Name is: "  + m_root.Name + "| YES!");
            //m_read.Read();
            //Console.WriteLine("Name 2 is: " + m_read.Name);

            if (m_root.Name.Equals("LEVEL"))
            {
                Console.WriteLine("Loading level...");
                XmlNodeList children = m_root.ChildNodes;

                foreach (XmlNode x in children)
                {
                    parseObject(x, m_objects, m_models);
                }
            }
        }

        private void parseObject(XmlNode x, List<GameObject> m_objects, List<Model> m_models)
        {
            if (x.Name.Equals("OBJECT"))
            {
                XmlNodeList children = x.ChildNodes;

                if (children.Count != 4)
                {
                    Console.WriteLine("Bad Parsing!!!");
                    throw new Exception();
                }

                ModelName name = parseName(children.Item(0));
                Vector3 pos = parsePosition(children.Item(1));
                //Vector3 dimensions = parseDimensions(children.Item(2));
                Vector3 rot = parseRotation(children.Item(3));
                Boolean locked = false;
                Quaternion quat = new Quaternion();
                Matrix rotMat = Matrix.CreateFromYawPitchRoll(rot.Y, rot.X, rot.Z);

                //add the object to the array of active objects
                Quaternion.CreateFromRotationMatrix(ref rotMat, out quat);

                if (name == ModelName.AI)
                {
                    m_objects.Add(new AIObject(null, name, pos, quat, new Vector3(60, 60, 60), 1000.0f, GameConstants.GameObjectScale));
                }
                else if (name == ModelName.BOX)
                {
                    m_objects.Add(new GameObject(null, ModelType.OBJECT, name, pos, quat, locked, new Vector3(55, 55, 55), 5000.0f, GameConstants.GameObjectScale * 55.0f/60.0f));
                }
                else if (name == ModelName.COIN)
                {
                    m_objects.Add(new CollectableObject(name, new CollectionCallback(_muffinGame.coinCollected), null, 40.0f, pos + new Vector3(0,30.0f, 0), new Vector3(60, 60, 60), GameConstants.GameObjectScale, false));
                }
                else if (name == ModelName.GRASS)
                {
                    m_objects.Add(new TerrainObject(null, name, pos, quat, new Vector3(60, 30, 60), GameConstants.GameObjectScale, (int)(pos.X / 60.0f), (int)(pos.Z / 60.0f)));
                }
                else if (name == ModelName.PLAYER)
                {
                    m_objects.Add(new PlayerObject(null, name, pos, quat, new Vector3(45, 45, 45), 1000.0f, 7.5f));
                }
                else if (name == ModelName.STAR)
                {
                    m_objects.Add(new CollectableObject(name, new CollectionCallback(_muffinGame.starCollected), null, 40.0f, pos + new Vector3(0,100.0f,0), new Vector3(60, 60, 60), GameConstants.GameObjectScale, true));
                }

            }
            else
            {
                Console.WriteLine("Bad parse input");
            }
        }

        private ModelName parseName(XmlNode x)
        {
            if (x.Name.Equals("MODEL"))
            {
                if (x.Attributes.Item(0).Name.Equals("NAME"))
                {
                    String s = x.Attributes.Item(0).Value;

                    if (s.Equals("grass"))
                    {
                        return ModelName.GRASS;
                    }
                    else if (s.Equals("ai"))
                    {
                        return ModelName.AI;
                    }
                    else if (s.Equals("box"))
                    {
                        return ModelName.BOX;
                    }
                    else if (s.Equals("coin"))
                    {
                        return ModelName.COIN;
                    }
                    else if (s.Equals("player"))
                    {
                        return ModelName.PLAYER;
                    }
                    else if (s.Equals("star"))
                    {
                        return ModelName.STAR;
                    }
                    else if (s.Equals("blank"))
                    {
                        return ModelName.NONE;
                    }
                    else
                    {
                        Console.WriteLine("Bad Parse!");
                        throw new Exception();
                    }
                }
                else
                {
                    throw new Exception();
                }
            }
            throw new Exception();
        }

        private Vector3 parsePosition(XmlNode x)
        {
            if (x.Name.Equals("POSITION"))
            {
                XmlAttributeCollection attributes = x.Attributes;
                float X, Y, Z;

                if (attributes.Item(0).Name.Equals("X")
                    && attributes.Item(1).Name.Equals("Y")
                    && attributes.Item(2).Name.Equals("Z"))
                {
                    X = ((float)Double.Parse(attributes.Item(0).Value));
                    Y = ((float)Double.Parse(attributes.Item(1).Value));
                    Z = ((float)Double.Parse(attributes.Item(2).Value));
                    return new Vector3(X, Y, Z);
                }
                else
                {
                    throw new Exception();
                }
            }
            throw new Exception();
        }

        private Vector3 parseRotation(XmlNode x)
        {
            if (x.Name.Equals("ROTATION"))
            {
                XmlAttributeCollection attributes = x.Attributes;
                float X, Y, Z;

                if (attributes.Item(0).Name.Equals("ANGLE"))
                {
                    X = 0.0f;
                    Y = (float)Double.Parse(attributes.Item(0).Value);
                    Z = 0.0f;

                    return new Vector3(X, Y, Z);
                }
                else
                {
                    throw new Exception();
                }
            }
            throw new Exception();
        }
    }
}
