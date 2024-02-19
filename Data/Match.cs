using System.ComponentModel.DataAnnotations;

namespace RugbyWatch.Data {
    public class Match {
        [Key]
        public int Id { get; set; }
        public string Day { get; set; }
        public string Time { get; set; }
        public string Category { get; set; }
        public string GameRound { get; set; }
        public string Field { get; set; }
        public string TeamLocal { get; set; }
        public string TeamVisitor { get; set; }
    }
}
