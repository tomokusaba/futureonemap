using Deque.AxeCore.Commons;
using Deque.AxeCore.Playwright;
using Microsoft.Playwright;
using Microsoft.Playwright.NUnit;

namespace FutureOneMap.A11yTests;

/// <summary>
/// axe-core を使用した WCAG 2.2 AA アクセシビリティ検査テスト。
/// </summary>
[TestFixture]
public class AccessibilityTests : PageTest
{
    private const string BaseUrl = "http://localhost:8080";
    private IPage _page = null!;

    [SetUp]
    public async Task SetUp()
    {
        _page = await Browser.NewPageAsync();
    }

    [TearDown]
    public async Task TearDown()
    {
        await _page.CloseAsync();
    }

    [Test]
    [Description("index.html が WCAG 2.2 Level AA に準拠していること")]
    public async Task IndexPage_ShouldHaveNoAccessibilityViolations()
    {
        await _page.GotoAsync(BaseUrl);
        await _page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        var results = await _page.RunAxe(CreateWcag22Options());

        ReportViolations(results);
        Assert.That(results.Violations, Is.Empty, "WCAG 2.2 AA の違反が検出されました");
    }

    [Test]
    [Description("モーダル表示時にアクセシビリティ違反がないこと")]
    public async Task Modal_ShouldHaveNoAccessibilityViolations()
    {
        await _page.GotoAsync(BaseUrl);
        await _page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        // 最初の画像が読み込まれるのを待機してクリック
        var firstImage = _page.Locator(".step-image").First;
        await firstImage.WaitForAsync(new() { State = WaitForSelectorState.Visible, Timeout = 10_000 });
        await firstImage.ClickAsync();

        // モーダルが表示されるのを待機
        await _page.WaitForSelectorAsync("#imageModal[style*='display: block']");

        var results = await _page.RunAxe(CreateWcag22Options());

        ReportViolations(results);
        Assert.That(results.Violations, Is.Empty, "モーダル表示時に WCAG 2.2 AA の違反が検出されました");
    }

    [Test]
    [Description("キーボードナビゲーションが正しく機能すること")]
    public async Task KeyboardNavigation_ShouldWork()
    {
        await _page.GotoAsync(BaseUrl);
        await _page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        // Tab でスキップリンクにフォーカスが当たること
        await _page.Keyboard.PressAsync("Tab");
        var skipLink = _page.Locator(".skip-link");
        await Expect(skipLink).ToBeFocusedAsync();

        // ArrowRight でステップ 2 へ移動
        await _page.Keyboard.PressAsync("ArrowRight");
        var liveRegion = _page.Locator("#liveRegion");
        await Expect(liveRegion).ToContainTextAsync("ステップ 2");

        // ArrowLeft でステップ 1 へ戻る
        await _page.Keyboard.PressAsync("ArrowLeft");
        await Expect(liveRegion).ToContainTextAsync("ステップ 1");
    }

    [Test]
    [Description("モーダルのフォーカストラップが機能すること")]
    public async Task ModalFocusTrap_ShouldWork()
    {
        await _page.GotoAsync(BaseUrl);
        await _page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        var firstImage = _page.Locator(".step-image").First;
        await firstImage.WaitForAsync(new() { State = WaitForSelectorState.Visible, Timeout = 10_000 });
        await firstImage.ClickAsync();

        await _page.WaitForSelectorAsync("#imageModal[style*='display: block']");

        // 閉じるボタンにフォーカスが当たっていること
        var closeBtn = _page.Locator("#closeModalBtn");
        await Expect(closeBtn).ToBeFocusedAsync();

        // Escape でモーダルが閉じること
        await _page.Keyboard.PressAsync("Escape");
        var modal = _page.Locator("#imageModal");
        await Expect(modal).ToHaveAttributeAsync("style", "display: none;");
    }

    [Test]
    [Description("prefers-reduced-motion が適用されてもアクセシビリティ違反がないこと")]
    public async Task ReducedMotion_ShouldHaveNoAccessibilityViolations()
    {
        await _page.EmulateMediaAsync(new() { ReducedMotion = ReducedMotion.Reduce });
        await _page.GotoAsync(BaseUrl);
        await _page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        var results = await _page.RunAxe(CreateWcag22Options());

        ReportViolations(results);
        Assert.That(results.Violations, Is.Empty, "reduced-motion 環境で WCAG 2.2 AA の違反が検出されました");
    }

    [Test]
    [Description("プログレスバーに適切な ARIA 属性が設定されていること")]
    public async Task ProgressBar_ShouldHaveAriaAttributes()
    {
        await _page.GotoAsync(BaseUrl);
        await _page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        var progressBar = _page.Locator("[role='progressbar']");
        await Expect(progressBar).ToHaveAttributeAsync("aria-valuemin", "0");
        await Expect(progressBar).ToHaveAttributeAsync("aria-valuemax", "100");
        await Expect(progressBar).ToHaveAttributeAsync("aria-label", "ナビゲーション進捗");
    }

    [Test]
    [Description("ランドマークが正しく設定されていること")]
    public async Task Landmarks_ShouldBePresent()
    {
        await _page.GotoAsync(BaseUrl);
        await _page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        // header, main, nav が存在すること
        await Expect(_page.Locator("header")).ToBeVisibleAsync();
        await Expect(_page.Locator("main")).ToBeVisibleAsync();
        await Expect(_page.Locator("nav").First).ToBeVisibleAsync();

        // dialog が存在すること
        var modal = _page.Locator("[role='dialog']");
        await Expect(modal).ToHaveCountAsync(1);
    }

    /// <summary>
    /// WCAG 2.2 AA タグでフィルタリングした AxeRunOptions を生成する。
    /// </summary>
    private static AxeRunOptions CreateWcag22Options() => new()
    {
        RunOnly = new RunOnlyOptions
        {
            Type = "tag",
            Values = new List<string> { "wcag2a", "wcag2aa", "wcag22aa" }
        }
    };

    /// <summary>
    /// 違反の詳細をテスト出力に書き込む。
    /// </summary>
    private static void ReportViolations(AxeResult results)
    {
        if (results.Violations.Length == 0) return;

        foreach (var violation in results.Violations)
        {
            TestContext.Out.WriteLine($"[{violation.Impact}] {violation.Id}: {violation.Description}");
            TestContext.Out.WriteLine($"  Help: {violation.HelpUrl}");
            foreach (var node in violation.Nodes)
            {
                TestContext.Out.WriteLine($"  Target: {string.Join(", ", node.Target)}");
            }
        }
    }
}
