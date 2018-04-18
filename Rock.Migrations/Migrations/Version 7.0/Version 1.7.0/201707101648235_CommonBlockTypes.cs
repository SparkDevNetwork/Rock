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
    public partial class CommonBlockTypes : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            AddColumn("dbo.BlockType", "IsCommon", c => c.Boolean(nullable: false));
            Sql( $"UPDATE [BlockType] SET [IsCommon] = 1 WHERE [Guid] = '{Rock.SystemGuid.BlockType.HTML_CONTENT}'" );
            Sql( $"UPDATE [BlockType] SET [IsCommon] = 1 WHERE [Guid] = '{Rock.SystemGuid.BlockType.PAGE_MENU}'" );
            Sql( $"UPDATE [BlockType] SET [IsCommon] = 1 WHERE [Guid] = '{Rock.SystemGuid.BlockType.CONTENT_CHANNEL_VIEW}'" );

            // Rollups
            // MP: Universal No Date Channel Type 
            Sql( @"INSERT INTO [ContentChannelType]
( [IsSystem], [Name], [DateRangeType], [Guid], [DisablePriority] )
VALUES
( 1, 'Universal No Date Channel Type', 3, '8A9B6DE0-6057-4356-894D-E84E33392F57', 0 )" );

            // MP: IsFormal Person Titles
            RockMigrationHelper.AddDefinedTypeAttribute( "4784CD23-518B-43EE-9B97-225BF6E07846", Rock.SystemGuid.FieldType.BOOLEAN, "Is Formal", "IsFormal",
 "This flag marks the title as being formal which will display it differently on various screens. For instance, the title Dr. is nice to know when addressing someone, but Mr. is common and not needed to be displayed in many places.", 0, false.ToString(), "EE60F454-3E87-4BB5-B8BE-10464C96B666" );

            Sql( "Update [Attribute] set [IsGridColumn] = 1 where [Guid] = 'EE60F454-3E87-4BB5-B8BE-10464C96B666' " );

            // Set 'Dr.' person title as IsFormal=True
            RockMigrationHelper.AddDefinedValueAttributeValue( "FAAFBC0F-6209-4647-BA20-4A9CF1950AB4", "EE60F454-3E87-4BB5-B8BE-10464C96B666", true.ToString() );

            // Set 'Rev.' person title as IsFormal=True
            RockMigrationHelper.AddDefinedValueAttributeValue( "F0EC14D2-3E4E-4F4A-B445-38A89A005812", "EE60F454-3E87-4BB5-B8BE-10464C96B666", true.ToString() );

            // Set 'Cpt.' person title as IsFormal=True
            RockMigrationHelper.AddDefinedValueAttributeValue( "7149B9E3-A220-47FD-93EA-84A0381F5C94", "EE60F454-3E87-4BB5-B8BE-10464C96B666", true.ToString() );

            // DT: Update Binary File proc
            Sql( MigrationSQL._201707101648235_CommonBlockTypes_spCore_BinaryFileGet );

            // SV: Renamed the "Inactive Record Status" to "Record Status" and show Inactive AND Pending
            Sql( @"
UPDATE [PersonBadge]
	SET 
	[Name]='Record Status'
	WHERE
	[Guid]='66972BFF-42CD-49AB-9A7A-E1B9DECA4ECA';

UPDATE A
	SET [Value]='{% if Person.RecordStatusValue.Value != empty and Person.RecordStatusValue.Value == ""Inactive"" -%}
                    <span class=""label label-danger"" title=""{{ Person.RecordStatusReasonValue.Value }}"" data-toggle=""tooltip"">{{ Person.RecordStatusValue.Value }}</span>
                    {% elseif Person.RecordStatusValue.Value != empty and Person.RecordStatusValue.Value == ""Pending"" -%}
                    <span class=""label label-warning"" title=""{{ Person.RecordStatusReasonValue.Value }}"" data-toggle=""tooltip"">{{ Person.RecordStatusValue.Value }}</span>
                {% endif -%}'
	FROM [AttributeValue] A  INNER JOIN
	[Attribute] B ON A.[AttributeId]=B.Id
	WHERE
	B.[Guid]='01C9BA59-D8D4-4137-90A6-B3C06C70BBC3'
	AND A.[EntityId]=(SELECT Id 
	FROM [PersonBadge]
	WHERE [Guid]='66972BFF-42CD-49AB-9A7A-E1B9DECA4ECA')
" );

            // MP: BlockTypes rollups
            RockMigrationHelper.UpdateBlockType( "Event Detail with Occurrences Search Lava", "Block that shows details of a specific event with a search for occurrences of the event", "~/Blocks/Event/EventDetailWithOccurrencesSearchLava.ascx", "Event", "B7788DFF-783D-40A3-BFD4-EA9561F950A8" );
            RockMigrationHelper.UpdateBlockType( "Event Item Occurrences Search Lava", "Block does a search for occurrences of events based on the EventCalendarId specified in the URL", "~/Blocks/Event/EventItemOccurrencesSearchLava.ascx", "Event", "01CA4723-8290-41C6-A2D2-88469FAA48E9" );
            RockMigrationHelper.UpdateBlockType( "Sample React Block", "Creates a generic counter to showcase React integration", "~/Blocks/Examples/SampleReactBlock.ascx", "Examples", "7DC6C490-E9AF-407C-9716-991564F93FF6" );
            RockMigrationHelper.UpdateBlockType( "Fundraising Progress", "Progress for all people in a fundraising opportunity", "~/Blocks/Fundraising/FundraisingProgress.ascx", "Fundraising", "859DCA73-938D-4214-B9AB-BFDB6CEA2DE0" );
            // Attrib for BlockType: Attributes:Enable Ordering
            RockMigrationHelper.UpdateBlockTypeAttribute( "E5EA2F6D-43A2-48E0-B59C-4409B78AC830", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Enable Ordering", "EnableOrdering", "", "Should the attributes be allowed to be sorted?", 3, @"False", "E27F1AE4-1792-4033-9A72-668647B0CD09" );

        }

        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            DropColumn( "dbo.BlockType", "IsCommon");

            // Rollups
            // MP: Universal No Date Channel Type (down)
            Sql( @"DELETE FROM [ContentChannelType]
WHERE [Guid] = '8A9B6DE0-6057-4356-894D-E84E33392F57'" );

            // MP: IsFormal Person Titles
            RockMigrationHelper.DeleteAttribute( "EE60F454-3E87-4BB5-B8BE-10464C96B666" );
        }
    }
}
