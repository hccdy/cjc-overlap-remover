# Conjac Jelly Charlieyan's Overlap Remover
You have a large MIDI, and you want to make a video for it as fast as possible, but it's too large for the software (like Zenith, UMP, etc.) to render it. What can you do? Now you have 4 choices:
1. Directly render. This is not a good choice, because it will take you lots of time rendering the video.
2. Render after `SAFOR`. This is a good choice. If the MIDI is not so large, you can do it to keep most of the note art, but it uses RB tree and will take lots of time and RAM.
3. Render after `AOR`. This is a better choice. It can render faster with larger MIDIs, but if you need to render an extremely large MIDI, you can't use it because it's a waste of RAM, even 16GB can't hold it.
4. Render after **`CJCOR`**. This is the best choice. Although it will ruin some of the art, but it's really fast on extremely large MIDI, and you will not care the ruin of the note art if the MIDI is extremely ruined by note art. It will read the file only twice (or three times if you use audio render). Also, if you need to open it in ssw, you can also make a **`CJCOR`** on it and then merge all the tracks.
