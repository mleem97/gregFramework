---
title: WorkshopUploader
sidebar_label: WorkshopUploader
description: Aplicación de escritorio para Windows para gestionar proyectos de Steam Workshop y metadatos para Data Center (FrikaMF).
---

# WorkshopUploader

**WorkshopUploader** es una aplicación de escritorio **.NET MAUI** para **Windows**. Te ayuda a preparar **contenido de Workshop** para *Data Center*: estructura de carpetas, `metadata.json`, imagen de vista previa y subida mediante la API de **Steamworks** (Steam debe estar en ejecución y el juego debe ser el contexto de App ID correcto).

## Qué hace la aplicación

- Crea un espacio de trabajo **`DataCenterWS`** en tu perfil de usuario (ruta más abajo).
- Lista **carpetas de proyecto**; cada proyecto puede tener una subcarpeta **`content/`** — esa carpeta es la que se sube al elemento del Workshop.
- En cada proyecto editas **título**, **descripción**, **visibilidad** (Public / FriendsOnly / Private) y **imagen de vista previa**; los datos se guardan en **`metadata.json`**.
- **Publish to Steam** crea un **nuevo** elemento del Workshop o actualiza uno existente si ya hay un **ID de archivo publicado** guardado.

## Requisitos

- **Windows** (el proyecto usa MAUI para Windows).
- **Steam** con una sesión iniciada en una cuenta que **tenga Data Center** y pueda publicar en el Workshop (App ID **4170200**).
- Opcional: **`WorkshopUploader.exe`** compilada junto a la instalación del juego (véase [Compilar y desplegar](#build-deploy)).

## Ruta del espacio de trabajo

El espacio de trabajo está fijado en **`DataCenterWS`** dentro de tu perfil, por ejemplo:

`%USERPROFILE%\DataCenterWS`

En el primer arranque la aplicación crea la estructura y puede dejar un **`metadata.sample.json`** de ejemplo en `.templates\`.

## Estructura del proyecto

Para cada proyecto de Workshop:

1. Crea una **carpeta** bajo `DataCenterWS` (el nombre aparece en la lista).
2. Añade una subcarpeta **`content\`** y coloca ahí los archivos que deben ir al elemento del Workshop (datos del mod, assets — solo **tu** contenido; no redistribuyas binarios del juego).
3. Opcional: **`metadata.json`** manual o rellenado en la app; la app guarda título, descripción, visibilidad, ruta de vista previa y tras la primera subida el **ID del archivo publicado**.
4. Opcional: **`preview.png`** en la raíz del proyecto (u otra ruta relativa en metadatos) — puedes elegir una imagen en la app; se copia como `preview.png`.

Sin **`content/`**, la lista muestra un aviso («Missing content/»); no se puede subir hasta que exista.

## Uso en la aplicación

1. **Inicio:** **Workshop projects** — arriba verás la **ruta del espacio de trabajo**. Desliza para actualizar la lista.
2. **Abrir proyecto:** toca una entrada → **Editor**.
3. **Editor:** título y descripción (límites de caracteres de Steam), **visibilidad**, **elegir vista previa**, luego:
   - **Save metadata.json** — solo guardar.
   - **Publish to Steam** — guarda y sube **`content/`**; la primera vez crea un elemento nuevo del Workshop después reutiliza el **ID** guardado.
4. El **registro** en la página principal muestra mensajes (inicio de Steam, progreso de subida, etc.).

Si Steam no puede inicializarse (p. ej. Steam cerrado), la aplicación lo indica.

## Compilar y desplegar {#build-deploy}

Desde el repositorio:

```bash
dotnet build WorkshopUploader/WorkshopUploader.csproj -c Debug
```

Release (publicación típica en Windows):

```bash
dotnet publish WorkshopUploader/WorkshopUploader.csproj -c Release
```

La salida suele estar en `WorkshopUploader\bin\Release\net9.0-windows10.0.19041.0\win10-x64\publish\WorkshopUploader.exe` (la ruta exacta depende del SDK / TFM).

Para colocar la herramienta **junto al juego** (no dentro de `Mods` / `MelonLoader`):

`{GameRoot}\WorkshopUploader\`

## Ver también

- README del repositorio: [`WorkshopUploader/README.md`](https://github.com/mleem97/gregFramework/blob/master/WorkshopUploader/README.md)
- Contexto del Workshop: [Steam Workshop and Tooling](/wiki/meta/Steam-Workshop-and-Tooling)
- Betas DevServer (`gregframework.eu`): [DevServer betas](/wiki/meta/devserver-betas)
