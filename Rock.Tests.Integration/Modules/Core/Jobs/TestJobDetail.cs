using System;

using Quartz;

namespace Rock.Tests.Integration.Modules.Core.Jobs
{
    /// <summary>
    /// Test Job Detail
    /// </summary>
    /// <seealso cref="Quartz.IJobDetail" />
    internal class TestJobDetail : IJobDetail
    {
        /// <summary>
        /// Gets the key.
        /// </summary>
        /// <value>
        /// The key.
        /// </value>
        public JobKey Key => new JobKey( "TestJob" );

        /// <summary>
        /// Gets the description.
        /// </summary>
        /// <value>
        /// The description.
        /// </value>
        public string Description => "Test Job Description";

        /// <summary>
        /// Gets the type of the job.
        /// </summary>
        /// <value>
        /// The type of the job.
        /// </value>
        public Type JobType => typeof( TestJobDetail );

        /// <summary>
        /// Gets the job data map.
        /// </summary>
        /// <value>
        /// The job data map.
        /// </value>
#pragma warning disable CS0618 // Type or member is obsolete
        public JobDataMap JobDataMap { get; set; } = new JobDataMap();
#pragma warning restore CS0618 // Type or member is obsolete

        /// <summary>
        /// Gets a value indicating whether this <see cref="TestJobDetail"/> is durable.
        /// </summary>
        /// <value>
        ///   <c>true</c> if durable; otherwise, <c>false</c>.
        /// </value>
        /// <exception cref="NotImplementedException"></exception>
        public bool Durable => throw new NotImplementedException();

        /// <summary>
        /// Gets a value indicating whether [persist job data after execution].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [persist job data after execution]; otherwise, <c>false</c>.
        /// </value>
        /// <exception cref="NotImplementedException"></exception>
        public bool PersistJobDataAfterExecution => throw new NotImplementedException();

        /// <summary>
        /// Gets a value indicating whether [concurrent exection disallowed].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [concurrent exection disallowed]; otherwise, <c>false</c>.
        /// </value>
        /// <exception cref="NotImplementedException"></exception>
        public bool ConcurrentExectionDisallowed => throw new NotImplementedException();

        /// <summary>
        /// Gets a value indicating whether [requests recovery].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [requests recovery]; otherwise, <c>false</c>.
        /// </value>
        /// <exception cref="NotImplementedException"></exception>
        public bool RequestsRecovery => throw new NotImplementedException();

        /// <summary>
        /// Creates a new object that is a copy of the current instance.
        /// </summary>
        /// <returns>
        /// A new object that is a copy of this instance.
        /// </returns>
        /// <exception cref="NotImplementedException"></exception>
        public object Clone()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Gets the job builder.
        /// </summary>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public JobBuilder GetJobBuilder()
        {
            return null;
        }
    }
}
