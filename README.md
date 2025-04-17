# Rimworld Mod Translator

[![License](https://img.shields.io/github/license/TokcDK/RimworldModTranslator)](LICENSE) [![Build Status](https://img.shields.io/github/actions/workflow/status/TokcDK/RimworldModTranslator/ci.yml)](https://github.com/TokcDK/RimworldModTranslator/actions) [![GitHub issues](https://img.shields.io/github/issues/TokcDK/RimworldModTranslator)](https://github.com/TokcDK/RimworldModTranslator/issues) [![Stars](https://img.shields.io/github/stars/TokcDK/RimworldModTranslator)](https://github.com/TokcDK/RimworldModTranslator/stargazers)

---

[Русский](README_RU.md) | [More languages](#more-languages)

---

## English

### 🚀 Introduction
Rimworld Mod Translator is a lightweight desktop utility designed to simplify the localization of Rimworld mods. Choose your mods folder, pick the mod you want to translate, and edit existing languages or add new ones through an intuitive translation editor window.

### ✨ Features
- **Easy setup:** Select the mods directory.
- **Mod list:** Browse and select from all available mods.
- **Built‑in editor:** Edit existing translations or create new language files.
- **Autotranslate:** Using exist translations to translate empty fields.
- **Autofill About.xml:** Fill target mod data to autofill it.
- **Open source:** GPLv3 licensed, contributions welcome.

### 🛠️ Installation
1. **Download** the latest release from the [Releases](https://github.com/TokcDK/RimworldModTranslator/releases/latest)).
2. **Extract** the archive to a folder of your choice.
3. **Run** the executable (`RimworldModTranslator.exe`).

Alternatively, **build from source**:

```bash
git clone https://github.com/TokcDK/RimworldModTranslator.git
cd rimworld-mod-translator
dotnet build --configuration Release
# The binary will be in bin/Release/net8.0
```

### 🎮 Usage
1. Launch the application.
2. Select your Rimworld `Mods` directory.
3. Optional select the config and game path.
4. Fill Target mod data to automatically fill it in new mod dir
5. Select a mod from the list.
6. Edit existing languages or click **Add Language** to start a new translation.
7. Save your changes with **Save strings** button

### 🛆 Topics
```
rimworld-mod-translator rimworld translator localization mod-tools dotnet csharp
```

### 🤝 Contributing
Contributions are welcomed! Please:
1. Fork the repository.
2. Create a feature branch (`git checkout -b feature/YourFeature`).
3. Commit your changes (`git commit -m 'Add feature'`).
4. Push to the branch (`git push origin feature/YourFeature`).
5. Open a Pull Request.

---

## More languages
Contributions for other languages are welcome. Just copy exist rmt.po and edit it with POEdit or other po-file editor.
