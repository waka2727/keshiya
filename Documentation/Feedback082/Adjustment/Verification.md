# 0.8.2追加調整 検証結果

検証日: 2026-10-06。基準: `3cf6182`。ブランチ: `fb-0.8.2`。

## 自動チェック

|区分|件数|失敗|
|---|---:|---:|
|legacy-editor|688|0|
|artwork-editor|119|0|
|artwork-python|145|0|
|foundation-editor|109|0|
|feedback082-editor|118|0|
|new-editor|121|0|
|new-artwork|9|0|
|frozen-archive|14|0|
|runtime|768|0|
|pipeline-python|22|0|
|合計|2113|0|

**既存実数1,983件＋追加130件（Editor 121＋画像検証9）＝2,113件成功。**

前回報告の1,993件は、旧Editor結果に含まれる「PASS ○件」という集計見出し10行を重複計上していた。チェック削除による減少ではない。今回から各レポートの宣言件数を集計する。追加チェックと再実行回数も重複計上しない。

追加検証: 全28依頼の旧値に対する半額・結果・一度だけの入金、他16道具アセットのハッシュ一致、無料受取・複数個体・入手時損傷・摩耗・保存・使い切り・負価格拒否・所持金0での受取・白紙からの売却物生成なし、Lv0で無料片を交換しながら95%完了、漫画手元マスクと保護線維持。

既存テストでは新仕様と矛盾する旧報酬、格安品は必ず有料／長尺最強という期待値を更新。RuntimeToolChecks内の仮依頼は未登録の検証用データなので、その依頼自身の設定額を参照する。通常28件の基本報酬とは混同しない。

RuntimeRoleChecksはGUIの最初の描画を待ってからアイコンを検査するようにし、初期化不成立時も例外で停止し続けず失敗を記録して終了する。非表示ウィンドウではGUIが描画されないため、GUI・スクリーンショット検証は `-VisibleForVisualChecks` で画面表示して実行した。これはテストハーネスの対策で、通常操作の変更ではない。

## Windowsビルド

|種別|出力|結果|
|---|---|---|
|Normal|`Builds/Feedback082-Adjustment-Normal/Keshiya.exe`|PASS Normal; bytes=490411579|
|Development|`Builds/Feedback082-Adjustment-Development/Keshiya.exe`|PASS Development; bytes=546722245|

Unity 6000.3.23f1。External Test 01のビルド／ZIPとは別出力。通常版に開発用DEVアセットを混入させないビルド検査も維持。

## Runtime回帰

通常Windows実行ファイルで14シナリオすべて終了コード0。TEST_001～008では実際のPaper.Stroke処理を使い、95%到達、紙の安全、精密依頼のProtect判定、結果・入金、遅さによる基本報酬減額なし、再挑戦によるアート復元を確認。TEST_003の手元はEraseMaskが空、ProtectMaskの完成線は残ることを別途検査。

ショップ画面のスクリーンショットで0円・「消しゴム片を1個受け取る / 無料」・説明・残量35%の受取通知を確認。漫画比較画像とOverlayは `preview.py` で生成し目視確認。

自動入力は物理マウスの操作感を保証しない。手元周辺のストレス軽減、残る精密量、無料片の交換頻度、半額報酬の実感は次回人間試遊で確認する。

## 固定版の保全

- `external-test-01` タグオブジェクト: `3a496f0dd02ab168af3969aed196af61f9de5ed7`
- 指すコミット: `09c948392d65d80388d8b45a7f5c5330f9f134e9`
- 保存ZIP SHA256: `9094dea45f531186e9e8be242a7dbf78a7ff37afb90c55ffd5e532ac8539a922`
- Releaseの更新・タグの付け替え・ZIP再生成・GitHubへのpushは行っていない。

## 再現手順

1. Unity batchmodeで `Keshiya.Editor.Feedback082AdjustmentChecks.Run` を実行（Import・Editor回帰・両ビルド）。
2. `Tools/ExternalArt/validate.py` と `Tools/JobPipeline/test_pipeline.py` を実行。
3. `Tools/JobPipeline/run-regression.ps1 -Build Feedback082-Adjustment-Normal -VisibleForVisualChecks` を実行。既存結果を使った再開時だけ `-From` を指定する。
4. `Tools/Feedback082/Adjustment/verify_frozen.py` で保存ZIPを読み取り専用検証。
5. `Tools/Feedback082/Adjustment/test_ledger.py` で宣言件数を集計。

生ログ・テスト用スクリーンショットは無視対象のTestResults／Builds内。比較画像・仕様・検証台帳のみDocumentationへ保存。
