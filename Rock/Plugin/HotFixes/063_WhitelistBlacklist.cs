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
namespace Rock.Plugin.HotFixes
{

    /// <summary>
    /// This hotfix update is to address security concerns in the file uploader and is being retroactivly applied to v7.6 and >.
    /// </summary>
    /// <seealso cref="Rock.Plugin.Migration" />
    [MigrationNumber( 63, "1.7.5" )]
    public class WhitelistBlacklist : Migration
    {
        /// <summary>
        /// The commands to run to migrate plugin to the specific version
        /// </summary>
        public override void Up()
        {
            //UpdateContentBlackList();
            //CreateContentWhiteList();
        }

        /// <summary>
        /// The commands to undo a migration from a specific version
        /// </summary>
        public override void Down()
        {
            // Down migration functionality not yet available for hotfix migrations.
        }

        /// <summary>
        /// Add the config file type to the black list global attribute
        /// </summary>
        private void UpdateContentBlackList()
        {
            Sql( @"
                -- First update the blacklist default value to include config files.
                UPDATE [Attribute] SET [DefaultValue] = 'ascx, ashx, aspx, ascx.cs, ashx.cs, aspx.cs, cs, aspx.cs, php, exe, dll, config' WHERE [Guid] = '9FFB15C1-AA53-4FBA-A480-64C9B348C5E5'

                -- Second find out if there is a custom blacklist value set, if not then we can stop
                DECLARE @valCount int = (SELECT COUNT(*)
	                FROM [Attribute] a
	                JOIN [AttributeValue] av on a.[Id] = av.[AttributeId]
	                WHERE a.[Guid] = '9FFB15C1-AA53-4FBA-A480-64C9B348C5E5')

                IF (@valCount > 0)
                BEGIN
	                -- We have a custom defined blacklist value that we need to add config to if it is not there already
	                DECLARE @attributeId int = (SELECT [Id] FROM [Attribute] WHERE [Guid] = '9FFB15C1-AA53-4FBA-A480-64C9B348C5E5')

	                -- See if the current AttributeValue.Value includes config, if not then add it to the end of the current list.
	                IF (SELECT [Value] FROM [AttributeValue] WHERE AttributeId = @attributeId AND [Value] NOT LIKE '%config%') IS NOT NULL
	                BEGIN
		                UPDATE [AttributeValue] SET [Value] = [Value] + ', config' WHERE [AttributeId] = @attributeId
	                END
                END" );
        }

        /// <summary>
        /// Creates the content white list global attribute and adds it to the Config category.
        /// </summary>
        private void CreateContentWhiteList()
        {
            RockMigrationHelper.AddGlobalAttribute( "9C204CD0-1233-41C5-818A-C5DA439445AA", "", "", "Content Filetype Whitelist", "A comma or semicolon separated list of file types that are allowed to be uploaded to the file system. If left blank then all files are allowed unless they are in \"Content Filetype Blacklist\". Filetypes in this list have a lower preference then the ones in \"Content Filetype Blacklist\". i.e. if a file type exists in both lists then it will not be allowed. This list does not prevent files from uploading as a binary file type which is saved to the database.", 0, "", SystemGuid.Attribute.CONTENT_FILETYPE_WHITELIST, "ContentFiletypeWhitelist", false );

            // Add the Content Filetype Whitelist to the config category
            Sql( @"
                DECLARE @attributeId INT = (SELECT [Id] FROM [Attribute] WHERE [Guid] = 'B895B6D7-BA21-45C0-8913-EF47FAAD69B1')

                DELETE FROM [AttributeCategory] WHERE [AttributeId] = @attributeId

                INSERT INTO [AttributeCategory] ([AttributeId], [CategoryId])
                VALUES(@attributeId, 5)" );
        }
    }
}
