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
    public partial class CampusContextAlignment : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {

            //
            // Add block attribute and value to make the campus context setter on the check-in manager align right
            //

            // Attrib for BlockType: Campus Context Setter:Alignment
            RockMigrationHelper.AddBlockTypeAttribute( "4A5AAFFC-B1C7-4EFD-A9E4-84363242EA85", "7525C4CB-EE6B-41D4-9B64-A08048D5A5C0", "Alignment", "Alignment", "", "Determines the alignment of the dropdown.", 8, @"1", "C894049D-3CBF-493C-8979-F2CD95AE8B77" );

            // Attrib Value for Block:Campus Context Setter, Attribute:Alignment , Layout: Full Width, Site: Rock Check-in Manager
            RockMigrationHelper.AddBlockAttributeValue( "8B940F43-C38A-4086-80D8-7C33961518E3", "C894049D-3CBF-493C-8979-F2CD95AE8B77", @"2" );

            // ---------------------------------------------------------

            //
            // Update from PageView Table
            //

            Sql( @"DECLARE @keepUpdating INT = 1

WHILE @keepUpdating > 0
BEGIN
    UPDATE PageView
    SET Url = stuff(Url, patindex('%rckipid=%', Url), CASE 
                WHEN charindex('&', SUBSTRING(Url, patindex('%rckipid=%', Url), 500)) > 0
                    THEN charindex('&', SUBSTRING(Url, patindex('%rckipid=%', Url), 500)) - 1
                ELSE 500
                END, 'rckipid_=XXXXXXXXXXXXXXXXXXXXXXXXXXXX')
    WHERE Url LIKE '%rckipid=%'

    SET @keepUpdating = @@ROWCOUNT
END

UPDATE PageView
SET Url = REPLACE(Url, 'rckipid_=', 'rckipid=')
WHERE Url LIKE '%rckipid_=%'" );

            
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            // Attrib for BlockType: Campus Context Setter:Alignment
            RockMigrationHelper.DeleteAttribute( "C894049D-3CBF-493C-8979-F2CD95AE8B77" );
            
        }
    }
}
