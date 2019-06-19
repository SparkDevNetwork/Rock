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
using System.Data.Entity;
using System.Linq;

using Quartz;

using Rock.Attribute;
using Rock.Data;
using Rock.Model;
using Rock.Web.Cache;

namespace Rock.Jobs
{
    /// <summary>
    /// 
    /// </summary>
    /// <seealso cref="Quartz.IJob" />
    [DisallowConcurrentExecution]
    [IntegerField( "How Many Records", "The number of attribute records to process on each run of this job.", false, 500000, "", 0, "HowMany" )]
    [IntegerField( "Command Timeout", "Maximum amount of time (in seconds) to wait for the SQL Query to complete. Leave blank to use the default for this job (3600). Note, it could take several minutes, so you might want to set it at 3600 (60 minutes) or higher", false, 60 * 60, "General", 1, "CommandTimeout" )]
    public class MigrateFamilyAlternateId : IJob
    {
        private const string _attributeGuid = "8F528431-A438-4488-8DC3-CA42E66C1B37";

        /// <summary>
        /// Executes the specified context.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <exception cref="System.NotImplementedException"></exception>

        public void Execute( IJobExecutionContext context )
        {
            JobDataMap dataMap = context.JobDetail.JobDataMap;

            int howMany = dataMap.GetString( "HowMany" ).AsIntegerOrNull() ?? 500000;
            var commandTimeout = dataMap.GetString( "CommandTimeout" ).AsIntegerOrNull() ?? 3600;

            bool anyRemaining = UpdateSearchValueRecords( context, howMany, commandTimeout );

            if ( !anyRemaining )
            {
                // Verify that there are not any history records that haven't been migrated
                using ( var rockContext = new RockContext() )
                {
                    rockContext.Database.CommandTimeout = commandTimeout;

                    var attributeService = new AttributeService( rockContext );
                    var attribute = attributeService.Get( "8F528431-A438-4488-8DC3-CA42E66C1B37".AsGuid() );
                    bool valuesExist = attribute != null;
                    if ( valuesExist )
                    {
                        valuesExist = new AttributeValueService( rockContext )
                            .Queryable().AsNoTracking()
                            .Where( a => a.AttributeId == attribute.Id )
                            .Any();
                    }

                    if ( !valuesExist )
                    { 
                        // Delete the attribute
                        if ( attribute != null )
                        {
                            attributeService.Delete( attribute );
                        }

                        // delete job if there are no un-migrated history rows  left
                        var jobId = context.GetJobId();
                        var jobService = new ServiceJobService( rockContext );
                        var job = jobService.Get( jobId );
                        if ( job != null )
                        {
                            jobService.Delete( job );
                            rockContext.SaveChanges();
                            return;
                        }
                    }
                }
            }
        }

        private bool UpdateSearchValueRecords( IJobExecutionContext context, int howManyToConvert, int commandTimeout )
        {
            bool anyRemaining = true;

            int howManyLeft = howManyToConvert;

            var attribute = AttributeCache.Get( "8F528431-A438-4488-8DC3-CA42E66C1B37".AsGuid() );
            var searchTypeValue = DefinedValueCache.Get( SystemGuid.DefinedValue.PERSON_SEARCH_KEYS_ALTERNATE_ID.AsGuid() );
            if ( attribute != null && searchTypeValue != null )
            {
                while ( howManyLeft > 0 )
                {
                    using ( var rockContext = new RockContext() )
                    {
                        var groupMemberService = new GroupMemberService( rockContext );
                        var attributeValueService = new AttributeValueService( rockContext );
                        var personSearchKeyService = new PersonSearchKeyService( rockContext );

                        int take = howManyLeft < 100 ? howManyLeft : 100;

                        var attributeValueRecords = attributeValueService
                            .Queryable()
                            .Where( v =>
                                v.AttributeId == attribute.Id &&
                                v.EntityId.HasValue )
                            .OrderBy( v => v.Id )
                            .Take( take )
                            .ToList();

                        anyRemaining = attributeValueRecords.Count >= take;
                        howManyLeft = anyRemaining ? howManyLeft - take : 0;

                        foreach ( var attributevalueRecord in attributeValueRecords )
                        {
                            var hoh = groupMemberService
                                .Queryable()
                                .Where( m => m.GroupId == attributevalueRecord.EntityId.Value )
                                .HeadOfHousehold();

                            if ( hoh != null && hoh.PrimaryAlias != null )
                            {
                                var keys = attributevalueRecord.Value.Split( new char[] { '|' }, StringSplitOptions.RemoveEmptyEntries );
                                foreach( var key in keys )
                                {
                                    var searchValue = new PersonSearchKey
                                    {
                                        PersonAliasId = hoh.PrimaryAlias.Id,
                                        SearchTypeValueId = searchTypeValue.Id,
                                        SearchValue = key
                                    };
                                    personSearchKeyService.Add( searchValue );
                                }
                            }

                            attributeValueService.Delete( attributevalueRecord );

                            rockContext.SaveChanges();
                        }

                        int numberMigrated = howManyToConvert - howManyLeft;
                        var percentComplete = howManyToConvert > 0 ? ( numberMigrated * 100.0 ) / howManyToConvert : 100.0;
                        var statusMessage = $@"Progress: {numberMigrated} of {howManyToConvert} ({Math.Round( percentComplete, 1 )}%) Family Check-in Identifiers migrated to person search key values";
                        context.UpdateLastStatusMessage( statusMessage );

                        rockContext.SaveChanges( disablePrePostProcessing: true );
                    }
                }
            }

            return anyRemaining;
        }

    }
}
