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
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.Entity;
using System.Linq;

using Quartz;

using Rock.Data;
using Rock.Model;
using Rock.Utility.Enums;

namespace Rock.Jobs
{
    /// <summary>
    /// Processes Elevated Security
    /// </summary>
    /// <seealso cref="Quartz.IJob" />
    [DisplayName( "Process Elevated Security" )]
    [Description( "Updates the person account protection profiles." )]

    [DisallowConcurrentExecution]
    public class ProcessElevatedSecurity : IJob
    {
        #region Constructor

        /// <summary> 
        /// Empty constructor for job initialization
        /// <para>
        /// Jobs require a public empty constructor so that the
        /// scheduler can instantiate the class whenever it needs.
        /// </para>
        /// </summary>
        public ProcessElevatedSecurity()
        {
        }

        #endregion Constructor

        #region fields

        /// <summary>
        /// The job status messages
        /// </summary>
        private List<string> _jobStatusMessages = null;

        #endregion

        /// <summary>
        /// Updates the person account protection profile.
        /// </summary>
        /// <returns></returns>
        private int UpdatePersonAccountProtectionProfile()
        {
            var rowsUpdated = 0;
            using ( var rockContext = new RockContext() )
            {
                rowsUpdated = PersonService.UpdateAccountProtectionProfileAll( rockContext );
            }

            return rowsUpdated;
        }

        /// <summary>
        /// Executes the job to update person records' protection levels per the PersonService.
        /// </summary>
        /// <param name="context">The execution context.</param>
        public void Execute( IJobExecutionContext context )
        {
            _jobStatusMessages = new List<string>();
            var rowsUpdated = UpdatePersonAccountProtectionProfile();
            if ( rowsUpdated > 0 )
            {
                _jobStatusMessages.Add( $"{rowsUpdated} account protection {"profile".PluralizeIf( rowsUpdated > 1 )} updated" );
            }

            if ( _jobStatusMessages.Any() )
            {
                context.UpdateLastStatusMessage( _jobStatusMessages.AsDelimited( ", ", " and " ) );
            }
            else
            {
                context.UpdateLastStatusMessage( "No processed account protection profiles." );
            }
        }
    }
}
