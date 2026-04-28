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
namespace Rock.Migrations
{
    using System;

    using Rock.Migrations.Migrations;

    /// <summary>
    ///
    /// </summary>
    public partial class GroupRequirementsImprovements : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            AddColumn("dbo.GroupMemberRequirement", "GroupMemberRequirementState", c => c.Int(nullable: false));

            // Backfill GroupMemberRequirementState to match UpdateGroupMemberRequirementState()
            Sql( @"
                UPDATE gmr
                SET [GroupMemberRequirementState] =
                    CASE
                        -- Meets
                        WHEN gmr.WasOverridden = 1
                          OR gmr.WasManuallyCompleted = 1
                          OR (
                                gmr.RequirementMetDateTime IS NOT NULL
                                AND gmr.RequirementWarningDateTime IS NULL
                             )
                        THEN 0

                        -- Meets with warning
                        WHEN gmr.RequirementWarningDateTime IS NOT NULL
                        THEN 2

                        -- Not met
                        ELSE 1
                    END
                FROM [dbo].[GroupMemberRequirement] gmr
            " );

            AddUpdateGroupRequirementsStoredProcedure();
        }

        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            DropColumn("dbo.GroupMemberRequirement", "GroupMemberRequirementState");
        }

        private void AddUpdateGroupRequirementsStoredProcedure()
        {
            // Ensure these settings are set as expected so they persist with the stored procedure.
            // But first, read their current values to restore them after the migration.
            var isAnsiNullsOn = Convert.ToBoolean( SqlScalar( "SELECT CASE WHEN SESSIONPROPERTY('ANSI_NULLS') = 1 THEN 1 ELSE 0 END;" ) );
            var isQuotedIdentifierOn = Convert.ToBoolean( SqlScalar( "SELECT CASE WHEN SESSIONPROPERTY('QUOTED_IDENTIFIER') = 1 THEN 1 ELSE 0 END;" ) );

            Sql( "SET ANSI_NULLS ON;" );
            Sql( "SET QUOTED_IDENTIFIER ON;" );

            // Add [spUpdateGroupMemberRequirements] (dropping it first if it already exists).
            Sql( @"
IF EXISTS (SELECT * FROM sys.objects WHERE OBJECT_ID = OBJECT_ID(N'[dbo].[spUpdateGroupMemberRequirements]') AND TYPE IN (N'P', N'PC'))
    DROP PROCEDURE [dbo].[spUpdateGroupMemberRequirements];" );

            Sql( RockMigrationSQL._202602062214190_GroupRequirementsImprovements_spUpdateGroupMemberRequiremennts );

            // Restore the original settings.
            Sql( $"SET ANSI_NULLS {( isAnsiNullsOn ? "ON" : "OFF" )};" );
            Sql( $"SET QUOTED_IDENTIFIER {( isQuotedIdentifierOn ? "ON" : "OFF" )};" );
        }
    }
}
