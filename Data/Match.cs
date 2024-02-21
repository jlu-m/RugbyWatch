using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RugbyWatch.Data {
    public class Match {
        [Key]
        public int Id { get; set; }
        public required DateTime Day { get; set; }
        public required string Time { get; set; }

        [NotMapped]
        public required string LeagueName { get; set; }

        [ForeignKey("LeagueId")]
        public int LeagueId { get; set; }
        public League? League { get; set; }
        public required string GameRound { get; set; }
        public required string Field { get; set; }

        [NotMapped]

        public required string LocalTeamName { get; set; }

        [ForeignKey("LocalTeamId")]
        public int LocalTeamId { get; set; }
        public Team? LocalTeam { get; set; }


        [NotMapped]
        public required string VisitorTeamName { get; set; }

        [ForeignKey("VisitorTeamId")]
        public int VisitorTeamId { get; set; }
        public Team? VisitorTeam { get; set; }

    }
}
