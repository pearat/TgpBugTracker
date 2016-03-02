using System;
using System.ComponentModel.DataAnnotations;

namespace TgpBugTracker.Models
{
    public class Log
    {
        [Required]
        public int Id { get; set; }
        public DateTimeOffset Date { get; set; }
        public string NewValue { get; set; }
        public string OldValue { get; set; }
        public string Property { get; set; }
        [Required]
        public int AuthorId { get; set; }
        [Required]
        public int TicketId { get; set; }
    }
}