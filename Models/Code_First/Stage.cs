﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace TgpBugTracker.Models
{
    public class Stage
    {
        [Required]
        public int Id { get; set; }
        public string Name { get; set; }
    }
}