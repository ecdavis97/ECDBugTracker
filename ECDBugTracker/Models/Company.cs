using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ECDBugTracker.Models
{
    public class Company
    {
        public int Id { get; set; }
        [Required]
        public string? Name { get; set; }
        public string? Description { get; set; }
        [NotMapped]
        public IFormFile? ImageFormFile { get; set; }
        public string? ImageFileName { get; set; }
        public string? ImageFileType { get; set; }

        //navigation properties

        public virtual Project? Projects { get; set; }
        public virtual Invite? Invites { get; set; }
        public virtual ICollection<BTUser> Members { get; set; } = new HashSet<BTUser>();

    }
}
