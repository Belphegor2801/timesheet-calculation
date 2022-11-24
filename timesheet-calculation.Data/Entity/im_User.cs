using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace timesheet_calculation.Data
{
    public class im_User
    {
        [Key]
        [Required]
        public Guid UserId { get; set; }
        public string Name { get; set; }
        public virtual ICollection<im_TimeSheet> TimeSheets { get; set; }
    }
}
