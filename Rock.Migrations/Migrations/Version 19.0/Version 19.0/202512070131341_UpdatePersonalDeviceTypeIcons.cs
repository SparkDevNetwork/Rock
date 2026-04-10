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
    /// <summary>
    ///
    /// </summary>
    public partial class UpdatePersonalDeviceTypeIcons : Rock.Migrations.RockMigration
    {
        private const string IconCssClassAttributeGuid = "DC0E00D2-7694-410E-82C0-E99A097D0A30";
        private const string PersonalDeviceTypeTvGuid = Rock.SystemGuid.DefinedValue.PERSONAL_DEVICE_TYPE_TV;
        private const string PersonalDeviceTypeMobileGuid = Rock.SystemGuid.DefinedValue.PERSONAL_DEVICE_TYPE_MOBILE;
        private const string PersonalDeviceTypeComputerGuid = Rock.SystemGuid.DefinedValue.PERSONAL_DEVICE_TYPE_COMPUTER;

        private const string IconCssClassTvValue = "ti ti-device-tv";
        private const string IconCssClassMobileValue = "ti ti-device-mobile";
        private const string IconCssClassDesktopValue = "ti ti-device-desktop";

        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            CreateIndex( "dbo.PersonalDevice", "PlatformValueId" );
            AddForeignKey( "dbo.PersonalDevice", "PlatformValueId", "dbo.DefinedValue", "Id" );

            AddTVIconClass();
            UpdateMobileIconClass();
            UpdateComputerIconClass();
        }

        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            DropForeignKey( "dbo.PersonalDevice", "PlatformValueId", "dbo.DefinedValue" );
            DropIndex( "dbo.PersonalDevice", new[] { "PlatformValueId" } );
        }

        #region MSE: Up Methods for Data Migration

        private void AddTVIconClass()
        {
            Sql( $@"
DECLARE @AttributeId INT = (SELECT [Id] FROM [Attribute] WHERE [Guid] = '{IconCssClassAttributeGuid}');
DECLARE @EntityId INT = (SELECT [Id] FROM [DefinedValue] WHERE [Guid] = '{PersonalDeviceTypeTvGuid}');

IF @AttributeId IS NOT NULL AND @EntityId IS NOT NULL
BEGIN
    IF NOT EXISTS (
        SELECT 1
        FROM [AttributeValue]
        WHERE [AttributeId] = @AttributeId AND [EntityId] = @EntityId
    )
    BEGIN
        INSERT INTO [AttributeValue] ( [IsSystem], [AttributeId], [EntityId], [Value], [Guid] )
        VALUES ( 1, @AttributeId, @EntityId, '{IconCssClassTvValue}', NEWID() );
    END
END
" );
        }

        private void UpdateMobileIconClass()
        {
            Sql( $@"
DECLARE @AttributeId INT = (SELECT [Id] FROM [Attribute] WHERE [Guid] = '{IconCssClassAttributeGuid}');
DECLARE @EntityId INT = (SELECT [Id] FROM [DefinedValue] WHERE [Guid] = '{PersonalDeviceTypeMobileGuid}');

IF @AttributeId IS NOT NULL AND @EntityId IS NOT NULL
BEGIN
    IF EXISTS (
        SELECT 1
        FROM [AttributeValue]
        WHERE [AttributeId] = @AttributeId AND [EntityId] = @EntityId
    )
    BEGIN
        UPDATE [AttributeValue]
        SET [Value] = '{IconCssClassMobileValue}'
        WHERE [AttributeId] = @AttributeId AND [EntityId] = @EntityId;
    END
    ELSE
    BEGIN
        INSERT INTO [AttributeValue] ( [IsSystem], [AttributeId], [EntityId], [Value], [Guid] )
        VALUES ( 1, @AttributeId, @EntityId, '{IconCssClassMobileValue}', NEWID() );
    END
END
" );
        }

        private void UpdateComputerIconClass()
        {
            Sql( $@"
DECLARE @AttributeId INT = (SELECT [Id] FROM [Attribute] WHERE [Guid] = '{IconCssClassAttributeGuid}');
DECLARE @EntityId INT = (SELECT [Id] FROM [DefinedValue] WHERE [Guid] = '{PersonalDeviceTypeComputerGuid}');

IF @AttributeId IS NOT NULL AND @EntityId IS NOT NULL
BEGIN
    IF EXISTS (
        SELECT 1
        FROM [AttributeValue]
        WHERE [AttributeId] = @AttributeId AND [EntityId] = @EntityId
    )
    BEGIN
        UPDATE [AttributeValue]
        SET [Value] = '{IconCssClassDesktopValue}'
        WHERE [AttributeId] = @AttributeId AND [EntityId] = @EntityId;
    END
    ELSE
    BEGIN
        INSERT INTO [AttributeValue] ( [IsSystem], [AttributeId], [EntityId], [Value], [Guid] )
        VALUES ( 1, @AttributeId, @EntityId, '{IconCssClassDesktopValue}', NEWID() );
    END
END
" );
        }

        #endregion
    }
}