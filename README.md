# EmailAttachmentExtractor

EmailAttachmentExtractor - это приложение на C# для извлечения вложений из электронных писем формата .eml.

## Особенности

- Извлечение вложений из .eml файлов
- Сохранение текста письма
- Извлечение встроенных изображений
- Поддержка рекурсивного обхода директорий
- Простой пользовательский интерфейс

## Требования

- .NET 8.0 или выше
- Windows OS

## Установка

1. Склонируйте репозиторий:
```rs
git clone https://github.com/yourusername/EmailAttachmentExtractor.git
```
3. Откройте решение в Visual Studio или другом редакторе кода
4. Соберите проект

## Использование

1. Запустите приложение
2. Выберите директорию с .eml файлами
3. Выберите директорию для сохранения вложений
4. Нажмите "Стврт" для запуска процесса извлечения

## Структура проекта

- `MainViewModel.cs`: Главная ViewModel, где расположена основная логика приложения
- `EmailAttachmentExtractService.cs`: Сервис для извлечения вложений
- `Commands/`: Команды для пользовательского интерфейса
- `Helpers/`: Вспомогательные классы и интерфейсы

## Лицензия

[MIT](https://choosealicense.com/licenses/mit/)
