namespace Application.Abstractions.Reports
{
    /// <summary>
    /// Convierte HTML a bytes de PDF usando un headless browser (Playwright/Chromium).
    /// </summary>
    public interface IPdfRenderService
    {
        Task<byte[]> RenderHtmlToPdfAsync(string html);
    }
}
