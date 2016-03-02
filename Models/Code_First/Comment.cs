using System;
using System.ComponentModel.DataAnnotations;

namespace TgpBugTracker.Models
{
    public class Comment
    {
        [Required]
        public int Id { get; set; }
        [Required]
        public DateTimeOffset Date { get; set; }
        [Required]
        public string Detail { get; set; }
        public string MediaURL { get; set; }
        public string Title { get; set; }
        [Required]
        public int AuthorId { get; set; }
        [Required]
        public int TicketId { get; set; }
    }
}