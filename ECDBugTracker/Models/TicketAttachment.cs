using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ECDBugTracker.Models
{
    public class TicketAttachment
    {
        public int Id { get; set; }
        public string? Description { get; set; }
        [DataType(DataType.DateTime)]
        public DateTime Created { get; set; }
        public int TicketId { get; set; }
        [Required]
        public string? UserId { get; set; }

        public byte[]? ImageFileData { get; set; }
        public string? ImageFileType { get; set; }
        [NotMapped]
        public IFormFile? ImageFormFile { get; set; }

        //navigation properties
        public virtual Ticket? Ticket { get; set; }
        public virtual BTUser? User { get; set; }

    }
}
