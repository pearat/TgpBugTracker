using System;
using System.ComponentModel.DataAnnotations;

namespace TgpBugTracker.Models
{
    public class Comment
    {
        [Required]
        public int Id { get; set; }
        [DataType(DataType.Date)]
        [Display(Name = "Comment Date")]
        [DisplayFormat(DataFormatString = "{0:MM/dd}", ApplyFormatInEditMode = true)]
        public DateTimeOffset Date { get; set; }
        
        public string Detail { get; set; }
        public string MediaURL { get; set; }
        [Required]
        public string Title { get; set; }
        
        public string AuthorId { get; set; }
        
        public int TicketId { get; set; }

        public virtual Ticket Ticket { get; set; }
    }
}