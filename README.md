# TiledMapEditorImporter(for Unity)
2017/8/11




# Version
Unity 5.5.1f1 Personal

TiledMapEditor 1.0.2




# このリポジトリについて
https://tmblr.co/ZDr3Rs1mBaDUU
この方のスクリプトを, 現在のバージョン用に軽く書き換えました.
変更点
・TiledMapEditorのLayerのDataの保存形式がcsvになっていたところに対応
・Spriteで１ブロックずつ, または1枚のTextureでLayerごとに生成可能に
・プロパティやオブジェクト情報がtmxの中身変更によって読めなかった問題を解決
・作者様が搭載していた機能をいくつか復元

オブジェクト情報とプロパティ情報はほぼ取り込めるはずなので, あとはうまく使ってください。




# 使い方
・TestCreateMapを使って見ながら見てください.
・TiledMapEditorのtmx,tsxファイルはunityでは読めないのでxmlファイルに書き換えてください.
・現在の設定では, 
Resources/TiledMap/Town/Test/ -> メインのxmlファイル(現在はエディタ上で紐付けで読み込んでいる)
Resources/Tiles/ -> Tileに使用するSpriteとそのxmlファイル




 
# 参照元
https://tmblr.co/ZDr3Rs1mBaDUU

Copyright (c) 2016 Ryota Murohoshi






# Licence
Copyright (c) 2017 196 Kakinuma

Released under the MIT license
