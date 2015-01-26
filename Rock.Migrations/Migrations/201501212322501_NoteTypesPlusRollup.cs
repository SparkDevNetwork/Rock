// <copyright>
// Copyright 2013 by the Spark Development Network
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.apache.org/licenses/LICENSE-2.0
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
    public partial class NoteTypesPlusRollup : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            
            //
            // Note Types
            //
            
            // approval
            RockMigrationHelper.AddDefinedValue( "FDC7A191-717E-4CA6-9DCF-A2B5BB09C782", "Approve", "Used for notes showing approval.", "EBB52236-B29A-4E00-B176-15B351679F99" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "EBB52236-B29A-4E00-B176-15B351679F99", "629CFBFF-3A95-4294-B13C-37F4FED04FE7", "fa fa-check" );

            // deny
            RockMigrationHelper.AddDefinedValue( "FDC7A191-717E-4CA6-9DCF-A2B5BB09C782", "Deny", "Used for notes showing denials.", "68221BAB-13E6-43BB-9CEA-81507C5E4BAF" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "68221BAB-13E6-43BB-9CEA-81507C5E4BAF", "629CFBFF-3A95-4294-B13C-37F4FED04FE7", "fa fa-ban" );

            // success
            RockMigrationHelper.AddDefinedValue( "FDC7A191-717E-4CA6-9DCF-A2B5BB09C782", "Success", "Used for styling notes using the Bootstrap success coloring.", "128C609C-1601-4247-8BA2-6CB60AB2562C" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "128C609C-1601-4247-8BA2-6CB60AB2562C", "629CFBFF-3A95-4294-B13C-37F4FED04FE7", "fa fa-star" );

            // info
            RockMigrationHelper.AddDefinedValue( "FDC7A191-717E-4CA6-9DCF-A2B5BB09C782", "Info", "Used for styling notes using the Bootstrap info coloring.", "A52CEA75-CD49-43F7-82FF-D781835EB73F" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "A52CEA75-CD49-43F7-82FF-D781835EB73F", "629CFBFF-3A95-4294-B13C-37F4FED04FE7", "fa fa-flag" );

            // warning
            RockMigrationHelper.AddDefinedValue( "FDC7A191-717E-4CA6-9DCF-A2B5BB09C782", "Warning", "Used for styling notes using the Bootstrap warning coloring.", "EF911C5F-A922-4B21-B601-FD306538B0E2" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "EF911C5F-A922-4B21-B601-FD306538B0E2", "629CFBFF-3A95-4294-B13C-37F4FED04FE7", "fa fa-exclamation-triangle" );

            // danger
            RockMigrationHelper.AddDefinedValue( "FDC7A191-717E-4CA6-9DCF-A2B5BB09C782", "Danger", "Used for styling notes using the Bootstrap danger coloring.", "B6B4ACE8-6B7E-4A67-AFFF-2CBEFC6C1BBF" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "B6B4ACE8-6B7E-4A67-AFFF-2CBEFC6C1BBF", "629CFBFF-3A95-4294-B13C-37F4FED04FE7", "fa fa-flash" );

            // security
            RockMigrationHelper.AddDefinedValue( "FDC7A191-717E-4CA6-9DCF-A2B5BB09C782", "Security", "Used for styling notes pertaining to security.", "9304261A-2369-4830-B5EE-77BE3C42E9BF" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "9304261A-2369-4830-B5EE-77BE3C42E9BF", "629CFBFF-3A95-4294-B13C-37F4FED04FE7", "fa fa-lock" );

            //
            // Security Fix To Allow Admins to Approve Content Channels Fixes: #773
            //
            Sql( @"DECLARE @AdminGroupId int = (SELECT TOP 1 [Id] FROM [Group] WHERE [Guid] = '628C51A8-4613-43ED-A18D-4A6FB999273E')
  DECLARE @ContentChannelEntityTypeId int = (SELECT TOP 1 [Id] FROM [EntityType] WHERE [Guid] = '44484685-477E-4668-89A6-84F29739EB68')
  
  DECLARE @ExternalAdsId int = (SELECT TOP 1 [Id] FROM [ContentChannel] WHERE [Guid] = '8E213BB1-9E6F-40C1-B468-B3F8A60D5D24')
  DECLARE @ServiceBulletinId int = (SELECT TOP 1 [Id] FROM [ContentChannel] WHERE [Guid] = '3CBD6C7A-30B4-4CF5-B1B9-4216C4EEF371')
  DECLARE @WebsiteBlogId int = (SELECT TOP 1 [Id] FROM [ContentChannel] WHERE [Guid] = '2B408DA7-BDD1-4E71-B6AC-F22D786B605F')

  INSERT INTO [Auth]
	([EntityTypeId], [EntityId], [Order], [Action], [AllowOrDeny], [SpecialRole], [GroupId], [Guid])
  VALUES
	(@ContentChannelEntityTypeId, @ExternalAdsId, 2, 'Approve', 'A', 0, @AdminGroupId, 'E7874EFF-F507-44EB-9405-D8A6FBA2F21C')

 INSERT INTO [Auth]
	([EntityTypeId], [EntityId], [Order], [Action], [AllowOrDeny], [SpecialRole], [GroupId], [Guid])
  VALUES
	(@ContentChannelEntityTypeId, @ServiceBulletinId, 2, 'Approve', 'A', 0, @AdminGroupId, 'D154C7B3-EC9F-4C93-BBE9-CE943712939E')

 INSERT INTO [Auth]
	([EntityTypeId], [EntityId], [Order], [Action], [AllowOrDeny], [SpecialRole], [GroupId], [Guid])
  VALUES
	(@ContentChannelEntityTypeId, @WebsiteBlogId, 2, 'Approve', 'A', 0, @AdminGroupId, '9FB9F2DA-04FF-49C3-A0E3-C09C2E130F6E')" );

            //
            // External Site Menu Fixes Fixes: #790
            //
            Sql( @"DECLARE @LeftSidebarBlockId int = (SELECT TOP 1 [Id] FROM [Block] WHERE [Guid] = '453D10D8-0C30-4721-8446-E4636969A524')
  DECLARE @RightSidebarBlockId int = (SELECT TOP 1 [Id] FROM [Block] WHERE [Guid] = 'D4651935-E652-469D-8EBD-69FF3E684BA0')

  DECLARE @IncludeQuerystringAttrib int = (SELECT TOP 1 [Id] FROM [Attribute] WHERE [Guid] = 'E4CF237D-1D12-4C93-AFD7-78EB296C4B69')
  DECLARE @IncludePageParmsAttrib int = (SELECT TOP 1 [Id] FROM [Attribute] WHERE [Guid] = 'EEE71DDE-C6BC-489B-BAA5-1753E322F183')

 UPDATE [AttributeValue]
 SET [Value] = 'False'
  WHERE [AttributeId] = @IncludeQuerystringAttrib AND [EntityId] IN (@LeftSidebarBlockId, @RightSidebarBlockId)

UPDATE [AttributeValue]
 SET [Value] = 'False'
  WHERE [AttributeId] = @IncludePageParmsAttrib AND [EntityId] IN( @LeftSidebarBlockId, @RightSidebarBlockId)" );

            //
            // Fix Transaction Detail Link Fixes: #794
            //
            Sql( @"DECLARE @BlockId int = (SELECT TOP 1 [Id] FROM [Block] WHERE [Guid] = '9382B285-3EF6-47F7-94BB-A47C498196A3')
  DECLARE @AttributeId int = (SELECT TOP 1 [Id] FROM [Attribute] WHERE [Guid] = 'C6D07A89-84C9-412A-A584-E37E59506566')

  UPDATE [AttributeValue]
  SET [Value] = 'B67E38CB-2EF1-43EA-863A-37DAA1C7340F'
  WHERE [AttributeId] = @AttributeId AND [EntityId] = @BlockId" );
            
            //
            // Update Get Address Function
            //
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

	</remarks>
	<code>
		SELECT [dbo].[ufnCrm_GetAddress](3, 'Home', 'Full')
		SELECT [dbo].[ufnCrm_GetAddress](3, 'Home', 'Street1')
		SELECT [dbo].[ufnCrm_GetAddress](3, 'Home', 'Street2')
		SELECT [dbo].[ufnCrm_GetAddress](3, 'Home', 'City')
		SELECT [dbo].[ufnCrm_GetAddress](3, 'Home', 'State')
		SELECT [dbo].[ufnCrm_GetAddress](3, 'Home', 'PostalCode')
		SELECT [dbo].[ufnCrm_GetAddress](3, 'Home', 'Country')
	</code>
</doc>
*/

ALTER FUNCTION [dbo].[ufnCrm_GetAddress](
	@PersonId int, 
	@AddressType varchar(20),
	@AddressComponent varchar(20)) 

RETURNS nvarchar(250) AS

BEGIN
	DECLARE @AddressTypeId int

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
		SET @AddressTypeId = CAST(@AddressType AS int)

	-- return address component
	IF (@AddressComponent = 'Street1')
		BEGIN
		RETURN (SELECT [Street1] FROM [Location] WHERE [Id] = (SELECT TOP 1 [LocationId] FROM [GroupLocation] WHERE  [GroupLocationTypeValueId] = @AddressTypeId AND  [GroupId] = (SELECT TOP 1 [GroupId] FROM [GroupMember] gm INNER JOIN [Group] g ON g.[Id] = gm.[GroupId] WHERE [PersonId] = @PersonId AND g.[GroupTypeId] = 10))) 
		END
	ELSE IF (@AddressComponent = 'Street2')
		BEGIN
		RETURN (SELECT [Street2] FROM [Location] WHERE [Id] = (SELECT TOP 1 [LocationId] FROM [GroupLocation] WHERE  [GroupLocationTypeValueId] = @AddressTypeId AND  [GroupId] = (SELECT TOP 1 [GroupId] FROM [GroupMember] gm INNER JOIN [Group] g ON g.[Id] = gm.[GroupId] WHERE [PersonId] = @PersonId AND g.[GroupTypeId] = 10))) 
		END
	ELSE IF (@AddressComponent = 'City')
		BEGIN
		RETURN (SELECT [City] FROM [Location] WHERE [Id] = (SELECT TOP 1 [LocationId] FROM [GroupLocation] WHERE  [GroupLocationTypeValueId] = @AddressTypeId AND  [GroupId] = (SELECT TOP 1 [GroupId] FROM [GroupMember] gm INNER JOIN [Group] g ON g.[Id] = gm.[GroupId] WHERE [PersonId] = @PersonId AND g.[GroupTypeId] = 10))) 
		END
	ELSE IF (@AddressComponent = 'State')
		BEGIN
		RETURN (SELECT [State] FROM [Location] WHERE [Id] = (SELECT TOP 1 [LocationId] FROM [GroupLocation] WHERE  [GroupLocationTypeValueId] = @AddressTypeId AND  [GroupId] = (SELECT TOP 1 [GroupId] FROM [GroupMember] gm INNER JOIN [Group] g ON g.[Id] = gm.[GroupId] WHERE [PersonId] = @PersonId AND g.[GroupTypeId] = 10))) 
		END
	ELSE IF (@AddressComponent = 'PostalCode')
		BEGIN
		RETURN (SELECT [PostalCode] FROM [Location] WHERE [Id] = (SELECT TOP 1 [LocationId] FROM [GroupLocation] WHERE  [GroupLocationTypeValueId] = @AddressTypeId AND  [GroupId] = (SELECT TOP 1 [GroupId] FROM [GroupMember] gm INNER JOIN [Group] g ON g.[Id] = gm.[GroupId] WHERE [PersonId] = @PersonId AND g.[GroupTypeId] = 10))) 
		END
	ELSE IF (@AddressComponent = 'Country')
		BEGIN
		RETURN (SELECT [Country] FROM [Location] WHERE [Id] = (SELECT TOP 1 [LocationId] FROM [GroupLocation] WHERE  [GroupLocationTypeValueId] = @AddressTypeId AND  [GroupId] = (SELECT TOP 1 [GroupId] FROM [GroupMember] gm INNER JOIN [Group] g ON g.[Id] = gm.[GroupId] WHERE [PersonId] = @PersonId AND g.[GroupTypeId] = 10))) 
		END
	ELSE 
		BEGIN
		RETURN (SELECT [Street1] + ' ' + [Street2] + ' ' + [City] + ', ' + [State] + ' ' + [PostalCode] FROM [Location] WHERE [Id] = (SELECT TOP 1 [LocationId] FROM [GroupLocation] WHERE  [GroupLocationTypeValueId] = @AddressTypeId AND  [GroupId] = (SELECT TOP 1 [GroupId] FROM [GroupMember] gm INNER JOIN [Group] g ON g.[Id] = gm.[GroupId] WHERE [PersonId] = @PersonId AND g.[GroupTypeId] = 10))) 
		END

	RETURN ''
END" );
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            //
            // Note Types
            //
            RockMigrationHelper.DeleteDefinedValue( "EBB52236-B29A-4E00-B176-15B351679F99" );
            RockMigrationHelper.DeleteDefinedValue( "68221BAB-13E6-43BB-9CEA-81507C5E4BAF" );
            RockMigrationHelper.DeleteDefinedValue( "643FC41E-AF5C-4FD1-8118-8DAAE254DAA5" );
            RockMigrationHelper.DeleteDefinedValue( "128C609C-1601-4247-8BA2-6CB60AB2562C" );
            RockMigrationHelper.DeleteDefinedValue( "A52CEA75-CD49-43F7-82FF-D781835EB73F" );
            RockMigrationHelper.DeleteDefinedValue( "EF911C5F-A922-4B21-B601-FD306538B0E2" );
            RockMigrationHelper.DeleteDefinedValue( "B6B4ACE8-6B7E-4A67-AFFF-2CBEFC6C1BBF" );
            RockMigrationHelper.DeleteDefinedValue( "9304261A-2369-4830-B5EE-77BE3C42E9BF" );
            
            //
            // Delete security settings for content channels
            //

            Sql( @"DELETE FROM [Auth] WHERE [Guid] in ( 'E7874EFF-F507-44EB-9405-D8A6FBA2F21C', 'D154C7B3-EC9F-4C93-BBE9-CE943712939E', '9FB9F2DA-04FF-49C3-A0E3-C09C2E130F6E' )" );
        }
    }
}
