using System.ComponentModel.DataAnnotations;

namespace RugbyWatch.Data {
    public class Team {

        [Key]
        public int Id { get; set; }
        public string Name { get; set; }
    }
}
