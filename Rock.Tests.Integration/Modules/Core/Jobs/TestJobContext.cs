using System;

using Quartz;

namespace Rock.Tests.Integration.Modules.Core.Jobs
{
    /// <summary>
    /// Test Job Context
    /// </summary>
    /// <seealso cref="Quartz.IJobExecutionContext" />
    internal class TestJobContext : IJobExecutionContext
    {
        /// <summary>
        /// Gets the scheduler.
        /// </summary>
        /// <value>
        /// The scheduler.
        /// </value>
        public IScheduler Scheduler => null;

        /// <summary>
        /// Gets the trigger.
        /// </summary>
        /// <value>
        /// The trigger.
        /// </value>
        public ITrigger Trigger => throw new NotImplementedException();

        /// <summary>
        /// Gets the calendar.
        /// </summary>
        /// <value>
        /// The calendar.
        /// </value>
        public ICalendar Calendar => throw new NotImplementedException();

        /// <summary>
        /// Gets a value indicating whether this <see cref="TestJobContext"/> is recovering.
        /// </summary>
        /// <value>
        ///   <c>true</c> if recovering; otherwise, <c>false</c>.
        /// </value>
        public bool Recovering => throw new NotImplementedException();

        /// <summary>
        /// Gets the refire count.
        /// </summary>
        /// <value>
        /// The refire count.
        /// </value>
        public int RefireCount => throw new NotImplementedException();

        /// <summary>
        /// Gets the merged job data map.
        /// </summary>
        /// <value>
        /// The merged job data map.
        /// </value>
        [Obsolete]
        [RockObsolete( "15.0" )]
        public JobDataMap MergedJobDataMap => throw new NotImplementedException();

        /// <summary>
        /// Gets the job detail.
        /// </summary>
        /// <value>
        /// The job detail.
        /// </value>
        [Obsolete]
        [RockObsolete( "15.0" )]
        public IJobDetail JobDetail { get; set; } = new TestJobDetail();

        /// <summary>
        /// Gets the job instance.
        /// </summary>
        /// <value>
        /// The job instance.
        /// </value>
        [Obsolete]
        [RockObsolete( "15.0" )]
        public IJob JobInstance => throw new NotImplementedException();

        /// <summary>
        /// Gets the fire time UTC.
        /// </summary>
        /// <value>
        /// The fire time UTC.
        /// </value>
        public DateTimeOffset? FireTimeUtc => throw new NotImplementedException();

        /// <summary>
        /// Gets the scheduled fire time UTC.
        /// </summary>
        /// <value>
        /// The scheduled fire time UTC.
        /// </value>
        public DateTimeOffset? ScheduledFireTimeUtc => throw new NotImplementedException();

        /// <summary>
        /// Gets the previous fire time UTC.
        /// </summary>
        /// <value>
        /// The previous fire time UTC.
        /// </value>
        public DateTimeOffset? PreviousFireTimeUtc => throw new NotImplementedException();

        /// <summary>
        /// Gets the next fire time UTC.
        /// </summary>
        /// <value>
        /// The next fire time UTC.
        /// </value>
        public DateTimeOffset? NextFireTimeUtc => throw new NotImplementedException();

        /// <summary>
        /// Gets the fire instance identifier.
        /// </summary>
        /// <value>
        /// The fire instance identifier.
        /// </value>
        public string FireInstanceId => throw new NotImplementedException();

        /// <summary>
        /// Gets or sets the result.
        /// </summary>
        /// <value>
        /// The result.
        /// </value>
        public object Result { get; set; }

        /// <summary>
        /// Gets the job run time.
        /// </summary>
        /// <value>
        /// The job run time.
        /// </value>
        public TimeSpan JobRunTime => throw new NotImplementedException();

        /// <summary>
        /// Gets the specified key.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public object Get( object key )
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Puts the specified key.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="objectValue">The object value.</param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public void Put( object key, object objectValue )
        {
            throw new NotImplementedException();
        }
    }
}
