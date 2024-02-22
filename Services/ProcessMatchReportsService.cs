using Microsoft.EntityFrameworkCore;
using Microsoft.Identity.Client.Extensions.Msal;
using RugbyWatch.Data;
using System.Diagnostics;

namespace RugbyWatch.Services {
    public class ProcessMatchReportsService(RugbyMatchDbContext dbContext, DownloadMatchReportService downloadService, StoreMatchReportService storeService, LineupComplianceCheckService lineupComplianceCheckService) {

        private readonly RugbyMatchDbContext _context = dbContext;
        private readonly DownloadMatchReportService _downloadService = downloadService;
        private readonly StoreMatchReportService _storeService = storeService;
        private readonly LineupComplianceCheckService _lineupComplianceCheckService = lineupComplianceCheckService;

        public async Task<int> Execute()
        {
            var tracker = await _context.LastDownloadedMatchReports!.FirstOrDefaultAsync() ?? new LastDownloadedMatchReport() { RegionalMatchReportId = 0 };
            int currentId = tracker.RegionalMatchReportId + 1;
            int consecutiveErrors = 0;
            int successfulReports = 0;
            while ( true )
            {
                    var response = await _downloadService.Execute(currentId);
                    if ( response == -1 ) {
                        consecutiveErrors++; 
                        if ( consecutiveErrors >= 25 ) 
                        {
                            tracker.RegionalMatchReportId = currentId - 1;
                            break; 
                        }
                        currentId++; 
                        continue; 
                    }
                    var match = _storeService.Execute(response);

                    if (match is { Match: not null })
                        _lineupComplianceCheckService.Execute(match.Match);
                    consecutiveErrors = 0;
                    tracker.RegionalMatchReportId = currentId; 
                    currentId++;
                    successfulReports++;
            }
            tracker.RegionalMatchReportId = currentId - 25;
            _context.LastDownloadedMatchReports!.Update(tracker);
            await _context.SaveChangesAsync();
            return successfulReports;
        }
    }
}
