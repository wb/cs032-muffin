using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using System.Text;

namespace WindowsGame1
{
    class Parser
    {
        private XmlTextReader m_xml_reader;

        void ParseFile(string file)
        {
            m_xml_reader = new XmlTextReader(file);
            while (m_xml_reader.Read())
            {
                switch (m_xml_reader.NodeType)
                {
                    case XmlNodeType.Element:
                        break;
                    default:
                        break;
                }
            }
        }

    }
}
