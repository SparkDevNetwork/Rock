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

using System;

namespace Rock.Plugin.HotFixes
{
    /// <summary>
    /// Plug-in migration
    /// </summary>
    /// <seealso cref="Rock.Plugin.Migration" />
    [MigrationNumber( 277, "18.2" )]
    public class ExcludeArchivedAndDuplicateListMembers : Migration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            JPH_spCommunication_SynchronizeListRecipients_ExcludeArchivedAndDuplicateListMembersUp_20260204();
        }

        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            JPH_spCommunication_SynchronizeListRecipients_ExcludeArchivedAndDuplicateListMembersDown_20260204();
        }

        /// <summary>
        /// JPH - spCommunication_SynchronizeListRecipients - Exclude Archived and Duplicate List Members - Up.
        /// </summary>
        private void JPH_spCommunication_SynchronizeListRecipients_ExcludeArchivedAndDuplicateListMembersUp_20260204()
        {
            // Ensure these settings are set as expected so they persist with the stored procedure.
            // But first, read their current values to restore them after the migration.
            var isAnsiNullsOn = Convert.ToBoolean( SqlScalar( "SELECT CASE WHEN SESSIONPROPERTY('ANSI_NULLS') = 1 THEN 1 ELSE 0 END;" ) );
            var isQuotedIdentifierOn = Convert.ToBoolean( SqlScalar( "SELECT CASE WHEN SESSIONPROPERTY('QUOTED_IDENTIFIER') = 1 THEN 1 ELSE 0 END;" ) );

            Sql( "SET ANSI_NULLS ON;" );
            Sql( "SET QUOTED_IDENTIFIER ON;" );

            // Add [spCommunication_SynchronizeListRecipients] (dropping it first if it already exists).
            Sql( @"
IF EXISTS (SELECT * FROM sys.objects WHERE OBJECT_ID = OBJECT_ID(N'[dbo].[spCommunication_SynchronizeListRecipients]') AND TYPE IN (N'P', N'PC'))
    DROP PROCEDURE [dbo].[spCommunication_SynchronizeListRecipients];" );

            Sql( HotFixMigrationResource._277_ExcludeArchivedAndDuplicateListMembers_spCommunication_SynchronizeListRecipients );

            // Restore the original settings.
            Sql( $"SET ANSI_NULLS {( isAnsiNullsOn ? "ON" : "OFF" )};" );
            Sql( $"SET QUOTED_IDENTIFIER {( isQuotedIdentifierOn ? "ON" : "OFF" )};" );
        }

        /// <summary>
        /// JPH - spCommunication_SynchronizeListRecipients - Exclude Archived and Duplicate List Members - Down.
        /// </summary>
        private void JPH_spCommunication_SynchronizeListRecipients_ExcludeArchivedAndDuplicateListMembersDown_20260204()
        {
            // Ensure these settings are set as expected so they persist with the stored procedure.
            // But first, read their current values to restore them after the migration.
            var isAnsiNullsOn = Convert.ToBoolean( SqlScalar( "SELECT CASE WHEN SESSIONPROPERTY('ANSI_NULLS') = 1 THEN 1 ELSE 0 END;" ) );
            var isQuotedIdentifierOn = Convert.ToBoolean( SqlScalar( "SELECT CASE WHEN SESSIONPROPERTY('QUOTED_IDENTIFIER') = 1 THEN 1 ELSE 0 END;" ) );

            Sql( "SET ANSI_NULLS ON;" );
            Sql( "SET QUOTED_IDENTIFIER ON;" );

            // Add [spCommunication_SynchronizeListRecipients] (dropping it first if it already exists).
            Sql( @"
IF EXISTS (SELECT * FROM sys.objects WHERE OBJECT_ID = OBJECT_ID(N'[dbo].[spCommunication_SynchronizeListRecipients]') AND TYPE IN (N'P', N'PC'))
    DROP PROCEDURE [dbo].[spCommunication_SynchronizeListRecipients];" );

            Sql( HotFixMigrationResource._268_ImproveSyncListRecipientsPerformance_spCommunication_SynchronizeListRecipients );

            // Restore the original settings.
            Sql( $"SET ANSI_NULLS {( isAnsiNullsOn ? "ON" : "OFF" )};" );
            Sql( $"SET QUOTED_IDENTIFIER {( isQuotedIdentifierOn ? "ON" : "OFF" )};" );
        }
    }
}
