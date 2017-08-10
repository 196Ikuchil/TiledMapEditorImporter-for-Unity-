/*********************************
2015-05-30
*********************************/
using UnityEngine;
using System.Collections.Generic;
using UniLinq;

namespace Resource.TiledMap
{
    public class TiledMapCreator : MonoBehaviour
    {
        public TextAsset TMX;
        public Material TiledDefaultMaterial = null;
        public string DirPath;

        private TiledMap tiledMap = null;
        private TiledMap.TileSet currentTileSet = null;
        private List<Sprite> sliceTileSet = null;



        private void Start()
        {
            // 読み込んでパース
            this.tiledMap = XMLParser.LoadFromXml<TiledMap>(this.TMX);
            //TiledMapEditorの現在の仕様に対応
            this.tiledMap.InitializeSettings();

            // 敷き詰め
            this.CreateTiledMap();
        }

        private void CreateTiledMap()
        {
            tiledMap.TileSets.ForEach(x =>
            {
                this.currentTileSet = x;
                // タイルセット画像をスライスしていく
                this.SliceTilseSet();
            });
            //this.currentTileSet = this.tiledMap.TileSets[0];    // 複数はとりあえず想定しない


            if (this.sliceTileSet == null || this.sliceTileSet.Count == 0)
            {
                return;
            }


            // 敷き詰める
            foreach (var layer in this.tiledMap.Layers)
            {
                this.Tiled(layer);
            }

            // オブジェクト配置
            this.SetUpObjects();

            // カメラさんにプレイヤーを中央にして移すように移動してもらう
            // Camera.main.SendMessage("SetupPlayer");

        }

        private void SliceTilseSet()
        {

            string path = tiledMap.GetImageSourcePath(currentTileSet);
            Sprite textureMap = Resources.Load(path, typeof(Sprite)) as Sprite;

            if (textureMap == null)
            {
                Debug.LogError("can not find tileset image..." + path);
                return;
            }
            //xmlのimage情報はid順に並んでいるので最初のみ初期化すれば,その後は順番にリストに格納されていく
            if (this.sliceTileSet == null)
                this.sliceTileSet = new List<Sprite>(this.tiledMap.Width * this.tiledMap.Height);

            // スライスしていく
            int width = this.currentTileSet.SourceImage.Width / this.currentTileSet.TileWidth;
            int height = this.currentTileSet.SourceImage.Height / this.currentTileSet.TileHeight;
            Vector2 pivot = new Vector2(0.5f, 0.5f);
            int counter = 0;
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    // 範囲ぎめ
                    Rect rect = new Rect(
                        x * this.tiledMap.TileWidth,
                        textureMap.texture.height - (y + 1) * this.tiledMap.TileHeight,
                        this.tiledMap.TileWidth,
                        this.tiledMap.TileHeight
                    );
                    // スライス
                    Sprite tile = Sprite.Create(textureMap.texture, rect, pivot, this.tiledMap.TileWidth);
                    // 格納
                    this.sliceTileSet.Add(tile);
                    counter++;
                }
            }

        }



        private void Tiled(TiledMap.Layer layer)
        {
            GameObject goLayer = new GameObject(layer.Name);
            int layerIndex = LayerMask.NameToLayer(layer.Name);
            if (layerIndex >= 0)
            {
                goLayer.layer = layerIndex;
            }

            goLayer.transform.parent = this.gameObject.transform;
            List<TiledMap.TiledData> tiles = this.tiledMap.GetLayerData(layer);
            foreach (var tiledData in tiles)
            {

                if (tiledData.GID == 0)
                {
                    // ブランクタイル
                    continue;
                }

                // タイルスプライトを作って設置
                GameObject tile = this.CreateTileSprite(tiledData, layer.Name);
                tile.transform.position = new Vector3(tiledData.X, tiledData.Y * -1, 0);
                tile.transform.parent = goLayer.transform;
                tile.layer = goLayer.layer;
            }
        }

        private GameObject CreateTileSprite(TiledMap.TiledData tiledData, string layerName)
        {
            GameObject tile = new GameObject("tile_" + tiledData.GID);
            SpriteRenderer tileRender = tile.AddComponent(typeof(SpriteRenderer)) as SpriteRenderer;
            tileRender.sprite = this.sliceTileSet[tiledData.ID];
            tileRender.sortingLayerName = layerName;
            tileRender.material = this.TiledDefaultMaterial;

            // Collision設定
            this.SetCollision(tiledData, tile);

            return tile;
        }

        private void SetCollision(TiledMap.TiledData tiledData, GameObject tile)
        {
            if (tiledData.Properties == null || tiledData.Properties.Count == 0)
            {
                return;
            }

            bool hasCollision = tiledData.Properties.Where(t =>
            {
                return t.Name.Equals("IsCollision") && t.Value.Equals("1");
            }).Any();

            if (hasCollision)
            {
                var boxCollider2d = tile.AddComponent<BoxCollider2D>();
                boxCollider2d.size = new Vector2(0.9f, 0.9f);    // ちょい小さくする
            }
        }

        private void SetUpObjects()
        {
            if (this.tiledMap.ObjectGroups == null || this.tiledMap.ObjectGroups.Count == 0)
            {
                Debug.Log("not find objects...");
                return;
            }

            foreach (var objGroup in this.tiledMap.ObjectGroups)
            {
                foreach (var obj in objGroup.Objects)
                {

                    var p = obj.Properties.Find("Path");
                    string path = "";
                    if (p != null)
                    {
                        path = p.Value;
                    }
                    // TypeをPrefabの名前として扱う
                    GameObject go = Instantiate(
                        Resources.Load(path + obj.ID),//obj.Type),
                    new Vector3(obj.X / obj.Width, obj.Y / obj.Height * -1, 0f),
                        this.gameObject.transform.rotation
                    ) as GameObject;

                    if (obj.Properties == null)
                    {
                        continue;
                    }
                    // レイヤーが指定されていればセット
                    p = obj.Properties.Find("Layer");
                    if (p != null)
                    {
                        go.layer = LayerMask.NameToLayer(p.Value);
                    }
                    // Sorting Layerが指定されていればセット
                    p = obj.Properties.Find("Sort");
                    if (p != null)
                    {
                        go.GetComponent<SpriteRenderer>().sortingLayerName = p.Value;
                    }
                }
            }
        }

    }
}