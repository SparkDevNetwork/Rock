-- Rename a workflow setting attribute
UPDATE [Attribute] SET
    [Name] = 'Use Same Options',
    [Key] = 'core_checkin_UseSameOptions'
WHERE [Guid] = 'EC7FA927-95D0-44A8-8AB3-2D74A9FA2F26'

-- Fix the ordering of the Person Search workflow activity
DECLARE @ActivityTypeId int = ( SELECT TOP 1 [Id] FROM [WorkflowActivityType] WHERE [Guid] = 'EB744DF1-E454-482C-B111-80A54EF8A674' )

DECLARE @EntityTypeId int = ( SELECT TOP 1 [Id] FROM [EntityType] WHERE [Name] = 'Rock.Workflow.Action.CheckIn.LoadLocations' )
DECLARE @Order int = ISNULL( ( SELECT TOP 1 [Order] FROM [WorkflowActionType] WHERE [ActivityTypeId] = @ActivityTypeId AND [EntityTypeId] = @EntityTypeId ), 0)
UPDATE [WorkflowActionType] SET [Order] = [Order] + 1 WHERE [ActivityTypeId] = @ActivityTypeId AND [Order] > @Order
UPDATE [WorkflowActionType] SET [Order] = @Order + 1 WHERE [Guid] = '6A4E09F0-7AAF-441A-AAD7-BFEA7AF08A6A'

SET @EntityTypeId = ( SELECT TOP 1 [Id] FROM [EntityType] WHERE [Name] = 'Rock.Workflow.Action.CheckIn.RemoveEmptyPeople' )
SET @Order = ISNULL( ( SELECT TOP 1 [Order] FROM [WorkflowActionType] WHERE [ActivityTypeId] = @ActivityTypeId AND [EntityTypeId] = @EntityTypeId ), 0)
UPDATE [WorkflowActionType] SET [Order] = [Order] + 1 WHERE [ActivityTypeId] = @ActivityTypeId AND [Order] > @Order
UPDATE [WorkflowActionType] SET [Order] = @Order + 1 WHERE [Guid] = '79CB608D-ED25-4526-A0F5-132D13642CDA'

SET @EntityTypeId = ( SELECT TOP 1 [Id] FROM [EntityType] WHERE [Name] = 'Rock.Workflow.Action.CheckIn.CalculateLastAttended' )
SET @Order = ISNULL( ( SELECT TOP 1 [Order] FROM [WorkflowActionType] WHERE [ActivityTypeId] = @ActivityTypeId AND [EntityTypeId] = @EntityTypeId ), 0)
UPDATE [WorkflowActionType] SET [Order] = [Order] + 1 WHERE [ActivityTypeId] = @ActivityTypeId AND [Order] > @Order
UPDATE [WorkflowActionType] SET [Order] = @Order + 1 WHERE [Guid] = '08D15C7A-4421-420A-BCA8-D6EE532E659F'

-- Fix the ordering of the Ability Level Search workflow activity
UPDATE [WorkflowActionType]
SET [Order] = ISNULL(
	(
		SELECT MAX(A.[Order]) + 1
		FROM [WorkflowActivityType] T 
		INNER JOIN [WorkflowActionType] A ON A.[ActivityTypeId] = T.[Id]
		WHERE T.[Guid] = '0E2F5EBA-2204-4C2F-845A-92C25AB67474'
		AND A.[Guid] <> '902931D2-6326-4A6A-967C-C9F65F8C1386'
	),0)
WHERE [Guid] = '902931D2-6326-4A6A-967C-C9F65F8C1386'

-- Update Check-in Labels
DECLARE @BinaryFileTypeId int = ( SELECT TOP 1 [Id] FROM [BinaryFileType] WHERE [Guid] = 'DE0E5C50-234B-474C-940C-C571F385E65F' )
DECLARE @StorageEntityTypeId int = ( SELECT TOP 1 [StorageEntityTypeId] FROM [BinaryFileType] WHERE [Id] = @BinaryFileTypeId )
DECLARE @MergeCodesAttributeId int = ( SELECT TOP 1 [Id] FROM [Attribute] WHERE [Guid] = 'CE57450F-634A-420A-BF5A-B43E9B20ABF2' )
DECLARE @LabelTypeAttributeId int = ( SELECT TOP 1 [Id] FROM [Attribute] WHERE [Guid] = '733944B7-A0D5-41B4-94D4-DE007F72B6F0' )

DECLARE @SecurityCodeValueId INT = (SELECT TOP 1 [Id] FROM [DefinedValue] WHERE [Guid] = '9DCB69C4-1B9B-4D31-B8B6-8C9DBFA9933B') 
DECLARE @NickNameValueId INT = (SELECT TOP 1 [Id] FROM [DefinedValue] WHERE [Guid] = 'E2DB40F4-A064-429E-9A76-C520E7E7A43A') 
DECLARE @LastNameValueId INT = (SELECT TOP 1 [Id] FROM [DefinedValue] WHERE [Guid] = '85B71255-19F9-4443-A7AE-EF670385DC71') 
DECLARE @BirthdayIconValueId INT = (SELECT TOP 1 [Id] FROM [DefinedValue] WHERE [Guid] = 'C6AA76B5-3E7F-4E14-905E-36173E60949D') 
DECLARE @ScheduleTimesValueId INT = (SELECT TOP 1 [Id] FROM [DefinedValue] WHERE [Guid] = '18B24BF8-26DD-43BB-A54D-8B10C57EA740') 
DECLARE @FullNameValueId INT = (SELECT TOP 1 [Id] FROM [DefinedValue] WHERE [Guid] = '0CDCE2CD-DA27-4BB8-A799-DB780674B00E') 
DECLARE @LocationsValueId INT = (SELECT TOP 1 [Id] FROM [DefinedValue] WHERE [Guid] = '13E9155C-B153-454B-B3B4-6687767C9400') 
DECLARE @BirthdayDayofWeekValueId INT = (SELECT TOP 1 [Id] FROM [DefinedValue] WHERE [Guid] = '604CB370-636B-43D2-9674-B7CE8A09EAFA') 
DECLARE @LegalIconValueId INT = (SELECT TOP 1 [Id] FROM [DefinedValue] WHERE [Guid] = 'BD31D356-0B9F-453C-A9DD-E066DB2DAB3C') 
DECLARE @LegalNoteValueId INT = (SELECT TOP 1 [Id] FROM [DefinedValue] WHERE [Guid] = '94CC0022-D324-4492-81A3-30E1BEB1C85A') 
DECLARE @AllergyIconValueId INT = (SELECT TOP 1 [Id] FROM [DefinedValue] WHERE [Guid] = 'CD131071-28B1-440D-AFC6-AAA998B59BF9') 
DECLARE @AllergyNoteValueId INT = (SELECT TOP 1 [Id] FROM [DefinedValue] WHERE [Guid] = 'A1E7ECBD-6248-42BA-8844-2378EA2B9F0E') 
DECLARE @FirsttimeIconValueId INT = (SELECT TOP 1 [Id] FROM [DefinedValue] WHERE [Guid] = 'BD1B1FEA-D4A9-45EB-9958-01EF75D5A949') 
DECLARE @CurrentDayDateValueId INT = (SELECT TOP 1 [Id] FROM [DefinedValue] WHERE [Guid] = '23502286-D921-4455-BABC-D8D6CB8FFB3D')
DECLARE @CodeAgesOddValueId INT = (SELECT TOP 1 [Id] FROM [DefinedValue] WHERE [Guid] = '170207B6-9218-4E6E-8ADA-661521E80E5E')
DECLARE @CodeAgesEvenValueId INT = (SELECT TOP 1 [Id] FROM [DefinedValue] WHERE [Guid] = '5B11A934-0398-429F-9A91-F727153392E7')
DECLARE @NameCodeOddValueId INT = (SELECT TOP 1 [Id] FROM [DefinedValue] WHERE [Guid] = '1FAA4DAC-5240-486E-A23F-2A47D7F36F31')
DECLARE @NameCodeEvenValueId INT = (SELECT TOP 1 [Id] FROM [DefinedValue] WHERE [Guid] = '3DCF76E8-866C-4EC9-B1FB-552691A8B440')
DECLARE @PersonLocTimesValueId INT = (SELECT TOP 1 [Id] FROM [DefinedValue] WHERE [Guid] = '08882D9E-4D49-4D1E-94D2-7E5CF64A570D')
DECLARE @LocTimesValueId INT = (SELECT TOP 1 [Id] FROM [DefinedValue] WHERE [Guid] = 'B407E2A5-A7DC-4C29-9B41-B88C99838BD1')

UPDATE [Attribute] SET [EntityTypeQualifierValue] = CAST( @BinaryFileTypeId AS VARCHAR ) WHERE [Guid] = '733944B7-A0D5-41B4-94D4-DE007F72B6F0'

DECLARE @LabelFileId int

-- Parent Label
SET @LabelFileId = ( SELECT TOP 1 [Id] FROM [BinaryFile] WHERE [Guid] = '9B098DB0-952C-43FB-A5BD-511E3C2B72FB' )

UPDATE [BinaryFileData] 
SET [Content] = 0xEFBBBF1043547E7E43442C7E43435E7E43547E0D0A5E58417E54413030307E4A534E5E4C54305E4D4E575E4D54445E504F4E5E504D4E5E4C48302C305E4A4D415E5052362C367E534431355E4A55535E4C524E5E4349305E585A0D0A5E58410D0A5E4D4D540D0A5E50573831320D0A5E4C4C303430360D0A5E4C53300D0A5E465432302C38305E41304E2C37332C37325E46423830302C302C302C4C5E46485C5E46444368696C64205069636B757020526563656970745E46530D0A5E46423230302C372C302C4C5E465433302C3339305E41304E2C33392C33385E4644315E46530D0A5E46423230302C372C302C4C5E46543431352C3339305E41304E2C33392C33385E4644325E46530D0A5E465431342C3336395E41304E2C32332C32345E46485C5E4644466F722074686520736166657479206F6620796F7572206368696C642C20796F75206D7573742070726573656E7420746869732072656365697074207768656E207069636B696E675E46530D0A5E465431352C3339335E41304E2C32332C32345E46485C5E4644757020796F7572206368696C642E20496620796F75206C6F7365207468697320706C6561736520736565207468652061726561206469726563746F722E5E46530D0A5E4C52595E464F302C305E47423831322C302C3130305E46535E4C524E0D0A5E5051312C302C312C595E585A
WHERE [Id] = @LabelFileId

UPDATE [AttributeValue] 
SET [Value] = N'1^' + CAST(@NameCodeOddValueId AS VARCHAR)
	+ '|2^' + CAST(@NameCodeEvenValueId AS VARCHAR)
	+ '|'
WHERE [AttributeId] = @MergeCodesAttributeId
AND [EntityId] = @LabelFileId

INSERT INTO [AttributeValue] 
	    ([IsSystem] 
	    ,[AttributeId] 
	    ,[EntityId] 
	    ,[Value] 
	    ,[Guid]) 
    VALUES
	    (0 
	    ,@LabelTypeAttributeId
	    ,@LabelFileId 
		,N'0'
		,'8F6B7705-90A0-4954-95D9-3A54B44785CC' )

-- Child Label (Text)
SET @LabelFileId = ( SELECT TOP 1 [Id] FROM [BinaryFile] WHERE [Guid] = '1B31A2A3-6438-4557-8788-7F057A025EAA' )

UPDATE [BinaryFileData] 
SET [Content] =	0xEFBBBF1043547E7E43442C7E43435E7E43547E0D0A5E58417E54413030307E4A534E5E4C54305E4D4E575E4D54445E504F4E5E504D4E5E4C48302C305E4A4D415E5052362C367E534431355E4A55535E4C524E5E4349305E585A0D0A5E58410D0A5E4D4D540D0A5E50573831320D0A5E4C4C303430360D0A5E4C53300D0A5E46543434332C3131395E41304E2C3133352C3133345E46423333332C312C302C525E46485C5E46445757575E46530D0A5E4654352C3234385E41304E2C3133352C3134365E46485C5E4644355E46530D0A5E4654342C3330335E41304E2C34352C34355E46485C5E4644365E46530D0A5E464F3632362C3334305E474237382C35362C35365E46530D0A5E46543632362C3338345E41304E2C34352C34355E464237382C312C302C435E46525E46485C5E46444141415E46530D0A5E464F3731392C3334305E474236362C35362C35365E46530D0A5E46543731392C3338345E41304E2C34352C34355E464236362C312C302C435E46525E46485C5E46444C4C4C5E46530D0A5E46543333362C3130335E41304E2C3130322C3130305E46485C5E4644325E46530D0A5E46543430312C3130335E41304E2C3130322C3130305E46485C5E4644335E46530D0A5E46543334322C3133305E41304E2C32382C32385E46485C5E4644345E46530D0A5E46423333302C322C302C4C5E4654382C3338325E41304E2C32382C32385E46485C5E4644395E46530D0A5E4C52595E464F302C305E47423831322C302C3133365E46535E4C524E0D0A5E5051312C302C312C595E585A0D0A
WHERE [Id] = @LabelFileId

UPDATE [AttributeValue] 
SET [Value] = N'WWW^' + CAST(@SecurityCodeValueId AS VARCHAR)
		+ '|5^' + CAST(@NickNameValueId AS VARCHAR)
		+ '|6^' + CAST(@LastNameValueId AS VARCHAR)
		+ '|AAA^' + CAST(@AllergyIconValueId AS VARCHAR)
		+ '|LLL^' + CAST(@LegalIconValueId AS VARCHAR)
		+ '|2^' + CAST(@BirthdayIconValueId AS VARCHAR)
		+ '|3^' + CAST(@FirsttimeIconValueId AS VARCHAR)
		+ '|4^' + CAST(@BirthdayDayofWeekValueId AS VARCHAR)
		+ '|9^' + CAST(@PersonLocTimesValueId AS VARCHAR)
		+ '|'
WHERE [AttributeId] = @MergeCodesAttributeId
AND [EntityId] = @LabelFileId

INSERT INTO [AttributeValue] 
	    ([IsSystem] 
	    ,[AttributeId] 
	    ,[EntityId] 
	    ,[Value] 
	    ,[Guid]) 
    VALUES
	    (0 
	    ,@LabelTypeAttributeId
	    ,@LabelFileId 
		,N'1'
		,'FCB20744-4E04-42DD-8CE2-88503636A49D' )

-- Child Label (Icon)
SET @LabelFileId = ( SELECT TOP 1 [Id] FROM [BinaryFile] WHERE [Guid] = 'C2E6B0A0-3991-4FAF-9E2F-49CF321CBB0D' )

UPDATE [BinaryFileData] 
SET [Content] =	0x1043547E7E43442C7E43435E7E43547E0A5E58417E54413030307E4A534E5E4C54305E4D4E575E4D54445E504F4E5E504D4E5E4C48302C305E4A4D415E5052362C367E534432345E4A55535E4C524E5E4349305E585A0A5E58410A5E4D4D540A5E50573831320A5E4C4C303430360A5E4C53300A5E46543435322C3131395E41304E2C3133352C3133345E46423333332C312C302C525E46485C5E46445757575E46530A5E465431322C3235345E41304E2C3133352C3134365E46485C5E4644355E46530A5E465431342C3330395E41304E2C34352C34355E46485C5E4644365E46530A5E43575A2C453A524F433030302E464E545E46543239332C38325E415A4E2C37332C36340A5E46485C5E4644425E46530A5E43575A2C453A524F433030302E464E545E46543337382C38315E415A4E2C37332C36340A5E46485C5E4644465E46530A5E46543239392C3132305E41304E2C32382C32385E46485C5E4644345E46530A5E46423333302C322C302C4C5E4654382C3338325E41304E2C32382C32385E46485C5E4644395E46530A5E43575A2C453A524F433030302E464E545E46543630352C3338335E415A4E2C37332C36345E46485C5E4644375E46530A5E43575A2C453A524F433030302E464E545E46543731352C3338365E415A4E2C37332C36345E46485C5E4644385E46530A5E4C52595E464F302C305E47423831322C302C3133365E46535E4C524E0A5E5051312C302C312C595E585A0A
WHERE [Id] = @LabelFileId

UPDATE [AttributeValue] 
SET [Value] = N'WWW^' + CAST(@SecurityCodeValueId AS VARCHAR)
		+ '|5^' + CAST(@NickNameValueId AS VARCHAR)
		+ '|6^' + CAST(@LastNameValueId AS VARCHAR)
		+ '|B^' + CAST(@BirthdayIconValueId AS VARCHAR)
		+ '|F^' + CAST(@FirsttimeIconValueId AS VARCHAR)
		+ '|4^' + CAST(@BirthdayDayofWeekValueId AS VARCHAR)
		+ '|9^' + CAST(@PersonLocTimesValueId AS VARCHAR)
		+ '|7^' + CAST(@AllergyIconValueId AS VARCHAR)
		+ '|8^' + CAST(@LegalIconValueId AS VARCHAR)
		+ '|'
WHERE [AttributeId] = @MergeCodesAttributeId
AND [EntityId] = @LabelFileId

INSERT INTO [AttributeValue] 
	    ([IsSystem] 
	    ,[AttributeId] 
	    ,[EntityId] 
	    ,[Value] 
	    ,[Guid]) 
    VALUES
	    (0 
	    ,@LabelTypeAttributeId
	    ,@LabelFileId 
		,N'1'
		,'404E7922-F0CE-4889-A509-A46A07C9D731' )

-- Note Label
SET @LabelFileId = ( SELECT TOP 1 [Id] FROM [BinaryFile] WHERE [Guid] = '50E743E2-76C1-428F-8072-E9BC157D0A08' )

UPDATE [BinaryFileData] 
SET [Content] =	0xEFBBBF1043547E7E43442C7E43435E7E43547E0D0A5E58417E54413030307E4A534E5E4C54305E4D4E575E4D54445E504F4E5E504D4E5E4C48302C305E4A4D415E5052362C367E534431355E4A55535E4C524E5E4349305E585A0D0A5E58410D0A5E4D4D540D0A5E50573831320D0A5E4C4C303430360D0A5E4C53300D0A5E46543630372C36385E41304E2C37332C37325E46423137372C312C302C525E46485C5E46445757575E46530D0A5E4654362C3132325E41304E2C33392C33385E46485C5E4644325E46530D0A5E464F382C3136315E474235372C34322C34325E46530D0A5E4654382C3139345E41304E2C33342C33335E464235372C312C302C435E46525E46485C5E46444141415E46530D0A5E46423333302C332C302C4C5E46543432372C3136385E41304E2C32352C32345E46485C5E4644335E46530D0A5E464F31322C3236345E474234382C34322C34325E46530D0A5E465431322C3239375E41304E2C33342C33335E464234382C312C302C435E46525E46485C5E46444C4C4C5E46530D0A5E46423333302C342C302C4C5E465436382C3235305E41304E2C32332C32345E46485C5E4644355E46530D0A5E46423333302C342C302C4C5E465436382C3335345E41304E2C32332C32345E46485C5E4644375E46530D0A5E46543432312C3231325E41304E2C32332C32345E46485C5E46444E6F7465733A5E46530D0A5E464F3430332C3135345E4742302C3233372C315E46530D0A5E464F3432322C3338365E47423336312C302C315E46530D0A5E464F3432332C3334355E47423336312C302C315E46530D0A5E464F3432312C3330345E47423336312C302C315E46530D0A5E464F3432312C3236335E47423336312C302C315E46530D0A5E4C52595E464F302C305E47423831322C302C38315E46535E4C524E0D0A5E5051312C302C312C595E585A0D0A
WHERE [Id] = @LabelFileId

UPDATE [AttributeValue] 
SET [Value] = N'WWW^' + CAST(@SecurityCodeValueId AS VARCHAR)
		+ '|2^' + CAST(@FullNameValueId AS VARCHAR)
		+ '|AAA^' + CAST(@AllergyIconValueId AS VARCHAR)
		+ '|3^' + CAST(@LocTimesValueId AS VARCHAR)
		+ '|LLL^' + CAST(@LegalIconValueId AS VARCHAR)
		+ '|5^' + CAST(@AllergyNoteValueId AS VARCHAR)
		+ '|7^' + CAST(@LegalNoteValueId AS VARCHAR)
		+ '|'
WHERE [AttributeId] = @MergeCodesAttributeId
AND [EntityId] = @LabelFileId

INSERT INTO [AttributeValue] 
	    ([IsSystem] 
	    ,[AttributeId] 
	    ,[EntityId] 
	    ,[Value] 
	    ,[Guid]) 
    VALUES
	    (0 
	    ,@LabelTypeAttributeId
	    ,@LabelFileId 
		,N'2'
		,'BE825EE8-C148-475B-9A5F-BD828237F60D' )

-- Set correct defaults for Check-in Type and Phone Search Type attributes
UPDATE [Attribute] SET [DefaultValue] = '1' WHERE GUID IN ( '90C34D24-7CFB-4A52-B39C-DFF05A40997C', '34D0971A-53AB-4D43-94EA-E251081D7F93' )

-- Update new installs to default to Family check-in
DECLARE @AttendanceRecords int = ( SELECT COUNT(*) FROM [Attendance] )
IF @AttendanceRecords IS NULL OR @AttendanceRecords <= 0
BEGIN

	DECLARE @GroupTypeId int = ( SELECT TOP 1 [Id] FROM [GroupType] WHERE [Guid] = 'FEDD389A-616F-4A53-906C-63D8255631C5' )

	DECLARE @AttributeId int = ( SELECT TOP 1 [Id] FROM [Attribute] WHERE [Guid] = '90C34D24-7CFB-4A52-B39C-DFF05A40997C' )
	UPDATE [AttributeValue] SET [Value] = '1' WHERE [AttributeId] = @AttributeId AND [EntityId] = @GroupTypeId 

	SET @AttributeId = ( SELECT TOP 1 [Id] FROM [Attribute] WHERE [Guid] = 'EC7FA927-95D0-44A8-8AB3-2D74A9FA2F26' )
	UPDATE [AttributeValue] SET [Value] = 'True' WHERE [AttributeId] = @AttributeId AND [EntityId] = @GroupTypeId 

END

-- Update the mergefield help text for labels
UPDATE [DefinedType]
SET [helptext] = 
'
Label merge fields are defined with a liquid syntax. Click the ''Show Merge Fields'' button below to view the available merge fields.

<p>
    <a data-toggle="collapse"  href="#collapseMergeFields" class=''btn btn-action btn-xs''>Show/Hide Merge Fields</a>
</p>

<div id="collapseMergeFields" class="panel-collapse collapse">

    <div class=''alert alert-info lava-debug''>
        <div class=''panel panel-default panel-lavadebug''><div class=''panel-heading clearfix collapsed'' data-toggle=''collapse'' data-target=''#collapse-8ba173c6-7c85-47e6-9cdf-4d747c050d14''><h5 class=''panel-title pull-left''>Global Attribute</h5> <div class=''pull-right''><i class=''fa fa-chevron-up''></i></div></div><div id=''collapse-8ba173c6-7c85-47e6-9cdf-4d747c050d14'' class=''panel-collapse collapse''><div class=''panel-body''><p>Global attributes should be accessed using <code>{{ ''Global'' | Attribute:''[AttributeKey]'' }}</code>. Find out more about using Global Attributes in Lava at <a href=''http://www.rockrms.com/lava/globalattributes'' target=''_blank''>rockrms.com/lava/globalattributes</a>.</p>
        <ul>
            <li><span class=''lava-debug-key''>ContentFiletypeBlacklist</span> <span class=''lava-debug-value''> - ascx, ashx, aspx, ascx.cs, ashx.cs, aspx.cs,...</span></li>
            <li><span class=''lava-debug-key''>ContentImageFiletypeWhitelist</span> <span class=''lava-debug-value''> - jpg,png,gif,bmp,svg</span></li>
            <li><span class=''lava-debug-key''>core.GradeLabel</span> <span class=''lava-debug-value''> - Grade</span></li>
            <li><span class=''lava-debug-key''>core.LavaSupportLevel</span> <span class=''lava-debug-value''> - Legacy</span></li>
            <li><span class=''lava-debug-key''>core.ValidUsernameCaption</span> <span class=''lava-debug-value''> - It must only contain letters, numbers, +, -,...</span></li>
            <li><span class=''lava-debug-key''>core.ValidUsernameRegularExpression</span> <span class=''lava-debug-value''> - ^[A-Za-z0-9+.@_-]{3,128}$</span></li>
            <li><span class=''lava-debug-key''>CurrencySymbol</span> <span class=''lava-debug-value''> - $</span></li>
            <li><span class=''lava-debug-key''>EmailExceptionsFilter</span> <span class=''lava-debug-value''> - </span></li>
            <li><span class=''lava-debug-key''>EmailExceptionsList</span> <span class=''lava-debug-value''> - </span></li>
            <li>
                <span class=''lava-debug-key''>EmailFooter</span> <span class=''lava-debug-value''>
                    -
                    ...
                </span>
            </li>
            <li>
                <span class=''lava-debug-key''>EmailHeader</span> <span class=''lava-debug-value''>
                    -
                    &lt;!DOCTYPE HTML PUBLIC &quot;-//W3C//DTD XHTML 1.0...
                </span>
            </li>
            <li><span class=''lava-debug-key''>EmailHeaderLogo</span> <span class=''lava-debug-value''> - assets/images/email-header.jpg</span></li>
            <li><span class=''lava-debug-key''>EnableAuditing</span> <span class=''lava-debug-value''> - No</span></li>
            <li><span class=''lava-debug-key''>GoogleAPIKey</span> <span class=''lava-debug-value''> - </span></li>
            <li><span class=''lava-debug-key''>GradeTransitionDate</span> <span class=''lava-debug-value''> - 6/1</span></li>
            <li><span class=''lava-debug-key''>InternalApplicationRoot</span> <span class=''lava-debug-value''> - http://rock.organization.com/</span></li>
            <li><span class=''lava-debug-key''>JobPulse</span> <span class=''lava-debug-value''> - 7/13/2012 4:58:30 PM</span></li>
            <li><span class=''lava-debug-key''>Log404AsException</span> <span class=''lava-debug-value''> - No</span></li>
            <li><span class=''lava-debug-key''>OrganizationAbbreviation</span> <span class=''lava-debug-value''> - </span></li>
            <li>
                <span class=''lava-debug-key''>OrganizationAddress</span> <span class=''lava-debug-value''>
                    - 3120 W Cholla St
                    Phoenix, AZ 85029
                </span>
            </li>
            <li><span class=''lava-debug-key''>OrganizationEmail</span> <span class=''lava-debug-value''> - info@organizationname.com</span></li>
            <li><span class=''lava-debug-key''>OrganizationName</span> <span class=''lava-debug-value''> - Rock Solid Church</span></li>
            <li><span class=''lava-debug-key''>OrganizationPhone</span> <span class=''lava-debug-value''> - </span></li>
            <li><span class=''lava-debug-key''>OrganizationWebsite</span> <span class=''lava-debug-value''> - www.organization.com</span></li>
            <li><span class=''lava-debug-key''>PasswordRegexFriendlyDescription</span> <span class=''lava-debug-value''> - Invalid Password. Password must be at least 6...</span></li>
            <li><span class=''lava-debug-key''>PasswordRegularExpression</span> <span class=''lava-debug-value''> - \\w{6,255}</span></li>
            <li><span class=''lava-debug-key''>PreferredEmailLinkType</span> <span class=''lava-debug-value''> - New Communication</span></li>
            <li><span class=''lava-debug-key''>PublicApplicationRoot</span> <span class=''lava-debug-value''> - http://www.organization.com/</span></li>
            <li><span class=''lava-debug-key''>SupportInternationalAddresses</span> <span class=''lava-debug-value''> - No</span></li>
            <li><span class=''lava-debug-key''>UpdateServerUrl</span> <span class=''lava-debug-value''> - http://update.rockrms.com/F/rock/api/v2/</span></li>
        </ul>
    </div></div></div><div class=''panel panel-default panel-lavadebug''>
        <div class=''panel-heading clearfix collapsed'' data-toggle=''collapse'' data-target=''#collapse-20fb5df4-c3ec-4ea3-a842-655a30fbee40''><h5 class=''panel-title pull-left''>Page Parameter</h5> <div class=''pull-right''><i class=''fa fa-chevron-up''></i></div></div><div id=''collapse-20fb5df4-c3ec-4ea3-a842-655a30fbee40'' class=''panel-collapse collapse''>
            <div class=''panel-body''>
                <p>PageParameter properties can be accessed by <code>{{ PageParameter.[PropertyKey] }}</code>.</p>
                <ul>
                    <li><span class=''lava-debug-key''>PageId</span> <span class=''lava-debug-value''> - 447</span></li>
                </ul>
            </div>
        </div>
    </div><div class=''panel panel-default panel-lavadebug''>
        <div class=''panel-heading clearfix collapsed'' data-toggle=''collapse'' data-target=''#collapse-7bebe6b0-493a-4d96-8ea7-a721a4c5a6da''><h5 class=''panel-title pull-left''>Current Person</h5> <div class=''pull-right''><i class=''fa fa-chevron-up''></i></div></div><div id=''collapse-7bebe6b0-493a-4d96-8ea7-a721a4c5a6da'' class=''panel-collapse collapse''>
            <div class=''panel-body''>
                <p>CurrentPerson properties can be accessed by <code>{{ CurrentPerson.[PropertyKey] }}</code>. Find out more about using ''Person'' fields in Lava at <a href=''http://www.rockrms.com/lava/person'' target=''_blank''>rockrms.com/lava/person</a>.</p>
                <ul>
                    <li><span class=''lava-debug-key''>IsSystem</span> <span class=''lava-debug-value''> - True</span></li>
                    <li><span class=''lava-debug-key''>RecordTypeValueId</span> <span class=''lava-debug-value''> - 1</span></li>
                    <li><span class=''lava-debug-key''>RecordStatusValueId</span> <span class=''lava-debug-value''> - 3</span></li>
                    <li><span class=''lava-debug-key''>RecordStatusLastModifiedDateTime</span> <span class=''lava-debug-value''> - </span></li>
                    <li><span class=''lava-debug-key''>RecordStatusReasonValueId</span> <span class=''lava-debug-value''> - </span></li>
                    <li><span class=''lava-debug-key''>ConnectionStatusValueId</span> <span class=''lava-debug-value''> - 66</span></li>
                    <li><span class=''lava-debug-key''>ReviewReasonValueId</span> <span class=''lava-debug-value''> - </span></li>
                    <li><span class=''lava-debug-key''>IsDeceased</span> <span class=''lava-debug-value''> - False</span></li>
                    <li><span class=''lava-debug-key''>TitleValueId</span> <span class=''lava-debug-value''> - </span></li>
                    <li><span class=''lava-debug-key''>FirstName</span> <span class=''lava-debug-value''> - Admin</span></li>
                    <li><span class=''lava-debug-key''>NickName</span> <span class=''lava-debug-value''> - Admin</span></li>
                    <li><span class=''lava-debug-key''>MiddleName</span> <span class=''lava-debug-value''> - </span></li>
                    <li><span class=''lava-debug-key''>LastName</span> <span class=''lava-debug-value''> - Admin</span></li>
                    <li><span class=''lava-debug-key''>SuffixValueId</span> <span class=''lava-debug-value''> - </span></li>
                    <li><span class=''lava-debug-key''>PhotoId</span> <span class=''lava-debug-value''> - </span></li>
                    <li><span class=''lava-debug-key''>BirthDay</span> <span class=''lava-debug-value''> - </span></li>
                    <li><span class=''lava-debug-key''>BirthMonth</span> <span class=''lava-debug-value''> - </span></li>
                    <li><span class=''lava-debug-key''>BirthYear</span> <span class=''lava-debug-value''> - </span></li>
                    <li><span class=''lava-debug-key''>Gender</span> <span class=''lava-debug-value''> - Unknown</span></li>
                    <li><span class=''lava-debug-key''>MaritalStatusValueId</span> <span class=''lava-debug-value''> - </span></li>
                    <li><span class=''lava-debug-key''>AnniversaryDate</span> <span class=''lava-debug-value''> - </span></li>
                    <li><span class=''lava-debug-key''>GraduationYear</span> <span class=''lava-debug-value''> - </span></li>
                    <li><span class=''lava-debug-key''>GivingGroupId</span> <span class=''lava-debug-value''> - </span></li>
                    <li><span class=''lava-debug-key''>GivingId</span> <span class=''lava-debug-value''> - P1</span></li>
                    <li><span class=''lava-debug-key''>GivingLeaderId</span> <span class=''lava-debug-value''> - 1</span></li>
                    <li><span class=''lava-debug-key''>Email</span> <span class=''lava-debug-value''> - admin@organization.com</span></li>
                    <li><span class=''lava-debug-key''>IsEmailActive</span> <span class=''lava-debug-value''> - False</span></li>
                    <li><span class=''lava-debug-key''>EmailNote</span> <span class=''lava-debug-value''> - </span></li>
                    <li><span class=''lava-debug-key''>EmailPreference</span> <span class=''lava-debug-value''> - EmailAllowed</span></li>
                    <li><span class=''lava-debug-key''>ReviewReasonNote</span> <span class=''lava-debug-value''> - </span></li>
                    <li><span class=''lava-debug-key''>InactiveReasonNote</span> <span class=''lava-debug-value''> - </span></li>
                    <li><span class=''lava-debug-key''>SystemNote</span> <span class=''lava-debug-value''> - </span></li>
                    <li><span class=''lava-debug-key''>ViewedCount</span> <span class=''lava-debug-value''> - </span></li>
                    <li>
                        <span class=''lava-debug-key lava-debug-section level-1''>PrimaryAlias</span>
                        <ul>
                            <li><span class=''lava-debug-key''>Name</span> <span class=''lava-debug-value''> - </span></li>
                            <li><span class=''lava-debug-key''>PersonId</span> <span class=''lava-debug-value''> - 1</span></li>
                            <li><span class=''lava-debug-key''>AliasPersonId</span> <span class=''lava-debug-value''> - 1</span></li>
                            <li><span class=''lava-debug-key''>AliasPersonGuid</span> <span class=''lava-debug-value''> - ad28da19-4af1-408f-9090-2672f8376f27</span></li>
                            <li><span class=''lava-debug-key''>Person</span> <span class=''lava-debug-value''> - </span></li>
                            <li><span class=''lava-debug-key''>Id</span> <span class=''lava-debug-value''> - 10</span></li>
                            <li><span class=''lava-debug-key''>Guid</span> <span class=''lava-debug-value''> - 93c3bdc3-3318-48f3-8a6c-327d0b10f4f5</span></li>
                            <li><span class=''lava-debug-key''>UrlEncodedKey</span> <span class=''lava-debug-value''> - EAAAACw5J!2fMPWJmtPFNtPxP!2fB91GAPCIKyFKkho07Za...</span></li>
                        </ul>
                    </li>
                    <li><span class=''lava-debug-key''>PrimaryAliasId</span> <span class=''lava-debug-value''> - 10</span></li>
                    <li><span class=''lava-debug-key''>FullName</span> <span class=''lava-debug-value''> - Admin Admin</span></li>
                    <li><span class=''lava-debug-key''>BirthdayDayOfWeek</span> <span class=''lava-debug-value''> - </span></li>
                    <li><span class=''lava-debug-key''>BirthdayDayOfWeekShort</span> <span class=''lava-debug-value''> - </span></li>
                    <li><span class=''lava-debug-key''>PhotoUrl</span> <span class=''lava-debug-value''> - /Assets/Images/person-no-photo-male.svg?</span></li>
                    <li>
                        <span class=''lava-debug-key lava-debug-section level-1''>Users</span>
                        {<ul>
                            <li>
                                [0]
                                <ul>
                                    <li><span class=''lava-debug-key''>EntityTypeId</span> <span class=''lava-debug-value''> - 27</span></li>
                                    <li><span class=''lava-debug-key''>UserName</span> <span class=''lava-debug-value''> - admin</span></li>
                                    <li><span class=''lava-debug-key''>IsConfirmed</span> <span class=''lava-debug-value''> - True</span></li>
                                    <li><span class=''lava-debug-key''>LastActivityDateTime</span> <span class=''lava-debug-value''> - 5/24/2016 5:27:37 AM</span></li>
                                    <li><span class=''lava-debug-key''>LastLoginDateTime</span> <span class=''lava-debug-value''> - 5/24/2016 5:23:45 AM</span></li>
                                    <li><span class=''lava-debug-key''>LastPasswordChangedDateTime</span> <span class=''lava-debug-value''> - 1/23/2012 3:43:25 AM</span></li>
                                    <li><span class=''lava-debug-key''>IsOnLine</span> <span class=''lava-debug-value''> - True</span></li>
                                    <li><span class=''lava-debug-key''>IsLockedOut</span> <span class=''lava-debug-value''> - </span></li>
                                    <li><span class=''lava-debug-key''>IsPasswordChangeRequired</span> <span class=''lava-debug-value''> - </span></li>
                                    <li><span class=''lava-debug-key''>LastLockedOutDateTime</span> <span class=''lava-debug-value''> - 12/15/2011 2:45:54 AM</span></li>
                                    <li><span class=''lava-debug-key''>FailedPasswordAttemptCount</span> <span class=''lava-debug-value''> - 1</span></li>
                                    <li><span class=''lava-debug-key''>FailedPasswordAttemptWindowStartDateTime</span> <span class=''lava-debug-value''> - 6/7/2012 3:25:06 PM</span></li>
                                    <li><span class=''lava-debug-key''>LastPasswordExpirationWarningDateTime</span> <span class=''lava-debug-value''> - </span></li>
                                    <li><span class=''lava-debug-key''>ApiKey</span> <span class=''lava-debug-value''> - </span></li>
                                    <li><span class=''lava-debug-key''>PersonId</span> <span class=''lava-debug-value''> - 1</span></li>
                                    <li><span class=''lava-debug-key''>ConfirmationCode</span> <span class=''lava-debug-value''> - EAAAAA8WH7prJrJh0VVWNVjvssHBjfS9szcDz0wmL15mRS2...</span></li>
                                    <li><span class=''lava-debug-key''>ConfirmationCodeEncoded</span> <span class=''lava-debug-value''> - EAAAADe6EidQRyVANNhSHPbWfcrm4FLHuvaUMJeRqEQ8BLm...</span></li>
                                    <li><span class=''lava-debug-key''>CreatedDateTime</span> <span class=''lava-debug-value''> - 3/19/2011 7:34:15 AM</span></li>
                                    <li><span class=''lava-debug-key''>ModifiedDateTime</span> <span class=''lava-debug-value''> - 5/24/2016 5:27:40 AM</span></li>
                                    <li><span class=''lava-debug-key''>CreatedByPersonAliasId</span> <span class=''lava-debug-value''> - </span></li>
                                    <li><span class=''lava-debug-key''>ModifiedByPersonAliasId</span> <span class=''lava-debug-value''> - </span></li>
                                    <li><span class=''lava-debug-key''>CreatedByPersonId</span> <span class=''lava-debug-value''> - </span></li>
                                    <li><span class=''lava-debug-key''>CreatedByPersonName</span> <span class=''lava-debug-value''> - </span></li>
                                    <li><span class=''lava-debug-key''>ModifiedByPersonId</span> <span class=''lava-debug-value''> - </span></li>
                                    <li><span class=''lava-debug-key''>ModifiedByPersonName</span> <span class=''lava-debug-value''> - </span></li>
                                    <li><span class=''lava-debug-key''>ModifiedAuditValuesAlreadyUpdated</span> <span class=''lava-debug-value''> - False</span></li>
                                    <li><span class=''lava-debug-key''>Id</span> <span class=''lava-debug-value''> - 1</span></li>
                                    <li><span class=''lava-debug-key''>Guid</span> <span class=''lava-debug-value''> - 7e10a764-ef6b-431f-87c7-861053c84131</span></li>
                                    <li><span class=''lava-debug-key''>UrlEncodedKey</span> <span class=''lava-debug-value''> - EAAAAIPHvKvnVBr5496TB4!2bgCYtfpTuYqhyR5z5H!2bz0...</span></li>
                                </ul>
                            </li>
                        </ul>}
                    </li>
                    <li>
                        <span class=''lava-debug-key lava-debug-section level-1''>PhoneNumbers</span>
                        {<ul></ul>}
                    </li>
                    <li><span class=''lava-debug-key''>MaritalStatusValue</span> <span class=''lava-debug-value''> - </span></li>
                    <li>
                        <span class=''lava-debug-key lava-debug-section level-1''>ConnectionStatusValue</span>
                        <ul>
                            <li><span class=''lava-debug-key''>IsSystem</span> <span class=''lava-debug-value''> - False</span></li>
                            <li><span class=''lava-debug-key''>DefinedTypeId</span> <span class=''lava-debug-value''> - 4</span></li>
                            <li><span class=''lava-debug-key''>Order</span> <span class=''lava-debug-value''> - 2</span></li>
                            <li><span class=''lava-debug-key''>Value</span> <span class=''lava-debug-value''> - Visitor</span></li>
                            <li><span class=''lava-debug-key''>Description</span> <span class=''lava-debug-value''> - Used when a person first enters through your...</span></li>
                            <li><span class=''lava-debug-key''>CreatedDateTime</span> <span class=''lava-debug-value''> - </span></li>
                            <li><span class=''lava-debug-key''>ModifiedDateTime</span> <span class=''lava-debug-value''> - </span></li>
                            <li><span class=''lava-debug-key''>CreatedByPersonAliasId</span> <span class=''lava-debug-value''> - </span></li>
                            <li><span class=''lava-debug-key''>ModifiedByPersonAliasId</span> <span class=''lava-debug-value''> - </span></li>
                            <li><span class=''lava-debug-key''>CreatedByPersonId</span> <span class=''lava-debug-value''> - </span></li>
                            <li><span class=''lava-debug-key''>CreatedByPersonName</span> <span class=''lava-debug-value''> - </span></li>
                            <li><span class=''lava-debug-key''>ModifiedByPersonId</span> <span class=''lava-debug-value''> - </span></li>
                            <li><span class=''lava-debug-key''>ModifiedByPersonName</span> <span class=''lava-debug-value''> - </span></li>
                            <li><span class=''lava-debug-key''>ModifiedAuditValuesAlreadyUpdated</span> <span class=''lava-debug-value''> - False</span></li>
                            <li><span class=''lava-debug-key''>Id</span> <span class=''lava-debug-value''> - 66</span></li>
                            <li><span class=''lava-debug-key''>Guid</span> <span class=''lava-debug-value''> - b91ba046-bc1e-400c-b85d-638c1f4e0ce2</span></li>
                            <li><span class=''lava-debug-key''>UrlEncodedKey</span> <span class=''lava-debug-value''> - EAAAADG!2fS!2bIo1kulf51q3HQadEMGQcMP4zIsLpPn2y!...</span></li>
                            <li>
                                <span class=''lava-debug-key lava-debug-section level-2''>Attributes <p class=''attributes''>Below is a list of attributes that can be retrieved using <code>{{ CurrentPerson.ConnectionStatusValue | Attribute:''[AttributeKey]'' }}</code>.</p></span>
                                <ul>
                                    <li><span class=''lava-debug-key''>Color</span> <span class=''lava-debug-value''> - #afd074</span></li>
                                </ul>
                            </li>
                        </ul>
                    </li>
                    <li><span class=''lava-debug-key''>ReviewReasonValue</span> <span class=''lava-debug-value''> - </span></li>
                    <li>
                        <span class=''lava-debug-key lava-debug-section level-1''>RecordStatusValue</span>
                        <ul>
                            <li><span class=''lava-debug-key''>IsSystem</span> <span class=''lava-debug-value''> - True</span></li>
                            <li><span class=''lava-debug-key''>DefinedTypeId</span> <span class=''lava-debug-value''> - 2</span></li>
                            <li><span class=''lava-debug-key''>Order</span> <span class=''lava-debug-value''> - 0</span></li>
                            <li><span class=''lava-debug-key''>Value</span> <span class=''lava-debug-value''> - Active</span></li>
                            <li><span class=''lava-debug-key''>Description</span> <span class=''lava-debug-value''> - Denotes an individual that is actively...</span></li>
                            <li><span class=''lava-debug-key''>CreatedDateTime</span> <span class=''lava-debug-value''> - </span></li>
                            <li><span class=''lava-debug-key''>ModifiedDateTime</span> <span class=''lava-debug-value''> - </span></li>
                            <li><span class=''lava-debug-key''>CreatedByPersonAliasId</span> <span class=''lava-debug-value''> - </span></li>
                            <li><span class=''lava-debug-key''>ModifiedByPersonAliasId</span> <span class=''lava-debug-value''> - </span></li>
                            <li><span class=''lava-debug-key''>CreatedByPersonId</span> <span class=''lava-debug-value''> - </span></li>
                            <li><span class=''lava-debug-key''>CreatedByPersonName</span> <span class=''lava-debug-value''> - </span></li>
                            <li><span class=''lava-debug-key''>ModifiedByPersonId</span> <span class=''lava-debug-value''> - </span></li>
                            <li><span class=''lava-debug-key''>ModifiedByPersonName</span> <span class=''lava-debug-value''> - </span></li>
                            <li><span class=''lava-debug-key''>ModifiedAuditValuesAlreadyUpdated</span> <span class=''lava-debug-value''> - False</span></li>
                            <li><span class=''lava-debug-key''>Id</span> <span class=''lava-debug-value''> - 3</span></li>
                            <li><span class=''lava-debug-key''>Guid</span> <span class=''lava-debug-value''> - 618f906c-c33d-4fa3-8aef-e58cb7b63f1e</span></li>
                            <li><span class=''lava-debug-key''>UrlEncodedKey</span> <span class=''lava-debug-value''> - EAAAAItfQizA3mcKyJC!2bB!2f2YlJaOfBG2LMCizX8O6KO...</span></li>
                        </ul>
                    </li>
                    <li><span class=''lava-debug-key''>RecordStatusReasonValue</span> <span class=''lava-debug-value''> - </span></li>
                    <li>
                        <span class=''lava-debug-key lava-debug-section level-1''>RecordTypeValue</span>
                        <ul>
                            <li><span class=''lava-debug-key''>IsSystem</span> <span class=''lava-debug-value''> - True</span></li>
                            <li><span class=''lava-debug-key''>DefinedTypeId</span> <span class=''lava-debug-value''> - 1</span></li>
                            <li><span class=''lava-debug-key''>Order</span> <span class=''lava-debug-value''> - 0</span></li>
                            <li><span class=''lava-debug-key''>Value</span> <span class=''lava-debug-value''> - Person</span></li>
                            <li><span class=''lava-debug-key''>Description</span> <span class=''lava-debug-value''> - A Person Record</span></li>
                            <li><span class=''lava-debug-key''>CreatedDateTime</span> <span class=''lava-debug-value''> - </span></li>
                            <li><span class=''lava-debug-key''>ModifiedDateTime</span> <span class=''lava-debug-value''> - </span></li>
                            <li><span class=''lava-debug-key''>CreatedByPersonAliasId</span> <span class=''lava-debug-value''> - </span></li>
                            <li><span class=''lava-debug-key''>ModifiedByPersonAliasId</span> <span class=''lava-debug-value''> - </span></li>
                            <li><span class=''lava-debug-key''>CreatedByPersonId</span> <span class=''lava-debug-value''> - </span></li>
                            <li><span class=''lava-debug-key''>CreatedByPersonName</span> <span class=''lava-debug-value''> - </span></li>
                            <li><span class=''lava-debug-key''>ModifiedByPersonId</span> <span class=''lava-debug-value''> - </span></li>
                            <li><span class=''lava-debug-key''>ModifiedByPersonName</span> <span class=''lava-debug-value''> - </span></li>
                            <li><span class=''lava-debug-key''>ModifiedAuditValuesAlreadyUpdated</span> <span class=''lava-debug-value''> - False</span></li>
                            <li><span class=''lava-debug-key''>Id</span> <span class=''lava-debug-value''> - 1</span></li>
                            <li><span class=''lava-debug-key''>Guid</span> <span class=''lava-debug-value''> - 36cf10d6-c695-413d-8e7c-4546efef385e</span></li>
                            <li><span class=''lava-debug-key''>UrlEncodedKey</span> <span class=''lava-debug-value''> - EAAAAMMbKe6lWUpR59cAX4KU!2f9!2bJyy!2buG4crphOwj...</span></li>
                        </ul>
                    </li>
                    <li><span class=''lava-debug-key''>SuffixValue</span> <span class=''lava-debug-value''> - </span></li>
                    <li><span class=''lava-debug-key''>TitleValue</span> <span class=''lava-debug-value''> - </span></li>
                    <li><span class=''lava-debug-key''>Photo</span> <span class=''lava-debug-value''> - </span></li>
                    <li><span class=''lava-debug-key''>BirthDate</span> <span class=''lava-debug-value''> - </span></li>
                    <li><span class=''lava-debug-key''>DaysUntilBirthday</span> <span class=''lava-debug-value''> - </span></li>
                    <li><span class=''lava-debug-key''>Age</span> <span class=''lava-debug-value''> - </span></li>
                    <li><span class=''lava-debug-key''>NextBirthDay</span> <span class=''lava-debug-value''> - </span></li>
                    <li><span class=''lava-debug-key''>DaysToBirthday</span> <span class=''lava-debug-value''> - 2147483647</span></li>
                    <li><span class=''lava-debug-key''>NextAnniversary</span> <span class=''lava-debug-value''> - </span></li>
                    <li><span class=''lava-debug-key''>GradeOffset</span> <span class=''lava-debug-value''> - </span></li>
                    <li><span class=''lava-debug-key''>HasGraduated</span> <span class=''lava-debug-value''> - </span></li>
                    <li><span class=''lava-debug-key''>GradeFormatted</span> <span class=''lava-debug-value''> - </span></li>
                    <li><span class=''lava-debug-key''>ImpersonationParameter</span> <span class=''lava-debug-value''> - rckipid=EAAAAP7cZijPF3Y0ngFMvJB34A%2bPTXBhA1NAH...</span></li>
                    <li><span class=''lava-debug-key''>CreatedDateTime</span> <span class=''lava-debug-value''> - </span></li>
                    <li><span class=''lava-debug-key''>ModifiedDateTime</span> <span class=''lava-debug-value''> - </span></li>
                    <li><span class=''lava-debug-key''>CreatedByPersonAliasId</span> <span class=''lava-debug-value''> - </span></li>
                    <li><span class=''lava-debug-key''>ModifiedByPersonAliasId</span> <span class=''lava-debug-value''> - </span></li>
                    <li><span class=''lava-debug-key''>CreatedByPersonId</span> <span class=''lava-debug-value''> - </span></li>
                    <li><span class=''lava-debug-key''>CreatedByPersonName</span> <span class=''lava-debug-value''> - </span></li>
                    <li><span class=''lava-debug-key''>ModifiedByPersonId</span> <span class=''lava-debug-value''> - </span></li>
                    <li><span class=''lava-debug-key''>ModifiedByPersonName</span> <span class=''lava-debug-value''> - </span></li>
                    <li><span class=''lava-debug-key''>ModifiedAuditValuesAlreadyUpdated</span> <span class=''lava-debug-value''> - False</span></li>
                    <li><span class=''lava-debug-key''>Id</span> <span class=''lava-debug-value''> - 1</span></li>
                    <li><span class=''lava-debug-key''>Guid</span> <span class=''lava-debug-value''> - ad28da19-4af1-408f-9090-2672f8376f27</span></li>
                    <li><span class=''lava-debug-key''>UrlEncodedKey</span> <span class=''lava-debug-value''> - EAAAAO7h9JfpfR4MVe!2fAOoDjzZk6a2b5jtL6dg8QevCMx...</span></li>
                    <li>
                        <span class=''lava-debug-key lava-debug-section level-1''>Attributes <p class=''attributes''>Below is a list of attributes that can be retrieved using <code>{{ CurrentPerson | Attribute:''[AttributeKey]'' }}</code>.</p></span>
                        <ul>
                            <li><span class=''lava-debug-key''>BaptismDate</span> <span class=''lava-debug-value''> - </span></li>
                            <li><span class=''lava-debug-key''>AbilityLevel</span> <span class=''lava-debug-value''> - </span></li>
                            <li><span class=''lava-debug-key''>Allergy</span> <span class=''lava-debug-value''> - </span></li>
                            <li><span class=''lava-debug-key''>BaptizedHere</span> <span class=''lava-debug-value''> - No</span></li>
                            <li><span class=''lava-debug-key''>LegalNotes</span> <span class=''lava-debug-value''> - </span></li>
                            <li><span class=''lava-debug-key''>PreviousChurch</span> <span class=''lava-debug-value''> - </span></li>
                            <li><span class=''lava-debug-key''>FirstVisit</span> <span class=''lava-debug-value''> - </span></li>
                            <li><span class=''lava-debug-key''>SecondVisit</span> <span class=''lava-debug-value''> - </span></li>
                            <li><span class=''lava-debug-key''>SourceofVisit</span> <span class=''lava-debug-value''> - </span></li>
                            <li><span class=''lava-debug-key''>School</span> <span class=''lava-debug-value''> - </span></li>
                            <li><span class=''lava-debug-key''>Employer</span> <span class=''lava-debug-value''> - </span></li>
                            <li><span class=''lava-debug-key''>Position</span> <span class=''lava-debug-value''> - </span></li>
                            <li><span class=''lava-debug-key''>MembershipDate</span> <span class=''lava-debug-value''> - </span></li>
                            <li><span class=''lava-debug-key''>Facebook</span> <span class=''lava-debug-value''> - </span></li>
                            <li><span class=''lava-debug-key''>Twitter</span> <span class=''lava-debug-value''> - </span></li>
                            <li><span class=''lava-debug-key''>Instagram</span> <span class=''lava-debug-value''> - </span></li>
                            <li><span class=''lava-debug-key''>AdaptiveD</span> <span class=''lava-debug-value''> - </span></li>
                            <li><span class=''lava-debug-key''>AdaptiveI</span> <span class=''lava-debug-value''> - </span></li>
                            <li><span class=''lava-debug-key''>AdaptiveS</span> <span class=''lava-debug-value''> - </span></li>
                            <li><span class=''lava-debug-key''>AdaptiveC</span> <span class=''lava-debug-value''> - </span></li>
                            <li><span class=''lava-debug-key''>NaturalD</span> <span class=''lava-debug-value''> - </span></li>
                            <li><span class=''lava-debug-key''>NaturalI</span> <span class=''lava-debug-value''> - </span></li>
                            <li><span class=''lava-debug-key''>NaturalS</span> <span class=''lava-debug-value''> - </span></li>
                            <li><span class=''lava-debug-key''>NaturalC</span> <span class=''lava-debug-value''> - </span></li>
                            <li><span class=''lava-debug-key''>LastSaveDate</span> <span class=''lava-debug-value''> - </span></li>
                            <li><span class=''lava-debug-key''>BackgroundChecked</span> <span class=''lava-debug-value''> - </span></li>
                            <li><span class=''lava-debug-key''>BackgroundCheckDate</span> <span class=''lava-debug-value''> - </span></li>
                            <li><span class=''lava-debug-key''>BackgroundCheckResult</span> <span class=''lava-debug-value''> - </span></li>
                            <li><span class=''lava-debug-key''>BackgroundCheckDocument</span> <span class=''lava-debug-value''> - </span></li>
                            <li><span class=''lava-debug-key''>PersonalityType</span> <span class=''lava-debug-value''> - </span></li>
                            <li><span class=''lava-debug-key''>com.sparkdevnetwork.DLNumber</span> <span class=''lava-debug-value''> - </span></li>
                            <li><span class=''lava-debug-key''>LastDiscRequestDate</span> <span class=''lava-debug-value''> - </span></li>
                            <li><span class=''lava-debug-key''>core_CurrentlyAnEra</span> <span class=''lava-debug-value''> - </span></li>
                            <li><span class=''lava-debug-key''>core_EraStartDate</span> <span class=''lava-debug-value''> - </span></li>
                            <li><span class=''lava-debug-key''>core_EraEndDate</span> <span class=''lava-debug-value''> - </span></li>
                            <li><span class=''lava-debug-key''>core_EraFirstCheckin</span> <span class=''lava-debug-value''> - </span></li>
                            <li><span class=''lava-debug-key''>core_EraLastCheckin</span> <span class=''lava-debug-value''> - </span></li>
                            <li><span class=''lava-debug-key''>core_EraLastGave</span> <span class=''lava-debug-value''> - </span></li>
                            <li><span class=''lava-debug-key''>core_EraFirstGave</span> <span class=''lava-debug-value''> - </span></li>
                            <li><span class=''lava-debug-key''>core_TimesCheckedIn16Wks</span> <span class=''lava-debug-value''> - </span></li>
                            <li><span class=''lava-debug-key''>core_EraTimesGiven52Wks</span> <span class=''lava-debug-value''> - </span></li>
                            <li><span class=''lava-debug-key''>core_EraTimesGiven6Wks</span> <span class=''lava-debug-value''> - </span></li>
                        </ul>
                    </li>
                </ul>
            </div>
        </div>
    </div><div class=''panel panel-default panel-lavadebug''>
        <div class=''panel-heading clearfix collapsed'' data-toggle=''collapse'' data-target=''#collapse-82d76f0a-c373-4f52-8f76-f8013b76a6a8''><h5 class=''panel-title pull-left''>Campuses</h5> <div class=''pull-right''><i class=''fa fa-chevron-up''></i></div></div><div id=''collapse-82d76f0a-c373-4f52-8f76-f8013b76a6a8'' class=''panel-collapse collapse''>
            <div class=''panel-body''>
                <p>Campuses properties can be accessed by <code>{% for campus in Campuses %}{{ campus.[PropertyKey] }}{% endfor %}</code>.</p>
                {<ul>
                    <li>
                        [0]
                        <ul>
                            <li><span class=''lava-debug-key''>IsSystem</span> <span class=''lava-debug-value''> - False</span></li>
                            <li><span class=''lava-debug-key''>Name</span> <span class=''lava-debug-value''> - Main Campus</span></li>
                            <li><span class=''lava-debug-key''>Description</span> <span class=''lava-debug-value''> - </span></li>
                            <li><span class=''lava-debug-key''>IsActive</span> <span class=''lava-debug-value''> - True</span></li>
                            <li><span class=''lava-debug-key''>ShortCode</span> <span class=''lava-debug-value''> - MAIN</span></li>
                            <li><span class=''lava-debug-key''>Url</span> <span class=''lava-debug-value''> - </span></li>
                            <li><span class=''lava-debug-key''>LocationId</span> <span class=''lava-debug-value''> - 2</span></li>
                            <li>
                                <span class=''lava-debug-key lava-debug-section level-1''>Location</span>
                                <ul>
                                    <li><span class=''lava-debug-key''>Street1</span> <span class=''lava-debug-value''> - </span></li>
                                    <li><span class=''lava-debug-key''>Street2</span> <span class=''lava-debug-value''> - </span></li>
                                    <li><span class=''lava-debug-key''>City</span> <span class=''lava-debug-value''> - </span></li>
                                    <li><span class=''lava-debug-key''>State</span> <span class=''lava-debug-value''> - </span></li>
                                    <li><span class=''lava-debug-key''>PostalCode</span> <span class=''lava-debug-value''> - </span></li>
                                    <li><span class=''lava-debug-key''>Country</span> <span class=''lava-debug-value''> - </span></li>
                                    <li><span class=''lava-debug-key''>Latitude</span> <span class=''lava-debug-value''> - </span></li>
                                    <li><span class=''lava-debug-key''>Longitude</span> <span class=''lava-debug-value''> - </span></li>
                                    <li><span class=''lava-debug-key''>ImageUrl</span> <span class=''lava-debug-value''> - </span></li>
                                </ul>
                            </li>
                            <li><span class=''lava-debug-key''>PhoneNumber</span> <span class=''lava-debug-value''> - </span></li>
                            <li><span class=''lava-debug-key''>LeaderPersonAliasId</span> <span class=''lava-debug-value''> - </span></li>
                            <li><span class=''lava-debug-key''>RawServiceTimes</span> <span class=''lava-debug-value''> - </span></li>
                            <li>
                                <span class=''lava-debug-key lava-debug-section level-1''>ServiceTimes</span>
                                {<ul></ul>}
                            </li>
                            <li><span class=''lava-debug-key''>Id</span> <span class=''lava-debug-value''> - 1</span></li>
                            <li><span class=''lava-debug-key''>Guid</span> <span class=''lava-debug-value''> - 76882ae3-1ce8-42a6-a2b6-8c0b29cf8cf8</span></li>
                            <li><span class=''lava-debug-key''>ForeignId</span> <span class=''lava-debug-value''> - </span></li>
                            <li><span class=''lava-debug-key''>ForeignGuid</span> <span class=''lava-debug-value''> - </span></li>
                            <li><span class=''lava-debug-key''>ForeignKey</span> <span class=''lava-debug-value''> - </span></li>
                        </ul>
                    </li>
                </ul>}
            </div>
        </div>
    </div><div class=''panel panel-default panel-lavadebug''>
        <div class=''panel-heading clearfix collapsed'' data-toggle=''collapse'' data-target=''#collapse-2b289b0d-a81f-4728-8397-9f76af96cd48''><h5 class=''panel-title pull-left''>Location</h5> <div class=''pull-right''><i class=''fa fa-chevron-up''></i></div></div><div id=''collapse-2b289b0d-a81f-4728-8397-9f76af96cd48'' class=''panel-collapse collapse''>
            <div class=''panel-body''>
                <p>Location properties can be accessed by <code>{{ Location.[PropertyKey] }}</code>.</p>
                <ul>
                    <li><span class=''lava-debug-key''>LastCheckIn</span> <span class=''lava-debug-value''> - </span></li>
                    <li><span class=''lava-debug-key''>Locations</span> <span class=''lava-debug-value''> - </span></li>
                    <li><span class=''lava-debug-key''>ParentLocationId</span> <span class=''lava-debug-value''> - 3</span></li>
                    <li><span class=''lava-debug-key''>Name</span> <span class=''lava-debug-value''> - Bobcats Room</span></li>
                    <li><span class=''lava-debug-key''>IsActive</span> <span class=''lava-debug-value''> - True</span></li>
                    <li><span class=''lava-debug-key''>LocationTypeValueId</span> <span class=''lava-debug-value''> - </span></li>
                    <li><span class=''lava-debug-key''>GeoPoint</span> <span class=''lava-debug-value''> - </span></li>
                    <li><span class=''lava-debug-key''>GeoFence</span> <span class=''lava-debug-value''> - </span></li>
                    <li><span class=''lava-debug-key''>Street1</span> <span class=''lava-debug-value''> - </span></li>
                    <li><span class=''lava-debug-key''>Street2</span> <span class=''lava-debug-value''> - </span></li>
                    <li><span class=''lava-debug-key''>City</span> <span class=''lava-debug-value''> - </span></li>
                    <li><span class=''lava-debug-key''>County</span> <span class=''lava-debug-value''> - </span></li>
                    <li><span class=''lava-debug-key''>State</span> <span class=''lava-debug-value''> - </span></li>
                    <li><span class=''lava-debug-key''>Country</span> <span class=''lava-debug-value''> - </span></li>
                    <li><span class=''lava-debug-key''>PostalCode</span> <span class=''lava-debug-value''> - </span></li>
                    <li><span class=''lava-debug-key''>Barcode</span> <span class=''lava-debug-value''> - </span></li>
                    <li><span class=''lava-debug-key''>AssessorParcelId</span> <span class=''lava-debug-value''> - </span></li>
                    <li><span class=''lava-debug-key''>StandardizeAttemptedDateTime</span> <span class=''lava-debug-value''> - </span></li>
                    <li><span class=''lava-debug-key''>StandardizeAttemptedServiceType</span> <span class=''lava-debug-value''> - </span></li>
                    <li><span class=''lava-debug-key''>StandardizeAttemptedResult</span> <span class=''lava-debug-value''> - </span></li>
                    <li><span class=''lava-debug-key''>StandardizedDateTime</span> <span class=''lava-debug-value''> - </span></li>
                    <li><span class=''lava-debug-key''>GeocodeAttemptedDateTime</span> <span class=''lava-debug-value''> - </span></li>
                    <li><span class=''lava-debug-key''>GeocodeAttemptedServiceType</span> <span class=''lava-debug-value''> - </span></li>
                    <li><span class=''lava-debug-key''>GeocodeAttemptedResult</span> <span class=''lava-debug-value''> - </span></li>
                    <li><span class=''lava-debug-key''>GeocodedDateTime</span> <span class=''lava-debug-value''> - </span></li>
                    <li><span class=''lava-debug-key''>IsGeoPointLocked</span> <span class=''lava-debug-value''> - </span></li>
                    <li><span class=''lava-debug-key''>PrinterDeviceId</span> <span class=''lava-debug-value''> - </span></li>
                    <li><span class=''lava-debug-key''>ImageId</span> <span class=''lava-debug-value''> - </span></li>
                    <li><span class=''lava-debug-key''>SoftRoomThreshold</span> <span class=''lava-debug-value''> - </span></li>
                    <li><span class=''lava-debug-key''>FirmRoomThreshold</span> <span class=''lava-debug-value''> - </span></li>
                    <li><span class=''lava-debug-key''>IsNamedLocation</span> <span class=''lava-debug-value''> - True</span></li>
                    <li><span class=''lava-debug-key''>LocationTypeValue</span> <span class=''lava-debug-value''> - </span></li>
                    <li>
                        <span class=''lava-debug-key lava-debug-section level-1''>ChildLocations</span>
                        {<ul></ul>}
                    </li>
                    <li><span class=''lava-debug-key''>PrinterDevice</span> <span class=''lava-debug-value''> - </span></li>
                    <li><span class=''lava-debug-key''>Image</span> <span class=''lava-debug-value''> - </span></li>
                    <li><span class=''lava-debug-key''>FormattedAddress</span> <span class=''lava-debug-value''> - </span></li>
                    <li><span class=''lava-debug-key''>FormattedHtmlAddress</span> <span class=''lava-debug-value''> - </span></li>
                    <li><span class=''lava-debug-key''>Latitude</span> <span class=''lava-debug-value''> - </span></li>
                    <li><span class=''lava-debug-key''>Longitude</span> <span class=''lava-debug-value''> - </span></li>
                    <li><span class=''lava-debug-key''>GeoFenceCoordinates</span> <span class=''lava-debug-value''> - </span></li>
                    <li><span class=''lava-debug-key''>CampusId</span> <span class=''lava-debug-value''> - </span></li>
                    <li><span class=''lava-debug-key''>Distance</span> <span class=''lava-debug-value''> - 0</span></li>
                    <li><span class=''lava-debug-key''>CreatedDateTime</span> <span class=''lava-debug-value''> - </span></li>
                    <li><span class=''lava-debug-key''>ModifiedDateTime</span> <span class=''lava-debug-value''> - </span></li>
                    <li><span class=''lava-debug-key''>CreatedByPersonAliasId</span> <span class=''lava-debug-value''> - </span></li>
                    <li><span class=''lava-debug-key''>ModifiedByPersonAliasId</span> <span class=''lava-debug-value''> - </span></li>
                    <li><span class=''lava-debug-key''>CreatedByPersonId</span> <span class=''lava-debug-value''> - </span></li>
                    <li><span class=''lava-debug-key''>CreatedByPersonName</span> <span class=''lava-debug-value''> - </span></li>
                    <li><span class=''lava-debug-key''>ModifiedByPersonId</span> <span class=''lava-debug-value''> - </span></li>
                    <li><span class=''lava-debug-key''>ModifiedByPersonName</span> <span class=''lava-debug-value''> - </span></li>
                    <li><span class=''lava-debug-key''>ModifiedAuditValuesAlreadyUpdated</span> <span class=''lava-debug-value''> - False</span></li>
                    <li><span class=''lava-debug-key''>Id</span> <span class=''lava-debug-value''> - 8</span></li>
                    <li><span class=''lava-debug-key''>Guid</span> <span class=''lava-debug-value''> - e847e31e-aba2-415b-a0e1-770ff1b64425</span></li>
                    <li><span class=''lava-debug-key''>UrlEncodedKey</span> <span class=''lava-debug-value''> - EAAAAFCOaWly!2fD!2fJkxt0eupgrgr!2fXcd7XEIPq!2f4...</span></li>
                </ul>
            </div>
        </div>
    </div><div class=''panel panel-default panel-lavadebug''>
        <div class=''panel-heading clearfix collapsed'' data-toggle=''collapse'' data-target=''#collapse-486b3c69-5b6b-45e8-ac80-70ce4dfa29af''><h5 class=''panel-title pull-left''>Group</h5> <div class=''pull-right''><i class=''fa fa-chevron-up''></i></div></div><div id=''collapse-486b3c69-5b6b-45e8-ac80-70ce4dfa29af'' class=''panel-collapse collapse''>
            <div class=''panel-body''>
                <p>Group properties can be accessed by <code>{{ Group.[PropertyKey] }}</code>.</p>
                <ul>
                    <li><span class=''lava-debug-key''>LastCheckIn</span> <span class=''lava-debug-value''> - </span></li>
                    <li>
                        <span class=''lava-debug-key lava-debug-section level-1''>Locations</span>
                        {<ul>
                            <li>
                                [0]
                                <ul>
                                    <li><span class=''lava-debug-key''>LastCheckIn</span> <span class=''lava-debug-value''> - </span></li>
                                    <li><span class=''lava-debug-key''>Locations</span> <span class=''lava-debug-value''> - </span></li>
                                    <li><span class=''lava-debug-key''>ParentLocationId</span> <span class=''lava-debug-value''> - 3</span></li>
                                    <li><span class=''lava-debug-key''>Name</span> <span class=''lava-debug-value''> - Bobcats Room</span></li>
                                    <li><span class=''lava-debug-key''>IsActive</span> <span class=''lava-debug-value''> - True</span></li>
                                    <li><span class=''lava-debug-key''>LocationTypeValueId</span> <span class=''lava-debug-value''> - </span></li>
                                    <li><span class=''lava-debug-key''>GeoPoint</span> <span class=''lava-debug-value''> - </span></li>
                                    <li><span class=''lava-debug-key''>GeoFence</span> <span class=''lava-debug-value''> - </span></li>
                                    <li><span class=''lava-debug-key''>Street1</span> <span class=''lava-debug-value''> - </span></li>
                                    <li><span class=''lava-debug-key''>Street2</span> <span class=''lava-debug-value''> - </span></li>
                                    <li><span class=''lava-debug-key''>City</span> <span class=''lava-debug-value''> - </span></li>
                                    <li><span class=''lava-debug-key''>County</span> <span class=''lava-debug-value''> - </span></li>
                                    <li><span class=''lava-debug-key''>State</span> <span class=''lava-debug-value''> - </span></li>
                                    <li><span class=''lava-debug-key''>Country</span> <span class=''lava-debug-value''> - </span></li>
                                    <li><span class=''lava-debug-key''>PostalCode</span> <span class=''lava-debug-value''> - </span></li>
                                    <li><span class=''lava-debug-key''>Barcode</span> <span class=''lava-debug-value''> - </span></li>
                                    <li><span class=''lava-debug-key''>AssessorParcelId</span> <span class=''lava-debug-value''> - </span></li>
                                    <li><span class=''lava-debug-key''>StandardizeAttemptedDateTime</span> <span class=''lava-debug-value''> - </span></li>
                                    <li><span class=''lava-debug-key''>StandardizeAttemptedServiceType</span> <span class=''lava-debug-value''> - </span></li>
                                    <li><span class=''lava-debug-key''>StandardizeAttemptedResult</span> <span class=''lava-debug-value''> - </span></li>
                                    <li><span class=''lava-debug-key''>StandardizedDateTime</span> <span class=''lava-debug-value''> - </span></li>
                                    <li><span class=''lava-debug-key''>GeocodeAttemptedDateTime</span> <span class=''lava-debug-value''> - </span></li>
                                    <li><span class=''lava-debug-key''>GeocodeAttemptedServiceType</span> <span class=''lava-debug-value''> - </span></li>
                                    <li><span class=''lava-debug-key''>GeocodeAttemptedResult</span> <span class=''lava-debug-value''> - </span></li>
                                    <li><span class=''lava-debug-key''>GeocodedDateTime</span> <span class=''lava-debug-value''> - </span></li>
                                    <li><span class=''lava-debug-key''>IsGeoPointLocked</span> <span class=''lava-debug-value''> - </span></li>
                                    <li><span class=''lava-debug-key''>PrinterDeviceId</span> <span class=''lava-debug-value''> - </span></li>
                                    <li><span class=''lava-debug-key''>ImageId</span> <span class=''lava-debug-value''> - </span></li>
                                    <li><span class=''lava-debug-key''>SoftRoomThreshold</span> <span class=''lava-debug-value''> - </span></li>
                                    <li><span class=''lava-debug-key''>FirmRoomThreshold</span> <span class=''lava-debug-value''> - </span></li>
                                    <li><span class=''lava-debug-key''>IsNamedLocation</span> <span class=''lava-debug-value''> - True</span></li>
                                    <li><span class=''lava-debug-key''>LocationTypeValue</span> <span class=''lava-debug-value''> - </span></li>
                                    <li>
                                        <span class=''lava-debug-key lava-debug-section level-1''>ChildLocations</span>
                                        {<ul></ul>}
                                    </li>
                                    <li><span class=''lava-debug-key''>PrinterDevice</span> <span class=''lava-debug-value''> - </span></li>
                                    <li><span class=''lava-debug-key''>Image</span> <span class=''lava-debug-value''> - </span></li>
                                    <li><span class=''lava-debug-key''>FormattedAddress</span> <span class=''lava-debug-value''> - </span></li>
                                    <li><span class=''lava-debug-key''>FormattedHtmlAddress</span> <span class=''lava-debug-value''> - </span></li>
                                    <li><span class=''lava-debug-key''>Latitude</span> <span class=''lava-debug-value''> - </span></li>
                                    <li><span class=''lava-debug-key''>Longitude</span> <span class=''lava-debug-value''> - </span></li>
                                    <li><span class=''lava-debug-key''>GeoFenceCoordinates</span> <span class=''lava-debug-value''> - </span></li>
                                    <li><span class=''lava-debug-key''>CampusId</span> <span class=''lava-debug-value''> - </span></li>
                                    <li><span class=''lava-debug-key''>Distance</span> <span class=''lava-debug-value''> - 0</span></li>
                                    <li><span class=''lava-debug-key''>CreatedDateTime</span> <span class=''lava-debug-value''> - </span></li>
                                    <li><span class=''lava-debug-key''>ModifiedDateTime</span> <span class=''lava-debug-value''> - </span></li>
                                    <li><span class=''lava-debug-key''>CreatedByPersonAliasId</span> <span class=''lava-debug-value''> - </span></li>
                                    <li><span class=''lava-debug-key''>ModifiedByPersonAliasId</span> <span class=''lava-debug-value''> - </span></li>
                                    <li><span class=''lava-debug-key''>CreatedByPersonId</span> <span class=''lava-debug-value''> - </span></li>
                                    <li><span class=''lava-debug-key''>CreatedByPersonName</span> <span class=''lava-debug-value''> - </span></li>
                                    <li><span class=''lava-debug-key''>ModifiedByPersonId</span> <span class=''lava-debug-value''> - </span></li>
                                    <li><span class=''lava-debug-key''>ModifiedByPersonName</span> <span class=''lava-debug-value''> - </span></li>
                                    <li><span class=''lava-debug-key''>ModifiedAuditValuesAlreadyUpdated</span> <span class=''lava-debug-value''> - False</span></li>
                                    <li><span class=''lava-debug-key''>Id</span> <span class=''lava-debug-value''> - 8</span></li>
                                    <li><span class=''lava-debug-key''>Guid</span> <span class=''lava-debug-value''> - e847e31e-aba2-415b-a0e1-770ff1b64425</span></li>
                                    <li><span class=''lava-debug-key''>UrlEncodedKey</span> <span class=''lava-debug-value''> - EAAAAJnacD02ODH!2fAXnp34N1YFEcFLTswnP1s!2fyTVHG...</span></li>
                                </ul>
                            </li>
                        </ul>}
                    </li>
                    <li><span class=''lava-debug-key''>IsSystem</span> <span class=''lava-debug-value''> - False</span></li>
                    <li><span class=''lava-debug-key''>ParentGroupId</span> <span class=''lava-debug-value''> - </span></li>
                    <li><span class=''lava-debug-key''>GroupTypeId</span> <span class=''lava-debug-value''> - 20</span></li>
                    <li><span class=''lava-debug-key''>CampusId</span> <span class=''lava-debug-value''> - </span></li>
                    <li><span class=''lava-debug-key''>ScheduleId</span> <span class=''lava-debug-value''> - </span></li>
                    <li><span class=''lava-debug-key''>Name</span> <span class=''lava-debug-value''> - Grades 2-3</span></li>
                    <li><span class=''lava-debug-key''>Description</span> <span class=''lava-debug-value''> - </span></li>
                    <li><span class=''lava-debug-key''>IsSecurityRole</span> <span class=''lava-debug-value''> - False</span></li>
                    <li><span class=''lava-debug-key''>IsActive</span> <span class=''lava-debug-value''> - True</span></li>
                    <li><span class=''lava-debug-key''>Order</span> <span class=''lava-debug-value''> - 5</span></li>
                    <li><span class=''lava-debug-key''>AllowGuests</span> <span class=''lava-debug-value''> - </span></li>
                    <li><span class=''lava-debug-key''>WelcomeSystemEmailId</span> <span class=''lava-debug-value''> - </span></li>
                    <li><span class=''lava-debug-key''>ExitSystemEmailId</span> <span class=''lava-debug-value''> - </span></li>
                    <li><span class=''lava-debug-key''>SyncDataViewId</span> <span class=''lava-debug-value''> - </span></li>
                    <li><span class=''lava-debug-key''>AddUserAccountsDuringSync</span> <span class=''lava-debug-value''> - </span></li>
                    <li><span class=''lava-debug-key''>MustMeetRequirementsToAddMember</span> <span class=''lava-debug-value''> - </span></li>
                    <li><span class=''lava-debug-key''>IsPublic</span> <span class=''lava-debug-value''> - True</span></li>
                    <li><span class=''lava-debug-key''>GroupCapacity</span> <span class=''lava-debug-value''> - </span></li>
                    <li><span class=''lava-debug-key''>GroupType</span> <span class=''lava-debug-value''> - </span></li>
                    <li><span class=''lava-debug-key''>Campus</span> <span class=''lava-debug-value''> - </span></li>
                    <li><span class=''lava-debug-key''>Schedule</span> <span class=''lava-debug-value''> - </span></li>
                    <li><span class=''lava-debug-key''>WelcomeSystemEmail</span> <span class=''lava-debug-value''> - </span></li>
                    <li><span class=''lava-debug-key''>ExitSystemEmail</span> <span class=''lava-debug-value''> - </span></li>
                    <li><span class=''lava-debug-key''>SyncDataView</span> <span class=''lava-debug-value''> - </span></li>
                    <li>
                        <span class=''lava-debug-key lava-debug-section level-1''>Groups</span>
                        {<ul></ul>}
                    </li>
                    <li>
                        <span class=''lava-debug-key lava-debug-section level-1''>Members</span>
                        {<ul></ul>}
                    </li>
                    <li>
                        <span class=''lava-debug-key lava-debug-section level-1''>GroupLocations</span>
                        {<ul></ul>}
                    </li>
                    <li>
                        <span class=''lava-debug-key lava-debug-section level-1''>GroupRequirements</span>
                        {<ul></ul>}
                    </li>
                    <li><span class=''lava-debug-key''>CreatedDateTime</span> <span class=''lava-debug-value''> - </span></li>
                    <li><span class=''lava-debug-key''>ModifiedDateTime</span> <span class=''lava-debug-value''> - </span></li>
                    <li><span class=''lava-debug-key''>CreatedByPersonAliasId</span> <span class=''lava-debug-value''> - </span></li>
                    <li><span class=''lava-debug-key''>ModifiedByPersonAliasId</span> <span class=''lava-debug-value''> - </span></li>
                    <li><span class=''lava-debug-key''>CreatedByPersonId</span> <span class=''lava-debug-value''> - </span></li>
                    <li><span class=''lava-debug-key''>CreatedByPersonName</span> <span class=''lava-debug-value''> - </span></li>
                    <li><span class=''lava-debug-key''>ModifiedByPersonId</span> <span class=''lava-debug-value''> - </span></li>
                    <li><span class=''lava-debug-key''>ModifiedByPersonName</span> <span class=''lava-debug-value''> - </span></li>
                    <li><span class=''lava-debug-key''>ModifiedAuditValuesAlreadyUpdated</span> <span class=''lava-debug-value''> - False</span></li>
                    <li><span class=''lava-debug-key''>Id</span> <span class=''lava-debug-value''> - 29</span></li>
                    <li><span class=''lava-debug-key''>Guid</span> <span class=''lava-debug-value''> - 24901861-14cf-474f-9fce-7ba1d6c84bff</span></li>
                    <li><span class=''lava-debug-key''>UrlEncodedKey</span> <span class=''lava-debug-value''> - EAAAAJelKKxqVz!2bfXWWqS6LE0Epa5jZedjcckPPXtBSK!...</span></li>
                </ul>
            </div>
        </div>
    </div><div class=''panel panel-default panel-lavadebug''>
        <div class=''panel-heading clearfix collapsed'' data-toggle=''collapse'' data-target=''#collapse-3a723c8b-61ed-4be0-8aea-1ca2a9da4982''><h5 class=''panel-title pull-left''>Person</h5> <div class=''pull-right''><i class=''fa fa-chevron-up''></i></div></div><div id=''collapse-3a723c8b-61ed-4be0-8aea-1ca2a9da4982'' class=''panel-collapse collapse''>
            <div class=''panel-body''>
                <p>Person properties can be accessed by <code>{{ Person.[PropertyKey] }}</code>.</p>
                <ul>
                    <li><span class=''lava-debug-key''>FamilyMember</span> <span class=''lava-debug-value''> - True</span></li>
                    <li><span class=''lava-debug-key''>LastCheckIn</span> <span class=''lava-debug-value''> - 5/17/2016 10:24:55 AM</span></li>
                    <li><span class=''lava-debug-key''>FirstTime</span> <span class=''lava-debug-value''> - False</span></li>
                    <li><span class=''lava-debug-key''>SecurityCode</span> <span class=''lava-debug-value''> - HFC</span></li>
                    <li><span class=''lava-debug-key''>IsSystem</span> <span class=''lava-debug-value''> - False</span></li>
                    <li><span class=''lava-debug-key''>RecordTypeValueId</span> <span class=''lava-debug-value''> - 1</span></li>
                    <li><span class=''lava-debug-key''>RecordStatusValueId</span> <span class=''lava-debug-value''> - 3</span></li>
                    <li><span class=''lava-debug-key''>RecordStatusLastModifiedDateTime</span> <span class=''lava-debug-value''> - </span></li>
                    <li><span class=''lava-debug-key''>RecordStatusReasonValueId</span> <span class=''lava-debug-value''> - </span></li>
                    <li><span class=''lava-debug-key''>ConnectionStatusValueId</span> <span class=''lava-debug-value''> - 146</span></li>
                    <li><span class=''lava-debug-key''>ReviewReasonValueId</span> <span class=''lava-debug-value''> - </span></li>
                    <li><span class=''lava-debug-key''>IsDeceased</span> <span class=''lava-debug-value''> - False</span></li>
                    <li><span class=''lava-debug-key''>TitleValueId</span> <span class=''lava-debug-value''> - </span></li>
                    <li><span class=''lava-debug-key''>FirstName</span> <span class=''lava-debug-value''> - Alexis</span></li>
                    <li><span class=''lava-debug-key''>NickName</span> <span class=''lava-debug-value''> - Alex</span></li>
                    <li><span class=''lava-debug-key''>MiddleName</span> <span class=''lava-debug-value''> - </span></li>
                    <li><span class=''lava-debug-key''>LastName</span> <span class=''lava-debug-value''> - Decker</span></li>
                    <li><span class=''lava-debug-key''>SuffixValueId</span> <span class=''lava-debug-value''> - </span></li>
                    <li><span class=''lava-debug-key''>PhotoId</span> <span class=''lava-debug-value''> - 48</span></li>
                    <li><span class=''lava-debug-key''>BirthDay</span> <span class=''lava-debug-value''> - 10</span></li>
                    <li><span class=''lava-debug-key''>BirthMonth</span> <span class=''lava-debug-value''> - 2</span></li>
                    <li><span class=''lava-debug-key''>BirthYear</span> <span class=''lava-debug-value''> - 2009</span></li>
                    <li><span class=''lava-debug-key''>Gender</span> <span class=''lava-debug-value''> - Female</span></li>
                    <li><span class=''lava-debug-key''>MaritalStatusValueId</span> <span class=''lava-debug-value''> - 144</span></li>
                    <li><span class=''lava-debug-key''>AnniversaryDate</span> <span class=''lava-debug-value''> - </span></li>
                    <li><span class=''lava-debug-key''>GraduationYear</span> <span class=''lava-debug-value''> - 2026</span></li>
                    <li><span class=''lava-debug-key''>GivingGroupId</span> <span class=''lava-debug-value''> - </span></li>
                    <li><span class=''lava-debug-key''>GivingId</span> <span class=''lava-debug-value''> - P6</span></li>
                    <li><span class=''lava-debug-key''>GivingLeaderId</span> <span class=''lava-debug-value''> - 0</span></li>
                    <li><span class=''lava-debug-key''>Email</span> <span class=''lava-debug-value''> - </span></li>
                    <li><span class=''lava-debug-key''>IsEmailActive</span> <span class=''lava-debug-value''> - True</span></li>
                    <li><span class=''lava-debug-key''>EmailNote</span> <span class=''lava-debug-value''> - </span></li>
                    <li><span class=''lava-debug-key''>EmailPreference</span> <span class=''lava-debug-value''> - EmailAllowed</span></li>
                    <li><span class=''lava-debug-key''>ReviewReasonNote</span> <span class=''lava-debug-value''> - </span></li>
                    <li><span class=''lava-debug-key''>InactiveReasonNote</span> <span class=''lava-debug-value''> - </span></li>
                    <li><span class=''lava-debug-key''>SystemNote</span> <span class=''lava-debug-value''> - </span></li>
                    <li><span class=''lava-debug-key''>ViewedCount</span> <span class=''lava-debug-value''> - </span></li>
                    <li><span class=''lava-debug-key''>PrimaryAlias</span> <span class=''lava-debug-value''> - </span></li>
                    <li><span class=''lava-debug-key''>PrimaryAliasId</span> <span class=''lava-debug-value''> - </span></li>
                    <li><span class=''lava-debug-key''>FullName</span> <span class=''lava-debug-value''> - Alex Decker</span></li>
                    <li><span class=''lava-debug-key''>BirthdayDayOfWeek</span> <span class=''lava-debug-value''> - Wednesday</span></li>
                    <li><span class=''lava-debug-key''>BirthdayDayOfWeekShort</span> <span class=''lava-debug-value''> - Wed</span></li>
                    <li><span class=''lava-debug-key''>PhotoUrl</span> <span class=''lava-debug-value''> - /GetImage.ashx?id=48</span></li>
                    <li>
                        <span class=''lava-debug-key lava-debug-section level-1''>Users</span>
                        {<ul></ul>}
                    </li>
                    <li>
                        <span class=''lava-debug-key lava-debug-section level-1''>PhoneNumbers</span>
                        {<ul></ul>}
                    </li>
                    <li><span class=''lava-debug-key''>MaritalStatusValue</span> <span class=''lava-debug-value''> - </span></li>
                    <li><span class=''lava-debug-key''>ConnectionStatusValue</span> <span class=''lava-debug-value''> - </span></li>
                    <li><span class=''lava-debug-key''>ReviewReasonValue</span> <span class=''lava-debug-value''> - </span></li>
                    <li><span class=''lava-debug-key''>RecordStatusValue</span> <span class=''lava-debug-value''> - </span></li>
                    <li><span class=''lava-debug-key''>RecordStatusReasonValue</span> <span class=''lava-debug-value''> - </span></li>
                    <li><span class=''lava-debug-key''>RecordTypeValue</span> <span class=''lava-debug-value''> - </span></li>
                    <li><span class=''lava-debug-key''>SuffixValue</span> <span class=''lava-debug-value''> - </span></li>
                    <li><span class=''lava-debug-key''>TitleValue</span> <span class=''lava-debug-value''> - </span></li>
                    <li><span class=''lava-debug-key''>Photo</span> <span class=''lava-debug-value''> - </span></li>
                    <li><span class=''lava-debug-key''>BirthDate</span> <span class=''lava-debug-value''> - 2/10/2009 12:00:00 AM</span></li>
                    <li><span class=''lava-debug-key''>DaysUntilBirthday</span> <span class=''lava-debug-value''> - </span></li>
                    <li><span class=''lava-debug-key''>Age</span> <span class=''lava-debug-value''> - 7</span></li>
                    <li><span class=''lava-debug-key''>NextBirthDay</span> <span class=''lava-debug-value''> - 2/10/2017 12:00:00 AM</span></li>
                    <li><span class=''lava-debug-key''>DaysToBirthday</span> <span class=''lava-debug-value''> - 262</span></li>
                    <li><span class=''lava-debug-key''>NextAnniversary</span> <span class=''lava-debug-value''> - </span></li>
                    <li><span class=''lava-debug-key''>GradeOffset</span> <span class=''lava-debug-value''> - 10</span></li>
                    <li><span class=''lava-debug-key''>HasGraduated</span> <span class=''lava-debug-value''> - False</span></li>
                    <li><span class=''lava-debug-key''>GradeFormatted</span> <span class=''lava-debug-value''> - 2nd Grade</span></li>
                    <li><span class=''lava-debug-key''>ImpersonationParameter</span> <span class=''lava-debug-value''> - rckipid=EAAAAHD3FoHwhq73An9lOfPKSi16F4aLILu6KQy...</span></li>
                    <li><span class=''lava-debug-key''>CreatedDateTime</span> <span class=''lava-debug-value''> - 5/23/2016 4:08:34 PM</span></li>
                    <li><span class=''lava-debug-key''>ModifiedDateTime</span> <span class=''lava-debug-value''> - </span></li>
                    <li><span class=''lava-debug-key''>CreatedByPersonAliasId</span> <span class=''lava-debug-value''> - 10</span></li>
                    <li><span class=''lava-debug-key''>ModifiedByPersonAliasId</span> <span class=''lava-debug-value''> - </span></li>
                    <li><span class=''lava-debug-key''>CreatedByPersonId</span> <span class=''lava-debug-value''> - </span></li>
                    <li><span class=''lava-debug-key''>CreatedByPersonName</span> <span class=''lava-debug-value''> - </span></li>
                    <li><span class=''lava-debug-key''>ModifiedByPersonId</span> <span class=''lava-debug-value''> - </span></li>
                    <li><span class=''lava-debug-key''>ModifiedByPersonName</span> <span class=''lava-debug-value''> - </span></li>
                    <li><span class=''lava-debug-key''>ModifiedAuditValuesAlreadyUpdated</span> <span class=''lava-debug-value''> - False</span></li>
                    <li><span class=''lava-debug-key''>Id</span> <span class=''lava-debug-value''> - 6</span></li>
                    <li><span class=''lava-debug-key''>Guid</span> <span class=''lava-debug-value''> - 27919690-3cce-4fa6-95c4-cd21419eb51f</span></li>
                    <li><span class=''lava-debug-key''>UrlEncodedKey</span> <span class=''lava-debug-value''> - EAAAAMo3E0AT6rBh3X7jTXvh83RSSF9rGC3JSYPZuKuoQOU...</span></li>
                </ul>
            </div>
        </div>
    </div><div class=''panel panel-default panel-lavadebug''>
        <div class=''panel-heading clearfix collapsed'' data-toggle=''collapse'' data-target=''#collapse-6c47ed41-952f-47d0-ae23-0a41ebbbf866''><h5 class=''panel-title pull-left''>People</h5> <div class=''pull-right''><i class=''fa fa-chevron-up''></i></div></div><div id=''collapse-6c47ed41-952f-47d0-ae23-0a41ebbbf866'' class=''panel-collapse collapse''>
            <div class=''panel-body''>
                <p>People properties can be accessed by <code>{% for person in People %}{{ person.[PropertyKey] }}{% endfor %}</code>.</p>
                {<ul>
                    <li>
                        [0]
                        <ul>
                            <li><span class=''lava-debug-key''>FamilyMember</span> <span class=''lava-debug-value''> - True</span></li>
                            <li><span class=''lava-debug-key''>LastCheckIn</span> <span class=''lava-debug-value''> - 5/17/2016 10:24:55 AM</span></li>
                            <li><span class=''lava-debug-key''>FirstTime</span> <span class=''lava-debug-value''> - False</span></li>
                            <li><span class=''lava-debug-key''>SecurityCode</span> <span class=''lava-debug-value''> - BNK</span></li>
                            <li><span class=''lava-debug-key''>IsSystem</span> <span class=''lava-debug-value''> - False</span></li>
                            <li><span class=''lava-debug-key''>RecordTypeValueId</span> <span class=''lava-debug-value''> - 1</span></li>
                            <li><span class=''lava-debug-key''>RecordStatusValueId</span> <span class=''lava-debug-value''> - 3</span></li>
                            <li><span class=''lava-debug-key''>RecordStatusLastModifiedDateTime</span> <span class=''lava-debug-value''> - </span></li>
                            <li><span class=''lava-debug-key''>RecordStatusReasonValueId</span> <span class=''lava-debug-value''> - </span></li>
                            <li><span class=''lava-debug-key''>ConnectionStatusValueId</span> <span class=''lava-debug-value''> - 146</span></li>
                            <li><span class=''lava-debug-key''>ReviewReasonValueId</span> <span class=''lava-debug-value''> - </span></li>
                            <li><span class=''lava-debug-key''>IsDeceased</span> <span class=''lava-debug-value''> - False</span></li>
                            <li><span class=''lava-debug-key''>TitleValueId</span> <span class=''lava-debug-value''> - </span></li>
                            <li><span class=''lava-debug-key''>FirstName</span> <span class=''lava-debug-value''> - Noah</span></li>
                            <li><span class=''lava-debug-key''>NickName</span> <span class=''lava-debug-value''> - Noah</span></li>
                            <li><span class=''lava-debug-key''>MiddleName</span> <span class=''lava-debug-value''> - </span></li>
                            <li><span class=''lava-debug-key''>LastName</span> <span class=''lava-debug-value''> - Decker</span></li>
                            <li><span class=''lava-debug-key''>SuffixValueId</span> <span class=''lava-debug-value''> - </span></li>
                            <li><span class=''lava-debug-key''>PhotoId</span> <span class=''lava-debug-value''> - 47</span></li>
                            <li><span class=''lava-debug-key''>BirthDay</span> <span class=''lava-debug-value''> - 10</span></li>
                            <li><span class=''lava-debug-key''>BirthMonth</span> <span class=''lava-debug-value''> - 3</span></li>
                            <li><span class=''lava-debug-key''>BirthYear</span> <span class=''lava-debug-value''> - 2006</span></li>
                            <li><span class=''lava-debug-key''>Gender</span> <span class=''lava-debug-value''> - Male</span></li>
                            <li><span class=''lava-debug-key''>MaritalStatusValueId</span> <span class=''lava-debug-value''> - 144</span></li>
                            <li><span class=''lava-debug-key''>AnniversaryDate</span> <span class=''lava-debug-value''> - </span></li>
                            <li><span class=''lava-debug-key''>GraduationYear</span> <span class=''lava-debug-value''> - 2023</span></li>
                            <li><span class=''lava-debug-key''>GivingGroupId</span> <span class=''lava-debug-value''> - </span></li>
                            <li><span class=''lava-debug-key''>GivingId</span> <span class=''lava-debug-value''> - P5</span></li>
                            <li><span class=''lava-debug-key''>GivingLeaderId</span> <span class=''lava-debug-value''> - 0</span></li>
                            <li><span class=''lava-debug-key''>Email</span> <span class=''lava-debug-value''> - </span></li>
                            <li><span class=''lava-debug-key''>IsEmailActive</span> <span class=''lava-debug-value''> - True</span></li>
                            <li><span class=''lava-debug-key''>EmailNote</span> <span class=''lava-debug-value''> - </span></li>
                            <li><span class=''lava-debug-key''>EmailPreference</span> <span class=''lava-debug-value''> - EmailAllowed</span></li>
                            <li><span class=''lava-debug-key''>ReviewReasonNote</span> <span class=''lava-debug-value''> - </span></li>
                            <li><span class=''lava-debug-key''>InactiveReasonNote</span> <span class=''lava-debug-value''> - </span></li>
                            <li><span class=''lava-debug-key''>SystemNote</span> <span class=''lava-debug-value''> - </span></li>
                            <li><span class=''lava-debug-key''>ViewedCount</span> <span class=''lava-debug-value''> - </span></li>
                            <li><span class=''lava-debug-key''>PrimaryAlias</span> <span class=''lava-debug-value''> - </span></li>
                            <li><span class=''lava-debug-key''>PrimaryAliasId</span> <span class=''lava-debug-value''> - </span></li>
                            <li><span class=''lava-debug-key''>FullName</span> <span class=''lava-debug-value''> - Noah Decker</span></li>
                            <li><span class=''lava-debug-key''>BirthdayDayOfWeek</span> <span class=''lava-debug-value''> - Thursday</span></li>
                            <li><span class=''lava-debug-key''>BirthdayDayOfWeekShort</span> <span class=''lava-debug-value''> - Thu</span></li>
                            <li><span class=''lava-debug-key''>PhotoUrl</span> <span class=''lava-debug-value''> - /GetImage.ashx?id=47</span></li>
                            <li>
                                <span class=''lava-debug-key lava-debug-section level-1''>Users</span>
                                {<ul></ul>}
                            </li>
                            <li>
                                <span class=''lava-debug-key lava-debug-section level-1''>PhoneNumbers</span>
                                {<ul></ul>}
                            </li>
                            <li><span class=''lava-debug-key''>MaritalStatusValue</span> <span class=''lava-debug-value''> - </span></li>
                            <li><span class=''lava-debug-key''>ConnectionStatusValue</span> <span class=''lava-debug-value''> - </span></li>
                            <li><span class=''lava-debug-key''>ReviewReasonValue</span> <span class=''lava-debug-value''> - </span></li>
                            <li><span class=''lava-debug-key''>RecordStatusValue</span> <span class=''lava-debug-value''> - </span></li>
                            <li><span class=''lava-debug-key''>RecordStatusReasonValue</span> <span class=''lava-debug-value''> - </span></li>
                            <li><span class=''lava-debug-key''>RecordTypeValue</span> <span class=''lava-debug-value''> - </span></li>
                            <li><span class=''lava-debug-key''>SuffixValue</span> <span class=''lava-debug-value''> - </span></li>
                            <li><span class=''lava-debug-key''>TitleValue</span> <span class=''lava-debug-value''> - </span></li>
                            <li><span class=''lava-debug-key''>Photo</span> <span class=''lava-debug-value''> - </span></li>
                            <li><span class=''lava-debug-key''>BirthDate</span> <span class=''lava-debug-value''> - 3/10/2006 12:00:00 AM</span></li>
                            <li><span class=''lava-debug-key''>DaysUntilBirthday</span> <span class=''lava-debug-value''> - </span></li>
                            <li><span class=''lava-debug-key''>Age</span> <span class=''lava-debug-value''> - 10</span></li>
                            <li><span class=''lava-debug-key''>NextBirthDay</span> <span class=''lava-debug-value''> - 3/10/2017 12:00:00 AM</span></li>
                            <li><span class=''lava-debug-key''>DaysToBirthday</span> <span class=''lava-debug-value''> - 290</span></li>
                            <li><span class=''lava-debug-key''>NextAnniversary</span> <span class=''lava-debug-value''> - </span></li>
                            <li><span class=''lava-debug-key''>GradeOffset</span> <span class=''lava-debug-value''> - 7</span></li>
                            <li><span class=''lava-debug-key''>HasGraduated</span> <span class=''lava-debug-value''> - False</span></li>
                            <li><span class=''lava-debug-key''>GradeFormatted</span> <span class=''lava-debug-value''> - 5th Grade</span></li>
                            <li><span class=''lava-debug-key''>ImpersonationParameter</span> <span class=''lava-debug-value''> - rckipid=EAAAAPs3yZQ9UP0gvXtq1yDsSBHZpn5xSUiI4V%...</span></li>
                            <li><span class=''lava-debug-key''>CreatedDateTime</span> <span class=''lava-debug-value''> - 5/23/2016 4:08:34 PM</span></li>
                            <li><span class=''lava-debug-key''>ModifiedDateTime</span> <span class=''lava-debug-value''> - </span></li>
                            <li><span class=''lava-debug-key''>CreatedByPersonAliasId</span> <span class=''lava-debug-value''> - 10</span></li>
                            <li><span class=''lava-debug-key''>ModifiedByPersonAliasId</span> <span class=''lava-debug-value''> - </span></li>
                            <li><span class=''lava-debug-key''>CreatedByPersonId</span> <span class=''lava-debug-value''> - </span></li>
                            <li><span class=''lava-debug-key''>CreatedByPersonName</span> <span class=''lava-debug-value''> - </span></li>
                            <li><span class=''lava-debug-key''>ModifiedByPersonId</span> <span class=''lava-debug-value''> - </span></li>
                            <li><span class=''lava-debug-key''>ModifiedByPersonName</span> <span class=''lava-debug-value''> - </span></li>
                            <li><span class=''lava-debug-key''>ModifiedAuditValuesAlreadyUpdated</span> <span class=''lava-debug-value''> - False</span></li>
                            <li><span class=''lava-debug-key''>Id</span> <span class=''lava-debug-value''> - 5</span></li>
                            <li><span class=''lava-debug-key''>Guid</span> <span class=''lava-debug-value''> - 32aab9e4-970d-4551-a17e-385e66113bd5</span></li>
                            <li><span class=''lava-debug-key''>UrlEncodedKey</span> <span class=''lava-debug-value''> - EAAAADSKnBBeHTdwjp4BlxvZlrPyFdnWP5e1NNCYTt8dgIA...</span></li>
                        </ul>
                    </li>
                    <li>
                        [1]
                        <ul>
                            <li><span class=''lava-debug-key''>FamilyMember</span> <span class=''lava-debug-value''> - True</span></li>
                            <li><span class=''lava-debug-key''>LastCheckIn</span> <span class=''lava-debug-value''> - 5/17/2016 10:24:55 AM</span></li>
                            <li><span class=''lava-debug-key''>FirstTime</span> <span class=''lava-debug-value''> - False</span></li>
                            <li><span class=''lava-debug-key''>SecurityCode</span> <span class=''lava-debug-value''> - HFC</span></li>
                            <li><span class=''lava-debug-key''>IsSystem</span> <span class=''lava-debug-value''> - False</span></li>
                            <li><span class=''lava-debug-key''>RecordTypeValueId</span> <span class=''lava-debug-value''> - 1</span></li>
                            <li><span class=''lava-debug-key''>RecordStatusValueId</span> <span class=''lava-debug-value''> - 3</span></li>
                            <li><span class=''lava-debug-key''>RecordStatusLastModifiedDateTime</span> <span class=''lava-debug-value''> - </span></li>
                            <li><span class=''lava-debug-key''>RecordStatusReasonValueId</span> <span class=''lava-debug-value''> - </span></li>
                            <li><span class=''lava-debug-key''>ConnectionStatusValueId</span> <span class=''lava-debug-value''> - 146</span></li>
                            <li><span class=''lava-debug-key''>ReviewReasonValueId</span> <span class=''lava-debug-value''> - </span></li>
                            <li><span class=''lava-debug-key''>IsDeceased</span> <span class=''lava-debug-value''> - False</span></li>
                            <li><span class=''lava-debug-key''>TitleValueId</span> <span class=''lava-debug-value''> - </span></li>
                            <li><span class=''lava-debug-key''>FirstName</span> <span class=''lava-debug-value''> - Alexis</span></li>
                            <li><span class=''lava-debug-key''>NickName</span> <span class=''lava-debug-value''> - Alex</span></li>
                            <li><span class=''lava-debug-key''>MiddleName</span> <span class=''lava-debug-value''> - </span></li>
                            <li><span class=''lava-debug-key''>LastName</span> <span class=''lava-debug-value''> - Decker</span></li>
                            <li><span class=''lava-debug-key''>SuffixValueId</span> <span class=''lava-debug-value''> - </span></li>
                            <li><span class=''lava-debug-key''>PhotoId</span> <span class=''lava-debug-value''> - 48</span></li>
                            <li><span class=''lava-debug-key''>BirthDay</span> <span class=''lava-debug-value''> - 10</span></li>
                            <li><span class=''lava-debug-key''>BirthMonth</span> <span class=''lava-debug-value''> - 2</span></li>
                            <li><span class=''lava-debug-key''>BirthYear</span> <span class=''lava-debug-value''> - 2009</span></li>
                            <li><span class=''lava-debug-key''>Gender</span> <span class=''lava-debug-value''> - Female</span></li>
                            <li><span class=''lava-debug-key''>MaritalStatusValueId</span> <span class=''lava-debug-value''> - 144</span></li>
                            <li><span class=''lava-debug-key''>AnniversaryDate</span> <span class=''lava-debug-value''> - </span></li>
                            <li><span class=''lava-debug-key''>GraduationYear</span> <span class=''lava-debug-value''> - 2026</span></li>
                            <li><span class=''lava-debug-key''>GivingGroupId</span> <span class=''lava-debug-value''> - </span></li>
                            <li><span class=''lava-debug-key''>GivingId</span> <span class=''lava-debug-value''> - P6</span></li>
                            <li><span class=''lava-debug-key''>GivingLeaderId</span> <span class=''lava-debug-value''> - 0</span></li>
                            <li><span class=''lava-debug-key''>Email</span> <span class=''lava-debug-value''> - </span></li>
                            <li><span class=''lava-debug-key''>IsEmailActive</span> <span class=''lava-debug-value''> - True</span></li>
                            <li><span class=''lava-debug-key''>EmailNote</span> <span class=''lava-debug-value''> - </span></li>
                            <li><span class=''lava-debug-key''>EmailPreference</span> <span class=''lava-debug-value''> - EmailAllowed</span></li>
                            <li><span class=''lava-debug-key''>ReviewReasonNote</span> <span class=''lava-debug-value''> - </span></li>
                            <li><span class=''lava-debug-key''>InactiveReasonNote</span> <span class=''lava-debug-value''> - </span></li>
                            <li><span class=''lava-debug-key''>SystemNote</span> <span class=''lava-debug-value''> - </span></li>
                            <li><span class=''lava-debug-key''>ViewedCount</span> <span class=''lava-debug-value''> - </span></li>
                            <li><span class=''lava-debug-key''>PrimaryAlias</span> <span class=''lava-debug-value''> - </span></li>
                            <li><span class=''lava-debug-key''>PrimaryAliasId</span> <span class=''lava-debug-value''> - </span></li>
                            <li><span class=''lava-debug-key''>FullName</span> <span class=''lava-debug-value''> - Alex Decker</span></li>
                            <li><span class=''lava-debug-key''>BirthdayDayOfWeek</span> <span class=''lava-debug-value''> - Wednesday</span></li>
                            <li><span class=''lava-debug-key''>BirthdayDayOfWeekShort</span> <span class=''lava-debug-value''> - Wed</span></li>
                            <li><span class=''lava-debug-key''>PhotoUrl</span> <span class=''lava-debug-value''> - /GetImage.ashx?id=48</span></li>
                            <li>
                                <span class=''lava-debug-key lava-debug-section level-1''>Users</span>
                                {<ul></ul>}
                            </li>
                            <li>
                                <span class=''lava-debug-key lava-debug-section level-1''>PhoneNumbers</span>
                                {<ul></ul>}
                            </li>
                            <li><span class=''lava-debug-key''>MaritalStatusValue</span> <span class=''lava-debug-value''> - </span></li>
                            <li><span class=''lava-debug-key''>ConnectionStatusValue</span> <span class=''lava-debug-value''> - </span></li>
                            <li><span class=''lava-debug-key''>ReviewReasonValue</span> <span class=''lava-debug-value''> - </span></li>
                            <li><span class=''lava-debug-key''>RecordStatusValue</span> <span class=''lava-debug-value''> - </span></li>
                            <li><span class=''lava-debug-key''>RecordStatusReasonValue</span> <span class=''lava-debug-value''> - </span></li>
                            <li><span class=''lava-debug-key''>RecordTypeValue</span> <span class=''lava-debug-value''> - </span></li>
                            <li><span class=''lava-debug-key''>SuffixValue</span> <span class=''lava-debug-value''> - </span></li>
                            <li><span class=''lava-debug-key''>TitleValue</span> <span class=''lava-debug-value''> - </span></li>
                            <li><span class=''lava-debug-key''>Photo</span> <span class=''lava-debug-value''> - </span></li>
                            <li><span class=''lava-debug-key''>BirthDate</span> <span class=''lava-debug-value''> - 2/10/2009 12:00:00 AM</span></li>
                            <li><span class=''lava-debug-key''>DaysUntilBirthday</span> <span class=''lava-debug-value''> - </span></li>
                            <li><span class=''lava-debug-key''>Age</span> <span class=''lava-debug-value''> - 7</span></li>
                            <li><span class=''lava-debug-key''>NextBirthDay</span> <span class=''lava-debug-value''> - 2/10/2017 12:00:00 AM</span></li>
                            <li><span class=''lava-debug-key''>DaysToBirthday</span> <span class=''lava-debug-value''> - 262</span></li>
                            <li><span class=''lava-debug-key''>NextAnniversary</span> <span class=''lava-debug-value''> - </span></li>
                            <li><span class=''lava-debug-key''>GradeOffset</span> <span class=''lava-debug-value''> - 10</span></li>
                            <li><span class=''lava-debug-key''>HasGraduated</span> <span class=''lava-debug-value''> - False</span></li>
                            <li><span class=''lava-debug-key''>GradeFormatted</span> <span class=''lava-debug-value''> - 2nd Grade</span></li>
                            <li><span class=''lava-debug-key''>ImpersonationParameter</span> <span class=''lava-debug-value''> - rckipid=EAAAANyezkn%2bjZUmiU1Vh34VqDzuoYZXSgwDb...</span></li>
                            <li><span class=''lava-debug-key''>CreatedDateTime</span> <span class=''lava-debug-value''> - 5/23/2016 4:08:34 PM</span></li>
                            <li><span class=''lava-debug-key''>ModifiedDateTime</span> <span class=''lava-debug-value''> - </span></li>
                            <li><span class=''lava-debug-key''>CreatedByPersonAliasId</span> <span class=''lava-debug-value''> - 10</span></li>
                            <li><span class=''lava-debug-key''>ModifiedByPersonAliasId</span> <span class=''lava-debug-value''> - </span></li>
                            <li><span class=''lava-debug-key''>CreatedByPersonId</span> <span class=''lava-debug-value''> - </span></li>
                            <li><span class=''lava-debug-key''>CreatedByPersonName</span> <span class=''lava-debug-value''> - </span></li>
                            <li><span class=''lava-debug-key''>ModifiedByPersonId</span> <span class=''lava-debug-value''> - </span></li>
                            <li><span class=''lava-debug-key''>ModifiedByPersonName</span> <span class=''lava-debug-value''> - </span></li>
                            <li><span class=''lava-debug-key''>ModifiedAuditValuesAlreadyUpdated</span> <span class=''lava-debug-value''> - False</span></li>
                            <li><span class=''lava-debug-key''>Id</span> <span class=''lava-debug-value''> - 6</span></li>
                            <li><span class=''lava-debug-key''>Guid</span> <span class=''lava-debug-value''> - 27919690-3cce-4fa6-95c4-cd21419eb51f</span></li>
                            <li><span class=''lava-debug-key''>UrlEncodedKey</span> <span class=''lava-debug-value''> - EAAAABg517P4N18QOQSywaweyHrvea5oV!2bBFl7YmACRx0...</span></li>
                        </ul>
                    </li>
                </ul>}
            </div>
        </div>
    </div><div class=''panel panel-default panel-lavadebug''>
        <div class=''panel-heading clearfix collapsed'' data-toggle=''collapse'' data-target=''#collapse-4ad46515-0cf9-465d-b703-5ec63ba9b3d6''><h5 class=''panel-title pull-left''>Group Type</h5> <div class=''pull-right''><i class=''fa fa-chevron-up''></i></div></div><div id=''collapse-4ad46515-0cf9-465d-b703-5ec63ba9b3d6'' class=''panel-collapse collapse''>
            <div class=''panel-body''>
                <p>GroupType properties can be accessed by <code>{{ GroupType.[PropertyKey] }}</code>.</p>
                <ul>
                    <li><span class=''lava-debug-key''>LastCheckIn</span> <span class=''lava-debug-value''> - 5/17/2016 10:24:55 AM</span></li>
                    <li>
                        <span class=''lava-debug-key lava-debug-section level-1''>Groups</span>
                        {<ul>
                            <li>
                                [0]
                                <ul>
                                    <li><span class=''lava-debug-key''>LastCheckIn</span> <span class=''lava-debug-value''> - </span></li>
                                    <li>
                                        <span class=''lava-debug-key lava-debug-section level-1''>Locations</span>
                                        {<ul>
                                            <li>
                                                [0]
                                                <ul>
                                                    <li><span class=''lava-debug-key''>LastCheckIn</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>Locations</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>ParentLocationId</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>Name</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>IsActive</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>LocationTypeValueId</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>GeoPoint</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>GeoFence</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>Street1</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>Street2</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>City</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>County</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>State</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>Country</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>PostalCode</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>Barcode</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>AssessorParcelId</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>StandardizeAttemptedDateTime</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>StandardizeAttemptedServiceType</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>StandardizeAttemptedResult</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>StandardizedDateTime</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>GeocodeAttemptedDateTime</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>GeocodeAttemptedServiceType</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>GeocodeAttemptedResult</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>GeocodedDateTime</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>IsGeoPointLocked</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>PrinterDeviceId</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>ImageId</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>SoftRoomThreshold</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>FirmRoomThreshold</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>IsNamedLocation</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>LocationTypeValue</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>ChildLocations</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>PrinterDevice</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>Image</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>FormattedAddress</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>FormattedHtmlAddress</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>Latitude</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>Longitude</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>GeoFenceCoordinates</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>CampusId</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>Distance</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>CreatedDateTime</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>ModifiedDateTime</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>CreatedByPersonAliasId</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>ModifiedByPersonAliasId</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>CreatedByPersonId</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>CreatedByPersonName</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>ModifiedByPersonId</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>ModifiedByPersonName</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>ModifiedAuditValuesAlreadyUpdated</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>Id</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>Guid</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>UrlEncodedKey</span> <span class=''lava-debug-value''> - </span></li>
                                                </ul>
                                            </li>
                                        </ul>}
                                    </li>
                                    <li><span class=''lava-debug-key''>IsSystem</span> <span class=''lava-debug-value''> - False</span></li>
                                    <li><span class=''lava-debug-key''>ParentGroupId</span> <span class=''lava-debug-value''> - </span></li>
                                    <li><span class=''lava-debug-key''>GroupTypeId</span> <span class=''lava-debug-value''> - 20</span></li>
                                    <li><span class=''lava-debug-key''>CampusId</span> <span class=''lava-debug-value''> - </span></li>
                                    <li><span class=''lava-debug-key''>ScheduleId</span> <span class=''lava-debug-value''> - </span></li>
                                    <li><span class=''lava-debug-key''>Name</span> <span class=''lava-debug-value''> - Grades 2-3</span></li>
                                    <li><span class=''lava-debug-key''>Description</span> <span class=''lava-debug-value''> - </span></li>
                                    <li><span class=''lava-debug-key''>IsSecurityRole</span> <span class=''lava-debug-value''> - False</span></li>
                                    <li><span class=''lava-debug-key''>IsActive</span> <span class=''lava-debug-value''> - True</span></li>
                                    <li><span class=''lava-debug-key''>Order</span> <span class=''lava-debug-value''> - 5</span></li>
                                    <li><span class=''lava-debug-key''>AllowGuests</span> <span class=''lava-debug-value''> - </span></li>
                                    <li><span class=''lava-debug-key''>WelcomeSystemEmailId</span> <span class=''lava-debug-value''> - </span></li>
                                    <li><span class=''lava-debug-key''>ExitSystemEmailId</span> <span class=''lava-debug-value''> - </span></li>
                                    <li><span class=''lava-debug-key''>SyncDataViewId</span> <span class=''lava-debug-value''> - </span></li>
                                    <li><span class=''lava-debug-key''>AddUserAccountsDuringSync</span> <span class=''lava-debug-value''> - </span></li>
                                    <li><span class=''lava-debug-key''>MustMeetRequirementsToAddMember</span> <span class=''lava-debug-value''> - </span></li>
                                    <li><span class=''lava-debug-key''>IsPublic</span> <span class=''lava-debug-value''> - True</span></li>
                                    <li><span class=''lava-debug-key''>GroupCapacity</span> <span class=''lava-debug-value''> - </span></li>
                                    <li><span class=''lava-debug-key''>GroupType</span> <span class=''lava-debug-value''> - </span></li>
                                    <li><span class=''lava-debug-key''>Campus</span> <span class=''lava-debug-value''> - </span></li>
                                    <li><span class=''lava-debug-key''>Schedule</span> <span class=''lava-debug-value''> - </span></li>
                                    <li><span class=''lava-debug-key''>WelcomeSystemEmail</span> <span class=''lava-debug-value''> - </span></li>
                                    <li><span class=''lava-debug-key''>ExitSystemEmail</span> <span class=''lava-debug-value''> - </span></li>
                                    <li><span class=''lava-debug-key''>SyncDataView</span> <span class=''lava-debug-value''> - </span></li>
                                    <li>
                                        <span class=''lava-debug-key lava-debug-section level-1''>Groups</span>
                                        {<ul></ul>}
                                    </li>
                                    <li>
                                        <span class=''lava-debug-key lava-debug-section level-1''>Members</span>
                                        {<ul></ul>}
                                    </li>
                                    <li>
                                        <span class=''lava-debug-key lava-debug-section level-1''>GroupLocations</span>
                                        {<ul></ul>}
                                    </li>
                                    <li>
                                        <span class=''lava-debug-key lava-debug-section level-1''>GroupRequirements</span>
                                        {<ul></ul>}
                                    </li>
                                    <li><span class=''lava-debug-key''>CreatedDateTime</span> <span class=''lava-debug-value''> - </span></li>
                                    <li><span class=''lava-debug-key''>ModifiedDateTime</span> <span class=''lava-debug-value''> - </span></li>
                                    <li><span class=''lava-debug-key''>CreatedByPersonAliasId</span> <span class=''lava-debug-value''> - </span></li>
                                    <li><span class=''lava-debug-key''>ModifiedByPersonAliasId</span> <span class=''lava-debug-value''> - </span></li>
                                    <li><span class=''lava-debug-key''>CreatedByPersonId</span> <span class=''lava-debug-value''> - </span></li>
                                    <li><span class=''lava-debug-key''>CreatedByPersonName</span> <span class=''lava-debug-value''> - </span></li>
                                    <li><span class=''lava-debug-key''>ModifiedByPersonId</span> <span class=''lava-debug-value''> - </span></li>
                                    <li><span class=''lava-debug-key''>ModifiedByPersonName</span> <span class=''lava-debug-value''> - </span></li>
                                    <li><span class=''lava-debug-key''>ModifiedAuditValuesAlreadyUpdated</span> <span class=''lava-debug-value''> - False</span></li>
                                    <li><span class=''lava-debug-key''>Id</span> <span class=''lava-debug-value''> - 29</span></li>
                                    <li><span class=''lava-debug-key''>Guid</span> <span class=''lava-debug-value''> - 24901861-14cf-474f-9fce-7ba1d6c84bff</span></li>
                                    <li><span class=''lava-debug-key''>UrlEncodedKey</span> <span class=''lava-debug-value''> - EAAAAIG27LDlHWCOlEiRM0G2znFnTV0jIcM4SsRS77YJ9QM...</span></li>
                                </ul>
                            </li>
                        </ul>}
                    </li>
                    <li><span class=''lava-debug-key''>IsSystem</span> <span class=''lava-debug-value''> - False</span></li>
                    <li><span class=''lava-debug-key''>Name</span> <span class=''lava-debug-value''> - Elementary Area</span></li>
                    <li><span class=''lava-debug-key''>Description</span> <span class=''lava-debug-value''> - </span></li>
                    <li><span class=''lava-debug-key''>GroupTerm</span> <span class=''lava-debug-value''> - Group</span></li>
                    <li><span class=''lava-debug-key''>GroupMemberTerm</span> <span class=''lava-debug-value''> - Member</span></li>
                    <li><span class=''lava-debug-key''>DefaultGroupRoleId</span> <span class=''lava-debug-value''> - 39</span></li>
                    <li><span class=''lava-debug-key''>AllowedScheduleTypes</span> <span class=''lava-debug-value''> - None</span></li>
                    <li><span class=''lava-debug-key''>AllowMultipleLocations</span> <span class=''lava-debug-value''> - True</span></li>
                    <li><span class=''lava-debug-key''>ShowInGroupList</span> <span class=''lava-debug-value''> - False</span></li>
                    <li><span class=''lava-debug-key''>ShowInNavigation</span> <span class=''lava-debug-value''> - False</span></li>
                    <li><span class=''lava-debug-key''>IconCssClass</span> <span class=''lava-debug-value''> - </span></li>
                    <li><span class=''lava-debug-key''>TakesAttendance</span> <span class=''lava-debug-value''> - True</span></li>
                    <li><span class=''lava-debug-key''>SendAttendanceReminder</span> <span class=''lava-debug-value''> - False</span></li>
                    <li><span class=''lava-debug-key''>AttendanceRule</span> <span class=''lava-debug-value''> - None</span></li>
                    <li><span class=''lava-debug-key''>AttendancePrintTo</span> <span class=''lava-debug-value''> - Default</span></li>
                    <li><span class=''lava-debug-key''>Order</span> <span class=''lava-debug-value''> - 2</span></li>
                    <li><span class=''lava-debug-key''>InheritedGroupTypeId</span> <span class=''lava-debug-value''> - 17</span></li>
                    <li>
                        <span class=''lava-debug-key lava-debug-section level-1''>InheritedGroupType</span>
                        <ul>
                            <li><span class=''lava-debug-key''>IsSystem</span> <span class=''lava-debug-value''> - False</span></li>
                            <li><span class=''lava-debug-key''>Name</span> <span class=''lava-debug-value''> - Check in by Grade</span></li>
                            <li><span class=''lava-debug-key''>Description</span> <span class=''lava-debug-value''> - </span></li>
                            <li><span class=''lava-debug-key''>GroupTerm</span> <span class=''lava-debug-value''> - Group</span></li>
                            <li><span class=''lava-debug-key''>GroupMemberTerm</span> <span class=''lava-debug-value''> - Member</span></li>
                            <li><span class=''lava-debug-key''>DefaultGroupRoleId</span> <span class=''lava-debug-value''> - 36</span></li>
                            <li><span class=''lava-debug-key''>AllowedScheduleTypes</span> <span class=''lava-debug-value''> - None</span></li>
                            <li><span class=''lava-debug-key''>AllowMultipleLocations</span> <span class=''lava-debug-value''> - False</span></li>
                            <li><span class=''lava-debug-key''>ShowInGroupList</span> <span class=''lava-debug-value''> - False</span></li>
                            <li><span class=''lava-debug-key''>ShowInNavigation</span> <span class=''lava-debug-value''> - False</span></li>
                            <li><span class=''lava-debug-key''>IconCssClass</span> <span class=''lava-debug-value''> - </span></li>
                            <li><span class=''lava-debug-key''>TakesAttendance</span> <span class=''lava-debug-value''> - False</span></li>
                            <li><span class=''lava-debug-key''>SendAttendanceReminder</span> <span class=''lava-debug-value''> - False</span></li>
                            <li><span class=''lava-debug-key''>AttendanceRule</span> <span class=''lava-debug-value''> - None</span></li>
                            <li><span class=''lava-debug-key''>AttendancePrintTo</span> <span class=''lava-debug-value''> - Default</span></li>
                            <li><span class=''lava-debug-key''>Order</span> <span class=''lava-debug-value''> - 0</span></li>
                            <li><span class=''lava-debug-key''>InheritedGroupTypeId</span> <span class=''lava-debug-value''> - 15</span></li>
                            <li>
                                <span class=''lava-debug-key lava-debug-section level-2''>InheritedGroupType</span>
                                <ul>
                                    <li><span class=''lava-debug-key''>IsSystem</span> <span class=''lava-debug-value''> - False</span></li>
                                    <li><span class=''lava-debug-key''>Name</span> <span class=''lava-debug-value''> - Check in by Age</span></li>
                                    <li><span class=''lava-debug-key''>Description</span> <span class=''lava-debug-value''> - </span></li>
                                    <li><span class=''lava-debug-key''>GroupTerm</span> <span class=''lava-debug-value''> - Group</span></li>
                                    <li><span class=''lava-debug-key''>GroupMemberTerm</span> <span class=''lava-debug-value''> - Member</span></li>
                                    <li><span class=''lava-debug-key''>DefaultGroupRoleId</span> <span class=''lava-debug-value''> - 34</span></li>
                                    <li><span class=''lava-debug-key''>AllowedScheduleTypes</span> <span class=''lava-debug-value''> - None</span></li>
                                    <li><span class=''lava-debug-key''>AllowMultipleLocations</span> <span class=''lava-debug-value''> - False</span></li>
                                    <li><span class=''lava-debug-key''>ShowInGroupList</span> <span class=''lava-debug-value''> - False</span></li>
                                    <li><span class=''lava-debug-key''>ShowInNavigation</span> <span class=''lava-debug-value''> - False</span></li>
                                    <li><span class=''lava-debug-key''>IconCssClass</span> <span class=''lava-debug-value''> - </span></li>
                                    <li><span class=''lava-debug-key''>TakesAttendance</span> <span class=''lava-debug-value''> - False</span></li>
                                    <li><span class=''lava-debug-key''>SendAttendanceReminder</span> <span class=''lava-debug-value''> - False</span></li>
                                    <li><span class=''lava-debug-key''>AttendanceRule</span> <span class=''lava-debug-value''> - None</span></li>
                                    <li><span class=''lava-debug-key''>AttendancePrintTo</span> <span class=''lava-debug-value''> - Default</span></li>
                                    <li><span class=''lava-debug-key''>Order</span> <span class=''lava-debug-value''> - 0</span></li>
                                    <li><span class=''lava-debug-key''>InheritedGroupTypeId</span> <span class=''lava-debug-value''> - </span></li>
                                    <li><span class=''lava-debug-key''>InheritedGroupType</span> <span class=''lava-debug-value''> - </span></li>
                                    <li><span class=''lava-debug-key''>LocationSelectionMode</span> <span class=''lava-debug-value''> - None</span></li>
                                    <li><span class=''lava-debug-key''>EnableLocationSchedules</span> <span class=''lava-debug-value''> - </span></li>
                                    <li><span class=''lava-debug-key''>EnableAlternatePlacements</span> <span class=''lava-debug-value''> - False</span></li>
                                    <li><span class=''lava-debug-key''>GroupTypePurposeValueId</span> <span class=''lava-debug-value''> - 145</span></li>
                                    <li>
                                        <span class=''lava-debug-key lava-debug-section level-3''>GroupTypePurposeValue</span>
                                        <ul>
                                            <li><span class=''lava-debug-key''>IsSystem</span> <span class=''lava-debug-value''> - True</span></li>
                                            <li><span class=''lava-debug-key''>DefinedTypeId</span> <span class=''lava-debug-value''> - 31</span></li>
                                            <li><span class=''lava-debug-key''>Order</span> <span class=''lava-debug-value''> - 1</span></li>
                                            <li><span class=''lava-debug-key''>Value</span> <span class=''lava-debug-value''> - Check-in Filter</span></li>
                                            <li><span class=''lava-debug-key''>Description</span> <span class=''lava-debug-value''> - A Group Type where the purpose is for check-in...</span></li>
                                            <li>
                                                <span class=''lava-debug-key lava-debug-section level-4''>DefinedType</span>
                                                <ul>
                                                    <li><span class=''lava-debug-key''>IsSystem</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>FieldTypeId</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>Order</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>CategoryId</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>Name</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>Description</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>Category</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>FieldType</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>DefinedValues</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>Id</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>Guid</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>ForeignId</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>ForeignGuid</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>ForeignKey</span> <span class=''lava-debug-value''> - </span></li>
                                                </ul>
                                            </li>
                                            <li><span class=''lava-debug-key''>Id</span> <span class=''lava-debug-value''> - 145</span></li>
                                            <li><span class=''lava-debug-key''>Guid</span> <span class=''lava-debug-value''> - 6bced84c-69ad-4f5a-9197-5c0f9c02dd34</span></li>
                                            <li><span class=''lava-debug-key''>ForeignId</span> <span class=''lava-debug-value''> - </span></li>
                                            <li><span class=''lava-debug-key''>ForeignGuid</span> <span class=''lava-debug-value''> - </span></li>
                                            <li><span class=''lava-debug-key''>ForeignKey</span> <span class=''lava-debug-value''> - </span></li>
                                        </ul>
                                    </li>
                                    <li>
                                        <span class=''lava-debug-key lava-debug-section level-3''>Roles</span>
                                        {<ul>
                                            <li>[0] <span class=''lava-debug-value''> - Member</span></li>
                                        </ul>}
                                    </li>
                                    <li>
                                        <span class=''lava-debug-key lava-debug-section level-3''>GroupScheduleExclusions</span>
                                        {<ul></ul>}
                                    </li>
                                    <li>
                                        <span class=''lava-debug-key lava-debug-section level-3''>ChildGroupTypes</span>
                                        {<ul></ul>}
                                    </li>
                                    <li>
                                        <span class=''lava-debug-key lava-debug-section level-3''>ParentGroupTypes</span>
                                        {<ul>
                                            <li>
                                                [0]
                                                <ul>
                                                    <li><span class=''lava-debug-key''>IsSystem</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>Name</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>Description</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>GroupTerm</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>GroupMemberTerm</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>DefaultGroupRoleId</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>AllowedScheduleTypes</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>AllowMultipleLocations</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>ShowInGroupList</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>ShowInNavigation</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>IconCssClass</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>TakesAttendance</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>SendAttendanceReminder</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>AttendanceRule</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>AttendancePrintTo</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>Order</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>InheritedGroupTypeId</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>InheritedGroupType</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>LocationSelectionMode</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>EnableLocationSchedules</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>EnableAlternatePlacements</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>GroupTypePurposeValueId</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>GroupTypePurposeValue</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>Roles</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>GroupScheduleExclusions</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>ChildGroupTypes</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>ParentGroupTypes</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>LocationTypeValues</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>Id</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>Guid</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>ForeignId</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>ForeignGuid</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>ForeignKey</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li>
                                                        <span class=''lava-debug-key lava-debug-section level-1''>Attributes <p class=''attributes''>Below is a list of attributes that can be retrieved using <code>{{ GroupType.InheritedGroupType.InheritedGroupType.ParentGroupTypes | Attribute:''[AttributeKey]'' }}</code>.</p></span>
                                                        <ul>
                                                            <li><span class=''lava-debug-key''>core_checkin_CheckInType</span> <span class=''lava-debug-value''> - Family</span></li>
                                                            <li><span class=''lava-debug-key''>core_checkin_EnableManagerOption</span> <span class=''lava-debug-value''> - Yes</span></li>
                                                            <li><span class=''lava-debug-key''>core_checkin_EnableOverride</span> <span class=''lava-debug-value''> - Yes</span></li>
                                                            <li><span class=''lava-debug-key''>core_checkin_SecurityCodeLength</span> <span class=''lava-debug-value''> - 3</span></li>
                                                            <li><span class=''lava-debug-key''>core_checkin_ReuseSameCode</span> <span class=''lava-debug-value''> - No</span></li>
                                                            <li><span class=''lava-debug-key''>core_checkin_UseSameOptions</span> <span class=''lava-debug-value''> - Yes</span></li>
                                                            <li><span class=''lava-debug-key''>core_checkin_SearchType</span> <span class=''lava-debug-value''> - Phone Number</span></li>
                                                            <li><span class=''lava-debug-key lava-debug-section level-2''>core_checkin_RegularExpressionFilter</span> </li>
                                                            <li><span class=''lava-debug-key''>core_checkin_MaxSearchResults</span> <span class=''lava-debug-value''> - 100</span></li>
                                                            <li><span class=''lava-debug-key''>core_checkin_MinimumPhoneSearchLength</span> <span class=''lava-debug-value''> - 4</span></li>
                                                            <li><span class=''lava-debug-key''>core_checkin_MaximumPhoneSearchLength</span> <span class=''lava-debug-value''> - 10</span></li>
                                                            <li><span class=''lava-debug-key''>core_checkin_PhoneSearchType</span> <span class=''lava-debug-value''> - Ends With</span></li>
                                                            <li><span class=''lava-debug-key''>core_checkin_RefreshInterval</span> <span class=''lava-debug-value''> - 10</span></li>
                                                            <li><span class=''lava-debug-key''>core_checkin_AgeRequired</span> <span class=''lava-debug-value''> - Yes</span></li>
                                                            <li><span class=''lava-debug-key''>core_checkin_DisplayLocationCount</span> <span class=''lava-debug-value''> - Yes</span></li>
                                                            <li><span class=''lava-debug-key''>core_checkin_AutoSelectDaysBack</span> <span class=''lava-debug-value''> - 10</span></li>
                                                        </ul>
                                                    </li>
                                                </ul>
                                            </li>
                                        </ul>}
                                    </li>
                                    <li>
                                        <span class=''lava-debug-key lava-debug-section level-3''>LocationTypeValues</span>
                                        {<ul></ul>}
                                    </li>
                                    <li><span class=''lava-debug-key''>Id</span> <span class=''lava-debug-value''> - 15</span></li>
                                    <li><span class=''lava-debug-key''>Guid</span> <span class=''lava-debug-value''> - 0572a5fe-20a4-4bf1-95cd-c71db5281392</span></li>
                                    <li><span class=''lava-debug-key''>ForeignId</span> <span class=''lava-debug-value''> - </span></li>
                                    <li><span class=''lava-debug-key''>ForeignGuid</span> <span class=''lava-debug-value''> - </span></li>
                                    <li><span class=''lava-debug-key''>ForeignKey</span> <span class=''lava-debug-value''> - </span></li>
                                </ul>
                            </li>
                            <li><span class=''lava-debug-key''>LocationSelectionMode</span> <span class=''lava-debug-value''> - None</span></li>
                            <li><span class=''lava-debug-key''>EnableLocationSchedules</span> <span class=''lava-debug-value''> - </span></li>
                            <li><span class=''lava-debug-key''>EnableAlternatePlacements</span> <span class=''lava-debug-value''> - False</span></li>
                            <li><span class=''lava-debug-key''>GroupTypePurposeValueId</span> <span class=''lava-debug-value''> - 145</span></li>
                            <li>
                                <span class=''lava-debug-key lava-debug-section level-2''>GroupTypePurposeValue</span>
                                <ul>
                                    <li><span class=''lava-debug-key''>IsSystem</span> <span class=''lava-debug-value''> - True</span></li>
                                    <li><span class=''lava-debug-key''>DefinedTypeId</span> <span class=''lava-debug-value''> - 31</span></li>
                                    <li><span class=''lava-debug-key''>Order</span> <span class=''lava-debug-value''> - 1</span></li>
                                    <li><span class=''lava-debug-key''>Value</span> <span class=''lava-debug-value''> - Check-in Filter</span></li>
                                    <li><span class=''lava-debug-key''>Description</span> <span class=''lava-debug-value''> - A Group Type where the purpose is for check-in...</span></li>
                                    <li>
                                        <span class=''lava-debug-key lava-debug-section level-3''>DefinedType</span>
                                        <ul>
                                            <li><span class=''lava-debug-key''>IsSystem</span> <span class=''lava-debug-value''> - True</span></li>
                                            <li><span class=''lava-debug-key''>FieldTypeId</span> <span class=''lava-debug-value''> - 1</span></li>
                                            <li><span class=''lava-debug-key''>Order</span> <span class=''lava-debug-value''> - 16</span></li>
                                            <li><span class=''lava-debug-key''>CategoryId</span> <span class=''lava-debug-value''> - 150</span></li>
                                            <li><span class=''lava-debug-key''>Name</span> <span class=''lava-debug-value''> - Group Type Purpose</span></li>
                                            <li><span class=''lava-debug-key''>Description</span> <span class=''lava-debug-value''> - Determines the role (check-in template,...</span></li>
                                            <li>
                                                <span class=''lava-debug-key lava-debug-section level-4''>Category</span>
                                                <ul>
                                                    <li><span class=''lava-debug-key''>IsSystem</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>ParentCategoryId</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>EntityTypeId</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>EntityTypeQualifierColumn</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>EntityTypeQualifierValue</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>Name</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>Description</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>Order</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>IconCssClass</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>HighlightColor</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>ParentCategory</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>Categories</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>Id</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>Guid</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>ForeignId</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>ForeignGuid</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>ForeignKey</span> <span class=''lava-debug-value''> - </span></li>
                                                </ul>
                                            </li>
                                            <li>
                                                <span class=''lava-debug-key lava-debug-section level-4''>FieldType</span>
                                                <ul>
                                                    <li><span class=''lava-debug-key''>IsSystem</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>Name</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>Description</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>Assembly</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>Class</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>Field</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>Id</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>Guid</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>ForeignId</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>ForeignGuid</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>ForeignKey</span> <span class=''lava-debug-value''> - </span></li>
                                                </ul>
                                            </li>
                                            <li>
                                                <span class=''lava-debug-key lava-debug-section level-4''>DefinedValues</span>
                                                {<ul>
                                                    <li>[0] <span class=''lava-debug-value''> - </span></li>
                                                    <li>[1] <span class=''lava-debug-value''> - </span></li>
                                                    <li>[2] <span class=''lava-debug-value''> - ...</span></li>
                                                </ul>}
                                            </li>
                                            <li><span class=''lava-debug-key''>Id</span> <span class=''lava-debug-value''> - 31</span></li>
                                            <li><span class=''lava-debug-key''>Guid</span> <span class=''lava-debug-value''> - b23f1e45-bc26-4e82-beb3-9b191fe5ccc3</span></li>
                                            <li><span class=''lava-debug-key''>ForeignId</span> <span class=''lava-debug-value''> - </span></li>
                                            <li><span class=''lava-debug-key''>ForeignGuid</span> <span class=''lava-debug-value''> - </span></li>
                                            <li><span class=''lava-debug-key''>ForeignKey</span> <span class=''lava-debug-value''> - </span></li>
                                        </ul>
                                    </li>
                                    <li><span class=''lava-debug-key''>Id</span> <span class=''lava-debug-value''> - 145</span></li>
                                    <li><span class=''lava-debug-key''>Guid</span> <span class=''lava-debug-value''> - 6bced84c-69ad-4f5a-9197-5c0f9c02dd34</span></li>
                                    <li><span class=''lava-debug-key''>ForeignId</span> <span class=''lava-debug-value''> - </span></li>
                                    <li><span class=''lava-debug-key''>ForeignGuid</span> <span class=''lava-debug-value''> - </span></li>
                                    <li><span class=''lava-debug-key''>ForeignKey</span> <span class=''lava-debug-value''> - </span></li>
                                </ul>
                            </li>
                            <li>
                                <span class=''lava-debug-key lava-debug-section level-2''>Roles</span>
                                {<ul>
                                    <li>[0] <span class=''lava-debug-value''> - Member</span></li>
                                </ul>}
                            </li>
                            <li>
                                <span class=''lava-debug-key lava-debug-section level-2''>GroupScheduleExclusions</span>
                                {<ul></ul>}
                            </li>
                            <li>
                                <span class=''lava-debug-key lava-debug-section level-2''>ChildGroupTypes</span>
                                {<ul></ul>}
                            </li>
                            <li>
                                <span class=''lava-debug-key lava-debug-section level-2''>ParentGroupTypes</span>
                                {<ul>
                                    <li>
                                        [0]
                                        <ul>
                                            <li><span class=''lava-debug-key''>IsSystem</span> <span class=''lava-debug-value''> - False</span></li>
                                            <li><span class=''lava-debug-key''>Name</span> <span class=''lava-debug-value''> - Weekly Service Check-in Area</span></li>
                                            <li><span class=''lava-debug-key''>Description</span> <span class=''lava-debug-value''> - </span></li>
                                            <li><span class=''lava-debug-key''>GroupTerm</span> <span class=''lava-debug-value''> - Group</span></li>
                                            <li><span class=''lava-debug-key''>GroupMemberTerm</span> <span class=''lava-debug-value''> - Member</span></li>
                                            <li><span class=''lava-debug-key''>DefaultGroupRoleId</span> <span class=''lava-debug-value''> - 33</span></li>
                                            <li><span class=''lava-debug-key''>AllowedScheduleTypes</span> <span class=''lava-debug-value''> - None</span></li>
                                            <li><span class=''lava-debug-key''>AllowMultipleLocations</span> <span class=''lava-debug-value''> - False</span></li>
                                            <li><span class=''lava-debug-key''>ShowInGroupList</span> <span class=''lava-debug-value''> - False</span></li>
                                            <li><span class=''lava-debug-key''>ShowInNavigation</span> <span class=''lava-debug-value''> - False</span></li>
                                            <li><span class=''lava-debug-key''>IconCssClass</span> <span class=''lava-debug-value''> - fa fa-child</span></li>
                                            <li><span class=''lava-debug-key''>TakesAttendance</span> <span class=''lava-debug-value''> - False</span></li>
                                            <li><span class=''lava-debug-key''>SendAttendanceReminder</span> <span class=''lava-debug-value''> - False</span></li>
                                            <li><span class=''lava-debug-key''>AttendanceRule</span> <span class=''lava-debug-value''> - None</span></li>
                                            <li><span class=''lava-debug-key''>AttendancePrintTo</span> <span class=''lava-debug-value''> - Default</span></li>
                                            <li><span class=''lava-debug-key''>Order</span> <span class=''lava-debug-value''> - 0</span></li>
                                            <li><span class=''lava-debug-key''>InheritedGroupTypeId</span> <span class=''lava-debug-value''> - </span></li>
                                            <li><span class=''lava-debug-key''>InheritedGroupType</span> <span class=''lava-debug-value''> - </span></li>
                                            <li><span class=''lava-debug-key''>LocationSelectionMode</span> <span class=''lava-debug-value''> - None</span></li>
                                            <li><span class=''lava-debug-key''>EnableLocationSchedules</span> <span class=''lava-debug-value''> - </span></li>
                                            <li><span class=''lava-debug-key''>EnableAlternatePlacements</span> <span class=''lava-debug-value''> - False</span></li>
                                            <li><span class=''lava-debug-key''>GroupTypePurposeValueId</span> <span class=''lava-debug-value''> - 142</span></li>
                                            <li>
                                                <span class=''lava-debug-key lava-debug-section level-1''>GroupTypePurposeValue</span>
                                                <ul>
                                                    <li><span class=''lava-debug-key''>IsSystem</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>DefinedTypeId</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>Order</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>Value</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>Description</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>DefinedType</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>Id</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>Guid</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>ForeignId</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>ForeignGuid</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>ForeignKey</span> <span class=''lava-debug-value''> - </span></li>
                                                </ul>
                                            </li>
                                            <li>
                                                <span class=''lava-debug-key lava-debug-section level-1''>Roles</span>
                                                {<ul>
                                                    <li>[0] <span class=''lava-debug-value''> - </span></li>
                                                </ul>}
                                            </li>
                                            <li>
                                                <span class=''lava-debug-key lava-debug-section level-1''>GroupScheduleExclusions</span>
                                                {<ul></ul>}
                                            </li>
                                            <li>
                                                <span class=''lava-debug-key lava-debug-section level-1''>ChildGroupTypes</span>
                                                {<ul>
                                                    <li>[0] <span class=''lava-debug-value''> - </span></li>
                                                    <li>[1] <span class=''lava-debug-value''> - </span></li>
                                                    <li>[2] <span class=''lava-debug-value''> - ...</span></li>
                                                </ul>}
                                            </li>
                                            <li>
                                                <span class=''lava-debug-key lava-debug-section level-1''>ParentGroupTypes</span>
                                                {<ul></ul>}
                                            </li>
                                            <li>
                                                <span class=''lava-debug-key lava-debug-section level-1''>LocationTypeValues</span>
                                                {<ul></ul>}
                                            </li>
                                            <li><span class=''lava-debug-key''>Id</span> <span class=''lava-debug-value''> - 14</span></li>
                                            <li><span class=''lava-debug-key''>Guid</span> <span class=''lava-debug-value''> - fedd389a-616f-4a53-906c-63d8255631c5</span></li>
                                            <li><span class=''lava-debug-key''>ForeignId</span> <span class=''lava-debug-value''> - </span></li>
                                            <li><span class=''lava-debug-key''>ForeignGuid</span> <span class=''lava-debug-value''> - </span></li>
                                            <li><span class=''lava-debug-key''>ForeignKey</span> <span class=''lava-debug-value''> - </span></li>
                                            <li>
                                                <span class=''lava-debug-key lava-debug-section level-1''>Attributes <p class=''attributes''>Below is a list of attributes that can be retrieved using <code>{{ GroupType.InheritedGroupType.ParentGroupTypes | Attribute:''[AttributeKey]'' }}</code>.</p></span>
                                                <ul>
                                                    <li><span class=''lava-debug-key''>core_checkin_CheckInType</span> <span class=''lava-debug-value''> - Family</span></li>
                                                    <li><span class=''lava-debug-key''>core_checkin_EnableManagerOption</span> <span class=''lava-debug-value''> - Yes</span></li>
                                                    <li><span class=''lava-debug-key''>core_checkin_EnableOverride</span> <span class=''lava-debug-value''> - Yes</span></li>
                                                    <li><span class=''lava-debug-key''>core_checkin_SecurityCodeLength</span> <span class=''lava-debug-value''> - 3</span></li>
                                                    <li><span class=''lava-debug-key''>core_checkin_ReuseSameCode</span> <span class=''lava-debug-value''> - No</span></li>
                                                    <li><span class=''lava-debug-key''>core_checkin_UseSameOptions</span> <span class=''lava-debug-value''> - Yes</span></li>
                                                    <li><span class=''lava-debug-key''>core_checkin_SearchType</span> <span class=''lava-debug-value''> - Phone Number</span></li>
                                                    <li><span class=''lava-debug-key lava-debug-section level-2''>core_checkin_RegularExpressionFilter</span> </li>
                                                    <li><span class=''lava-debug-key''>core_checkin_MaxSearchResults</span> <span class=''lava-debug-value''> - 100</span></li>
                                                    <li><span class=''lava-debug-key''>core_checkin_MinimumPhoneSearchLength</span> <span class=''lava-debug-value''> - 4</span></li>
                                                    <li><span class=''lava-debug-key''>core_checkin_MaximumPhoneSearchLength</span> <span class=''lava-debug-value''> - 10</span></li>
                                                    <li><span class=''lava-debug-key''>core_checkin_PhoneSearchType</span> <span class=''lava-debug-value''> - Ends With</span></li>
                                                    <li><span class=''lava-debug-key''>core_checkin_RefreshInterval</span> <span class=''lava-debug-value''> - 10</span></li>
                                                    <li><span class=''lava-debug-key''>core_checkin_AgeRequired</span> <span class=''lava-debug-value''> - Yes</span></li>
                                                    <li><span class=''lava-debug-key''>core_checkin_DisplayLocationCount</span> <span class=''lava-debug-value''> - Yes</span></li>
                                                    <li><span class=''lava-debug-key''>core_checkin_AutoSelectDaysBack</span> <span class=''lava-debug-value''> - 10</span></li>
                                                </ul>
                                            </li>
                                        </ul>
                                    </li>
                                </ul>}
                            </li>
                            <li>
                                <span class=''lava-debug-key lava-debug-section level-2''>LocationTypeValues</span>
                                {<ul></ul>}
                            </li>
                            <li><span class=''lava-debug-key''>Id</span> <span class=''lava-debug-value''> - 17</span></li>
                            <li><span class=''lava-debug-key''>Guid</span> <span class=''lava-debug-value''> - 4f9565a7-dd5a-41c3-b4e8-13f0b872b10b</span></li>
                            <li><span class=''lava-debug-key''>ForeignId</span> <span class=''lava-debug-value''> - </span></li>
                            <li><span class=''lava-debug-key''>ForeignGuid</span> <span class=''lava-debug-value''> - </span></li>
                            <li><span class=''lava-debug-key''>ForeignKey</span> <span class=''lava-debug-value''> - </span></li>
                        </ul>
                    </li>
                    <li><span class=''lava-debug-key''>LocationSelectionMode</span> <span class=''lava-debug-value''> - None</span></li>
                    <li><span class=''lava-debug-key''>EnableLocationSchedules</span> <span class=''lava-debug-value''> - True</span></li>
                    <li><span class=''lava-debug-key''>EnableAlternatePlacements</span> <span class=''lava-debug-value''> - False</span></li>
                    <li><span class=''lava-debug-key''>GroupTypePurposeValueId</span> <span class=''lava-debug-value''> - </span></li>
                    <li><span class=''lava-debug-key''>GroupTypePurposeValue</span> <span class=''lava-debug-value''> - </span></li>
                    <li>
                        <span class=''lava-debug-key lava-debug-section level-1''>Roles</span>
                        {<ul>
                            <li>[0] <span class=''lava-debug-value''> - Member</span></li>
                        </ul>}
                    </li>
                    <li>
                        <span class=''lava-debug-key lava-debug-section level-1''>GroupScheduleExclusions</span>
                        {<ul></ul>}
                    </li>
                    <li>
                        <span class=''lava-debug-key lava-debug-section level-1''>ChildGroupTypes</span>
                        {<ul></ul>}
                    </li>
                    <li>
                        <span class=''lava-debug-key lava-debug-section level-1''>ParentGroupTypes</span>
                        {<ul>
                            <li>
                                [0]
                                <ul>
                                    <li><span class=''lava-debug-key''>IsSystem</span> <span class=''lava-debug-value''> - False</span></li>
                                    <li><span class=''lava-debug-key''>Name</span> <span class=''lava-debug-value''> - Weekly Service Check-in Area</span></li>
                                    <li><span class=''lava-debug-key''>Description</span> <span class=''lava-debug-value''> - </span></li>
                                    <li><span class=''lava-debug-key''>GroupTerm</span> <span class=''lava-debug-value''> - Group</span></li>
                                    <li><span class=''lava-debug-key''>GroupMemberTerm</span> <span class=''lava-debug-value''> - Member</span></li>
                                    <li><span class=''lava-debug-key''>DefaultGroupRoleId</span> <span class=''lava-debug-value''> - 33</span></li>
                                    <li><span class=''lava-debug-key''>AllowedScheduleTypes</span> <span class=''lava-debug-value''> - None</span></li>
                                    <li><span class=''lava-debug-key''>AllowMultipleLocations</span> <span class=''lava-debug-value''> - False</span></li>
                                    <li><span class=''lava-debug-key''>ShowInGroupList</span> <span class=''lava-debug-value''> - False</span></li>
                                    <li><span class=''lava-debug-key''>ShowInNavigation</span> <span class=''lava-debug-value''> - False</span></li>
                                    <li><span class=''lava-debug-key''>IconCssClass</span> <span class=''lava-debug-value''> - fa fa-child</span></li>
                                    <li><span class=''lava-debug-key''>TakesAttendance</span> <span class=''lava-debug-value''> - False</span></li>
                                    <li><span class=''lava-debug-key''>SendAttendanceReminder</span> <span class=''lava-debug-value''> - False</span></li>
                                    <li><span class=''lava-debug-key''>AttendanceRule</span> <span class=''lava-debug-value''> - None</span></li>
                                    <li><span class=''lava-debug-key''>AttendancePrintTo</span> <span class=''lava-debug-value''> - Default</span></li>
                                    <li><span class=''lava-debug-key''>Order</span> <span class=''lava-debug-value''> - 0</span></li>
                                    <li><span class=''lava-debug-key''>InheritedGroupTypeId</span> <span class=''lava-debug-value''> - </span></li>
                                    <li><span class=''lava-debug-key''>InheritedGroupType</span> <span class=''lava-debug-value''> - </span></li>
                                    <li><span class=''lava-debug-key''>LocationSelectionMode</span> <span class=''lava-debug-value''> - None</span></li>
                                    <li><span class=''lava-debug-key''>EnableLocationSchedules</span> <span class=''lava-debug-value''> - </span></li>
                                    <li><span class=''lava-debug-key''>EnableAlternatePlacements</span> <span class=''lava-debug-value''> - False</span></li>
                                    <li><span class=''lava-debug-key''>GroupTypePurposeValueId</span> <span class=''lava-debug-value''> - 142</span></li>
                                    <li>
                                        <span class=''lava-debug-key lava-debug-section level-1''>GroupTypePurposeValue</span>
                                        <ul>
                                            <li><span class=''lava-debug-key''>IsSystem</span> <span class=''lava-debug-value''> - True</span></li>
                                            <li><span class=''lava-debug-key''>DefinedTypeId</span> <span class=''lava-debug-value''> - 31</span></li>
                                            <li><span class=''lava-debug-key''>Order</span> <span class=''lava-debug-value''> - 0</span></li>
                                            <li><span class=''lava-debug-key''>Value</span> <span class=''lava-debug-value''> - Check-in Template</span></li>
                                            <li><span class=''lava-debug-key''>Description</span> <span class=''lava-debug-value''> - A Group Type where the purpose is for check-in...</span></li>
                                            <li>
                                                <span class=''lava-debug-key lava-debug-section level-2''>DefinedType</span>
                                                <ul>
                                                    <li><span class=''lava-debug-key''>IsSystem</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>FieldTypeId</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>Order</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>CategoryId</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>Name</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>Description</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>Category</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>FieldType</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>DefinedValues</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>Id</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>Guid</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>ForeignId</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>ForeignGuid</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>ForeignKey</span> <span class=''lava-debug-value''> - </span></li>
                                                </ul>
                                            </li>
                                            <li><span class=''lava-debug-key''>Id</span> <span class=''lava-debug-value''> - 142</span></li>
                                            <li><span class=''lava-debug-key''>Guid</span> <span class=''lava-debug-value''> - 4a406cb0-495b-4795-b788-52bdfde00b01</span></li>
                                            <li><span class=''lava-debug-key''>ForeignId</span> <span class=''lava-debug-value''> - </span></li>
                                            <li><span class=''lava-debug-key''>ForeignGuid</span> <span class=''lava-debug-value''> - </span></li>
                                            <li><span class=''lava-debug-key''>ForeignKey</span> <span class=''lava-debug-value''> - </span></li>
                                        </ul>
                                    </li>
                                    <li>
                                        <span class=''lava-debug-key lava-debug-section level-1''>Roles</span>
                                        {<ul>
                                            <li>[0] <span class=''lava-debug-value''> - Member</span></li>
                                        </ul>}
                                    </li>
                                    <li>
                                        <span class=''lava-debug-key lava-debug-section level-1''>GroupScheduleExclusions</span>
                                        {<ul></ul>}
                                    </li>
                                    <li>
                                        <span class=''lava-debug-key lava-debug-section level-1''>ChildGroupTypes</span>
                                        {<ul>
                                            <li>
                                                [0]
                                                <ul>
                                                    <li><span class=''lava-debug-key''>IsSystem</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>Name</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>Description</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>GroupTerm</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>GroupMemberTerm</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>DefaultGroupRoleId</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>AllowedScheduleTypes</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>AllowMultipleLocations</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>ShowInGroupList</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>ShowInNavigation</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>IconCssClass</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>TakesAttendance</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>SendAttendanceReminder</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>AttendanceRule</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>AttendancePrintTo</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>Order</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>InheritedGroupTypeId</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>InheritedGroupType</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>LocationSelectionMode</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>EnableLocationSchedules</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>EnableAlternatePlacements</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>GroupTypePurposeValueId</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>GroupTypePurposeValue</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>Roles</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>GroupScheduleExclusions</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>ChildGroupTypes</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>ParentGroupTypes</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>LocationTypeValues</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>Id</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>Guid</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>ForeignId</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>ForeignGuid</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>ForeignKey</span> <span class=''lava-debug-value''> - </span></li>
                                                </ul>
                                            </li>
                                            <li>
                                                [1]
                                                <ul>
                                                    <li><span class=''lava-debug-key''>IsSystem</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>Name</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>Description</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>GroupTerm</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>GroupMemberTerm</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>DefaultGroupRoleId</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>AllowedScheduleTypes</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>AllowMultipleLocations</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>ShowInGroupList</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>ShowInNavigation</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>IconCssClass</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>TakesAttendance</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>SendAttendanceReminder</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>AttendanceRule</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>AttendancePrintTo</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>Order</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>InheritedGroupTypeId</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>InheritedGroupType</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>LocationSelectionMode</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>EnableLocationSchedules</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>EnableAlternatePlacements</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>GroupTypePurposeValueId</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>GroupTypePurposeValue</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>Roles</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>GroupScheduleExclusions</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>ChildGroupTypes</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>ParentGroupTypes</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>LocationTypeValues</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>Id</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>Guid</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>ForeignId</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>ForeignGuid</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>ForeignKey</span> <span class=''lava-debug-value''> - </span></li>
                                                </ul>
                                            </li>
                                            <li>[2] <span class=''lava-debug-value''> - ...</span></li>
                                        </ul>}
                                    </li>
                                    <li>
                                        <span class=''lava-debug-key lava-debug-section level-1''>ParentGroupTypes</span>
                                        {<ul></ul>}
                                    </li>
                                    <li>
                                        <span class=''lava-debug-key lava-debug-section level-1''>LocationTypeValues</span>
                                        {<ul></ul>}
                                    </li>
                                    <li><span class=''lava-debug-key''>Id</span> <span class=''lava-debug-value''> - 14</span></li>
                                    <li><span class=''lava-debug-key''>Guid</span> <span class=''lava-debug-value''> - fedd389a-616f-4a53-906c-63d8255631c5</span></li>
                                    <li><span class=''lava-debug-key''>ForeignId</span> <span class=''lava-debug-value''> - </span></li>
                                    <li><span class=''lava-debug-key''>ForeignGuid</span> <span class=''lava-debug-value''> - </span></li>
                                    <li><span class=''lava-debug-key''>ForeignKey</span> <span class=''lava-debug-value''> - </span></li>
                                    <li>
                                        <span class=''lava-debug-key lava-debug-section level-1''>Attributes <p class=''attributes''>Below is a list of attributes that can be retrieved using <code>{{ GroupType.ParentGroupTypes | Attribute:''[AttributeKey]'' }}</code>.</p></span>
                                        <ul>
                                            <li><span class=''lava-debug-key''>core_checkin_CheckInType</span> <span class=''lava-debug-value''> - Family</span></li>
                                            <li><span class=''lava-debug-key''>core_checkin_EnableManagerOption</span> <span class=''lava-debug-value''> - Yes</span></li>
                                            <li><span class=''lava-debug-key''>core_checkin_EnableOverride</span> <span class=''lava-debug-value''> - Yes</span></li>
                                            <li><span class=''lava-debug-key''>core_checkin_SecurityCodeLength</span> <span class=''lava-debug-value''> - 3</span></li>
                                            <li><span class=''lava-debug-key''>core_checkin_ReuseSameCode</span> <span class=''lava-debug-value''> - No</span></li>
                                            <li><span class=''lava-debug-key''>core_checkin_UseSameOptions</span> <span class=''lava-debug-value''> - Yes</span></li>
                                            <li><span class=''lava-debug-key''>core_checkin_SearchType</span> <span class=''lava-debug-value''> - Phone Number</span></li>
                                            <li><span class=''lava-debug-key lava-debug-section level-2''>core_checkin_RegularExpressionFilter</span> </li>
                                            <li><span class=''lava-debug-key''>core_checkin_MaxSearchResults</span> <span class=''lava-debug-value''> - 100</span></li>
                                            <li><span class=''lava-debug-key''>core_checkin_MinimumPhoneSearchLength</span> <span class=''lava-debug-value''> - 4</span></li>
                                            <li><span class=''lava-debug-key''>core_checkin_MaximumPhoneSearchLength</span> <span class=''lava-debug-value''> - 10</span></li>
                                            <li><span class=''lava-debug-key''>core_checkin_PhoneSearchType</span> <span class=''lava-debug-value''> - Ends With</span></li>
                                            <li><span class=''lava-debug-key''>core_checkin_RefreshInterval</span> <span class=''lava-debug-value''> - 10</span></li>
                                            <li><span class=''lava-debug-key''>core_checkin_AgeRequired</span> <span class=''lava-debug-value''> - Yes</span></li>
                                            <li><span class=''lava-debug-key''>core_checkin_DisplayLocationCount</span> <span class=''lava-debug-value''> - Yes</span></li>
                                            <li><span class=''lava-debug-key''>core_checkin_AutoSelectDaysBack</span> <span class=''lava-debug-value''> - 10</span></li>
                                        </ul>
                                    </li>
                                </ul>
                            </li>
                        </ul>}
                    </li>
                    <li>
                        <span class=''lava-debug-key lava-debug-section level-1''>LocationTypeValues</span>
                        {<ul></ul>}
                    </li>
                    <li><span class=''lava-debug-key''>Id</span> <span class=''lava-debug-value''> - 20</span></li>
                    <li><span class=''lava-debug-key''>Guid</span> <span class=''lava-debug-value''> - e3c8f7d6-5ceb-43bb-802f-66c3e734049e</span></li>
                    <li><span class=''lava-debug-key''>ForeignId</span> <span class=''lava-debug-value''> - </span></li>
                    <li><span class=''lava-debug-key''>ForeignGuid</span> <span class=''lava-debug-value''> - </span></li>
                    <li><span class=''lava-debug-key''>ForeignKey</span> <span class=''lava-debug-value''> - </span></li>
                </ul>
            </div>
        </div>
    </div><div class=''panel panel-default panel-lavadebug''>
        <div class=''panel-heading clearfix collapsed'' data-toggle=''collapse'' data-target=''#collapse-f20faa42-181f-4d44-84fe-118c530d4db1''><h5 class=''panel-title pull-left''>Group Members</h5> <div class=''pull-right''><i class=''fa fa-chevron-up''></i></div></div><div id=''collapse-f20faa42-181f-4d44-84fe-118c530d4db1'' class=''panel-collapse collapse''>
            <div class=''panel-body''>
                <p>GroupMembers properties can be accessed by <code>{% for groupmember in GroupMembers %}{{ groupmember.[PropertyKey] }}{% endfor %}</code>.</p>
                {<ul></ul>}
            </div>
        </div>
    </div></div>


</div>
'
WHERE [GUID] = 'E4D289A9-70FA-4381-913E-2A757AD11147'
