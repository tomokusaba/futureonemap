# FutureOneMap

FutureOne 本社への道のりを画像付きステップで案内する、Web ベースのナビゲーションシステムです。大崎駅南改札口から FutureOne 本社まで、写真とテキストで道順を分かりやすく表示します。

## デモ

Azure Static Web Apps にデプロイされています。`main` ブランチへの push で自動デプロイされます。

## 機能

- 写真付きステップバイステップの道順案内
- サムネイル画像による高速な初期読み込み
- クリック/タップで写真を拡大表示（モーダル）
- キーボードの左右矢印キーによるステップ移動
- プログレスバーによる進捗表示
- WCAG 2.2 Level AA 準拠のアクセシビリティ対応

## アクセシビリティ

WCAG 2.2 Level AA に準拠しています。

- セマンティック HTML（`<header>`, `<main>`, `<nav>`, `<section>`）
- スキップリンクによるメインコンテンツへの直接移動
- 適切な見出し階層（h1 → h2）
- WCAG AA 基準を満たす色コントラスト比
- すべての操作要素に可視フォーカスインジケーター
- ARIA 属性（プログレスバー、モーダル、ナビゲーション）
- モーダルのフォーカストラップとフォーカス復帰
- `aria-live` リージョンによるステップ変更の通知
- `prefers-reduced-motion` メディアクエリ対応

## プロジェクト構成

```
futureonemap/
├── index.html                  # メインページ
├── generate_thumbnails.sh      # サムネイル生成スクリプト
├── img/                        # ナビゲーション画像
│   └── thumbnails/             # サムネイル画像
├── tests/
│   └── FutureOneMap.A11yTests/ # アクセシビリティテスト (C#)
└── .github/
    └── workflows/
        ├── azure-static-web-apps-black-plant-0767a7310.yml  # デプロイ
        └── accessibility.yml                                 # a11y CI
```

## 開発

### 前提条件

- .NET 9.0 SDK
- Node.js（ローカルサーバー用）
- ImageMagick（サムネイル生成用、任意）

### ローカルサーバーの起動

```bash
npx http-server . -p 8080
```

### サムネイルの生成

新しい画像を追加した場合、サムネイルを生成してください。

```bash
bash generate_thumbnails.sh
```

## アクセシビリティテスト

[Deque.AxeCore.Playwright](https://github.com/dequelabs/axe-core-nuget) と Playwright を使用した自動アクセシビリティ検査を実行できます。

### テストの実行

```bash
# ビルド
dotnet build tests/FutureOneMap.A11yTests

# テスト実行（ローカルサーバーが起動している必要があります）
dotnet test tests/FutureOneMap.A11yTests --settings tests/FutureOneMap.A11yTests/.runsettings
```

### テスト項目

| テスト | 内容 |
|---|---|
| `IndexPage_ShouldHaveNoAccessibilityViolations` | WCAG 2.2 AA 全体スキャン |
| `Modal_ShouldHaveNoAccessibilityViolations` | モーダル表示時のスキャン |
| `KeyboardNavigation_ShouldWork` | キーボード操作の検証 |
| `ModalFocusTrap_ShouldWork` | フォーカストラップと Escape キー |
| `ReducedMotion_ShouldHaveNoAccessibilityViolations` | モーション軽減環境でのスキャン |
| `ProgressBar_ShouldHaveAriaAttributes` | ARIA 属性の検証 |
| `Landmarks_ShouldBePresent` | ランドマーク要素の検証 |

## CI/CD

GitHub Actions で 2 つのワークフローが動作します。

- **Azure Static Web Apps CI/CD** — `main` ブランチへの push / PR で Azure Static Web Apps へ自動デプロイ
- **Accessibility CI (axe DevTools)** — `main` ブランチへの push / PR で axe-core によるアクセシビリティ検査を自動実行

## 技術スタック

- HTML / CSS / JavaScript（フレームワーク不使用）
- Azure Static Web Apps（ホスティング）
- C# / .NET 9 / NUnit / Playwright / axe-core（アクセシビリティテスト）
- GitHub Actions（CI/CD）
