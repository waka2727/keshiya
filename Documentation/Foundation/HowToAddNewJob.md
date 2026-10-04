# 9件目以降の追加手順

1. `Tools/JobPipeline/Source/DEV_PIPELINE_001.json`を新IDのJSONへコピー。既存TEST_001〜008を編集しない。
2. id/title/clientId/clientName/clientLetter/workInstruction/completionMessage、紙・筆記具アセット参照、報酬、難易度、消去率を記入。本文は画像生成へ任せず日本語文字列を使う。
3. 制作中はvisibility=development、runtimeAsset/artworkFolderを`Assets/Keshiya/Jobs/Editor/Generated/<ID>`配下へ指定。canvas/runtimeサイズ、font、text/lines配列を調整する。
4. `python Tools/JobPipeline/pipeline.py generate --id <ID>`。6レイヤーとInitialPreview、manifestが同じ座標で生成される。
5. `python Tools/JobPipeline/pipeline.py validate`、Unityメニュー`Keshiya/Content Pipeline/1 Import Definitions`、`2 Validate All Jobs`を実行。
6. `3 Job Preview`で全レイヤー・Overlay・50/95/100%・完成状態を確認。DEVの実操作テストはFoundationChecksの方式を使用する。
7. Pythonテスト、Editor FoundationChecks、既存回帰テストを実行する。

公開承認後だけvisibility=runtimeに変更し、runtimeAsset/artworkFolderを`Assets/Keshiya/Jobs/Runtime/<ID>`へ設定して生成・登録し直す。Editorの古いDEV原本はRuntime参照に入れない。Registryが自動結合するのでSceneを手作業しない。

現在の通常画面はET01専用8件フィルタを維持している。承認済み新規依頼を同画面へ出す場合だけincludeInExternalTest=trueを明示する。既存の8件限定回帰件数もそのリリース時に意図的に更新する。今回はDEVを追加しただけで、表示8件に変更はない。Editorの「全依頼」表示ではRegistryの承認済み依頼を確認できる。

文章の初回登録後はTexts/ja.jsonが表示文字列の正本。JSON内の初期文章は移行元・基準文として残る。既存8件は完全一致テストで保護される。追加依頼の修文時には正本と仕様の基準文を同じ変更としてレビューする。
