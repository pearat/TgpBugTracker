using System;
using System.ComponentModel.DataAnnotations;

namespace TgpBugTracker.Models
{
    public class Notice
    {
        public int Id { get; set; }
        [Required]
        [DataType(DataType.Date)]
        [Display(Name = "Due Date")]
        [DisplayFormat(DataFormatString = "{0:MM/dd}")]
        public DateTimeOffset Date { get; set; }
        [Required]        
        public string Detail { get; set; }
        [Required]
        public string AuthorId { get; set; }
        [Required]  
        public string LeaderId { get; set; }

        [Required]
        public int TicketId { get; set; }

    }
}