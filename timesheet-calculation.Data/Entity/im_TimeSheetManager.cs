using System;
using System.ComponentModel.DataAnnotations;

namespace timesheet_calculation.Data
{
    public class im_TimeSheetManager
    {
        [Key]
        public Guid Id { get; set; }
        [Required]
        public int Day { get; set; }
        [Required]
        public int Month { get; set; }
        [Required]
        public int Year { get; set; }
        [Required]
        public int Type { get; set; }
        public string Note { get; set; }
    }   
}
