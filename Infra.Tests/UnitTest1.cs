using PuppeteerSharp;
using Xunit;
using Xunit.Abstractions;

namespace Infra.Tests
{
    public class GoogleScreenshotTests : IClassFixture<PuppeteerFixture>
    {
        private readonly PuppeteerFixture _fixture;
        private readonly ITestOutputHelper _output;

        public GoogleScreenshotTests(PuppeteerFixture fixture, ITestOutputHelper output)
        {
            _fixture = fixture;
            _output = output;
        }

        [Fact]
        public async Task CanTakeScreenshotOfGoogle()
        {
            var browser = await _fixture.BrowserTask;
            var page = await browser.NewPageAsync();

            await page.GoToAsync("https://www.google.com");

            // Take a screenshot of the page before searching
            await page.AttachScreenshotAsync("Screenshot of Google before search", _output);

            // Enter "Test" into the search box and take a screenshot
            var searchBox = await page.QuerySelectorAsync("input[name='q']");
            await searchBox.TypeAsync("Test");
            await page.AttachScreenshotAsync("Screenshot of Google with search query", _output);

            // Click the search button and take a screenshot of the search results page
            var searchButton = await page.QuerySelectorAsync("input[name='btnK']");
            await searchButton.ClickAsync();
            await page.WaitForNavigationAsync();
            await page.AttachScreenshotAsync("Screenshot of Google search results", _output);

            await page.CloseAsync();
        }

        [Fact]
        public async Task CanTakeScreenshotOfGoogle2()
        {
            var browser = await _fixture.BrowserTask;
            var page = await browser.NewPageAsync();

            await page.GoToAsync("https://www.google.com");

            // Take a screenshot of the page before searching
            await page.AttachScreenshotAsync("Screenshot of Google before search", _output);

            // Enter "Test" into the search box and take a screenshot
            var searchBox = await page.QuerySelectorAsync("input[name='q']");
            await searchBox.TypeAsync("Test");
            await page.AttachScreenshotAsync("Screenshot of Google with search query", _output);

            // Click the search button and take a screenshot of the search results page
            var searchButton = await page.QuerySelectorAsync("input[name='btnK']");
            await searchButton.ClickAsync();
            await page.WaitForNavigationAsync();
            await page.AttachScreenshotAsync("Screenshot of Google search results", _output);

            await page.CloseAsync();
        }
    }

    public class PuppeteerFixture : IDisposable
    {
        public Task<IBrowser> BrowserTask { get; }

        public PuppeteerFixture()
        {
            var browserFetcher = new BrowserFetcher();

            BrowserTask = Task.Run(async () =>
            {
                using var browserFetcher = new BrowserFetcher();
                await browserFetcher.DownloadAsync(BrowserFetcher.DefaultChromiumRevision);
                return await Puppeteer.LaunchAsync(new LaunchOptions
                {
                    Headless = true
                });
            });
        }

        public void Dispose()
        {
            BrowserTask.Result.CloseAsync().GetAwaiter().GetResult();
        }
    }
}