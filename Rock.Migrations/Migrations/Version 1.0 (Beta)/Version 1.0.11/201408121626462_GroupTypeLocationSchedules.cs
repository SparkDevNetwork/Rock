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
    public partial class GroupTypeLocationSchedules : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            AddColumn("dbo.GroupType", "EnableLocationSchedules", c => c.Boolean());

            Sql( @"
    UPDATE [GroupType] SET [EnableLocationSchedules] = 1
    WHERE [Guid] IN (
          'CADB2D12-7836-44BC-8EEA-3C6AB22FD5E8' -- Nursery/Preschool Area
        , 'E3C8F7D6-5CEB-43BB-802F-66C3E734049E' -- Elementary Area
        , '7A17235B-69AD-439B-BAB0-1A0A472DB96F' -- Jr High Area
        , '9A88743B-F336-4404-B877-2A623689195D' -- High School Area
        , '2C42B2D4-1C5F-4AD5-A9AD-08631B872AC4' -- Serving Team
    )

    DECLARE @ServingTeamGroupTypeId int = ( 
        SELECT TOP 1 [Id] FROM [GroupType]
        WHERE [Guid] = '2C42B2D4-1C5F-4AD5-A9AD-08631B872AC4'
    )

    UPDATE [GroupType] SET 
          [LocationSelectionMode] = 2 -- Named
        , [AttendanceRule] = 2 -- Must be in group
    WHERE [Id] = @ServingTeamGroupTypeId

    DECLARE @MeetingLocationId int = (
        SELECT TOP 1 [Id] FROM [DefinedValue]
        WHERE [Guid] = '96D540F5-071D-4BBD-9906-28F0A64D39C4'
    )

    IF NOT EXISTS ( 
        SELECT [GroupTypeId] FROM [GroupTypeLocationType]
        WHERE [GroupTypeId] = @ServingTeamGroupTypeId AND [LocationTypeValueId] = @MeetingLocationId
    )
    BEGIN
        INSERT INTO [GroupTypeLocationType] ( [GroupTypeId], [LocationTypeValueId] )
        VALUES ( @ServingTeamGroupTypeId, @MeetingLocationId )
    END
   
    -- Add Name Tag label to Serving Team Group Type
    DECLARE @CheckInLabelFileTypeId int = ( SELECT TOP 1 [Id] FROM [BinaryFileType] WHERE [Guid] = 'DE0E5C50-234B-474C-940C-C571F385E65F' )
    DECLARE @NameTagBinaryFileId int = ( SELECT TOP 1 [Id] FROM [BinaryFile] WHERE [Guid] = '918F55A6-BD4B-4070-9017-D5FC5207653E' )
    DECLARE @EntityTypeId int = ( SELECT TOP 1 [Id] FROM [EntityType] WHERE [Name] = 'Rock.Model.GroupType' )
    DECLARE @FieldTypeId int = ( SELECT TOP 1 [Id] FROM [FieldType] WHERE [Guid] = 'C403E219-A56B-439E-9D50-9302DFE760CF' )

    DELETE [Attribute] 
    WHERE [EntityTypeId] = @EntityTypeId
    AND [Key] = 'NameTag'
    AND [EntityTypeQualifierColumn] = 'Id'
    AND [EntityTypeQualifierValue] = CAST(@ServingTeamGroupTypeId AS VARCHAR)

    INSERT INTO [Attribute] (
        [IsSystem],[FieldTypeId],[EntityTypeId],[EntityTypeQualifierColumn],[EntityTypeQualifierValue],
        [Key],[Name],
        [Order],[IsGridColumn],[DefaultValue],[IsMultiValue],[IsRequired],
        [Guid])
    VALUES(
        1,@FieldTypeId,@EntityTypeid,'Id',CAST(@ServingTeamGroupTypeId AS VARCHAR),
        'NameTag','Name Tag',
        0,0,CAST(@NameTagBinaryFileId AS VARCHAR),0,0,
        '1B07007A-2A4B-4F03-9711-4AC107FAFABA')

    DECLARE @AttributeId int = ( SELECT TOP 1 [Id] FROM [Attribute] WHERE [Guid] = '1B07007A-2A4B-4F03-9711-4AC107FAFABA' )
    INSERT INTO [AttributeQualifier] ( [IsSystem], [AttributeId], [Key], [Value], [Guid] )
    VALUES ( 1, @AttributeId, 'binaryFileType', CAST( @CheckInLabelFileTypeId AS VARCHAR ), '06401D07-102F-4FEC-8F2C-ED4F4E177184' )
    
    -- Update the Central Kiosk device to be associated with campus location instead of each room (change to checking to support this is coming)
    DECLARE @KioskId int = ( SELECT TOP 1 [Id] FROM [Device] WHERE [Guid] = '61111232-01D7-427D-9C1F-D45CF4D3F7CB' )
    DECLARE @CampusLocationId int = ( SELECT TOP 1 [Id] FROM [Location] WHERE [Guid] = 'D5171D44-C801-4B7D-9335-D23FB4EA0E60' )
    
    DELETE DL
    FROM [DeviceLocation] DL
    INNER JOIN [Location] L on L.[Id] = DL.[LocationId]
    WHERE L.[Guid] IN (
          '844336F4-88B4-4894-B416-769C95A4702D'
        , 'C07B25A6-EB58-4F77-8ED3-952C5A4DEE9F'
        , '0BD7BF34-647D-4DC1-8C61-DD154B403687'
        , 'DFCBFE8A-D1E8-4E95-9561-81826C1C2783'
        , 'E847E31E-ABA2-415B-A0E1-770FF1B64425'
        , 'F9F33251-F829-4618-904E-42A1EDA2B58E'
        , '40AABD2F-40A0-4F95-96CD-1ACACA7460B0'
        , '2459095D-78DF-405E-8974-DDBDBD053AEE'    
    )

    INSERT INTO [DeviceLocation] ( [DeviceId], [LocationId] )
    VALUES ( @KioskId, @CampusLocationId )
    
" );
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            RockMigrationHelper.DeleteAttribute( "1B07007A-2A4B-4F03-9711-4AC107FAFABA" );

            DropColumn( "dbo.GroupType", "EnableLocationSchedules" );
        }
    }
}
