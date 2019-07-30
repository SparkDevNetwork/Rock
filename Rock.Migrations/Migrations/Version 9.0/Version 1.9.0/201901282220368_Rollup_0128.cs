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
    public partial class Rollup_0128 : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            CreateContentWhiteListUp();
            UpdateContentBlackList();
            PersonPickerRestActionUp();
            RemoveIndexingOnPages();
            UpdateCommunicationSettingsNameDesc();
            UpdateCheckinSuccessTemplate();
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            CreateContentWhiteListDown();
            PersonPickerRestActionDown();
        }

        /// <summary>
        /// Creates the content white list global attribute and adds it to the Config category.
        /// </summary>
        private void CreateContentWhiteListUp()
        {
            RockMigrationHelper.AddGlobalAttribute( "9C204CD0-1233-41C5-818A-C5DA439445AA", "", "", "Content Filetype Whitelist", "A comma or semicolon separated list of file types that are allowed to be uploaded to the file system. If left blank then all files are allowed unless they are in \"Content Filetype Blacklist\". Filetypes in this list have a lower preference then the ones in \"Content Filetype Blacklist\". i.e. if a file type exists in both lists then it will not be allowed. This list does not prevent files from uploading as a binary file type which is saved to the database.", 0, "", "B895B6D7-BA21-45C0-8913-EF47FAAD69B1", "ContentFiletypeWhitelist", false );

            // Add the Content Filetype Whitelist to the config category
            Sql( @"
                DECLARE @attributeId INT = (SELECT [Id] FROM [Attribute] WHERE [Guid] = 'B895B6D7-BA21-45C0-8913-EF47FAAD69B1')

                DELETE FROM [AttributeCategory] WHERE [AttributeId] = @attributeId

                INSERT INTO [AttributeCategory] ([AttributeId], [CategoryId])
                VALUES(@attributeId, 5)" );
        }

        private void CreateContentWhiteListDown()
        {
            // Remove the Content Filetype Whitelist from the config category
            Sql( @"
                DECLARE @attributeId INT = (SELECT [Id] FROM [Attribute] WHERE [Guid] = 'B895B6D7-BA21-45C0-8913-EF47FAAD69B1')
                DELETE FROM [AttributeCategory] WHERE [AttributeId] = @attributeId" );

            RockMigrationHelper.DeleteAttribute( "B895B6D7-BA21-45C0-8913-EF47FAAD69B1" );
        }

        /// <summary>
        /// Add the config file type to the black list global attribute
        /// Does not have a down because it is conceivable that the users would have added config on their own.
        /// </summary>
        private void UpdateContentBlackList()
        {
            Sql( @"
                DECLARE @attributeId int = (SELECT [Id] FROM [Attribute] WHERE [Guid] = '9FFB15C1-AA53-4FBA-A480-64C9B348C5E5')

                -- First update the blacklist default value to include config files.
                IF ( SELECT COUNT(*) FROM [Attribute] WHERE [Id] = @attributeId AND [DefaultValue] LIKE '%config%') = 0
                BEGIN
	                UPDATE [Attribute] SET [DefaultValue] = 'ascx, ashx, aspx, ascx.cs, ashx.cs, aspx.cs, cs, aspx.cs, php, exe, dll, config' WHERE [Id] = @attributeId
                END

                -- Second find out if there is a custom blacklist value set, if not then we can stop
                DECLARE @valCount int = (SELECT COUNT(*)
	                FROM [Attribute] a
	                JOIN [AttributeValue] av on a.[Id] = av.[AttributeId]
	                WHERE a.[Id] = @attributeId)

                IF (@valCount > 0)
                BEGIN
	                -- See if the current AttributeValue.Value includes config, if not then add it to the end of the current list.
	                IF (SELECT [Value] FROM [AttributeValue] WHERE AttributeId = @attributeId AND [Value] NOT LIKE '%config%') IS NOT NULL
	                BEGIN
		                UPDATE [AttributeValue] SET [Value] = [Value] + ', config' WHERE [AttributeId] = @attributeId
	                END
                END" );
        }

        /// <summary>
        /// MP: Update RestAction name for Person Picker search
        /// </summary>
        private void PersonPickerRestActionUp()
        {
             // MP -Update GETapi/People/Search to new ApiId and Path
             Sql( @"
            UPDATE RestAction
            SET [ApiId] = 'GETapi/People/Search?name={name}&includeDetails={includeDetails}&includeBusinesses={includeBusinesses}&includeDeceased={includeDeceased}&address={address}&phone={phone}&email={email}'
                ,[Path] = 'api/People/Search?name={name}&includeDetails={includeDetails}&includeBusinesses={includeBusinesses}&includeDeceased={includeDeceased}&address={address}&phone={phone}&email={email}'
            WHERE [ApiId] = 'GETapi/People/Search?name={name}&includeHtml={includeHtml}&includeDetails={includeDetails}&includeBusinesses={includeBusinesses}&includeDeceased={includeDeceased}'
                AND [Path] = 'api/People/Search?name={name}&includeHtml={includeHtml}&includeDetails={includeDetails}&includeBusinesses={includeBusinesses}&includeDeceased={includeDeceased}'
                AND [Method] = 'GET'" );
        }

        /// <summary>
        /// MP: Update RestAction name for Person Picker search
        /// </summary>
        private void PersonPickerRestActionDown()
        {
            // MP -Revert GETapi/People/Search back to before
             Sql( @"
                UPDATE RestAction
                SET [ApiId] = 'GETapi/People/Search?name={name}&includeHtml={includeHtml}&includeDetails={includeDetails}&includeBusinesses={includeBusinesses}&includeDeceased={includeDeceased}'
                    ,[Path] = 'api/People/Search?name={name}&includeHtml={includeHtml}&includeDetails={includeDetails}&includeBusinesses={includeBusinesses}&includeDeceased={includeDeceased}'
                WHERE [ApiId] = 'GETapi/People/Search?name={name}&includeDetails={includeDetails}&includeBusinesses={includeBusinesses}&includeDeceased={includeDeceased}&address={address}&phone={phone}&email={email}'
                    AND [Path] = 'api/People/Search?name={name}&includeDetails={includeDetails}&includeBusinesses={includeBusinesses}&includeDeceased={includeDeceased}&address={address}&phone={phone}&email={email}'
                    AND [Method] = 'GET'" );
        }

        /// <summary>
        /// SK: Remove Allow Indexing on un-indexable pages
        /// </summary>
        private void RemoveIndexingOnPages()
        {
            Sql( @"
                UPDATE [Page]
                SET [AllowIndexing] = 0
                WHERE [Guid] IN (
                     'C9767AC5-11A8-4B48-B487-911BA9CADF8C'
			        ,'80613598-D1F6-4819-BCB0-7204E59D98AC'
			        ,'A50EBCA2-11B3-4DD2-A2DD-E7939EDDF23F'
			        ,'B37D22BE-D2A8-4EFA-8B2B-2E0EFF6EDB44'
			        ,'C8CEF4B0-4A09-46D2-9B6B-CD2B6D3078B1'
                )" );
        }

        /// <summary>
        /// SK: Updated Communication Settings Name and description
        /// </summary>
        private void UpdateCommunicationSettingsNameDesc()
        {
            Sql( @"
                UPDATE [Attribute]
                SET [Name]='Enable Do Not Disturb'
	                , [Description] = 'When enabled, communications will not be sent between the starting and ending ''Do Not Disturb'' hours.'
                WHERE [Guid]='1BE30413-5C90-4B78-B324-BD31AA83C002'

                UPDATE [Attribute]
                SET [Name]='Do Not Disturb Start'
	                , [Description] = 'The hour that starts the Do Not Disturb window.'
                WHERE [Guid]='4A558666-32C7-4490-B860-0F41358E14CA'

                UPDATE [Attribute]
                SET [Name]='Do Not Disturb End'
	                , [Description] = 'The hour that ends the Do Not Disturb window.'
                WHERE [Guid]='661802FC-E636-4CE2-B75A-4AC05595A347'" );
        }

        /// <summary>
        /// SK: Updated Check-in Success Template to include error messages when capacity increases threshold value
        /// </summary>
        private void UpdateCheckinSuccessTemplate()
        {
             Sql( @"
                DECLARE @CheckInSuccessTemplateId INT = (SELECT [Id] FROM [Attribute] WHERE [Guid] = 'F5BA6DCC-0A4D-4616-871D-ECBA7082C45F')

                UPDATE [Attribute]
                SET [DefaultValue] = '<ol class=''checkin-messages checkin-body-container''>
{% for checkinMessage in Messages %}
    <li><div class=''alert alert-{{ checkinMessage.MessageType | Downcase }}''>
        {{ checkinMessage.MessageText }}
        </div>
    </li>
    {% endfor %}
</ol>
' + [DefaultValue]
                WHERE [Id] = @CheckInSuccessTemplateId AND [DefaultValue] NOT LIKE '%checkinMessage in Messages%'

                UPDATE [AttributeValue]
                SET [Value] = '<ol class=''checkin-messages checkin-body-container''>
{% for checkinMessage in Messages %}
    <li><div class=''alert alert-{{ checkinMessage.MessageType | Downcase }}''>
        {{ checkinMessage.MessageText }}
        </div>
    </li>
    {% endfor %}
</ol>
' + [Value]
                WHERE [AttributeId] = @CheckInSuccessTemplateId AND [Value] NOT LIKE '%checkinMessage in Messages%'" );
        }


    }
}
