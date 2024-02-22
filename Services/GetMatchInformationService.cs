using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using RugbyWatch.Data;

namespace RugbyWatch.Services
{
    public class GetMatchInformationService
    {
        private readonly RugbyMatchDbContext _context;

        public GetMatchInformationService(RugbyMatchDbContext context)
        {
            _context = context;
        }

        public async Task<Match> Execute(int matchId)
        {
            return (await _context.Matches!
                .Include(m => m.LocalTeam)
                .Include(m => m.VisitorTeam)
                .FirstOrDefaultAsync(m => m.Id == matchId))!;
        }
    }
}