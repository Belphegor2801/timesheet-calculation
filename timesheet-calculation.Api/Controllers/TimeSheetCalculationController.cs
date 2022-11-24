using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using timesheet_calculation.Business;
using timesheet_calculation.Data;
using Newtonsoft.Json;
using static timesheet_calculation.Business.TimeSheetCalculator;
using static timesheet_calculation.Business.TimeSheetManager;
using timesheet_calculation.Common;

namespace timesheet_calculation.Api
{
    [ApiController]
    [Route("api/v1/timesheet")]
    [ApiExplorerSettings(GroupName = "TimeSheet")]
    public class TimeSheetCalculationController : ControllerBase
    {

        private readonly TimeSheetCalculator _timesheet;
        private readonly TimeSheetManager _manager;

        public TimeSheetCalculationController(TimeSheetCalculator timesheet, TimeSheetManager manager)
        {
            _timesheet = timesheet;
            _manager = manager;
        }
        [HttpPost]
        [Route("calculation")]
        [AllowAnonymous]
        public UserMonthlyTimesheetModel TimeSheetCalculation([FromBody] CalculationModel model)
        {
            var result = new UserMonthlyTimesheetModel();
            result = _timesheet.Calculate(model).Data;            
            return result;
        }
        [HttpPost]
        [Route("create")]
        [AllowAnonymous]
        public ActionResult CreateUser()
        {
            Response response = _timesheet.CreateUser();
            return Ok();
        }
        [HttpPost]
        [Route("checkin/{userId}")]
        [AllowAnonymous]
        public ActionResult CheckIn(Guid userId)
        {
            Response response = _timesheet.CheckIn(userId);
            return Ok();
        }
        [HttpPut]
        [Route("checkout/{id}")]
        [AllowAnonymous]
        public ActionResult CheckOut(Guid id)
        {
            Response response = _timesheet.CheckOut(id);
            return Ok();
        }
        [HttpPost]
        [Route("calendar/{year}")]
        [AllowAnonymous]
        public ActionResult CreateCalendar(int year)
        {
            Response response = _manager.CalculateAllDays(year);
            return Ok();
        }
        [HttpPut]
        [Route("calendar/remark")]
        [AllowAnonymous]
        public ActionResult MarkHoliday([FromBody]MarkHolidayModel model)
        {
            Response response = _manager.MarkHoliday(model);
            return Ok();
        }
    }
}
