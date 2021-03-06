﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace iSpeak.Models
{
    public class NewStudentViewModels
    {
        [Display(Name = "Student Name")]
        public string Name { get; set; }
        [Display(Name = "Lesson Package Qty")]
        public int Qty { get; set; }
    }

    public class NewStudentBeforeViewModels
    {
        public string StudentId { get; set; }
        public string Name { get; set; }
        public int Qty { get; set; }
    }
}