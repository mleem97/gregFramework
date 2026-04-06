---
title: WorkshopUploader
sidebar_label: WorkshopUploader
description: Data Center（FrikaMF）向け Steam Workshop プロジェクトとメタデータを管理する Windows デスクトップアプリ。
---

# WorkshopUploader

**WorkshopUploader** は **Windows** 向けの **.NET MAUI** デスクトップアプリです。*Data Center* の **Workshop 用コンテンツ**（フォルダ構成、`metadata.json`、プレビュー画像）を用意し、**Steamworks** API を使ってアップロードします（Steam を起動し、ゲームの App ID コンテキストが必要です）。

## アプリの機能

- ユーザープロファイル下に **`DataCenterWS`** ワークスペースを作成します（パスは下記）。
- その下の **プロジェクトフォルダ** を一覧表示します。各プロジェクトに **`content/`** サブフォルダを置くと、その中身が Workshop アイテムにアップロードされます。
- プロジェクトごとに **タイトル**、**説明**、**公開範囲**（Public / FriendsOnly / Private）、**プレビュー画像** を編集し、**`metadata.json`** に保存します。
- **Publish to Steam** で **新規** Workshop アイテムを作成するか、**公開済みファイル ID** が保存されていれば既存アイテムを更新します。

## 前提条件

- **Windows**（MAUI は Windows 向け）。
- **Steam** にログインし、**Data Center** を所有し、Workshop にアップロードできるアカウント（App ID **4170200**）。
- 任意: ゲームインストールの隣にビルド済み **`WorkshopUploader.exe`** を置く（[ビルドと配置](#build-deploy) を参照）。

## ワークスペースのパス

ワークスペースはプロファイル内の **`DataCenterWS`** に固定されています。例:

`%USERPROFILE%\DataCenterWS`

初回起動時にフォルダを作成し、`.templates\` に **`metadata.sample.json`** のサンプルを置くことがあります。

## プロジェクト構成

各 Workshop プロジェクトについて:

1. `DataCenterWS` の下に **フォルダ** を作成します（名前が一覧に表示されます）。
2. **`content\`** サブフォルダを作り、Workshop に載せるファイルを置きます（モッドデータやアセット。**自分の**コンテンツのみ。ゲームのバイナリは再配布しないでください）。
3. 任意で **`metadata.json`** を手元で作成するか、アプリから入力します。アプリはタイトル、説明、公開範囲、プレビューへのパス、初回アップロード後の **公開ファイル ID** を保存します。
4. 任意で **`preview.png`** をプロジェクトルートに（またはメタデータの相対パス）。アプリで画像を選ぶと `preview.png` としてコピーされます。

**`content/`** がない場合、一覧に警告（「Missing content/」）が出ます。アップロードはできません。

## アプリの操作

1. **ホーム:** **Workshop projects** — 上部に **ワークスペースのパス** を表示。プルして一覧を更新します。
2. **プロジェクトを開く:** 項目をタップ → **Editor**。
3. **Editor:** タイトルと説明（Steam の文字数制限）、**公開範囲**、**プレビュー選択** のあと:
   - **Save metadata.json** — 保存のみ。
   - **Publish to Steam** — 保存して **`content/`** をアップロード。初回は新規 Workshop アイテム、以降は保存された **ID** を再利用します。
4. ホームの **ログ** にメッセージが表示されます（Steam 初期化、アップロード進行など）。

Steam が初期化できない場合（Steam 未起動など）、アプリが通知します。

## ビルドと配置 {#build-deploy}

リポジトリから:

```bash
dotnet build WorkshopUploader/WorkshopUploader.csproj -c Debug
```

Release（Windows での典型的な publish）:

```bash
dotnet publish WorkshopUploader/WorkshopUploader.csproj -c Release
```

出力は通常 `WorkshopUploader\bin\Release\net9.0-windows10.0.19041.0\win10-x64\publish\WorkshopUploader.exe` 付近（SDK / TFM によりパスは変わります）。

ゲームの隣に **`WorkshopUploader`** フォルダとして配置する場合（`Mods` / `MelonLoader` 内ではない）:

`{GameRoot}\WorkshopUploader\`

## 関連リンク

- リポジトリ README: [`WorkshopUploader/README.md`](https://github.com/mleem97/gregFramework/blob/master/WorkshopUploader/README.md)
- Workshop の背景: [Steam Workshop and Tooling](/wiki/meta/Steam-Workshop-and-Tooling)
- DevServer ベータ（`gregframework.eu`）: [DevServer betas](/wiki/meta/devserver-betas)
