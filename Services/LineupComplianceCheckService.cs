using RugbyWatch.Data;

namespace RugbyWatch.Services {
    public class LineupComplianceCheckService( RugbyMatchDbContext dbContext ) {

        public void Execute( Match match ) {
            var teamsFromSameClub = GetTeamsFromSameClub(match.LocalTeamId);
            var matchesFromSuperiorLeagues = GetLastMatchesFromSuperiorLeagues(match, teamsFromSameClub);
            var commonPlayers = GetCommonPlayers(match.Id, matchesFromSuperiorLeagues);
        }


        private List<int> GetTeamsFromSameClub( int localTeamId ) {
            var currentTeam = dbContext.Teams.FirstOrDefault(t => t.Id == localTeamId);
            if ( currentTeam == null ) return new List<int>();

            var teamsFromSameClub = dbContext.Teams
                .Where(t => t.ClubId == currentTeam.ClubId && t.Id != localTeamId)
                .Select(t => t.Id)
                .ToList();

            return teamsFromSameClub;
        }

        private List<int> GetLastMatchesFromSuperiorLeagues( Match match, List<int> teams ) {
            return dbContext.Matches.Where(m =>
                m.Day > match.Day.AddDays(-9)
                && m.Day < match.Day
                && GetSuperiorLeagues(match.LeagueId)
                    .Contains(m.LeagueId)
                && (teams.Contains(m.LocalTeamId) || teams.Contains(m.VisitorTeamId)))
                .Select(m => m.Id)
                .ToList();
        }

        private List<string> GetCommonPlayers( int matchId, List<int> matchIdsFromSuperiorLeagues ) {
            var players = dbContext.Lineups
                .Where(l => l.MatchId == matchId)
                .Select(l => l.PlayerId).ToList();

            var playersFromSuperiorLeagues = dbContext.Lineups
                .Where(l => matchIdsFromSuperiorLeagues.Contains(l.MatchId))
                .Select(l => l.PlayerId).ToList();

            var commonPlayers = dbContext.Players
                .Where(p => players
                    .Intersect(playersFromSuperiorLeagues)
                    .Contains(p.Id))
                .Select(p => p.FullName)
                .ToList();

            string filePath = @"PDFRepository\\Regional\\Alert.txt";
            string contentToWrite = $"On match {matchId}: {commonPlayers}";
            File.AppendAllText(filePath, contentToWrite);

            return commonPlayers;
        }

        private List<int> GetSuperiorLeagues( int leagueId ) {
            var league = dbContext.Leagues.FirstOrDefault(l => l.Id == leagueId).Name;
            List<string> superiorLeagues = new List<string>();
            switch ( league ) {
                case "1ª Regional":
                    return new List<int>();
                case "2ª Regional - Grupo Ascenso":
                    superiorLeagues.AddRange(new List<string> { "1ª Regional" });
                    break;
                case "2ª Regional - Grupo Permanencia":
                    superiorLeagues.AddRange(new List<string> { "1ª Regional", "2ª Regional - Grupo Ascenso" });
                    break;
                case "2ª Regional - Grupo Descenso":
                    superiorLeagues.AddRange(new List<string> { "1ª Regional", "2ª Regional - Grupo Ascenso", "2ª Regional - Grupo Permanencia" });
                    break;
                case "2ª Regional - Grupo A":
                    superiorLeagues.AddRange(new List<string> { "1ª Regional" });
                    break;
                case "2ª Regional - Grupo B":
                    superiorLeagues.AddRange(new List<string> { "1ª Regional" });
                    break;
                case "3ª Regional - Grupo Ascenso":
                    superiorLeagues.AddRange(new List<string>
                    {
                        "1ª Regional", "2ª Regional - Grupo Ascenso", "2ª Regional - Grupo Permanencia",
                        "2ª Regional - Grupo Descenso", "2ª Regional - Grupo A", "2ª Regional - Grupo B"
                    });
                    break;
                case "3ª Regional - Grupo Permanencia":
                    superiorLeagues.AddRange(new List<string>
                    {
                        "1ª Regional", "2ª Regional - Grupo Ascenso", "2ª Regional - Grupo Permanencia",
                        "2ª Regional - Grupo Descenso","2ª Regional - Grupo A", "2ª Regional - Grupo B",
                        "3ª Regional - Grupo Ascenso"
                    });
                    break;
                default:
                    return new List<int>();
            }
            var superiorLeaguesIds = dbContext.Leagues.Where(l => superiorLeagues.Contains(l.Name)).Select(l => l.Id).ToList();
            return superiorLeaguesIds ;
        }
    }
}
