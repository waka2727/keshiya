# Architecture Overview

既存のPrototypeGameがセッション、WorkSessionが依頼遷移、Paper/Drawing/ProtectedDrawingが消去判定を担当する。EraserDefinitionと各個体EraserState、PlayerProgress、CrumbEconomyは継続利用する。接触・報酬・EXP等の計算式を基盤に移さない。

新しい依頼は既存JobDefinitionへインポートする。JobContentMetadataは依頼人ID、再登場、登場段階、タグ、精密難易度・紙リスク等、特殊ルール、ストーリーフラグ、解放条件、後続依頼ID、EXP設定参照、開発メモ、Text IDを保持する。これらは今は記述情報であり、旧8依頼の解放やEXPを変更しない。

JobRegistryはSceneの既存依頼順を保って承認済み追加依頼を結合する。重複IDを二重表示しない。DEV依頼を除外する。EditorのValidatorは登録前に重複をエラーにする。

データ境界:

- `Tools/JobPipeline/Source`: 新方式のJSON定義。既存8件の読み取り用定義を含む。
- `Tools/ExternalArt`: 旧8件の凍結した原本と生成コード。
- `Tools/JobPipeline/GeneratedSource`: v2高解像度生成原本。
- `Assets/Keshiya/Jobs/Editor/Generated`: DEV専用アセット。Build非搭載。
- `Assets/Keshiya/Jobs/Runtime`: 今後承認される依頼の配置先。
- `Assets/Keshiya/Resources/JobRegistry.asset`: Runtime参照。
- `Assets/Keshiya/Resources/Texts/ja.json`: 日本語Text ID表。

SaveStoreは既存状態をDTOへ変換して保存。SceneやScriptableObject自体を保存しない。開発中の明示保存に限定し、ET01の毎起動初期状態は維持する。
