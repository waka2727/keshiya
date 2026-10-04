# Job Content Pipeline v2

JSON定義 → コード生成 → PNG/manifest → 検証 → 既存JobDefinition/JobArtworkData → Registry → Preview → 回帰テスト。

`python Tools/JobPipeline/pipeline.py generate --id DEV_PIPELINE_001`

`python Tools/JobPipeline/pipeline.py validate`

`python Tools/JobPipeline/test_pipeline.py`

生成はPillowによる日本語組版と線描画。必要ライブラリはrequirements.txt。Windowsの指定フォントを利用し、フォント名とファイルSHAをmanifestへ記録する。個人フォルダのパスは出力しない。生成時刻は入れず、同じ入力・フォント・ライブラリで同じmanifestにする。

既存8件はvisibility=existingで生成コマンドを拒否する。画像主体の漫画・ラフは従来の分離済み原本と生成コードを継続利用する。元絵を作り直したりAIへ文字を再生成させたりしない。

Editorの `1 Import Definitions` はScene/Prefab編集不要。新規依頼だけテクスチャ設定を適用し、既存8件は内容・描画設定を維持してmetadata/Text IDを追加する。日本語表に既存IDがある場合はその値を採用する。

`2 Validate All Jobs` は参照、ID、依頼人、文章、価格・難易度、各レイヤーの原画像/インポート解像度、alpha、グレースケールmask、空mask、重複、色空間、readability、mipmap、圧縮、wrap、実際のTexture参照を確認する。

保護なしの仕事では空ProtectMaskが正常。precision=trueで空ならエラー。既存8件の重なりは凍結対象のためmaxOverlapRatio=1.0で受け入れる。新規DEVは0.02。数値を下げて既存紙面を自動改変しない。Previewで重なり統計を確認し、新規依頼ごとに意図した許容値を作者と設定する。
