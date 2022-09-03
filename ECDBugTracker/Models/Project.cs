using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ECDBugTracker.Models
{
    public class Project
    {
        public int Id { get; set; }
        public int CompanyId { get; set; }
        [Required]
        [StringLength(100)]
        [DisplayName("Project Name")]
        public string? Name { get; set; }
        [Required]
        [StringLength(2000)]
        [DisplayName("Project Description")]
        public string? Description { get; set; }

        [DataType(DataType.Date)]
        [DisplayName("Date Created")]
        public DateTime Created { get; set; }

        [DataType(DataType.Date)]
        [DisplayName("Project Start Date")]
        public DateTime StartDate { get; set; }

        [DataType(DataType.Date)]
        [DisplayName("Project End Date")]
        public DateTime EndDate { get; set; }
        public int ProjectPriorityId { get; set; }

        [NotMapped]
        [DataType(DataType.Upload)]
        public virtual IFormFile? ImageFormFile { get; set; }

        [DisplayName("Project Image")]
        public byte[]? ImageFileData { get; set; }

        [DisplayName("File Extension")]
        public string? ImageContentType { get; set; }

        public bool Archived { get; set; }

        //navigation properties 
        public virtual Company? Company { get; set; }
        public virtual ProjectPriority? ProjectPriority { get; set; }
        public virtual ICollection<BTUser>? Members { get; set; } = new HashSet<BTUser>();
        public virtual ICollection<Ticket>? Tickets { get; set; } = new HashSet<Ticket>();

    }
}
