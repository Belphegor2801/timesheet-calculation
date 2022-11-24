using Microsoft.Extensions.Configuration;
using System;
using System.Linq;
using timesheet_calculation.Data;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Globalization;
using timesheet_calculation.Common;
using Microsoft.Extensions.Logging;


namespace timesheet_calculation.Business
{
    public class TimeSheetCalculator
    {        
        private readonly TimesheetDbContext _dbContext;
        private readonly ILogger<TimeSheetCalculator> _logger;

        public TimeSheetCalculator(TimesheetDbContext dbContext, ILogger<TimeSheetCalculator> logger)
        {           
            _dbContext = dbContext;
            _logger = logger;
        }
        public bool isLeap(int year)
        {
            var check = new DateTime(year,12,31);
            if (check.DayOfYear > 365)
                return true;
            else return false;
        }

        public int dayOfMonth(int month, int year)
        {
            if (month == 1 || month == 3 || month == 5
                          || month == 7 || month == 8
                          || month == 10 || month == 12)
                return 31;
            else if (month == 4 || month == 6
                  || month == 9 || month == 1)
                return 30;
            else if (month == 2)
            {
                if (isLeap(year) == true)
                    return 29;
                else return 28;
            }
            else return 0;
        }
        public Response CreateUser()
        {
            try
            {
                _logger.LogInformation("Create User");
                var user = new im_User()
                {
                    UserId = new Guid(),
                    Name = "Trung"
                };
                _dbContext.im_User.Add(user);
                _dbContext.SaveChanges();
                _logger.LogInformation("Create User: Success!");
                return new Response(System.Net.HttpStatusCode.OK, "Create User: Success!");
            }
            catch (Exception ex)
            {
                _logger.LogError("Create User: Fail! - Error: " + ex);
                return new Response(System.Net.HttpStatusCode.BadRequest, "Create User: Fail - Error: " + ex);
            }


        }
        public Response CheckIn(Guid userId)
        {
            _logger.LogInformation("Check in!");
            var check = _dbContext.im_User.Where(c => c.UserId == userId).FirstOrDefault();
            if (check == null) return new ResponseError(System.Net.HttpStatusCode.NotFound, "UserId not found!");

            try
            {
                var user = new im_TimeSheet();
                user.Id = new Guid();
                user.CheckInTime = DateTime.Now;
                user.UserId = userId;
                _dbContext.im_TimeSheet.Add(user);
                _dbContext.SaveChanges();

                _logger.LogInformation("Check in: Success!!");
                return new Response(System.Net.HttpStatusCode.OK, "Check in: Success!!");
            }

            catch (Exception ex)
            {
                _logger.LogError("Check in: Fail!! - Error: " + ex);
                return new Response(System.Net.HttpStatusCode.BadRequest, "Check in: Fail - Error: " + ex);
            }        
        }
        public Response CheckOut(Guid id)
        {
            _logger.LogInformation("Check out!");
            var user = _dbContext.im_TimeSheet.Where(u => u.Id == id)
                                                .FirstOrDefault();
            if (user == null) 
            {
                _logger.LogError("UserId not Found!");
                _logger.LogInformation("Check out: Fail!!");
                return new ResponseError(System.Net.HttpStatusCode.NotFound, "UserId not found!");
            } 

            try
            {
                user.CheckOutTime = DateTime.Now;
                _dbContext.SaveChanges();
                _logger.LogInformation("Check out: Success!!");
                return new Response(System.Net.HttpStatusCode.OK, "Check out: Success!");
            }
            catch (Exception ex)
            {
                _logger.LogError("Check out: Fail!! - Error: " + ex);
                return new Response(System.Net.HttpStatusCode.BadRequest, "Check ouw: Fail - Error: " + ex);
            }

        }       
        public class CalculationModel
        {
            public Guid UserId { get; set; }
            public int Month { get; set; }
            public int Year { get; set; }        
            public bool isFirstSaturdayWorking { get; set; }        
        }
        public Response<UserMonthlyTimesheetModel> Calculate(CalculationModel model)
        {
            try
            {
                var user = _dbContext.im_TimeSheet.Where(u => u.UserId == model.UserId).FirstOrDefault();
                if (user == null)
                    return new Response<UserMonthlyTimesheetModel>(System.Net.HttpStatusCode.NotFound, null, "User not found!!");
                var checkInTime = _dbContext.im_TimeSheet.Where(u => u.CheckInTime.Value.Year == model.Year)
                                                        .Where(u => u.CheckInTime.Value.Month == model.Month)
                                                        .ToList();            
                var days = dayOfMonth(model.Month, model.Year);
                if (DateTime.Now.Day < days)
                    days = DateTime.Now.Day;
                var checkInTimeConstant = new TimeSpan(8, 30, 0);
                var breakTimeConstant = new TimeSpan(12, 0, 0);
                var afternoonTimeConstant = new TimeSpan(13, 30, 0);                
                var monthly = new UserMonthlyTimesheetModel();
                monthly.DailyTimesheets = new List<UserDailyTimesheetModel>();
                for (int i = 1; i <= days; i++)
                {
                    var daily = new UserDailyTimesheetModel();
                    var date = new DateTime(model.Year, model.Month, i);
                    var typeOfDay = _dbContext.im_TimeSheetManager.Where(u => u.Year == model.Year)
                                                               .Where(u => u.Month == model.Month)
                                                               .Where(u => u.Day == i).FirstOrDefault();                    
                    if (typeOfDay.Type == 1 || typeOfDay.Type == 2)
                    {
                        daily.Day = i;
                        daily.Month = model.Month;
                        daily.Year = model.Year;
                        daily.IsLate = false;
                        daily.TotalActualWorkingTimeInSeconds = 0;
                        daily.TotalLateInSeconds = 0;
                        daily.Status = "FREE";
                        model.isFirstSaturdayWorking = true;
                        monthly.DailyTimesheets.Add(daily);
                    }
                    else
                    {
                        var time = _dbContext.im_TimeSheet.Where(u => u.CheckInTime.Value.Year == model.Year)
                                                                            .Where(u => u.CheckInTime.Value.Month == model.Month)
                                                                            .Where(u => u.CheckInTime.Value.Day == i)
                                                                            .FirstOrDefault();                        
                        if(time == null)
                        {
                            daily.Day = i;
                            daily.Month = model.Month;
                            daily.Year = model.Year;
                            daily.Status = "ABSENT";
                            daily.TotalActualWorkingTimeInSeconds = 0;
                            daily.TotalLateInSeconds = 0;
                            daily.IsLate = false;
                            monthly.DailyTimesheets.Add(daily);                            
                        }
                        else
                        {
                            if (time.CheckInTime.Value.TimeOfDay > checkInTimeConstant && time.CheckInTime.Value.TimeOfDay < breakTimeConstant)
                            {
                                daily.IsLate = true;
                                daily.TotalLateInSeconds = (int)(time.CheckInTime.Value.TimeOfDay.TotalSeconds - checkInTimeConstant.TotalSeconds);
                                daily.TotalActualWorkingTimeInSeconds = (int)(breakTimeConstant.TotalSeconds - time.CheckInTime.Value.TimeOfDay.TotalSeconds)
                                                                    + (int)(time.CheckOutTime.Value.TimeOfDay.TotalSeconds - afternoonTimeConstant.TotalSeconds);
                            }
                            else if(time.CheckInTime.Value.TimeOfDay < afternoonTimeConstant && time.CheckInTime.Value.TimeOfDay > breakTimeConstant)
                            {
                                daily.IsLate = true;
                                daily.TotalLateInSeconds = (int)(time.CheckInTime.Value.TimeOfDay.TotalSeconds - checkInTimeConstant.TotalSeconds);
                                daily.TotalActualWorkingTimeInSeconds = (int)(time.CheckOutTime.Value.TimeOfDay.TotalSeconds - afternoonTimeConstant.TotalSeconds);
                            }    
                            else if (time.CheckInTime.Value.TimeOfDay > afternoonTimeConstant)
                            {
                                daily.IsLate = true;
                                daily.TotalLateInSeconds = (int)(time.CheckInTime.Value.TimeOfDay.TotalSeconds - checkInTimeConstant.TotalSeconds);
                                daily.TotalActualWorkingTimeInSeconds = (int)(time.CheckOutTime.Value.TimeOfDay.TotalSeconds - time.CheckInTime.Value.TimeOfDay.TotalSeconds);
                            }    
                            else
                            {
                                daily.TotalLateInSeconds = 0;
                                daily.TotalActualWorkingTimeInSeconds = (int)(breakTimeConstant.TotalSeconds - time.CheckInTime.Value.TimeOfDay.TotalSeconds)
                                                                    + (int)(time.CheckOutTime.Value.TimeOfDay.TotalSeconds - afternoonTimeConstant.TotalSeconds);
                            }                                                        

                            if (daily.TotalActualWorkingTimeInSeconds >= 28800 && daily.IsLate == false && time.CheckInTime != null && time.CheckOutTime != null)
                            {
                                daily.Day = time.CheckInTime.Value.Day;
                                daily.Month = time.CheckInTime.Value.Month;
                                daily.Year = time.CheckInTime.Value.Year;                                
                                daily.Status = "VALID";
                            }
                            else if(time.CheckInTime != null && time.CheckOutTime != null)
                            {
                                daily.Day = time.CheckInTime.Value.Day;
                                daily.Month = time.CheckInTime.Value.Month;
                                daily.Year = time.CheckInTime.Value.Year;
                                daily.Status = "DONE";
                            }                   

                            else if (time.CheckInTime != null && time.CheckOutTime == null)
                            {
                                daily.Day = time.CheckInTime.Value.Day;
                                daily.Month = time.CheckInTime.Value.Month;
                                daily.Year = time.CheckInTime.Value.Year;
                                daily.Status = "INPROCESS";
                            }
                            monthly.DailyTimesheets.Add(daily);
                        }                                                
                        monthly.TotalActualWorkingHours += daily.TotalActualWorkingTimeInSeconds;
                    }
                }                       
                monthly.TotalWorkingHours = checkInTime.Count * 8;
                monthly.TotalActualWorkingHours = Math.Round(monthly.TotalActualWorkingHours/3600, 2);
                return new Response<UserMonthlyTimesheetModel>(System.Net.HttpStatusCode.OK, monthly, "OK");
            }
            catch(Exception ex)
            {
                return null;
            }
        }       
    }
}



