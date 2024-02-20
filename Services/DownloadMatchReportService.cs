using Microsoft.EntityFrameworkCore;
using RugbyWatch.Data;

namespace RugbyWatch.Services {
    public class DownloadMatchReportService
    {
    private readonly RugbyMatchDbContext _context;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IConfiguration _configuration;

    public DownloadMatchReportService(RugbyMatchDbContext context, IHttpClientFactory httpClientFactory, IConfiguration configuration)
    {
        _context = context;
        _httpClientFactory = httpClientFactory;
        _configuration = configuration;
    }


public async Task DownloadPdfsAsync()
{
    var tracker = await _context.LastDownloadedMatchReports!.FirstOrDefaultAsync() ?? new LastDownloadedMatchReport() { RegionalMatchReportId = 0 };
    int currentId = tracker.RegionalMatchReportId + 1;
    int consecutiveErrors = 0; // Counter for consecutive errors

    while (true) // Infinite loop, break condition inside
    {
        string url = $"https://rugbymadrid.matchready.es/es/public/competition/match_pdf/{currentId}/act/";
        try
        {
            var httpClient = _httpClientFactory.CreateClient();
            var response = await httpClient.GetAsync(url);
            
            if (!response.IsSuccessStatusCode)
            {
                consecutiveErrors++; // Increment error counter
                if (consecutiveErrors >= 5) // Check if 5 consecutive errors occurred
                {
                    tracker.RegionalMatchReportId= currentId - 1; 
                    await _context.SaveChangesAsync();
                    break; // Exit the loop after 5 consecutive errors
                }
                currentId++; // Move to the next ID even in case of error
                continue; // Continue to the next iteration
            }

            // Reset consecutive errors counter after a successful response
            consecutiveErrors = 0;

            using (var ms = await response.Content.ReadAsStreamAsync())
            {
                await SavePdfAsync(ms, $"{currentId}.pdf"); // Ensure SavePdf is awaited
            }

            tracker.RegionalMatchReportId = currentId; // Update tracker with the successfully processed ID
            currentId++; // Prepare next ID
        }
        catch
        {
            consecutiveErrors++; // Increment error counter on exception
            if (consecutiveErrors >= 5)
            {
                tracker.RegionalMatchReportId = currentId - 1;
                await _context.SaveChangesAsync();
                break; // Stop processing after 5 consecutive errors
            }
            currentId++; // Move to the next ID even in case of exception
        }
    }
}


    private async Task SavePdfAsync(Stream pdfStream, string fileName)
    {
        var directoryPath = _configuration.GetValue<string>("PdfFilesDirectory");

        var filePath = Path.Combine(directoryPath!, fileName);

        using (var fileStream = new FileStream(filePath, FileMode.Create, FileAccess.Write, FileShare.None, bufferSize: 4096, useAsync: true))
        {
            if (pdfStream.CanSeek)
            {
                pdfStream.Position = 0;
            }

            await pdfStream.CopyToAsync(fileStream);
        }
    }

}

}
