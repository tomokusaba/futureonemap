using Microsoft.Playwright.NUnit;

namespace FutureOneMap.A11yTests;

/// <summary>
/// Playwright ブラウザの設定。
/// </summary>
[SetUpFixture]
public class GlobalSetup
{
    [OneTimeSetUp]
    public void InstallBrowsers()
    {
        // CI 環境では playwright install で事前にインストールされるが、
        // ローカルでも自動インストールできるようにする
        var exitCode = Microsoft.Playwright.Program.Main(["install", "chromium"]);
        if (exitCode != 0)
        {
            throw new InvalidOperationException($"Playwright ブラウザのインストールに失敗しました (exit code: {exitCode})");
        }
    }
}
