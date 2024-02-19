using System.Text.RegularExpressions;
using RugbyWatch.Data;
using Match = RugbyWatch.Data.Match;

namespace RugbyWatch.Helpers {
    public class PdfParser
    {
        private readonly string[] MinuteSectionSeparators =
            new string[] { "Alineaciones", "Resultado del partido", "Cambios" };
        private readonly string[] MatchSectionSeparators = new string[] { "Celebrado en:", "Categoría:", "Jornada:", "El día:", "A la hora:", "En el campo:", "Equipo local:", "Equipo visitante:", "FEDERACIÓN DE RUGBY DE MADRID" };
        public Minute ParseMinute(string extractedText)
        {
            var minute = new Minute();
            string[] sections = extractedText.Split(MinuteSectionSeparators, StringSplitOptions.RemoveEmptyEntries);
            minute.Match =  ParseMatchSection(sections[0]);
            minute.LocalPlayers = ParsePlayers(sections[1])[0];
            minute.VisitorPlayers = ParsePlayers(sections[1])[1];
             return minute;
        }

        private Match ParseMatchSection(string sectionText)
        {
            string cleanedString = sectionText.Replace("\r", "").Replace("\n", "");
            string[] sections = cleanedString.Split(MatchSectionSeparators, StringSplitOptions.RemoveEmptyEntries);
            Match match = new Match();

            match.Category = sections[1];
            match.GameRound = sections[2];
            match.Day = sections[3];
            match.Time = sections[4];
            match.Field = sections [5];
            match.TeamLocal = sections [6];
            match.TeamVisitor = sections [7];

            return match;
        }


        public List<List<Player>> ParsePlayers(string extractedText)
        {
            List<List<Player>> totalPlayers = new List<List<Player>>();
            List<Player> localPlayers = new List<Player>();
            List<Player> visitorPlayers = new List<Player>();

            string[] sections = extractedText.Split(new string[] { "Dorsal *" }, StringSplitOptions.RemoveEmptyEntries);
            var playersAsText = sections
                .SelectMany(section => section.Split(new string[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries))
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
                string cleanPlayerName = playerRow.Substring(0, playerRow.Length - 8);
                cleanPlayerName = Regex.Replace(cleanPlayerName, @"\d", "");
                if (cleanPlayerName.StartsWith(" X "))
                    cleanPlayerName = cleanPlayerName.Substring(2);
                if (cleanPlayerName.StartsWith(" C/X "))
                    cleanPlayerName = cleanPlayerName.Substring(4);
                if(isLocal)
                    localPlayers.Add(new Player(){FullName = cleanPlayerName});
                else
                    visitorPlayers.Add(new Player(){FullName = cleanPlayerName});
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
