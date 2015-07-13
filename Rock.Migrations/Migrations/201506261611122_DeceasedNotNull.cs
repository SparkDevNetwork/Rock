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
    public partial class DeceasedNotNull : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            Sql( @"DROP INDEX [FirstLastName] ON [dbo].[Person]" );
            Sql( @"DROP INDEX [LastFirstName] ON [dbo].[Person]" );
            Sql( "UPDATE Person set IsDeceased = 0 where IsDeceased is null" );
            AlterColumn( "dbo.Person", "IsDeceased", c => c.Boolean( nullable: false ) );

            Sql( @"
CREATE NONCLUSTERED INDEX [FirstLastName] ON [dbo].[Person]
(
	[IsDeceased] ASC,
	[FirstName] ASC,
	[LastName] ASC
)" );

            Sql( @"
CREATE NONCLUSTERED INDEX [LastFirstName] ON [dbo].[Person]
(
	[IsDeceased] ASC,
	[LastName] ASC,
	[FirstName] ASC
)" );

            // NA: Fix For Position Approval getting set to Approved when Denied
            RockMigrationHelper.UpdateWorkflowActionForm( @"<h1>Position Approval</h1>
<p>
    {{ Workflow.Requestor }} has requested a new {{ Workflow.PositionTitle }} position. 
    Please approve or deny this request.
</p>", @"", "Approve^c88fef94-95b9-444a-bc93-58e983f3c047^0511AD1D-72B0-4B3A-AE28-3291D24CE498^You have approved this request. HR and the requester will be notified.|Deny^d6b809a9-c1cc-4ebb-816e-33d8c1e53ea4^8CDD18F5-4279-46CA-886C-128571D868EE^You have denied this request. HR and the requester will be notified.|", "88C7D1CC-3478-4562-A301-AE7D4D7FFF6D", true, "", "59102A3B-24C8-4B4F-BA76-EEB692087ADC" ); // Position Approval:Approval Process:Approval Entry

            // TC: Migrations for External Calendar block
            RockMigrationHelper.AddBlockTypeAttribute( "8760D668-8ADF-48C8-9D90-09461FB75B88", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Show Month View", "ShowMonthView", "", "Determines whether the month view option is shown", 0, @"True", "64C45947-5146-42E2-B0BE-1BAF844FB689" );
            RockMigrationHelper.AddBlockTypeAttribute( "8760D668-8ADF-48C8-9D90-09461FB75B88", "7EDFA2DE-FDD3-4AC1-B356-1F5BFC231DAE", "Start of Week Day", "StartofWeekDay", "", "Determines what day is the start of day", 0, @"1", "825BA8AF-443C-4D2F-AC77-23728868B987" );
            RockMigrationHelper.AddBlockTypeAttribute( "8760D668-8ADF-48C8-9D90-09461FB75B88", "7525C4CB-EE6B-41D4-9B64-A08048D5A5C0", "Default View Option", "DefaultViewOption", "", "Determines the default view option", 0, @"Week", "415ECE01-4234-4941-8117-9C56784ED2C1" );
            RockMigrationHelper.AddBlockTypeAttribute( "8760D668-8ADF-48C8-9D90-09461FB75B88", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Show Week View", "ShowWeekView", "", "Determines whether the week view option is shown", 0, @"True", "38D75F22-704D-4272-9A99-90F9F8892C28" );
            RockMigrationHelper.AddBlockTypeAttribute( "8760D668-8ADF-48C8-9D90-09461FB75B88", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Show Day View", "ShowDayView", "", "Determines whether the day view option is shown", 0, @"False", "6DDDF4F6-E96F-4C7D-AD4F-E388F2A4F2C0" );
            RockMigrationHelper.UpdateBlockTypeAttribute( "8760D668-8ADF-48C8-9D90-09461FB75B88", "1D0D3794-C210-48A8-8C68-3FBEC08A6BA5", "Lava Template", "LavaTemplate", "", "Lava template to use to display the list of events.", 2, @"{% include '~/Themes/Stark/Assets/Lava/ExternalCalendar.lava' %}", "1D3EC083-581E-4435-8FC8-930C48AC50F4" );
            RockMigrationHelper.AddBlockAttributeValue( "0ADEEFE5-8293-48AC-AFA9-E0F0E363FCE7", "1D3EC083-581E-4435-8FC8-930C48AC50F4", @"{% include '~/Themes/Stark/Assets/Lava/ExternalCalendar.lava' %}" ); // Lava Template
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            // TC: Migrations for External Calendar block (down)
            RockMigrationHelper.DeleteAttribute( "6DDDF4F6-E96F-4C7D-AD4F-E388F2A4F2C0" );
            RockMigrationHelper.DeleteAttribute( "38D75F22-704D-4272-9A99-90F9F8892C28" );
            RockMigrationHelper.DeleteAttribute( "415ECE01-4234-4941-8117-9C56784ED2C1" );
            RockMigrationHelper.DeleteAttribute( "825BA8AF-443C-4D2F-AC77-23728868B987" );
            RockMigrationHelper.DeleteAttribute( "64C45947-5146-42E2-B0BE-1BAF844FB689" );
            
            AlterColumn("dbo.Person", "IsDeceased", c => c.Boolean());
        }
    }
}
