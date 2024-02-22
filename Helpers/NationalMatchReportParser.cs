using System.Text.RegularExpressions;
using RugbyWatch.Data;
using Match = RugbyWatch.Data.Match;

namespace RugbyWatch.Helpers {
    public class NationalMatchReportParser : IMatchReportParser
    {
        public string MatchReportDirectoryPath { get; }
        public string MatchReportArchiveDirectory { get; }
        public string MatchReportErrorDirectory { get; }
        public string MatchReportFilterDirectory { get; }
        public NationalMatchReportParser(IConfiguration configuration)
        {
            MatchReportDirectoryPath = configuration.GetValue<string>("NationalMatchReportDirectory")!;
            MatchReportArchiveDirectory = configuration.GetValue<string>("NationalMatchReportArchiveDirectory")!;
            MatchReportErrorDirectory = configuration.GetValue<string>("NationalMatchReportErrorDirectory")!;
            MatchReportFilterDirectory = configuration.GetValue<string>("NationalMatchReportFilterDirectory")!;
        }
        public MatchReport ParseMatchReport(string extractedText)
        {
            throw new NotImplementedException();
        }
    }

}
