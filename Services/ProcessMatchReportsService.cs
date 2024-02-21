using Microsoft.EntityFrameworkCore;
using RugbyWatch.Data;

namespace RugbyWatch.Services {
    public class ProcessMatchReportsService(RugbyMatchDbContext dbContext, DownloadMatchReportService downloadService, StoreMatchReportService storeService) {

        private readonly RugbyMatchDbContext _context = dbContext;
        private readonly DownloadMatchReportService _downloadService = downloadService;
        private readonly StoreMatchReportService _storeService = storeService;


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
                    _storeService.Execute();
                    consecutiveErrors = 0;
                    tracker.RegionalMatchReportId = currentId; 
                    currentId++;
                    successfulReports++;
            }

            tracker.RegionalMatchReportId = currentId - 25;
            _context.LastDownloadedMatchReports.Update(tracker);
            await _context.SaveChangesAsync();
            return successfulReports;
        }
    }
}
