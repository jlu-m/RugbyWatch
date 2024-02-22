using System.Text;
using Docnet.Core;
using Docnet.Core.Models;
using RugbyWatch.Data;
using RugbyWatch.Helpers;

namespace RugbyWatch.Services;

public class StoreMatchReportService( RugbyMatchDbContext dbContext, IConfiguration configuration, IMatchReportParser parser ) {
    private readonly IMatchReportParser _parser = parser;

    public MatchReport Execute( int matchReportId ) {
        var matchReport = Directory
            .EnumerateFiles(_parser.MatchReportDirectoryPath!, $"{matchReportId}.pdf")
            .FirstOrDefault();

        if ( matchReport != null ) {
            try {
                var formattedMatchReport = ExtractMatchReportFromFile(matchReport);
                formattedMatchReport.Match!.FileId = matchReportId;
                var fileName = Path.GetFileName(matchReport);

                if ( !IsMensRegionalLeague(formattedMatchReport) ) {
                    var regionalMatchReportFilter = Path.Combine(_parser.MatchReportFilterDirectory!, fileName);
                    File.Move(matchReport, regionalMatchReportFilter, true);
                    Console.WriteLine($"{matchReport} discarded: Match league is {formattedMatchReport?.Match?.LeagueName}");
                    return null!;
                }
                SaveMatchReportToDatabase(formattedMatchReport);
                var regionalMatchReportArchive = Path.Combine(_parser.MatchReportArchiveDirectory!, fileName);
                File.Move(matchReport, regionalMatchReportArchive, true);
                Console.WriteLine($"{matchReport} successfully integrated!");
                return formattedMatchReport;
            }
            catch ( Exception ex ) {
                Console.WriteLine($"Error processing file {matchReport}: {ex.Message}");
                var fileName = Path.GetFileName(matchReport);
                var regionalMatchReportError = Path.Combine(_parser.MatchReportErrorDirectory!, fileName);
                File.Move(matchReport, regionalMatchReportError, true);
                return null!;
            }
        }
        return null!;
    }

    private MatchReport ExtractMatchReportFromFile( string filePath ) {
        if ( !File.Exists(filePath) )
            throw new FileNotFoundException("The PDF file does not exist.", filePath);

        var extractedText = new StringBuilder();
        using ( var docReader = DocLib.Instance.GetDocReader(filePath, new PageDimensions()) ) {
            int pageCount = docReader.GetPageCount();
            for ( int pageIndex = 0; pageIndex < pageCount; pageIndex++ )
                extractedText.AppendLine(docReader.GetPageReader(pageIndex).GetText());
        }
        return _parser.ParseMatchReport(extractedText.ToString());
    }

    private bool IsMensRegionalLeague( MatchReport matchReport ) {
        if ( matchReport.Match!.LeagueName.StartsWith("3ª") || (matchReport.Match.LeagueName.StartsWith("2ª")) || (matchReport.Match.LeagueName.StartsWith("1ª")) )
            return true;
        return false;
    }

    private void SaveMatchReportToDatabase( MatchReport data ) {
        data.Match!.LeagueId = GetOrCreateLeague(data.Match.LeagueName).Id;
        data.Match.LocalTeamId = GetOrCreateTeam(data.Match.LocalTeamName).Id;
        data.Match.VisitorTeamId = GetOrCreateTeam(data.Match.VisitorTeamName).Id;
        var existingMatch = dbContext.Matches!.FirstOrDefault(m =>
            m.FileId == data.Match.FileId);
        if ( existingMatch == null ) {
            dbContext.Matches!.Add(data.Match);
            dbContext.SaveChanges();
            InsertLineUp(data);
            dbContext.SaveChanges();
        }
        else {
            data.Match = existingMatch;
        }
    }


    private Team GetOrCreateTeam( string teamName ) {
        var existingTeam = dbContext.Teams!.FirstOrDefault(t => t.Name == teamName);
        if ( existingTeam == null ) {
            var club = dbContext.Clubs!.FirstOrDefault(c => teamName.StartsWith(c.Name.Substring(0, 10)));
            var newTeam = new Team { Name = teamName, ClubId = club?.Id };
            dbContext.Teams!.Add(newTeam);
            dbContext.SaveChanges();
            return newTeam;
        }
        else {
            return existingTeam;
        }
    }

    private League GetOrCreateLeague( string leagueName ) {
        var existingLeague = dbContext.Leagues!.FirstOrDefault(l => l.Name == leagueName);
        if ( existingLeague == null ) {
            var newLeague = new League { Name = leagueName };
            dbContext.Leagues!.Add(newLeague);
            dbContext.SaveChanges();
            return newLeague;
        }

        return existingLeague;
    }

    private Player GetOrCreatePlayer( Player player ) {

        var existingPlayer = dbContext.Players!.FirstOrDefault(m => m.FullName == player.FullName);
        if ( existingPlayer == null ) {
            dbContext.Players!.Add(player);
            dbContext.SaveChanges();
            return player;
        }

        return existingPlayer;
    }
    private void InsertLineUp( MatchReport data ) {
        if ( data.Match == null ) return; // Early exit if there's no match data

        InsertPlayersIntoLineup(data, data.LocalPlayers, data.Match.LocalTeamId);
        InsertPlayersIntoLineup(data, data.VisitorPlayers, data.Match.VisitorTeamId);
    }

    private void InsertPlayersIntoLineup( MatchReport data, List<Player>? players, int teamId ) {
        foreach ( var player in players! ) {
            var playerEntity = GetOrCreatePlayer(player);
            var playerId = playerEntity.Id;

            bool existingLineup = dbContext.Lineups!.Any(l =>
                data.Match != null &&
                l.MatchId == data.Match.Id &&
                l.PlayerId == playerId &&
                l.TeamId == teamId);

            if ( !existingLineup ) {
                var lineup = new Lineup {
                    MatchId = data.Match!.Id,
                    PlayerId = playerId,
                    TeamId = teamId
                };

                dbContext.Lineups!.Add(lineup);
            }
        }
        dbContext.SaveChanges();
    }
}