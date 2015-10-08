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
    public partial class NewBadgeSetup : Rock.Migrations.RockMigration3
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            // add entities for new badge types
            Sql(@"
    
                IF NOT EXISTS (SELECT [ID] FROM [EntityType] WHERE [Name] = 'Rock.Model.PersonBadge')
                BEGIN
	                INSERT INTO [dbo].[EntityType]
                           ([Name]
                           ,[AssemblyName]
                           ,[FriendlyName]
                           ,[IsEntity]
                           ,[IsSecured]
                           ,[IsCommon]
                           ,[Guid])
                     VALUES (
                           'Rock.Model.PersonBadge'
                           ,'Rock.Model.PersonBadge, Rock, Version=0.1.5.0, Culture=neutral, PublicKeyToken=null'
                           ,'Person Badge'
                           ,1
                           ,1
                           ,0
                           ,'99300129-6F4C-45B2-B486-71123F046289')
                END

                IF NOT EXISTS (SELECT [ID] FROM [EntityType] WHERE [Name] = 'Rock.PersonProfile.Badge.FamilyWeeksAttendedInDuration')
                BEGIN
	                INSERT INTO [dbo].[EntityType]
                           ([Name]
                           ,[AssemblyName]
                           ,[FriendlyName]
                           ,[IsEntity]
                           ,[IsSecured]
                           ,[IsCommon]
                           ,[Guid])
                     VALUES (
                           'Rock.PersonProfile.Badge.FamilyWeeksAttendedInDuration'
                           ,'Rock.PersonProfile.Badge.FamilyWeeksAttendedInDuration, Rock, Version=0.1.5.0, Culture=neutral, PublicKeyToken=null'
                           ,'Family Weeks Attended In Duration'
                           ,0
                           ,1
                           ,0
                           ,'537D05E5-5F89-421C-9F3D-04ADDEEC7C10')
                END

                IF NOT EXISTS (SELECT [ID] FROM [EntityType] WHERE [Name] = 'Rock.PersonProfile.Badge.AttendingDuration')
                BEGIN
	                INSERT INTO [dbo].[EntityType]
                           ([Name]
                           ,[AssemblyName]
                           ,[FriendlyName]
                           ,[IsEntity]
                           ,[IsSecured]
                           ,[IsCommon]
                           ,[Guid])
                     VALUES (
                           'Rock.PersonProfile.Badge.AttendingDuration'
                           ,'Rock.PersonProfile.Badge.AttendingDuration, Rock, Version=0.1.5.0, Culture=neutral, PublicKeyToken=null'
                           ,'Attending Duration'
                           ,0
                           ,1
                           ,0
                           ,'B50090B4-9424-4963-B34F-957394FFBB3E')
                END

                IF NOT EXISTS (SELECT [ID] FROM [EntityType] WHERE [Name] = 'Rock.PersonProfile.Badge.InGroupOfType')
                BEGIN
	                INSERT INTO [dbo].[EntityType]
                           ([Name]
                           ,[AssemblyName]
                           ,[FriendlyName]
                           ,[IsEntity]
                           ,[IsSecured]
                           ,[IsCommon]
                           ,[Guid])
                     VALUES (
                           'Rock.PersonProfile.Badge.InGroupOfType'
                           ,'Rock.PersonProfile.Badge.InGroupOfType, Rock, Version=0.1.5.0, Culture=neutral, PublicKeyToken=null'
                           ,'In Group Of Type'
                           ,0
                           ,1
                           ,0
                           ,'E7CE9210-FE85-4772-9225-E6C721B816BD')
                END

                IF NOT EXISTS (SELECT [ID] FROM [EntityType] WHERE [Name] = 'Rock.PersonProfile.Badge.Liquid')
                BEGIN
	                INSERT INTO [dbo].[EntityType]
                           ([Name]
                           ,[AssemblyName]
                           ,[FriendlyName]
                           ,[IsEntity]
                           ,[IsSecured]
                           ,[IsCommon]
                           ,[Guid])
                     VALUES (
                           'Rock.PersonProfile.Badge.Liquid'
                           ,'Rock.PersonProfile.Badge.Liquid, Rock, Version=0.1.5.0, Culture=neutral, PublicKeyToken=null'
                           ,'Liquid'
                           ,0
                           ,1
                           ,0
                           ,'95912004-62B5-4460-951F-D752427D44FE')
                END

            ");
            
            // make liquid attribute 'well known'
            Sql(@"
                    -- fix liquid attribute field id
                    DECLARE @LiquidAttributeGuid uniqueidentifier = '01C9BA59-D8D4-4137-90A6-B3C06C70BBC3' 
                    DECLARE @PersonBadgeEntityTypeId int = (SELECT [Id] FROM [EntityType] WHERE [Name] = 'Rock.Model.PersonBadge')
                    DECLARE @LiquidBadgeEntityTypeId int = (SELECT [Id] FROM [EntityType] WHERE [Name] = 'Rock.PersonProfile.Badge.Liquid')
                    DECLARE @CodeEditorFieldTypeId int = (SELECT [Id] FROM [FieldType] WHERE [Guid] = '1D0D3794-C210-48A8-8C68-3FBEC08A6BA5')

                    IF EXISTS (SELECT [Id] FROM [Attribute] 
                           WHERE [EntityTypeId] = @PersonBadgeEntityTypeId
                           AND [EntityTypeQualifierColumn] = 'EntityTypeId' 
                           AND [EntityTypeQualifierValue] = CAST(@LiquidBadgeEntityTypeId AS varchar)
                           AND [Key] = 'DisplayText')
                    BEGIN
		                    UPDATE [Attribute] SET [Guid] = @LiquidAttributeGuid
		                    WHERE [EntityTypeId] = @PersonBadgeEntityTypeId
		                    AND [EntityTypeQualifierColumn] = 'EntityTypeId' 
		                    AND [EntityTypeQualifierValue] = CAST(@LiquidBadgeEntityTypeId AS varchar)
		                    AND [Key] = 'DisplayText'
		
                    END
                    ELSE
                    BEGIN
      
	                       INSERT INTO [Attribute] ([IsSystem], [FieldTypeId], [EntityTypeId], [EntityTypeQualifierColumn], [EntityTypeQualifierValue],
                                  [Key], [Name], [Order], [IsGridColumn], [IsMultiValue], [IsRequired], [Guid])
                           VALUES ( 1, @CodeEditorFieldTypeId, @PersonBadgeEntityTypeId, 'EntityTypeId',  CAST(@LiquidBadgeEntityTypeId AS varchar),
                                  'DisplayText', 'Display Text', 0, 0, 0, 0, @LiquidAttributeGuid)
                    END
            ");

            // add badges
            Sql(@"
                DECLARE @LiquidAttributeGuid uniqueidentifier = '01C9BA59-D8D4-4137-90A6-B3C06C70BBC3' 
                DECLARE @PersonBadgeEntityTypeId int = (SELECT [Id] FROM [EntityType] WHERE [Name] = 'Rock.Model.PersonBadge')
                DECLARE @LiquidBadgeEntityTypeId int = (SELECT [Id] FROM [EntityType] WHERE [Name] = 'Rock.PersonProfile.Badge.Liquid')
                DECLARE @CodeEditorFieldTypeId int = (SELECT [Id] FROM [FieldType] WHERE [Guid] = '1D0D3794-C210-48A8-8C68-3FBEC08A6BA5')
                DECLARE @LiquidAttributeId int = (SELECT [ID] FROM [Attribute] WHERE [Guid] = '01C9BA59-D8D4-4137-90A6-B3C06C70BBC3' )

                -- add badge for attendance duration

	                DECLARE @FamilyWeeksAttendedInDurationBadge int = (SELECT [ID] FROM [EntityType] WHERE [Name] = 'Rock.PersonProfile.Badge.FamilyWeeksAttendedInDuration')
	                INSERT INTO [PersonBadge] ([Name],[Description],[EntityTypeId],[Order],[Guid])
                VALUES
	                ('Family 16 Week Attendance', 'Shows a the number of times a family attended in the last 16 weeks.', @FamilyWeeksAttendedInDurationBadge, 1, '66972BFF-42CD-49AB-9A7A-E1B9DECA4EBA')
	
	                -- Set first badge list badges
                    UPDATE [AttributeValue] SET
                    VALUE = '66972BFF-42CD-49AB-9A7A-E1B9DECA4EBA'
                    WHERE [Guid] = '4F41CA56-BF42-4601-8123-EC71737C4E36'

                -- add baptism badge
                DECLARE @LiquidBadge int
                SET @LiquidBadge = (SELECT [ID] FROM [EntityType] WHERE [Name] = 'Rock.PersonProfile.Badge.Liquid')

                INSERT INTO [PersonBadge] ([Name],[Description],[EntityTypeId],[Order],[Guid])
                VALUES
	                ('Baptism', 'Shows if individual has been baptized.', @LiquidBadge, 0, '66972BFF-42CD-49AB-9A7A-E1B9DECA4EBE')
	
                INSERT INTO [AttributeValue] ([IsSystem], [AttributeId], [EntityId], [Order], [Value], [Guid], [CreatedDateTime], [ModifiedDateTime], [CreatedByPersonAliasId], [ModifiedByPersonAliasId])
                VALUES
	                (0, @LiquidAttributeId, @@IDENTITY, 0, '{% if Person.BaptismDate != empty -%}
                    <div class=""badge badge-baptism"" data-original-title=""{{ Person.NickName }} was baptized on {{ Person.BaptismDate }}."">
                        <i class=""badge-icon fa fa-tint""></i>
                    </div>
                {% else -%}
                    <div class=""badge badge-baptism"" data-original-title=""No baptism date entered for {{ Person.NickName }}."">
                        <i class=""badge-icon badge-disabled fa fa-tint""></i>
                    </div>
                {% endif -%}', newid(), getdate(), getdate(), null,null)

                -- Set third badge list badges
                    UPDATE [AttributeValue] SET
                    VALUE = '66972BFF-42CD-49AB-9A7A-E1B9DECA4EBE'
                    WHERE [Guid] = 'AC977001-8059-43A1-AB35-55F37DA40695'

                -- add connection status badge 

                INSERT INTO [PersonBadge] ([Name],[Description],[EntityTypeId],[Order],[Guid])
                VALUES
	                ('Connection Status', 'Displays the connection status as a label.', @LiquidBadge, 0, '66972BFF-42CD-49AB-9A7A-E1B9DECA4EBF')
	
                INSERT INTO [AttributeValue] ([IsSystem], [AttributeId], [EntityId], [Order], [Value], [Guid], [CreatedDateTime], [ModifiedDateTime], [CreatedByPersonAliasId], [ModifiedByPersonAliasId])
                VALUES
	                (0, @LiquidAttributeId, @@IDENTITY, 0, '{% if Person.ConnectionStatusValue.Name != empty -%}
                    <span class=""label label-success"">{{ Person.ConnectionStatusValue.Name }}</span> 
                {% endif -%}', newid(), getdate(), getdate(), null,null)

                -- add inactive badge
                INSERT INTO [PersonBadge] ([Name],[Description],[EntityTypeId],[Order],[Guid])
                VALUES
	                ('Inactive Record Status', 'Displays label if record status is inactive.', @LiquidBadge, 3, '66972BFF-42CD-49AB-9A7A-E1B9DECA4ECA')
	
                INSERT INTO [AttributeValue] ([IsSystem], [AttributeId], [EntityId], [Order], [Value], [Guid], [CreatedDateTime], [ModifiedDateTime], [CreatedByPersonAliasId], [ModifiedByPersonAliasId])
                VALUES
	                (0, @LiquidAttributeId, @@IDENTITY, 0, '{% if Person.RecordStatusValue.Name != empty and Person.RecordStatusValue.Name == ""Inactive"" -%}
                    <span class=""label label-danger"" title=""{{ Person.RecordStatusReasonValue.Name }}"" data-toggle=""tooltip"">{{ Person.RecordStatusValue.Name }}</span> 
                {% endif -%}
                ', newid(), getdate(), getdate(), null,null)

                -- set bio badges
                    UPDATE [AttributeValue] SET
                    VALUE = 'B21DCD49-AC35-4B2B-9857-75213209B643,66972BFF-42CD-49AB-9A7A-E1B9DECA4ECA,66972BFF-42CD-49AB-9A7A-E1B9DECA4EBF'
                    WHERE [Guid] = 'E0C5374F-3B93-4A78-BAB3-854D022B0E96'

                -- add attending duration

                DECLARE @AttedningDurationBadge int  = (SELECT [ID] FROM [EntityType] WHERE [Name] = 'Rock.PersonProfile.Badge.AttendingDuration')

                INSERT INTO [PersonBadge] ([Name],[Description],[EntityTypeId],[Order],[Guid])
                VALUES
	                ('Attending Duration', 'Displays how long the individual has been attending.', @AttedningDurationBadge, 0, '260EAD7D-5073-4F88-A6A9-427F6E95985E')
	
	                -- Set first badge list badges
                    UPDATE [AttributeValue] SET
                    VALUE = '66972BFF-42CD-49AB-9A7A-E1B9DECA4EBA,260EAD7D-5073-4F88-A6A9-427F6E95985E'
                    WHERE [Guid] = '4F41CA56-BF42-4601-8123-EC71737C4E36'

                -- add 'in group of type' badge

                DECLARE @InGroupOfTypeBadge int  = (SELECT [ID] FROM [EntityType] WHERE [Name] = 'Rock.PersonProfile.Badge.InGroupOfType')

                -- create attributes for this badge

                DECLARE @GroupTypeAttributeGuid uniqueidentifier = '4AC30B82-A5A3-4B65-BB54-620EB10CE83B' 
                DECLARE @BadgeColorAttributeGuid uniqueidentifier = '9C204CD0-1233-41C5-818A-C5DA439445AB' 
                DECLARE @AttendingDurationBadgeEntityTypeId int  = (SELECT [Id] FROM [EntityType] WHERE [Name] = 'Rock.PersonProfile.Badge.InGroupOfType')
                DECLARE @GroupTypeFieldTypeId int  = (SELECT [Id] FROM [FieldType] WHERE [Guid] = '18E29E23-B43B-4CF7-AE41-C85672C09F50')
                DECLARE @TextBoxFieldTypeId int  = (SELECT [Id] FROM [FieldType] WHERE [Guid] = '9C204CD0-1233-41C5-818A-C5DA439445AA')

                -- setup group type attribute
                IF EXISTS (SELECT [Id] FROM [Attribute] 
                       WHERE [EntityTypeId] = @PersonBadgeEntityTypeId
                       AND [EntityTypeQualifierColumn] = 'EntityTypeId' 
                       AND [EntityTypeQualifierValue] = CAST(@AttendingDurationBadgeEntityTypeId AS varchar)
                       AND [Key] = 'GroupType')
                BEGIN
          
	                UPDATE [Attribute] SET [Guid] = @GroupTypeAttributeGuid
                    WHERE [EntityTypeId] = @PersonBadgeEntityTypeId
                    AND [EntityTypeQualifierColumn] = 'EntityTypeId' 
                    AND [EntityTypeQualifierValue] = CAST(@AttendingDurationBadgeEntityTypeId AS varchar)
                    AND [Key] = 'GroupType'
                END
                ELSE
                BEGIN

	                   INSERT INTO [Attribute] ([IsSystem], [FieldTypeId], [EntityTypeId], [EntityTypeQualifierColumn], [EntityTypeQualifierValue],
                              [Key], [Name], [Order], [IsGridColumn], [IsMultiValue], [IsRequired], [Guid], [Description])
                       VALUES ( 1, @GroupTypeFieldTypeId, @PersonBadgeEntityTypeId, 'EntityTypeId',  CAST(@AttendingDurationBadgeEntityTypeId AS varchar),
                              'GroupType', 'Group Type', 0, 0, 0, 1, @GroupTypeAttributeGuid, 'The type of group to use.')
                END

                -- setup color attribute
                IF EXISTS (SELECT [Id] FROM [Attribute] 
                       WHERE [EntityTypeId] = @PersonBadgeEntityTypeId
                       AND [EntityTypeQualifierColumn] = 'EntityTypeId' 
                       AND [EntityTypeQualifierValue] = CAST(@AttendingDurationBadgeEntityTypeId AS varchar)
                       AND [Key] = 'BadgeColor')
                BEGIN
          
	                UPDATE [Attribute] SET [Guid] = @BadgeColorAttributeGuid
                    WHERE [EntityTypeId] = @PersonBadgeEntityTypeId
                    AND [EntityTypeQualifierColumn] = 'EntityTypeId' 
                    AND [EntityTypeQualifierValue] = CAST(@AttendingDurationBadgeEntityTypeId AS varchar)
                    AND [Key] = 'BadgeColor'
                END
                ELSE
                BEGIN

	                   INSERT INTO [Attribute] ([IsSystem], [FieldTypeId], [EntityTypeId], [EntityTypeQualifierColumn], [EntityTypeQualifierValue],
                              [Key], [Name], [Order], [IsGridColumn], [IsMultiValue], [IsRequired], [Guid], [Description])
                       VALUES ( 1, @TextBoxFieldTypeId, @PersonBadgeEntityTypeId, 'EntityTypeId',  CAST(@AttendingDurationBadgeEntityTypeId AS varchar),
                              'BadgeColor', 'Badge Color', 0, 0, 0, 1, @BadgeColorAttributeGuid, 'The color to make the icon (#ffffff).')
                END


                -- creat instance of badge
                INSERT INTO [PersonBadge] ([Name],[Description],[EntityTypeId],[Order],[Guid])
                VALUES
	                ('In Serving Team', 'Show if individual is in a serving team.', @AttendingDurationBadgeEntityTypeId, 1, 'E0455598-82B0-4F49-B806-C3A41C71E9DA')
	

                DECLARE @GroupTypeAttributeId int  = (SELECT [Id] FROM [Attribute] 
									                   WHERE [EntityTypeId] = @PersonBadgeEntityTypeId
									                   AND [EntityTypeQualifierColumn] = 'EntityTypeId' 
									                   AND [EntityTypeQualifierValue] = CAST(@AttendingDurationBadgeEntityTypeId AS varchar)
									                   AND [Key] = 'GroupType')
                DECLARE @ColorAttributeId int = (SELECT [Id] FROM [Attribute] 
									                   WHERE [EntityTypeId] = @PersonBadgeEntityTypeId
									                   AND [EntityTypeQualifierColumn] = 'EntityTypeId' 
									                   AND [EntityTypeQualifierValue] = CAST(@AttendingDurationBadgeEntityTypeId AS varchar)
									                   AND [Key] = 'BadgeColor')

                DECLARE @ServingBadgeId int = @@IDENTITY

	                INSERT INTO [AttributeValue] ([IsSystem], [AttributeId], [EntityId], [Order], [Value], [Guid], [CreatedDateTime], [ModifiedDateTime], [CreatedByPersonAliasId], [ModifiedByPersonAliasId])
                VALUES
	                (0, @GroupTypeAttributeId, @ServingBadgeId, 0, '2C42B2D4-1C5F-4AD5-A9AD-08631B872AC4', newid(), getdate(), getdate(), null,null)

	                INSERT INTO [AttributeValue] ([IsSystem], [AttributeId], [EntityId], [Order], [Value], [Guid], [CreatedDateTime], [ModifiedDateTime], [CreatedByPersonAliasId], [ModifiedByPersonAliasId])
                VALUES
	                (0, @ColorAttributeId, @ServingBadgeId, 0, '#443b56', newid(), getdate(), getdate(), null,null)

	                -- Set third badge list badges
                    UPDATE [AttributeValue] SET
                    VALUE = '66972BFF-42CD-49AB-9A7A-E1B9DECA4EBE, E0455598-82B0-4F49-B806-C3A41C71E9DA'
                    WHERE [Guid] = 'AC977001-8059-43A1-AB35-55F37DA40695'
            ");

            // add sql procs and functions for badges
            Sql(@"
                    -- create function for attendance duration
                    /*
                    <doc>
	                    <summary>
 		                    This function returns the date of the previous Sunday.
	                    </summary>

	                    <returns>
		                    Datetime of the previous Sunday.
	                    </returns>
	                    <remarks>
		
	                    </remarks>
	                    <code>
		                    SELECT [dbo].[ufnUtility_GetPreviousSundayDate]()
	                    </code>
                    </doc>
                    */

                    CREATE FUNCTION [dbo].[ufnUtility_GetPreviousSundayDate]() 

                    RETURNS date AS

                    BEGIN

	                    RETURN DATEADD(""day"", -7, dbo.ufnUtility_GetSundayDate(getdate()))
                    END
            ");

            Sql(@"
                    -- create stored proc for attendance duration
                    /*
                    <doc>
	                    <summary>
 		                    This function returns the number of weekends a member of a family has attended a weekend service
		                    in the last X weeks.
	                    </summary>

	                    <returns>
		                    * Number of weeks
	                    </returns>
	                    <param name=""PersonId"" datatype=""int"">The person id to use</param>
	                    <param name=""WeekDuration"" datatype=""int"">The number of weeks to use as the duration (default 16)</param>
	                    <remarks>	
	                    </remarks>
	                    <code>
		                    EXEC [dbo].[spCheckin_WeeksAttendedInDuration] 2 -- Ted Decker
	                    </code>
                    </doc>
                    */

                    CREATE PROCEDURE [dbo].[spCheckin_WeeksAttendedInDuration]
	                    @PersonId int
	                    ,@WeekDuration int = 16
                    AS
                    BEGIN
	
                    DECLARE @LastSunday datetime 

                    SET @LastSunday = [dbo].[ufnUtility_GetPreviousSundayDate]()

                    SELECT 
	                    COUNT(DISTINCT dbo.ufnUtility_GetSundayDate(a.[StartDateTime]) )
                    FROM
	                    [Attendance] a
                    WHERE 
	                    [GroupId] IN (SELECT [Id] FROM [dbo].[ufnCheckin_WeeklyServiceGroups]())
	                    AND a.[PersonId] IN (SELECT [Id] FROM [dbo].[ufnCrm_FamilyMembersOfPersonId](@PersonId))
	                    AND a.[StartDateTime] BETWEEN DATEADD(""week"", (@WeekDuration * -1), @LastSunday) AND @LastSunday 

                    END
            ");

            // add icon to the serving team type so it's badge works
            Sql(@"
                UPDATE [GroupType]
	            SET [IconCssClass] = 'fa fa-clock-o'
	            WHERE [Guid] = '2C42B2D4-1C5F-4AD5-A9AD-08631B872AC4'
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
