/*********************************
 2015-05-31 XMLパーサー
*********************************/
using UnityEngine;
using System.Xml.Serialization;

namespace Resource.TiledMap
{
    public class XMLParser
    {
        public static T LoadFromXml<T>(TextAsset xml)
            where T : class
        {

            if (xml == null)
            {
                Debug.LogError("Can not file xml file!! .... ");
                return null;
            }

            var ser = new XmlSerializer(typeof(T));

            var stringReader = new System.IO.StringReader(xml.text);

            var obj = ser.Deserialize(stringReader);

            var retClass = (T)obj;

            return retClass;
        }
    }
}