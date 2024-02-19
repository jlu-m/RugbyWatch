using System.ComponentModel.DataAnnotations;

namespace RugbyWatch.Data {
    public class Player {
        [Key]
        public int Id { get; set; }
        public string FullName { get; set; }
    }
}
