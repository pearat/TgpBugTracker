using System.ComponentModel.DataAnnotations;

namespace TgpBugTracker.Models
{
    public class Priority
    {
        [Required]
        public int Id { get; set; }
        public string Name { get; set; }
    }
}