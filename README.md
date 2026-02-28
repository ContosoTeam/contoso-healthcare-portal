![メインブランチ CI](https://img.shields.io/badge/%E3%83%A1%E3%82%A4%E3%83%B3%E3%83%96%E3%83%A9%E3%83%B3%E3%83%81_CI-passing-brightgreen)
![CodeQL セキュリティ分析](https://img.shields.io/badge/CodeQL_%E3%82%BB%E3%82%AD%E3%83%A5%E3%83%AA%E3%83%86%E3%82%A3%E5%88%86%E6%9E%90-passing-brightgreen)
![OpenSSF Scorecard](https://img.shields.io/badge/openssf_scorecard-8.0-brightgreen)
![OpenSSF Best Practices](https://img.shields.io/badge/openssf_best_practices-gold-yellow)
![ライセンス](https://img.shields.io/badge/%E3%83%A9%E3%82%A4%E3%82%BB%E3%83%B3%E3%82%B9-MIT-green)
![.NET](https://img.shields.io/badge/.NET-8.0-512BD4)
![Blazor](https://img.shields.io/badge/Blazor-Server-512BD4)

# Contoso ヘルスケアポータル

ASP.NET Core と Blazor で構築された患者情報管理ポータルです。医療機関向けに患者記録、予約、処方箋の管理機能を提供します。

## 機能

- 患者登録と電子カルテ管理
- 予約スケジュール管理
- 処方箋管理
- 検査結果ビューア
- 医療従事者ダッシュボード
- FHIR 互換 API

## クイックスタート

```bash
dotnet restore
dotnet run
```

## 技術スタック

- ASP.NET Core 8.0
- Blazor Server
- Entity Framework Core
- SQL Server
- Azure Blob Storage（医療画像）

## ライセンス

このプロジェクトは [MIT ライセンス](LICENSE)の下で公開されています。

## セキュリティ

脆弱性を発見された場合は、[セキュリティポリシー](SECURITY.md)をご確認ください。
