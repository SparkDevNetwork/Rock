using System;
using Rock.Plugin;

namespace com.minecartstudio.PbxSwitchvox.Migrations
{
    [MigrationNumber( 1, "1.7.0" )]
    public class IntitalSetup : Rock.Plugin.Migration
    {
        public override void Up()
        {
            // todo this should be core?
            RockMigrationHelper.AddDefinedValue( Rock.SystemGuid.DefinedType.INTERACTION_CHANNEL_MEDIUM, "PBX CDR", "Used for tracking phone calls coming from phone systems.", SystemGuid.DefinedValue.PBX_CDR_MEDIUM_VALUE );

            // add component
            RockMigrationHelper.UpdateEntityType( "com.minecartstudio.PbxSwitchvox.Pbx.Provider.Switchvox", "Switchvox PBX Provider", "com.minecartstudio.PbxSwitchvox.Pbx.Provider.Switchvox, com.minecartstudio.PbxSwitchvox, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null", false, true, SystemGuid.EntityType.PBX_SWITCHVOX );

            // add interaction channel and component
            Sql( string.Format( @"
  DECLARE @SwitchvoxEntityId int = (SELECT TOP 1 [Id] FROM [EntityType] WHERE [Name] = 'com.minecartstudio.PbxSwitchvox.Pbx.Provider.Switchvox')
  DECLARE @InteractionEntityTypeId int = (SELECT TOP 1 [Id] FROM [EntityType] WHERE [Name] = 'Rock.Model.Person')
  DECLARE @ChannelTypeMediumValueId int = (SELECT TOP 1 [Id] FROM [DefinedValue] WHERE [Guid] = '{0}')

  INSERT INTO [InteractionChannel]
  ([Name], [InteractionEntityTypeId], [ChannelTypeMediumValueId], [Guid])
  VALUES
  ('PBX CDR Records', @InteractionEntityTypeId, @ChannelTypeMediumValueId, '{1}')

  DECLARE @ChannelId int = SCOPE_IDENTITY()

  INSERT INTO [InteractionComponent]
  ([Name], [EntityId], [ChannelId], [Guid])
  VALUES
  ('Switchvox', @SwitchvoxEntityId, @ChannelId, '{2}')", SystemGuid.DefinedValue.PBX_CDR_MEDIUM_VALUE,  SystemGuid.InteractionChannel.PBX_CDR, SystemGuid.InteractionComponent.PBX_SWITCHVOX) );
        }

        public override void Down()
        {
            throw new NotImplementedException();
        }

        
    }
}
