using System;
using System.Globalization;
using System.Linq;
using timesheet_calculation.Data;
using timesheet_calculation.Common;
using Microsoft.Extensions.Logging;

namespace timesheet_calculation.Business
{
    public class TimeSheetManager
    {
        private readonly TimesheetDbContext _dbContext;
        private readonly ILogger<TimeSheetManager> _logger;

        public TimeSheetManager(TimesheetDbContext dbContext, ILogger<TimeSheetManager> logger)
        {
            _dbContext = dbContext;
            _logger = logger;
        }
        public enum Type
        {
            WORKING = 0,
            DAYOFF = 1,
            HOLIDAY = 2
        }
        public class MarkHolidayModel
        {
            public int Day { get; set; }           
            public int Month { get; set; }
            public int Year { get; set; }
            public int Type { get; set; }
            public string Note { get; set; }
        }     
        public Response CalculateAllDays(int year)
        {
            _logger.LogInformation("Creating Calender...");
            try
            {
                bool isFirstSaturdayWorking = true;
                var lunardate = new ChineseLunisolarCalendar();
                var time = new DateTime(year, 1, 1);
                int numberOfMonth;
                if (lunardate.IsLeapYear(lunardate.GetYear(time)) == true)
                    numberOfMonth = 13;
                else numberOfMonth = 12;
                while (time < (new DateTime(year, 1, 1)).AddYears(1))
                {
                    var result = new im_TimeSheetManager();
                    result.Id = new Guid();
                    result.Day = time.Day;
                    result.Month = time.Month;
                    result.Year = year;
                    if ((time.Day == 1 && time.Month == 1))
                    {
                        result.Type = (int)Type.HOLIDAY;
                        result.Note = "Tết Dương lịch";
                    }
                    else if ((time.Day == 30 && time.Month == 4))
                    {
                        result.Type = (int)Type.HOLIDAY;
                        result.Note = "Giải phóng miền Nam";
                    }
                    else if ((time.Day == 1 && time.Month == 5))
                    {
                        result.Type = (int)Type.HOLIDAY;
                        result.Note = "Quốc tế Lao Động";
                    }
                    else if ((time.Day == 2 && time.Month == 9))
                    {
                        result.Type = (int)Type.HOLIDAY;
                        result.Note = "Quốc Khánh";
                    }
                    else if ((lunardate.GetDayOfMonth(time) == 10 && lunardate.GetMonth(time) == 3))
                    {
                        result.Type = (int)Type.HOLIDAY;
                        result.Note = "Giỗ tổ Hùng Vương";
                    }
                    else if ((lunardate.GetDayOfMonth(time) == 1 || lunardate.GetDayOfMonth(time) == 2
                            || lunardate.GetDayOfMonth(time) == 3 || lunardate.GetDayOfMonth(time) == 4
                            || lunardate.GetDayOfMonth(time) == 5) && lunardate.GetMonth(time) == 1)

                    {
                        result.Type = (int)Type.HOLIDAY;
                        result.Note = "Tết Nguyên Đán";
                    }
                    else if ((lunardate.GetDayOfMonth(time) == 29 && lunardate.GetMonth(time) == numberOfMonth)
                        || (lunardate.GetDayOfMonth(time) == 30 && lunardate.GetMonth(time) == numberOfMonth))
                    {
                        result.Type = (int)Type.HOLIDAY;
                        result.Note = "Tết Nguyên Đán";
                    }
                    else if (time.DayOfWeek == DayOfWeek.Saturday && isFirstSaturdayWorking == false)
                    {
                        result.Type = (int)Type.DAYOFF;
                        isFirstSaturdayWorking = true;
                    }
                    else if (time.DayOfWeek == DayOfWeek.Sunday)
                    {
                        result.Type = (int)Type.DAYOFF;
                    }
                    else
                    {
                        result.Type = (int)Type.WORKING;
                        if (time.DayOfWeek == DayOfWeek.Saturday)
                            isFirstSaturdayWorking = false;
                    }
                    _dbContext.im_TimeSheetManager.Add(result);
                    _dbContext.SaveChanges();
                    time = time.AddDays(1);
                }
                _logger.LogInformation("End creating Calender: Success!");
                return new Response(System.Net.HttpStatusCode.OK, "Create Calender: Success!");
            }
            catch (Exception ex)
            {
                _logger.LogError("Create Calender: Fail! - Error: " + ex);
                return new Response(System.Net.HttpStatusCode.BadRequest, "Create Calender: Fail - Error: " + ex);
            }

        }
        public Response MarkHoliday(MarkHolidayModel model)
        {
            _logger.LogInformation("Mark holiday!");
            try
            {
                var check = _dbContext.im_TimeSheetManager.Where(c => c.Day == model.Day)
                                                      .Where(c => c.Month == model.Month)
                                                      .Where(c => c.Year == model.Year).FirstOrDefault();
                if (check != null)
                {
                    check.Type = model.Type;
                    check.Note = model.Note;
                }
                _dbContext.im_TimeSheetManager.Update(check);
                _dbContext.SaveChanges();
                _logger.LogInformation("Mark holidat: Success!");
                return new Response(System.Net.HttpStatusCode.OK, "Mark holiday: Success!!");
            }
            catch (Exception ex)
            {
                _logger.LogError("Mark holiday: Fail - Error: " + ex);
                return new Response(System.Net.HttpStatusCode.BadRequest, "mark holiday: Fail - Error: " + ex);
            }
        }
    }
}
