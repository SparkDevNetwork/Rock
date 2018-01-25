using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
            // Fix for #2700
            RockMigrationHelper.AddSecurityAuthForBinaryFileType( Rock.SystemGuid.BinaryFiletype.COMMUNICATION_ATTACHMENT, 0, Rock.Security.Authorization.EDIT, true, Rock.SystemGuid.Group.GROUP_STAFF_LIKE_MEMBERS, Rock.Model.SpecialRole.None, "CC13A66F-282E-4739-AC56-A95C03EFA22E" );
            RockMigrationHelper.AddSecurityAuthForBinaryFileType( Rock.SystemGuid.BinaryFiletype.COMMUNICATION_ATTACHMENT, 1, Rock.Security.Authorization.EDIT, true, Rock.SystemGuid.Group.GROUP_STAFF_MEMBERS, Rock.Model.SpecialRole.None, "E5D49B9C-DB39-42F9-9807-A4C0A96BD5C6" );
            RockMigrationHelper.AddSecurityAuthForBinaryFileType( Rock.SystemGuid.BinaryFiletype.COMMUNICATION_IMAGE, 0, Rock.Security.Authorization.EDIT, true, Rock.SystemGuid.Group.GROUP_STAFF_LIKE_MEMBERS, Rock.Model.SpecialRole.None, "540B63CA-A6ED-4F8E-B2E3-0A2EC815C7DE" );
            RockMigrationHelper.AddSecurityAuthForBinaryFileType( Rock.SystemGuid.BinaryFiletype.COMMUNICATION_IMAGE, 1, Rock.Security.Authorization.EDIT, true, Rock.SystemGuid.Group.GROUP_STAFF_MEMBERS, Rock.Model.SpecialRole.None, "A963D209-94DC-4A4E-AD6F-6922558F77B9" );

            // Fix for Label Merge Fields Page HelpText corruption
            Sql( HotFixMigrationResource._039_MigrationRollupsForV7_2_UpdateCheckInMergefieldDebugInfo );

            // Add Interaction Channel for CDR
            Sql( @"DECLARE @ChannelGuid uniqueidentifier
SET @ChannelGuid = CAST( 'B3904B57-62A2-57AC-43EA-94D4DEBA3D51' as uniqueidentifier )

IF NOT EXISTS( SELECT * FROM[InteractionChannel]  WHERE[Guid] = @ChannelGuid )
BEGIN
  DECLARE @InteractionEntityTypeId int = ( SELECT TOP 1[Id] FROM[EntityType] WHERE[Name] = 'Rock.Model.Person' )
  DECLARE @ChannelTypeMediumValueId int = ( SELECT TOP 1[Id] FROM[DefinedValue] WHERE[Guid] = 'B3904B57-62A2-57AC-43EA-94D4DEBA3D51')

  INSERT INTO[InteractionChannel]

    ([Name], [InteractionEntityTypeId], [ChannelTypeMediumValueId], [Guid])
  VALUES
    ( 'PBX CDR Records', @InteractionEntityTypeId, @ChannelTypeMediumValueId, @ChannelGuid )
END" );
        }

        /// <summary>
        /// The commands to undo a migration from a specific version
        /// </summary>
        public override void Down()
        {
        }
    }
}
