# Job Preview / Inspector

Editorメニュー `Keshiya/Content Pipeline/3 Job Preview`。

Job ID、依頼名、依頼人、紙名を部分一致で検索。既存の幾何図形依頼も一覧には見えるが、レイヤーのない依頼は説明表示のみ。

Composite / Paper / Protected / Erasable / EraseMask / ProtectMask / CompletePreviewを切替。Eraseはcyan、ProtectはmagentaのOverlay。対象pixel数、占有率、重複pixel数を表示する。

0/50/95/100%は、EraseMaskの重みを走査して部分的に除去した確認画像。全画像を均一に薄くするだけではない。ただし実際の消しゴム操作・摩耗・スコアをシミュレートする機能ではなく、アートとマスクの検証用。別途Paper.Strokeの自動テストを行う。

Previewは複製Textureを読み、元PNGやゲーム中Paperを変更しない。切替時と閉じたときにTextureを破棄する。EditorフォルダのクラスなのでPlayerビルドへ入らない。
