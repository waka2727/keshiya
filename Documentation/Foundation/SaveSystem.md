# Save System v2

External Test 01はディスクへの進行セーブを持たず、PlayerProgressを起動中だけ保持していた。今回も通常プレイヤーの初期状態・UI・自動保存挙動は変更しない。

Editor Play Modeで依頼一覧へ戻ってから`Keshiya/Development/Save and Performance`を開く。Save progress/Load progressでDevelopment専用スロットを使用できる。作業中の紙面途中セーブは対象外。紙上や救済待ちのカスが残る場合は保存を拒否し、黙って破棄しない。

保存先: `Application.persistentDataPath/DevelopmentV2/`。ゲーム画面に絶対パスを表示しない。

- progress.json: version2、PlayerProgressのwallet/tools/個体特殊状態/EXP/SP/技能/統計、WorkSnapshotの履歴、重複報酬・EXP防止ID、EconomySnapshotの長尺在庫・玉・素材台帳・連番、将来用storyFlags/unlockedJobIds。
- settings.json: SettingsVersion1、音量、解像度、fullscreen、接触ガイド。
- 各`.bak`: 直前の正常世代。

JSON payloadとSHA-256をenvelopeに格納する。暗号化ではなく破損検知。`.tmp`へ書き込み、Flush(true)、再読込・検証後にFile.Replaceで置換。正常な旧primaryをbackupへ残す。primaryが壊れている場合、正常backupを上書きせず保存を拒否する。

Loadはprimary→backup順。両方破損ならRequiresReset/statusを返し、勝手に初期化・上書きしない。Editor読込時も確認後に復旧する。設定は独立して読込・保存される。進行初期化は設定を消す必須動作にしない。

PlayerProgress.ImportSaveはWalletとToolInventoryのインスタンスを維持し、CrumbEconomyやUIが参照する財布が切断されないよう復元する。ScriptableObject参照は保存せず、定義IDを持つ。在庫IDが将来削除された場合の代替品判断は製品移行時の決定事項で、現時点では定義を削除しない運用。
