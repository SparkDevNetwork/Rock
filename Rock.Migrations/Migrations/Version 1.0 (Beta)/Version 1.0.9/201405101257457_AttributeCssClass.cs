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
    public partial class AttributeCssClass : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            AddColumn("dbo.Attribute", "IconCssClass", c => c.String());

            RockMigrationHelper.UpdateEntityType( "Rock.Model.Person", "72657ED8-D16E-492E-AC12-144C5E7567E7", true, true );
            RockMigrationHelper.UpdateCategory( "72657ED8-D16E-492E-AC12-144C5E7567E7", "Social Media", "fa fa-users", "A person's social media identifiers", "DD8F467D-B83C-444F-B04C-C681167046A1" );
            RockMigrationHelper.UpdatePersonAttribute( "9C204CD0-1233-41C5-818A-C5DA439445AA", "DD8F467D-B83C-444F-B04C-C681167046A1", "Facebook", "Facebook", "fa fa-fw fa-facebook", "Link to person's Facebook profile page", 0, "", "2B8A03D3-B7DC-4DA3-A31E-826D655435D5" );
            RockMigrationHelper.UpdatePersonAttribute( "9C204CD0-1233-41C5-818A-C5DA439445AA", "DD8F467D-B83C-444F-B04C-C681167046A1", "Twitter", "Twitter", "fa fa-fw fa-twitter", "Link to person's Twitter page", 1, "", "12E9C8A7-03E4-472D-9E20-9EC8F3453B2F" );
            RockMigrationHelper.UpdatePersonAttribute( "9C204CD0-1233-41C5-818A-C5DA439445AA", "DD8F467D-B83C-444F-B04C-C681167046A1", "Instagram", "Instagram", "fa fa-fw fa-instagram", "Link to person's Instagram page", 2, "", "8796567C-4047-43C1-AF32-2FDBE030BEAC" );

            // Add Block to Page: Extended Attributes, Site: Rock RMS
            RockMigrationHelper.AddBlock( "1C737278-4CBA-404B-B6B3-E3F0E05AB5FE", "", "D70A59DC-16BE-43BE-9880-59598FA7A94C", "Attribute Values", "SectionB1", "", "", 1, "DCA9E640-B5EA-4C73-90BC-4A91330528D5" );
            // Attrib Value for Block:Attribute Values, Attribute:Category Page: Extended Attributes, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "DCA9E640-B5EA-4C73-90BC-4A91330528D5", "EC43CF32-3BDF-4544-8B6A-CE9208DD7C81", @"DD8F467D-B83C-444F-B04C-C681167046A1" );

            Sql( @"
    UPDATE [Attribute] SET 
        [DefaultValue] = 'Invalid Password. Password must be at least 6 characters long and can only contain letters and/or numbers'
    WHERE [Guid] = 'B35C04BE-8F40-4478-B263-C5A7844D5FBA'
    AND [ModifiedDateTime] IS NULL
" );

            Sql( @"
    -----------------------------------------------------------------------------------------------
    -- START script for Report (No Longer Attending) and DataView (Self Inactivated)
    -----------------------------------------------------------------------------------------------
	
	DECLARE @ReportEntityTypeId INT
	SET @ReportEntityTypeId = (SELECT [Id] FROM [EntityType] WHERE [Guid] = 'F1F22D3E-FEFA-4C84-9FFA-9E8ACE60FCE7')

	-- add Organization category for Reports
	INSERT INTO [Category]
			([IsSystem]
			,[ParentCategoryId]
			,[EntityTypeId]
			,[EntityTypeQualifierColumn]
			,[EntityTypeQualifierValue]
			,[Name]
			,[IconCssClass]
			,[Guid]
			,[Order]
			,[Description]
			,[CreatedDateTime]
			,[ModifiedDateTime])
	VALUES
			(1
			,NULL
			,@ReportEntityTypeId
			,N''
			,N''
			,N'Organization'
			,N'fa fa-building-o'
			,'b88e45fc-c4f8-487f-ab16-9e30157da967'
			,0
			,NULL
			,'2014-05-01 10:51:46.623'
			,'2014-05-01 10:51:46.623')

    -----------------------------------------------------------------------------------------------
    -- START script for DataView: Self Inactivated
    -----------------------------------------------------------------------------------------------
    
    DECLARE @PropertyFilterEntityTypeId INT
    SET @PropertyFilterEntityTypeId = (SELECT [Id] FROM [EntityType] WHERE [Guid] = '03F0D6AC-D181-48B6-B4BC-1F2652B55323')

    -- parent DataViewFilter for DataView: Self Inactivated
    DECLARE @ParentDataViewFilterId INT
    INSERT INTO [DataViewFilter]
        ([ExpressionType]
        ,[Guid]
        ,[CreatedDateTime]
        ,[ModifiedDateTime])
    VALUES
        (1
        ,'f1544655-2443-4b42-a750-d609aeddaac8'
        ,'5/1/2014 9:29:00 AM'
        ,'5/1/2014 9:29:00 AM')
    SET @ParentDataViewFilterId = SCOPE_IDENTITY()

    
        -- child DataViewFilters for DataView: Self Inactivated
        INSERT INTO [DataViewFilter]
            ([ExpressionType]
            ,[ParentId]
            ,[EntityTypeId]
            ,[Selection]
            ,[Guid]
            ,[CreatedDateTime]
            ,[ModifiedDateTime])
        VALUES
            (0
            ,@ParentDataViewFilterId
            ,@PropertyFilterEntityTypeId
            ,N'[
  ""RecordStatusValueId"",
  ""[\r\n  \""1DAD99D5-41A9-4865-8366-F269902B80A4\""\r\n]""
]'
            ,'83c592bb-c62a-41d8-a64a-4ec2c1df9bc0'
            ,'5/1/2014 9:29:00 AM'
            ,'5/1/2014 9:29:00 AM')
            
        -- child DataViewFilters for DataView: Self Inactivated
        INSERT INTO [DataViewFilter]
            ([ExpressionType]
            ,[ParentId]
            ,[EntityTypeId]
            ,[Selection]
            ,[Guid]
            ,[CreatedDateTime]
            ,[ModifiedDateTime])
        VALUES
            (0
            ,@ParentDataViewFilterId
            ,@PropertyFilterEntityTypeId
            ,N'[
  ""ReviewReasonValueId"",
  ""[\r\n  \""D539C356-6856-4E94-80B4-8FEA869AF38B\""\r\n]""
]'
            ,'7694c7de-b5b8-40ec-aff8-793ac67abaaf'
            ,'5/1/2014 9:29:00 AM'
            ,'5/1/2014 9:29:00 AM')
            
    -- add DataView: Self Inactivated
    DECLARE @CategoryId INT
    SET @CategoryId = (SELECT [Id] FROM [Category] WHERE [Guid] = 'bdd2c36f-7575-48a8-8b70-3a566e3811ed')

    DECLARE @DataViewId INT
    INSERT INTO [DataView]
        ([IsSystem]
        ,[Name]
        ,[Description]
        ,[CategoryId]
        ,[EntityTypeId]
        ,[DataViewFilterId]
        ,[TransformEntityTypeId]
        ,[Guid]
        ,[CreatedDateTime]
        ,[ModifiedDateTime])
    VALUES
        (1
        ,N'Self Inactivated'
        ,N'People who are Inactive and have a Review Reason of ''self inactivated'''
        ,@CategoryId
        ,15
        ,@ParentDataViewFilterId
        ,NULL
        ,'6296b6ee-10e4-4bb7-8565-1268cae7969f'
        ,'5/1/2014 9:29:00 AM'
        ,'5/1/2014 9:29:00 AM')
    SET @DataViewId = SCOPE_IDENTITY()
    
    -----------------------------------------------------------------------------------------------
    -- END script for DataView: Self Inactivated
    -----------------------------------------------------------------------------------------------
  
    -----------------------------------------------------------------------------------------------
    -- START script for Report: No Longer Attending
    -----------------------------------------------------------------------------------------------

        -- add Report: No Longer Attending
        DECLARE @CategoryId_1 INT
        SET @CategoryId_1 = (SELECT [Id] FROM [Category] WHERE [Guid] = 'b88e45fc-c4f8-487f-ab16-9e30157da967')

        DECLARE @DataViewId_1 INT
        SET @DataViewId_1 = (SELECT [Id] FROM [DataView] WHERE [Guid] = '6296b6ee-10e4-4bb7-8565-1268cae7969f')

        DECLARE @ReportId_1 INT
        INSERT INTO [Report]
            ([IsSystem]
            ,[Name]
            ,[Description]
            ,[CategoryId]
            ,[EntityTypeId]
            ,[DataViewId]
            ,[Guid]
            ,[CreatedDateTime]
            ,[ModifiedDateTime]
            ,[FetchTop])
        VALUES
            (0
            ,N'No Longer Attending'
            ,N'People who have marked themselves as not part of the church.'
            ,@CategoryId_1
            ,15
            ,@DataViewId_1
            ,'87d3e118-ada8-4424-b63b-9482a7d9e609'
            ,'4/29/2014 1:38:11 PM'
            ,'4/29/2014 1:38:11 PM'
            ,500)
        SET @ReportId_1 = SCOPE_IDENTITY()
        
        -- add ReportFields for Report: No Longer Attending
        INSERT INTO [ReportField]
            ([ReportId]
            ,[ReportFieldType]
            ,[ShowInGrid]
            ,[DataSelectComponentEntityTypeId]
            ,[Selection]
            ,[Order]
            ,[Guid]
            ,[ColumnHeaderText]
            ,[CreatedDateTime]
            ,[ModifiedDateTime])
        VALUES
            (@ReportId_1
            ,0
            ,1
            ,NULL
            ,N'NickName'
            ,0
            ,'6113132a-bd29-4e25-8c6d-22bac0fd795b'
            ,N'Nick Name'
            ,'4/29/2014 3:47:20 PM'
            ,'4/29/2014 3:47:20 PM')
            
        -- add ReportFields for Report: No Longer Attending
        INSERT INTO [ReportField]
            ([ReportId]
            ,[ReportFieldType]
            ,[ShowInGrid]
            ,[DataSelectComponentEntityTypeId]
            ,[Selection]
            ,[Order]
            ,[Guid]
            ,[ColumnHeaderText]
            ,[CreatedDateTime]
            ,[ModifiedDateTime])
        VALUES
            (@ReportId_1
            ,0
            ,1
            ,NULL
            ,N'LastName'
            ,1
            ,'c1b78a6c-eb40-4eaa-be75-c6de8c00629c'
            ,N'Last Name'
            ,'4/29/2014 3:47:20 PM'
            ,'4/29/2014 3:47:20 PM')
            
        -- add ReportFields for Report: No Longer Attending
        INSERT INTO [ReportField]
            ([ReportId]
            ,[ReportFieldType]
            ,[ShowInGrid]
            ,[DataSelectComponentEntityTypeId]
            ,[Selection]
            ,[Order]
            ,[Guid]
            ,[ColumnHeaderText]
            ,[CreatedDateTime]
            ,[ModifiedDateTime])
        VALUES
            (@ReportId_1
            ,0
            ,1
            ,NULL
            ,N'ReviewReasonNote'
            ,2
            ,'29f09daa-9d05-4e6f-9f3d-814dbc1d18e4'
            ,N'Review Reason Note'
            ,'4/29/2014 3:47:20 PM'
            ,'4/29/2014 3:47:20 PM')
            
        -- add ReportFields for Report: No Longer Attending
        INSERT INTO [ReportField]
            ([ReportId]
            ,[ReportFieldType]
            ,[ShowInGrid]
            ,[DataSelectComponentEntityTypeId]
            ,[Selection]
            ,[Order]
            ,[Guid]
            ,[ColumnHeaderText]
            ,[CreatedDateTime]
            ,[ModifiedDateTime])
        VALUES
            (@ReportId_1
            ,0
            ,1
            ,NULL
            ,N'InactiveReasonNote'
            ,3
            ,'65382c7a-44b2-4049-8dc8-8ada9edcf231'
            ,N'Inactive Reason Note'
            ,'4/29/2014 3:47:20 PM'
            ,'4/29/2014 3:47:20 PM')
            
        -- add ReportFields for Report: No Longer Attending
        INSERT INTO [ReportField]
            ([ReportId]
            ,[ReportFieldType]
            ,[ShowInGrid]
            ,[DataSelectComponentEntityTypeId]
            ,[Selection]
            ,[Order]
            ,[Guid]
            ,[ColumnHeaderText]
            ,[CreatedDateTime]
            ,[ModifiedDateTime])
        VALUES
            (@ReportId_1
            ,0
            ,1
            ,NULL
            ,N'RecordStatusReasonValueId'
            ,4
            ,'fc9e7bda-93e8-491e-b7a1-e7b2e1fe712a'
            ,N'Inactive Record Reason'
            ,'4/29/2014 3:47:20 PM'
            ,'4/29/2014 3:47:20 PM')
      
      -----------------------------------------------------------------------------------------------
      -- END script for Report: No Longer Attending
      -----------------------------------------------------------------------------------------------
" );
            
            // Change Data filters to use guids
            Sql( @"
			-- record status: active
			UPDATE [DataViewFilter]
			SET [Selection] = '[    ""RecordStatusValueId"",    ""[\r\n  \""618f906c-c33d-4fa3-8aef-e58cb7b63f1e\""\r\n]""  ]'
			WHERE [Guid] = '01A3855A-39B8-42FF-B469-3A0F851C32E3'
			
			-- connection status: member
			UPDATE [DataViewFilter]
			SET [Selection] = '[    ""ConnectionStatusValueId"",    ""[\r\n  \""41540783-d9ef-4c70-8f1d-c9e83d91ed5f\""\r\n]""  ]'
			WHERE [Guid] = 'FDF47D1E-AF28-46BA-97E6-67138E582AE5'
			
			-- record status: pending
			UPDATE [DataViewFilter]
			SET [Selection] = '[    ""RecordStatusValueId"",    ""[\r\n  \""283999ec-7346-42e3-b807-bce9b2babb49\""\r\n]""  ]'
			WHERE [Guid] = 'A3E1EB8E-CB2A-4EE0-9A27-3499392D5A1D'
" );

            Sql( @"
    /* 
    Migration to set up check-in labels. 
    */ 
    -- update existing Label Merge field name, order or descriptions (defined values)
    UPDATE [DefinedValue] 
    SET    [Description] = N'The security code generated during check-in.' 
    WHERE  [Guid] = '9DCB69C4-1B9B-4D31-B8B6-8C9DBFA9933B' 

    UPDATE [DefinedValue] 
    SET    [Name] = N'Nick Name' 
           ,[Description] = N'The nick-name of the person who checked in.' 
    WHERE  [Guid] = 'E2DB40F4-A064-429E-9A76-C520E7E7A43A' 

    UPDATE [DefinedValue] 
    SET    [Description] = N'The last name of the person who checked in.' 
    WHERE  [Guid] = '85B71255-19F9-4443-A7AE-EF670385DC71' 

    UPDATE [DefinedValue] 
    SET    [Order] = 6 
           ,[Name] = N'Birthday Icon' 
           ,[Description] = N'An upcoming birthday indicator.' 
    WHERE  [Guid] = 'C6AA76B5-3E7F-4E14-905E-36173E60949D' 

    UPDATE [DefinedValue] 
    SET    [Description] = N'The schedule(s) the person checked in to.' 
    WHERE  [Guid] = '18B24BF8-26DD-43BB-A54D-8B10C57EA740' 

    -- add the 8 new merge fields 
    INSERT INTO [DefinedValue] 
	    ([IsSystem] 
	    ,[DefinedTypeId] 
	    ,[Order] 
	    ,[Name] 
	    ,[Description] 
	    ,[Guid]) 
    VALUES
	    (1 
	    ,19 
	    ,3 
	    ,N'Full Name' 
	    ,N'The full name of the person who checked in.' 
	    ,'0cdce2cd-da27-4bb8-a799-db780674b00e') 

    INSERT INTO [DefinedValue] 
	    ([IsSystem] 
	    ,[DefinedTypeId] 
	    ,[Order] 
	    ,[Name] 
	    ,[Description] 
	    ,[Guid]) 
    VALUES
	    (1 
	    ,19 
	    ,5 
	    ,N'Locations' 
	    ,N'The locations the person checked in to.' 
	    ,'13e9155c-b153-454b-b3b4-6687767c9400') 

    INSERT INTO [DefinedValue] 
	    ([IsSystem] 
	    ,[DefinedTypeId] 
	    ,[Order] 
	    ,[Name] 
	    ,[Description] 
	    ,[Guid]) 
    VALUES
	    (1 
	    ,19 
	    ,7 
	    ,N'Birthday Day of Week' 
	    ,N'The day of the week of the child''s birthday (e.g. Wed.).' 
	    ,'604cb370-636b-43d2-9674-b7ce8a09eafa') 

    INSERT INTO [DefinedValue] 
	    ([IsSystem] 
	     ,[DefinedTypeId] 
	     ,[Order] 
	     ,[Name] 
	     ,[Description] 
	     ,[Guid]) 
    VALUES
	    (1 
	    ,19 
	    ,8 
	    ,N'Legal Icon' 
	    ,N'Displays ''L'' if legal notes exist.' 
	    ,'bd31d356-0b9f-453c-a9dd-e066db2dab3c') 

    INSERT INTO [DefinedValue] 
	    ([IsSystem] 
	     ,[DefinedTypeId] 
	     ,[Order] 
	     ,[Name] 
	     ,[Description] 
	     ,[Guid]) 
    VALUES 
	    (1 
	    ,19 
	    ,9 
	    ,N'Legal Note' 
	    ,N'The legal note for the person who checked in.' 
	    ,'94cc0022-d324-4492-81a3-30e1beb1c85a') 

    INSERT INTO [DefinedValue] 
	    ([IsSystem] 
	     ,[DefinedTypeId] 
	     ,[Order] 
	     ,[Name] 
	     ,[Description] 
	     ,[Guid]) 
    VALUES
	    (1 
	    ,19 
	    ,10 
	    ,N'Allergy Icon' 
	    ,N'Displays ''A'' if allergies exist.' 
	    ,'cd131071-28b1-440d-afc6-aaa998b59bf9') 

    INSERT INTO [DefinedValue] 
	    ([IsSystem] 
	     ,[DefinedTypeId] 
	     ,[Order] 
	     ,[Name] 
	     ,[Description] 
	     ,[Guid]) 
    VALUES
	    (1 
	    ,19 
	    ,11 
	    ,N'Allergy Note' 
	    ,N'Allergy note for the person who checked in.' 
	    ,'a1e7ecbd-6248-42ba-8844-2378ea2b9f0e') 

    INSERT INTO [DefinedValue] 
	    ([IsSystem] 
	     ,[DefinedTypeId] 
	     ,[Order] 
	     ,[Name] 
	     ,[Description] 
	     ,[Guid]) 
    VALUES
	    (1 
	    ,19 
	    ,12 
	    ,N'First-time Icon' 
	    ,N'Displays ''F'' if it''s the person''s first visit.' 
	    ,'bd1b1fea-d4a9-45eb-9958-01ef75d5a949') 

    --update existing Merge Field attribute values 
    UPDATE [AttributeValue] 
    SET    [Value] = N'{{ Person.NickName }}' 
    WHERE  [Guid] = '240B5E6D-CE63-439D-BB40-3A8A3EC4F99E' 

    UPDATE [AttributeValue] 
    SET    [Value] = N'{{ Person.LastName }}' 
    WHERE  [Guid] = 'EE773F9D-D5E5-408D-9315-75A48F775053' 

    UPDATE [AttributeValue] 
    SET    [Value] = N'{% if Person.DaysToBirthday <= 7 %}B{% endif %}' 
    WHERE  [Guid] = '0BFB850D-920C-415E-9684-086DAB950731' 

    UPDATE [AttributeValue] 
    SET    [Value] = N'{% for group in GroupType.Groups %}{% for location in group.Locations %}{% for schedule in location.Schedules %}{{schedule.Name}}{% endfor %}{% endfor %}{% endfor %}' 
    WHERE  [Guid] = '0E5AD48B-4B80-48C5-8F38-1D9C7F6532BC' 

    -- add the ZPL files and data for the four labels 
    DECLARE @ChildLabelFileId INT 
    DECLARE @NameTagLabelFileId INT 
    DECLARE @NoteLabelFileId INT 
    DECLARE @ParentLabelFileId INT 

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
	    ,N'Child Label (Text)' 
	    ,N'text/plain' 
	    ,N'Text label for attaching to the child.' 
	    ,NULL 
	    ,'1b31a2a3-6438-4557-8788-7f057a025eaa') 

    SET @ChildLabelFileId = SCOPE_IDENTITY() 

    INSERT INTO [BinaryFileData] 
	    ([Id] 
	    ,[Content] 
	    ,[Guid]) 
    VALUES
	    (@ChildLabelFileId 
	    ,0x0a3c212d2d2073617665642066726f6d2075726c3d283030373629687474703a2f2f6c6f63616c686f73743a363232392f47657446696c652e617368783f677569643d66393865396236312d336662342d343366342d386333302d346565636432663534393637202d2d3e0a3c68746d6c3e3c686561643e3c6d65746120687474702d65717569763d22436f6e74656e742d547970652220636f6e74656e743d22746578742f68746d6c3b20636861727365743d5554462d38223e3c2f686561643e3c626f64793e3c707265207374796c653d22776f72642d777261703a20627265616b2d776f72643b2077686974652d73706163653a207072652d777261703b223e1043547e7e43442c7e43435e7e43547e0a5e58417e54413030307e4a534e5e4c54305e4d4e575e4d54445e504f4e5e504d4e5e4c48302c305e4a4d415e5052362c367e534431355e4a55535e4c524e5e4349305e585a0a5e58410a5e4d4d540a5e50573831320a5e4c4c303430360a5e4c53300a5e46543434332c3131395e41304e2c3133352c3133345e46423333332c312c302c525e46485c5e46445757575e46530a5e465431322c3236385e41304e2c3133352c3134365e46485c5e4644355e46530a5e465431342c3332375e41304e2c34352c34355e46485c5e4644365e46530a5e464f3632362c3334305e474236302c35362c35365e46530a5e46543632362c3338345e41304e2c34352c34355e464237302c312c302c435e46525e46485c5e46444141415e46530a5e464f3731392c3334305e474236302c35362c35365e46530a5e46543731392c3338345e41304e2c34352c34355e464237302c312c302c435e46525e46485c5e46444c4c4c5e46530a5e46543333362c3130335e41304e2c3130322c3130305e46485c5e4644325e46530a5e46543430312c3130335e41304e2c3130322c3130305e46485c5e4644335e46530a5e46543334322c3133305e41304e2c32382c32385e46485c5e4644345e46530a5e46543333382c3338355e41304e2c32382c32385e46485c5e464431305e46530a5e465431332c3338355e41304e2c32382c32385e46485c5e4644395e46530a5e4c52595e464f302c305e47423831322c302c3133365e46535e4c524e0a5e5051312c302c312c595e585a0a3c2f7072653e3c2f626f64793e3c2f68746d6c3e
	    ,'df44734e-473b-4009-9099-7ba25c8ea36a') 

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
	    ,N'Name Tag' 
	    ,N'text/plain' 
	    ,N'Generic name tag for check-in.' 
	    ,NULL 
	    ,'918f55a6-bd4b-4070-9017-d5fc5207653e') 

    SET @NameTagLabelFileId = SCOPE_IDENTITY() 

    INSERT INTO [BinaryFileData] 
	    ([Id] 
	    ,[Content] 
	    ,[Guid]) 
    VALUES
	    (@NameTagLabelFileId 
	    ,0xefbbbf1043547e7e43442c7e43435e7e43547e0d0a5e58417e54413030307e4a534e5e4c54305e4d4e575e4d54445e504f4e5e504d4e5e4c48302c305e4a4d415e5052362c367e534432345e4a55535e4c524e5e4349305e585a0d0a5e58410d0a5e4d4d540d0a5e50573831320d0a5e4c4c303430360d0a5e4c53300d0a5e465431362c3235365e41304e2c3133352c3134365e46485c5e46444e69636b4e616d655e46530d0a5e465431382c3332385e41304e2c35362c35355e46485c5e46444c6173744e616d655e46530d0a5e4c52595e464f302c305e47423831322c302c3132325e46535e4c524e0d0a5e5051312c302c312c595e585a0d0a
	    ,'915f7059-589c-442e-a79d-136dfa320e21') 

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
	    ,N'Note Label' 
	    ,N'text/plain' 
	    ,N'Label that contains legal and allergy notes about the child to be used in a log book.'
	    ,NULL 
	    ,'50e743e2-76c1-428f-8072-e9bc157d0a08') 

    SET @NoteLabelFileId = SCOPE_IDENTITY() 

    INSERT INTO [BinaryFileData] 
	    ([Id] 
	    ,[Content] 
	    ,[Guid]) 
    VALUES
	    (@NoteLabelFileId 
	    ,0xefbbbf1043547e7e43442c7e43435e7e43547e0d0a5e58417e54413030307e4a534e5e4c54305e4d4e575e4d54445e504f4e5e504d4e5e4c48302c305e4a4d415e5052362c367e534431355e4a55535e4c524e5e4349305e585a0d0a5e58410d0a5e4d4d540d0a5e50573831320d0a5e4c4c303430360d0a5e4c53300d0a5e46543630372c36385e41304e2c37332c37325e46423137372c312c302c525e46485c5e46445757575e46530d0a5e4654362c3132325e41304e2c33392c33385e46485c5e4644325e46530d0a5e46543633312c3131385e41304e2c32352c32345e46485c5e4644345e46530d0a5e464f31322c3136315e474234302c34322c34325e46530d0a5e4654382c3139345e41304e2c33342c33335e464235372c312c302c435e46525e46485c5e46444141415e46530d0a5e46543432372c3131385e41304e2c32352c32345e46485c5e4644335e46530d0a5e464f31322c3236345e474234302c34322c34325e46530d0a5e465431322c3239375e41304e2c33342c33335e464234382c312c302c435e46525e46485c5e46444c4c4c5e46530d0a5e46423333302c342c302c4c5e465436382c3235305e41304e2c32332c32345e46485c5e4644355e46530d0a5e46423333302c342c302c4c5e465436382c3335345e41304e2c32332c32345e46485c5e4644375e46530d0a5e46543432302c3137375e41304e2c32332c32345e46485c5e46444e6f7465733a5e46530d0a5e464f3430332c3135345e4742302c3233372c315e46530d0a5e464f3432322c3338365e47423336312c302c315e46530d0a5e464f3432332c3334355e47423336312c302c315e46530d0a5e464f3432312c3330345e47423336312c302c315e46530d0a5e464f3432312c3236335e47423336312c302c315e46530d0a5e464f3432312c3232375e47423336312c302c315e46530d0a5e4c52595e464f302c305e47423831322c302c38315e46535e4c524e0d0a5e5051312c302c312c595e585a0d0a
	    ,'0131f1f8-8ef7-48c8-80c0-0c10aea4fa1a') 

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
	    ,N'Parent Label' 
	    ,N'text/plain' 
	    ,N'Text label for the parent to use to pick-up the child.' 
	    ,NULL 
	    ,'9b098db0-952c-43fb-a5bd-511e3c2b72fb') 

    SET @ParentLabelFileId = SCOPE_IDENTITY() 

    INSERT INTO [BinaryFileData] 
	    ([Id] 
	    ,[Content] 
	    ,[Guid]) 
    VALUES
	    (@ParentLabelFileId 
	    ,0xefbbbf1043547e7e43442c7e43435e7e43547e0d0a5e58417e54413030307e4a534e5e4c54305e4d4e575e4d54445e504f4e5e504d4e5e4c48302c305e4a4d415e5052362c367e534431355e4a55535e4c524e5e4349305e585a0d0a5e58410d0a5e4d4d540d0a5e50573831320d0a5e4c4c303430360d0a5e4c53300d0a5e46543437332c3131385e41304e2c3133352c3133345e46423331302c312c302c525e46485c5e46444d4d4d5e46530d0a5e465431322c3236385e41304e2c3133352c3134365e46485c5e4644325e46530d0a5e465431342c3334325e41304e2c33392c33385e46485c5e46444368696c64205069636b2d757020526563656970745e46530d0a5e465431352c3336395e41304e2c32332c32345e46485c5e4644466f722074686520736166657479206f6620796f7572206368696c642c20796f75206d7573742070726573656e7420746869732072656365697074207768656e207069636b696e675e46530d0a5e465431352c3339335e41304e2c32332c32345e46485c5e4644757020796f7572206368696c642e20496620796f75206c6f7365207468697320706c6561736520736565207468652061726561206469726563746f722e5e46530d0a5e4c52595e464f302c305e47423831322c302c3133365e46535e4c524e0d0a5e5051312c302c312c595e585a0d0a
	    ,'fffce2e5-cba0-48a1-91bd-4705c472dd95') 

    --- add label attributes for the check-in groups (et91=GroupType, ft37=BinaryFile, etqv19-22=Nursery,Elem,JH,HS) 
    --- set the qualifier value for the above attributes. 
    DECLARE @AttributeId INT 

    INSERT INTO [Attribute] 
	    ([IsSystem] 
	    ,[FieldTypeId] 
	    ,[EntityTypeId] 
	    ,[EntityTypeQualifierColumn] 
	    ,[EntityTypeQualifierValue] 
	    ,[Key] 
	    ,[Name] 
	    ,[Description] 
	    ,[Order] 
	    ,[IsGridColumn] 
	    ,[DefaultValue] 
	    ,[IsMultiValue] 
	    ,[IsRequired] 
	    ,[Guid]) 
    VALUES
	    (1 
	    ,37 
	    ,91 
	    ,N'Id' 
	    ,N'18' 
	    ,N'ChildLabel(Text)' 
	    ,N'Child Label (Text)' 
	    ,NULL 
	    ,0 
	    ,0 
	    ,CAST(@ChildLabelFileId AS VARCHAR) 
	    ,0 
	    ,0 
	    ,'22a188b3-a9b2-4173-a884-499758a4bb01') 

    SET @AttributeId = SCOPE_IDENTITY() 

    INSERT INTO [AttributeQualifier] 
	    ([IsSystem] 
	    ,[AttributeId] 
	    ,[Key] 
	    ,[Value] 
	    ,[Guid]) 
    VALUES
	    (1 
	    ,@AttributeId 
	    ,N'binaryFileType' 
	    ,N'1' 
	    ,'3c86024e-6812-429f-901f-02ef35867b6c') 

    INSERT INTO [Attribute] 
	    ([IsSystem] 
	    ,[FieldTypeId] 
	    ,[EntityTypeId] 
	    ,[EntityTypeQualifierColumn] 
	    ,[EntityTypeQualifierValue] 
	    ,[Key] 
	    ,[Name] 
	    ,[Description] 
	    ,[Order] 
	    ,[IsGridColumn] 
	    ,[DefaultValue] 
	    ,[IsMultiValue] 
	    ,[IsRequired] 
	    ,[Guid]) 
    VALUES
	    (1 
	    ,37 
	    ,91 
	    ,N'Id' 
	    ,N'18' 
	    ,N'NoteLabel' 
	    ,N'Note Label' 
	    ,NULL 
	    ,0 
	    ,0 
	    ,CAST(@NoteLabelFileId AS VARCHAR) 
	    ,0 
	    ,0 
	    ,'7f9db622-7e2a-441b-8bc0-2bfefe5345f3') 

    SET @AttributeId = SCOPE_IDENTITY() 

    INSERT INTO [AttributeQualifier] 
	    ([IsSystem] 
	    ,[AttributeId] 
	    ,[Key] 
	    ,[Value] 
	    ,[Guid]) 
    VALUES
	    (1 
	    ,@AttributeId 
	    ,N'binaryFileType' 
	    ,N'1' 
	    ,'6e33504f-1f10-4467-be67-88ece259a649') 

    INSERT INTO [Attribute] 
	    ([IsSystem] 
	    ,[FieldTypeId] 
	    ,[EntityTypeId] 
	    ,[EntityTypeQualifierColumn] 
	    ,[EntityTypeQualifierValue] 
	    ,[Key] 
	    ,[Name] 
	    ,[Description] 
	    ,[Order] 
	    ,[IsGridColumn] 
	    ,[DefaultValue] 
	    ,[IsMultiValue] 
	    ,[IsRequired] 
	    ,[Guid]) 
    VALUES
	    (1 
	    ,37 
	    ,91 
	    ,N'Id' 
	    ,N'18' 
	    ,N'ParentLabel' 
	    ,N'Parent Label' 
	    ,NULL 
	    ,0 
	    ,0 
	    ,CAST(@ParentLabelFileId AS VARCHAR) 
	    ,0 
	    ,0 
	    ,'85fc0774-c926-4d27-86b0-60129db7c984') 

    SET @AttributeId = SCOPE_IDENTITY() 

    INSERT INTO [AttributeQualifier] 
	    ([IsSystem] 
	    ,[AttributeId] 
	    ,[Key] 
	    ,[Value] 
	    ,[Guid]) 
    VALUES
	    (1 
	    ,@AttributeId 
	    ,N'binaryFileType' 
	    ,N'1' 
	    ,'270460d5-87e5-4d31-acdb-89bf71cd7c4e') 

    INSERT INTO [Attribute] 
	    ([IsSystem] 
	    ,[FieldTypeId] 
	    ,[EntityTypeId] 
	    ,[EntityTypeQualifierColumn] 
	    ,[EntityTypeQualifierValue] 
	    ,[Key] 
	    ,[Name] 
	    ,[Description] 
	    ,[Order] 
	    ,[IsGridColumn] 
	    ,[DefaultValue] 
	    ,[IsMultiValue] 
	    ,[IsRequired] 
	    ,[Guid]) 
    VALUES
	    (1 
	    ,37 
	    ,91 
	    ,N'Id' 
	    ,N'19' 
	    ,N'ChildLabel(Text)' 
	    ,N'Child Label (Text)' 
	    ,NULL 
	    ,0 
	    ,0 
	    ,CAST(@ChildLabelFileId AS VARCHAR) 
	    ,0 
	    ,0 
	    ,'7bbd4336-77a0-4d91-9f08-0fc4c456b876') 

    SET @AttributeId = SCOPE_IDENTITY() 

    INSERT INTO [AttributeQualifier] 
	    ([IsSystem] 
	    ,[AttributeId] 
	    ,[Key] 
	    ,[Value] 
	    ,[Guid]) 
    VALUES
	    (1 
	    ,@AttributeId 
	    ,N'binaryFileType' 
	    ,N'1' 
	    ,'538cd702-6b85-4b07-8d57-6478d4871ee7') 

    INSERT INTO [Attribute] 
	    ([IsSystem] 
	    ,[FieldTypeId] 
	    ,[EntityTypeId] 
	    ,[EntityTypeQualifierColumn] 
	    ,[EntityTypeQualifierValue] 
	    ,[Key] 
	    ,[Name] 
	    ,[Description] 
	    ,[Order] 
	    ,[IsGridColumn] 
	    ,[DefaultValue] 
	    ,[IsMultiValue] 
	    ,[IsRequired] 
	    ,[Guid]) 
    VALUES
	    (1 
	    ,37 
	    ,91 
	    ,N'Id' 
	    ,N'19' 
	    ,N'ParentLabel' 
	    ,N'Parent Label' 
	    ,NULL 
	    ,0 
	    ,0 
	    ,CAST(@ParentLabelFileId AS VARCHAR) 
	    ,0 
	    ,0 
	    ,'a445df1d-2d3d-4cef-beca-cbed24c46642') 

    SET @AttributeId = SCOPE_IDENTITY() 

    INSERT INTO [AttributeQualifier] 
	    ([IsSystem] 
	    ,[AttributeId] 
	    ,[Key] 
	    ,[Value] 
	    ,[Guid]) 
    VALUES
	    (0 
	    ,@AttributeId 
	    ,N'binaryFileType' 
	    ,N'1' 
	    ,'afc65f38-7121-48d9-85ef-b6db29ae9578') 
    /*
    INSERT INTO [Attribute] 
	    ([IsSystem] 
	    ,[FieldTypeId] 
	    ,[EntityTypeId] 
	    ,[EntityTypeQualifierColumn] 
	    ,[EntityTypeQualifierValue] 
	    ,[Key] 
	    ,[Name] 
	    ,[Description] 
	    ,[Order] 
	    ,[IsGridColumn] 
	    ,[DefaultValue] 
	    ,[IsMultiValue] 
	    ,[IsRequired] 
	    ,[Guid]) 
    VALUES
	    (1 
	    ,37 
	    ,91 
	    ,N'Id' 
	    ,N'19' 
	    ,N'NameTag' 
	    ,N'Name Tag' 
	    ,NULL 
	    ,0 
	    ,0 
	    ,CAST(@NameTagLabelFileId AS VARCHAR) 
	    ,0 
	    ,0 
	    ,'3494d86f-af58-4dab-bddd-54560a78b1d7') 

    SET @AttributeId = SCOPE_IDENTITY() 

    INSERT INTO [AttributeQualifier] 
	    ([IsSystem] 
	    ,[AttributeId] 
	    ,[Key] 
	    ,[Value] 
	    ,[Guid]) 
    VALUES
	    (0 
	    ,@AttributeId 
	    ,N'binaryFileType' 
	    ,N'1' 
	    ,'eefa1a6e-6e0a-40df-889f-05d468c8f7c8') 
    */

    INSERT INTO [Attribute] 
	    ([IsSystem] 
	    ,[FieldTypeId] 
	    ,[EntityTypeId] 
	    ,[EntityTypeQualifierColumn] 
	    ,[EntityTypeQualifierValue] 
	    ,[Key] 
	    ,[Name] 
	    ,[Description] 
	    ,[Order] 
	    ,[IsGridColumn] 
	    ,[DefaultValue] 
	    ,[IsMultiValue] 
	    ,[IsRequired] 
	    ,[Guid]) 
    VALUES
	    (1 
	    ,37 
	    ,91 
	    ,N'Id' 
	    ,N'19' 
	    ,N'NoteLabel' 
	    ,N'Note Label' 
	    ,NULL 
	    ,0 
	    ,0 
	    ,CAST(@NoteLabelFileId AS VARCHAR) 
	    ,0 
	    ,0 
	    ,'5928becd-7ab2-4939-8c1a-8f1c6e6e06dc') 

    SET @AttributeId = SCOPE_IDENTITY() 

    INSERT INTO [AttributeQualifier] 
	    ([IsSystem] 
	    ,[AttributeId] 
	    ,[Key] 
	    ,[Value] 
	    ,[Guid]) 
    VALUES
	    (0 
	    ,@AttributeId 
	    ,N'binaryFileType' 
	    ,N'1' 
	    ,'df7a57bf-f863-4be3-8365-bb78dd80518e') 

    INSERT INTO [Attribute] 
	    ([IsSystem] 
	    ,[FieldTypeId] 
	    ,[EntityTypeId] 
	    ,[EntityTypeQualifierColumn] 
	    ,[EntityTypeQualifierValue] 
	    ,[Key] 
	    ,[Name] 
	    ,[Description] 
	    ,[Order] 
	    ,[IsGridColumn] 
	    ,[DefaultValue] 
	    ,[IsMultiValue] 
	    ,[IsRequired] 
	    ,[Guid]) 
    VALUES
	    (1 
	    ,37 
	    ,91 
	    ,N'Id' 
	    ,N'20' 
	    ,N'ChildLabel(Text)' 
	    ,N'Child Label (Text)' 
	    ,NULL 
	    ,0 
	    ,0 
	    ,CAST(@ChildLabelFileId AS VARCHAR) 
	    ,0 
	    ,0 
	    ,'0db6af3e-7fb0-43f7-86a5-0af84328ea44') 

    SET @AttributeId = SCOPE_IDENTITY() 

    INSERT INTO [AttributeQualifier] 
	    ([IsSystem] 
	    ,[AttributeId] 
	    ,[Key] 
	    ,[Value] 
	    ,[Guid]) 
    VALUES
	    (0 
	    ,@AttributeId 
	    ,N'binaryFileType' 
	    ,N'1' 
	    ,'089db3ab-854a-4f73-827c-d69999296d10') 
    /*
    INSERT INTO [Attribute] 
	    ([IsSystem] 
	    ,[FieldTypeId] 
	    ,[EntityTypeId] 
	    ,[EntityTypeQualifierColumn] 
	    ,[EntityTypeQualifierValue] 
	    ,[Key] 
	    ,[Name] 
	    ,[Description] 
	    ,[Order] 
	    ,[IsGridColumn] 
	    ,[DefaultValue] 
	    ,[IsMultiValue] 
	    ,[IsRequired] 
	    ,[Guid]) 
    VALUES
	    (1 
	    ,37 
	    ,91 
	    ,N'Id' 
	    ,N'20' 
	    ,N'NameTag' 
	    ,N'Name Tag' 
	    ,NULL 
	    ,0 
	    ,0 
	    ,CAST(@NameTagLabelFileId AS VARCHAR) 
	    ,0 
	    ,0 
	    ,'8bcd8c4e-b73f-4ab2-b2e7-83f8b400592b') 

    SET @AttributeId = SCOPE_IDENTITY() 

    INSERT INTO [AttributeQualifier] 
	    ([IsSystem] 
	    ,[AttributeId] 
	    ,[Key] 
	    ,[Value] 
	    ,[Guid]) 
    VALUES
	    (0 
	    ,@AttributeId 
	    ,N'binaryFileType' 
	    ,N'1' 
	    ,'6630a810-8665-4bca-b4c3-e9dab6258550') 
    */
    INSERT INTO [Attribute] 
	    ([IsSystem] 
	    ,[FieldTypeId] 
	    ,[EntityTypeId] 
	    ,[EntityTypeQualifierColumn] 
	    ,[EntityTypeQualifierValue] 
	    ,[Key] 
	    ,[Name] 
	    ,[Description] 
	    ,[Order] 
	    ,[IsGridColumn] 
	    ,[DefaultValue] 
	    ,[IsMultiValue] 
	    ,[IsRequired] 
	    ,[Guid]) 
    VALUES
	    (1 
	    ,37 
	    ,91 
	    ,N'Id' 
	    ,N'20' 
	    ,N'NoteLabel' 
	    ,N'Note Label' 
	    ,NULL 
	    ,0 
	    ,0 
	    ,CAST(@NoteLabelFileId AS VARCHAR) 
	    ,0 
	    ,0 
	    ,'9162c40b-1115-4143-b38a-b10fc1095b07') 

    SET @AttributeId = SCOPE_IDENTITY() 

    INSERT INTO [AttributeQualifier] 
	    ([IsSystem] 
	    ,[AttributeId] 
	    ,[Key] 
	    ,[Value] 
	    ,[Guid]) 
    VALUES
	    (0 
	    ,@AttributeId 
	    ,N'binaryFileType' 
	    ,N'1' 
	    ,'7dfbbfd1-890a-4dd1-8d73-a5df8c405ad2') 

    INSERT INTO [Attribute] 
	    ([IsSystem] 
	    ,[FieldTypeId] 
	    ,[EntityTypeId] 
	    ,[EntityTypeQualifierColumn] 
	    ,[EntityTypeQualifierValue] 
	    ,[Key] 
	    ,[Name] 
	    ,[Description] 
	    ,[Order] 
	    ,[IsGridColumn] 
	    ,[DefaultValue] 
	    ,[IsMultiValue] 
	    ,[IsRequired] 
	    ,[Guid]) 
    VALUES
	    (1 
	    ,37 
	    ,91 
	    ,N'Id' 
	    ,N'20' 
	    ,N'ParentLabel' 
	    ,N'Parent Label' 
	    ,NULL 
	    ,0 
	    ,0 
	    ,CAST(@ParentLabelFileId AS VARCHAR) 
	    ,0 
	    ,0 
	    ,'93386c97-13d3-468c-a184-6b66e2a79f3b') 

    SET @AttributeId = SCOPE_IDENTITY() 

    INSERT INTO [AttributeQualifier] 
	    ([IsSystem] 
	    ,[AttributeId] 
	    ,[Key] 
	    ,[Value] 
	    ,[Guid]) 
    VALUES
	    (0 
	    ,@AttributeId 
	    ,N'binaryFileType' 
	    ,N'1' 
	    ,'c8c3bede-3eea-4f20-9959-18ad3fbd09ea') 

    INSERT INTO [Attribute] 
	    ([IsSystem] 
	    ,[FieldTypeId] 
	    ,[EntityTypeId] 
	    ,[EntityTypeQualifierColumn] 
	    ,[EntityTypeQualifierValue] 
	    ,[Key] 
	    ,[Name] 
	    ,[Description] 
	    ,[Order] 
	    ,[IsGridColumn] 
	    ,[DefaultValue] 
	    ,[IsMultiValue] 
	    ,[IsRequired] 
	    ,[Guid]) 
    VALUES
	    (1 
	    ,37 
	    ,91 
	    ,N'Id' 
	    ,N'21' 
	    ,N'NameTag' 
	    ,N'Name Tag' 
	    ,NULL 
	    ,0 
	    ,0 
	    ,CAST(@NameTagLabelFileId AS VARCHAR) 
	    ,0 
	    ,0 
	    ,'8f618ac5-8743-43cd-8672-cf0ee49438be') 

    SET @AttributeId = SCOPE_IDENTITY() 

    INSERT INTO [AttributeQualifier] 
	    ([IsSystem] 
	    ,[AttributeId] 
	    ,[Key] 
	    ,[Value] 
	    ,[Guid]) 
    VALUES
	    (0 
	    ,@AttributeId 
	    ,N'binaryFileType' 
	    ,N'1' 
	    ,'f0e30b8b-4f8f-48e0-8e0e-a8e9083a61b1') 

    INSERT INTO [Attribute] 
	    ([IsSystem] 
	    ,[FieldTypeId] 
	    ,[EntityTypeId] 
	    ,[EntityTypeQualifierColumn] 
	    ,[EntityTypeQualifierValue] 
	    ,[Key] 
	    ,[Name] 
	    ,[Description] 
	    ,[Order] 
	    ,[IsGridColumn] 
	    ,[DefaultValue] 
	    ,[IsMultiValue] 
	    ,[IsRequired] 
	    ,[Guid]) 
    VALUES
	    (1 
	    ,37 
	    ,91 
	    ,N'Id' 
	    ,N'22' 
	    ,N'NameTag' 
	    ,N'Name Tag' 
	    ,NULL 
	    ,0 
	    ,0 
	    ,CAST(@NameTagLabelFileId AS VARCHAR) 
	    ,0 
	    ,0 
	    ,'4976e06d-8f24-492a-9a20-ec608667f284') 

    SET @AttributeId = SCOPE_IDENTITY() 

    INSERT INTO [AttributeQualifier] 
	    ([IsSystem] 
	    ,[AttributeId] 
	    ,[Key] 
	    ,[Value] 
	    ,[Guid]) 
    VALUES
	    (0 
	    ,@AttributeId 
	    ,N'binaryFileType' 
	    ,N'1' 
	    ,'966cb830-b268-45e3-9f36-b305e126b777') 

    -- set values for the (a398=MergeField, a399=MergeCodes) attributes of the DefinedValues of the Label Merge Fields DT
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

    INSERT INTO [AttributeValue] 
	    ([IsSystem] 
	    ,[AttributeId] 
	    ,[EntityId] 
	    ,[Order] 
	    ,[Value] 
	    ,[Guid]) 
    VALUES
	    (1 
	    ,398 
	    ,@FullNameValueId 
	    ,0 
	    ,N'{{ Person.NickName }} {{ Person.LastName }}' 
	    ,'b39d38d8-4220-46d7-84f0-7c3f7112ff7a') 

    INSERT INTO [AttributeValue] 
	    ([IsSystem] 
	    ,[AttributeId] 
	    ,[EntityId] 
	    ,[Order] 
	    ,[Value] 
	    ,[Guid]) 
    VALUES
	    (1 
	    ,398 
	    ,@LocationsValueId 
	    ,0 
	    ,N'{% for group in GroupType.Groups %}{% for location in group.Locations %}{{location.Name}}{% endfor %}{% endfor %}' 
	    ,'11f1f64e-ee3c-473b-8eb3-64590ec2ed96') 

    INSERT INTO [AttributeValue] 
	    ([IsSystem] 
	    ,[AttributeId] 
	    ,[EntityId] 
	    ,[Order] 
	    ,[Value] 
	    ,[Guid]) 
    VALUES
	    (1 
	    ,398 
	    ,@BirthdayDayofWeekValueId 
	    ,0 
	    ,N'{% if Person.DaysToBirthday <= 7 %}{{Person.BirthdayDayOfWeekShort}}{% endif %}' 
	    ,'73b61090-7689-4ef7-9964-a16480ed36fc') 

    INSERT INTO [AttributeValue] 
	    ([IsSystem] 
	    ,[AttributeId] 
	    ,[EntityId] 
	    ,[Order] 
	    ,[Value] 
	    ,[Guid]) 
    VALUES
	    (1 
	    ,398 
	    ,@LegalIconValueId 
	    ,0 
	    ,N'{% if Person.LegalNotes != '''' %}L{% endif %}' 
	    ,'872dbf30-e0c0-4810-a36e-d28fc3124a51') 

    INSERT INTO [AttributeValue] 
	    ([IsSystem] 
	    ,[AttributeId] 
	    ,[EntityId] 
	    ,[Order] 
	    ,[Value] 
	    ,[Guid]) 
    VALUES
	    (1 
	    ,398 
	    ,@LegalNoteValueId 
	    ,0 
	    ,N'{% if Person.LegalNotes != '''' %}{{Person.LegalNotes}}{% endif %}' 
	    ,'89c604fa-61a9-4255-ae1f-b6381b23603f') 

    INSERT INTO [AttributeValue] 
	    ([IsSystem] 
	    ,[AttributeId] 
	    ,[EntityId] 
	    ,[Order] 
	    ,[Value] 
	    ,[Guid]) 
    VALUES
	    (1 
	    ,398 
	    ,@AllergyIconValueId 
	    ,0 
	    ,N'{% if Person.Allergy != '''' -%}A{% endif -%}' 
	    ,'5dd35431-d22d-4410-9a55-55eac9859c35') 

    INSERT INTO [AttributeValue] 
	    ([IsSystem] 
	    ,[AttributeId] 
	    ,[EntityId] 
	    ,[Order] 
	    ,[Value] 
	    ,[Guid]) 
    VALUES
	    (1 
	    ,398 
	    ,@AllergyNoteValueId 
	    ,0 
	    ,N'{% if Person.Allergy != '''' %}{{Person.Allergy}}{% endif %}' 
	    ,'4315a58e-6514-49a8-b80c-22ac7710ac19') 

    INSERT INTO [AttributeValue] 
	    ([IsSystem] 
	    ,[AttributeId] 
	    ,[EntityId] 
	    ,[Order] 
	    ,[Value] 
	    ,[Guid]) 
    VALUES
	    (1 
	    ,398 
	    ,@FirsttimeIconValueId 
	    ,0 
	    ,N'{% if Person.LastCheckIn == '''' %}F{% endif %}' 
	    ,'f9d654e2-b715-4c6a-b875-ac690b21b3e8') 

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
	    ,@ChildLabelFileId 
	    ,0 
	    ,N'WWW^' + CAST(@SecurityCodeValueId AS VARCHAR) 
	    + '|5^' + CAST(@NickNameValueId AS VARCHAR) 
	    + '|6^' + CAST(@LastNameValueId AS VARCHAR) 
	    + '|AAA^' + CAST(@AllergyIconValueId AS VARCHAR) 
	    + '|LLL^' + CAST(@LegalIconValueId AS VARCHAR) 
	    + '|2^'  + CAST(@BirthdayIconValueId AS VARCHAR) 
	    + '|3^'  + CAST(@FirsttimeIconValueId AS VARCHAR) 
	    + '|4^'  + CAST(@BirthdayDayofWeekValueId AS VARCHAR) 
	    + '|10^' + CAST(@ScheduleTimesValueId AS VARCHAR) 
	    + '|9^' + CAST(@LocationsValueId AS VARCHAR) 
	    + '|' 
	    ,'66eee8d3-e44e-446f-86a4-85f4a1061a24') 

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
	    ,@NameTagLabelFileId 
	    ,0 
	    ,N'NickName^' + CAST(@NickNameValueId AS VARCHAR) 
	    + '|LastName^' + CAST(@LastNameValueId AS VARCHAR) + '|' 
	    ,'839f5936-3f4a-4b7e-a47a-040e14f3c5d6') 

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
	    ,@NoteLabelFileId 
	    ,0 
	    ,N'WWW^' + CAST(@SecurityCodeValueId AS VARCHAR) 
	    + '|2^' + CAST(@FullNameValueId AS VARCHAR) 
	    + '|4^' + CAST(@ScheduleTimesValueId AS VARCHAR) 
	    + '|AAA^' + CAST(@AllergyIconValueId AS VARCHAR) 
	    + '|3^' + CAST(@LocationsValueId AS VARCHAR) 
	    + '|LLL^' + CAST(@LegalIconValueId AS VARCHAR)
	    + '|5^' + CAST(@AllergyNoteValueId AS VARCHAR) 
	    + '|7^' + CAST(@LegalNoteValueId AS VARCHAR) 
	    + '|Notes:^|' 
	    ,'7de21f27-aff3-4366-adfb-0fb82ff38b25') 

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
	    ,@ParentLabelFileId 
	    ,0 
	    ,N'MMM^' + CAST(@SecurityCodeValueId AS VARCHAR) 
	    + '|2^' + CAST(@NickNameValueId AS VARCHAR) 
	    + '|Child Pick-up Receipt^|up your child. If you lose this please see the area director.^|' 
	    ,'10e7a555-01a0-4a7c-86e3-e8d183acea13') 

    -- Insert workflow type to parse labels upon adding new ones (CategoryId 59 = Check-in)
    DECLARE @WorkflowTypeId INT
    DECLARE @ActivityTypeId INT
    INSERT INTO [WorkflowType]
	    ([IsSystem]
	    ,[IsActive]
	    ,[Name]
	    ,[Description]
	    ,[CategoryId]
	    ,[Order]
	    ,[WorkTerm]
	    ,[ProcessingIntervalSeconds]
	    ,[IsPersisted]
	    ,[LoggingLevel]
	    ,[Guid])
    VALUES
	    (1
	    ,1
	    ,N'Parse Check-in Label'
	    ,N''
	    ,59
	    ,0
	    ,N'Parse'
	    ,NULL
	    ,0
	    ,0
	    ,'C93EEC26-4BE3-4EB5-92D4-5C30EEF069D9'
	    )
    SET @WorkflowTypeId = SCOPE_IDENTITY()

    -- Insert activity type
    INSERT INTO [WorkflowActivityType]
	    ([IsActive]
	    ,[WorkflowTypeId]
	    ,[Name]
	    ,[Description]
	    ,[IsActivatedWithWorkflow]
	    ,[Order]
	    ,[Guid])
         VALUES
	    (1
	    ,@WorkflowTypeId
	    ,N'Parse Label'
	    ,N''
	    ,1
	    ,0
	    ,'051C0511-4881-4865-87F6-0C2B9550DEC8')
    SET @ActivityTypeId = SCOPE_IDENTITY()

    -- Insert action type (et36=Rock.Workflow.Action.ParseZebraLabel)
    INSERT INTO [WorkflowActionType]
	    ([ActivityTypeId]
	    ,[Name]
	    ,[Order]
	    ,[EntityTypeId]
	    ,[IsActionCompletedOnSuccess]
	    ,[IsActivityCompletedOnSuccess]
	    ,[Guid])
         VALUES
	    (@ActivityTypeId
	    ,N'Parse Zebra Label'
	    ,0
	    ,36
	    ,1
	    ,1
	    ,'7E9A7D34-B5F1-441F-8065-343B8F6D1FB9')

    -- Add the new workflow type to the check-in label block
    DECLARE @BlockId INT
    SET @BlockId = (SELECT [Id] FROM [Block] WHERE [Guid] = 'F52CEDB1-F822-485C-9A1C-BA6D05383FAA')

    DECLARE @WorkflowAttributeId INT
    SET @WorkflowAttributeId = (SELECT [Id] FROM [Attribute] WHERE [Guid] = '27DDFD07-835C-4735-B42D-DD45629E465F')

    -- Delete existing attribute value first (might have been created by Rock system)
    DELETE [AttributeValue]
    WHERE
	    [AttributeId] = @WorkflowAttributeId
	    AND [EntityId] = @BlockId

    INSERT INTO [AttributeValue]
	    ([IsSystem]
	    ,[AttributeId]
	    ,[EntityId]
	    ,[Order]
	    ,[Value]
	    ,[Guid])
    VALUES
	    (1
	    ,@WorkflowAttributeId
	    ,@BlockId
	    ,0
	    ,'C93EEC26-4BE3-4EB5-92D4-5C30EEF069D9',
	    NEWID()
	    )
" );

        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            // Change Data filters to use Ids
            Sql( @"
			-- record status: active
			UPDATE [DataViewFilter]
			SET [Selection] = '[    ""RecordStatusValueId"",    ""[\r\n  \""3\""\r\n]""  ]'
			WHERE [Guid] = '01A3855A-39B8-42FF-B469-3A0F851C32E3'
			
			-- connection status: member
			UPDATE [DataViewFilter]
			SET [Selection] = '[    ""ConnectionStatusValueId"",    ""[\r\n  \""65\""\r\n]""  ]'
			WHERE [Guid] = 'FDF47D1E-AF28-46BA-97E6-67138E582AE5'
			
			-- record status: pending
			UPDATE [DataViewFilter]
			SET [Selection] = '[    ""RecordStatusValueId"",    ""[\r\n  \""5\""\r\n]""  ]'
			WHERE [Guid] = 'A3E1EB8E-CB2A-4EE0-9A27-3499392D5A1D'
" );
   

            Sql( @"
	-----------------------------------------------------------------------------------------------
	-- Uninstall Report: No Longer Attending
	-----------------------------------------------------------------------------------------------
            
		DELETE [ReportField] WHERE [Guid] = '6113132a-bd29-4e25-8c6d-22bac0fd795b'
		DELETE [ReportField] WHERE [Guid] = 'c1b78a6c-eb40-4eaa-be75-c6de8c00629c'
		DELETE [ReportField] WHERE [Guid] = '29f09daa-9d05-4e6f-9f3d-814dbc1d18e4'
		DELETE [ReportField] WHERE [Guid] = '65382c7a-44b2-4049-8dc8-8ada9edcf231'
		DELETE [ReportField] WHERE [Guid] = 'fc9e7bda-93e8-491e-b7a1-e7b2e1fe712a'
		DELETE [Report] WHERE [Guid] = '87d3e118-ada8-4424-b63b-9482a7d9e609'
		DELETE [Category] WHERE [Guid] = 'b88e45fc-c4f8-487f-ab16-9e30157da967'    
		
	-----------------------------------------------------------------------------------------------
	-- Uninstall DataView: Self Inactivated
	-----------------------------------------------------------------------------------------------
    DELETE [DataView] WHERE [Guid] = '6296b6ee-10e4-4bb7-8565-1268cae7969f'
    DELETE [DataViewFilter] WHERE [Guid] = '83c592bb-c62a-41d8-a64a-4ec2c1df9bc0'
    DELETE [DataViewFilter] WHERE [Guid] = '7694c7de-b5b8-40ec-aff8-793ac67abaaf'
    DELETE [DataViewFilter] WHERE [Guid] = 'f1544655-2443-4b42-a750-d609aeddaac8'
" );

            RockMigrationHelper.DeleteBlock( "DCA9E640-B5EA-4C73-90BC-4A91330528D5" );
            RockMigrationHelper.DeleteAttribute( "8796567C-4047-43C1-AF32-2FDBE030BEAC" );
            RockMigrationHelper.DeleteAttribute( "12E9C8A7-03E4-472D-9E20-9EC8F3453B2F" );
            RockMigrationHelper.DeleteAttribute( "2B8A03D3-B7DC-4DA3-A31E-826D655435D5" );
            RockMigrationHelper.DeleteCategory( "DD8F467D-B83C-444F-B04C-C681167046A1" );

            DropColumn( "dbo.Attribute", "IconCssClass" );
        }
    }
}
