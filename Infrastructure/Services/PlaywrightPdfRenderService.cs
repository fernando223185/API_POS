using Application.Abstractions.Reports;
using Microsoft.Playwright;

namespace Infrastructure.Services
{
    /// <summary>
    /// Convierte HTML a PDF usando Playwright con Chromium headless.
    /// Requiere que los binarios de Chromium estén instalados:
    ///   dotnet tool install --global Microsoft.Playwright.CLI
    ///   playwright install chromium
    /// En producción (EC2/Docker) se instala en el Dockerfile o deployment script.
    /// </summary>
    public class PlaywrightPdfRenderService : IPdfRenderService
    {
        public async Task<byte[]> RenderHtmlToPdfAsync(string html)
        {
            using var playwright = await Playwright.CreateAsync();
            await using var browser = await playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions
            {
                Headless = true,
                Args = new[] { "--no-sandbox", "--disable-setuid-sandbox" }
            });

            var page = await browser.NewPageAsync();

            await page.SetContentAsync(html, new PageSetContentOptions
            {
                WaitUntil = WaitUntilState.NetworkIdle
            });

            var pdfBytes = await page.PdfAsync(new PagePdfOptions
            {
                Format = "A4",
                PrintBackground = true,
                Margin = new Margin
                {
                    Top = "12mm",
                    Bottom = "12mm",
                    Left = "12mm",
                    Right = "12mm",
                },
            });

            return pdfBytes;
        }
    }
}
