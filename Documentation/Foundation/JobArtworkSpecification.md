# Job Artwork Specification

全レイヤーは同一キャンバス・座標・UVで書き出す。PNGは上端原点の組版、Unity読込後はGetPixelsの下端原点を既存Paperの座標変換で処理する。個別Textureの位置合わせやSpriteのtrimをしない。

| Layer | 内容 | Alpha / Color |
|---|---|---|
| Paper | 消えない紙 | RGBA、不透明、sRGB |
| Protected | 残す完成線 | RGBA、背景透明、sRGB |
| Erasable | 消す鉛筆線 | RGBA、背景透明、sRGB |
| EraseMask | 消去対象の重み | グレーRGB、alpha255、linear |
| ProtectMask | 保護接触の重み | グレーRGB、alpha255、linear |
| CompletePreview | Paper＋Protected | 不透明、sRGB |
| InitialPreview | Paper＋Protected＋Erasable | 不透明、sRGB、制作補助 |

mask判定はred値。描画の色・明度から実行中に判定しない。マスクはRead/Write有効、mipmapなし、NPOT None、非圧縮、Clamp、Bilinear、最大4096。通常画像はRead/Write不要。

原本2480×3508、実使用1240×1754。既存8件の解像度を変更していない。v2はJSONのcanvas/runtime値で再生成可能。UVは常に全面0〜1。worldSizeは幅5、縦は画像比率から決定する。既存8件は従来worldSizeを保持する。ズームは既存カメラ処理のままで接触Worldサイズを変えない。

マニフェスト: jobId/sourceVersion/generatorVersion、canvas/runtimeサイズ、sourceType、座標原点/UV、フォント名・ハッシュ、仕様ハッシュ、各PNGハッシュ。絶対パスやユーザー名を含めない。
