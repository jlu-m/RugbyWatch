using System.ComponentModel.DataAnnotations;

namespace RugbyWatch.Data {
    public class League {

        [Key]
        public int Id { get; set; }
        public required string Name { get; set; }
    }
}
