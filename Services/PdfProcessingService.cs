using RugbyWatch.Data;
using System.Text;
using Docnet.Core;
using Docnet.Core.Models;
using RugbyWatch.Helpers;


public class PdfProcessingService {
    private readonly RugbyMatchDbContext _dbContext;
    private readonly string _pdfDirectoryPath;

    public PdfProcessingService( RugbyMatchDbContext dbContext, IConfiguration configuration ) {
        _dbContext = dbContext;
        _pdfDirectoryPath = configuration.GetValue<string>("PdfFilesDirectory");
    }

    public async Task<int> ProcessFilesAsync() {
        int processedFilesCount = 0;
        var pdfFiles = Directory.EnumerateFiles(_pdfDirectoryPath, "*.pdf");

        foreach ( var file in pdfFiles ) {
            try {
                using ( var transaction = await _dbContext.Database.BeginTransactionAsync() ) {
                    var extractedData = ExtractMinuteFromFile(file);
                    SaveMinuteToDatabase(extractedData);
                    processedFilesCount++;

                    await transaction.CommitAsync();
                }
            }
            catch ( Exception ex ) {
                Console.WriteLine($"Error processing file {file}: {ex.Message}");
            }
        }
        return processedFilesCount;
    }

    private Minute ExtractMinuteFromFile( string filePath ) {
        if ( !File.Exists(filePath) )
            throw new FileNotFoundException("The PDF file does not exist.", filePath);

        var extractedText = new StringBuilder();
        using ( var docReader = DocLib.Instance.GetDocReader(filePath, new PageDimensions()) ) {
            int pageCount = docReader.GetPageCount();
            for ( int pageIndex = 0; pageIndex < pageCount; pageIndex++ )
                extractedText.AppendLine(docReader.GetPageReader(pageIndex).GetText());
        }
        var parser = new PdfParser();
        return parser.ParseMinute(extractedText.ToString());
    }

    private void SaveMinuteToDatabase( Minute data ) {
        data.Match.LocalTeamId = GetOrCreateTeam(data.Match.LocalTeamName).Id;
        data.Match.VisitorTeamId = GetOrCreateTeam(data.Match.VisitorTeamName).Id;
        var existingMatch = _dbContext.Matches.FirstOrDefault(m =>
            m.GameRound == data.Match.GameRound &&
            m.LocalTeamId == data.Match.LocalTeamId &&
            m.VisitorTeamId == data.Match.VisitorTeamId);
        if ( existingMatch == null ) {
            _dbContext.Matches.Add(data.Match);
            _dbContext.SaveChanges();
        }
        else {
            data.Match = existingMatch;
            Console.WriteLine("This match already exists in the database.");
        }
        InsertLineUp(data);
    }



    private Team GetOrCreateTeam( string teamName ) {
        var existingTeam = _dbContext.Teams.FirstOrDefault(t => t.Name == teamName);
        if ( existingTeam == null ) {
            var newTeam = new Team { Name = teamName };
            _dbContext.Teams.Add(newTeam);
            _dbContext.SaveChanges();
            return newTeam;
        }
        else {
            Console.WriteLine("This team already exists in the database.");
            return existingTeam;
        }
    }



    private Player InsertPlayer( Player player ) {

        var existingPlayer = _dbContext.Players.FirstOrDefault(m =>
            m.FullName == player.FullName);
        if ( existingPlayer == null ) {
            _dbContext.Players.Add(player);
            _dbContext.SaveChanges();
            return player;
        }

        return existingPlayer;
    }

    private async void InsertLineUp( Minute data ) {
        foreach ( var player in data.LocalPlayers ) {
            Console.WriteLine($"Preparing to insert player {player.FullName}");
            var playerEntity = InsertPlayer(player);
            var playerId = playerEntity.Id;

            Console.WriteLine($"Verifying if lineup exists");
            var existingLineup = _dbContext.Lineups
                .Any(l => l.MatchId == data.Match.Id && l.PlayerId == playerId && l.TeamId == data.Match.LocalTeamId);

            if ( !existingLineup ) {
                var lineup = new Lineup {
                    MatchId = data.Match.Id,
                    PlayerId = playerId,
                    TeamId = data.Match.LocalTeamId
                };
                Console.WriteLine($"Preparing to insert lineup");

                _dbContext.Lineups.Add(lineup);
                _dbContext.SaveChanges();

            }
        }

        foreach ( var player in data.VisitorPlayers ) {
            var playerEntity = InsertPlayer(player);
            var playerId = playerEntity.Id;

            var existingLineup = _dbContext.Lineups
                .Any(l => l.MatchId == data.Match.Id && l.PlayerId == playerId && l.TeamId == data.Match.VisitorTeamId);

            if ( !existingLineup ) {
                var lineup = new Lineup {
                    MatchId = data.Match.Id,
                    PlayerId = playerId,
                    TeamId = data.Match.VisitorTeamId
                };
                _dbContext.Lineups.Add(lineup);
                _dbContext.SaveChanges();

            }
        }

    }

}