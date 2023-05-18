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
using System.ComponentModel;using Rock.Attribute;
using Rock.Data;
using Rock.Model;

namespace Rock.Jobs
{
    /// <summary>
    /// A run once job for V13.6 to fix eRA Start Date data broken in v13.4
    /// </summary>
    [DisplayName( "Rock Update Helper v13.6 - Fix Incorrect eRA Start Dates." )]
    [Description( "This job fixes eRA Start Dates (broken in v13.4) for people who are currently eRA." )]

    [IntegerField(
        "Command Timeout",
        AttributeKey.CommandTimeout,
        Description = "Maximum amount of time (in seconds) to wait for each SQL command to complete. On a large database with lots of data, this could take several minutes or more.",
        IsRequired = false,
        DefaultIntegerValue = 3600 )]
    public class PostV136FixIncorrectERAStartDate : RockJob
    {
        private static class AttributeKey
        {
            public const string CommandTimeout = "CommandTimeout";
        }

        /// <inheritdoc cref="RockJob.Execute()"/>
        public override void Execute()
        {
            // get the configured timeout, or default to 60 minutes if it is blank
            var commandTimeout = GetAttributeValue( AttributeKey.CommandTimeout ).AsIntegerOrNull() ?? 3600;

            // If this somehow fails and throws an exeption, the DeleteJob() call below won't run and it will try again later.
            using ( var rockContext = new RockContext() )
            {
                rockContext.Database.CommandTimeout = commandTimeout;
                rockContext.Database.ExecuteSqlCommand( Plugin.HotFixes.HotFixMigrationResource._153_FixERAStartDate_RecoverERAStartDate_Update );
            }

            // Delete the job when this run-once job completes.
            ServiceJobService.DeleteJob( this.GetJobId() );
        }
    }
}
