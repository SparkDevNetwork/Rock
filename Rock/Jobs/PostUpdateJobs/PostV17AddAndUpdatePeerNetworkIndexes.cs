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
using System.ComponentModel;

using Rock.Attribute;
using Rock.Data;
using Rock.Model;

namespace Rock.Jobs
{
    /// <summary>
    /// Run once job for v17 to add new and update existing indexes to support the Peer Network feature.
    /// </summary>
    [DisplayName( "Rock Update Helper v17.0 - Add and Update Peer Network Indexes" )]
    [Description( "This job will add new and update existing indexes to support the Peer Network feature." )]

    [IntegerField( "Command Timeout",
        Key = AttributeKey.CommandTimeout,
        Description = "Maximum amount of time (in seconds) to wait for each SQL command to complete. On a large database with lots of communications, this could take several minutes or more.",
        IsRequired = false,
        DefaultIntegerValue = 14400 )]

    public class PostV17AddAndUpdatePeerNetworkIndexes : RockJob
    {
        private static class AttributeKey
        {
            public const string CommandTimeout = "CommandTimeout";
        }

        /// <inheritdoc />
        public override void Execute()
        {
            // Get the configured timeout, or default to 240 minutes if it is blank.
            var commandTimeout = GetAttributeValue( AttributeKey.CommandTimeout ).AsIntegerOrNull() ?? 14400;
            var jobMigration = new JobMigration( commandTimeout );

            ReplaceGroupGroupTypeIdIndex( jobMigration );
            AddPeerNetworkRelationshipScoreIndexForDeletes( jobMigration );
            AddPeerNetworkSourcePersonIdIndexForLavaSql( jobMigration );

            DeleteJob();
        }

        /// <summary>
        /// Removes the existing IX_GroupTypeId index from Group and adds a new IX_GroupTypeId_CampusId index in its
        /// place, along with several includes.
        /// </summary>
        /// <param name="jobMigration">The job migration.</param>
        private void ReplaceGroupGroupTypeIdIndex( JobMigration jobMigration )
        {
            jobMigration.Sql( @"
IF EXISTS (SELECT * FROM sys.indexes WHERE NAME = N'IX_GroupTypeId' AND object_id = OBJECT_ID('Group'))
BEGIN
    DROP INDEX [IX_GroupTypeId] ON [dbo].[Group];
END

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE NAME = N'IX_GroupTypeId_CampusId' AND object_id = OBJECT_ID('Group'))
BEGIN
    CREATE NONCLUSTERED INDEX [IX_GroupTypeId_CampusId] ON [dbo].[Group]
    (
        [GroupTypeId] ASC
        , [CampusId] ASC
    )
    INCLUDE
    (
        [ParentGroupId]
        , [Name]
        , [IsActive]
        , [Order]
        , [Guid]
        , [IsArchived]
        , [StatusValueId]
        , [RelationshipStrengthOverride]
    );
END" );
        }

        /// <summary>
        /// Adds a new IX_RelationshipScore index to PeerNetwork to improve zero-score record deletes within the SPROC.
        /// </summary>
        /// <param name="jobMigration">The job migration.</param>
        private void AddPeerNetworkRelationshipScoreIndexForDeletes( JobMigration jobMigration )
        {
            jobMigration.Sql( @"
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE NAME = N'IX_RelationshipScore' AND object_id = OBJECT_ID('PeerNetwork'))
BEGIN
    CREATE NONCLUSTERED INDEX [IX_RelationshipScore] ON [dbo].[PeerNetwork]
    (
        [RelationshipScore] ASC
    );
END" );
        }

        /// <summary>
        /// Adds a new IX_SourcePersonId index to PeerNetwork to improve Lava SQL performance.
        /// </summary>
        /// <param name="jobMigration">The job migration.</param>
        private void AddPeerNetworkSourcePersonIdIndexForLavaSql( JobMigration jobMigration )
        {
            jobMigration.Sql( @"
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE NAME = N'IX_SourcePersonId' AND object_id = OBJECT_ID('PeerNetwork'))
BEGIN
    CREATE NONCLUSTERED INDEX [IX_SourcePersonId] ON [dbo].[PeerNetwork]
    (
        [SourcePersonId] ASC
    )
    INCLUDE
    (
        [RelationshipTypeValueId]
        , [RelatedEntityId]
        , [RelationshipScore]
        , [RelationshipScoreLastUpdateValue]
        , [RelationshipTrend]
        , [TargetPersonId]
        , [Caption]
    );
END" );
        }

        /// <summary>
        /// Deletes the job.
        /// </summary>
        private void DeleteJob()
        {
            using ( var rockContext = new RockContext() )
            {
                var jobService = new ServiceJobService( rockContext );
                var job = jobService.Get( GetJobId() );

                if ( job != null )
                {
                    jobService.Delete( job );
                    rockContext.SaveChanges();
                }
            }
        }
    }
}
