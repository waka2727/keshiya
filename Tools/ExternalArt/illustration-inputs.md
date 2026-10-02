# Saved illustration inputs

These three original PNGs are authored for this project using the imagegen skill. They are not references to existing manga or characters. The files, not a live image-generation service, are inputs to the deterministic layer generator.

## manga-ink-original.png
Original black-and-white five-panel shonen-style page. A young delivery runner in a small town: street establishing action, face close-up, hand fastening a satchel, full-body leap over a puddle, reaction panel. Finished ink, hair, clothes, buildings, perspective, panel borders, empty speech balloon and motion lines. No generated Japanese text. The generator uses the ink image as Protected and creates separate blue-gray construction marks. The reference production brief requested an original character and excluded imitation of existing manga.

## child-original.png
Flat A4 scan-style drawing by a seven-year-old: uneven pencil house, sun, stick family, cat, stars, spirals and meaningless marks. Pure white background, grayscale pencil, no text, watermark, desk or physical pencil. The saved raster is converted to an alpha layer; pale background noise below 12/255 is excluded.

## rough-original.png
Flat A4 art-school rejected composition study. Dense graphite 4B, seated human figure at a train platform, repeated anatomical attempts, station architecture, perspective guides, abandoned arm/head positions, hatch swatches and margin thumbnails. Unfinished work, not a polished illustration. No generated text, no existing-character reference.

The original files remain unchanged. Processing only derives same-canvas transparent layers and masks. Text-sensitive documents are composed with native Japanese fonts by generate.py, never by image generation.
