# Development Foundation v2 実装報告

External Test 01のFB待ち期間に、依頼制作・文章・保存・開発確認の基盤を追加した。作業対象は別プロジェクトの `Keshiya-Development`。通常プレイへ新しい依頼やストーリーを追加していない。

## 基準版の保存

- GitHubのPrivate repository `waka2727/keshiya`、tag `external-test-01`を維持。
- 基準commit: `09c948392d65d80388d8b45a7f5c5330f9f134e9`。
- Release: `Keshiya External Test 01`。
- 配布ZIPのSHA-256: `9094dea45f531186e9e8be242a7dbf78a7ff37afb90c55ffd5e532ac8539a922`。
- ローカルにも `outputs/ExternalTest01-Frozen` へソースZIP、配布ZIP、688ファイルのhash manifestを保存。
- 今回の開発変更はGitHubへ公開・pushしていない。既存Releaseや配布ZIPを再生成していない。

## Job Content Pipeline v2

既存 `JobDefinition` と `JobArtworkData` を引き続き使用する。並行するゲーム実行系を作らず、JSON原本から既存ScriptableObjectへ登録するEditor処理を追加した。

`JobContentMetadata` はClient ID、再登場、登場段階、精密難易度、紙リスク、消去量、保護密度、タグ、特殊ルール、EXP設定参照、Story flags、解放条件、後続Job ID、開発メモ、Text IDを保持する。従来の報酬、紙、筆記具、消去率、本文、アート参照はJobDefinitionの既存フィールドを利用する。未実装の世界設定・解放ルールを有効にするものではない。

`JobRegistry` がSceneの依頼順を保って承認済み追加Jobを結合する。DEV依頼を除外し、同じIDの二重表示を防ぐ。現状のScene内28依頼と、通常画面のExternal Test用8件フィルタは維持している。

### 新規依頼を追加する手順

1. `Tools/JobPipeline/Source/DEV_PIPELINE_001.json`を新IDへ複製する。
2. 依頼文、完成文、作業情報、紙・筆記具、報酬、レイアウトを記入する。
3. 制作中は`visibility=development`としてEditor配下を出力先にする。
4. `python Tools/JobPipeline/pipeline.py generate --id <ID>`で生成する。
5. 同スクリプトの`validate`、Unityの`Keshiya/Content Pipeline/1 Import Definitions`、`2 Validate All Jobs`を実行する。
6. `3 Job Preview`でレイヤー・マスク・完成状態を確認する。
7. Python/Editor/Runtime回帰テストを実行する。

作者承認後にのみ`visibility=runtime`とRuntime出力先へ変更する。External Test一覧に追加する場合も`includeInExternalTest=true`を明示する。既存8依頼を自動で増減しない。

## アセット生成

- 新方式はPython/Pillowによる日本語組版・線描画。FigmaやCLIP STUDIOによる人手作業は不要。
- JSONレイアウトからPaper / Protected / Erasable / EraseMask / ProtectMask / CompletePreviewとInitialPreviewを生成。
- DEV原本2480×3508、実使用1240×1754。解像度、文章、配置、フォントは原本で変更可能。
- 各レイヤーは同一キャンバス。判定はマスクの明度、表示は透明レイヤーとして分離する。
- ManifestにはJob ID、source version、generator version、寸法、原本・フォント・PNGのhash、座標規約を保存。個人パスを入れない。
- 同じ原本・フォント・依存バージョンで再生成した結果の一致をテストする。
- 既存8件は`Tools/ExternalArt`の元の制作方式を維持。TEST_002を別出力先で再生成し、7画像が元と一致することを確認した。
- `visibility=existing`の生成は拒否し、凍結アートの事故上書きを防ぐ。

## Validator / Preview

ValidatorはJob ID重複、Client ID、必須本文、報酬、Runtime参照、レイヤー欠落、寸法、Alpha、mask grayscale、空のEraseMask、精密依頼の空ProtectMask、重複率、Import設定を検査する。保護対象がない依頼の空ProtectMaskは正常として扱う。重複率の許容量は依頼ごとに指定し、既存アートを無断変更しない。

Job PreviewはEditor限定。ID・依頼名・依頼人・紙で検索し、Compositeと6レイヤー、色付きErase/Protect overlay、pixel数と率、重複数を確認できる。0/50/95/100%状態はマスク重量に沿う走査表示で、実際のプレイヤーストロークを再現するものではない。

Editor実画面でもDEV合成画像とシアン/マゼンタのマスク表示を操作して確認した。初期ウインドウが細すぎないよう最小サイズも設定した。

DEV_PIPELINE_001では生成→登録→検証→全レイヤープレビュー→実際のPaper.Strokeによる95%消去と保護本文維持まで自動確認した。Editorフォルダに隔離し、両Buildのpacked assetsへ含まれないことも確認する。

## Text管理

日本語正本は`Assets/Keshiya/Resources/Texts/ja.json`。既存8件の主要4文章、商品・スキルの文字列、基本チュートリアル7文をID管理する。Editorインポートで既存SOフィールドへ適用し、現在のUIを維持する。

8依頼の文章を原本と完全一致で検査する。TEST_003の誤字、全角英字、改行も保持する。Missing IDと重複IDは検出する。

既存UIのボタン名や数値を含む動的文はまだコード内に残る。今後同じ表へ段階的に移行する。今回は全UI置換や英訳をしていない。

## Save System v2

調査の結果、基準版は起動中の状態だけを保持し、進行のディスク保存はなかった。このため「既存セーブファイルを読み込んだ」とはしていない。旧セッション状態の明示的なversion1 export DTOからversion2へ移行する仕組みとテストを追加した。

- SaveVersion=2。
- 財布、EXP/SP/11スキル、個体残量・角・特殊状態、在庫、購入・収集統計、依頼履歴、報酬/EXP二重加算防止、回収済みカス・玉・素材台帳を保存。
- ScriptableObject自体を保存せず、定義IDを利用。
- UserSettingsは別ファイル。音量、解像度、全画面、接触ガイドを保持。
- SHA-256 envelope、tmp書込、flush、読込検証、File.Replaceによる置換、1世代backup。
- primary破損時はbackupへfallback。両方破損は初期化要求を返し、無断上書きしない。
- 復元時も既存Wallet/ToolInventory参照を維持し、消しカス売却が別の財布へ入らないようにした。

現段階ではEditor Play Modeの依頼一覧で、`Keshiya/Development/Save and Performance`から明示保存・確認付き復元を行う。保存先は`Application.persistentDataPath/DevelopmentV2`。通常版の毎起動初期状態・UIは変更しない。作業途中の紙面保存は対象外で、未処理の紙上カスなどが残る場合は保存を拒否する。

## シナリオ資料

`Documentation/Story/StoryBible.md`へ作者が示した主人公、郵送依頼の店、約2年間、テーマ、正式な事実と記憶・感情の区別を整理した。

`OpenWorldRules.md`には原本/コピー、デジタル、写真、複数原本、契約、戸籍、婚姻届、再記入、責任、拒否、仕組みの認知の11論点をOPENとして記録した。

`JobRoadmap-DRAFT.md`には序盤・中盤・終盤の20案を作成した。何を消す/残すか、理由、現実への影響、それでも残るもの、主体験、再登場可能性を記載した。CanonでもRuntimeコンテンツでもない。

## 性能計測

EditorまたはDevelopment Buildでのみ、job-paper-load、mask-read、job-switch、Textureメモリ、managed heap差を計測する。キュー上限100。Development版は`--measure-performance`で有効化し、終了時に平均FPS、最遅フレーム相当FPS、managed bytes、Gen0 GC回数、操作別結果をJSON保存する。

実行して終了時保存を確認済み。保存先は`persistentDataPath/DevelopmentV2/Measurements`。個人情報や絶対パスは含めない。テスト中のFPSは性能基準値として採用しない。

managed heap差は正確なGC allocationではない。Texture非同期ロード時間やRenderTexture専用使用量は未分離。未計測の数値を理由に解像度やゲーム処理を変更していない。

## 変更した主要ファイル

新規Runtime基盤:

- `Scripts/JobContentMetadata.cs`, `JobRegistry.cs`, `TextCatalog.cs`
- `Scripts/ProgressSnapshot.cs`, `SaveStore.cs`
- `Scripts/DevelopmentMetrics.cs`, `DevelopmentPerformanceRecorder.cs`

既存コードの接続:

- `JobDefinition.cs`: inert metadata
- `PrototypeGame.cs`: Registry結合と計測区間
- `JobArtworkData.cs`: 読込計測
- `PlayerProgress.cs`, `WorkSession.cs`, `CrumbEconomy.cs`: snapshot拡張用partial
- `CrumbPiece.cs`: 保存用serializationと復元
- `TutorialHints.cs`: 元の7文をText IDへ移行、遅延初期化
- `External-TEST_001～008.asset`: metadataのみ

Editor:

- `JobContentPipeline.cs`, `JobPreviewWindow.cs`, `DevelopmentToolsWindow.cs`
- `FoundationChecks.cs`, `FoundationBuild.cs`

原本・生成・検証:

- `Tools/JobPipeline/pipeline.py`, `test_pipeline.py`, `audit_baseline.py`, `run-regression.ps1`, `requirements.txt`
- `Tools/JobPipeline/Source/*.json`, `tutorial-baseline.json`, `GeneratedSource/`
- `Resources/Texts/ja.json`, `Resources/JobRegistry.asset`
- `Jobs/Editor/Generated/DEV_PIPELINE_001*`と各`.meta`

Documentation:

- `Foundation/README.md`, `ArchitectureOverview.md`, `JobContentPipeline.md`
- `HowToAddNewJob.md`, `JobArtworkSpecification.md`, `TextManagement.md`
- `SaveSystem.md`, `SaveMigration.md`, `JobPreview.md`, `PerformanceMeasurement.md`
- `Story/StoryBible.md`, `OpenWorldRules.md`, `JobRoadmap-DRAFT.md`
- 本報告と`Verification.json`

## 検証記録

既存1,735件＋追加131件（Editor 109、Python 22）＝1,866件成功、0件失敗。通常Windows版とDevelopment版のビルドに成功した。詳細は同フォルダの`Verification.json`を参照。既存回帰、追加Editor、追加Pythonを分けて集計する。同じチェックの再実行やDevelopment版の重複実行を件数へ加算しない。

通常版専用External01チェックをDevelopment版でも診断実行した際は、「通常ビルドではDebug.isDebugBuildがfalse」という1件が対象違いで失敗した。通常版では成功しており、テスト条件を弱めていない。Development版には互換性のあるsmoke 26件を適用して成功した。隠したウインドウではスクリーンショット取得警告が出たため、Developmentログを画像表示確認の根拠には使わない。

冷起動確認で、TextCatalogをMonoBehaviourの初期化経路から早期読込するとUnityが拒否する不具合を検出した。今回追加したTutorialHintsの読込を遅延化して修正し、専用テストと冷起動で確認した。基準版のゲームデザイン変更ではない。

## 意図的に変更しなかったこと

TEST_001～008の本文・アート・配置・難易度、面辺角の判定、回転、Wheelスクロール/Ctrl+Wheelズーム、紙/Protectダメージ、消しゴム性能と摩耗、スキル倍率、EXP/SP、価格・報酬、消しカス生成・価格、チュートリアル文、現在の依頼UI、解放テンポ。

凍結688ファイル中672ファイルは完全一致。8依頼SOの差分はmetadataに限定し、残る8コードファイルは上記の接続だけをレビューした。主要13メソッドはコンパイル後ILも基準版と一致している。

## 次に作者が判断すること

- OpenWorldRulesの各未決定事項を、どの順番で確定するか。
- 常連候補と再登場のつながり、20件のDRAFTから採用する依頼。
- External Testの実際のFBを受けてから、どの操作感・バランスを見直すか。

技術基盤を使うために、これらを今すぐ決める必要はない。DRAFTを作者承認前にゲームへ登録しない。
