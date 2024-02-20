using System.ComponentModel.DataAnnotations;

namespace RugbyWatch.Data {
    public class LastDownloadedMatchReport {
        [Key]
        public int Id { get; set; }
        public int regionalMatchReportId { get; set; }
    }
}
