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

        private TiledMap tiledMap = null;
        private TiledMap.TileSet currentTileSet = null;
        private List<Sprite> sliceTileSet = null;

        [SerializeField]
        bool sprite = false;

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


            if (this.sliceTileSet == null || this.sliceTileSet.Count == 0)
            {
                return;
            }

            int z = 0;
            // 敷き詰める
            foreach (var layer in this.tiledMap.Layers)
            {
                this.Tiled(layer, z);
                z++;
            }

            // オブジェクト配置
            this.SetUpObjects();


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
                    Sprite tile = Sprite.Create(textureMap.texture, rect, pivot, this.tiledMap.TileWidth, 0, SpriteMeshType.FullRect);
                    // 格納
                    this.sliceTileSet.Add(tile);
                    counter++;
                }
            }

        }



        private void Tiled(TiledMap.Layer layer, int z)
        {
            GameObject goLayer = new GameObject(layer.Name);
            int layerIndex = LayerMask.NameToLayer(layer.Name);
            if (layerIndex >= 0)
            {
                goLayer.layer = layerIndex;
            }

            goLayer.transform.parent = this.gameObject.transform;
            goLayer.transform.position = new Vector3(0, 0, z * -1);


            List<TiledMap.TiledData> tiles = this.tiledMap.GetLayerData(layer);

            if (sprite)
            {
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
            else
            {
                //Textureでレイヤーを作成
                var texture = CreateLayerTexture(tiles, layer);
                SpriteRenderer spriteRenderer = goLayer.AddComponent<SpriteRenderer>();
                var sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), Vector2.zero, 32f, 0, SpriteMeshType.FullRect);
                spriteRenderer.sprite = sprite;
                spriteRenderer.sharedMaterial = this.TiledDefaultMaterial;
            }

        }

        Color clear = new Color(0, 0, 0, 0);
        private Texture2D CreateLayerTexture(List<TiledMap.TiledData> tiles, TiledMap.Layer layer)
        {
            var texture = new Texture2D(layer.Width * tiledMap.TileWidth, layer.Height * tiledMap.TileHeight, TextureFormat.ARGB32, false);

            Sprite tileSprite = this.sliceTileSet[0];
            Color[] colors = colors = tileSprite.texture.GetPixels(
                            (int)tileSprite.textureRect.x,
                            (int)tileSprite.textureRect.y,
                            (int)tileSprite.textureRect.width,
                            (int)tileSprite.textureRect.height
                        );
            for (int i = 0; i < colors.Length; i++)
                colors[i] = clear;
            //透明用
            Color[] emptyColors = colors;



            foreach (var tiledData in tiles)
            {
                if (tiledData.ID == -1)//空の透明マップ
                {
                    colors = emptyColors;

                }
                else//何かしらのマップ
                {
                    tileSprite = this.sliceTileSet[tiledData.ID];
                    //ピクセルの取得      
                    colors = tileSprite.texture.GetPixels(
                    (int)tileSprite.textureRect.x,
                    (int)tileSprite.textureRect.y,
                    (int)tileSprite.textureRect.width,
                    (int)tileSprite.textureRect.height);
                }

                // 空のテクスチャへ指定の位置にセット(左上から下に向けて描画)
                texture.SetPixels(
                    tiledData.X * (int)tileSprite.textureRect.width, // タイルのx座標位置
                    texture.height - (tiledData.Y + 1) * (int)tileSprite.textureRect.height, // タイルのy座標位置
                    (int)tileSprite.textureRect.width,
                    (int)tileSprite.textureRect.height,
                    colors  // 抽出したタイルのピクセル情報
                );
            }
            texture.Apply();
            return texture;
        }

        //spriteを使用する場合に使う
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
                        Resources.Load(path),
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