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
    public partial class EventItemOccurrence : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            RenameTable(name: "dbo.EventItemCampusGroupMap", newName: "EventItemOccurrenceGroupMap");
            RenameTable(name: "dbo.EventItemCampus", newName: "EventItemOccurrence");
            RenameColumn(table: "dbo.EventItemSchedule", name: "EventItemCampusId", newName: "EventItemOccurrenceId");
            RenameColumn(table: "dbo.EventItemOccurrenceGroupMap", name: "EventItemCampusId", newName: "EventItemOccurrenceId");
            RenameIndex(table: "dbo.EventItemOccurrenceGroupMap", name: "IX_EventItemCampusId", newName: "IX_EventItemOccurrenceId");
            RenameIndex(table: "dbo.EventItemSchedule", name: "IX_EventItemCampusId", newName: "IX_EventItemOccurrenceId");

            Sql( @"
    DELETE FROM [BlockType] 
	WHERE [Path] = '~/Blocks/Event/CalendarItemOccurrenceList.ascx'

    UPDATE [BlockType] SET 
		[Path] = '~/Blocks/Event/CalendarItemOccurrenceList.ascx', 
		[Name] = 'Calendar Item Occurrence List',
		[Description] = 'Displays the occurrence details for a given calendar item.'
	WHERE [Path] = '~/Blocks/Event/CalendarItemCampusList.ascx'

    DELETE FROM [BlockType] 
	WHERE [Path] = '~/Blocks/Event/CalendarItemOccurrenceDetail.ascx'

    UPDATE [BlockType] SET 
		[Path] = '~/Blocks/Event/CalendarItemOccurrenceDetail.ascx',
		[Name] = 'Calendar Item Occurrence Detail',
		[Description] = 'Displays the details of a given calendar item occurrence.'
	WHERE [Path] = '~/Blocks/Event/CalendarItemCampusDetail.ascx'
" );
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            RenameIndex(table: "dbo.EventItemSchedule", name: "IX_EventItemOccurrenceId", newName: "IX_EventItemCampusId");
            RenameIndex(table: "dbo.EventItemOccurrenceGroupMap", name: "IX_EventItemOccurrenceId", newName: "IX_EventItemCampusId");
            RenameColumn(table: "dbo.EventItemOccurrenceGroupMap", name: "EventItemOccurrenceId", newName: "EventItemCampusId");
            RenameColumn(table: "dbo.EventItemSchedule", name: "EventItemOccurrenceId", newName: "EventItemCampusId");
            RenameTable(name: "dbo.EventItemOccurrence", newName: "EventItemCampus");
            RenameTable(name: "dbo.EventItemOccurrenceGroupMap", newName: "EventItemCampusGroupMap");
        }
    }
}
