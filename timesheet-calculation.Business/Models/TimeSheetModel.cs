using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace timesheet_calculation.Business
{
    public class UserDailyTimesheetModel
    {
        public Guid UserId { get; set; }
        public int Day { get; set; }
        public int Month { get; set; }
        public int Year { get; set; }
        public bool IsLate { get; set; }
        public int TotalLateInSeconds { get; set; }
        public int TotalActualWorkingTimeInSeconds { get; set; }
        public string Status { get; set; }
    }

    public class UserMonthlyTimesheetModel
    {
        public Guid UserId { get; set; }
        public int TotalWorkingHours { get; set; }
        public decimal TotalActualWorkingHours { get; set; }
        public List<UserDailyTimesheetModel> DailyTimesheets { get; set; }
    }
}
