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
    using System.Data.Entity.Migrations;
    
    /// <summary>
    ///
    /// </summary>
    public partial class Analytics3 : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            // Drop the AnalyticsDimFinancialTransactionType view to be consistent 
            Sql( @"IF OBJECT_ID(N'[dbo].[AnalyticsDimFinancialTransactionType]', 'V') IS NOT NULL
    DROP VIEW AnalyticsDimFinancialTransactionType
GO" );
            // MP: Set 'Default' Communication Template IsActive=1
            Sql( @"UPDATE CommunicationTemplate
SET IsActive = 1
WHERE [Guid] = 'A3C7F623-7F6F-4C48-B66F-CBEE2DF30B6A';
" );

            // MP: Update Group Member Attribute "Preferred Communication Type" to "Preferred Communication Medium"
            RockMigrationHelper.DeleteBlockAttribute( "D7941908-1F65-CC9B-416C-CCFABE4221B9" );
            RockMigrationHelper.AddGroupTypeGroupMemberAttribute( "D1D95777-FFA3-CBB3-4A6D-658706DAED33", SystemGuid.FieldType.COMMUNICATION_PREFERENCE_TYPE, "Preferred Communication Medium", "The preferred communication medium for this group member. Select None to use the person's default communication preference.", 0, "0", "D7941908-1F65-CC9B-416C-CCFABE4221B9" );

            // MP: Update GroupList block for CommunicationList to have CustomGridColumns config
            // Attrib for BlockType: Group List:core.CustomGridColumnsConfig
            RockMigrationHelper.UpdateBlockTypeAttribute( "3D7FB6BE-6BBD-49F7-96B4-96310AF3048A", "9C204CD0-1233-41C5-818A-C5DA439445AA", "core.CustomGridColumnsConfig", "core.CustomGridColumnsConfig", "", "", 0, @"", "487A898B-2192-459D-9546-32AE3EE9A9C5" );
            // Attrib Value for Block:Group List, Attribute:core.CustomGridColumnsConfig Page: Communication Lists, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "426EC86B-5784-411D-94ED-DD007E6DF783", "487A898B-2192-459D-9546-32AE3EE9A9C5", @"{
 ""ColumnsConfig"": [
 {
 ""HeaderText"": """",
 ""HeaderClass"": ""grid-columncommand"",
 ""ItemClass"": ""grid-columncommand"",
 ""LavaTemplate"": ""<div class='text-center'>\n <a href='~/EmailAnalytics?CommunicationListId={{Row.Id}}' class='btn btn-default btn-sm' title='Email Analytics'>\n <i class='fa fa-line-chart'></i>\n </a>\n</div>"",
 ""PositionOffsetType"": 1,
 ""PositionOffset"": 1
 }
 ]
}" );

            // MP: Update Wizard Communication Block to use Block Settings and Approvers from existing block
            Sql( MigrationSQL._201710051632496_Analytics3_CommunicationBlockSettings );
        }

        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
        }
    }
}
