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

using Rock.Migrations.Migrations;

namespace Rock.Migrations
{
    /// <summary>
    ///
    /// </summary>
    public partial class AddVersionToCommunicationTemplateAndFixRecipientDetailsSP : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            AddColumn( "dbo.CommunicationTemplate", "Version", c => c.Int( nullable: false, defaultValue: 0 ) );
            FixSmsEligibilityLogicInCommunicationRecipientDetails();
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            DropColumn( "dbo.CommunicationTemplate", "Version" );
        }

        /// <summary>
        /// Fixes incorrect SMS eligibility logic and adds GroupMember.CommunicationPreference to spCommunicationRecipientDetails.
        /// </summary>
        private void FixSmsEligibilityLogicInCommunicationRecipientDetails()
        {
            // Ensure these settings are set as expected so they persist with the stored procedure.
            // But first, read their current values to restore them after the migration.
            var isAnsiNullsOn = Convert.ToBoolean( SqlScalar( "SELECT CASE WHEN SESSIONPROPERTY('ANSI_NULLS') = 1 THEN 1 ELSE 0 END;" ) );
            var isQuotedIdentifierOn = Convert.ToBoolean( SqlScalar( "SELECT CASE WHEN SESSIONPROPERTY('QUOTED_IDENTIFIER') = 1 THEN 1 ELSE 0 END;" ) );

            Sql( "SET ANSI_NULLS ON;" );
            Sql( "SET QUOTED_IDENTIFIER ON;" );

            Sql( RockMigrationSQL._202504021715459_AddVersionToCommunicationTemplateAndFixRecipientDetailsSP_spCommunicationRecipientDetails );
            
            // Restore the original settings.
            Sql( $"SET ANSI_NULLS {( isAnsiNullsOn ? "ON" : "OFF" )};" );
            Sql( $"SET QUOTED_IDENTIFIER {( isQuotedIdentifierOn ? "ON" : "OFF" )};" );
        }
    }
}
