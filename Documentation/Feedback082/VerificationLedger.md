> 訂正：この旧報告はPASS見出し10行を重複集計していた。既存Editorは698ではなく688、前回全体は1,993ではなく1,983。今回の自動集計はTools/Feedback082/Adjustment/test_ledger.pyを参照。チェック削除ではない。

# 0.8.2 Verification ledger

チェックはassertion単位。過去の1,866という総計をそのまま複写せず、保存された結果のChecks行と今回の再実行を照合した。基盤作業前の履歴と一致する既存実集計は1,875。9件の差は旧報告の集計差で、新機能の追加件数ではない。

|Suite|Checks|
|---|---:|
|既存12 Editor: balance45/economy45/prototype14/feel16/role34/shop96/skill108/specialist132/tool39/work53/world84/zigzag32|698|
|ExternalArtwork Editor|119|
|Runtime 14 suites|768|
|ExternalArt Python Validator|145|
|Frozen ZIP read-only packaging validation|14|
|Foundation Editor|109|
|JobPipeline Python|22|
|既存実集計|1875|
|Feedback082 Editor additions|118|
|合計成功|1993|
|失敗|0|

Runtime内訳: role35, tool25, zigzag15, economy25, work61, skills27, shop36, balance14, specialist25, world79, external106, external01 60, legacy26, UX234。再実行した同一suiteは重複計上しない。

追加118は各依頼Reaction/Text、fallback/分類、猫表記、面辺角/回転/Zoom、状態テクスチャ設定、実Paper.Stroke境界、damage段階、100%SE latch/Mute/Retryを含む。テスト用の境界は画素中心で比較し、表示の線幅・antialiasを判定面積へ足さない。

再実行:

```text
Unity -batchmode -projectPath <project> -executeMethod Keshiya.Editor.Feedback082Build.Run -quit -logFile <log>
Tools/JobPipeline/run-regression.ps1 -Build Feedback082-Normal
```

個別のEditor再検証はFeedback082Build.Verify、ビルドのみはBuildOnly。実際の結果はローカルのTestResults-Feedback082、TestResults-Foundation、およびBuilds/Feedback082-Normal/TestResults*に保存（Git対象外）。既存ZIPを作るpackage.pyは実行しない。

Development通常画面確認: TEST_001/003/004/006。全8依頼進行はRuntime suite。UI自動入力は物理マウス試遊の代替ではない。視覚的な破損の印象・連続擦り操作・SE聴感は未保証。
