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
    public partial class Rollup_20211215 : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            MediaAccountsPageDisplay();
            EnableCssInlining();
            FixContributionTemplatessRoute();
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
        }

        /// <summary>
        /// NA/SK: Media Elements page must be change to default to view in menu "When Allowed"
        /// </summary>
        private void MediaAccountsPageDisplay()
        {
            Sql( @"
                UPDATE
	                [Page]
                SET 
	                [DisplayInNavWhen]=0
                WHERE 
	                [Guid]='07CB7BB5-1465-4E75-8DD4-28FA6EA48222'" );
        }

        /// <summary>
        /// NA/CR: Enable Default CSSInliningEnabled to True
        /// </summary>
        private void EnableCssInlining()
        {
            // Set 'CSS Inlining Enabled' (on Email communication medium) to True:
            RockMigrationHelper.AddOrUpdateEntityAttribute( 
                "Rock.Communication.Medium.Email", 
                SystemGuid.FieldType.BOOLEAN, 
                string.Empty, 
                string.Empty, 
                "CSS Inlining Enabled", 
                "CSS Inlining Enabled", 
                "Enable to move CSS styles to inline attributes. This can help maximize compatibility with email clients.", 
                4, 
                @"True", 
                SystemGuid.Attribute.COMMUNICATION_MEDIUM_EMAIL_CSS_INLINING_ENABLED, 
                "CSSInliningEnabled" );
        }

        /// <summary>
        /// ED: Fix contribution-templatess route
        /// </summary>
        private void FixContributionTemplatessRoute()
        {
            Sql( @"
                -- Page: Contribution Template Detail
                -- Route: finance/settings/contribution-templates/{StatementTemplateId}

                DECLARE @pageId int = (SELECT [Id] FROM [Page] WHERE [Guid] = 'D4CB4CE6-FBF9-4FBD-B8C4-08BE022F97D7')
                DECLARE @routeId int = (SELECT [Id] FROM [PageRoute] WHERE [PageId] = @PageId AND [Route] = 'finance/settings/contribution-templatess/{StatementTemplateId}')

                IF @PageId IS NOT NULL AND @routeId IS NOT NULL
                BEGIN
	                UPDATE [PageRoute]
	                SET [Route] =  'finance/settings/contribution-templates/{StatementTemplateId}'
	                WHERE Id = @routeId
                END" );
        }
    }
}
