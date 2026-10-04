# Performance Measurement

最適化前の計測基盤。紙解像度、ストローク計算、ズーム・スクロール係数は変更していない。

Editorでは `Keshiya/Development/Save and Performance` のCapture performanceをON。通常はOFF。最大100サンプルのリング状キューでメモリを制限する。

- job-paper-load: Paper/Drawing/保護判定の作成時間
- mask-read: Texture→float mask読み出し時間
- job-switch: 従来Restart処理全体の経過時間
- texture: Unity ProfilerのTextureメモリ。状態Textureも含む
- managed Δ: 前後の生存managed heap差。正確なGC allocationではない

Development Buildは`--measure-performance`付きで起動すると計測する。終了時に`persistentDataPath/DevelopmentV2/Measurements`へJSONを保存。平均FPS、最遅フレーム相当FPS、managedメモリ、Gen0 GC回数、上記操作を記録。個人パスや識別子は記録しない。

計測コード本体はUNITY_EDITORまたはDEVELOPMENT_BUILDだけ。通常Buildはno-opで記録UIも出ない。ReleaseのF2による既存FPS表示は変更していない。

Textureの非同期ロード時間とRenderTexture専用使用量は、現在の描画方式に独立した経路がないため未分離。GPUメモリの正確な合計やフレーム当たりallocationを推測しない。必要になった時にUnity Profilerと照合する。
