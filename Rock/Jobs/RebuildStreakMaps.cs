// <copyright>
// Copyright by the Spark Development Network
//
// Licensed under the Rock Community License (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.rockrms.com/license
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
// </copyright>
//
using System;
using System.Web;
using Quartz;
using Rock.Model;

namespace Rock.Jobs
{
    /// <summary>
    /// Uses <see cref="StreakTypeService.RebuildStreakTypeFromAttendance"/> to rebuild streak maps.
    /// </summary>
    [DisallowConcurrentExecution]
    public class RebuildStreakMaps : IJob
    {
        /// <summary>
        /// Keys for the data map
        /// </summary>
        public static class DataMapKey
        {
            /// <summary>
            /// The streak type identifier
            /// </summary>
            public const string StreakTypeId = "StreakTypeId";
        }

        /// <summary> 
        /// Empty constructor for job initialization
        /// <para>
        /// Jobs require a public empty constructor so that the
        /// scheduler can instantiate the class whenever it needs.
        /// </para>
        /// </summary>
        public RebuildStreakMaps()
        {
        }

        /// <summary>
        /// Executes the specified context.
        /// </summary>
        /// <param name="context">The context.</param>
        public void Execute( IJobExecutionContext context )
        {
            var streakTypeId = context.JobDetail.JobDataMap.GetString( DataMapKey.StreakTypeId ).AsInteger();
            StreakTypeService.RebuildStreakTypeFromAttendance( streakTypeId, out var errorMessage );

            if ( errorMessage.IsNullOrWhiteSpace() )
            {
                context.Result = string.Format( "Streak maps have been rebuilt for streak type id {0}", streakTypeId );
                return;
            }

            var exception = new Exception( errorMessage );
            var context2 = HttpContext.Current;
            ExceptionLogService.LogException( exception, context2 );

            throw exception;
        }
    }
}
