using System;
using System.ComponentModel.DataAnnotations;

namespace TgpBugTracker.Models
{
    public class Comment
    {
        [Required]
        public int Id { get; set; }
        [Required]
        [DataType(DataType.Date)]
        [Display(Name = "Tkt Date")]
        [DisplayFormat(DataFormatString = "{0:MM/dd}", ApplyFormatInEditMode = true)]
        public DateTimeOffset Date { get; set; }
        [Required]
        public string Detail { get; set; }
        public string MediaURL { get; set; }
        public string Title { get; set; }
        [Required]
        public string AuthorId { get; set; }
        [Required]
        public int TicketId { get; set; }

        public virtual Ticket Ticket { get; set; }
    }
}