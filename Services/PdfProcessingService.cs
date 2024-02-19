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
                var extractedData = ExtractDataFromFile(file);
                await SaveDataToDatabase(extractedData);
                processedFilesCount++;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error processing file {file}: {ex.Message}");
            }
        }

        return processedFilesCount;
    }

    private Minute ExtractDataFromFile(string filePath)
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


    [SuppressMessage("ReSharper.DPA", "DPA0006: Large number of DB commands", MessageId = "count: 81")]
    private async Task SaveDataToDatabase(Minute data)
    {
        var existingMatch = _dbContext.Matches.FirstOrDefault(m =>
            m.GameRound == data.Match.GameRound &&
            m.TeamLocal == data.Match.TeamLocal &&
            m.TeamVisitor == data.Match.TeamVisitor);
        if (existingMatch == null)
        {
            _dbContext.Matches.Add(data.Match);
            _dbContext.SaveChanges();
        }
        else
            Console.WriteLine("This match already exists in the database.");

        var totalPlayers = new List<Player>();
        totalPlayers.AddRange(data.LocalPlayers);
        totalPlayers.AddRange(data.VisitorPlayers);
        foreach (var player in totalPlayers)
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
}