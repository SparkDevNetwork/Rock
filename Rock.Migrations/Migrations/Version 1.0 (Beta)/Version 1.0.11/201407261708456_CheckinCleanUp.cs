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
    public partial class CheckinCleanUp : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            // rename 'Rock ChMS Settings' page for installs that missed a migration due to smash-up
            Sql( @"UPDATE [Page]
                    SET [BrowserTitle] = 'Rock Settings',
	                    [PageTitle] = 'Rock Settings',
	                    [InternalName] = 'Rock Settings'
                    WHERE [Guid] = '550A898C-EDEA-48B5-9C58-B20EC13AF13B'" );

            // add description to current named locations page
            Sql( @"UPDATE [Page]
                    SET 
	                    [Description] = 'Allows for the administration of specific locations.'
                    WHERE [Guid] = '2BECFB85-D566-464F-B6AC-0BE90189A418'" );

            // add description to current device detail page
            Sql( @"UPDATE [Page]
                    SET 
	                    [Description] = 'Edits the details of a device.'
                    WHERE [Guid] = 'EE0CC3F3-50FD-4161-BA5C-A852D4A10E7B'" );

            // change checkin manager route to 'CheckinManager'
            Sql( @"  UPDATE [PageRoute]
                      SET [Route] = 'CheckinManager'
                      WHERE [Route] = 'ManageCheckin'" );

            // delete de-centralized kiosk devices
            Sql( @"
    DECLARE @DeviceId int = ( SELECT TOP 1 [Id] FROM [Device] WHERE [Guid] = '61111232-01D7-427D-9C1F-D45CF4D3F7CB' )
    UPDATE A 
        SET [DeviceId] = @DeviceId
    FROM [Attendance] A
        INNER JOIN [Device] D ON D.[Id] = A.[DeviceId]
    WHERE D.[Guid] IN (
        '1155287E-B79B-4464-9EA7-70DE1E43278C', 
        '1043CF18-4944-4372-BD4D-6D44FD10D9A8', 
        'E6138007-302F-42C1-AE98-7C992205ED13',
        'BE051412-AA0A-4D95-B78E-2A24FBB78560',
        '051302BE-8E11-456F-AE1D-A5364DE9A895',
        '82C9DE79-E86A-4A16-A5AD-E5FC3FDF0AFD',
        'A0F276F3-F6A3-452D-A870-FB48ED3113B2',
        'FDD8AC58-A82F-408A-85C7-8011BFB99021'
    )

    DELETE FROM [Device]
    WHERE [Guid] IN (
        '1155287E-B79B-4464-9EA7-70DE1E43278C', 
        '1043CF18-4944-4372-BD4D-6D44FD10D9A8', 
        'E6138007-302F-42C1-AE98-7C992205ED13',
        'BE051412-AA0A-4D95-B78E-2A24FBB78560',
        '051302BE-8E11-456F-AE1D-A5364DE9A895',
        '82C9DE79-E86A-4A16-A5AD-E5FC3FDF0AFD',
        'A0F276F3-F6A3-452D-A870-FB48ED3113B2',
        'FDD8AC58-A82F-408A-85C7-8011BFB99021'
    )
" );

            // rename 'Test Label Printer' to 'Check-in Label Printer'
            Sql( @"UPDATE [Device]
            SET [Name] = 'Check-in Label Printer' 
            WHERE [Guid] = '36EA94D3-9D2A-4BAD-AE15-116D0727CD9C'" );

            // create new 'Admin Tools' child page
            RockMigrationHelper.AddPage( "550A898C-EDEA-48B5-9C58-B20EC13AF13B", "D65F783D-87A9-4CC9-8110-E83466A0EADB", "Check-in Settings", "Naviagtion page for check-in configuration pages.", "66C5DD58-094C-4FF9-9AFB-44801FCFCC2D", "fa fa-check" );

            RockMigrationHelper.AddBlock( "66C5DD58-094C-4FF9-9AFB-44801FCFCC2D", "", "CACB9D1A-A820-4587-986A-D66A69EE9948", "Page List as Blocks", "Main", "", "", 0, "67768707-9371-4638-BA0F-9144574FC25A" );
            RockMigrationHelper.AddBlockAttributeValue( "67768707-9371-4638-BA0F-9144574FC25A", "1322186A-862A-4CF1-B349-28ECB67229BA", "{% include 'PageListAsBlocks' %}" );

            // move check-in configuration page 
            Sql( @"UPDATE [Page]
                    SET 
	                    [ParentPageId] = (SELECT [Id] FROM [Page] WHERE [Guid] = '66C5DD58-094C-4FF9-9AFB-44801FCFCC2D')
	                    , [Order] = 1
                    WHERE [Guid] = 'C646A95A-D12D-4A67-9BE6-C9695C0267ED'" );

            // create duplicate pages under check-in settings
            // named locations
            RockMigrationHelper.AddPage( "66C5DD58-094C-4FF9-9AFB-44801FCFCC2D", "0CB60906-6B74-44FD-AB25-026050EF70EB", "Named Locations", "Allows for the administration of specific locations.", "96501070-BB46-4432-AA3C-A8C496691629", "fa fa-map-marker" );
            RockMigrationHelper.AddBlock( "96501070-BB46-4432-AA3C-A8C496691629", "", "468B99CE-D276-4D30-84A9-7842933BDBCD", "Location Tree View", "Sidebar1", "", "", 1, "31AD774C-B8FE-49CF-BF53-40BD46C2DCC1" );
            RockMigrationHelper.AddBlockAttributeValue( "31AD774C-B8FE-49CF-BF53-40BD46C2DCC1", "BA252362-F8D3-4D2D-91B5-7E90BDB154DA", "96501070-bb46-4432-aa3c-a8c496691629" ); 
            RockMigrationHelper.AddBlockAttributeValue( "31AD774C-B8FE-49CF-BF53-40BD46C2DCC1", "EC259AD8-CE80-401D-948B-66322ED5B5D0", "" ); 
            RockMigrationHelper.AddBlock( "96501070-BB46-4432-AA3C-A8C496691629", "", "08189564-1245-48F8-86CC-560F4DD48733", "Location Detail", "Main", "", "", 1, "71334750-1F04-41DE-BEC4-8350BBC7D844" );
            RockMigrationHelper.AddBlockAttributeValue( "71334750-1F04-41DE-BEC4-8350BBC7D844", "A4BE7F16-A8E0-4F2F-AFFD-9323AEE51F1D", @"
    {% if point or polygon %}
        <div class='group-location-map'>
            <img src='http://maps.googleapis.com/maps/api/staticmap?sensor=false&size=350x200&format=png&style=feature:all|saturation:0|hue:0xe7ecf0&style=feature:road|saturation:-70&style=feature:transit|visibility:off&style=feature:poi|visibility:off&style=feature:water|visibility:simplified|saturation:-60{% if point %}&markers=color:0x779cb1|{{ point.latitude }},{{ point.longitude }}{% endif %}{% if polygon %}&path=fillcolor:0x779cb155|color:0xFFFFFF00|enc:{{ polygon.google_encoded_polygon }}{% endif %}&visual_refresh=true'/>
        </div>
    {% endif %}
" );
            RockMigrationHelper.AddBlockAttributeValue( "71334750-1F04-41DE-BEC4-8350BBC7D844", "3475B7C5-3B57-4617-A1CA-CBACF20E7463", "fdc5d6ba-a818-4a06-96b1-9ef31b4087ac" ); 

            // schedules
            RockMigrationHelper.AddPage( "66C5DD58-094C-4FF9-9AFB-44801FCFCC2D", "0CB60906-6B74-44FD-AB25-026050EF70EB", "Schedules", "Configure schedules used throughout the system.", "AFFFB245-A0EB-4002-B736-A2D52DD692CF", "fa fa-calendar" );
            RockMigrationHelper.AddBlock( "AFFFB245-A0EB-4002-B736-A2D52DD692CF", "", "ADE003C7-649B-466A-872B-B8AC952E7841", "Schedules Treeview", "Sidebar1", "", "", 1, "0AEF4F84-4365-484A-8B0F-EAA20E992EC4" );
            RockMigrationHelper.AddBlockAttributeValue( "0AEF4F84-4365-484A-8B0F-EAA20E992EC4", "AA057D3E-00CC-42BD-9998-600873356EDB", "ScheduleId" ); // page parameter key
            RockMigrationHelper.AddBlockAttributeValue( "0AEF4F84-4365-484A-8B0F-EAA20E992EC4", "06D414F0-AA20-4D3C-B297-1530CCD64395", "0B2C38A7-D79C-4F85-9757-F1B045D32C8A" ); // entity type
            RockMigrationHelper.AddBlockAttributeValue( "0AEF4F84-4365-484A-8B0F-EAA20E992EC4", "AEE521D8-124D-4BB3-8A80-5F368E5CEC15", "AFFFB245-A0EB-4002-B736-A2D52DD692CF" ); // detail page
            RockMigrationHelper.AddBlock( "AFFFB245-A0EB-4002-B736-A2D52DD692CF", "", "7BC54887-21C2-4688-BD1D-C1C8B9C86F7C", "Category Detail", "Main", "", "", 1, "83DBFFDF-E92F-420D-8269-E68B9EB5BEDC" );
            RockMigrationHelper.AddBlockAttributeValue( "83DBFFDF-E92F-420D-8269-E68B9EB5BEDC", "FF3A33CF-8897-4FC6-9C16-64FA25E6C297", "0B2C38A7-D79C-4F85-9757-F1B045D32C8A" ); // entity type
            RockMigrationHelper.AddBlock( "AFFFB245-A0EB-4002-B736-A2D52DD692CF", "", "59C9C862-570C-4410-99B6-DD9064B5E594", "Schedule Detail", "Main", "", "", 2, "765DE272-6F00-47C3-B445-303A7CF80486" );
                       
            // device list
            RockMigrationHelper.AddPage( "66C5DD58-094C-4FF9-9AFB-44801FCFCC2D", "D65F783D-87A9-4CC9-8110-E83466A0EADB", "Devices", "Setup devices available on the network (check-in kiosks, printers, etc.)", "5A06C807-251C-4155-BBE7-AAC73D0745E3", "fa fa-desktop" );
            RockMigrationHelper.AddBlock( "5A06C807-251C-4155-BBE7-AAC73D0745E3", "", "32183AD6-01CB-4533-858B-1BDA5120AAD5", "Device List", "Main", "", "", 1, "8244812E-2513-4862-BEEC-E982C4FA5665" );

            // device detail (can't reuse current one or breadcrumbs will be wrong)
            RockMigrationHelper.AddPage( "5A06C807-251C-4155-BBE7-AAC73D0745E3", "D65F783D-87A9-4CC9-8110-E83466A0EADB", "Device Detail", "Edits the details of a device.", "7D5311B3-F526-4E22-8153-EA1799467886", "fa fa-desktop" );
            RockMigrationHelper.AddBlock( "7D5311B3-F526-4E22-8153-EA1799467886", "", "8CD3C212-B9EE-4258-904C-91BA3570EE11", "Device Detail", "Main", "", "", 1, "888666A4-A0F4-4CC4-8AAF-2CF0DC29DA5E" );

            // set detail page property now that it exists
            RockMigrationHelper.AddBlockAttributeValue( "8244812E-2513-4862-BEEC-E982C4FA5665", "7D8B226A-02D7-4929-AC9B-AB8C8C5B6804", "7D5311B3-F526-4E22-8153-EA1799467886" );

            // move check-in label page
            Sql( @"UPDATE [Page]
                    SET 
	                    [ParentPageId] = (SELECT [Id] FROM [Page] WHERE [Guid] = '66C5DD58-094C-4FF9-9AFB-44801FCFCC2D')
	                    , [Order] = 5
                    WHERE [Guid] = '7C093A63-F2AC-4FE3-A826-8BF06D204EA2'" );

            // reorder config pages
            Sql( @"
                    UPDATE [Page] 
	                    SET [Order] = 5
	                    WHERE [Guid] = '66C5DD58-094C-4FF9-9AFB-44801FCFCC2D'

                    UPDATE [Page] 
	                    SET [Order] = 6
	                    WHERE [Guid] = '7F1F4130-CB98-473B-9DE1-7A886D2283ED'

                    UPDATE [Page] 
	                    SET [Order] = 7
	                    WHERE [Guid] = 'C831428A-6ACD-4D49-9B2D-046D399E3123'
            " );
        
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            // move pages back
            Sql( @"UPDATE [Page]
                    SET 
	                    [ParentPageId] = (SELECT [Id] FROM [Page] WHERE [Guid] = '0B213645-FA4E-44A5-8E4C-B2D8EF054985')
	                    , [Order] = 12
                    WHERE [Guid] = 'C646A95A-D12D-4A67-9BE6-C9695C0267ED'" );

            Sql( @"UPDATE [Page]
                    SET 
	                    [ParentPageId] = (SELECT [Id] FROM [Page] WHERE [Guid] = '0B213645-FA4E-44A5-8E4C-B2D8EF054985')
	                    , [Order] = 13
                    WHERE [Guid] = '7C093A63-F2AC-4FE3-A826-8BF06D204EA2'" );

            // delete named locations page
            RockMigrationHelper.DeleteBlock( "31AD774C-B8FE-49CF-BF53-40BD46C2DCC1" );
            RockMigrationHelper.DeleteBlock( "71334750-1F04-41DE-BEC4-8350BBC7D844" );
            RockMigrationHelper.DeletePage( "96501070-BB46-4432-AA3C-A8C496691629" );

            // delete schedules page
            RockMigrationHelper.DeleteBlock( "0AEF4F84-4365-484A-8B0F-EAA20E992EC4" );
            RockMigrationHelper.DeleteBlock( "83DBFFDF-E92F-420D-8269-E68B9EB5BEDC" );
            RockMigrationHelper.DeleteBlock( "765DE272-6F00-47C3-B445-303A7CF80486" );
            RockMigrationHelper.DeletePage( "AFFFB245-A0EB-4002-B736-A2D52DD692CF" );
            
            // delete device details
            RockMigrationHelper.DeleteBlock( "888666A4-A0F4-4CC4-8AAF-2CF0DC29DA5E" );
            RockMigrationHelper.DeletePage( "7D5311B3-F526-4E22-8153-EA1799467886" );

            // delete device list
            RockMigrationHelper.DeleteBlock( "8244812E-2513-4862-BEEC-E982C4FA5665" );
            RockMigrationHelper.DeletePage( "5A06C807-251C-4155-BBE7-AAC73D0745E3" );
            
            // delete new Check-in Settings page
            RockMigrationHelper.DeleteBlock( "67768707-9371-4638-BA0F-9144574FC25A" );
            RockMigrationHelper.DeletePage( "66C5DD58-094C-4FF9-9AFB-44801FCFCC2D" );
        }
    }
}
