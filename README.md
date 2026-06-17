# TypeAestetic ⌨️✨

A premium keyboard overlay for Windows that turns typing into a visual and auditory experience. Shows a translucent on-screen keyboard with animated key presses, plays mechanical keyboard sounds with pitch variation, and tracks your typing stats — all rendered as a beautiful glassmorphism overlay.

![.NET 10](https://img.shields.io/badge/.NET-10.0-512BD4?style=flat-square) ![WPF](https://img.shields.io/badge/WPF-Windows-0078D6?style=flat-square) ![License](https://img.shields.io/badge/License-GPLv3-blue?style=flat-square)

## Features

- **🎹 Visual Keyboard Overlay** — Frosted glass keyboard with gradient glow effects, particle sparkles, and spring-bounce animations on every keypress
- **🔊 Mechanical Sound Engine** — Pre-cached audio with round-robin output pooling (12 channels), subtle pitch variation for realism, and instant playback
- **📊 Live Typing Stats** — Real-time WPM counter, total keystroke tracker, and typing streak indicator with 🔥 emoji
- **⚙️ Settings Window** — Dark-themed settings with volume/opacity sliders, sound pack selector, overlay position, and feature toggles
- **🔔 System Tray** — Tray icon with context menu for quick access to all controls, sound pack switching, and clean exit
- **💾 Persistent Settings** — All preferences saved to JSON and restored on launch
- **🎨 Sound Pack System** — Drop custom sound packs into `assets/sounds/` and switch between them live

## Building

```bash
# Run in development
dotnet run --project src/TypeAestetic.csproj

# Build release
dotnet publish src/TypeAestetic.csproj -c Release -r win-x64 --self-contained -o build/

# Or use Make
make run    # dev mode
make        # release build
make clean  # clean artifacts
```

## Sound Pack Format

Sound packs live in `assets/sounds/<pack-name>/` and contain:

```
my-pack/
├── config.json
├── clicks/
│   └── *.wav
└── release/
    └── *.wav
```

### config.json

```json
{
  "name": "My Custom Pack",
  "author": "your-name",
  "settings": {
    "volume": 0.8,
    "pitch_variation": 0.05
  },
  "maps": {
    "SPACE":   { "click": "clicks/space_click.wav",   "release": "release/space_release.wav" },
    "RETURN":  { "click": "clicks/enter_click.wav",   "release": "release/enter_release.wav" },
    "SHIFT":   { "click": "clicks/shift_click.wav",   "release": "release/shift_release.wav" },
    "BACK":    { "click": "clicks/back_click.wav",    "release": "release/back_release.wav" },
    "DEFAULT": { "click": "clicks/generic_click.wav", "release": "release/generic_release.wav" }
  }
}
```

- `volume` — Base volume (0.0–1.0)
- `pitch_variation` — Random pitch shift range (±). `0.05` = ±5% for subtle realism
- `maps` — Key-to-sound mappings. `DEFAULT` is used as fallback for any unmapped key

## Controls

| Action | How |
|--------|-----|
| **Toggle overlay** | Double-click tray icon |
| **Open settings** | Right-click tray → Settings |
| **Switch sound pack** | Right-click tray → Sound Pack |
| **Exit** | Right-click tray → Exit |

## Requirements

- Windows 10/11
- [.NET 10 Runtime](https://dotnet.microsoft.com/download)

## License

[GNU General Public License v3.0](LICENSE)