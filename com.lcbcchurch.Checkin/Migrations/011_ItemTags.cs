// <copyright>
// Copyright by LCBC Church
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
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Rock;
using Rock.Plugin;
namespace com.lcbcchurch.Checkin.Migrations
{
    [MigrationNumber( 11, "1.0.14" )]
    public class ItemTags : Migration
    {
        public override void Up()
        {
            // Add the Merge Fields Needed for our custom labels
            RockMigrationHelper.AddDefinedValue( "E4D289A9-70FA-4381-913E-2A757AD11147", "NickNameShort", "Used for NickNames that are shorter than 13 characters", "CFD33CF0-E898-4EE3-AD06-C6CC9D591349", false );
            RockMigrationHelper.AddDefinedValue( "E4D289A9-70FA-4381-913E-2A757AD11147", "NickNameMedium", "Used for NickNames that are 13 to 18 characters long (inclusive).", "61795E3D-CE01-44C1-AF64-1C3BC71DE97C", false );
            RockMigrationHelper.AddDefinedValue( "E4D289A9-70FA-4381-913E-2A757AD11147", "NickNameLong", "Used for NickNames that are 19-22 characters long (hopefully no one has a longer nickname than this!)", "C359F5D7-40BE-4EB0-BE9D-DFDFA6447194", false );

            RockMigrationHelper.AddDefinedValueAttributeValue( "CFD33CF0-E898-4EE3-AD06-C6CC9D591349", "51EB8583-55EA-4431-8B66-B5BD0F83D81E", @"{% assign nameLength = Person.NickName | Size %}
{% if nameLength < 13 %}
{{Person.NickName}}
{% endif %}" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "61795E3D-CE01-44C1-AF64-1C3BC71DE97C", "51EB8583-55EA-4431-8B66-B5BD0F83D81E", @"{% assign nameLength = Person.NickName | Size %}
{% if nameLength > 12 and nameLength < 19 %}
{{Person.NickName}}
{% endif %}" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "C359F5D7-40BE-4EB0-BE9D-DFDFA6447194", "51EB8583-55EA-4431-8B66-B5BD0F83D81E", @"{% assign nameLength = Person.NickName | Size %}
{% if nameLength > 18 %}
{{Person.NickName}}
{% endif %}" );
            Sql( @"

    -- add the ZPL files and data for the labels 
    DECLARE @kidVentureIslandItemFileId INT

    -- KVI Child Label
    INSERT INTO [BinaryFile] 
	    ([IsTemporary] 
	    ,[IsSystem] 
	    ,[BinaryFileTypeId]
	    ,[FileName] 
	    ,[MimeType] 
	    ,[Description] 
	    ,[StorageEntityTypeId] 
	    ,[Guid]
		,[Path]
		,[FileSize]) 
    VALUES
	    (0 
	    ,0 
	    ,1  
	    ,N'kidVenture Island - Item' 
	    ,N'text/plain' 
	    ,N'Item Label for kidVenture Island Check-In.' 
	    ,51
	    ,N'08B13135-1352-4FA4-B271-51FA3A8451E0'
		,N'~/GetFile.ashx?guid=08B13135-1352-4FA4-B271-51FA3A8451E0'
		,1736) 

    SET @kidVentureIslandItemFileId = SCOPE_IDENTITY() 

    INSERT INTO [BinaryFileData] 
	    ([Id] 
	    ,[Content] 
	    ,[Guid]) 
    VALUES
	    (@kidVentureIslandItemFileId 
	    ,0x1043547E7E43442C7E43435E7E43547E0A5E58417E54413030307E4A534E5E4C54305E4D4E575E4D54445E504F4E5E504D4E5E4C48302C305E4A4D415E5052362C367E534431355E4C524E5E434932385E585A0A5E58410A5E4D4D430A5E50573630390A5E4C4C303430360A5E4C53300A5E464F3136302C3138355E4746412C30343630382C30343630382C30303033362C3A5A36343A0A654A7A746C37324F4654304D686D63596958536B704547454330486B747234436B546E616774764B6968744A527A73644B664B4E695A332F6E356C5430434663374E6C6450636678367A694F7379782F6961304138503839776F4273662B4946376278687050592F4E6738646C776750487768644D696B4D446D42755667756D37694A4358595963375665494A465557395631464C614E30737768776334524454672B37554A595353493441394E314B337078584E67754951575737384A4750706D726D594C4D4D4E57374172624F67477A63656D475278617848516368516D4F75595134375A324346672B6947633963374A42764F775A32506F6457776345486A3344523062335352795841744D6C63624955484B706C426C556F5872614D6D444E377A61674A343054447A4D4B4273325649756175386D59305933537658486E5878642F7734656332734B76782F5251596C576E4C644D457459796E2B363849636864475377454A514C476A584A614E65694D4A6450693351684F477767414778674E494E46574D726E695844485043686B58372F636572313430763266437235587A49624D7557414A652B62675948313239745950653541734C316C3542757659433339467075525A454F4F2F36794177786D2F787A35615279466A61443838346A6B634C6A56634D554B385668646C565A43726C79426869446B6B4D3941774C6A43784D4B4A577A596A6778576B5847386E4F4C3956504F6851674D4A455A6B7070777653637865474D646972525A4745664E497A4345544938743578394D504D553750474F6E436F62574657514D545976446E54697362475A56377931597A52766D396A63796854456B504D6C474C51623957424B5A734F77384D5377794C322B4A3751743475305445694D61386C7A584C30453762397061525A345135567A4A71597A58574D793877536D5A4E564B61775A6A635565475A3754732F614D6C6F4678776C77794A6A4A573576527350584F4949355A6254672F724752735A6F35614B7764796641374E6E365374766D4664694B4D39726C72346C4A757A70417738614D6565575A62305A6D65444838537A727252675A716838723970364A74566F593879584C656A6377767346672F50712F7A4877553150544432666B56474D72565769374B44774C43506D4D662B414576754266595A73354E6A777A326B7764577670616E583965796A4379665A574945587673624E634B6C485A412B4A7A2B47346669776858375A587639665266674F484C35474E776A467376506D78736D4D5859546633314173656D3175396D2B4A7756326D70735347306442665571485138614451425371676E772F4F4F46485349454F584E5177444C57544759435478397430484A7069724C75646D59466C6864724F3351612F2B63706E59336A49764536534E65635571482B7A6F47443168644D6359395751706244596A593376476969644C3457517754466E446D4A6F4F386155715A474359612F61523254746B484F4D685862445A7A4D416F75737A76496B6247626B2F63594A64764272625A4330625164482F72686835496C667270327752625A6A5773376A4D475730515A443263726852464B70386E343671554561644B3765626D7071796771457A415531574373576F56644D4547544A75545358307A4F3876346D726C4B4831302F622F49693565434F537066713551564A457477683565706169662F5948396876446B435A313A453136410A5E46423630392C312C302C435E4654302C39355E41304E2C39392C39385E46485C5E46444E69636B4E616D6553686F72745E46530A5E46423630392C312C302C435E4654302C39355E41304E2C37352C37345E46485C5E46444E69636B4E616D654D656469756D5E46530A5E46423630392C312C302C435E4654302C39355E41304E2C36302C35395E46485C5E46444E69636B4E616D654C6F6E675E46530A5E46423630392C312C302C435E4654302C3135305E41304E2C35352C35345E46485C5E46444C6173744E616D655E46530A5E464F302C3333305E47423630392C302C355E46530A5E46543138302C3338305E41304E2C34302C33395E46485C5E46444974656D205461675E46530A5E46543334302C3338305E41304E2C34302C33395E46485C5E4644446174655E46530A5E5051312C302C312C595E585A0A
	    ,'F8E7C741-7B0D-4150-A24A-D44B73152681') 
" );
            var itemLabelId = SqlScalar( "Select Top 1 Id From BinaryFile Where Guid = '08B13135-1352-4FA4-B271-51FA3A8451E0'" ).ToString().AsInteger();


            if ( itemLabelId > 0 )
            {
                var nickNameShortId = SqlScalar( "Select Top 1 Id From DefinedValue Where Guid = 'CFD33CF0-E898-4EE3-AD06-C6CC9D591349'" ).ToString().AsInteger();
                var nickNameMediumId = SqlScalar( "Select Top 1 Id From DefinedValue Where Guid = '61795E3D-CE01-44C1-AF64-1C3BC71DE97C'" ).ToString().AsInteger();
                var nickNameLongId = SqlScalar( "Select Top 1 Id From DefinedValue Where Guid = 'C359F5D7-40BE-4EB0-BE9D-DFDFA6447194'" ).ToString().AsInteger();
                var LastNameId = SqlScalar( "Select Top 1 Id From DefinedValue Where Guid = '85B71255-19F9-4443-A7AE-EF670385DC71'" ).ToString().AsInteger();
                var DateId = SqlScalar( "Select Top 1 Id From DefinedValue Where Guid = '18B24BF8-26DD-43BB-A54D-8B10C57EA740'" ).ToString().AsInteger();

                var attributeValueString = String.Format( "NickNameShort^{0}|NickNameMedium^{1}|NickNameLong^{2}|LastName^{3}|Date^{4}", nickNameShortId, nickNameMediumId, nickNameLongId, LastNameId, DateId );
                RockMigrationHelper.AddAttributeValue( "733944B7-A0D5-41B4-94D4-DE007F72B6F0", itemLabelId, "1", "3F727A9B-3994-4153-BE04-82304E5D2C95" );
                RockMigrationHelper.AddAttributeValue( "CE57450F-634A-420A-BF5A-B43E9B20ABF2", itemLabelId, attributeValueString, "65E20D5B-1856-4CA8-A24B-E2C8B5518A8A" );

            }


            // Add new entity attributes
            RockMigrationHelper.AddGroupTypeGroupAttribute( "0572A5FE-20A4-4BF1-95CD-C71DB5281392", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Are Item Tags Offered", "", 4, "False", "23F26764-E99C-4E6A-873D-1C6490D7EB77" );
            RockMigrationHelper.AddEntityAttribute( "Rock.Model.Device", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "", "", "Allow Item Tags to be Offered?", "", "", 1008, "False", "E239D379-01C6-4CB4-A998-41B3C26D7B5A", "AllowItemTags" );

            // Add Workflow

            #region EntityTypes
            RockMigrationHelper.UpdateEntityType( "com.lcbcchurch.Checkin.Workflow.Action.CheckIn.LCBCCreateLabels", "F006D1F9-74B4-4AFD-82D4-96737404DFBA", false, true );
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute( "F006D1F9-74B4-4AFD-82D4-96737404DFBA", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Active", "Active", "Should Service be used?", 0, @"False", "94BC9BDF-1AF2-4D53-B8E2-79C5ADC8C285" ); // com.lcbcchurch.Checkin.Workflow.Action.CheckIn.LCBCCreateLabels:Active
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute( "F006D1F9-74B4-4AFD-82D4-96737404DFBA", "8814CF68-435F-40F4-9A4C-ED6582835E0E", "Item Tag Label", "ItemTagLabel", "", 0, @"", "52B8BCD3-A2DF-4990-BB00-0CE0FF3BCCCE" ); // com.lcbcchurch.Checkin.Workflow.Action.CheckIn.LCBCCreateLabels:Item Tag Label
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute( "F006D1F9-74B4-4AFD-82D4-96737404DFBA", "A75DFC58-7A1B-4799-BF31-451B2BBE38FF", "Order", "Order", "The order that this service should be used (priority)", 0, @"", "CEF15819-4E30-4599-BC64-F97780A496A7" ); // com.lcbcchurch.Checkin.Workflow.Action.CheckIn.LCBCCreateLabels:Order

            #endregion

            #region Categories

            RockMigrationHelper.UpdateCategory( "C9F3C4A5-1526-474D-803F-D6C7A45CBBAE", "Check-in", "fa fa-check-square", "", "8F8B272D-D351-485E-86D6-3EE5B7C84D99", 0 ); // Check-in

            #endregion

            #region LCBC Unattended Check-in

            RockMigrationHelper.UpdateWorkflowType( false, true, "LCBC Unattended Check-in", "", "8F8B272D-D351-485E-86D6-3EE5B7C84D99", "Check-in", "fa fa-list-ol", 0, true, 3, "A0BBC045-00E5-4485-88CB-69A73AC7C78D", 0 ); // LCBC Unattended Check-in
            RockMigrationHelper.UpdateWorkflowActionType( "D271B9C9-FC75-4958-8AE5-B6118C806B52", "Create Labels", 1, "F006D1F9-74B4-4AFD-82D4-96737404DFBA", true, false, "", "", 1, "", "0F8B5072-7B9C-40A7-84F5-130107B57D94" ); // LCBC Unattended Check-in:Save Attendance:Create Labels
            RockMigrationHelper.AddActionTypeAttributeValue( "0F8B5072-7B9C-40A7-84F5-130107B57D94", "52B8BCD3-A2DF-4990-BB00-0CE0FF3BCCCE", @"08B13135-1352-4FA4-B271-51FA3A8451E0" ); // LCBC Unattended Check-in:Save Attendance:Create Labels:Item Tag Label
            RockMigrationHelper.AddActionTypeAttributeValue( "0F8B5072-7B9C-40A7-84F5-130107B57D94", "CEF15819-4E30-4599-BC64-F97780A496A7", @"" ); // LCBC Unattended Check-in:Save Attendance:Create Labels:Order
            RockMigrationHelper.AddActionTypeAttributeValue( "0F8B5072-7B9C-40A7-84F5-130107B57D94", "94BC9BDF-1AF2-4D53-B8E2-79C5ADC8C285", @"False" ); // LCBC Unattended Check-in:Save Attendance:Create Labels:Active

            #endregion

            // Page: Item Tags
            RockMigrationHelper.AddPage( "CDF2C599-D341-42FD-B7DC-CD402EA96050", "66FA0143-F04C-4447-A67A-2A10A6BB1A2B", "Item Tags", "", "F533D1FD-5904-4232-BB2B-432567208FD4", "" ); // Site:Rock Check-in
            RockMigrationHelper.UpdateBlockType( "Idle Redirect", "Redirects user to a new url after a specific number of idle seconds.", "~/Blocks/Utility/IdleRedirect.ascx", "Utility", "49FC4B38-741E-4B0B-B395-7C1929340D88" );
            RockMigrationHelper.UpdateBlockType( "Item Tag Select", "Displays a number box to enter how many item tags you would like printed.", "~/Plugins/com_bemadev/Checkin/ItemTagSelect.ascx", "com_bemadev > Check-in", "09113783-54B7-4C9F-B626-218B0BA75646" );
            // Add Block to Page: Item Tags, Site: Rock Check-in
            RockMigrationHelper.AddBlock( true, "F533D1FD-5904-4232-BB2B-432567208FD4", "", "49FC4B38-741E-4B0B-B395-7C1929340D88", "Idle Timeout", "Main", "", "", 2, "0686D893-51FB-4932-8CC7-15980345D6E6" );
            // Add Block to Page: Item Tags, Site: Rock Check-in
            RockMigrationHelper.AddBlock( true, "F533D1FD-5904-4232-BB2B-432567208FD4", "", "09113783-54B7-4C9F-B626-218B0BA75646", "Item Tag Select", "Main", "", "", 1, "503BD55F-ED26-4021-9317-999A414B75C8" );
            // Attrib for BlockType: Idle Redirect:Idle Seconds
            RockMigrationHelper.UpdateBlockTypeAttribute( "49FC4B38-741E-4B0B-B395-7C1929340D88", "A75DFC58-7A1B-4799-BF31-451B2BBE38FF", "Idle Seconds", "IdleSeconds", "", "How many seconds of idle time to wait before redirecting user", 0, @"20", "1CAC7B16-041A-4F40-8AEE-A39DFA076C14" );
            // Attrib for BlockType: Idle Redirect:New Location
            RockMigrationHelper.UpdateBlockTypeAttribute( "49FC4B38-741E-4B0B-B395-7C1929340D88", "9C204CD0-1233-41C5-818A-C5DA439445AA", "New Location", "NewLocation", "", "The new location URL to send user to after idle time", 0, @"", "2254B67B-9CB1-47DE-A63D-D0B56051ECD4" );
            // Attrib for BlockType: Item Tag Select:Workflow Type
            RockMigrationHelper.UpdateBlockTypeAttribute( "09113783-54B7-4C9F-B626-218B0BA75646", "46A03F59-55D3-4ACE-ADD5-B4642225DD20", "Workflow Type", "WorkflowType", "", "The workflow type to activate for check-in", 0, @"", "CAE5DBEF-AE31-42AF-BD8B-E6484F893A13" );
            // Attrib for BlockType: Item Tag Select:Workflow Activity
            RockMigrationHelper.UpdateBlockTypeAttribute( "09113783-54B7-4C9F-B626-218B0BA75646", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Workflow Activity", "WorkflowActivity", "", "The name of the workflow activity to run on selection.", 1, @"", "613FB891-9999-45FB-9AB9-A9D1719DB9C0" );
            // Attrib for BlockType: Item Tag Select:Home Page
            RockMigrationHelper.UpdateBlockTypeAttribute( "09113783-54B7-4C9F-B626-218B0BA75646", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Home Page", "HomePage", "", "", 2, @"", "C01FAE24-5AA1-492C-9CC7-B8FC11E129DC" );
            // Attrib for BlockType: Item Tag Select:Previous Page
            RockMigrationHelper.UpdateBlockTypeAttribute( "09113783-54B7-4C9F-B626-218B0BA75646", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Previous Page", "PreviousPage", "", "", 3, @"", "AC712866-CD2F-4ED5-9F89-A3736FBA1FB9" );
            // Attrib for BlockType: Item Tag Select:Next Page
            RockMigrationHelper.UpdateBlockTypeAttribute( "09113783-54B7-4C9F-B626-218B0BA75646", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Next Page", "NextPage", "", "", 4, @"", "506E9720-83A5-48D0-AAFA-F8259E4C05EE" );
            // Attrib for BlockType: Item Tag Select:Multi-Person First Page (Family Check-in)
            RockMigrationHelper.UpdateBlockTypeAttribute( "09113783-54B7-4C9F-B626-218B0BA75646", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Multi-Person First Page (Family Check-in)", "MultiPersonFirstPage", "", "The first page for each person during family check-in.", 5, @"", "F382E6E8-C59C-41C3-8323-95187C3EA7EA" );
            // Attrib for BlockType: Item Tag Select:Multi-Person Last Page  (Family Check-in)
            RockMigrationHelper.UpdateBlockTypeAttribute( "09113783-54B7-4C9F-B626-218B0BA75646", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Multi-Person Last Page  (Family Check-in)", "MultiPersonLastPage", "", "The last page for each person during family check-in.", 6, @"", "01563A60-D2F5-46BA-9CAD-8F65EF2A7698" );
            // Attrib for BlockType: Item Tag Select:Multi-Person Done Page (Family Check-in)
            RockMigrationHelper.UpdateBlockTypeAttribute( "09113783-54B7-4C9F-B626-218B0BA75646", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Multi-Person Done Page (Family Check-in)", "MultiPersonDonePage", "", "The page to navigate to once all people have checked in during family check-in.", 7, @"", "ABCAFE43-AC95-40B7-B25C-78E50532B42F" );
            // Attrib for BlockType: Item Tag Select:Title
            RockMigrationHelper.UpdateBlockTypeAttribute( "09113783-54B7-4C9F-B626-218B0BA75646", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Title", "Title", "", "Title to display. Use {0} for person/schedule.", 8, @"{0}", "91665645-2FC8-4A27-9CAF-D509092B0DDA" );
            // Attrib for BlockType: Item Tag Select:Sub Title
            RockMigrationHelper.UpdateBlockTypeAttribute( "09113783-54B7-4C9F-B626-218B0BA75646", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Sub Title", "SubTitle", "", "Sub-Title to display. Use {0} for selected group name.", 9, @"{0}", "5FD74F6F-FF46-4481-9A5A-CD3FA55E332C" );
            // Attrib for BlockType: Item Tag Select:Caption
            RockMigrationHelper.UpdateBlockTypeAttribute( "09113783-54B7-4C9F-B626-218B0BA75646", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Caption", "Caption", "", "", 10, @"Select Location", "021B9635-938D-461D-9CE8-14B0ADDB65B0" );
            // Attrib for BlockType: Item Tag Select:No Option Message
            RockMigrationHelper.UpdateBlockTypeAttribute( "09113783-54B7-4C9F-B626-218B0BA75646", "9C204CD0-1233-41C5-818A-C5DA439445AA", "No Option Message", "NoOptionMessage", "", "Message to display when there are not any options available. Use {0} for person's name, and {1} for schedule name.", 11, @"Sorry, there are currently not any available locations that {0} can check into at {1}.", "44D298CB-392D-4A53-86CA-1D9313632429" );
            // Attrib for BlockType: Item Tag Select:No Option After Select Message
            RockMigrationHelper.UpdateBlockTypeAttribute( "09113783-54B7-4C9F-B626-218B0BA75646", "9C204CD0-1233-41C5-818A-C5DA439445AA", "No Option After Select Message", "NoOptionAfterSelectMessage", "", "Message to display when there are not any options available after location is selected. Use {0} for person's name", 12, @"Sorry, based on your selection, there are currently not any available times that {0} can check into.", "5D15A404-DC64-4A61-949E-4434F48B5CCD" );
            // Attrib Value for Block:Idle Timeout, Attribute:Idle Seconds Page: Item Tags, Site: Rock Check-in
            RockMigrationHelper.AddBlockAttributeValue( "0686D893-51FB-4932-8CC7-15980345D6E6", "1CAC7B16-041A-4F40-8AEE-A39DFA076C14", @"20" );
            // Attrib Value for Block:Idle Timeout, Attribute:New Location Page: Item Tags, Site: Rock Check-in
            RockMigrationHelper.AddBlockAttributeValue( "0686D893-51FB-4932-8CC7-15980345D6E6", "2254B67B-9CB1-47DE-A63D-D0B56051ECD4", @"/checkin/welcome" );
            // Attrib Value for Block:Item Tag Select, Attribute:Workflow Type Page: Item Tags, Site: Rock Check-in
            RockMigrationHelper.AddBlockAttributeValue( "503BD55F-ED26-4021-9317-999A414B75C8", "CAE5DBEF-AE31-42AF-BD8B-E6484F893A13", @"a0bbc045-00e5-4485-88cb-69a73ac7c78d" );
            // Attrib Value for Block:Item Tag Select, Attribute:Workflow Activity Page: Item Tags, Site: Rock Check-in
            RockMigrationHelper.AddBlockAttributeValue( "503BD55F-ED26-4021-9317-999A414B75C8", "613FB891-9999-45FB-9AB9-A9D1719DB9C0", @"" );
            // Attrib Value for Block:Item Tag Select, Attribute:Home Page Page: Item Tags, Site: Rock Check-in
            RockMigrationHelper.AddBlockAttributeValue( "503BD55F-ED26-4021-9317-999A414B75C8", "C01FAE24-5AA1-492C-9CC7-B8FC11E129DC", @"432b615a-75ff-4b14-9c99-3e769f866950" );
            // Attrib Value for Block:Item Tag Select, Attribute:Previous Page Page: Item Tags, Site: Rock Check-in
            RockMigrationHelper.AddBlockAttributeValue( "503BD55F-ED26-4021-9317-999A414B75C8", "AC712866-CD2F-4ED5-9F89-A3736FBA1FB9", @"6f0cb22b-e05b-42f1-a329-9219e81f6c34" );
            // Attrib Value for Block:Item Tag Select, Attribute:Next Page Page: Item Tags, Site: Rock Check-in
            RockMigrationHelper.AddBlockAttributeValue( "503BD55F-ED26-4021-9317-999A414B75C8", "506E9720-83A5-48D0-AAFA-F8259E4C05EE", @"eb789391-f355-4815-b151-0775bec4e8b6" );
            // Attrib Value for Block:Item Tag Select, Attribute:Multi-Person First Page (Family Check-in) Page: Item Tags, Site: Rock Check-in
            RockMigrationHelper.AddBlockAttributeValue( "503BD55F-ED26-4021-9317-999A414B75C8", "F382E6E8-C59C-41C3-8323-95187C3EA7EA", @"a1cbdaa4-94dd-4156-8260-5a3781e39fd0" );
            // Attrib Value for Block:Item Tag Select, Attribute:Multi-Person Last Page  (Family Check-in) Page: Item Tags, Site: Rock Check-in
            RockMigrationHelper.AddBlockAttributeValue( "503BD55F-ED26-4021-9317-999A414B75C8", "01563A60-D2F5-46BA-9CAD-8F65EF2A7698", @"043bb717-5799-446f-b8da-30e575110b0c" );
            // Attrib Value for Block:Item Tag Select, Attribute:Multi-Person Done Page (Family Check-in) Page: Item Tags, Site: Rock Check-in
            RockMigrationHelper.AddBlockAttributeValue( "503BD55F-ED26-4021-9317-999A414B75C8", "ABCAFE43-AC95-40B7-B25C-78E50532B42F", @"4af7a0e1-e991-4ae5-a2b5-c440f67a2e6a" );
            // Attrib Value for Block:Item Tag Select, Attribute:Title Page: Item Tags, Site: Rock Check-in
            RockMigrationHelper.AddBlockAttributeValue( "503BD55F-ED26-4021-9317-999A414B75C8", "91665645-2FC8-4A27-9CAF-D509092B0DDA", @"{0}" );
            // Attrib Value for Block:Item Tag Select, Attribute:Sub Title Page: Item Tags, Site: Rock Check-in
            RockMigrationHelper.AddBlockAttributeValue( "503BD55F-ED26-4021-9317-999A414B75C8", "5FD74F6F-FF46-4481-9A5A-CD3FA55E332C", @"{0}" );
            // Attrib Value for Block:Item Tag Select, Attribute:Caption Page: Item Tags, Site: Rock Check-in
            RockMigrationHelper.AddBlockAttributeValue( "503BD55F-ED26-4021-9317-999A414B75C8", "021B9635-938D-461D-9CE8-14B0ADDB65B0", @"Select Location" );
            // Attrib Value for Block:Item Tag Select, Attribute:No Option Message Page: Item Tags, Site: Rock Check-in
            RockMigrationHelper.AddBlockAttributeValue( "503BD55F-ED26-4021-9317-999A414B75C8", "44D298CB-392D-4A53-86CA-1D9313632429", @"Sorry, there are currently not any available locations that {0} can check into at {1}." );
            // Attrib Value for Block:Item Tag Select, Attribute:No Option After Select Message Page: Item Tags, Site: Rock Check-in
            RockMigrationHelper.AddBlockAttributeValue( "503BD55F-ED26-4021-9317-999A414B75C8", "5D15A404-DC64-4A61-949E-4434F48B5CCD", @"Sorry, based on your selection, there are currently not any available times that {0} can check into." );

            // Update other pages with new workflow and Item Tag page
            RockMigrationHelper.AddBlockAttributeValue( "887E090F-5468-44F0-BE84-9AE21D822987", "BA9AD11A-DB90-4BF6-ACDA-6FFB56C0358A", @"a0bbc045-00e5-4485-88cb-69a73ac7c78d" );
            RockMigrationHelper.AddBlockAttributeValue( "147CE2DA-1D94-4FFE-BEBA-7A6721D54604", "E4F7B489-39B8-49F9-8C8C-533275FAACDF", @"f533d1fd-5904-4232-bb2b-432567208fd4" );
            RockMigrationHelper.AddBlockAttributeValue( "96041DBC-A35D-4E7D-8A28-6D4E1E19A775", "1335CE7A-76CD-47AD-8BF3-D2A169F664EE", @"f533d1fd-5904-4232-bb2b-432567208fd4" );

        }
        public override void Down()
        {
        }
    }
}
