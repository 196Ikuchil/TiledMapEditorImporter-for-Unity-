using System.Collections;
using System.Xml.Serialization;


namespace Resource.TiledMap
{
    /// <summary>
    /// TileSet用クラス
    /// </summary>
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

    }




}