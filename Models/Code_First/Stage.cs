using System.ComponentModel.DataAnnotations;

namespace TgpBugTracker.Models
{
    public class Stage
    {
        [Required]
        public int Id { get; set; }
        public string Name { get; set; }
    }
}