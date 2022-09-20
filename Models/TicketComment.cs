using System.ComponentModel.DataAnnotations;

namespace ECDBugTracker.Models
{
    public class TicketComment
    {
        public int Id { get; set; }
        [Required]
        [StringLength(2000)]
        public string? Comment { get; set; }
        [DataType(DataType.DateTime)]
        public DateTime Created { get; set; }
        public int TicketId { get; set; }
        [Required]
        public string? UserId { get; set; }

        //navigation properties
        public virtual Ticket? Ticket { get; set; }
        public virtual BTUser? User { get; set; }
    }
}
