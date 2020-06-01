# LogTalk

ファイルの変更を監視して、変更内容を CeVIO で読み上げるプログラムです。

## 動作環境

* Windows 10 64ビット
* .NET Framework 4.8
* CeVIO Creative Studio 7

## 使用方法

1. 監視対象のファイルを選択します。
1. 発声パラメータを設定します。
1. 開始ボタンをクリックします。
1. 監視対象のファイルに内容が追加されると追加分を読み上げます。

## 制限事項

* ファイルの文字コードは UTF-8 のみ対応しています。
* 英単語はローマ字風の発声になります。

## ライセンス

Copyright (C) akira 2020
このプログラムは LGPL バージョン3以降 の元で配布されます。

このプログラムは以下のライブラリを使用しています。

* [CeVIO Creative Studio 外部連携インターフェイス](http://cevio.jp/)
* [Material Design In XAML](http://materialdesigninxaml.net/)
* [Prism Library](https://prismlibrary.com/)
* [ReactiveProperty](https://github.com/runceel/ReactiveProperty)
