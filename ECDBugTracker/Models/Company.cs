using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ECDBugTracker.Models
{
    public class Company
    {
        public int Id { get; set; }
        [Required]
        [DisplayName("CompanyName")]
        public string? Name { get; set; }
        [DisplayName("Company Description")]
        public string? Description { get; set; }
        [NotMapped]
        public IFormFile? ImageFormFile { get; set; }
        public string? ImageFileName { get; set; }
        public string? ImageFileType { get; set; }

        //navigation properties

        public virtual ICollection<Project>? Projects { get; set; } = new HashSet<Project>();
        public virtual ICollection<Invite>? Invites { get; set; } = new HashSet<Invite>();
        public virtual ICollection<BTUser> Members { get; set; } = new HashSet<BTUser>();

    }
}
