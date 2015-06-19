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
    public partial class LabelUpdateGroupMemberIsNotified : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            
            //
            // Migration Roll-ups
            //

            // JE: Update name of note type
            Sql( @"UPDATE [NoteType]
	                SET [Name] = 'Timeline'
	                WHERE [Guid] = 'BE0BF5B4-2EB3-4734-A3E6-B0FBFD0B148B'" );

            // NA: Update SampleData block attribute for 4.0 changes
            RockMigrationHelper.AddBlockAttributeValue( "34CA1FA0-F8F1-449F-9788-B5E6315DC058", "5E26439E-4E98-45B1-B19B-D5B2F3405963", @"http://storage.rockrms.com/sampledata/sampledata_1_4_0.xml" );
            

            //
            // Add column for group member IsNotified
            //
            AddColumn("dbo.GroupMember", "IsNotified", c => c.Boolean(nullable: false));


            //
            // Update notes label (moved group and location to separate lines)
            //
            Sql( @"  UPDATE [BinaryFileData]
	                    SET [Content] = 0xEFBBBF1043547E7E43442C7E43435E7E43547E0D0A5E58417E54413030307E4A534E5E4C54305E4D4E575E4D54445E504F4E5E504D4E5E4C48302C305E4A4D415E5052362C367E534431355E4A55535E4C524E5E4349305E585A0D0A5E58410D0A5E4D4D540D0A5E50573831320D0A5E4C4C303430360D0A5E4C53300D0A5E46543630372C36385E41304E2C37332C37325E46423137372C312C302C525E46485C5E46445757575E46530D0A5E4654362C3132325E41304E2C33392C33385E46485C5E4644325E46530D0A5E46543432362C3135305E41304E2C32352C32345E46485C5E4644345E46530D0A5E464F382C3136315E474235372C34322C34325E46530D0A5E4654382C3139345E41304E2C33342C33335E464235372C312C302C435E46525E46485C5E46444141415E46530D0A5E46543432372C3131385E41304E2C32352C32345E46485C5E4644335E46530D0A5E464F31322C3236345E474234382C34322C34325E46530D0A5E465431322C3239375E41304E2C33342C33335E464234382C312C302C435E46525E46485C5E46444C4C4C5E46530D0A5E46423333302C342C302C4C5E465436382C3235305E41304E2C32332C32345E46485C5E4644355E46530D0A5E46423333302C342C302C4C5E465436382C3335345E41304E2C32332C32345E46485C5E4644375E46530D0A5E46543432312C3231325E41304E2C32332C32345E46485C5E46444E6F7465733A5E46530D0A5E464F3430332C3135345E4742302C3233372C315E46530D0A5E464F3432322C3338365E47423336312C302C315E46530D0A5E464F3432332C3334355E47423336312C302C315E46530D0A5E464F3432312C3330345E47423336312C302C315E46530D0A5E464F3432312C3236335E47423336312C302C315E46530D0A5E4C52595E464F302C305E47423831322C302C38315E46535E4C524E0D0A5E5051312C302C312C595E585A0D0A
	                    WHERE [Guid] = '0131F1F8-8EF7-48C8-80C0-0C10AEA4FA1A'
		                    AND [ModifiedDateTime] IS NULL" ); 

            //
            // Move Location Editor Page Under Data Integrity
            //

            Sql( @"  UPDATE [Page]
	  SET [ParentPageId] = (SELECT TOP 1 [Id] FROM [Page] WHERE [Guid] = '84FD84DF-F58B-4B9D-A407-96276C40AB7E')
	  WHERE [Guid] = '47BFA50A-68D8-4841-849B-75AB3E5BCD6D'" );
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            //
            // Remove column for group member IsNotified
            //
            
            DropColumn("dbo.GroupMember", "IsNotified");
        }
    }
}
