# External Test：依頼アート制作・追加手順

## 制作方式
Figma、CLIP STUDIO、Illustratorは使用していません。画像生成で制作したオリジナルの漫画完成線・子どもの落書き・美術ラフを保存し、Pillow / NumPyで紙、文字、下書き、マスク、見本を決定的に生成します。日本語はWindowsのUDデジタル教科書体などで組版するため、AI画像内の不正確な文字を採用していません。フォントファイル自体は配布しません。

保存済み `*-original.png` が画像原本です。同じ原本・フォント・スクリプトからの再書き出しは再現可能です。画像生成モデルへ同じ指示を再送しても、同一画像になる保証はありません。原本は削除せず、更新時は別名で保存してください。原本ハッシュは `inputs.sha256.json` に記録しています。

## フォルダー
- `generate.py`：組版・レイヤー・マスクの生成。乱数シードは依頼IDに固定。
- `job-001.json` ～ `job-008.json`：依頼文、成功文、紙・筆記具、報酬、精密／支給品設定。
- `Source/TEST_001` ～ `Source/TEST_008`：2480×3508の制作原本PNG、文章と座標を含むmanifest。
- `../../Assets/Keshiya/ExternalArt/TEST_001` ～ `TEST_008`：1240×1754の実行用PNGとmanifest。
- `../../Assets/Keshiya/Resources/Artwork-TEST_*.asset`：共通JobArtworkData。
- `../../Assets/Keshiya/Resources/External-TEST_*.asset`：共通JobDefinition。

各依頼は同じ座標の7画像を持ちます：Paper / Protected / Erasable / EraseMask / ProtectMask / CompletePreview / InitialPreview。最初の6画像が正式なアート形式で、InitialPreviewは選択画面用の合成見本です。

## 再生成
Windows、Python、Pillow、NumPy、およびスクリプトに指定したWindows日本語フォントが必要です。

```powershell
python Tools/ExternalArt/generate.py
python Tools/ExternalArt/validate.py
```

一部だけ更新：`python Tools/ExternalArt/generate.py --jobs 2,6`
低解像度版：`python Tools/ExternalArt/generate.py --runtime-width 880`
高解像度版：`python Tools/ExternalArt/generate.py --runtime-width 2048`
制作原本は常に2480×3508です。実行用は縦横比を維持します。検証スクリプトの標準サイズ期待値は1240×1754なので、解像度を変更した場合は検証期待値も変更してください。

Unityのメニュー `Keshiya > External Test > Import artwork and configure` を実行すると、manifestを列挙してTextureImporter、JobArtworkData、JobDefinition、シーン参照を更新します。Unity上で画像を手で位置合わせする作業はありません。

## 9件目以降
1. `generate.py` に `make009()` を追加。`Doc(9, '依頼名')` を作り、`paper`、`keep`、`erase`へ同一サイズで描画します。既存文書の `text`、`line`、`rules` を再利用できます。任意の外部原本も同サイズで合成できます。
2. `job-008.json` をコピーして `job-009.json` を作り、依頼文、素材ID、報酬などを編集します。
3. `python Tools/ExternalArt/generate.py --jobs 9` を実行。7レイヤーとmanifestが自動生成されます。
4. 上記Unityインポートメニューを実行。フォルダー列挙なので読み込みコードの追加は不要です。
5. 新規依頼の完成見本、文字、EraseMask、ProtectMaskを確認し、対象数のテスト期待値を更新してビルドします。消去コードを増やす必要はありません。

## 判定と座標
EraseMask / ProtectMaskはRGBの8bitグレースケール。白が完全対象、黒が対象外、中間値がアンチエイリアスの重みです。色を見て対象を判定するのではなく、独立したマスクをDrawing / ProtectedDrawingが読みます。EraseMaskの初期重み総量と残量から消去率を計算します。筆記具の黒鉛量補正は全体へ均等に適用します。

座標は画像左上原点から、Unityでは右向きX・紙の奥向きZへ変換します。紙面サイズは5×7.07258 world units。Unityのテクスチャ配列は下から上なのでYを反転します。画面入力はカメラから紙平面へのRayでWorldへ戻し、Worldから共通UVへ変換します。ズームは正投影カメラのサイズのみ変更するため、消しゴムのWorld接触サイズは変わりません。

ProtectMaskは保護画像のアルファを1原本ピクセルだけ内側へ縮めています。漫画・レシピの下書きは、完成線の周囲に細型の角が通るわずかな間隔を確保しています。消せない保護線の直下に正式な消去目標を隠してはいません。

## 描画・負荷
高解像度のPaper / Protected / ErasableはGPUで合成します。CPUは黒鉛残量・保護残量・紙損傷を状態テクスチャへ書きます。擦った領域だけCPUの画素を再計算し、更新は最大30Hzです。マスクは非sRGB、非圧縮、Readable。紙面と文字は非圧縮、ミップマップなし、Bilinearです。制作原本はAssetsの外に置き、Windowsビルドへ重複収録しません。

## 試遊操作
- 左ドラッグ：押し付けて擦る
- ホイール／＋／－：カーソル付近を中心に1～4倍ズーム
- 中ドラッグ：パン、Home：全体表示
- Q/E：面・辺・角、R／Shift+R：回転
- H：精密依頼の完成見本
- 1～9／Tab／道具トレイ：持ち替え、T：持ち物
- C：回収、G：玉、V：買取、Space：吹き払い、Z：救済、M：ミュート
- F4：開発用接触範囲表示（通常はOFF）

手紙の余分な「ま」を消しても、手書きの左右の文字が自動で詰まることはありません。余分な字の跡に余白が残る、実際の紙の修正として表示します。残る文字列は「しています」です。
