using Microsoft.EntityFrameworkCore;
using RugbyWatch.Data;

namespace RugbyWatch.Services {
    public class DownloadMatchReportService( RugbyMatchDbContext dbContext, IHttpClientFactory httpClientFactory, IConfiguration configuration ) {
        private readonly RugbyMatchDbContext _context = dbContext;
        private readonly IHttpClientFactory _httpClientFactory = httpClientFactory;
        private readonly IConfiguration _configuration = configuration;
        private readonly string? _regionalMatchReportDirectoryPath = configuration.GetValue<string>("RegionalMatchReportDirectory");

        public async Task<int> Execute( int currentId ) {

            string url = $"https://rugbymadrid.matchready.es/es/public/competition/match_pdf/{currentId}/act/";
            try {
                var httpClient = _httpClientFactory.CreateClient();
                var response = await httpClient.GetAsync(url);

                if ( !response.IsSuccessStatusCode ) {
                    return -1;
                }


                using ( var ms = await response.Content.ReadAsStreamAsync() ) {
                    await SaveMatchReportAsync(ms, $"{currentId}.pdf"); // Ensure SavePdf is awaited
                }

            }
            catch
            {
                return -1;
            }

            return currentId;

        }


        private async Task SaveMatchReportAsync( Stream pdfStream, string fileName ) {
            var filePath = Path.Combine(_regionalMatchReportDirectoryPath!, fileName);

            await using var fileStream = new FileStream(filePath, FileMode.Create, FileAccess.Write, FileShare.None, bufferSize: 4096, useAsync: true);
            if ( pdfStream.CanSeek ) {
                pdfStream.Position = 0;
            }

            await pdfStream.CopyToAsync(fileStream);
        }


    }

}
