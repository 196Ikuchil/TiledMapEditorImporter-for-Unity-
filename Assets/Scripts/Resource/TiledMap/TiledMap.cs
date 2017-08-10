/*********************************
2015-05-31 TiledMapデータ
(.xmlの要素に従って配置)
*********************************/
using System.Collections.Generic;
using System.Xml.Serialization;
using UniLinq;
using UnityEngine;

namespace Resource.TiledMap
{

    public class Image
    {

        [XmlAttribute("source")]
        public string Source;
        [XmlAttribute("width")]
        public int Width;
        [XmlAttribute("height")]
        public int Height;

        public string SourceName
        {
            get
            {
                return this.Source.Substring(0, Source.LastIndexOf("."));
            }
        }

    }

    [XmlRoot("map")]
    public class TiledMap
    {

        public class TileSet
        {
            [XmlAttribute("firstgid")]
            public int FirstGID;
            [XmlAttribute("source")]
            public string Source;
            [XmlAttribute("name")]
            public string Name;
            [XmlAttribute("tilewidth")]
            public int TileWidth;
            [XmlAttribute("tileheight")]
            public int TileHeight;
            [XmlElement("image")]
            public Image SourceImage;
            [XmlElement("tile")]
            public List<Tile> Tiles;
        }

        public class Properties
        {
            [XmlElement("property")]
            public List<Property> All;

            public Property Find(string name)
            {
                if (this.All == null)
                {
                    return null;
                }
                foreach (var p in this.All)
                {
                    if (p.Name.Equals(name))
                    {
                        return p;
                    }
                }
                return null;
            }

            public override string ToString()
            {
                if (this.All == null || this.All.Count == 0)
                {
                    return "no properties...";
                }
                return string.Join("\n", this.All.Select(p =>
                {
                    return p.ToString();
                }).ToArray());
            }
        }

        public class Property
        {
            [XmlAttribute("name")]
            public string Name;
            [XmlAttribute("value")]
            public string Value;

            public override string ToString()
            {
                return string.Format("key:{0} value:{1}", this.Name, this.Value);
            }
        }

        public class Layer
        {
            [XmlAttribute("name")]
            public string Name;
            [XmlAttribute("width")]
            public int Width;
            [XmlAttribute("height")]
            public int Height;
            [XmlElement("data")]
            public Data TileData;
        }



        public class Data
        {
            [XmlAttribute("encoding")]
            public string Encoding;

            [XmlElement("tile")]
            public List<Tile> Tiles;

            [XmlText()]
            public string data;

            /// <summary>
            /// Tiledがcsv保存になったため, それを吸収
            /// </summary>
            public void SetTilesFromData()
            {
                foreach (string line in data.Split('\n'))
                {
                    foreach (string s in line.Split(','))
                    {
                        int v = 0;
                        // ","で終わるのでチェックが必要
                        if (int.TryParse(s, out v) == false) { continue; }
                        // 値を設定
                        var tile = new Tile();
                        tile.SetInfo(v);
                        Tiles.Add(tile);
                    }
                }
            }
        }

        public class Tile
        {
            [XmlAttribute("id")]
            public int ID;
            [XmlAttribute("gid")]
            public int GID;
            [XmlElement("properties")]
            public Properties Properties;

            public void SetInfo(int gid)
            {
                this.GID = gid;
            }
        }

        public class ObjectGroup
        {
            [XmlElement("object")]
            public List<Object> Objects;
        }
        public class Object
        {
            [XmlAttribute("id")]
            public int ID;
            [XmlAttribute("name")]
            public string Name;
            [XmlAttribute("type")]
            public string Type;
            [XmlAttribute("x")]
            public int X;
            [XmlAttribute("y")]
            public int Y;
            [XmlAttribute("width")]
            public int Width;
            [XmlAttribute("height")]
            public int Height;
            [XmlElement("properties")]
            public Properties Properties;
        }

        [XmlAttribute("version")]
        public string Version;
        [XmlAttribute("orientation")]
        public string Orientation;
        [XmlAttribute("width")]
        public int Width;    // タイル数
        [XmlAttribute("height")]
        public int Height;    // タイル数
        [XmlAttribute("tilewidth")]
        public int TileWidth;    // 1タイルの横幅
        [XmlAttribute("tileheight")]
        public int TileHeight;    // 1タイルの縦幅

        [XmlElement("tileset")]
        public List<TileSet> TileSets;

        [XmlElement("layer")]
        public List<Layer> Layers;
        [XmlElement("objectgroup")]
        public List<ObjectGroup> ObjectGroups;

        //TileSetのImageとxmlまでのResource以下のpath
        private string tileImagePathHead = "Tiles/";

        public class TiledData
        {
            public int Number;
            public int ID;    // レイヤー内での unique
            public int GID;    // 全レイヤー内での unique

            public int X;
            public int Y;
            public List<Property> Properties;

            public override string ToString()
            {
                return string.Format("tile[{0}] ID:{1} GID:{2}, x-y:{3}-{4}", this.Number, this.ID, this.GID, this.X, this.Y);
            }
        }

        private Dictionary<int, List<Property>> allProperty;

        public List<TiledData> GetLayerData(string name)
        {

            Layer targetLayer = this.GetLayer(name);

            if (targetLayer == null)
            {
                return null;
            }
            return this.GetLayerData(targetLayer);
        }
        public List<TiledData> GetLayerData(Layer targetLayer)
        {
            List<Tile> tiles = targetLayer.TileData.Tiles;

            List<TiledData> grid = new List<TiledData>(this.Width * this.Height);

            TiledData data = null;
            var allproperty = this.getProperties();        // マップに設定されているプロパティを全取得

            for (int y = 0; y < targetLayer.Height; y++)
            {

                for (int x = 0; x < targetLayer.Width; x++)
                {
                    int tileNo = this.GetTileNo(x, y);
                    data = new TiledData();
                    data.GID = tiles[tileNo].GID;
                    data.ID = this.convertToIDFromGID(data.GID);
                    data.X = x;
                    data.Y = y;
                    if (allproperty == null)
                    {
                        data.Properties = null;
                    }
                    else
                    {
                        data.Properties = allproperty.ContainsKey(data.ID) ? allproperty[data.ID] : null;
                    }
                    grid.Add(data);
                }
            }
            return grid;
        }

        public Layer GetLayer(string name)
        {

            foreach (var l in this.Layers)
            {
                if (l.Name == name)
                {
                    return l;
                }
            }

            return null;
        }

        private Dictionary<int, List<Property>> getProperties()
        {

            // すでに一度ロードしていたらそれを返す
            if (this.allProperty != null)
            {
                return this.allProperty;
            }

            // MARK 複数タイルセットを考慮するならもう一手間
            TileSet currentTiseSet = this.TileSets[0];

            // マップによってはプロパティ設定が無いかもなので
            if (currentTiseSet.Tiles == null || currentTiseSet.Tiles.Count == 0)
            {
                return null;
            }

            this.allProperty = new Dictionary<int, List<Property>>();
            foreach (Tile t in currentTiseSet.Tiles)
            {
                // ここの id から gid を作って設定しておく
                this.allProperty[t.ID] = t.Properties.All;
            }
            return this.allProperty;
        }

        public int GetTileNo(int x, int y)
        {
            return x + (y * this.Width);
        }

        private int convertToGIDFromID(int id)
        {
            // MARK 複数のtilesetに対応するなら firstgid をちゃんと見て考慮する必要がある
            return id + 1;
        }

        private int convertToIDFromGID(int gid)
        {
            // MARK 複数のtilesetに対応するなら firstgid をちゃんと見て考慮する必要がある
            return gid - 1;
        }

        #region AdjustForProductVersion

        /// <summary>
        /// xmlPaserでインスタンスを生成後呼ぶ
        /// TiledMapEditorの仕様変更のクッション
        /// </summary>
        public void InitializeSettings()
        {
            //Imageを全て自分の中にロード
            for (int i = 0; i < this.TileSets.Count; i++)
            {
                var tileset = GetTileSetXml(this.TileSets[i].Source);
                this.SetImageData(i, tileset);
            }

            //TileをstringからList<Tile>に変換代入
            Layers.ForEach(x => x.TileData.SetTilesFromData());
        }

        /// <summary>
        /// xmlから読み込んだimageのxmlpathから, xmlを読み込む
        /// </summary>
        /// <returns>The tile set xml.</returns>
        /// <param name="path">Path.</param>
        private TileSetXml GetTileSetXml(string path)
        {
            var filename = GetFileNameFromPath(path);
            var TMX = Resources.Load<TextAsset>(tileImagePathHead + filename);
            var tileSetXml = XMLParser.LoadFromXml<TileSetXml>(TMX);
            return tileSetXml;
        }
        /// <summary>
        /// 別のファイルから読み込んだデータを自分の中に移植.
        /// </summary>
        /// <param name="i">The index.</param>
        /// <param name="tileset">Tileset.</param>
        public void SetImageData(int i, TileSetXml tileset)
        {
            var sets = this.TileSets[i];
            sets.SourceImage = tileset.SourceImage;
            sets.TileHeight = tileset.TileHeight;
            sets.TileWidth = tileset.TileWidth;
            sets.Name = tileset.Name;
        }


        /// <summary>
        /// ファイルパスから最後のファイル名のみ抽出する
        /// </summary>
        /// <returns>The tile set xml path.</returns>
        /// <param name="path">Path.</param>
        private string GetFileNameFromPath(string path)
        {
            string[] str = path.Split('/');
            string[] st = str[str.Length - 1].Split('.');
            return st[0];
        }

        public string GetImageSourcePath(TileSet set)
        {
            return tileImagePathHead + this.GetFileNameFromPath(set.SourceImage.SourceName);
        }

        #endregion
    }
}