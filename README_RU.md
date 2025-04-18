# Rimworld Mod Translator

[![Release](https://img.shields.io/github/v/release/TokcDK/RimworldModTranslator
)](LICENSE) [![Build Status](https://img.shields.io/github/actions/workflow/status/TokcDK/RimworldModTranslator/ci.yml)](https://github.com/TokcDK/RimworldModTranslator/actions) [![GitHub issues](https://img.shields.io/github/issues/TokcDK/RimworldModTranslator)](https://github.com/TokcDK/RimworldModTranslator/issues) [![Stars](https://img.shields.io/github/stars/TokcDK/RimworldModTranslator)](https://github.com/TokcDK/RimworldModTranslator/stargazers)

---

[English](README.md) | [Добавить перевод](#more-languages)

---

## Русский

### 🚀 Описание
Rimworld Mod Translator — это простая утилита для удобного перевода модов Rimworld. Выбираете папку с модами, если нужно папку с конфигом и игрой, затем выбираете нужный мод и в открывшемся окне редактора переводите существующие языки или добавляете новые.

### ✨ Возможности
- **Лёгкая настройка:** Просто выбрать папку с модами.
- **Список модов:** Удобный выбор из всех установленных модов.
- **Встроенный редактор:** Редактируйте существующие переводы или создавайте новые.
- **Автоперевод:** Автозагрузка перевода из существующих модов.
- **Автозаполнение About.xml:** Укажите данные целевого мода для автозаполнения их в новом моде.
- **Открытость:** Лицензия GPLv3, рады вашим вкладом.

### 🛠️ Установка
1. **Скачайте** последний релиз на странице [Releases](https://github.com/TokcDK/RimworldModTranslator/releases/latest).
2. **Распакуйте** архив в удобное место.
3. **Запустите** исполняемый файл (`RimworldModTranslator.exe`).

Или **соберите из исходников**:

```bash
git clone https://github.com/TokcDK/RimworldModTranslator.git
cd RimworldModTranslator
dotnet build --configuration Release
# Бинарник появится в bin/Release/net8.0
```

### 🎮 Использование
1. Запустите приложение.
2. Выберите папку с модами Rimworld.
3. Опционально можно указать папку с конфигурацией и папку с игрой.
4. Выберите мод из списка.
5. Нажмите **Загрузить строки**
6. Нажмите **DB** для автопоиска перевода.
7. Добавьте ноый язык или отредактируйте существующие пустые ячейки.
8. Нажмите **Сохранить строки** для сохранения строк в новый мод.

### 🛆 Темы
```
RimworldModTranslator rimworld перевод локализация инструменты-модов dotnet csharp
```

### 🤝 Участие в проекте
Ваши вклады важны! Для участия:
1. Форкните репозиторий.
2. Создайте ветку (`git checkout -b feature/YourFeature`).
3. Зафиксируйте изменения (`git commit -m 'Добавлена фича'`).
4. Запушьте ветку (`git push origin feature/YourFeature`).
5. Откройте Pull Request.

---

## More languages
Вклад путем добавления других языков приветствуется. Для добавления файла языка скопируйте одну из папок в папке locale и отредактируйте файл rmt.po в POEdit или другом редакторе.
