# 消し屋 / Keshiya

Unity PCゲーム「消し屋」のソースです。

## External Test 01

- 固定タグ: `external-test-01`
- Unity: `6000.3.23f1`（詳細は `ProjectSettings/ProjectVersion.txt`）
- Unity Hubからこのフォルダをプロジェクトとして開いてください。
- Windows試遊版はGitHub Release「Keshiya External Test 01」の `Keshiya-ExternalTest01.zip` を展開して起動してください。
- ZIPおよびUnity自動生成ファイルはGit管理対象外です。

mainにはExternal Test 01を基準にしたDevelopment Foundation v2を含みます。試遊版そのもののソースは`external-test-01`タグで固定しています。

## Development Foundation v2

- Job Content Pipeline、Validator、Editor限定Job Preview
- Text ID管理、Save v2・移行・バックアップ・設定分離
- Development限定の性能計測
- Story Bible、未決定ルール、Runtime未登録の依頼案
- 検証: 既存1,735件＋追加131件＝1,866件成功、0件失敗
- Windows通常版・Development版ビルド成功

[開発資料](Documentation/Foundation/README.md) / [実装報告](Documentation/Foundation/ImplementationReport.md) / [新しい依頼の追加手順](Documentation/Foundation/HowToAddNewJob.md)

既存8依頼の内容・操作・バランスは維持しています。Save v2は開発用の明示保存で、通常版の初期状態は変更していません。

操作: Wheelで上下移動、Ctrl + Wheelでズーム。

開発中の試遊版です。UI・グラフィック・バランス・内容は今後変更されます。
