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
    public partial class UpdateSiteAndLayoutWithMobileColumns : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            AddColumn("dbo.Layout", "LayoutMobilePhone", c => c.String());
            AddColumn("dbo.Layout", "LayoutMobileTablet", c => c.String());
            AddColumn("dbo.Site", "IsActive", c => c.Boolean(nullable: false,defaultValue:true));
            AddColumn("dbo.Site", "ConfigurationMobilePhoneFileId", c => c.Int());
            AddColumn("dbo.Site", "ConfigurationMobileTabletFileId", c => c.Int());
            AddColumn("dbo.Site", "AdditionalSettings", c => c.String());
            AddColumn("dbo.Site", "SiteType", c => c.Int(nullable: false));
            AddColumn("dbo.Site", "ThumbnailFileId", c => c.Int());
            AddColumn("dbo.Site", "LatestVersionDateTime", c => c.DateTime());

            // Add Site Type Attribute to Attribute table
            RockMigrationHelper.AddBlockTypeAttribute( "441D5A71-C250-4FF5-90C3-DEEAD3AC028D", "BD0D9B57-2A41-4490-89FF-F01DAB7D4904", "Site Type", "SiteType", "", @"Includes Items with the following Type.", 1, @"", "786B9AA2-EA35-4C96-BA33-7A6F9945A10E" );

            // Default the Value to Web 
            RockMigrationHelper.AddBlockAttributeValue( "9EAAC77C-3B75-428E-A393-F51B2D29097F", "786B9AA2-EA35-4C96-BA33-7A6F9945A10E", "0" );
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            DropColumn("dbo.Site", "LatestVersionDateTime");
            DropColumn("dbo.Site", "ThumbnailFileId");
            DropColumn("dbo.Site", "SiteType");
            DropColumn("dbo.Site", "AdditionalSettings");
            DropColumn("dbo.Site", "ConfigurationMobileTabletFileId");
            DropColumn("dbo.Site", "ConfigurationMobilePhoneFileId");
            DropColumn("dbo.Site", "IsActive");
            DropColumn("dbo.Layout", "LayoutMobileTablet");
            DropColumn("dbo.Layout", "LayoutMobilePhone");

            RockMigrationHelper.DeleteAttribute( "786B9AA2-EA35-4C96-BA33-7A6F9945A10E" );
        }
    }
}
