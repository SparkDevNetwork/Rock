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
    public partial class AdminPageUpdates : Rock.Migrations.RockMigration3
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            Sql(@"

DECLARE @GeneralSettingsPage int =  (SELECT Id FROM [Page] WHERE Guid = '0B213645-FA4E-44A5-8E4C-B2D8EF054985')
DECLARE @SecurityPage int =  (SELECT Id FROM [Page] WHERE Guid = '91CCB1C9-5F9F-44F5-8BE2-9EC3A3CFD46F')
DECLARE @CommunicationsPage int =  (SELECT Id FROM [Page] WHERE Guid = '199DC522-F4D6-4D82-AF44-3C16EE9D2CDA')
DECLARE @PowerToolsPage int =  (SELECT Id FROM [Page] WHERE Guid = '7F1F4130-CB98-473B-9DE1-7A886D2283ED')

-- update person settings to security
UPDATE [Page]
	SET   [InternalName] = 'Security'
		, [PageTitle] = 'Security'
		, [BrowserTitle] = 'Security'
		, [IconCssClass] = 'fa fa-lock'
	WHERE [Guid] = '91CCB1C9-5F9F-44F5-8BE2-9EC3A3CFD46F'

-- move update to general settings
UPDATE [Page]
	SET   [ParentPageId] = @GeneralSettingsPage
		, [Order] = 1
	WHERE [Guid] = 'A3990266-CB0D-4FB5-882C-3852ED5D96AB'

-- move pages from person settings
UPDATE [Page]
	SET   [ParentPageId] = @GeneralSettingsPage
		, [Order] = 17
	WHERE [Guid] = '7BA1FAF4-B63C-4423-A818-CC794DDB14E3'

UPDATE [Page]
	SET   [ParentPageId] = @GeneralSettingsPage
		, [Order] = 18
	WHERE [Guid] = '26547B83-A92D-4D7E-82ED-691F403F16B6'

-- remove control gallery
DELETE FROM [Page] WHERE [Guid] = 'F8B18AA5-A85C-42D4-87D4-C8F7E2FB0159'

-- move pages to security
UPDATE [Page]
	SET   [ParentPageId] = @SecurityPage
		, [Order] = 1
	WHERE [Guid] = '306BFEF8-596C-482A-8DEC-34A7B622E688'

UPDATE [Page]
	SET   [ParentPageId] = @SecurityPage
		, [Order] = 2
	WHERE [Guid] = 'D9678FEF-C086-4232-972C-5DBAC14BFEE6'

UPDATE [Page]
	SET   [ParentPageId] = @SecurityPage
		, [Order] = 3
	WHERE [Guid] = '0D51F443-1C0D-4C71-8BAE-E5F5A35E8B79'

UPDATE [Page]
	SET   [ParentPageId] = @SecurityPage
		, [Order] = 4
	WHERE [Guid] = '4D7F3953-0BD9-4B4B-83F9-5FCC6B2BBE30'

UPDATE [Page]
	SET   [ParentPageId] = @SecurityPage
		, [Order] = 5
	WHERE [Guid] = 'F7F41856-F7EA-49A8-9D9B-917AC1964602'

UPDATE [Page]
	SET   [ParentPageId] = @SecurityPage
		, [Order] = 6
	WHERE [Guid] = '23507C90-3F78-40D4-B847-6FE8941FCD32'

-- move ad types
UPDATE [Page]
	SET   [ParentPageId] = @CommunicationsPage
		, [Order] = 4
	WHERE [Guid] = 'E6F5F06B-65EE-4949-AA56-1FE4E2933C63'

-- move external apps
UPDATE [Page]
	SET   [ParentPageId] = @PowerToolsPage
		, [Order] = 2
	WHERE [Guid] = '5A676DCC-37F0-4624-8CCD-408A5A471D8A'


-- reorder general settings
UPDATE [Page]
	SET  [Order] = 2
	WHERE [Guid] = 'A2753E03-96B1-4C83-AA11-FCD68C631571'

UPDATE [Page]
	SET  [Order] = 3
	WHERE [Guid] = 'E0E1DE66-B825-4BFB-A0B3-6E069AA9AA40'

UPDATE [Page]
	SET  [Order] = 4
	WHERE [Guid] = '40899BCD-82B0-47F2-8F2A-B6AA3877B445'

UPDATE [Page]
	SET  [Order] = 5
	WHERE [Guid] = '5EE91A54-C750-48DC-9392-F1F0F0581C3A'

UPDATE [Page]
	SET  [Order] = 6
	WHERE [Guid] = 'F111791B-6A58-4388-8533-00E913F48F41'

UPDATE [Page]
	SET  [Order] = 7
	WHERE [Guid] = 'DCB18A76-6DFF-48A5-A66E-2CAA10D2CA1A'

UPDATE [Page]
	SET  [Order] = 8
	WHERE [Guid] = '1A233978-5BF4-4A09-9B86-6CC4C081F48B'

UPDATE [Page]
	SET  [Order] = 9
	WHERE [Guid] = '66031C31-B397-4F78-8AB2-389B7D8731AA'

UPDATE [Page]
	SET  [Order] = 10
	WHERE [Guid] = '7E660A4D-72C5-4CF8-B144-16CFC2ADD4D7'

UPDATE [Page]
	SET  [Order] = 11
	WHERE [Guid] = 'C646A95A-D12D-4A67-9BE6-C9695C0267ED'

UPDATE [Page]
	SET  [Order] = 12
	WHERE [Guid] = '7C093A63-F2AC-4FE3-A826-8BF06D204EA2'

UPDATE [Page]
	SET  [Order] = 13
	WHERE [Guid] = 'F5D6D7DD-FD5F-494C-83DC-E2AF63C705D1'

UPDATE [Page]
	SET  [Order] = 14
	WHERE [Guid] = '220D72F5-B589-4378-9852-BBB6F145AD7F'

UPDATE [Page]
	SET  [Order] = 15
	WHERE [Guid] = '84DB9BA0-2725-40A5-A3CA-9A1C043C31B0'

UPDATE [Page]
	SET  [Order] = 16
	WHERE [Guid] = 'FA2A1171-9308-41C7-948C-C9EBEA5BD668'

");
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
        }
    }
}
