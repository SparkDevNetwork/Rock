using System.Linq;
using System.Text;
using Quartz;
using Rock.Attribute;
using Rock.Data;
using Rock.Model;

namespace Rock.Jobs
{
    [DisallowConcurrentExecution]
    [IntegerField( "SQL Command Timeout", "Maximum amount of time (in seconds) to wait for each SQL command to complete. Leave blank to use the default for this job (300 seconds). ", false, 5 * 60, "General", 1, "SqlCommandTimeout" )]
    public class UpdatePersistedDataviews : IJob
    {
        /// <summary> 
        /// Empty constructor for job initialization
        /// <para>
        /// Jobs require a public empty constructor so that the
        /// scheduler can instantiate the class whenever it needs.
        /// </para>
        /// </summary>
        public UpdatePersistedDataviews()
        {
        }

        /// <summary>
        /// Executes the specified context.
        /// </summary>
        /// <param name="context">The context.</param>
        public void Execute( IJobExecutionContext context )
        {
            JobDataMap dataMap = context.JobDetail.JobDataMap;
            int sqlCommandTimeout = dataMap.GetString( "SQLCommandTimeout" ).AsIntegerOrNull() ?? 300;
            StringBuilder results = new StringBuilder();
            int updatedDataViewCount = 0;
            using ( var rockContext = new RockContext() )
            {
                var currentDateTime = RockDateTime.Now;

                // get a list of all the dataviews that need to be refreshed
                var expiredPersistedDataViews = new DataViewService( rockContext ).Queryable()
                    .Where( a => a.PersistedScheduleIntervalMinutes.HasValue )
                    .Where( a =>
                      ( a.PersistedLastRefreshDateTime == null )
                      || ( System.Data.Entity.SqlServer.SqlFunctions.DateAdd( "mi", a.PersistedScheduleIntervalMinutes.Value, a.PersistedLastRefreshDateTime.Value ) < currentDateTime )
                     );

                var expiredPersistedDataViewsList = expiredPersistedDataViews.ToList();
                foreach ( var dataView in expiredPersistedDataViewsList )
                {
                    context.UpdateLastStatusMessage( $"Updating {dataView.Name}" );
                    dataView.PersistResult( sqlCommandTimeout );
                    dataView.PersistedLastRefreshDateTime = RockDateTime.Now;
                    rockContext.SaveChanges();
                    updatedDataViewCount++;
                }
            }

            results.AppendLine( $"Updated {updatedDataViewCount} {"dataview".PluralizeIf( updatedDataViewCount != 1 )}" );
            context.UpdateLastStatusMessage( results.ToString() );
        }
    }
}
