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
    [MigrationNumber( 128, "1.10.3" )]
    public class MigrationRollupsFor10_4_1 : Migration
    {

        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            //UpdateCheckScannerUrlUp();
            //UpdateGroupAttendanceDigestUp();
            //AlterFunctionUfnCrmGetAddressUp();
        }

        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            // Down migrations are not yet supported in plug-in migrations.
        }

        /// <summary>
        /// JH: Update Check Scanner URL.
        /// </summary>
        private void UpdateCheckScannerUrlUp()
        {
            Sql( @"UPDATE [AttributeValue]
                SET [Value] = 'http://storage.rockrms.com/externalapplications/sparkdevnetwork/checkscanner/1.10.4/checkscanner.msi'
                WHERE ([Guid] = '82960DBD-2EAA-47DF-B9AC-86F7A2FCA180');" );
        }

        /// <summary>
        /// JH: Update GroupAttendanceDigest SystemCommunication.
        /// </summary>
        private void UpdateGroupAttendanceDigestUp()
        {
            Sql( @"-- Update the table header cells.
                UPDATE [SystemCommunication]
                SET [Body] = REPLACE([Body], '<th style=""background-color: #a6a5a5; color: #ffffff; padding: 4px 8px; border: 1px solid #c4c4c4; text-align: left;"">', '<th nowrap style=""background-color: #a6a5a5; color: #ffffff; padding: 4px 8px; border: 1px solid #c4c4c4; text-align: left;"">')
                WHERE ([Guid] = '345CD403-11D2-4B74-A467-ADD15572DD4F');

                -- Update the table data cells.
                UPDATE [SystemCommunication]
                SET [Body] = REPLACE([Body], '<td style=""border: 1px solid #c4c4c4; padding: 6px;"">', '<td style=""border: 1px solid #c4c4c4; padding: 6px; vertical-align: top; word-break: normal;"">')
                WHERE ([Guid] = '345CD403-11D2-4B74-A467-ADD15572DD4F');" );
        }

        /// <summary>
        /// JH: Alter Ufn_CrmGetAddress Function to Prevent SQL 2019 Exception.
        /// </summary>
        private void AlterFunctionUfnCrmGetAddressUp()
        {
            Sql( @"/*
<doc>
    <summary>
         This function returns the address of the person provided.
    </summary>

    <returns>
        Address of the person.
    </returns>
    <remarks>
        This function allows you to request an address for a specific person. It will return
        the first address of that type (multiple address are possible if the individual is in
        multiple families). 
        
        You can provide the address type by specifing 'Home', 'Previous', 
        'Work'. For custom address types provide the AddressTypeId like '19'.

        You can also determine which component of the address you'd like. Values include:
            + 'Full' - the full address 
            + 'Street1'
            + 'Street2'
            + 'City'
            + 'State'
            + 'PostalCode'
            + 'Country'
            + 'Latitude'
            + 'Longitude'

    </remarks>
    <code>
        SELECT [dbo].[ufnCrm_GetAddress](3, 'Home', 'Full')
        SELECT [dbo].[ufnCrm_GetAddress](3, 'Home', 'Street1')
        SELECT [dbo].[ufnCrm_GetAddress](3, 'Home', 'Street2')
        SELECT [dbo].[ufnCrm_GetAddress](3, 'Home', 'City')
        SELECT [dbo].[ufnCrm_GetAddress](3, 'Home', 'State')
        SELECT [dbo].[ufnCrm_GetAddress](3, 'Home', 'PostalCode')
        SELECT [dbo].[ufnCrm_GetAddress](3, 'Home', 'Country')
        SELECT [dbo].[ufnCrm_GetAddress](3, 'Home', 'Latitude')
        SELECT [dbo].[ufnCrm_GetAddress](3, 'Home', 'Longitude')
    </code>
</doc>
*/

ALTER FUNCTION [dbo].[ufnCrm_GetAddress](
    @PersonId int,
    @AddressType varchar(20),
    @AddressComponent varchar(20)) 

RETURNS nvarchar(500) AS

BEGIN
    DECLARE @AddressTypeId int,
        @Address nvarchar(500)

    -- get address type
    IF (@AddressType = 'Home')
        BEGIN
        SET @AddressTypeId = 19
        END
    ELSE IF (@AddressType = 'Work')
        BEGIN
        SET @AddressTypeId = 20
        END
    ELSE IF (@AddressType = 'Previous')
        BEGIN
        SET @AddressTypeId = 137
        END
    ELSE
        BEGIN
        SET @AddressTypeId = CAST(@AddressType AS int)
        END

    -- return address component
    IF (@AddressComponent = 'Street1')
        BEGIN
        SET @Address = (SELECT [Street1] FROM [Location] WHERE [Id] = (SELECT TOP 1 [LocationId] FROM [GroupLocation] WHERE  [GroupLocationTypeValueId] = @AddressTypeId AND  [GroupId] = (SELECT TOP 1 [GroupId] FROM [GroupMember] gm INNER JOIN [Group] g ON g.[Id] = gm.[GroupId] WHERE [PersonId] = @PersonId AND g.[GroupTypeId] = 10 ORDER BY isnull(gm.GroupOrder, 99999) ))) 
        END
    ELSE IF (@AddressComponent = 'Street2')
        BEGIN
        SET @Address = (SELECT [Street2] FROM [Location] WHERE [Id] = (SELECT TOP 1 [LocationId] FROM [GroupLocation] WHERE  [GroupLocationTypeValueId] = @AddressTypeId AND  [GroupId] = (SELECT TOP 1 [GroupId] FROM [GroupMember] gm INNER JOIN [Group] g ON g.[Id] = gm.[GroupId] WHERE [PersonId] = @PersonId AND g.[GroupTypeId] = 10 ORDER BY isnull(gm.GroupOrder, 99999) ))) 
        END
    ELSE IF (@AddressComponent = 'City')
        BEGIN
        SET @Address = (SELECT [City] FROM [Location] WHERE [Id] = (SELECT TOP 1 [LocationId] FROM [GroupLocation] WHERE  [GroupLocationTypeValueId] = @AddressTypeId AND  [GroupId] = (SELECT TOP 1 [GroupId] FROM [GroupMember] gm INNER JOIN [Group] g ON g.[Id] = gm.[GroupId] WHERE [PersonId] = @PersonId AND g.[GroupTypeId] = 10 ORDER BY isnull(gm.GroupOrder, 99999) ))) 
        END
    ELSE IF (@AddressComponent = 'State')
        BEGIN
        SET @Address = (SELECT [State] FROM [Location] WHERE [Id] = (SELECT TOP 1 [LocationId] FROM [GroupLocation] WHERE  [GroupLocationTypeValueId] = @AddressTypeId AND  [GroupId] = (SELECT TOP 1 [GroupId] FROM [GroupMember] gm INNER JOIN [Group] g ON g.[Id] = gm.[GroupId] WHERE [PersonId] = @PersonId AND g.[GroupTypeId] = 10 ORDER BY isnull(gm.GroupOrder, 99999) ))) 
        END
    ELSE IF (@AddressComponent = 'PostalCode')
        BEGIN
        SET @Address = (SELECT [PostalCode] FROM [Location] WHERE [Id] = (SELECT TOP 1 [LocationId] FROM [GroupLocation] WHERE  [GroupLocationTypeValueId] = @AddressTypeId AND  [GroupId] = (SELECT TOP 1 [GroupId] FROM [GroupMember] gm INNER JOIN [Group] g ON g.[Id] = gm.[GroupId] WHERE [PersonId] = @PersonId AND g.[GroupTypeId] = 10 ORDER BY isnull(gm.GroupOrder, 99999) ))) 
        END
    ELSE IF (@AddressComponent = 'Country')
        BEGIN
        SET @Address = (SELECT [Country] FROM [Location] WHERE [Id] = (SELECT TOP 1 [LocationId] FROM [GroupLocation] WHERE  [GroupLocationTypeValueId] = @AddressTypeId AND  [GroupId] = (SELECT TOP 1 [GroupId] FROM [GroupMember] gm INNER JOIN [Group] g ON g.[Id] = gm.[GroupId] WHERE [PersonId] = @PersonId AND g.[GroupTypeId] = 10 ORDER BY isnull(gm.GroupOrder, 99999) ))) 
        END
    ELSE IF (@AddressComponent = 'Latitude')
        BEGIN
        SET @Address = (SELECT [GeoPoint].[Lat] FROM [Location] WHERE [Id] = (SELECT TOP 1 [LocationId] FROM [GroupLocation] WHERE  [GroupLocationTypeValueId] = @AddressTypeId AND  [GroupId] = (SELECT TOP 1 [GroupId] FROM [GroupMember] gm INNER JOIN [Group] g ON g.[Id] = gm.[GroupId] WHERE [PersonId] = @PersonId AND g.[GroupTypeId] = 10 ORDER BY isnull(gm.GroupOrder, 99999) ))) 
        END
    ELSE IF (@AddressComponent = 'Longitude')
        BEGIN
        SET @Address = (SELECT [GeoPoint].[Long] FROM [Location] WHERE [Id] = (SELECT TOP 1 [LocationId] FROM [GroupLocation] WHERE  [GroupLocationTypeValueId] = @AddressTypeId AND  [GroupId] = (SELECT TOP 1 [GroupId] FROM [GroupMember] gm INNER JOIN [Group] g ON g.[Id] = gm.[GroupId] WHERE [PersonId] = @PersonId AND g.[GroupTypeId] = 10 ORDER BY isnull(gm.GroupOrder, 99999) ))) 
        END
    ELSE 
        BEGIN
        SET @Address = (SELECT ISNULL([Street1], '') + ' ' + ISNULL([Street2], '') + ' ' + ISNULL([City], '') + ', ' + ISNULL([State], '') + ' ' + ISNULL([PostalCode], '') FROM [Location] WHERE [Id] = (SELECT TOP 1 [LocationId] FROM [GroupLocation] WHERE  [GroupLocationTypeValueId] = @AddressTypeId AND  [GroupId] = (SELECT TOP 1 [GroupId] FROM [GroupMember] gm INNER JOIN [Group] g ON g.[Id] = gm.[GroupId] WHERE [PersonId] = @PersonId AND g.[GroupTypeId] = 10 ORDER BY isnull(gm.GroupOrder, 99999) ))) 
        END

    RETURN @Address
END" );
        }


    }
}
