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
    public partial class UpdateGroupRequirementsJob : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            CreateIdListUserDefinedTableType();
            UpdateStepFlowStoredProcedure();
            AddUpdateGroupRequirementsStoredProcedure();
            RemoveUnusedEntityIdListType();
        }

        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
        }

        private void CreateIdListUserDefinedTableType()
        {
            Sql( @"
CREATE TYPE dbo.IdList AS TABLE (
    Id INT NOT NULL PRIMARY KEY
);" );
        }

        private void UpdateStepFlowStoredProcedure()
        {
            // Ensure these settings are set as expected so they persist with the stored procedure.
            // But first, read their current values to restore them after the migration.
            var isAnsiNullsOn = Convert.ToBoolean( SqlScalar( "SELECT CASE WHEN SESSIONPROPERTY('ANSI_NULLS') = 1 THEN 1 ELSE 0 END;" ) );
            var isQuotedIdentifierOn = Convert.ToBoolean( SqlScalar( "SELECT CASE WHEN SESSIONPROPERTY('QUOTED_IDENTIFIER') = 1 THEN 1 ELSE 0 END;" ) );

            Sql( "SET ANSI_NULLS ON;" );
            Sql( "SET QUOTED_IDENTIFIER ON;" );

            // Add [spSteps_StepFlow] (dropping it first if it already exists).
            Sql( @"
IF EXISTS (SELECT * FROM sys.objects WHERE OBJECT_ID = OBJECT_ID(N'[dbo].[spSteps_StepFlow]') AND TYPE IN (N'P', N'PC'))
    DROP PROCEDURE [dbo].[spSteps_StepFlow];" );

            Sql( RockMigrationSQL._202512121548566_UpdateGroupRequirementsJob_spSteps_StepFlow );

            // Restore the original settings.
            Sql( $"SET ANSI_NULLS {( isAnsiNullsOn ? "ON" : "OFF" )};" );
            Sql( $"SET QUOTED_IDENTIFIER {( isQuotedIdentifierOn ? "ON" : "OFF" )};" );
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

            Sql( RockMigrationSQL._202512121548566_UpdateGroupRequirementsJob_spUpdateGroupMemberRequirements );

            // Restore the original settings.
            Sql( $"SET ANSI_NULLS {( isAnsiNullsOn ? "ON" : "OFF" )};" );
            Sql( $"SET QUOTED_IDENTIFIER {( isQuotedIdentifierOn ? "ON" : "OFF" )};" );
        }

        private void RemoveUnusedEntityIdListType()
        {
            Sql( @"
IF EXISTS (
    SELECT 1
    FROM sys.types
    WHERE is_table_type = 1
      AND name = 'EntityIdList'
      AND schema_id = SCHEMA_ID('dbo')
)
AND NOT EXISTS (
    SELECT 1
    FROM sys.parameters p
    INNER JOIN sys.types t
        ON p.user_type_id = t.user_type_id
    WHERE t.name = 'EntityIdList'
      AND SCHEMA_NAME(t.schema_id) = 'dbo'
)
BEGIN
    DROP TYPE [dbo].[EntityIdList];
END" );
        }
    }
}
