# Save Migration

バージョン2が初めてのファイル保存形式。ET01に実在しない旧セーブファイルを読んだとは扱わない。

`LegacySessionExport`（SaveVersion=1）はET01のセッション状態を明示的にエクスポートするための互換DTO。progress/economyを保持し、Decodeでv2へ変換して空のstoryFlags/unlockedJobIdsを追加する。PlayerProgress/個体データ自体のJSON互換性を利用する。

既存の保存がない場合はLoadがnew-gameを返す。version不明・将来version・必須欠落・不正値は拒否する。破損回復とスキーマ移行は別処理。

次回スキーマ変更時:

1. CurrentVersionとDTOを更新する前に現行のfixtureを保存。
2. v2→v3の明示変換を追加。既存v1入力も変換チェーンを通す。
3. 財布、重複報酬防止ID、個体残量、角、特殊状態、消しカス在庫の往復テストを維持。
4. 未知のIDを削除する移行は作者判断を得る。元ファイルをbackupとして保持。
5. 通常プレイの自動保存導入は別リリースでUIと初期化仕様を確認する。
