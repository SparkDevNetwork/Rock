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
    public partial class ScheduleTypes : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            CreateTable(
                "dbo.GroupScheduleExclusion",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        GroupTypeId = c.Int(nullable: false),
                        StartDate = c.DateTime(storeType: "date"),
                        EndDate = c.DateTime(storeType: "date"),
                        CreatedDateTime = c.DateTime(),
                        ModifiedDateTime = c.DateTime(),
                        CreatedByPersonAliasId = c.Int(),
                        ModifiedByPersonAliasId = c.Int(),
                        Guid = c.Guid(nullable: false),
                        ForeignId = c.String(maxLength: 50),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.PersonAlias", t => t.CreatedByPersonAliasId)
                .ForeignKey("dbo.GroupType", t => t.GroupTypeId)
                .ForeignKey("dbo.PersonAlias", t => t.ModifiedByPersonAliasId)
                .Index(t => t.GroupTypeId)
                .Index(t => t.CreatedByPersonAliasId)
                .Index(t => t.ModifiedByPersonAliasId)
                .Index(t => t.Guid, unique: true);
            
            AddColumn("dbo.Attendance", "DidNotOccur", c => c.Boolean());
            AddColumn("dbo.Attendance", "Processed", c => c.Boolean());
            AddColumn("dbo.Schedule", "WeeklyDayOfWeek", c => c.Int());
            AddColumn("dbo.Schedule", "WeeklyTimeOfDay", c => c.Time(precision: 7));
            AddColumn("dbo.GroupType", "AllowedScheduleTypes", c => c.Int(nullable: false));
            DropColumn("dbo.Attendance", "Response");

            // JE: Move lava to file system
            Sql( @"
    -- update lava for the giving page
    DECLARE @AttributeId int = (SELECT TOP 1 [Id] FROM [Attribute] WHERE [Guid] = '394C30E6-22EE-4312-9870-EA90336F5778')
    DECLARE @EntityId int = (SELECT TOP 1 [Id] FROM [Block] WHERE [Guid] = 'E7B2EEF7-B06E-4FFF-8443-A10DEC30E1FD')

    UPDATE [AttributeValue]
    SET [Value] = '{% include ''~~/Assets/Lava/ScheduledTransactionSummary.lava''  %}'
    WHERE [AttributeId] = @AttributeId AND [EntityId] = @EntityId
" );

            Sql( @"
    -- update the default lava for the attribute
    DECLARE @AttributeId int = (SELECT TOP 1 [Id] FROM [Attribute] WHERE [Guid] = '14BC144B-878D-4432-A8A7-8B1BB8B27E89')
    DECLARE @EntityId int = (SELECT TOP 1 [Id] FROM [Block] WHERE [Guid] = '0D91DD2F-519C-4A4A-AB03-0933FC12BE7E')

    UPDATE [AttributeValue]
    SET [Value] = '{% include ''~~/Assets/Lava/ScheduledTransactionListLiquid.lava''  %}'
    WHERE [AttributeId] = @AttributeId AND [EntityId] = @EntityId
" );

            // MP: School Grade Defined Type updates
            Sql( @"
    /* Update School Grades to be sorted from K to Senior */
    update DefinedValue set [Order] = 12 where [Guid] = 'C49BD3AF-FF94-4A7C-99E1-08503A3C746E'
    update DefinedValue set [Order] = 11 where [Guid] = '78F7D773-8244-4995-8BC4-AD6F6A7B7820'
    update DefinedValue set [Order] = 10 where [Guid] = 'E04E3F62-EF5C-4860-8F32-1C152CA1700A'
    update DefinedValue set [Order] = 9 where [Guid] =  '2A130E04-3712-427A-8BB0-473EB8FF8924'
    update DefinedValue set [Order] = 8 where [Guid] = 'D58D70AF-3CCC-4D4E-BFAF-2014D8579D60'
    update DefinedValue set [Order] = 7 where [Guid] = '3FE728AC-BE25-409A-98CB-3CFCE5FA063B'
    update DefinedValue set [Order] = 6 where [Guid] = '2D702ED8-7046-4DA5-AFFA-9633A211F594'
    update DefinedValue set [Order] = 5 where [Guid] = '3D8CDBC8-8840-4A7E-85D0-B7C29A019EBB'
    update DefinedValue set [Order] = 4 where [Guid] = 'F0F98B9C-E6BE-4C42-B8F4-0D8AB1A18847'
    update DefinedValue set [Order] = 3 where [Guid] = '23CC6288-78ED-4849-AFC9-417E0DA5A4A9'
    update DefinedValue set [Order] = 2 where [Guid] = 'E475D0CA-5979-4C76-8788-D91ADF595E10'
    update DefinedValue set [Order] = 1 where [Guid] = '6B5CDFBD-9882-4EBB-A01A-7856BCD0CF61'
    update DefinedValue set [Order] = 0 where [Guid] = '0FED3291-51F3-4EED-886D-1D3DF826BEAC'

    /* show the Grade Abbreviation attribute in the Grid */
    update Attribute set IsGridColumn = 1 where [Guid] = '839B67E3-6A12-46D9-8F7F-5DFCB4F8DBE0'
" );
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            AddColumn("dbo.Attendance", "Response", c => c.String(maxLength: 200));
            DropForeignKey("dbo.GroupScheduleExclusion", "ModifiedByPersonAliasId", "dbo.PersonAlias");
            DropForeignKey("dbo.GroupScheduleExclusion", "GroupTypeId", "dbo.GroupType");
            DropForeignKey("dbo.GroupScheduleExclusion", "CreatedByPersonAliasId", "dbo.PersonAlias");
            DropIndex("dbo.GroupScheduleExclusion", new[] { "Guid" });
            DropIndex("dbo.GroupScheduleExclusion", new[] { "ModifiedByPersonAliasId" });
            DropIndex("dbo.GroupScheduleExclusion", new[] { "CreatedByPersonAliasId" });
            DropIndex("dbo.GroupScheduleExclusion", new[] { "GroupTypeId" });
            DropColumn("dbo.GroupType", "AllowedScheduleTypes");
            DropColumn("dbo.Schedule", "WeeklyTimeOfDay");
            DropColumn("dbo.Schedule", "WeeklyDayOfWeek");
            DropColumn("dbo.Attendance", "Processed");
            DropColumn("dbo.Attendance", "DidNotOccur");
            DropTable("dbo.GroupScheduleExclusion");
        }
    }
}
