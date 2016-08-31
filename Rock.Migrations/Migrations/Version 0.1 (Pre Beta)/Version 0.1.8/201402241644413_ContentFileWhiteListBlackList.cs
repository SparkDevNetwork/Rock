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
    public partial class ContentFileWhiteListBlackList : Rock.Migrations.RockMigration4
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            AddGlobalAttribute( "9C204CD0-1233-41C5-818A-C5DA439445AA", "", "", "Content Image Filetype Whitelist", "List of file types that are allowed to be uploaded as images in the HTML Editor.", 0, "jpg,png,gif,bmp,svg", "0F842054-7629-419F-BC72-90BDDE9F3676" );
            AddGlobalAttribute( "9C204CD0-1233-41C5-818A-C5DA439445AA", "", "", "Content Filetype Blacklist", "List of file types are not allowed to be uploaded in the HTML Editor.", 0, "ascx,ashx,aspx,ascx.cs,ashx.cs,aspx.cs,cs,aspx.cs,php,exe,dll", "9FFB15C1-AA53-4FBA-A480-64C9B348C5E5" );

            // set attribute category
            Sql( @"
                DELETE FROM [AttributeCategory] WHERE [AttributeId] in (SELECT [ID] FROM [Attribute] WHERE [GUID] in ('0F842054-7629-419F-BC72-90BDDE9F3676', '9FFB15C1-AA53-4FBA-A480-64C9B348C5E5'));

                INSERT INTO [AttributeCategory] ([AttributeId], [CategoryId])
                    SELECT [ID], 5 FROM [Attribute] WHERE [GUID] in ('0F842054-7629-419F-BC72-90BDDE9F3676', '9FFB15C1-AA53-4FBA-A480-64C9B348C5E5');
            " );
        }

        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            DeleteAttribute( "0F842054-7629-419F-BC72-90BDDE9F3676" );
            DeleteAttribute( "9FFB15C1-AA53-4FBA-A480-64C9B348C5E5" );
        }
    }
}
