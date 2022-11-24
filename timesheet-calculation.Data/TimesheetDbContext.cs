using Microsoft.EntityFrameworkCore;
using System;
using timesheet_calculation.Common;

namespace timesheet_calculation.Data
{
    public class TimesheetDbContext : DbContext
    {
        public TimesheetDbContext(DbContextOptions<TimesheetDbContext> options) : base(options)
        {
        }

        private string connectionString;

        public TimesheetDbContext()
        {
            connectionString = Utils.GetConfig("ConnectionStrings:PostgreSQLDatabase");
        }
        public virtual DbSet<im_TimeSheet> im_TimeSheet { get; set; }
        public virtual DbSet<im_User> im_User { get; set; }
        public virtual DbSet<im_TimeSheetManager> im_TimeSheetManager { get; set; }
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                optionsBuilder.UseNpgsql(connectionString);
            }
        }
    }
}
