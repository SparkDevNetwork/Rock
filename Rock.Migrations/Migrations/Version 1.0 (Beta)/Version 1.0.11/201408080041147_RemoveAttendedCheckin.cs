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
    public partial class RemoveAttendedCheckin : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            // Delete Attended Check-in Page: Activity Select
            RockMigrationHelper.DeleteAttribute( "5046A353-D901-45BB-9981-9CC1B33550C6" );
            RockMigrationHelper.DeleteAttribute( "BEC10B87-4B19-4CD5-8952-A4D59DDA3E9C" );
            RockMigrationHelper.DeleteAttribute( "39008E18-48C9-445F-B9D7-78334B76A7EE" );
            RockMigrationHelper.DeleteAttribute( "6048A23D-6544-441A-A8B3-5782CAF5B468" );
            RockMigrationHelper.DeleteBlock( "8C8CBBE9-2502-4FEC-804D-C0DA13C07FA4" );
            RockMigrationHelper.DeleteBlockType( "78E2AB4A-FDF7-4864-92F7-F052050BC4BB" );
            RockMigrationHelper.DeletePage( "C87916FE-417E-4A11-8831-5CFA7678A228" );

            // Delete Attended Check-in Page: Confirmation
            RockMigrationHelper.DeleteAttribute( "2D54A2C9-759C-45B6-8E23-42F39E134170" );
            RockMigrationHelper.DeleteAttribute( "DEB23724-94F9-4164-BFAB-AD2DDE1F90ED" );
            RockMigrationHelper.DeleteAttribute( "2A71729F-E7CA-4ACD-9996-A6A661A069FD" );
            RockMigrationHelper.DeleteAttribute( "48813610-DD26-4E72-9D19-817535802C49" );
            RockMigrationHelper.DeleteAttribute( "E45D2B10-D1B1-4CBE-9C7A-3098B1D95F47" );
            RockMigrationHelper.DeleteBlock( "7CC68DD4-A6EF-4B67-9FEA-A144C479E058" );
            RockMigrationHelper.DeleteBlockType( "5B1D4187-9B34-4AB6-AC57-7E2CF67B266F" );
            RockMigrationHelper.DeletePage( "BE996C9B-3DFE-407F-BD53-D6F58D85A035" );

            // Delete Attended Check-in Page: Family Select
            RockMigrationHelper.DeleteAttribute( "2DF1D39B-DFC7-4FB2-B638-3D99C3C4F4DF" );
            RockMigrationHelper.DeleteAttribute( "338CAD91-3272-465B-B768-0AC2F07A0B40" );
            RockMigrationHelper.DeleteAttribute( "81A02B6F-F760-4110-839C-4507CF285A7E" );
            RockMigrationHelper.DeleteAttribute( "DD9F93C9-009B-4FA5-8FF9-B186E4969ACB" );
            RockMigrationHelper.DeleteAttribute( "A7F99980-BED4-4A80-AB83-DDAB5C7D7AAD" );
            RockMigrationHelper.DeleteAttribute( "C4204D6E-715E-4E3A-BA1B-949D20D26487" );
            RockMigrationHelper.DeleteBlock( "BDD502FF-40D2-42E6-845E-95C49C3505B3" );
            RockMigrationHelper.DeleteBlock( "82929409-8551-413C-972A-98EDBC23F420" );
            RockMigrationHelper.DeleteBlockType( "4D48B5F0-F0B2-4C10-8498-DAF690761A80" );
            RockMigrationHelper.DeleteBlockType( "0DF27F26-691D-41F8-B0F7-987E4FEC375C" );
            RockMigrationHelper.DeletePage( "AF83D0B2-2995-4E46-B0DF-1A4763637A68" );

            // Delete Attended Check-in Page: Search
            RockMigrationHelper.DeleteAttribute( "09536DD6-8020-400F-856C-DF3BEA6F76C5" );
            RockMigrationHelper.DeleteAttribute( "970A9BD6-D58A-4F8E-8B20-EECB845E6BD6" );
            RockMigrationHelper.DeleteAttribute( "EBE397EF-07FF-4B97-BFF3-152D139F9B80" );
            RockMigrationHelper.DeleteAttribute( "C4E992EA-62AE-4211-BE5A-9EEF5131235C" );
            RockMigrationHelper.DeleteAttribute( "BF8AAB12-57A2-4F50-992C-428C5DDCB89B" );
            RockMigrationHelper.DeleteAttribute( "72E40960-2072-4F08-8EA8-5A766B49A2E0" );
            RockMigrationHelper.DeleteAttribute( "BBB93FF9-C021-4E82-8C03-55942FA4141E" );
            RockMigrationHelper.DeleteBlock( "182C9AA0-E76F-4AAF-9F61-5418EE5A0CDB" );
            RockMigrationHelper.DeleteBlockType( "645D3F2F-0901-44FE-93E9-446DBC8A1680" );
            RockMigrationHelper.DeletePage( "8F618315-F554-4751-AB7F-00CC5658120A" );

            // Delete Attended Check-in Page: Admin
            RockMigrationHelper.DeleteAttribute( "F5512AB9-CDE2-46F7-82A8-99168D7784B2" );
            RockMigrationHelper.DeleteAttribute( "40F39C36-3092-4B87-81F8-A9B1C6B261B2" );
            RockMigrationHelper.DeleteAttribute( "18864DE7-F075-437D-BA72-A6054C209FA5" );
            RockMigrationHelper.DeleteAttribute( "7332D1F1-A1A5-48AE-BAB9-91C3AF085DB0" );
            RockMigrationHelper.DeleteAttribute( "B196160E-4397-4C6F-8C5A-317CAD3C118F" );
            RockMigrationHelper.DeleteBlock( "9F8731AB-07DB-406F-A344-45E31D0DE301" );
            RockMigrationHelper.DeleteBlockType( "2C51230E-BA2E-4646-BB10-817B26C16218" );
            RockMigrationHelper.DeletePage( "771E3CF1-63BD-4880-BC43-AC29B4CCE963" );

            // Delete Attended Check-in Page: Root
            Sql( @"
    UPDATE [Site] SET [DefaultPageId] = NULL WHERE [Guid] = '30FB46F7-4814-4691-852A-04FB56CC07F0'
" );
            RockMigrationHelper.DeletePage( "32A132A6-63A2-4840-B4A5-23D80994CCBD" );

            // Delete Attended Check-in site
            RockMigrationHelper.DeleteLayout( "3BD6CFC1-0BF2-43C8-AD38-44E711D6ACE0" );
            RockMigrationHelper.DeleteSite( "30FB46F7-4814-4691-852A-04FB56CC07F0" );

            // Delete the FilterGroupByGender, SelectByBestFit, and SelectByBestLocation workflow actions
            RockMigrationHelper.DeleteEntityType( "A9E0B879-65D5-4852-B012-402B217E9D4A" );
            RockMigrationHelper.DeleteEntityType( "97216A7E-2F7A-4C01-9EA6-04FC7DB2C9BB" );
            RockMigrationHelper.DeleteEntityType( "FDA16F87-B444-4D0F-97D3-281435B57B6B" );
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
        }
    }
}
