using System.Globalization;
using System.Text.RegularExpressions;
using RugbyWatch.Data;
using Match = RugbyWatch.Data.Match;

namespace RugbyWatch.Helpers {
    public class RegionalMatchReportParser :IMatchReportParser
    {
        private readonly string[] _matchReportSectionSeparators =
            { "Alineaciones", "Resultado del partido", "Cambios" };
        private readonly string[] _matchSectionSeparators = { "Celebrado en:", "Categoría:", "Jornada:", "El día:", "A la hora:", "En el campo:", "Equipo local:", "Equipo visitante:", "FEDERACIÓN DE RUGBY DE MADRID" };
        public string MatchReportDirectoryPath { get; }
        public string MatchReportArchiveDirectory { get; }
        public string MatchReportErrorDirectory { get; }
        public string MatchReportFilterDirectory { get; }
        public RegionalMatchReportParser(IConfiguration configuration)
        {
            MatchReportDirectoryPath = configuration.GetValue<string>("RegionalMatchReportDirectory")!;
            MatchReportArchiveDirectory = configuration.GetValue<string>("RegionalMatchReportArchiveDirectory")!;
            MatchReportErrorDirectory = configuration.GetValue<string>("RegionalMatchReportErrorDirectory")!;
            MatchReportFilterDirectory = configuration.GetValue<string>("RegionalMatchReportFilterDirectory")!;
        }
        public MatchReport ParseMatchReport(string extractedText)
        {
            var matchReport = new MatchReport();
            string[] sections = extractedText.Split(_matchReportSectionSeparators, StringSplitOptions.RemoveEmptyEntries);
            matchReport.Match =  ParseMatchSection(sections[0]);
            matchReport.LocalPlayers = ParsePlayers(sections[1])[0];
            matchReport.VisitorPlayers = ParsePlayers(sections[1])[1];
             return matchReport;
        }

        private Match ParseMatchSection(string sectionText)
        {
            string cleanedString = sectionText.Replace("\r", "").Replace("\n", "");
            string[] sections = cleanedString.Split(_matchSectionSeparators, StringSplitOptions.RemoveEmptyEntries);
            Match match = new Match
            {
                LeagueName = sections[1].Trim(),
                GameRound = sections[2].Trim(),
                Day = DateTime.ParseExact(sections [ 3 ].Trim(), "dd/MM/yyyy", CultureInfo.InvariantCulture),
                Time = sections [ 4 ].Trim(),
                Field = sections [ 5 ].Trim(),
                LocalTeamName = sections [ 6 ].Trim(),
                VisitorTeamName = sections [ 7 ].Trim(),
            };



            return match;
        }


        public List<List<Player>> ParsePlayers(string extractedText)
        {
            List<List<Player>> totalPlayers = new List<List<Player>>();
            List<Player> localPlayers = new List<Player>();
            List<Player> visitorPlayers = new List<Player>();

            string[] sections = extractedText.Split(new[] { "Dorsal *" }, StringSplitOptions.RemoveEmptyEntries);
            var playersAsText = sections
                .SelectMany(section => section.Split(new[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries))
                .ToList();
            bool isLocal = true;
            foreach(var playerRow in playersAsText)
            {
                if (IsHeader(playerRow))
                {
                    isLocal = !isLocal;
                    continue;
                }
                if (IsEndOfPlayers((playerRow)))
                    break;
                string cleanPlayerName = "";
                if(playerRow.Length > 8)
                    cleanPlayerName = playerRow.Substring(0, playerRow.Length - 8);
                cleanPlayerName = Regex.Replace(cleanPlayerName, @"\d", "");
                if (cleanPlayerName.StartsWith(" X "))
                    cleanPlayerName = cleanPlayerName.Substring(2);
                if (cleanPlayerName.StartsWith(" C/X "))
                    cleanPlayerName = cleanPlayerName.Substring(4);
                if(isLocal)
                    localPlayers.Add(new Player {FullName = cleanPlayerName.Trim()});
                else
                    visitorPlayers.Add(new Player {FullName = cleanPlayerName.Trim() });
            }
            totalPlayers.Add(localPlayers);
            totalPlayers.Add(visitorPlayers);
            return totalPlayers;
        }

        private bool IsHeader(string playerRow)
        {
            if (playerRow.StartsWith("(*) Señale") || playerRow.StartsWith(" Apellidos y nombre") ||
                playerRow.StartsWith("Nº") || playerRow.StartsWith("licencia"))
                return true;
            return false;
        }
        private bool IsEndOfPlayers(string playerRow)
        {
            if (playerRow.StartsWith("-"))
                return true;
            return false;
        }

    }

}
