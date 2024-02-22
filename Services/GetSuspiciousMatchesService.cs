using Microsoft.EntityFrameworkCore;
using RugbyWatch.Data;

namespace RugbyWatch.Services {
    public class GetSuspiciousMatchesService {
        private readonly RugbyMatchDbContext _context;

        public GetSuspiciousMatchesService (RugbyMatchDbContext context)
        {
            _context = context;
        }

        public async Task<List<SuspiciousMatch>> Execute()
        {
            return await _context.SuspiciousMatches!.ToListAsync();
        }
    }
}
