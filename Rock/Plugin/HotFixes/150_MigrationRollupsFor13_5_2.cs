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

namespace Rock.Plugin.HotFixes
{
    /// <summary>
    /// Plug-in migration
    /// </summary>
    /// <seealso cref="Rock.Plugin.Migration" />
    [MigrationNumber( 150, "1.13.4" )]
    public class MigrationRollupsFor13_5_2 : Migration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            WindowsCheckinClientDownloadLinkUp();
            UpdateStatementGeneratorDownloadLinkUp();
            AddBlockSettingToGroupScheduleToolboxv2Up();
            UpdateGroupTypeDefaultLavaUp();
            UpdatePanelLavaShortCodeDefault();
        }

        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            // Down migrations are not yet supported in plug-in migrations.
        }

        /// <summary>
        /// MP: Update Windows Check-in Client Download Link
        /// </summary>
        private void WindowsCheckinClientDownloadLinkUp()
        {
            Sql( @"
                DECLARE @winCheckinClientDefinedValueId INT = (SELECT Id FROM [DefinedValue] WHERE [Guid] = 'C162F21E-7D86-4BB4-A72C-7F4A0E5B02C3')
                DECLARE @downloadUrlAttributeId INT = (SELECT [Id] FROM [Attribute] WHERE [Guid] = 'E0AF9B30-15EA-413B-BAC4-25B286D91FD9')

                UPDATE [AttributeValue]
                SET [Value] = 'https://storage.rockrms.com/externalapplications/sparkdevnetwork/windowscheckin/1.13.0/checkinclient.msi'
                WHERE AttributeId = @downloadUrlAttributeId and EntityId = @winCheckinClientDefinedValueId" );
        }

        /// <summary>
        /// MP: Update Windows Check-in Client Download Link - Restores the old Rock Windows Check-in Client download link.
        /// </summary>
        private void WindowsCheckinClientDownloadLinkDown()
        {
            Sql( @"
                DECLARE @winCheckinClientDefinedValueId INT = (SELECT Id FROM [DefinedValue] WHERE [Guid] = 'C162F21E-7D86-4BB4-A72C-7F4A0E5B02C3')
                DECLARE @downloadUrlAttributeId INT = (SELECT [Id] FROM [Attribute] WHERE [Guid] = 'E0AF9B30-15EA-413B-BAC4-25B286D91FD9')

                UPDATE [AttributeValue]
                SET [Value] = 'https://storage.rockrms.com/externalapplications/sparkdevnetwork/windowscheckin/1.13.5/checkinclient.msi'
                WHERE AttributeId = @downloadUrlAttributeId and EntityId = @winCheckinClientDefinedValueId" );
        }

        /// <summary>
        /// MP: Statement Generator Download Location - Updates the statement generator download link up.
        /// </summary>
        private void UpdateStatementGeneratorDownloadLinkUp()
        {
            Sql( @"
                DECLARE @statementGeneratorDefinedValueId INT = (SELECT Id FROM [DefinedValue] WHERE [Guid] = '54E1EBCC-5A5A-4B26-9CCB-36E7CEB49C3C')
                DECLARE @downloadUrlAttributeId INT = (SELECT [Id] FROM [Attribute] WHERE [Guid] = 'E0AF9B30-15EA-413B-BAC4-25B286D91FD9')

                UPDATE [AttributeValue]
                SET [Value] = 'https://storage.rockrms.com/externalapplications/sparkdevnetwork/statementgenerator/1.13.5/statementgenerator.msi'
                WHERE AttributeId = @downloadUrlAttributeId and EntityId = @statementGeneratorDefinedValueId" );
        }

        /// <summary>
        /// MP: Statement Generator Download Location - Updates the statement generator download link down.
        /// </summary>
        private void UpdateStatementGeneratorDownloadLinkDown()
        {
            Sql( @"
                DECLARE @statementGeneratorDefinedValueId INT = (SELECT Id FROM [DefinedValue] WHERE [Guid] = '54E1EBCC-5A5A-4B26-9CCB-36E7CEB49C3C')
                DECLARE @downloadUrlAttributeId INT = (SELECT [Id] FROM [Attribute] WHERE [Guid] = 'E0AF9B30-15EA-413B-BAC4-25B286D91FD9')

                UPDATE [AttributeValue]
                SET [Value] = 'https://storage.rockrms.com/externalapplications/sparkdevnetwork/statementgenerator/1.13.1/statementgenerator.msi'
                WHERE AttributeId = @downloadUrlAttributeId and EntityId = @statementGeneratorDefinedValueId" );
        }

        /// <summary>
        /// SK: Add BlockSetting to Group Schedule Toolbox v2 and Replace Group Schedule Toolbox to Group Schedule Toolbox V2 in Person Profile Page
        /// </summary>
        private void AddBlockSettingToGroupScheduleToolboxv2Up()
        {
            // Attribute for BlockType              //   BlockType: Group Schedule Toolbox v2              //   Category: Group Scheduling              //   Attribute: Schedule Unavailability Header
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "18A6DCE3-376C-4A62-B1DD-5E5177C11595", "1D0D3794-C210-48A8-8C68-3FBEC08A6BA5", "Schedule Unavailability Header", "ScheduleUnavailabilityHeader", "Schedule Unavailability Header", @"Header content to put on the Schedule Unavailability panel. <span class='tip tip-lava'></span>", 13, @"  <p>      <a class=""btn btn-sm btn-default"" href=""javascript:history.back()""><i class=""fa fa-chevron-left""></i> Back</a>  </p>", "B175A2F3-0A8A-48D2-B122-FDBBD7EA44C0" );
            // Attribute for BlockType              //   BlockType: Group Schedule Toolbox v2              //   Category: Group Scheduling              //   Attribute: Update Schedule Preferences Header
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "18A6DCE3-376C-4A62-B1DD-5E5177C11595", "1D0D3794-C210-48A8-8C68-3FBEC08A6BA5", "Update Schedule Preferences Header", "UpdateSchedulePreferencesHeader", "Update Schedule Preferences Header", @"Header content to put on the Update Schedule Preferences panel. <span class='tip tip-lava'></span>", 14, @"  <p>      <a class=""btn btn-sm btn-default"" href=""javascript:history.back()""><i class=""fa fa-chevron-left""></i> Back</a>  </p>", "7AE2253D-4A0E-41A3-BDE4-34B350FD1E2E" );
            // Attribute for BlockType              //   BlockType: Group Schedule Toolbox v2              //   Category: Group Scheduling              //   Attribute: Sign-up for Additional Times Header
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "18A6DCE3-376C-4A62-B1DD-5E5177C11595", "1D0D3794-C210-48A8-8C68-3FBEC08A6BA5", "Sign-up for Additional Times Header", "SignupforAdditionalTimesHeader", "Sign-up for Additional Times Header", @"Header content to put on the Sign-up for Additional Times panel. <span class='tip tip-lava'></span>", 15, @"  <p>      <a class=""btn btn-sm btn-default"" href=""javascript:history.back()""><i class=""fa fa-chevron-left""></i> Back</a>  </p>", "5EAC6DAB-4E16-4302-B388-E77ED31EBFBB" );
            // Attribute for BlockType              //   BlockType: Group Schedule Toolbox v2              //   Category: Group Scheduling              //   Attribute: Override Hide from Toolbox
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "18A6DCE3-376C-4A62-B1DD-5E5177C11595", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Override Hide from Toolbox", "OverrideHideFromToolbox", "Override Hide from Toolbox", @" When enabled this setting will show all schedule enabled groups no matter what their 'Disable Schedule Toolbox Access' setting is set to.", 12, @"False", "4C4E1DAD-F2D8-4F53-8A5E-762E8E2E937E" );

            Sql( @"
                DECLARE @GroupScheduleToolboxId int = ( SELECT TOP 1 [Id] FROM [BlockType] WHERE [Guid] = '7F9CEA6F-DCE5-4F60-A551-924965289F1D' )
                DECLARE @PageId int = ( SELECT TOP 1 [Id] FROM [Page] WHERE [Guid] = '183B7B7E-105A-4C9A-A4BC-06CD26B7FE6D' )
                DECLARE @GroupScheduleToolboxBlockId int = ( SELECT TOP 1 [Id] FROM [Block] WHERE [BlockTypeId] = @GroupScheduleToolboxId AND [PageId]=@PageId  )
                DECLARE @GroupScheduleToolboxV2Id int = ( SELECT TOP 1 [Id] FROM [BlockType] WHERE [Guid] = '18A6DCE3-376C-4A62-B1DD-5E5177C11595' )

                IF @GroupScheduleToolboxBlockId IS NOT NULL
                BEGIN
                        -- update block of Group Schedule Toolbox block type with Group Schedule Toolbox v2  Block Type Id
                        UPDATE 
                            [dbo].[Block]
                        SET [BlockTypeId] = @GroupScheduleToolboxV2Id
                        WHERE
                            [Id] = @GroupScheduleToolboxBlockId

                        UPDATE
                            a
                        SET a.AttributeId=c.[Id]
                        FROM [dbo].[AttributeValue] a INNER JOIN [dbo].[Attribute] b ON a.AttributeId = b.[Id] AND b.[EntityTypeQualifierColumn] = 'BlockTypeId' and b.EntityTypeQualifierValue = @GroupScheduleToolboxId
                        INNER JOIN [dbo].[Attribute] c ON c.[EntityTypeQualifierColumn] = 'BlockTypeId' AND c.[EntityTypeQualifierValue] = @GroupScheduleToolboxV2Id AND c.[Key] = b.[Key]
                        WHERE a.[EntityId] = @GroupScheduleToolboxBlockId

                        UPDATE
                            a
                        SET a.AttributeId=c.[Id]
                        FROM [dbo].[AttributeValue] a INNER JOIN [dbo].[Attribute] b ON a.AttributeId = b.[Id] AND b.[EntityTypeQualifierColumn] = 'BlockTypeId' and b.EntityTypeQualifierValue = @GroupScheduleToolboxId
                        INNER JOIN [dbo].[Attribute] c ON c.[EntityTypeQualifierColumn] = 'BlockTypeId' AND c.[EntityTypeQualifierValue] = @GroupScheduleToolboxV2Id AND c.[Key] = 'EnableAdditionalTimeSignUp' AND B.[Key]='EnableSignup'
                        WHERE a.[EntityId] = @GroupScheduleToolboxBlockId
                END
                " );

            // Block Attribute Value for Override Hide from Toolbox
            RockMigrationHelper.AddBlockAttributeValue( "47199FAE-BB88-4CDC-B9EA-5BAB72042D64", "4C4E1DAD-F2D8-4F53-8A5E-762E8E2E937E", @"True" );
        }

        /// <summary>
        /// GJ: Update Group Type Default Lava to hide if value is blank
        /// </summary>
        private void UpdateGroupTypeDefaultLavaUp()
        {
            Sql( @"
UPDATE
        [GroupType] 
SET [GroupViewLavaTemplate] = REPLACE([GroupViewLavaTemplate], '<dl>
        {% for attribute in Group.AttributeValues %}
        <dt>{{ attribute.AttributeName }}:</dt>

<dd>{{ attribute.ValueFormatted }} </dd>
        {% endfor %}
        </dl>', '<dl>
        {% for attribute in Group.AttributeValues %}
            {% if attribute.ValueFormatted != '''' %}
                <dt>{{ attribute.AttributeName }}</dt>
                <dd>{{ attribute.ValueFormatted }}</dd>
            {% endif %}
        {% endfor %}
        </dl>')
WHERE
        [GroupViewLavaTemplate] LIKE '%<dl>
        {% for attribute in Group.AttributeValues %}
        <dt>{{ attribute.AttributeName }}:</dt>

<dd>{{ attribute.ValueFormatted }} </dd>
        {% endfor %}
        </dl>%'
" );
        }

        /// <summary>
        /// Updates the panel lava short code default.
        /// </summary>
        private void UpdatePanelLavaShortCodeDefault()
        {
            Sql( @"
                UPDATE [LavaShortcode]
                SET [Documentation]=N'<p>
The panel shortcode allows you to easily add a 
<a href=""https://community.rockrms.com/styling/components/panels"" target=""_blank"">Bootstrap panel</a> to your markup. This is a pretty simple shortcode, but it does save you some time.
</p>

<p>Basic Usage:<br>  
</p><pre>{[ panel title:''Important Stuff'' icon:''fa fa-star'' ]}<br>  
This is a super simple panel.<br> 
{[ endpanel ]}</pre>

<p></p><p>
As you can see the body of the shortcode is placed in the body of the panel. Optional parameters include:
</p>

<ul>
<li><b>title</b> – The title to show in the heading. If no title is provided then the panel title section will not be displayed.</li>
<li><b>icon </b> – The icon to use with the title.</li>
<li><b>footer</b> – If provided the text will be placed in the panel’s footer.</li>
<li><b>type</b> (default) – Change the type of panel displayed. Options include: default, primary, success, info, warning, danger, block and widget.</li>
</ul>', [Parameters]=N'type^default|icon^|title^|footer^'
                WHERE ([Guid]='ADB1F75D-500D-4305-9805-99AF04A2CD88')" );
        }
    }
}
