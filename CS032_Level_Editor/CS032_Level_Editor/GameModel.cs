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
    class GameModel
    {
        Model theModel;
        String modelName;
        ContentManager Content;
        float theRotation, xSize, zSize, xPosition, yPosition, zPosition, baseSize, height;
        Vector3 translateToCenter;

        public GameModel(String name, ContentManager c, float x, float y, float z)
        {
            modelName = name;
            Content = c;
            theModel = Content.Load<Model>("Models\\" + name);
            height = this.modelHeight(name);
            baseSize = this.modelBase(name);
            xSize = 60;
            zSize = 60;
            xPosition = x;
            yPosition = y;
            zPosition = z;
            theRotation = 0;
            translateToCenter = new Vector3(xSize / 2.0f, 0, zSize / -2.0f); 
        }

        public Model getModel()
        {
            return theModel;
        }

        public void setRotation(float rotation)
        {
            theRotation = rotation % 360;
        }

        public float getRotation()
        {
            return theRotation;
        }

        public float getHeight()
        {
            return height;
        }

        public void setModel(String name)
        {
            if (name == "blank" || name == "blank_selected")
                theModel = null;
            else
                theModel = Content.Load<Model>("Models\\" + name);
            height = this.modelHeight(name);

            modelName = name;
        }

        private float modelHeight(String name)
        {
            switch (name)
            {
                case "flat":
                case "wedge":
                case "corner":
                case "inverted_corner":
                case "flat_selected":
                case "wedge_selected":
                case "corner_selected":
                case "inverted_corner_selected":
                    return 1.415f;

                case "blank":
                case "blank_selected":
                    return 0;
                default:
                    return 0;
            }

        }

        private float modelBase(String name)
        {
            switch (name)
            {
                case "flat":
                case "wedge":
                case "corner":
                case "inverted_corner":
                case "flat_selected":
                case "wedge_selected":
                case "corner_selected":
                case "inverted_corner_selected":
                    return 5;

                case "blank":
                case "blank_selected":
                    return 5;
                default:
                    return 5;
            }

        }

        public Vector3 getTranslateToCenter()
        {
            return translateToCenter;
        }

        public Vector3 getPosition()
        {
            return new Vector3(12 * baseSize * xPosition, 12 * height * yPosition, 12 * baseSize * zPosition);
        }

        public void writeXML(XmlWriter w)
        {
            w.WriteStartElement("OBJECT");
            w.WriteStartElement("MODEL");
            w.WriteAttributeString("NAME", modelName);
            w.WriteEndElement();
            w.WriteStartElement("POSITION");
            w.WriteAttributeString("X", xPosition.ToString());
            w.WriteAttributeString("Y", yPosition.ToString());
            w.WriteAttributeString("Z", zPosition.ToString());
            w.WriteEndElement();
            w.WriteStartElement("ROTATION");
            w.WriteAttributeString("ANGLE", MathHelper.ToRadians(this.getRotation()).ToString());
            w.WriteEndElement();
            w.WriteEndElement();
        }
    }
}
