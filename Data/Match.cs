using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RugbyWatch.Data {
    public class Match {
        [Key]
        public int Id { get; set; }
        public string Day { get; set; }
        public string Time { get; set; }
        public string Category { get; set; }
        public string GameRound { get; set; }
        public string Field { get; set; }

        [NotMapped]

        public string LocalTeamName { get; set; }

        [ForeignKey("LocalTeamId")]
        public int LocalTeamId { get; set; }
        public Team LocalTeam { get; set; }


        [NotMapped]
        public string VisitorTeamName { get; set; }

        [ForeignKey("VisitorTeamId")]
        public int VisitorTeamId { get; set; }
        public Team VisitorTeam { get; set; }
    }
}
