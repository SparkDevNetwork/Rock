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
    public partial class ExtendWorkflowName : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            AlterColumn("dbo.Workflow", "Name", c => c.String(nullable: false, maxLength: 250));

            //
            // rebuild the Dataview for 'Background check is still valid' 
            //
            Sql( @"
DELETE FROM [DataViewFilter] where [Guid] = 'A0C00B93-C656-44E3-B64A-28036D9B9E5E'
DELETE FROM [DataViewFilter] where [Guid] = 'F020509F-C8D0-477A-AC9C-22541918A2CC'
DELETE FROM [DataViewFilter] where [Guid] = '256E15E6-9D9A-4539-8BE1-0B0F68BD2342'
DELETE FROM [DataViewFilter] where [Guid] = 'BD7FE43A-B887-41A0-9C65-A2E6D9685596'
DELETE FROM [DataViewFilter] where [Guid] = '6B3326F1-ADF4-472A-8FD8-461AA85B2624'
DELETE FROM [DataViewFilter] where [Guid] = '841466E9-FB82-4AC1-BAFB-87B6153CE3EF'
DELETE FROM [DataViewFilter] where [Guid] = '529CC78B-83F9-442B-A8CC-3BB9004AF0A8'
DELETE FROM [DataViewFilter] where [Guid] = 'D6FCA2A2-8DF6-40D6-A8E9-A88EA9ADB94A'
DELETE FROM [DataViewFilter] where [Guid] = '1696FD0E-6B2B-4042-AEED-59E003755901'
DELETE FROM [DataViewFilter] where [Guid] = '8E6BA6EE-0977-48E1-AC53-E40DA02E245E'
DELETE FROM [DataViewFilter] where [Guid] = 'DE876BBC-C94C-47EE-BF7B-E374F8807C78'
" );
            //  change base DataViewFilter to a 'GroupAny'
            Sql( @"UPDATE [DataViewFilter] set ExpressionType = 2 where [Guid] = 'D294FB17-8872-47C8-AC29-96714B3DDE9F'" );

            // Create [GroupAny] DataViewFilter for DataView: Background check is still valid
            Sql( @"
IF NOT EXISTS (SELECT * FROM DataViewFilter where [Guid] = 'D294FB17-8872-47C8-AC29-96714B3DDE9F') BEGIN    
    DECLARE
        @ParentDataViewFilterId int = (select Id from DataViewFilter where [Guid] = '00000000-0000-0000-0000-000000000000'),
        @DataViewFilterEntityTypeId int = (select Id from EntityType where [Guid] = '00000000-0000-0000-0000-000000000000')
    INSERT INTO [DataViewFilter] (ExpressionType, ParentId, EntityTypeId, Selection, [Guid]) 
    values (2,@ParentDataViewFilterId,@DataViewFilterEntityTypeId,'','D294FB17-8872-47C8-AC29-96714B3DDE9F')
END
" );
            // Create [GroupAll] DataViewFilter for DataView: Background check is still valid
            Sql( @"
IF NOT EXISTS (SELECT * FROM DataViewFilter where [Guid] = 'DE876BBC-C94C-47EE-BF7B-E374F8807C78') BEGIN    
    DECLARE
        @ParentDataViewFilterId int = (select Id from DataViewFilter where [Guid] = 'D294FB17-8872-47C8-AC29-96714B3DDE9F'),
        @DataViewFilterEntityTypeId int = (select Id from EntityType where [Guid] = '00000000-0000-0000-0000-000000000000')
    INSERT INTO [DataViewFilter] (ExpressionType, ParentId, EntityTypeId, Selection, [Guid]) 
    values (1,@ParentDataViewFilterId,@DataViewFilterEntityTypeId,'','DE876BBC-C94C-47EE-BF7B-E374F8807C78')
END
" );
            // Create [GroupAll] DataViewFilter for DataView: Background check is still valid
            Sql( @"
IF NOT EXISTS (SELECT * FROM DataViewFilter where [Guid] = '8E6BA6EE-0977-48E1-AC53-E40DA02E245E') BEGIN    
    DECLARE
        @ParentDataViewFilterId int = (select Id from DataViewFilter where [Guid] = 'D294FB17-8872-47C8-AC29-96714B3DDE9F'),
        @DataViewFilterEntityTypeId int = (select Id from EntityType where [Guid] = '00000000-0000-0000-0000-000000000000')
    INSERT INTO [DataViewFilter] (ExpressionType, ParentId, EntityTypeId, Selection, [Guid]) 
    values (1,@ParentDataViewFilterId,@DataViewFilterEntityTypeId,'','8E6BA6EE-0977-48E1-AC53-E40DA02E245E')
END
" );
            // Create Rock.Reporting.DataFilter.Person.AgeFilter DataViewFilter for DataView: Background check is still valid
            Sql( @"
IF NOT EXISTS (SELECT * FROM DataViewFilter where [Guid] = '1696FD0E-6B2B-4042-AEED-59E003755901') BEGIN    
    DECLARE
        @ParentDataViewFilterId int = (select Id from DataViewFilter where [Guid] = 'DE876BBC-C94C-47EE-BF7B-E374F8807C78'),
        @DataViewFilterEntityTypeId int = (select Id from EntityType where [Guid] = '4911C63D-71BB-4686-AAA3-D66EA41DA465')
    INSERT INTO [DataViewFilter] (ExpressionType, ParentId, EntityTypeId, Selection, [Guid]) 
    values (0,@ParentDataViewFilterId,@DataViewFilterEntityTypeId,'512|18|,','1696FD0E-6B2B-4042-AEED-59E003755901')
END
" );
            // Create Rock.Reporting.DataFilter.PropertyFilter DataViewFilter for DataView: Background check is still valid
            Sql( @"
IF NOT EXISTS (SELECT * FROM DataViewFilter where [Guid] = 'D6FCA2A2-8DF6-40D6-A8E9-A88EA9ADB94A') BEGIN    
    DECLARE
        @ParentDataViewFilterId int = (select Id from DataViewFilter where [Guid] = '8E6BA6EE-0977-48E1-AC53-E40DA02E245E'),
        @DataViewFilterEntityTypeId int = (select Id from EntityType where [Guid] = '03F0D6AC-D181-48B6-B4BC-1F2652B55323')
    INSERT INTO [DataViewFilter] (ExpressionType, ParentId, EntityTypeId, Selection, [Guid]) 
    values (0,@ParentDataViewFilterId,@DataViewFilterEntityTypeId,'[
  ""BackgroundChecked"",
  ""1"",
  ""True""
]','D6FCA2A2-8DF6-40D6-A8E9-A88EA9ADB94A')
END
" );
            // Create Rock.Reporting.DataFilter.PropertyFilter DataViewFilter for DataView: Background check is still valid
            Sql( @"
IF NOT EXISTS (SELECT * FROM DataViewFilter where [Guid] = '529CC78B-83F9-442B-A8CC-3BB9004AF0A8') BEGIN    
    DECLARE
        @ParentDataViewFilterId int = (select Id from DataViewFilter where [Guid] = '8E6BA6EE-0977-48E1-AC53-E40DA02E245E'),
        @DataViewFilterEntityTypeId int = (select Id from EntityType where [Guid] = '03F0D6AC-D181-48B6-B4BC-1F2652B55323')
    INSERT INTO [DataViewFilter] (ExpressionType, ParentId, EntityTypeId, Selection, [Guid]) 
    values (0,@ParentDataViewFilterId,@DataViewFilterEntityTypeId,'[
  ""BackgroundCheckDate"",
  ""256"",
  ""CURRENT:-1095\tAll||||""
]','529CC78B-83F9-442B-A8CC-3BB9004AF0A8')
END
" );
            // Create Rock.Reporting.DataFilter.Person.AgeFilter DataViewFilter for DataView: Background check is still valid
            Sql( @"
IF NOT EXISTS (SELECT * FROM DataViewFilter where [Guid] = '841466E9-FB82-4AC1-BAFB-87B6153CE3EF') BEGIN    
    DECLARE
        @ParentDataViewFilterId int = (select Id from DataViewFilter where [Guid] = '8E6BA6EE-0977-48E1-AC53-E40DA02E245E'),
        @DataViewFilterEntityTypeId int = (select Id from EntityType where [Guid] = '4911C63D-71BB-4686-AAA3-D66EA41DA465')
    INSERT INTO [DataViewFilter] (ExpressionType, ParentId, EntityTypeId, Selection, [Guid]) 
    values (0,@ParentDataViewFilterId,@DataViewFilterEntityTypeId,'256|18|,','841466E9-FB82-4AC1-BAFB-87B6153CE3EF')
END
" );
            // Create Rock.Reporting.DataFilter.PropertyFilter DataViewFilter for DataView: Background check is still valid
            Sql( @"
IF NOT EXISTS (SELECT * FROM DataViewFilter where [Guid] = '6B3326F1-ADF4-472A-8FD8-461AA85B2624') BEGIN    
    DECLARE
        @ParentDataViewFilterId int = (select Id from DataViewFilter where [Guid] = '8E6BA6EE-0977-48E1-AC53-E40DA02E245E'),
        @DataViewFilterEntityTypeId int = (select Id from EntityType where [Guid] = '03F0D6AC-D181-48B6-B4BC-1F2652B55323')
    INSERT INTO [DataViewFilter] (ExpressionType, ParentId, EntityTypeId, Selection, [Guid]) 
    values (0,@ParentDataViewFilterId,@DataViewFilterEntityTypeId,'[
  ""BackgroundCheckResult"",
  ""Pass""
]','6B3326F1-ADF4-472A-8FD8-461AA85B2624')
END
" );
            //
            // blank SSN
            //

            RockMigrationHelper.UpdateWorkflowActionType( "F95F8B4B-3ACD-4906-81F5-EBF589F87831", "Clear SSN Value", 3, "C789E457-0783-44B3-9D8F-2EBAB5F11110", true, false, "", "", 1, "", "EE581993-40BB-4D67-82AE-0CC152FE9620" ); // Background Check:Complete Request:Clear SSN Value

            RockMigrationHelper.AddActionTypeAttributeValue( "EE581993-40BB-4D67-82AE-0CC152FE9620", "D7EAA859-F500-4521-9523-488B12EAA7D2", @"False" ); // Background Check:Complete Request:Clear SSN Value:Active
            RockMigrationHelper.AddActionTypeAttributeValue( "EE581993-40BB-4D67-82AE-0CC152FE9620", "44A0B977-4730-4519-8FF6-B0A01A95B212", @"1fab9a4c-c5a2-4938-b9bd-80935f0a598c" ); // Background Check:Complete Request:Clear SSN Value:Attribute
            RockMigrationHelper.AddActionTypeAttributeValue( "EE581993-40BB-4D67-82AE-0CC152FE9620", "57093B41-50ED-48E5-B72B-8829E62704C8", @"" ); // Background Check:Complete Request:Clear SSN Value:Order
            RockMigrationHelper.AddActionTypeAttributeValue( "EE581993-40BB-4D67-82AE-0CC152FE9620", "E5272B11-A2B8-49DC-860D-8D574E2BC15C", @"xxx-xx-xxxx" ); // Background Check:Complete Request:Clear SSN Value:Text Value|Attribute Value

            Sql( @"
DECLARE @CompleteWorkflowEntityTypeId int = ( SELECT [Id] FROM [EntityType] WHERE [Guid] = 'EEDA4318-F014-4A46-9C76-4C052EF81AA1' )
DECLARE @ActivityTypeId int = (SELECT [Id] FROM [WorkflowActivityType] WHERE [Guid] = 'F95F8B4B-3ACD-4906-81F5-EBF589F87831')
DECLARE @Order int = ( SELECT TOP 1 [Order] FROM [WorkflowActionType] WHERE [ActivityTypeId] = @ActivityTypeId )
IF @Order IS NULL
BEGIN
	SET @order  = ISNULL((SELECT Max([Order]) FROM [WorkflowActionType] WHERE [ActivityTypeId] = @ActivityTypeId)+1,0)
END

IF @Order IS NOT NULL
BEGIN
	UPDATE [WorkflowActionType] 
	SET [Order] = [Order] + 1
	WHERE [ActivityTypeId] = @ActivityTypeId
	AND [Order] >= @Order

	UPDATE [WorkflowActionType] 
	SET [Order] = @Order
	WHERE [Guid] = 'EE581993-40BB-4D67-82AE-0CC152FE9620'
END
" );

            //
            // Fix context on person person profile new transaciton links
            //

            // Attrib Value for Block:Transaction Links, Attribute:Entity Type Page: Contributions, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "6F7F97D3-6C7D-4B58-A6E7-9A21BF55428A", "6783D47D-92F9-4F48-93C0-16111D675A0F", @"72657ed8-d16e-492e-ac12-144c5e7567e7" );

            //
            // Blank out page descriptions on internal site
            //

            Sql( @"UPDATE [Page]
SET [Description] = ''
WHERE [Guid] IN ('85F25819-E948-4960-9DDF-00F54D32444E', 
'7596D389-4EAB-4535-8BEE-229737F46F44', 
'03CB988A-138C-448B-A43D-8891844EEB18', 
'7D4E2142-D24E-4DD2-84BC-B34C5C3D0D46', 
'5FBE9019-862A-41C6-ACDC-287D7934757D', 
'20F97A93-7949-4C2A-8A5E-C756FE8585CA', 
'9F36531F-C1B5-4E23-8FA3-18B6DAFF1B0B', 
'0C4B3F4D-53FD-4A65-8C93-3868CE4DA137', 
'7F2581A1-941E-4D51-8A9D-5BE9B881B003', 
'895F58FB-C1C4-4399-A4D8-A9A10225EA09', 
'F0B34893-9550-4864-ADB4-EE860E4E427C', 
'86D5E33E-E351-4CA5-9925-849C6C467257', 
'D58F205E-E9CC-4BD9-BC79-F3DA86F6E346', 
'37759B50-DB4A-440D-A83B-4EF3B4727B1E', 
'1FD5698F-7279-463F-9637-9A80DB86BB86', 
'550A898C-EDEA-48B5-9C58-B20EC13AF13B', 
'A2753E03-96B1-4C83-AA11-FCD68C631571', 
'89B7A631-EA6F-4DA3-9380-04EE67B63E9E', 
'7BA1FAF4-B63C-4423-A818-CC794DDB14E3', 
'142627AE-6590-48E3-BFCA-3669260B8CF2', 
'0B213645-FA4E-44A5-8E4C-B2D8EF054985', 
'18C9E5C3-3E28-4AA3-84F6-78CD4EA2DD3C', 
'E0E1DE66-B825-4BFB-A0B3-6E069AA9AA40', 
'5EE91A54-C750-48DC-9392-F1F0F0581C3A', 
'8A97CC93-3E93-4286-8440-E5217B65A904', 
'1719F597-5BA9-458D-9362-9C3E558E5C82', 
'5E036ADE-C2A4-4988-B393-DAC58230F02E', 
'F111791B-6A58-4388-8533-00E913F48F41', 
'BF04BB7E-BE3A-4A38-A37C-386B55496303', 
'08DBD8A5-2C35-4146-B4A8-0F7652348B25', 
'4A833BE3-7D5E-4C38-AF60-5706260015EA', 
'CE2170A9-2C8E-40B1-A42E-DFA73762D01D', 
'40899BCD-82B0-47F2-8F2A-B6AA3877B445', 
'C58ADA1A-6322-4998-8FED-C3565DE87EFA', 
'EC7A06CD-AAB5-4455-962E-B4043EA2440E', 
'F7F41856-F7EA-49A8-9D9B-917AC1964602', 
'F7105BFE-B28C-41B6-9CE6-F1018D77DD8F', 
'D9678FEF-C086-4232-972C-5DBAC14BFEE6', 
'4E237286-B715-4109-A578-C1445EC02707', 
'1A3437C8-D4CB-4329-A366-8D6A4CBF79BF', 
'BB0ACD18-24FB-42BA-B89A-2FFD80472F5B', 
'4011CB37-28AA-46C4-99D5-826F4A9CADF5', 
'03C49950-9C4C-4668-9C65-9A0DF43D1B33', 
'5537F375-B652-4603-8E04-119C74414CD7', 
'66031C31-B397-4F78-8AB2-389B7D8731AA', 
'1A233978-5BF4-4A09-9B86-6CC4C081F48B', 
'7E660A4D-72C5-4CF8-B144-16CFC2ADD4D7', 
'EE0CC3F3-50FD-4161-BA5C-A852D4A10E7B', 
'7C093A63-F2AC-4FE3-A826-8BF06D204EA2', 
'1570D2AF-4FE2-4FC7-BED9-F20EBCBE9867', 
'EF7AA296-CA69-49BC-A28B-901A8AAA9466', 
'EF65EFF2-99AC-4081-8E09-32A04518683A', 
'606BDA31-A8FE-473A-B3F8-A00ECF7E06EC', 
'B67E38CB-2EF1-43EA-863A-37DAA1C7340F', 
'2A22D08D-73A8-4AAF-AC7E-220E8B2E7857', 
'6FF35C53-F89F-4601-8543-2E2328C623F8', 
'29CC8A0B-6476-4200-8B93-DC9BA8767D59', 
'CADB44F2-2453-4DB5-AB11-DADA5162AB79', 
'21DA6141-0A03-4F00-B0A8-3B110FBE2438', 
'26547B83-A92D-4D7E-82ED-691F403F16B6', 
'F5D6D7DD-FD5F-494C-83DC-E2AF63C705D1', 
'220D72F5-B589-4378-9852-BBB6F145AD7F', 
'7F1F4130-CB98-473B-9DE1-7A886D2283ED', 
'23507C90-3F78-40D4-B847-6FE8941FCD32', 
'FEA8D6FC-B26F-48D5-BE69-6BCEF7CDC4E5', 
'9C9CAD94-095E-4CC9-BC29-24BDE30492B2', 
'97ECDC48-6DF6-492E-8C72-161F76AE111B', 
'B0F4B33D-DD11-4CCC-B79D-9342831B8701', 
'C646A95A-D12D-4A67-9BE6-C9695C0267ED', 
'9C569E6B-F745-40E4-B91B-A518CD6C2922', 
'EBAA5140-4B8F-44B8-B1E8-C73B654E4B22', 
'4D7F3953-0BD9-4B4B-83F9-5FCC6B2BBE30', 
'1C763885-291F-44B7-A5E3-539584E07085', 
'48A9DF54-CC19-42FA-BDC6-97AF3E63029D', 
'227FDFB9-8C29-4B34-ABE5-E0579A3A6018', 
'2BECFB85-D566-464F-B6AC-0BE90189A418', 
'F4DF4899-2D44-4997-BA9B-9D2C64958A20', 
'D2B43273-C64F-4F57-9AAE-9571E1982BAC', 
'881AB1C2-4E00-4A73-80CC-9886B3717A20', 
'594692AA-5647-4F9A-9488-AADB990FDE56', 
'8559A9F1-C6A4-4945-B393-74F6706A8FA2', 
'66C5DD58-094C-4FF9-9AFB-44801FCFCC2D', 
'96501070-BB46-4432-AA3C-A8C496691629', 
'AFFFB245-A0EB-4002-B736-A2D52DD692CF', 
'5A06C807-251C-4155-BBE7-AAC73D0745E3', 
'7D5311B3-F526-4E22-8153-EA1799467886', 
'BA078BB8-7205-46F4-9530-B2FB9EAD3E57', 
'07E4BA19-614A-42D0-9D75-DFB31374844D', 
'04141667-1A08-4E15-8BB7-E3E312233E11', 
'5180AE8E-BF1C-444F-A154-14E5A8A4ACC9')" );

            //
            // DT: Delete orphaned Payment Detail records
            //
            Sql( @"
    DELETE [FinancialPaymentDetail] 
    WHERE [Id] NOT IN (
	    SELECT [FinancialPaymentDetailId] FROM [FinancialTransaction] WHERE [FinancialPaymentDetailId] IS NOT NULL
	    UNION
	    SELECT [FinancialPaymentDetailId] FROM [FinancialPersonSavedAccount] WHERE [FinancialPaymentDetailId] IS NOT NULL
	    UNION
	    SELECT [FinancialPaymentDetailId] FROM [FinancialScheduledTransaction] WHERE [FinancialPaymentDetailId] IS NOT NULL
    )
" );
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            AlterColumn("dbo.Workflow", "Name", c => c.String(nullable: false, maxLength: 100));
        }
    }
}
