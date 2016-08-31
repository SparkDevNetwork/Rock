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
    public partial class AddFamilyAttribute : Rock.Migrations.RockMigration4
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            AddBlockTypeAttribute( "DE156975-597A-4C55-A649-FE46712F91C3", "59D5A94C-94A0-4630-B80A-BB25697D74C7", "Default Connection Status", "DefaultConnectionStatus", "", "The connection status that should be set by default.", 1, @"B91BA046-BC1E-400C-B85D-638C1F4E0CE2", "135036D6-6F3C-45A0-8FCA-42EA953C0255" );
            AddBlockAttributeValue( "613536BE-86BC-4755-B815-807C236B92E6", "135036D6-6F3C-45A0-8FCA-42EA953C0255", @"B91BA046-BC1E-400C-B85D-638C1F4E0CE2" );

            // Delete BinaryFileType of ExternalFile's storage root path attribute
            DeleteAttribute( "434E89D8-8BF5-4480-9D58-9D0713EDAEAF" );
            DeleteAttribute( "7F09424A-454F-4F25-9FDD-4888894A7992" );

            // Update text to 'Ad'
            Sql( @"
    UPDATE [Attribute] SET 
	    [Key] = REPLACE([Key], 'Promotion', 'Ad')
	    , [Name] = REPLACE([Name], 'Promotion', 'Ad')
    WHERE [Guid] IN ('D06D03C1-0FB8-488B-A087-56C204B15B3D', '2D594CB6-F438-4038-8A64-8530525ABDB5', '5CD67DD2-71BA-495E-B3DB-45F39FEC0173')

    UPDATE [AttributeValue] SET 
	    [Value] = REPLACE([Value], 'Promotion', 'Ad')
    WHERE [Guid] IN ('7354DAA2-6EC2-45B2-8A71-786FAE8B3E01', '5664C99F-32E7-4C76-85C8-43F0CB030F32')

    UPDATE [DefinedValue] SET
	    [Name] = REPLACE([Name], 'Promotion', 'Ad')
	    , [Description] = REPLACE([Description], 'Promotion', 'Ad')
    WHERE [Guid] = '57B2A23F-3B0C-43A8-9F45-332120DCD0EE'
" );
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            // Attrib for BlockType: Add Family:Adult Marital Status
            DeleteAttribute( "135036D6-6F3C-45A0-8FCA-42EA953C0255" );
        }
    }
}
