---
title: WorkshopUploader
sidebar_label: WorkshopUploader
description: Application de bureau Windows pour gérer les projets Steam Workshop et les métadonnées pour Data Center (FrikaMF).
---

# WorkshopUploader

**WorkshopUploader** est une application de bureau **.NET MAUI** pour **Windows**. Elle sert à préparer le **contenu Workshop** pour *Data Center* : arborescence, `metadata.json`, image d’aperçu et envoi via l’API **Steamworks** (Steam doit être lancé et le jeu doit correspondre à l’App ID).

## Rôle de l’application

- Crée un espace de travail **`DataCenterWS`** dans le profil utilisateur (chemin ci-dessous).
- Affiche les **dossiers de projet** ; chaque projet peut contenir un sous-dossier **`content/`** — c’est ce dossier qui est téléversé vers l’élément Workshop.
- Pour chaque projet vous modifiez **titre**, **description**, **visibilité** (Public / FriendsOnly / Private) et **image d’aperçu** ; tout est enregistré dans **`metadata.json`**.
- **Publish to Steam** crée un **nouvel** élément Workshop ou met à jour l’existant si un **identifiant de fichier publié** est déjà enregistré.

## Prérequis

- **Windows** (cible MAUI pour ce projet).
- **Steam** avec un compte connecté qui **possède Data Center** et peut publier sur le Workshop (App ID **4170200**).
- Facultatif : **`WorkshopUploader.exe`** compilée à côté de l’installation du jeu (voir [Compiler et déployer](#build-deploy)).

## Chemin de l’espace de travail

L’espace de travail est fixé **`DataCenterWS`** sous votre profil, par exemple :

`%USERPROFILE%\DataCenterWS`

Au premier lancement, l’application crée la structure et peut déposer un exemple **`metadata.sample.json`** dans `.templates\`.

## Structure d’un projet

Pour chaque projet Workshop :

1. Créez un **dossier** sous `DataCenterWS` (le nom apparaît dans la liste).
2. Ajoutez un sous-dossier **`content\`** et placez-y les fichiers à publier (données de mod, assets — **uniquement** votre contenu, ne redistribuez pas les binaires du jeu).
3. Facultatif : **`metadata.json`** à la main ou via l’application ; l’app enregistre titre, description, visibilité, chemin d’aperçu et après le premier envoi l’**ID du fichier publié**.
4. Facultatif : **`preview.png`** à la racine (ou chemin relatif dans les métadonnées) — vous pouvez choisir une image dans l’app ; elle est copiée en `preview.png`.

Sans **`content/`**, la liste affiche un avertissement (« Missing content/ ») ; l’envoi est impossible tant qu’il manque.

## Utilisation dans l’application

1. **Accueil :** **Workshop projects** — le **chemin de l’espace de travail** est en haut. Tirez pour actualiser.
2. **Ouvrir un projet :** appuyez sur une entrée → **Editor**.
3. **Editor :** titre et description (limites Steam), **visibilité**, **choisir l’aperçu**, puis :
   - **Save metadata.json** — enregistrement seul.
   - **Publish to Steam** — enregistre et envoie **`content/`** ; la première fois crée un nouvel élément Workshop, ensuite réutilise l’**ID** enregistrée.
4. Le **journal** sur l’accueil affiche les messages (init Steam, progression de l’upload, etc.).

Si Steam ne peut pas s’initialiser (Steam fermé, etc.), l’application l’indique.

## Compiler et déployer {#build-deploy}

Depuis le dépôt :

```bash
dotnet build WorkshopUploader/WorkshopUploader.csproj -c Debug
```

Release (publication typique sur Windows) :

```bash
dotnet publish WorkshopUploader/WorkshopUploader.csproj -c Release
```

Sortie typique sous `WorkshopUploader\bin\Release\net9.0-windows10.0.19041.0\win10-x64\publish\WorkshopUploader.exe` (chemin exact selon SDK / TFM).

Pour placer l’outil **à côté du jeu** (pas dans `Mods` / `MelonLoader`) :

`{GameRoot}\WorkshopUploader\`

## Voir aussi

- README du dépôt : [`WorkshopUploader/README.md`](https://github.com/mleem97/gregFramework/blob/master/WorkshopUploader/README.md)
- Contexte Workshop : [Steam Workshop and Tooling](/wiki/meta/Steam-Workshop-and-Tooling)
- Bêtas DevServer (`gregframework.eu`) : [DevServer betas](/wiki/meta/devserver-betas)
