using System.ComponentModel.DataAnnotations;

namespace TgpBugTracker.Models
{
    public class IssueType
    {
        [Required]
        public int Id { get; set; }
        public string Name { get; set; }
    }
}