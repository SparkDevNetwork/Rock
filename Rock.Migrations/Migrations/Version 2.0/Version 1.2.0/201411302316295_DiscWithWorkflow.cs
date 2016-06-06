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
    public partial class DiscWithWorkflow : Rock.Migrations.RockMigration
    {
        public static readonly string DISC_BADGE = "6C491A10-E942-4CA5-8D13-ACBC28511714";
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            RockMigrationHelper.UpdatePersonAttribute( Rock.SystemGuid.FieldType.TEXT, "0B187C81-2106-4875-82B6-FBF1277AE23B", "Personality Type", "PersonalityType", "", "The one or two letters from a DISC assessment that closest approximates ones Natural behaviors.", 10, string.Empty, "C7B529C6-B6C8-45B5-B892-5D9821CEDDCD" );

            RockMigrationHelper.AddPage( "EBAA5140-4B8F-44B8-B1E8-C73B654E4B22", "5FEAF34C-7FB6-4A11-8A1E-C452EC7849BD", "DISC Assessment", "", "C8CEF4B0-4A09-46D2-9B6B-CD2B6D3078B1", "fa fa-bar-chart" ); // Site:External Website
            RockMigrationHelper.AddPageRoute( "C8CEF4B0-4A09-46D2-9B6B-CD2B6D3078B1", "DISC/{rckipid}" );

            RockMigrationHelper.AddPage( "08DBD8A5-2C35-4146-B4A8-0F7652348B25", "D65F783D-87A9-4CC9-8110-E83466A0EADB", "DISC Result", "", "039F770B-5734-4735-ABF1-B39B77C84AD0", "fa fa-bar-chart" ); // Site:Rock RMS
            RockMigrationHelper.UpdateBlockType( "DISC Result", "View the results of a DISC assessment.", "~/Blocks/Crm/DiscResult.ascx", "CRM", "0549519D-4048-4B28-89CC-94493B29BBD4" );

            // Add Block to Page: DISC Result, Site: Rock RMS
            RockMigrationHelper.AddBlock( "039F770B-5734-4735-ABF1-B39B77C84AD0", "", "0549519D-4048-4B28-89CC-94493B29BBD4", "DISC Result", "Main", "", "", 0, "E376E821-2980-4165-8CF8-717C6F562B9A" );

            // Add Block to Page: DISC Assessment, Site: External Website
            RockMigrationHelper.AddBlock( "C8CEF4B0-4A09-46D2-9B6B-CD2B6D3078B1", "", "A161D12D-FEA7-422F-B00E-A689629680E4", "DISC Assessment", "Main", "", "", 0, "BBD72B66-A29F-41B3-B8A7-31A0D25F44E2" );

            // Attrib for BlockType: Disc:Min Days To Retake
            RockMigrationHelper.AddBlockTypeAttribute( "A161D12D-FEA7-422F-B00E-A689629680E4", "A75DFC58-7A1B-4799-BF31-451B2BBE38FF", "Min Days To Retake", "MinDaysToRetake", "", "The number of days that must pass before the test can be taken again.", 0, @"30", "3162C5CD-1244-4CB9-9099-BC484CE090D3" );

            // Attrib for BlockType: Disc:Instructions
            RockMigrationHelper.AddBlockTypeAttribute( "A161D12D-FEA7-422F-B00E-A689629680E4", "1D0D3794-C210-48A8-8C68-3FBEC08A6BA5", "Instructions", "Instructions", "", "The text (HTML) to display at the top of the instructions section.  <span class='tip tip-lava'></span> <span class='tip tip-html'></span>", 0, @"
            <h2>Welcome!</h2>
            <p>
                {{ Person.NickName }}, in this assessment you are given a series of questions, each containing four phrases.
                Select one phrase that MOST describes you and one phrase that LEAST describes you.
            </p>
            <p>
                This assessment is environmentally sensitive, which means that you may score differently
                in different situations. In other words, you may act differently at home than you
                do on the job. So, as you complete the assessment you should focus on one environment
                for which you are seeking to understand yourself. For instance, if you are trying
                to understand yourself in marriage, you should only think of your responses to situations
                in the context of your marriage. On the other hand, if you want to know your behavioral
                needs on the job, then only think of how you would respond in the job context.
            </p>
            <p>
                One final thought as you give your responses. On these kinds of assessments, it
                is often best and easiest if you respond quickly and do not deliberate too long
                on each question. Your response on one question will not unduly influence your scores,
                so simply answer as quickly as possible and enjoy the process. Don't get too hung
                up, if none of the phrases describe you or if there are some phrases that seem too
                similar, just go with your instinct.
            </p>
            <p>
                When you are ready, click the 'Start' button to proceed.
            </p>
", "5F2BA045-D481-4908-A137-DE28C71AAF67" );

            // Attrib Value for Block:DISC Assessment, Attribute:Min Days To Retake Page: DISC Assessment, Site: External Website
            RockMigrationHelper.AddBlockAttributeValue( "BBD72B66-A29F-41B3-B8A7-31A0D25F44E2", "3162C5CD-1244-4CB9-9099-BC484CE090D3", @"30" );

            // Attrib Value for Block:DISC Assessment, Attribute:Instructions Page: DISC Assessment, Site: External Website
            RockMigrationHelper.AddBlockAttributeValue( "BBD72B66-A29F-41B3-B8A7-31A0D25F44E2", "5F2BA045-D481-4908-A137-DE28C71AAF67", @"<h2>Welcome!</h2>
            <p>
                {{ Person.NickName }}, in this assessment you are given a series of questions, each containing four phrases.
                Select one phrase that MOST describes you and one phrase that LEAST describes you.
            </p>
            <p>
                This assessment is environmentally sensitive, which means that you may score differently
                in different situations. In other words, you may act differently at home than you
                do on the job. So, as you complete the assessment you should focus on one environment
                for which you are seeking to understand yourself. For instance, if you are trying
                to understand yourself in marriage, you should only think of your responses to situations
                in the context of your marriage. On the other hand, if you want to know your behavioral
                needs on the job, then only think of how you would respond in the job context.
            </p>
            <p>
                One final thought as you give your responses. On these kinds of assessments, it
                is often best and easiest if you respond quickly and do not deliberate too long
                on each question. Your response on one question will not unduly influence your scores,
                so simply answer as quickly as possible and enjoy the process. Don't get too hung
                up, if none of the phrases describe you or if there are some phrases that seem too
                similar, just go with your instinct.
            </p>
            <p>
                When you are ready, click the 'Start' button to proceed.
            </p>
" );

            // DISC Workflow for person profile action list
            RockMigrationHelper.UpdateWorkflowType( false, true, "DISC Request", "Used to request a person take the DISC analysis test.", "BBAE05FD-8192-4616-A71E-903A927E0D90", "Request", "fa fa-bar-chart", 0, false, 0, "885CBA61-44EA-4B4A-B6E1-289041B6A195" ); // DISC Request

            RockMigrationHelper.UpdateWorkflowTypeAttribute( "885CBA61-44EA-4B4A-B6E1-289041B6A195", "C28C7BF3-A552-4D77-9408-DEDCF760CED0", "Custom Message", "CustomMessage", "A custom message you would like to include in the request.  Otherwise the default will be used.", 1, @"DISC is a popular four quadrant behavioral and personality assessments that we like to use to help us understand you and your needs better.

The results of the test will help us form a stronger relationship with you and can also be used for building healthier teams and groups.

Please take less than five minutes and take the assessment using the button below - it will go a long way toward helping us help you.  Thanks in advance!", "840E6A84-9F83-4482-92D1-6F635F062251" ); // DISC Request:Custom Message

            RockMigrationHelper.UpdateWorkflowTypeAttribute( "885CBA61-44EA-4B4A-B6E1-289041B6A195", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "OptOut", "OptOut", "Holds a 1 if the person has opted out of bulk or email requests.", 3, @"False", "82B3A401-463C-4AD5-8AD1-23D067E48458" ); // DISC Request:OptOut

            RockMigrationHelper.UpdateWorkflowTypeAttribute( "885CBA61-44EA-4B4A-B6E1-289041B6A195", "E4EAB7B2-0B76-429B-AFE4-AD86D7428C70", "Person", "Person", "The person that you are requesting the assessment for.  If initiated directly from the person profile record (using 'Actions' option), this value will automatically be populated.", 0, @"", "C0BC984C-84C3-494B-A861-49840E452F68" ); // DISC Request:Person

            RockMigrationHelper.UpdateWorkflowTypeAttribute( "885CBA61-44EA-4B4A-B6E1-289041B6A195", "E4EAB7B2-0B76-429B-AFE4-AD86D7428C70", "Sender", "Sender", "The person sending the request.", 2, @"", "D44036C8-2787-4942-A70E-5E85087787EB" ); // DISC Request:Sender

            RockMigrationHelper.UpdateWorkflowTypeAttribute( "885CBA61-44EA-4B4A-B6E1-289041B6A195", "DD7ED4C0-A9E0-434F-ACFE-BD4F56B043DF", "Warning Message", "WarningMessage", "A warning message that may be displayed if the person has opted out from bulk and email requests.", 4, @"", "74E7BD11-BE21-4256-B02A-E97AD54C7511" ); // DISC Request:Warning Message

            RockMigrationHelper.AddAttributeQualifier( "840E6A84-9F83-4482-92D1-6F635F062251", "numberofrows", @"6", "DD982F91-6FD1-4D42-8DD4-135029B0B691" ); // DISC Request:Custom Message:numberofrows

            RockMigrationHelper.AddAttributeQualifier( "82B3A401-463C-4AD5-8AD1-23D067E48458", "falsetext", @"No", "A388A9DB-498A-464C-993F-788FA17A67BD" ); // DISC Request:OptOut:falsetext

            RockMigrationHelper.AddAttributeQualifier( "82B3A401-463C-4AD5-8AD1-23D067E48458", "truetext", @"Yes", "067873ED-0583-495E-A448-66BA0D0DCACF" ); // DISC Request:OptOut:truetext

            RockMigrationHelper.UpdateWorkflowActivityType( "885CBA61-44EA-4B4A-B6E1-289041B6A195", true, "Launch From Person Profile", "When this workflow is initiated from the Person Profile page, the \"Entity\" will have a value so the first action will run successfully, and the workflow will then be persisted.", true, 0, "4A8E2E61-FE0D-4AB3-BA48-9419A20E4541" ); // DISC Request:Launch From Person Profile

            RockMigrationHelper.UpdateWorkflowActionForm( @"{{ Workflow.WarningMessage }}", @"", "Send^fdc397cd-8b4a-436e-bea1-bce2e6717c03^^Your information has been submitted successfully.|", "", true, "", "4AFAB342-D584-4B79-B038-A99C0C469D74" ); // DISC Request:Launch From Person Profile:Custom Message

            RockMigrationHelper.UpdateWorkflowActionFormAttribute( "4AFAB342-D584-4B79-B038-A99C0C469D74", "C0BC984C-84C3-494B-A861-49840E452F68", 0, false, true, false, "82351FDD-D82F-49F9-B85A-3FF46E7F8D0E" ); // DISC Request:Launch From Person Profile:Custom Message:Person

            RockMigrationHelper.UpdateWorkflowActionFormAttribute( "4AFAB342-D584-4B79-B038-A99C0C469D74", "840E6A84-9F83-4482-92D1-6F635F062251", 1, true, false, false, "94B20E87-9C79-4C37-926C-0426496AC722" ); // DISC Request:Launch From Person Profile:Custom Message:Custom Message

            RockMigrationHelper.UpdateWorkflowActionFormAttribute( "4AFAB342-D584-4B79-B038-A99C0C469D74", "D44036C8-2787-4942-A70E-5E85087787EB", 2, false, true, false, "196541C8-D5EC-4189-A2A6-47E669565C56" ); // DISC Request:Launch From Person Profile:Custom Message:Sender

            RockMigrationHelper.UpdateWorkflowActionFormAttribute( "4AFAB342-D584-4B79-B038-A99C0C469D74", "82B3A401-463C-4AD5-8AD1-23D067E48458", 3, false, true, false, "058D0466-8ABF-4FA8-9EEC-3A80CF5BD1DF" ); // DISC Request:Launch From Person Profile:Custom Message:OptOut

            RockMigrationHelper.UpdateWorkflowActionFormAttribute( "4AFAB342-D584-4B79-B038-A99C0C469D74", "74E7BD11-BE21-4256-B02A-E97AD54C7511", 4, false, true, false, "B2E0A2A5-CE42-441C-A2E2-B6058A4577BF" ); // DISC Request:Launch From Person Profile:Custom Message:Warning Message

            RockMigrationHelper.UpdateWorkflowActionType( "4A8E2E61-FE0D-4AB3-BA48-9419A20E4541", "Complete the Workflow", 7, "EEDA4318-F014-4A46-9C76-4C052EF81AA1", true, true, "", "", 1, "", "EA4FF8DD-0E66-4C98-84FC-845DEAB76A61" ); // DISC Request:Launch From Person Profile:Complete the Workflow

            RockMigrationHelper.UpdateWorkflowActionType( "4A8E2E61-FE0D-4AB3-BA48-9419A20E4541", "Send Email Action", 6, "66197B01-D1F0-4924-A315-47AD54E030DE", true, false, "", "", 1, "", "666FC137-BC95-49BE-A976-0BFF2417F44C" ); // DISC Request:Launch From Person Profile:Send Email Action

            RockMigrationHelper.UpdateWorkflowActionType( "4A8E2E61-FE0D-4AB3-BA48-9419A20E4541", "Check for Opt Out", 2, "A41216D6-6FB0-4019-B222-2C29B4519CF4", true, false, "", "", 1, "", "A5967573-B263-4003-AED3-36862D4A0251" ); // DISC Request:Launch From Person Profile:Check for Opt Out

            RockMigrationHelper.UpdateWorkflowActionType( "4A8E2E61-FE0D-4AB3-BA48-9419A20E4541", "Persist Workflow", 3, "F1A39347-6FE0-43D4-89FB-544195088ECF", true, false, "", "", 1, "", "555A729C-5EC4-4C83-B35F-036234E5EFCC" ); // DISC Request:Launch From Person Profile:Persist Workflow

            RockMigrationHelper.UpdateWorkflowActionType( "4A8E2E61-FE0D-4AB3-BA48-9419A20E4541", "Set Sender", 1, "24B7D5E6-C30F-48F4-9D7E-AF45A342CF3A", true, false, "", "", 1, "", "28CBC348-0C56-4B2A-A1CE-5504A1CD6A1E" ); // DISC Request:Launch From Person Profile:Set Sender

            RockMigrationHelper.UpdateWorkflowActionType( "4A8E2E61-FE0D-4AB3-BA48-9419A20E4541", "Set Person", 0, "972F19B9-598B-474B-97A4-50E56E7B59D2", true, false, "", "", 1, "", "07A37FA1-FA95-4E86-B797-200D7573EC5F" ); // DISC Request:Launch From Person Profile:Set Person

            RockMigrationHelper.UpdateWorkflowActionType( "4A8E2E61-FE0D-4AB3-BA48-9419A20E4541", "Set Warning", 4, "C789E457-0783-44B3-9D8F-2EBAB5F11110", true, false, "", "82B3A401-463C-4AD5-8AD1-23D067E48458", 1, "True", "080944DB-B45F-41CA-BF5E-1F697B503C16" ); // DISC Request:Launch From Person Profile:Set Warning

            RockMigrationHelper.UpdateWorkflowActionType( "4A8E2E61-FE0D-4AB3-BA48-9419A20E4541", "Custom Message", 5, "486DC4FA-FCBC-425F-90B0-E606DA8A9F68", true, false, "4AFAB342-D584-4B79-B038-A99C0C469D74", "", 1, "", "1A08A4EC-B2C6-43B5-926F-2F86CFA35102" ); // DISC Request:Launch From Person Profile:Custom Message

            RockMigrationHelper.AddActionTypeAttributeValue( "07A37FA1-FA95-4E86-B797-200D7573EC5F", "9392E3D7-A28B-4CD8-8B03-5E147B102EF1", @"False" ); // DISC Request:Launch From Person Profile:Set Person:Active

            RockMigrationHelper.AddActionTypeAttributeValue( "07A37FA1-FA95-4E86-B797-200D7573EC5F", "61E6E1BC-E657-4F00-B2E9-769AAA25B9F7", @"c0bc984c-84c3-494b-a861-49840e452f68" ); // DISC Request:Launch From Person Profile:Set Person:Attribute

            RockMigrationHelper.AddActionTypeAttributeValue( "07A37FA1-FA95-4E86-B797-200D7573EC5F", "AD4EFAC4-E687-43DF-832F-0DC3856ABABB", @"" ); // DISC Request:Launch From Person Profile:Set Person:Order

            RockMigrationHelper.AddActionTypeAttributeValue( "07A37FA1-FA95-4E86-B797-200D7573EC5F", "1246C53A-FD92-4E08-ABDE-9A6C37E70C7B", @"False" ); // DISC Request:Launch From Person Profile:Set Person:Use Id instead of Guid

            RockMigrationHelper.AddActionTypeAttributeValue( "28CBC348-0C56-4B2A-A1CE-5504A1CD6A1E", "DE9CB292-4785-4EA3-976D-3826F91E9E98", @"False" ); // DISC Request:Launch From Person Profile:Set Sender:Active

            RockMigrationHelper.AddActionTypeAttributeValue( "28CBC348-0C56-4B2A-A1CE-5504A1CD6A1E", "89E9BCED-91AB-47B0-AD52-D78B0B7CB9E8", @"" ); // DISC Request:Launch From Person Profile:Set Sender:Order

            RockMigrationHelper.AddActionTypeAttributeValue( "28CBC348-0C56-4B2A-A1CE-5504A1CD6A1E", "BBED8A83-8BB2-4D35-BAFB-05F67DCAD112", @"d44036c8-2787-4942-a70e-5e85087787eb" ); // DISC Request:Launch From Person Profile:Set Sender:Person Attribute

            RockMigrationHelper.AddActionTypeAttributeValue( "A5967573-B263-4003-AED3-36862D4A0251", "A18C3143-0586-4565-9F36-E603BC674B4E", @"False" ); // DISC Request:Launch From Person Profile:Check for Opt Out:Active

            RockMigrationHelper.AddActionTypeAttributeValue( "A5967573-B263-4003-AED3-36862D4A0251", "FA7C685D-8636-41EF-9998-90FFF3998F76", @"" ); // DISC Request:Launch From Person Profile:Check for Opt Out:Order

            RockMigrationHelper.AddActionTypeAttributeValue( "A5967573-B263-4003-AED3-36862D4A0251", "F3B9908B-096F-460B-8320-122CF046D1F9", @"DECLARE @PersonAliasGuid uniqueidentifier = '{{ Workflow.Person_unformatted }}'

SELECT  CASE
    WHEN EXISTS ( SELECT 1
        FROM [Person] P
        INNER JOIN [PersonAlias] PA ON PA.[Guid] = @PersonAliasGuid AND P.[Id] = PA.[PersonId]
        WHERE P.[EmailPreference] <> 0 )
    THEN 'True'
    ELSE 'False'
    END" ); // DISC Request:Launch From Person Profile:Check for Opt Out:SQLQuery

            RockMigrationHelper.AddActionTypeAttributeValue( "A5967573-B263-4003-AED3-36862D4A0251", "56997192-2545-4EA1-B5B2-313B04588984", @"82b3a401-463c-4ad5-8ad1-23d067e48458" ); // DISC Request:Launch From Person Profile:Check for Opt Out:Result Attribute

            RockMigrationHelper.AddActionTypeAttributeValue( "555A729C-5EC4-4C83-B35F-036234E5EFCC", "50B01639-4938-40D2-A791-AA0EB4F86847", @"False" ); // DISC Request:Launch From Person Profile:Persist Workflow:Active

            RockMigrationHelper.AddActionTypeAttributeValue( "555A729C-5EC4-4C83-B35F-036234E5EFCC", "86F795B0-0CB6-4DA4-9CE4-B11D0922F361", @"" ); // DISC Request:Launch From Person Profile:Persist Workflow:Order

            RockMigrationHelper.AddActionTypeAttributeValue( "080944DB-B45F-41CA-BF5E-1F697B503C16", "D7EAA859-F500-4521-9523-488B12EAA7D2", @"False" ); // DISC Request:Launch From Person Profile:Set Warning:Active

            RockMigrationHelper.AddActionTypeAttributeValue( "080944DB-B45F-41CA-BF5E-1F697B503C16", "44A0B977-4730-4519-8FF6-B0A01A95B212", @"74e7bd11-be21-4256-b02a-e97ad54c7511" ); // DISC Request:Launch From Person Profile:Set Warning:Attribute

            RockMigrationHelper.AddActionTypeAttributeValue( "080944DB-B45F-41CA-BF5E-1F697B503C16", "57093B41-50ED-48E5-B72B-8829E62704C8", @"" ); // DISC Request:Launch From Person Profile:Set Warning:Order

            RockMigrationHelper.AddActionTypeAttributeValue( "080944DB-B45F-41CA-BF5E-1F697B503C16", "E5272B11-A2B8-49DC-860D-8D574E2BC15C", @"<div class=""alert alert-warning"">{{ Workflow.Person }} has previously opted out from email and bulk requests.  Make sure you want to override this preference.</div>" ); // DISC Request:Launch From Person Profile:Set Warning:Text Value|Attribute Value

            RockMigrationHelper.AddActionTypeAttributeValue( "1A08A4EC-B2C6-43B5-926F-2F86CFA35102", "234910F2-A0DB-4D7D-BAF7-83C880EF30AE", @"False" ); // DISC Request:Launch From Person Profile:Custom Message:Active

            RockMigrationHelper.AddActionTypeAttributeValue( "1A08A4EC-B2C6-43B5-926F-2F86CFA35102", "C178113D-7C86-4229-8424-C6D0CF4A7E23", @"" ); // DISC Request:Launch From Person Profile:Custom Message:Order

            RockMigrationHelper.AddActionTypeAttributeValue( "666FC137-BC95-49BE-A976-0BFF2417F44C", "36197160-7D3D-490D-AB42-7E29105AFE91", @"False" ); // DISC Request:Launch From Person Profile:Send Email Action:Active

            RockMigrationHelper.AddActionTypeAttributeValue( "666FC137-BC95-49BE-A976-0BFF2417F44C", "9F5F7CEC-F369-4FDF-802A-99074CE7A7FC", @"d44036c8-2787-4942-a70e-5e85087787eb" ); // DISC Request:Launch From Person Profile:Send Email Action:From Email Address|Attribute Value

            RockMigrationHelper.AddActionTypeAttributeValue( "666FC137-BC95-49BE-A976-0BFF2417F44C", "D1269254-C15A-40BD-B784-ADCC231D3950", @"" ); // DISC Request:Launch From Person Profile:Send Email Action:Order

            RockMigrationHelper.AddActionTypeAttributeValue( "666FC137-BC95-49BE-A976-0BFF2417F44C", "0C4C13B8-7076-4872-925A-F950886B5E16", @"c0bc984c-84c3-494b-a861-49840e452f68" ); // DISC Request:Launch From Person Profile:Send Email Action:Send To Email Address|Attribute Value

            RockMigrationHelper.AddActionTypeAttributeValue( "666FC137-BC95-49BE-A976-0BFF2417F44C", "5D9B13B6-CD96-4C7C-86FA-4512B9D28386", @"DISC Assessment Request from {{ Workflow.Sender }}" ); // DISC Request:Launch From Person Profile:Send Email Action:Subject

            RockMigrationHelper.AddActionTypeAttributeValue( "666FC137-BC95-49BE-A976-0BFF2417F44C", "4D245B9E-6B03-46E7-8482-A51FBA190E4D", @"{{ GlobalAttribute.EmailStyles }}
{{ GlobalAttribute.EmailHeader }}
<p>{{ Person.NickName }},</p>

<p>{{ Workflow.CustomMessage | NewlineToBr }}</p>

<p><a href=""{{ GlobalAttribute.PublicApplicationRoot }}DISC/{{ Person.UrlEncodedKey }}"">Take DISC Analysis Test </a></p>

<p><a href=""{{ GlobalAttribute.PublicApplicationRoot }}Unsubscribe/{{ Person.UrlEncodedKey }}"">I&#39;m no longer involved with {{ GlobalAttribute.OrganizationName }}. Please remove me from all future communications.</a></p>

<p>- {{ Workflow.Sender }}</p>

{{ GlobalAttribute.EmailFooter }}" ); // DISC Request:Launch From Person Profile:Send Email Action:Body

            RockMigrationHelper.AddActionTypeAttributeValue( "EA4FF8DD-0E66-4C98-84FC-845DEAB76A61", "0CA0DDEF-48EF-4ABC-9822-A05E225DE26C", @"False" ); // DISC Request:Launch From Person Profile:Complete the Workflow:Active

            RockMigrationHelper.AddActionTypeAttributeValue( "EA4FF8DD-0E66-4C98-84FC-845DEAB76A61", "25CAD4BE-5A00-409D-9BAB-E32518D89956", @"" ); // DISC Request:Launch From Person Profile:Complete the Workflow:Order

            // Update the Bio block's WorkflowAction attribute value to include this new DISC WF
            RockMigrationHelper.AddBlockAttributeValue( "B5C1FDB6-0224-43E4-8E26-6B2EAF86253A", "7197A0FB-B330-43C4-8E62-F3C14F649813", "885CBA61-44EA-4B4A-B6E1-289041B6A195", appendToExisting: true );

            // Badge items
            // Ensure the badge is a registered EntityType
            RockMigrationHelper.UpdateEntityType( "Rock.PersonProfile.Badge.DISC", "DISC", "Rock.PersonProfile.Badge.DISC, Rock, Version=1.1.2.0, Culture=neutral, PublicKeyToken=null", false, true, "6D29DB44-EDC5-42AA-B42C-482BC0920AD0" );

            // Ensure the PersonBadge for Rock.PersonProfile.Badge.DISC is added
            RockMigrationHelper.UpdatePersonBadge( "DISC Personality Assessment Result", "Shows a small chart of a person's DISC personality assessment results and links to the details of their assessment.",
                "Rock.PersonProfile.Badge.DISC", 0, DISC_BADGE );

            // Add/Update the Active Attribute
            RockMigrationHelper.AddPersonBadgeAttribute( DISC_BADGE, Rock.SystemGuid.FieldType.BOOLEAN,
                "Active", "Active", "Should Service be used?", 0, string.Empty, "F1F4AA7A-E657-4BA4-B3E2-6EEEB5A839CE" );

            // Add/Update the Order Attribute
            RockMigrationHelper.AddPersonBadgeAttribute( DISC_BADGE, Rock.SystemGuid.FieldType.INTEGER,
                "Order", "Order", "The order that this service should be used (priority)", 1, string.Empty, "EC96AA8D-31D8-4281-AE5B-6DCB4FEC6234" );

            // Add/Update the DISC Result Detail Attribute
            RockMigrationHelper.AddPersonBadgeAttribute( DISC_BADGE, Rock.SystemGuid.FieldType.PAGE_REFERENCE,
                "DISC Result Detail", "DISCResultDetail", "Page to show the details of the DISC assessment results. If blank no link is created.", 2, string.Empty, "0EB9498A-5F92-41E2-94CD-F5F86B4E7D6F" );

            // add the badge to PersonBadge DISC Result Detail page guid
            RockMigrationHelper.AddPersonBadgeAttributeValue( DISC_BADGE, "0EB9498A-5F92-41E2-94CD-F5F86B4E7D6F", "039F770B-5734-4735-ABF1-B39B77C84AD0" );

            // add the new badge to the person profile's Badge 3 block
            RockMigrationHelper.AddBlockAttributeValue( "F3E6CC14-C540-4FFC-A5A9-48AD9CC0A61B", "F5AB231E-3836-4D52-BD03-BF79773C237A", DISC_BADGE, appendToExisting: true );
        }

        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            // delete badge items
            // delete the Badge 3 attribute value
            RemoveBlockAttributeValue( "F3E6CC14-C540-4FFC-A5A9-48AD9CC0A61B", "F5AB231E-3836-4D52-BD03-BF79773C237A", DISC_BADGE );

            // delete PersonBadgeAttribute
            RockMigrationHelper.DeleteAttribute( "F1F4AA7A-E657-4BA4-B3E2-6EEEB5A839CE" );
            RockMigrationHelper.DeleteAttribute( "EC96AA8D-31D8-4281-AE5B-6DCB4FEC6234" );
            RockMigrationHelper.DeleteAttribute( "0EB9498A-5F92-41E2-94CD-F5F86B4E7D6F" );

            DeletePersonBadge( DISC_BADGE );
            RockMigrationHelper.DeleteEntityType( "6D29DB44-EDC5-42AA-B42C-482BC0920AD0" );

            // blocks, pages, blocktypes, etc.
            RockMigrationHelper.DeleteBlock( "BBD72B66-A29F-41B3-B8A7-31A0D25F44E2" );
            RockMigrationHelper.DeleteBlock( "E376E821-2980-4165-8CF8-717C6F562B9A" );

            RockMigrationHelper.DeletePage( "C8CEF4B0-4A09-46D2-9B6B-CD2B6D3078B1" );
            RockMigrationHelper.DeletePage( "039F770B-5734-4735-ABF1-B39B77C84AD0" );

            RockMigrationHelper.DeleteBlockType( "0549519D-4048-4B28-89CC-94493B29BBD4" );
            // delete the Personality Type attribute 
            RockMigrationHelper.DeleteAttribute( "C7B529C6-B6C8-45B5-B892-5D9821CEDDCD" );
        }

        /// <summary>
        /// Deletes the person badge.
        /// </summary>
        /// <param name="guid">The unique identifier.</param>
        public void DeletePersonBadge( string guid )
        {
            Sql( string.Format( @"
            DELETE FROM [PersonBadge] where [Guid] = '{0}'", guid ) );
        }

        /// <summary>
        /// Removes only the given value string from the comma delimited list of values in an AttributeValue.
        /// It assumes the value is specific enough and only appears once in the string.
        /// </summary>
        /// <param name="blockGuid">The block GUID.</param>
        /// <param name="attributeGuid">The attribute GUID.</param>
        /// <param name="value">The value to remove.</param>
        public void RemoveBlockAttributeValue( string blockGuid, string attributeGuid, string value )
        {
            Sql( string.Format( @"
                -- replace a string in an AttributeValue
                DECLARE @BlockId int
                SET @BlockId = (SELECT [Id] FROM [Block] WHERE [Guid] = '{0}')

                DECLARE @AttributeId int
                SET @AttributeId = (SELECT [Id] FROM [Attribute] WHERE [Guid] = '{1}')

                DECLARE @TheValue NVARCHAR(MAX) = '{2}'

                -- Get the current value
                IF EXISTS (SELECT 1 FROM [AttributeValue] WHERE [AttributeId] = @AttributeId AND [EntityId] = @BlockId )
                BEGIN
                    -- replace if there is a trailing comma
                    -- and replace if there is a leading comma
                    UPDATE [AttributeValue]
                    SET [Value] = 
                        REPLACE(
                            REPLACE([Value], '{2},', ''),
                            ',{2}', ''
                        )
                    WHERE [AttributeId] = @AttributeId AND [EntityId] = @BlockId
                END
",
                    blockGuid,
                    attributeGuid,
                    value.Replace( "'", "''" )
                )
            );
        }
    }
}
