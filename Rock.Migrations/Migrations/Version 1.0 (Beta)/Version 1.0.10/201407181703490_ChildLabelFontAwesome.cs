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
    public partial class ChildLabelFontAwesome : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            // add the ZPL file and data for the child label (font awesome)
            Sql( @"
    DECLARE @ChildLabelIconFileId INT 

    INSERT INTO [BinaryFile] 
	    ([IsTemporary] 
	    ,[IsSystem] 
	    ,[BinaryFileTypeId] 
	    ,[Url] 
	    ,[FileName] 
	    ,[MimeType] 
	    ,[Description] 
	    ,[StorageEntityTypeId] 
	    ,[Guid]) 
    VALUES
	    (0 
	    ,1 
	    ,1 
	    ,NULL 
	    ,N'Child Label (Icon)' 
	    ,N'text/plain' 
	    ,N'Icon label for attaching to the child.' 
	    ,NULL 
	    ,'C2E6B0A0-3991-4FAF-9E2F-49CF321CBB0D') 

    SET @ChildLabelIconFileId = SCOPE_IDENTITY() 

    INSERT INTO [BinaryFileData] 
	    ([Id] 
	    ,[Content] 
	    ,[Guid]) 
    VALUES
	    (@ChildLabelIconFileId 
	    ,0xEFBBBF1043547E7E43442C7E43435E7E43547E0D0A5E58417E54413030307E4A534E5E4C54305E4D4E575E4D54445E504F4E5E504D4E5E4C48302C305E4A4D415E5052362C367E534432345E4A55535E4C524E5E4349305E585A0D0A5E58410D0A5E4D4D540D0A5E50573831320D0A5E4C4C303430360D0A5E4C53300D0A5E46543435322C3131395E41304E2C3133352C3133345E46423333332C312C302C525E46485C5E46445757575E46530D0A5E465431322C3236385E41304E2C3133352C3134365E46485C5E4644355E46530D0A5E465431342C3332375E41304E2C34352C34355E46485C5E4644365E46530D0A5E43575A2C453A524F433030302E464E545E46543239332C38325E415A4E2C37332C36340D0A5E46485C5E4644425E46530D0A5E43575A2C453A524F433030302E464E545E46543337382C38315E415A4E2C37332C36340D0A5E46485C5E4644465E46530D0A5E46543239392C3132305E41304E2C32382C32385E46485C5E4644345E46530D0A5E46543333382C3338355E41304E2C32382C32385E46485C5E464431305E46530D0A5E465431332C3338355E41304E2C32382C32385E46485C5E4644395E46530D0A5E43575A2C453A524F433030302E464E545E46543630352C3338335E415A4E2C37332C36340D0A5E46485C5E4644375E46530D0A5E43575A2C453A524F433030302E464E545E46543731352C3338365E415A4E2C37332C36340D0A5E46485C5E4644385E46530D0A5E4C52595E464F302C305E47423831322C302C3133365E46535E4C524E0D0A5E5051312C302C312C595E585A0D0A
	    ,'673F9243-3702-4097-8E2D-F10036F48F18') 

-- add the merge fields for the label

DECLARE @SecurityCodeValueId INT = (SELECT [Id] 
       FROM   [DefinedValue] 
       WHERE  [Guid] = '9DCB69C4-1B9B-4D31-B8B6-8C9DBFA9933B') 
    DECLARE @NickNameValueId INT = (SELECT [Id] 
       FROM   [DefinedValue] 
       WHERE  [Guid] = 'E2DB40F4-A064-429E-9A76-C520E7E7A43A') 
    DECLARE @LastNameValueId INT = (SELECT [Id] 
       FROM   [DefinedValue] 
       WHERE  [Guid] = '85B71255-19F9-4443-A7AE-EF670385DC71') 
    DECLARE @BirthdayIconValueId INT = (SELECT [Id] 
       FROM   [DefinedValue] 
       WHERE  [Guid] = 'C6AA76B5-3E7F-4E14-905E-36173E60949D') 
    DECLARE @ScheduleTimesValueId INT = (SELECT [Id] 
       FROM   [DefinedValue] 
       WHERE  [Guid] = '18B24BF8-26DD-43BB-A54D-8B10C57EA740') 
    DECLARE @FullNameValueId INT = (SELECT [Id] 
       FROM   [DefinedValue] 
       WHERE  [Guid] = '0CDCE2CD-DA27-4BB8-A799-DB780674B00E') 
    DECLARE @LocationsValueId INT = (SELECT [Id] 
       FROM   [DefinedValue] 
       WHERE  [Guid] = '13E9155C-B153-454B-B3B4-6687767C9400') 
    DECLARE @BirthdayDayofWeekValueId INT = (SELECT [Id] 
       FROM   [DefinedValue] 
       WHERE  [Guid] = '604CB370-636B-43D2-9674-B7CE8A09EAFA') 
    DECLARE @LegalIconValueId INT = (SELECT [Id] 
       FROM   [DefinedValue] 
       WHERE  [Guid] = 'BD31D356-0B9F-453C-A9DD-E066DB2DAB3C') 
    DECLARE @LegalNoteValueId INT = (SELECT [Id] 
       FROM   [DefinedValue] 
       WHERE  [Guid] = '94CC0022-D324-4492-81A3-30E1BEB1C85A') 
    DECLARE @AllergyIconValueId INT = (SELECT [Id] 
       FROM   [DefinedValue] 
       WHERE  [Guid] = 'CD131071-28B1-440D-AFC6-AAA998B59BF9') 
    DECLARE @AllergyNoteValueId INT = (SELECT [Id] 
       FROM   [DefinedValue] 
       WHERE  [Guid] = 'A1E7ECBD-6248-42BA-8844-2378EA2B9F0E') 
    DECLARE @FirsttimeIconValueId INT = (SELECT [Id] 
       FROM   [DefinedValue] 
       WHERE  [Guid] = 'BD1B1FEA-D4A9-45EB-9958-01EF75D5A949') 

-- BinaryFile label attributes 
INSERT INTO [AttributeValue] 
	    ([IsSystem] 
	    ,[AttributeId] 
	    ,[EntityId] 
	    ,[Order] 
	    ,[Value] 
	    ,[Guid]) 
    VALUES
	    (1 
	    ,399 
	    ,@ChildLabelIconFileId 
	    ,0
        ,N'WWW^' + CAST(@SecurityCodeValueId AS VARCHAR)
        + '|5^' + CAST(@NickNameValueId AS VARCHAR)
        + '|6^' + CAST(@LastNameValueId AS VARCHAR)
        + '|2^' + CAST(@BirthdayIconValueId AS VARCHAR)
        + '|3^' + CAST(@FirsttimeIconValueId AS VARCHAR)
        + '|4^' + CAST(@BirthdayDayofWeekValueId AS VARCHAR)
        + '|10^' + CAST(@ScheduleTimesValueId AS VARCHAR)
        + '|9^' + CAST(@LocationsValueId AS VARCHAR)
        + '|7^' + CAST(@AllergyIconValueId AS VARCHAR)
        + '|8^' + CAST(@LegalIconValueId AS VARCHAR)
        + '|' 
	    ,'DACC00A3-B65D-4557-9EC9-22BC1C1ED946') 

    DECLARE @AttributeId int = ( SELECT [Id] FROM [Attribute] WHERE [Guid] = 'C4204D6E-715E-4E3A-BA1B-949D20D26487' )

    UPDATE [AttributeValue]
    SET [Value] = '~/checkin/welcome'
    WHERE [AttributeId] = @AttributeId
    AND [Value] = 'welcome'

    UPDATE [AttributeValue]
    SET [Value] = '~/attendedcheckin/search'
    WHERE [AttributeId] = @AttributeId
    AND [Value] = 'search'

    DECLARE @PageId int = (SELECT [Id] FROM [Page] WHERE [Guid] = 'A4DCE339-9C11-40CA-9A02-D2FE64EA164B')
    UPDATE [PageRoute] SET [PageId] = @PageId WHERE [Route] = 'ManageCheckin'
" );
            // Attrib for BlockType: Locations:Area Select Page
            RockMigrationHelper.AddBlockTypeAttribute( "00FC1DEA-FE34-41E3-BC0A-2EE9138091EC", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Area Select Page", "AreaSelectPage", "", "The page to redirect user to if area has not be configured or selected.", 3, @"", "FD0CCA8C-D9B7-45AF-BF12-23C9C7E82F54" );
            // Attrib Value for Block:Check-in Manager, Attribute:Area Select Page Page: Check-in Manager, Site: Rock Check-in Manager
            RockMigrationHelper.AddBlockAttributeValue( "D38C2DA2-4F76-4BA5-9B26-ADA39D98DEDC", "FD0CCA8C-D9B7-45AF-BF12-23C9C7E82F54", @"62c70118-0a6f-432a-9d84-a5296655cb9e" );

        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            Sql( @"
    DELETE FROM [BinaryFile] where [Guid] = 'C2E6B0A0-3991-4FAF-9E2F-49CF321CBB0D'
    DELETE FROM [AttributeValue] where [Guid] = 'DACC00A3-B65D-4557-9EC9-22BC1C1ED946'

    DECLARE @AttributeId int = ( SELECT [Id] FROM [Attribute] WHERE [Guid] = 'C4204D6E-715E-4E3A-BA1B-949D20D26487' )

    UPDATE [AttributeValue]
    SET [Value] = 'welcome'
    WHERE [AttributeId] = @AttributeId
    AND [Value] = '~/checkin/welcome'

    UPDATE [AttributeValue]
    SET [Value] = 'search'
    WHERE [AttributeId] = @AttributeId
    AND [Value] = '~/attendedcheckin/search'

    DECLARE @PageId int = (SELECT [Id] FROM [Page] WHERE [Guid] = '62C70118-0A6F-432A-9D84-A5296655CB9E')
    UPDATE [PageRoute] SET [PageId] = @PageId WHERE [Route] = 'ManageCheckin'
" );

            // Attrib for BlockType: Locations:Area Select Page
            RockMigrationHelper.DeleteAttribute( "FD0CCA8C-D9B7-45AF-BF12-23C9C7E82F54" );
        }
    }
}
