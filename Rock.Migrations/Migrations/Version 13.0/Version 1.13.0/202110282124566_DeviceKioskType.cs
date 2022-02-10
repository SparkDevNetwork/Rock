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
    public partial class DeviceKioskType : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            AddColumn("dbo.Device", "KioskType", c => c.Int());

            WindowsCheckinClientDownloadLinkUp();
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            DropColumn("dbo.Device", "KioskType");

            WindowsCheckinClientDownloadLinkDown();
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
                SET [Value] = 'https://storage.rockrms.com/externalapplications/sparkdevnetwork/windowscheckin/1.13.0.26/checkinclient.msi'
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
                SET [Value] = 'https://storage.rockrms.com/externalapplications/sparkdevnetwork/windowscheckin/1.13.0/checkinclient.msi'
                WHERE AttributeId = @downloadUrlAttributeId and EntityId = @winCheckinClientDefinedValueId" );
        }
    }
}
