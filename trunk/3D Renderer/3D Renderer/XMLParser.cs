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

namespace _3D_Renderer
{
    class XMLParser
    {
        XmlDocument m_doc;
        XmlElement m_root;

        public XMLParser(XmlDocument doc)
        {
            m_doc = doc;
            m_root = doc.DocumentElement;
        }

        public void loadLevel(List<GameObject> m_objects, List<Model> m_models) 
        {
            //Console.WriteLine("Name is: "  + m_root.Name + "| YES!");
            //m_read.Read();
            //Console.WriteLine("Name 2 is: " + m_read.Name);

            if(m_root.Name.Equals("LEVEL")) 
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

                if (children.Count != 3)
                {
                    Console.WriteLine("Bad Parsing!!!");
                    throw new Exception();
                }
                   
                ModelName name = parseName(children.Item(0));
                Vector3 pos = parsePosition(children.Item(1));
                Vector3 rotation = parseRotation(children.Item(2));

                Console.WriteLine("Model: ");
                Console.WriteLine(pos.ToString());
                    Console.WriteLine(rotation.ToString());
                        
                // I DID THAT - quaternion of 0s
                //add the object to the array of active objects
                m_objects.Add(new GameObject((Model)m_models.ElementAt((int)name),ModelType.TERRAIN, name, pos, rotation, true, new Vector3(60,24,60)));
            } else {
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

                    if (s.Equals("flat"))
                    {
                        return ModelName.FLAT;
                    }
                    else if (s.Equals("wedge"))
                    {
                        return ModelName.WEDGE;
                    }
                    else if (s.Equals("corner"))
                    {
                        return ModelName.CORNER;
                    }
                    else if (s.Equals("inverted_corner"))
                    {
                        return ModelName.INVERTED_CORNER;
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
                    X = ((float) Double.Parse(attributes.Item(0).Value) * 60.0f);
                    Y = ((float) Double.Parse(attributes.Item(1).Value) * 17.0f);
                    Z = ((float) Double.Parse(attributes.Item(2).Value) * 60.0f);
                    return new Vector3(X,Y,Z);
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
                    Y = MathHelper.ToDegrees((float)Double.Parse(attributes.Item(0).Value));
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
