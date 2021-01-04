using System;
using Quartz;

namespace Rock.Tests.Integration.Jobs
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
        /// <exception cref="NotImplementedException"></exception>
        public JobKey Key => throw new NotImplementedException();

        /// <summary>
        /// Gets the description.
        /// </summary>
        /// <value>
        /// The description.
        /// </value>
        /// <exception cref="NotImplementedException"></exception>
        public string Description => throw new NotImplementedException();

        /// <summary>
        /// Gets the type of the job.
        /// </summary>
        /// <value>
        /// The type of the job.
        /// </value>
        /// <exception cref="NotImplementedException"></exception>
        public Type JobType => throw new NotImplementedException();

        /// <summary>
        /// Gets the job data map.
        /// </summary>
        /// <value>
        /// The job data map.
        /// </value>
        public JobDataMap JobDataMap { get; set; } = new JobDataMap();

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
            throw new NotImplementedException();
        }
    }
}
