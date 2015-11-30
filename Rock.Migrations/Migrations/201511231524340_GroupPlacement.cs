// <copyright>
// Copyright 2013 by the Spark Development Network
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.apache.org/licenses/LICENSE-2.0
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
    public partial class GroupPlacement : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            AddColumn("dbo.RegistrationTemplate", "AllowGroupPlacement", c => c.Boolean(nullable: false));

            RockMigrationHelper.UpdateBlockType( "Dynamic Chart", "Block to display a chart using SQL as the chart datasource", "~/Blocks/Reporting/DynamicChart.ascx", "Reporting", "7BCCBFB0-26A5-4376-B1F3-DC6ADD7C3723" );
            RockMigrationHelper.UpdateBlockType( "Dynamic Heat Map", "Block to a map of the locations of people", "~/Blocks/Reporting/DynamicHeatMap.ascx", "Reporting", "FAFBB883-D0B4-498E-91EE-CAC5652E5095" );

            RockMigrationHelper.AddBlockTypeAttribute( "4D6B686A-79DF-4EFC-A8BA-9841C248BF74", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Show Prayer Count", "ShowPrayerCount", "", "If enabled, the block will show the current prayer count for each request in the list.", 2, @"False", "0BD12941-651E-421E-9A14-3740593C843F" );

            // add new global attribute to determine email link preference
            RockMigrationHelper.AddGlobalAttribute( SystemGuid.FieldType.SINGLE_SELECT, "", "", "Preferred Email Link Type", "The type of link you prefer email links to use. 'New Communication' will link to the new communication page, while 'Mailto' will use a mailto tag which will take the user to their configured mail client.", 99, "1", "F1BECEF9-1047-E89F-4CC8-8F856750E5D0" );
            Sql( @"  
    DECLARE @EmailLinkAttributeId int = (SELECT TOP 1 [Id] FROM [Attribute] WHERE [Guid] = 'F1BECEF9-1047-E89F-4CC8-8F856750E5D0')

    INSERT INTO [AttributeQualifier] 
	    ([IsSystem], [AttributeId], [Key], [Value], [Guid])
	    VALUES
		    (0, @EmailLinkAttributeId, 'fieldtype', 'ddl', newid())

    INSERT INTO [AttributeQualifier] 
	    ([IsSystem], [AttributeId], [Key], [Value], [Guid])
	    VALUES
		    (0, @EmailLinkAttributeId, 'values', '1^New Communication,2^Mailto', newid())
");
        
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            DropColumn("dbo.RegistrationTemplate", "AllowGroupPlacement");

            RockMigrationHelper.DeleteAttribute( "0BD12941-651E-421E-9A14-3740593C843F" );

            RockMigrationHelper.DeleteAttribute( "F1BECEF9-1047-E89F-4CC8-8F856750E5D0" );
        }
    }
}
