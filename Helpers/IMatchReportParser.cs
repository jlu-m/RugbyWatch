using RugbyWatch.Data;

namespace RugbyWatch.Helpers {
    public interface IMatchReportParser {
        string MatchReportDirectoryPath { get; }
        string MatchReportArchiveDirectory { get; }
        string MatchReportErrorDirectory { get; }
        string MatchReportFilterDirectory { get; }
        MatchReport ParseMatchReport( string extractedText );
    }

}
