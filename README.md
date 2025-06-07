# DurableMultiAgentWorkflowSample

マルチエージェントのレビュープロセスを Azure Durable Functions と Azure OpenAI Service を用いて実装したサンプルアプリケーションです。

## プロジェクトの概要

このサンプルは以下の主要コンポーネントから構成されています：

1. **Workflow サービス** - Azure Functions と Azure Durable Functions を使用してマルチエージェントのオーケストレーションを行います
2. **Windows クライアント** - WPF で実装されたデスクトップクライアント
3. **SignalR** - リアルタイムの通信に使用

### エージェントのワークフロー

このサンプルでは、以下の3つのAIエージェントを使って文章生成・レビュープロセスを実装しています：

- **Writer Agent**: ユーザーの入力に基づいて文章を生成
- **Reviewer Agent**: 生成された文章をレビューしてフィードバックを提供
- **Approver Agent**: レビュー結果に基づいて承認または差し戻しを判断

## 実行方法

### 前提条件

- .NET 9
- Azure OpenAI Service のアクセス
- Azure SignalR Service (Serverless モード)

### AppHost プロジェクトの設定

AppHost プロジェクトは .NET Aspire プロジェクトです。パラメーターの設定は `appsettings.json` などの設定ファイルで行います。

`appsettings.json` の例:{
  "Parameters": {
    "aoai-endpoint": "<your-aoai-endpoint>",
    "aoai-modeldeploymentname": "<your-model-deployment-name>",
    "signalr-connectionstring": "<your-signalr-connection-string>"
  }
}
パラメーターの説明:
- `aoai-endpoint`: Azure OpenAI Service のエンドポイント
- `aoai-modeldeploymentname`: Azure OpenAI にデプロイしたモデルのデプロイ名
- `signalr-connectionstring`: Serverless モードの Azure SignalR Service の接続文字列

### アプリケーションの起動手順

1. **AppHost プロジェクトの起動**:
   - Visual Studio から DurableMultiAgentWorkflowSample.AppHost プロジェクトを起動するか、
   - コマンドラインから `dotnet run --project DurableMultiAgentWorkflowSample.AppHost` を実行

2. **Windows クライアントの起動**:
   - AppHost の起動完了後、自動的に Windows クライアントが起動します
   - または手動で DurableMultiAgentWorkflowSample.WindowsClient プロジェクトを実行します

3. **ワークフローの実行**:
   - Windows クライアントでテキストを入力し「Start Workflow」ボタンをクリックすると、AIエージェントによるレビュープロセスが開始されます
   - 進捗状況はリアルタイムで表示され、必要に応じてユーザー入力を求められます