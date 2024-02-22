using System.Globalization;
using System.Text.RegularExpressions;
using RugbyWatch.Data;
using Match = RugbyWatch.Data.Match;

namespace RugbyWatch.Helpers {
    public class NationalMatchReportParser : IMatchReportParser {
        public string MatchReportDirectoryPath { get; }
        public string MatchReportArchiveDirectory { get; }
        public string MatchReportErrorDirectory { get; }
        public string MatchReportFilterDirectory { get; }
        public NationalMatchReportParser( IConfiguration configuration ) {
            MatchReportDirectoryPath = configuration.GetValue<string>("NationalMatchReportDirectory")!;
            MatchReportArchiveDirectory = configuration.GetValue<string>("NationalMatchReportArchiveDirectory")!;
            MatchReportErrorDirectory = configuration.GetValue<string>("NationalMatchReportErrorDirectory")!;
            MatchReportFilterDirectory = configuration.GetValue<string>("NationalMatchReportFilterDirectory")!;
        }
        public MatchReport ParseMatchReport( string extractedText ) {
            var matchReport = new MatchReport();
            string [] sections = extractedText.Split(new [] { "REAL FEDERACION", "MARCADOR", "REEMPLAZOS", "EXPULSIONES", "OBSERVACIONES / ANEXO" }, StringSplitOptions.RemoveEmptyEntries);
            matchReport.Match = ParseMatchSection(sections [ 0 ]);
            matchReport.LocalPlayers = ParsePlayers(sections [ 1 ]);
            matchReport.VisitorPlayers = ParsePlayers(sections [ 2 ]);
            return matchReport;
        }

        private Match ParseMatchSection( string sectionText ) {
            string cleanedString = sectionText.Replace("\r", "").Replace("\n", "");
            string [] sections = cleanedString.Split(new [] { "COMPETICION", "FECHA", "TERRENO DE JUEGO", "TV / STREAMING", "CLUB DE RUGBY", "VRAC SUB-23" }, StringSplitOptions.RemoveEmptyEntries);
            Match match = new Match {
                LeagueName = sections [ 0 ].Trim(),
                Day = DateTime.ParseExact(sections [ 1 ].Trim(), "dd/MM/yyyy", CultureInfo.InvariantCulture),
                Time = sections [ 2 ].Trim(),
                Field = sections [ 3 ].Trim(),
                LocalTeamName = sections [ 4 ].Trim(),
                VisitorTeamName = sections [ 5 ].Trim(),
                GameRound = null
            };

            return match;
        }

        public List<Player> ParsePlayers( string extractedText ) {
            List<Player> players = new List<Player>();

            string [] sections = extractedText.Split(new [] { "Nº 1ªLin APELLIDOS Y NOMBRE Cat A,B,C F" }, StringSplitOptions.RemoveEmptyEntries);
            var playersAsText = sections
                .SelectMany(section => section.Split(new [] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries))
                .ToList();

            foreach ( var playerRow in playersAsText ) {
                if ( IsHeader(playerRow) ) {
                    continue;
                }
                if ( IsEndOfPlayers((playerRow)) )
                    break;
                string cleanPlayerName = "";
                if ( playerRow.Length > 8 )
                    cleanPlayerName = playerRow.Substring(0, playerRow.Length - 8);
                cleanPlayerName = Regex.Replace(cleanPlayerName, @"\d", "");
                if ( cleanPlayerName.StartsWith(" X ") )
                    cleanPlayerName = cleanPlayerName.Substring(2);
                if ( cleanPlayerName.StartsWith(" C/X ") )
                    cleanPlayerName = cleanPlayerName.Substring(4);
                players.Add(new Player { FullName = cleanPlayerName.Trim() });
            }

            return players;
        }

        private bool IsEndOfPlayers( string playerRow ) {
            throw new NotImplementedException();
        }

        private bool IsHeader( string playerRow ) {
            throw new NotImplementedException();
        }
    }

}
