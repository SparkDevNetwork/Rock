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
    public partial class AddGivingAutomationProcedures : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            CreateAttributeValueUpdateListUserDefinedTableType();
            AddUpdateGivingBinsAndPercentilesProcedure();
            AddUpdateGivingJourneyStagesProcedure();
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
        }

        #region Private Methods

        private void CreateAttributeValueUpdateListUserDefinedTableType()
        {
            Sql( @"
CREATE TYPE dbo.AttributeValueUpdateList AS TABLE
(
    EntityId    INT            NOT NULL,
    AttributeId INT            NOT NULL,
    Value       NVARCHAR(4000) NOT NULL,

    PRIMARY KEY NONCLUSTERED (AttributeId, EntityId)
);" );
        }

        private void AddUpdateGivingBinsAndPercentilesProcedure()
        {
            // Ensure these settings are set as expected so they persist with the stored procedure.
            // But first, read their current values to restore them after the migration.
            var isAnsiNullsOn = Convert.ToBoolean( SqlScalar( "SELECT CASE WHEN SESSIONPROPERTY('ANSI_NULLS') = 1 THEN 1 ELSE 0 END;" ) );
            var isQuotedIdentifierOn = Convert.ToBoolean( SqlScalar( "SELECT CASE WHEN SESSIONPROPERTY('QUOTED_IDENTIFIER') = 1 THEN 1 ELSE 0 END;" ) );

            Sql( "SET ANSI_NULLS ON;" );
            Sql( "SET QUOTED_IDENTIFIER ON;" );

            // Add procedure (dropping it first if it already exists).
            Sql( @"
IF EXISTS (SELECT * FROM sys.objects WHERE OBJECT_ID = OBJECT_ID(N'[dbo].[spGivingAutomation_UpdateGivingBinsAndPercentiles]') AND TYPE IN (N'P', N'PC'))
    DROP PROCEDURE [dbo].[spGivingAutomation_UpdateGivingBinsAndPercentiles];" );

            Sql( RockMigrationSQL._202601152051145_AddGivingAutomationProcedures_spGivingAutomation_UpdateGivingBinsAndPercentiles );

            // Restore the original settings.
            Sql( $"SET ANSI_NULLS {( isAnsiNullsOn ? "ON" : "OFF" )};" );
            Sql( $"SET QUOTED_IDENTIFIER {( isQuotedIdentifierOn ? "ON" : "OFF" )};" );
        }

        private void AddUpdateGivingJourneyStagesProcedure()
        {
            // Ensure these settings are set as expected so they persist with the stored procedure.
            // But first, read their current values to restore them after the migration.
            var isAnsiNullsOn = Convert.ToBoolean( SqlScalar( "SELECT CASE WHEN SESSIONPROPERTY('ANSI_NULLS') = 1 THEN 1 ELSE 0 END;" ) );
            var isQuotedIdentifierOn = Convert.ToBoolean( SqlScalar( "SELECT CASE WHEN SESSIONPROPERTY('QUOTED_IDENTIFIER') = 1 THEN 1 ELSE 0 END;" ) );

            Sql( "SET ANSI_NULLS ON;" );
            Sql( "SET QUOTED_IDENTIFIER ON;" );

            // Add procedure (dropping it first if it already exists).
            Sql( @"
IF EXISTS (SELECT * FROM sys.objects WHERE OBJECT_ID = OBJECT_ID(N'[dbo].[spGivingAutomation_UpdateGivingJourneyStages]') AND TYPE IN (N'P', N'PC'))
    DROP PROCEDURE [dbo].[spGivingAutomation_UpdateGivingJourneyStages];" );

            Sql( RockMigrationSQL._202601152051145_AddGivingAutomationProcedures_spGivingAutomation_UpdateGivingJourneyStages );

            // Restore the original settings.
            Sql( $"SET ANSI_NULLS {( isAnsiNullsOn ? "ON" : "OFF" )};" );
            Sql( $"SET QUOTED_IDENTIFIER {( isQuotedIdentifierOn ? "ON" : "OFF" )};" );
        }

        #endregion Private Methods
    }
}
