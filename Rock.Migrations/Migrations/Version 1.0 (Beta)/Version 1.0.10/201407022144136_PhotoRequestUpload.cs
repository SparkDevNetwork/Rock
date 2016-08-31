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
    public partial class PhotoRequestUpload : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            RockMigrationHelper.AddPage( "EBAA5140-4B8F-44B8-B1E8-C73B654E4B22", "5FEAF34C-7FB6-4A11-8A1E-C452EC7849BD", "Photo Upload", "Page where people can upload their photo or for members of their family.", "8559A9F1-C6A4-4945-B393-74F6706A8FA2", "" );
            RockMigrationHelper.UpdateBlockType( "Upload", "Allows a photo to be uploaded for the given person (logged in person) and optionally their family members.", "~/Blocks/Crm/PhotoRequest/Upload.ascx", "CRM > PhotoRequest", "7764E323-7460-4CB7-8024-056136C99603" );

            // Attrib for BlockType: Upload:Include Family Members
            RockMigrationHelper.AddBlockTypeAttribute( "7764E323-7460-4CB7-8024-056136C99603", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Include Family Members", "IncludeFamilyMembers", "", "If checked, other family members will also be displayed allowing their photos to be uploaded.", 0, @"True", "69748C7C-AE7A-4C3D-A5B6-BF29EA4685A4" );
            // Attrib for BlockType: Upload:Allow Staff
            RockMigrationHelper.AddBlockTypeAttribute( "7764E323-7460-4CB7-8024-056136C99603", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Allow Staff", "AllowStaff", "", "If checked, staff members will also be allowed to upload new photos for themselves.", 0, @"False", "2C9DC553-2CB9-4FAD-A87C-43A53EE652B6" );
            // Add Block to Page: Photo Upload, Site: Rock Solid Church
            RockMigrationHelper.AddBlock( "8559A9F1-C6A4-4945-B393-74F6706A8FA2", "", "19B61D65-37E3-459F-A44F-DEF0089118A3", "Instructions", "Main", "", "", 0, "1FC171B7-6555-4557-84E9-89A2C5E4051D" );
            // Add Block to Page: Photo Upload, Site: Rock Solid Church
            RockMigrationHelper.AddBlock( "8559A9F1-C6A4-4945-B393-74F6706A8FA2", "", "7764E323-7460-4CB7-8024-056136C99603", "Upload", "Main", "", "", 1, "E3009763-C34E-4F1C-985F-4CE9A9CA0715" );
            // Set CacheDuration to 0 for Instructions HTML block with dynamic user content.
            RockMigrationHelper.AddBlockAttributeValue( "1FC171B7-6555-4557-84E9-89A2C5E4051D", "4DFDB295-6D0F-40A1-BEF9-7B70C56F66C4", @"0" );

            Sql( @"
    DECLARE @BlockId int = (SELECT [Id] FROM [Block] WHERE [Guid] = '1FC171B7-6555-4557-84E9-89A2C5E4051D')
    INSERT INTO [HtmlContent]
        ([BlockId]
        ,[EntityValue]
        ,[Version]
        ,[Content]
        ,[IsApproved]
        ,[Guid])
    VALUES
        (@BlockId
        , ''
        , 1
        , 
'   <div class=""jumbotron row"">
        <div class=""col-md-2"">
            <i class=""fa fa-camera fa-5x""></i>
        </div>
        <div class=""col-md-10"">
            <h2>
            {% if Person.NickName != Empty %}{{ Person.NickName }}, {% endif %}
            Upload Your Photo</h2>
            <p>
                Thanks for taking the time to improve the quality of {{ GlobalAttribute.OrganizationName }}.
                To upload a photo simply click the ''Select Photo'' next to
                the individuals below, select the desired photo, and
                press ''Open''. It''s as easy as that!
            </p>
        </div>
    </div>'
        ,1
        ,'33A47BDE-2CFE-487B-9786-4847CE45C44F')
" );
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            Sql( @"
    DELETE [HtmlContent] WHERE [Guid] = '33A47BDE-2CFE-487B-9786-4847CE45C44F'
" );
            RockMigrationHelper.DeleteBlockAttributeValue( "1FC171B7-6555-4557-84E9-89A2C5E4051D", "4DFDB295-6D0F-40A1-BEF9-7B70C56F66C4" );
            RockMigrationHelper.DeleteBlockAttribute( "69748C7C-AE7A-4C3D-A5B6-BF29EA4685A4" );
            RockMigrationHelper.DeleteBlockAttribute( "2C9DC553-2CB9-4FAD-A87C-43A53EE652B6" );

            // Remove Block: Instructions, from Page: Photo Upload, Site: Rock Solid Church
            RockMigrationHelper.DeleteBlock( "1FC171B7-6555-4557-84E9-89A2C5E4051D" );
            // Remove Block: Upload, from Page: Photo Upload, Site: Rock Solid Church
            RockMigrationHelper.DeleteBlock( "E3009763-C34E-4F1C-985F-4CE9A9CA0715" );

            RockMigrationHelper.DeleteBlockType( "7764E323-7460-4CB7-8024-056136C99603" ); // Upload
            RockMigrationHelper.DeletePage( "8559A9F1-C6A4-4945-B393-74F6706A8FA2" );
        }
    }
}
