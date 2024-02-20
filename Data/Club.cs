using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RugbyWatch.Data {
    public class Club {

        [Key]
        public int Id { get; set; }

        public required string Name { get; set; }
    }
}
