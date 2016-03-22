using System.ComponentModel.DataAnnotations;

namespace TgpBugTracker
{
    public class UserProject
    {
        [Required]
        public int Id { get; set; }
        [Required]
        public string UserId { get; set; }
        [Required]
        public int ProjectId { get; set; }
    }
}