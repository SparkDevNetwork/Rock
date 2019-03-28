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
    /// 
    /// </summary>
    /// <seealso cref="Rock.Plugin.Migration" />
    [MigrationNumber( 39, "1.7.0" )]
    public class MigrationRollupsForV7_2 : Migration
    {
        /// <summary>
        /// The commands to run to migrate plugin to the specific version
        /// </summary>
        public override void Up()
        {
            // MP:Fix for #2700
            // Moved to 201711162212312_SiteFontAwesome
            //RockMigrationHelper.AddSecurityAuthForBinaryFileType( Rock.SystemGuid.BinaryFiletype.COMMUNICATION_ATTACHMENT, 0, Rock.Security.Authorization.EDIT, true, Rock.SystemGuid.Group.GROUP_STAFF_LIKE_MEMBERS, Rock.Model.SpecialRole.None, "CC13A66F-282E-4739-AC56-A95C03EFA22E" );
            //RockMigrationHelper.AddSecurityAuthForBinaryFileType( Rock.SystemGuid.BinaryFiletype.COMMUNICATION_ATTACHMENT, 1, Rock.Security.Authorization.EDIT, true, Rock.SystemGuid.Group.GROUP_STAFF_MEMBERS, Rock.Model.SpecialRole.None, "E5D49B9C-DB39-42F9-9807-A4C0A96BD5C6" );
            //RockMigrationHelper.AddSecurityAuthForBinaryFileType( Rock.SystemGuid.BinaryFiletype.COMMUNICATION_IMAGE, 0, Rock.Security.Authorization.EDIT, true, Rock.SystemGuid.Group.GROUP_STAFF_LIKE_MEMBERS, Rock.Model.SpecialRole.None, "540B63CA-A6ED-4F8E-B2E3-0A2EC815C7DE" );
            //RockMigrationHelper.AddSecurityAuthForBinaryFileType( Rock.SystemGuid.BinaryFiletype.COMMUNICATION_IMAGE, 1, Rock.Security.Authorization.EDIT, true, Rock.SystemGuid.Group.GROUP_STAFF_MEMBERS, Rock.Model.SpecialRole.None, "A963D209-94DC-4A4E-AD6F-6922558F77B9" );

            // MP:Fix for Label Merge Fields Page HelpText corruption
            // Original Migration was fixed, so this isn't needed in develop branch
            // Sql( HotFixMigrationResource._039_MigrationRollupsForV7_2_UpdateCheckInMergefieldDebugInfo );

            // JE:Add Interaction Channel for CDR
            // Moved to 201801291304513_Rollup_0129
            //            Sql( @"DECLARE @ChannelGuid uniqueidentifier
            //SET @ChannelGuid = CAST( 'B3904B57-62A2-57AC-43EA-94D4DEBA3D51' as uniqueidentifier )

            //IF NOT EXISTS( SELECT * FROM[InteractionChannel]  WHERE[Guid] = @ChannelGuid )
            //BEGIN
            //  DECLARE @InteractionEntityTypeId int = ( SELECT TOP 1[Id] FROM[EntityType] WHERE[Name] = 'Rock.Model.Person' )
            //  DECLARE @ChannelTypeMediumValueId int = ( SELECT TOP 1[Id] FROM[DefinedValue] WHERE[Guid] = 'B3904B57-62A2-57AC-43EA-94D4DEBA3D51')

            //  INSERT INTO[InteractionChannel]

            //    ([Name], [InteractionEntityTypeId], [ChannelTypeMediumValueId], [Guid])
            //  VALUES
            //    ( 'PBX CDR Records', @InteractionEntityTypeId, @ChannelTypeMediumValueId, @ChannelGuid )
            //END" );

            // Moved to 201712192301416_MoveInteractionTemplates
            //// MP: Add new 'Communication Page' BlockSetting to Bio and set bio block on the internal person profile page to use the Simple Communication page.
            //// Attrib for BlockType: Person Bio:Communication Page
            //RockMigrationHelper.UpdateBlockTypeAttribute( "0F5922BB-CD68-40AC-BF3C-4AAB1B98760C", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Communication Page", "CommunicationPage", "", @"The communication page to use for when the person's email address is clicked. Leave this blank to use the default.", 15, @"", "18B917AE-6395-4B70-95F4-AA5E6EA1F799" );
            //// Attrib Value for Block:Bio, Attribute:Communication Page , Layout: PersonDetail, Site: Rock RMS
            //RockMigrationHelper.AddBlockAttributeValue( "B5C1FDB6-0224-43E4-8E26-6B2EAF86253A", "18B917AE-6395-4B70-95F4-AA5E6EA1F799", @"7e8408b2-354c-4a5a-8707-36754ae80b9a" );
        }

        /// <summary>
        /// The commands to undo a migration from a specific version
        /// </summary>
        public override void Down()
        {
        }
    }
}
