using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Identity.Client.Extensions.Msal;
using RugbyWatch.Data;
using RugbyWatch.Helpers;
using System.Diagnostics;

namespace RugbyWatch.Services {
    public class ProcessMatchReportsService {
        private readonly RugbyMatchDbContext _context;
        private readonly DownloadMatchReportService _downloadService;
        private readonly StoreMatchReportService _regionalStoreService;
        private readonly StoreMatchReportService _nationalStoreService;
        private readonly LineupComplianceCheckService _lineupComplianceCheckService;

        public ProcessMatchReportsService( RugbyMatchDbContext dbContext, IConfiguration configuration, DownloadMatchReportService downloadService, RegionalMatchReportParser regionalParser, NationalMatchReportParser nationalParser, LineupComplianceCheckService lineupComplianceCheckService ) {
            _context = dbContext;
            _downloadService = downloadService;
            _regionalStoreService = new StoreMatchReportService(dbContext, configuration, regionalParser);
            _nationalStoreService = new StoreMatchReportService(dbContext, configuration, nationalParser);
            _lineupComplianceCheckService = lineupComplianceCheckService;
        }

        public async Task<int> Execute() {
            await ProcessNationalMatchReport();
            return await ProcessRegionalMatchReport();
        }

        private async Task<int> ProcessRegionalMatchReport() {
            var tracker = await _context.LastDownloadedMatchReports!.FirstOrDefaultAsync() ?? new LastDownloadedMatchReport() { RegionalMatchReportId = 0 };
            int currentId = tracker.RegionalMatchReportId + 1;
            int consecutiveErrors = 0;
            int successfulRegionalMatchReports = 0;
            while ( true ) {
                var response = await _downloadService.Execute(currentId);
                if ( response == -1 ) {
                    consecutiveErrors++;
                    if ( consecutiveErrors >= 25 ) {
                        tracker.RegionalMatchReportId = currentId - 1;
                        break;
                    }
                    currentId++;
                    continue;
                }
                var match = _regionalStoreService.Execute(response);

                if ( match is { Match: not null } )
                    _lineupComplianceCheckService.Execute(match.Match);
                consecutiveErrors = 0;
                tracker.RegionalMatchReportId = currentId;
                currentId++;
                successfulRegionalMatchReports++;
            }
            tracker.RegionalMatchReportId = currentId - 25;
            _context.LastDownloadedMatchReports!.Update(tracker);
            await _context.SaveChangesAsync();
            return successfulRegionalMatchReports;
        }

        private async Task<int> ProcessNationalMatchReport() {
            _nationalStoreService.Execute(967);
            return 0;
        }
    }
}
