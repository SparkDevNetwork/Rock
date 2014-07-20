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
    public partial class ChildLabelFontAwesome : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            Down();
            
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
" );
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            Sql( "DELETE FROM [BinaryFile] where [Guid] = 'C2E6B0A0-3991-4FAF-9E2F-49CF321CBB0D'" );
        }
    }
}
