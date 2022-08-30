using System.ComponentModel.DataAnnotations;

namespace ECDBugTracker.Models
{
    public class Company
    {
        public int Id { get; set; }
        [Required]
        public string? Name { get; set; }
        public string? Description { get; set; }
        public IFormFile? imageFormFile { get; set; }
        public string? ImageFileName { get; set; }
        public string? ImageFileType { get; set; }

        //navigation properties

        public virtual Project? Projects { get; set; }
        public virtual Invite? Invites { get; set; }
        public virtual ICollection<BTUser> Members { get; set; } = new HashSet<BTUser>();

    }
}
