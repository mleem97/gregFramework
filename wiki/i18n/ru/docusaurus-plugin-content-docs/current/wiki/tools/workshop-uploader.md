---
title: WorkshopUploader
sidebar_label: WorkshopUploader
description: Настольное приложение Windows для подготовки проектов Steam Workshop и метаданных для Data Center (FrikaMF).
---

# WorkshopUploader

**WorkshopUploader** — настольное приложение на **.NET MAUI** для **Windows**. Оно помогает готовить **контент для Workshop** для *Data Center*: структура папок, `metadata.json`, превью и загрузка через API **Steamworks** (Steam должен быть запущен, игра — в контексте нужного App ID).

## Возможности

- Создаёт рабочую папку **`DataCenterWS`** в профиле пользователя (путь ниже).
- Показывает **папки проектов**; у каждого проекта может быть подпапка **`content/`** — именно она загружается в элемент Workshop.
- Для каждого проекта задаются **заголовок**, **описание**, **видимость** (Public / FriendsOnly / Private) и **превью**; всё сохраняется в **`metadata.json`**.
- **Publish to Steam** создаёт **новый** элемент Workshop или обновляет существующий, если уже сохранён **идентификатор опубликованного файла**.

## Требования

- **Windows** (MAUI для Windows).
- **Steam** с учётной записью, у которой **есть Data Center** и есть право публиковать в Workshop (App ID **4170200**).
- По желанию: собранная **`WorkshopUploader.exe`** рядом с установкой игры (см. [Сборка и развёртывание](#build-deploy)).

## Путь к рабочей папке

Рабочая папка — **`DataCenterWS`** в профиле пользователя, например:

`%USERPROFILE%\DataCenterWS`

При первом запуске приложение создаёт структуру и может положить пример **`metadata.sample.json`** в `.templates\`.

## Структура проекта

Для каждого проекта Workshop:

1. Создайте **папку** внутри `DataCenterWS` (имя отображается в списке).
2. Добавьте подпапку **`content\`** и поместите туда файлы для элемента Workshop (данные мода, ресурсы — только **ваш** контент; не распространяйте бинарники игры).
3. При необходимости создайте **`metadata.json`** вручную или заполните в приложении; приложение сохраняет заголовок, описание, видимость, путь к превью и после первой загрузки **ID опубликованного файла**.
4. При необходимости **`preview.png`** в корне проекта (или относительный путь в метаданных) — можно выбрать изображение в приложении; оно копируется как `preview.png`.

Без **`content/`** в списке будет предупреждение («Missing content/»); загрузка без этой папки невозможна.

## Работа в приложении

1. **Главный экран:** **Workshop projects** — сверху виден **путь к рабочей папке**. Потяните для обновления списка.
2. **Открыть проект:** нажмите элемент → **Editor**.
3. **Editor:** заголовок и описание (лимиты Steam), **видимость**, **выбор превью**, затем:
   - **Save metadata.json** — только сохранить.
   - **Publish to Steam** — сохранит и загрузит **`content/`**; первый раз создаётся новый элемент Workshop, далее используется сохранённый **ID**.
4. **Журнал** на главной странице показывает сообщения (инициализация Steam, прогресс загрузки и т. д.).

Если Steam не инициализируется (например, не запущен), приложение сообщит об этом.

## Сборка и развёртывание {#build-deploy}

Из репозитория:

```bash
dotnet build WorkshopUploader/WorkshopUploader.csproj -c Debug
```

Release (типичная публикация под Windows):

```bash
dotnet publish WorkshopUploader/WorkshopUploader.csproj -c Release
```

Результат обычно в `WorkshopUploader\bin\Release\net9.0-windows10.0.19041.0\win10-x64\publish\WorkshopUploader.exe` (точный путь зависит от SDK / TFM).

Чтобы положить утилиту **рядом с игрой** (не в `Mods` / `MelonLoader`):

`{GameRoot}\WorkshopUploader\`

## См. также

- README в репозитории: [`WorkshopUploader/README.md`](https://github.com/mleem97/gregFramework/blob/master/WorkshopUploader/README.md)
- Контекст Workshop: [Steam Workshop and Tooling](/wiki/meta/Steam-Workshop-and-Tooling)
- Беты DevServer (`gregframework.eu`): [DevServer betas](/wiki/meta/devserver-betas)
