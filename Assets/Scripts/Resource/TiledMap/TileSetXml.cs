using System.Collections;
using System.Xml.Serialization;

namespace Resource.TiledMap
{
    [XmlRoot("tileset")]
    public class TileSetXml
    {

        [XmlAttribute("name")]
        public string Name;
        [XmlAttribute("tilewidth")]
        public int TileWidth;
        [XmlAttribute("tileheight")]
        public int TileHeight;
        [XmlAttribute("tilecount")]
        public int TileCount;
        [XmlAttribute("columns")]
        public int Columns;
        [XmlElement("image")]
        public Image SourceImage;


        //ResourceからImageまでのフルパスを渡す.
        /*public string GetImageSourcePath(string pathHead)
        {
            string[] str = SourceImage.SourceName.Split('/');
            var st = str[str.Length - 1].Split('.');
            return pathHead + st[0];
        }*/

    }




}