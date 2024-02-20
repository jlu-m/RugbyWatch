using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RugbyWatch.Data {
    public class Team {

        [Key]
        public int Id { get; set; }
        public required string Name { get; set; }

        [ForeignKey("ClubId")]
        public int? ClubId { get; set; }
        public Club? Club { get; set; }
    }
}
