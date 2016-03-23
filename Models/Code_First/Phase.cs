using System.ComponentModel.DataAnnotations;

namespace TgpBugTracker.Models
{
    public class Phase
    {
        public int Id { get; set; }
        
        public int Step { get; set; }
        
        public string Name { get; set; }
    }
}