using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Resource.TiledMap;
namespace Test
{

    public class TileSetXmlTest : MonoBehaviour
    {

        //../hoge.xmlを打ち込んでもらう
        public string path;
        TileSetXml tiledMap;
        TextAsset TMX;

        string pathHead = "Tiles/";
        // Use this for initialization
        void Start()
        {
            string filename = GetXmlFileName(path);

            //文字から
            TMX = Resources.Load<TextAsset>(pathHead + filename);
            this.tiledMap = XMLParser.LoadFromXml<TileSetXml>(this.TMX);
            Debug.Log(tiledMap.SourceImage.SourceName);
            Debug.Log(GetImageSourcePath(tiledMap.SourceImage.SourceName));
        }

        private string GetXmlFileName(string path)
        {
            string[] str = path.Split('/');
            string[] st = str[str.Length - 1].Split('.');
            return st[0];
        }

        //ResourceからImageまでのフルパスを渡す.
        private string GetImageSourcePath(string path)
        {
            string[] str = path.Split('/');
            return pathHead + str[str.Length - 1];

        }


        // Update is called once per frame
        void Update()
        {

        }
    }
}