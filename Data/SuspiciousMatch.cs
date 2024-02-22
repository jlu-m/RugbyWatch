using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RugbyWatch.Data {
    public class SuspiciousMatch{

        [Key]
        public int Id { get; set; }

        [ForeignKey("MatchId")]
        public int MatchId { get; set; }
        public required Match Match { get; set; }

        public required string IllegalPlayers { get; set; }
        public int? MainMatchFileId { get; set; }

        public List<int?>? PreviousMatchesFileId { get; set; }  
    }
}
