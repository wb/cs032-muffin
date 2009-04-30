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
    class XMLSave
    {
        public XMLSave()
        {
        }

        public void write(Level level)
        {
            Console.WriteLine("Saving file.");

            FileStream fs = new FileStream("level.xml", FileMode.Create);

            XmlWriter w = XmlWriter.Create(fs);

            w.WriteStartDocument();
            w.WriteStartElement("LEVEL");

            // add each object to the xml file
            DrawableObject[, ,] objects = level.getInternalArray();

            foreach (DrawableObject o in objects)
            {
                if (!(o is LevelObject))
                    continue;

                LevelObject theObject = (LevelObject) o;

                // Object
                w.WriteStartElement("OBJECT");

                // Model
                w.WriteStartElement("MODEL");
                w.WriteAttributeString("NAME", theObject.modelName);
                w.WriteEndElement();

                // Position
                w.WriteStartElement("POSITION");
                w.WriteAttributeString("X", theObject.position.X.ToString());
                w.WriteAttributeString("Y", theObject.position.Y.ToString());
                w.WriteAttributeString("Z", theObject.position.Z.ToString());
                w.WriteEndElement();

                
                // Size
                w.WriteStartElement("SIZE");
                w.WriteAttributeString("X", theObject.size.X.ToString());
                w.WriteAttributeString("Y", theObject.size.Y.ToString());
                w.WriteAttributeString("Z", theObject.size.Z.ToString());
                w.WriteEndElement();
                
                // Rotation
                w.WriteStartElement("ROTATION");
                w.WriteAttributeString("ANGLE", theObject.rotation.ToString());
                w.WriteEndElement();

                w.WriteEndElement();
            }
            w.WriteEndElement();

            w.WriteEndDocument();

            w.Flush();
            fs.Close();
        }
    }
}
