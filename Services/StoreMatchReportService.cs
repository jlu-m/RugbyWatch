﻿using System.Text;
using Docnet.Core;
using Docnet.Core.Models;
using RugbyWatch.Data;
using RugbyWatch.Helpers;

namespace RugbyWatch.Services;

public class StoreMatchReportService(RugbyMatchDbContext dbContext, IConfiguration configuration)
{
    private readonly string? _regionalMatchReportDirectoryPath = ConfigurationBinder.GetValue<string>(configuration, "RegionalMatchReportDirectory");
    private readonly string? _regionalMatchReportArchiveDirectory = ConfigurationBinder.GetValue<string>(configuration, "RegionalMatchReportArchiveDirectory");
    private readonly string? _regionalMatchReportErrorDirectory = ConfigurationBinder.GetValue<string>(configuration, "RegionalMatchReportErrorDirectory");
    private readonly string? _regionalMatchReportFilterDirectory = ConfigurationBinder.GetValue<string>(configuration, "RegionalMatchReportFilterDirectory");

    public int Execute() {
        int processedFilesCount = 0;
        var regionalMatchReports = Directory.EnumerateFiles(_regionalMatchReportDirectoryPath!, "*.pdf");

        foreach ( var regionalMatchReport in regionalMatchReports ) {
            try {
                var formattedMatchReport = ExtractMatchReportFromFile(regionalMatchReport);
                var fileName = Path.GetFileName(regionalMatchReport);

                if (!isMensRegionalLeague(formattedMatchReport))
                {
                    var regionalMatchReportFilter = Path.Combine(_regionalMatchReportFilterDirectory!, fileName);
                    File.Move(regionalMatchReport, regionalMatchReportFilter, true);
                    continue;
                }
                SaveMatchReportToDatabase(formattedMatchReport);
                processedFilesCount++;
                var regionalMatchReportArchive = Path.Combine(_regionalMatchReportArchiveDirectory!, fileName);
                File.Move(regionalMatchReport, regionalMatchReportArchive, true);
            }
            catch ( Exception ex ) {
                Console.WriteLine($"Error processing file {regionalMatchReport}: {ex.Message}");
                var fileName = Path.GetFileName(regionalMatchReport);
                var regionalMatchReportError = Path.Combine(_regionalMatchReportArchiveDirectory!, fileName);
                File.Move(regionalMatchReport, regionalMatchReportError, true);
            }
        }
        return processedFilesCount;
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
        var parser = new RegionalMatchReportParser();
        return parser.ParseMatchReport(extractedText.ToString());
    }

    private bool isMensRegionalLeague(MatchReport matchReport)
    {
        if(matchReport.Match.LeagueName.StartsWith(" 3ª") || (matchReport.Match.LeagueName.StartsWith(" 2ª")) || (matchReport.Match.LeagueName.StartsWith(" 1ª")))
            return true;
        return false;
    }

    private void SaveMatchReportToDatabase( MatchReport data ) {
        data.Match!.LeagueId = GetOrCreateLeague(data.Match.LeagueName).Id;
        data.Match.LocalTeamId = GetOrCreateTeam(data.Match.LocalTeamName).Id;
        data.Match.VisitorTeamId = GetOrCreateTeam(data.Match.VisitorTeamName).Id;
        var existingMatch = dbContext.Matches!.FirstOrDefault(m =>
            m.GameRound == data.Match.GameRound &&
            m.LocalTeamId == data.Match.LocalTeamId &&
            m.VisitorTeamId == data.Match.VisitorTeamId);
        if ( existingMatch == null ) {
            dbContext.Matches!.Add(data.Match);
            dbContext.SaveChanges();
        }
        else {
            data.Match = existingMatch;
            Console.WriteLine("This match already exists in the database.");
        }
        InsertLineUp(data);
    }


    private Team GetOrCreateTeam( string teamName ) {
        var existingTeam = dbContext.Teams!.FirstOrDefault(t => t.Name == teamName);
        if ( existingTeam == null ) {
            var newTeam = new Team { Name = teamName };
            dbContext.Teams!.Add(newTeam);
            dbContext.SaveChanges();
            return newTeam;
        }
        else {
            Console.WriteLine("This team already exists in the database.");
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
    private void InsertLineUp(MatchReport data)
    {
        if (data.Match == null) return; // Early exit if there's no match data

        InsertPlayersIntoLineup(data, data.LocalPlayers, data.Match.LocalTeamId);
        InsertPlayersIntoLineup(data, data.VisitorPlayers, data.Match.VisitorTeamId);
    }

    private void InsertPlayersIntoLineup(MatchReport data, List<Player>? players, int teamId)
    {
        foreach (var player in players!)
        {
            var playerEntity = GetOrCreatePlayer(player);
            var playerId = playerEntity.Id;

            bool existingLineup = dbContext.Lineups!.Any(l => 
                data.Match != null && 
                l.MatchId == data.Match.Id && 
                l.PlayerId == playerId && 
                l.TeamId == teamId);

            if (!existingLineup)
            {
                var lineup = new Lineup
                {
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