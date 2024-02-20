using System.Diagnostics.CodeAnalysis;
using RugbyWatch.Data;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Docnet.Core;
using Docnet.Core.Readers;
using Docnet.Core.Models;
using RugbyWatch.Helpers;


public class PdfProcessingService
{
    private readonly RugbyMatchDbContext _dbContext;
    private readonly string _pdfDirectoryPath;

    public PdfProcessingService(RugbyMatchDbContext dbContext, IConfiguration configuration)
    {
        _dbContext = dbContext;
        _pdfDirectoryPath = configuration.GetValue<string>("PdfFilesDirectory");
    }

    public async Task<int> ProcessFilesAsync()
    {
        int processedFilesCount = 0;
        var pdfFiles = Directory.EnumerateFiles(_pdfDirectoryPath, "*.pdf");

        foreach (var file in pdfFiles)
        {
            try
            {
                var extractedData = ExtractMinuteFromFile(file);
                await SaveMinuteToDatabase(extractedData);
                processedFilesCount++;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error processing file {file}: {ex.Message}");
            }
        }

        return processedFilesCount;
    }

    private Minute ExtractMinuteFromFile(string filePath)
    {
        if (!File.Exists(filePath))
            throw new FileNotFoundException("The PDF file does not exist.", filePath);

        var extractedText = new StringBuilder();
        using (var docReader = DocLib.Instance.GetDocReader(filePath, new PageDimensions()))
        {
            int pageCount = docReader.GetPageCount();
            for (int pageIndex = 0; pageIndex < pageCount; pageIndex++)
                extractedText.AppendLine(docReader.GetPageReader(pageIndex).GetText());
        }
        var parser = new PdfParser();
        return parser.ParseMinute(extractedText.ToString());
    }


    private async Task SaveMinuteToDatabase(Minute data)
    {
        InsertTeams(data.Match.LocalTeamName);
        InsertTeams(data.Match.VisitorTeamName);
        InsertMatch(data.Match);
        InsertPlayers(data.LocalPlayers);
        InsertPlayers(data.VisitorPlayers);
        InsertLineUp(data.LocalPlayers);
        InsertLineUp(data.VisitorPlayers);
    }

    private async void InsertTeams(string team)
    {

    }

    private async void InsertMatch(Match match)
    {
        var existingMatch = _dbContext.Matches.FirstOrDefault(m =>
            m.GameRound == match.GameRound &&
            m.LocalTeamName == match.LocalTeamName &&
            m.VisitorTeamName == match.VisitorTeamName);
        if (existingMatch == null)
        {
            _dbContext.Matches.Add(match);
            _dbContext.SaveChanges();
        }
        else
            Console.WriteLine("This match already exists in the database.");
    }

    private async void InsertPlayers(List<Player> players)
    {
        foreach (var player in players)
        {
            var existingPlayer = _dbContext.Players.FirstOrDefault(m =>
                m.FullName == player.FullName);
            if (existingPlayer == null)
            {
                _dbContext.Players.Add(player);
                _dbContext.SaveChanges();
            }
            else
                Console.WriteLine("This player already exists in the database.");
        }
    }

    private async void InsertLineUp(List<Player> players)
    {

    }
}