//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//
namespace Rock.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    /// <summary>
    /// 
    /// </summary>
    public partial class AddDeviceBlocks : RockMigration_4
    {

        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            AddPage( "0B213645-FA4E-44A5-8E4C-B2D8EF054985", "Devices", "The devices available on the network.  This includes Check-in Kiosks, Printers, etc.", "Default", "7E660A4D-72C5-4CF8-B144-16CFC2ADD4D7" );
            AddPage( "7E660A4D-72C5-4CF8-B144-16CFC2ADD4D7", "Device Detail", "", "Default", "EE0CC3F3-50FD-4161-BA5C-A852D4A10E7B" );
            AddBlockType( "Administration - Device Detail", "", "~/Blocks/Administration/DeviceDetail.ascx", "8CD3C212-B9EE-4258-904C-91BA3570EE11" );
            AddBlockType( "Administration - Device List", "", "~/Blocks/Administration/DeviceList.ascx", "32183AD6-01CB-4533-858B-1BDA5120AAD5" );
            AddBlock( "7E660A4D-72C5-4CF8-B144-16CFC2ADD4D7", "32183AD6-01CB-4533-858B-1BDA5120AAD5", "Device List", "", "Content", 0, "9C204AC8-E82E-4900-9B9F-D15D0A571734" );
            AddBlock( "EE0CC3F3-50FD-4161-BA5C-A852D4A10E7B", "8CD3C212-B9EE-4258-904C-91BA3570EE11", "Device Detail", "", "Content", 0, "6BA11D70-7904-4A32-AAEE-A3CA221F24F9" );
            AddBlockTypeAttribute( "32183AD6-01CB-4533-858B-1BDA5120AAD5", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Detail Page Guid", "DetailPageGuid", "", "", 0, "", "7D8B226A-02D7-4929-AC9B-AB8C8C5B6804" );

            // Attrib Value for Device List:Detail Page Guid
            AddBlockAttributeValue( "9C204AC8-E82E-4900-9B9F-D15D0A571734", "7D8B226A-02D7-4929-AC9B-AB8C8C5B6804", "EE0CC3F3-50FD-4161-BA5C-A852D4A10E7B" );

            Sql( @"
UPDATE [dbo].[Page] SET
    MenuDisplayDescription = 0,
    IconCssClass = 'icon-desktop'
WHERE [Guid] = '7E660A4D-72C5-4CF8-B144-16CFC2ADD4D7'

DECLARE @DeviceTypeTypeId int
SET @DeviceTypeTypeId = (SELECT [Id] FROM [DefinedType] WHERE [Name] = 'Device Type')
IF @DeviceTypeTypeId IS NULL
BEGIN
    INSERT INTO [DefinedType] (IsSystem, FieldTypeId, [Order], Category, Name, Description, Guid)
	VALUES (1, 1, 0, 'Organization', 'Device Type', 'The types of devices available', '0368B637-327A-4F5E-80C2-832079E482EE')
    SET @DeviceTypeTypeId = SCOPE_IDENTITY()END
ELSE
BEGIN
	UPDATE [DefinedType] SET 
		IsSystem = 1,
		Category = 'Organization',
		Description = 'The types of devices available',
		Guid = '0368B637-327A-4F5E-80C2-832079E482EE'
	WHERE [Id] = @DeviceTypeTypeId
END

IF NOT EXISTS (SELECT [Id] FROM [DefinedValue] WHERE DefinedTypeId = @DeviceTypeTypeId AND Guid = 'BC809626-1389-4543-B8BB-6FAC79C27AFD')
	INSERT INTO [DefinedValue] ([IsSystem], [DefinedTypeId], [Order], [Name], [Description], [Guid])
		VALUES (1, @DeviceTypeTypeId, 0, 'Check-in Kiosk', 'Device uses as a check-in kiosk', 'BC809626-1389-4543-B8BB-6FAC79C27AFD')

IF NOT EXISTS (SELECT [Id] FROM [DefinedValue] WHERE DefinedTypeId = @DeviceTypeTypeId AND Guid = '64A1DBE5-10AD-42F1-A9BA-646A781D4112')
	INSERT INTO [DefinedValue] ([IsSystem], [DefinedTypeId], [Order], [Name], [Description], [Guid])
		VALUES (1, @DeviceTypeTypeId, 0, 'Giving Kiosk', 'Device uses as a giving kiosk', '64A1DBE5-10AD-42F1-A9BA-646A781D4112')


IF NOT EXISTS (SELECT [Id] FROM [DefinedValue] WHERE DefinedTypeId = @DeviceTypeTypeId AND Guid = '8284B128-E73B-4863-9FC2-43E6827B65E6')
	INSERT INTO [DefinedValue] ([IsSystem], [DefinedTypeId], [Order], [Name], [Description], [Guid])
		VALUES (1, @DeviceTypeTypeId, 0, 'Printer', 'Device uses as a printer', '8284B128-E73B-4863-9FC2-43E6827B65E6')


IF NOT EXISTS (SELECT [Id] FROM [DefinedValue] WHERE DefinedTypeId = @DeviceTypeTypeId AND Guid = '01B585B1-389D-4C86-A9DA-267A8564699D')
	INSERT INTO [DefinedValue] ([IsSystem], [DefinedTypeId], [Order], [Name], [Description], [Guid])
		VALUES (1, @DeviceTypeTypeId, 0, 'Digital Signage', 'Device uses to display digital signage', '01B585B1-389D-4C86-A9DA-267A8564699D')

" );

        }

        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            DeleteAttribute( "7D8B226A-02D7-4929-AC9B-AB8C8C5B6804" ); // Detail Page Guid
            DeleteBlock( "9C204AC8-E82E-4900-9B9F-D15D0A571734" ); // Device List
            DeleteBlock( "6BA11D70-7904-4A32-AAEE-A3CA221F24F9" ); // Device Detail
            DeleteBlockType( "8CD3C212-B9EE-4258-904C-91BA3570EE11" ); // Administration - Device Detail
            DeleteBlockType( "32183AD6-01CB-4533-858B-1BDA5120AAD5" ); // Administration - Device List
            DeletePage( "EE0CC3F3-50FD-4161-BA5C-A852D4A10E7B" ); // Device Detail
            DeletePage( "7E660A4D-72C5-4CF8-B144-16CFC2ADD4D7" ); // Devices
        }

    }
}
