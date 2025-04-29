# Azure Monitor Migrator

A Model Context Protocol (MCP) server tool to help you migrate from the legacy Application Insights SDK to the new Azure Monitor OpenTelemetry distribution.

## Overview

This tool analyzes your code for Application Insights usage and provides specific guidance for converting it to Azure Monitor OpenTelemetry. It helps automate the process of identifying and replacing legacy Application Insights APIs with their proper OpenTelemetry equivalents.

## Features

- Detects Application Insights SDK usage in your code
- Analyzes existing TelemetryClient pattern usage
- Generates specific conversion instructions for different application types:
  - ASP.NET Core applications
  - Console applications
  - Worker Service applications
- Converts Application Insights APIs to OpenTelemetry APIs

## Usage with Visual Studio Code

This tool is designed to work with VS Code as an MCP server. To use it:

1. Configure the tool as an MCP server in VS Code by adding the following to your `claude_desktop_config.json`:

```json
{
  "mcpServers": {
    "AzureMonitorMigrator": {
      "command": "Path\\to\\AzureMonitorMigrator.exe",
      "args": []
    }
  }
}
```

2. Replace `Path\\to\\AzureMonitorMigrator.exe` with the actual path to the executable.

3. Restart VS Code and Claude Desktop if they're already running.

## Available Commands

The tool provides the following MCP commands:

- `CheckForApplicationInsights` - Checks if a project file or folder contains Application Insights SDK references
- `AnalyzeProject` - Performs in-depth analysis of a project and suggests changes needed for migration
- `GenerateMigrationCode` - Generates conversion code examples for specific application types
- `ListSupportedAppTypes` - Lists all supported application types
- `CreateMigrationRule` - Creates a new rule file for a custom migration scenario

