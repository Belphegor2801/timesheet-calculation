using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace timesheet_calculation.Data
{
    public class im_TimeSheet
    {
        [Key]
        [Required]
        public Guid Id { get; set; }
        public DateTime? CheckInTime { get; set; }
        public DateTime? CheckOutTime { get; set; }
        [Required]
        [ForeignKey("im_User")]
        public Guid UserId { get; set; }
        public virtual im_User User { get; set; }
    }
}
