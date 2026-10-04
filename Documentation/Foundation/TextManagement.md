# Text Management

日本語正本は`Assets/Keshiya/Resources/Texts/ja.json`。翻訳は作成しない。

- JOB_TEST_003_TITLE / CLIENT_LETTER / INSTRUCTION / COMPLETION
- ITEM_<asset name>_<serialized property path>
- SKILL_<catalog name>_<serialized property path>
- TUTORIAL_BASIC_01〜07

JobContentPipelineが既存JobDefinition/EraserDefinition/SkillCatalogの文字列フィールドへ日本語表を適用する。ゲームUIは従来フィールドを読むので描画構成が変わらない。TutorialHintsはText IDから従来の7文章を読む。

TextCatalog.Requireは未定義IDを例外にし、Parseは重複IDを拒否する。黙って別言語や空文字へ置換しない。既存8依頼はTools/JobPipeline/Sourceの基準文との一致、チュートリアルはtutorial-baseline.jsonとの一致で回帰保護する。改行、全角/半角、TEST_003の誤字を正規化しない。

移行範囲は依頼の主要4文章、商品・スキルの文字列、基本チュートリアル。UIのボタン名や数値を含む動的な文章は既存コードに残す。今後は同じText ID表へ段階移行できる。今回、全UIの置換による表示リスクを持ち込まない。新しい固定文はText IDを作り、Requireで参照する。
