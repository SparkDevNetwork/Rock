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
    public partial class AddPersonViewed : Rock.Migrations.RockMigration3
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            // Create a PersonViewedDetail page
            AddPage( "0E56F56E-FB32-4827-A69A-B90D43CB47F5", "195BCD57-1C10-4969-886F-7324B6287B75", "Person Viewed Detail", "Detailed profile viewing information for a person", "48A9DF54-CC19-42FA-BDC6-97AF3E63029D" );
            // Add the PersonViewedDetail BlockType
            AddBlockType( "Person Viewed Detail", "Displays detailed profile viewing information for a person", "~/Blocks/Security/PersonViewedDetail.ascx", "132D18F3-D169-4260-94E0-84F42A40B356" );
            Sql( @"
                UPDATE [BlockType] 
                SET [Category]='Security' 
                WHERE [Guid]='132D18F3-D169-4260-94E0-84F42A40B356'
            " );
            // Add the PersonViewedDetail Block to the PersonViewedDetail page, main section
            AddBlock( "48A9DF54-CC19-42FA-BDC6-97AF3E63029D", "", "132D18F3-D169-4260-94E0-84F42A40B356", "Person Viewed Detail", "Main", "", "", 0, "D957F3CA-8DE6-4A9D-9455-96E3AEF3618E" );

            // Add the PersonViewedSummary BlockType
            AddBlockType( "Person Viewed Summary", "Displays summary profile viewing information", "~/Blocks/Security/PersonViewedSummary.ascx", "1DAF15F9-E237-4B2B-8309-F335456F8FE4" );
            Sql( @"
                UPDATE [BlockType] 
                SET [Category]='Security' 
                WHERE [Guid]='1DAF15F9-E237-4B2B-8309-F335456F8FE4'
            " );

            // Add the PersonViewedSummary Block to the Person page, Security tab, section C1. This is the Profile Viewed By block
            AddBlock( "0E56F56E-FB32-4827-A69A-B90D43CB47F5", "", "1DAF15F9-E237-4B2B-8309-F335456F8FE4", "Profile Viewed By", "SectionC1", "", "", 2, "5F773D7C-47CA-473A-9787-D017FC42537D" );
            // Set the Detail Page attribute of the PersonViewedSummary block to the PersonViewedDetail page
            AddBlockTypeAttribute( "1DAF15F9-E237-4B2B-8309-F335456F8FE4", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Detail Page", "DetailPage", "", "", 0, "48A9DF54-CC19-42FA-BDC6-97AF3E63029D", "C9FEE4E9-B363-484F-A29A-84187D0D2639" );
            AddBlockAttributeValue( "5F773D7C-47CA-473A-9787-D017FC42537D", "C9FEE4E9-B363-484F-A29A-84187D0D2639", "48A9DF54-CC19-42FA-BDC6-97AF3E63029D" );
            // Set the SeeProfilesViewed attribute of the PersonViewedSummary block to false
            AddBlockTypeAttribute( "1DAF15F9-E237-4B2B-8309-F335456F8FE4", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "See Profiles Viewed", "SeeProfilesViewed", "", "", 0, "False", "CAFF8EA2-5569-455C-8061-6FCC0934F084" );
            AddBlockAttributeValue( "5F773D7C-47CA-473A-9787-D017FC42537D", "CAFF8EA2-5569-455C-8061-6FCC0934F084", "False" );

            // Add another PersonViewedSummary Block to the Person page, Security tab, section C1. This is the Profiles Viewed block.
            AddBlock( "0E56F56E-FB32-4827-A69A-B90D43CB47F5", "", "1DAF15F9-E237-4B2B-8309-F335456F8FE4", "Profiles Viewed", "SectionC1", "", "", 3, "CAA4CB7F-9DC2-45AD-AAFE-28DE237DC4AE" );
            // Set the Detail Page attribute of the PersonViewedSummary block to the PersonViewedDetail page
            AddBlockAttributeValue( "CAA4CB7F-9DC2-45AD-AAFE-28DE237DC4AE", "C9FEE4E9-B363-484F-A29A-84187D0D2639", "48A9DF54-CC19-42FA-BDC6-97AF3E63029D" );
            // Set the SeeProfilesViewed attribute of the PersonViewedSummary block to true
            AddBlockAttributeValue( "CAA4CB7F-9DC2-45AD-AAFE-28DE237DC4AE", "CAFF8EA2-5569-455C-8061-6FCC0934F084", "True" );
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            // Reverse all that stuff from up above.
            DeleteBlockAttributeValue( "CAA4CB7F-9DC2-45AD-AAFE-28DE237DC4AE", "CAFF8EA2-5569-455C-8061-6FCC0934F084" );
            DeleteBlockAttributeValue( "CAA4CB7F-9DC2-45AD-AAFE-28DE237DC4AE", "C9FEE4E9-B363-484F-A29A-84187D0D2639" );
            DeleteBlock( "CAA4CB7F-9DC2-45AD-AAFE-28DE237DC4AE" );
            DeleteBlockAttributeValue( "5F773D7C-47CA-473A-9787-D017FC42537D", "CAFF8EA2-5569-455C-8061-6FCC0934F084" );
            DeleteBlockAttribute( "CAFF8EA2-5569-455C-8061-6FCC0934F084" );
            DeleteBlockAttributeValue( "5F773D7C-47CA-473A-9787-D017FC42537D", "C9FEE4E9-B363-484F-A29A-84187D0D2639" );
            DeleteBlockAttribute( "C9FEE4E9-B363-484F-A29A-84187D0D2639" );
            DeleteBlock( "5F773D7C-47CA-473A-9787-D017FC42537D" );
            DeleteBlockType( "1DAF15F9-E237-4B2B-8309-F335456F8FE4" );
            DeleteBlock( "D957F3CA-8DE6-4A9D-9455-96E3AEF3618E" );
            DeleteBlockType( "132D18F3-D169-4260-94E0-84F42A40B356" );
            DeletePage( "48A9DF54-CC19-42FA-BDC6-97AF3E63029D" );
        }
    }
}
