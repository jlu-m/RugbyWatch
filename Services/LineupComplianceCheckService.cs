using RugbyWatch.Data;

namespace RugbyWatch.Services {
    public class LineupComplianceCheckService( RugbyMatchDbContext dbContext ) {

        public void Execute( Match match ) {
            if (match.FileId == 11207)
            {
                Console.WriteLine("Match 11207");
            }
            var localTeamsFromSameClub = GetTeamsFromSameClub(match.LocalTeamId);
            var localMatchesFromSuperiorLeagues = GetLastMatchesFromSuperiorLeagues(match, localTeamsFromSameClub);
            var localCommonPlayers = GetCommonPlayers(match, localMatchesFromSuperiorLeagues);
            StoreSuspiciousMatch(match, localCommonPlayers, localMatchesFromSuperiorLeagues);
            var visitorTeamsFromSameClub = GetTeamsFromSameClub(match.VisitorTeamId);
            var visitorMatchesFromSuperiorLeagues = GetLastMatchesFromSuperiorLeagues(match, visitorTeamsFromSameClub);
            var visitorCommonPlayers = GetCommonPlayers(match, visitorMatchesFromSuperiorLeagues);
            StoreSuspiciousMatch(match, visitorCommonPlayers, visitorMatchesFromSuperiorLeagues);
        }

        private void StoreSuspiciousMatch(Match match, List<Player> commonPlayers, List<int> matchesFromSuperiorLeagues)
        {
            if (commonPlayers.Count > 3)
            {
                var suspiciousMatch = new SuspiciousMatch
                {
                    MatchId = match.Id,
                    Match = match,
                    IllegalPlayers = string.Join("; ", commonPlayers.Select(p => p.FullName)),
                    MainMatchFileId = match.FileId,
                    PreviousMatchesFileId = dbContext.Matches!.Where(m => matchesFromSuperiorLeagues.Contains(m.Id)).Select(m => m.FileId).ToList()
                };
                dbContext.SuspiciousMatches!.Add(suspiciousMatch);
                dbContext.SaveChanges();
            }
        }


        private List<int> GetTeamsFromSameClub( int localTeamId ) {
            var currentTeam = dbContext.Teams!.FirstOrDefault(t => t.Id == localTeamId);
            if ( currentTeam == null ||  currentTeam.ClubId == null) return new List<int>();
            
            var teamsFromSameClub = dbContext.Teams!
                .Where(t => t.ClubId == currentTeam.ClubId && t.Id != localTeamId)
                .Select(t => t.Id)
                .ToList();

            return teamsFromSameClub;
        }

        private List<int> GetLastMatchesFromSuperiorLeagues( Match match, List<int> teams ) {
            return dbContext.Matches!.Where(m =>
                m.Day > match.Day.AddDays(-9)
                && m.Day < match.Day
                && GetSuperiorLeagues(match.LeagueId)
                    .Contains(m.LeagueId)
                && (teams.Contains(m.LocalTeamId) || teams.Contains(m.VisitorTeamId)))
                .Select(m => m.Id)
                .ToList();
        }

        private List<Player> GetCommonPlayers( Match match, List<int> matchIdsFromSuperiorLeagues ) {
            var players = dbContext.Lineups!
                .Where(l => l.MatchId == match.Id)
                .Select(l => l.PlayerId).ToList();

            var playersFromSuperiorLeagues = dbContext.Lineups!
                .Where(l => matchIdsFromSuperiorLeagues.Contains(l.MatchId))
                .Select(l => l.PlayerId).ToList();

            var commonPlayers = dbContext.Players!
                .Where(p => players
                    .Intersect(playersFromSuperiorLeagues)
                    .Contains(p.Id)).ToList();
            return commonPlayers;
        }

        private List<int> GetSuperiorLeagues( int leagueId ) {
            var league = dbContext.Leagues!.FirstOrDefault(l => l.Id == leagueId)!.Name;
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
            var superiorLeaguesIds = dbContext.Leagues!.Where(l => superiorLeagues.Contains(l.Name)).Select(l => l.Id).ToList();
            return superiorLeaguesIds ;
        }
    }
}
