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
    public partial class UpdateDiscRequestWorkflow : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            UpdateDiscWorkflow();
            UpdateDiscDefinedValueDescription();
            UpdateDiscScoreConversion();
        }

        private void UpdateDiscDefinedValueDescription()
        {
            Sql( @"

			UPDATE 
				[DefinedValue]
			SET
				[Description]='You enjoy maintaining high standards and following procedures correctly. You may have difficulty accepting the lack of quality in projects or people around you.'
			WHERE
				[Guid]='B428B0CA-2E41-4B3E-9A37-5B618C116CA3'

			UPDATE 
				[DefinedValue]
			SET
				[Description]='You enjoy maintaining high standards and following procedures correctly while also accomplishing the task. You may be seen by others as being only concerned with accomplishing a goal to the exclusion of the people in the process.'
			WHERE
				[Guid]='C742748E-E86F-4145-962B-3468CC563D71'
		
			UPDATE 
				[DefinedValue]
			SET
				[Description]='You enjoy maintaining high standards and following procedures correctly while also valuing having fun with people in the process. You may be frustrated because it is difficult to maintaining excellence while having fun with people.'
			WHERE
				[Guid]='227A4C9A-F92B-48D7-9E2B-62D328369C3D'

			UPDATE 
				[DefinedValue]
			SET
				[Description]='You enjoy maintaining high standards and following procedures correctly while keeping a stable and consistent pace. You may tend to hold high standards for yourself, your work as well as those around you; which may cause others not to meet your expectations.'
			WHERE
				[Guid]='3FDC0A3C-A826-4252-90A4-4BF710C12311'

			UPDATE 
				[DefinedValue]
			SET
				[Description]='You enjoy accomplishing the end goal and are willing to do whatever it takes to accomplish the task. You may hurt people in accomplishing the goal, but you feel the goal is more important.'
			WHERE
				[Guid]='8B33090D-DD62-4BBB-BFDA-2DC67F26745D'

			UPDATE 
				[DefinedValue]
			SET
				[Description]='You enjoy accomplishing the end goal as well as all of the steps needed to accomplish the goal. You have an excellent ability to see the end goal and know the specific steps needed to accomplish this which can make it difficult to work with you on a team.'
			WHERE
				[Guid]='20706B1C-B8CC-45AB-AF41-4D52E55583BB'

			UPDATE 
				[DefinedValue]
			SET
				[Description]='You enjoy accomplishing the end goal and want to bring people with you to accomplish the task. You are able to persuade people to get on board, but when they are no longer needed, you can move on without them.'
			WHERE
				[Guid]='43164D7B-9521-40FF-8877-FAFA1C753284'

			UPDATE 
				[DefinedValue]
			SET
				[Description]='You enjoy accomplishing the end goal and want to accomplish it without alienating people in the process. You may struggle between accomplishing the task and keeping people on board, the tradeoff is often difficult for you.'
			WHERE
				[Guid]='09D96C14-1F68-4454-954B-7EC8898B0A86'

			UPDATE 
				[DefinedValue]
			SET
				[Description]='You enjoy bringing energy and life to people that you are with. You may not be able to keep focused on the task, but you enjoy hanging with people.'
			WHERE
				[Guid]='AD076E20-ADAB-4DF3-9598-8F9FCC19B9F0'

			UPDATE 
				[DefinedValue]
			SET
				[Description]='You enjoy bringing energy and life to people that you are with as well as following rules and procedures. Because you have competing behavioral needs of enjoying people and focusing on details; you may find yourself torn between meeting the needs of people and paying attention to the rules or processes necessary to accomplish a task well.'
			WHERE
				[Guid]='879149E8-D396-41D9-92C2-EC9E62BE8916'

			UPDATE 
				[DefinedValue]
			SET
				[Description]='You enjoy bringing energy and life to people that you are with as well as following rules and procedures. Because you have competing behavioral needs of enjoying people and focusing on details; you may find yourself torn between meeting the needs of people and paying attention to the rules or processes necessary to accomplish a task well.'
			WHERE
				[Guid]='879149E8-D396-41D9-92C2-EC9E62BE8916'


			UPDATE 
				[DefinedValue]
			SET
				[Description]='You enjoy bringing energy and life to people that you are with and moving them in a direction to accomplish a task. You may not always accomplish the task, because you don’t want to hurt peoples’ feelings.'
			WHERE
				[Guid]='03B518C2-F9EB-4B84-A470-45EBEBAB7F17'

			UPDATE 
				[DefinedValue]
			SET
				[Description]='You enjoy bringing energy and life to people that you are with and moving them in a direction to accomplish a task. You may not always accomplish the task, because you don’t want to hurt peoples’ feelings.'
			WHERE
				[Guid]='03B518C2-F9EB-4B84-A470-45EBEBAB7F17'

			UPDATE 
				[DefinedValue]
			SET
				[Description]='You enjoy bringing energy and life to people that you are with and keeping everyone content and on board in long term relationships. You would tend to focus more on people than the task at hand, which may frustrate task oriented people.'
			WHERE
				[Guid]='99D3129C-295C-471A-8E8A-804A9EBF36B9'

			UPDATE 
				[DefinedValue]
			SET
				[Description]='You enjoy a stable and consistent pace with everyone in harmony. You may have difficulty with change because it usually involves some sense of the unknown.'
			WHERE
				[Guid]='03DE220E-7924-47A5-90D4-D6383285340C'

			UPDATE 
				[DefinedValue]
			SET
				[Description]='You enjoy a stable and consistent pace with everyone in harmony; while maintaining high standards and precise procedures. You may tend to hold high standards for yourself, your work as well as those around you; which may cause others not to meet your expectations.'
			WHERE
				[Guid]='AC246F37-0858-4692-A373-C0F125036355'

			UPDATE 
				[DefinedValue]
			SET
				[Description]='You enjoy a stable and consistent pace with everyone in harmony while also wanting to accomplish a goal and/or task. You may struggle with valuing how to get the task done, while attempting to keep things consistent and everyone in harmony.'
			WHERE
				[Guid]='5580070A-3D01-4CB6-BC45-2645E222DC21'

			UPDATE 
				[DefinedValue]
			SET
				[Description]='You enjoy a stable and consistent pace with everyone in harmony as well as having fun in the moment. You would tend to focus more on people than the task at hand, which may frustrate task oriented people.'
			WHERE
				[Guid]='05868DFC-BED9-497D-8998-6B5474280568'
" );
            RockMigrationHelper.DeleteAttribute( "A500116E-37CB-400F-996B-A56940FD24E9" ); // RelationshipMatrix
        }

        private void UpdateDiscWorkflow()
        {
            RockMigrationHelper.UpdateEntityType( "Rock.Workflow.Action.RunLava", "BC21E57A-1477-44B3-A7C2-61A806118945", false, true );

            RockMigrationHelper.UpdateWorkflowActionEntityAttribute( "BC21E57A-1477-44B3-A7C2-61A806118945", "1D0D3794-C210-48A8-8C68-3FBEC08A6BA5", "Lava", "Value", "The <span class='tip tip-lava'></span> to run.", 0, @"", "F1F6F9D6-FDC5-489C-8261-4B9F45B3EED4" ); // Rock.Workflow.Action.RunLava:Lava
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute( "BC21E57A-1477-44B3-A7C2-61A806118945", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Active", "Active", "Should Service be used?", 0, @"False", "F1924BDC-9B79-4018-9D4A-C3516C87A514" ); // Rock.Workflow.Action.RunLava:Active
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute( "BC21E57A-1477-44B3-A7C2-61A806118945", "33E6DF69-BDFA-407A-9744-C175B60643AE", "Attribute", "Attribute", "The attribute to store the result in.", 1, @"", "431273C6-342D-4030-ADC7-7CDEDC7F8B27" ); // Rock.Workflow.Action.RunLava:Attribute
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute( "BC21E57A-1477-44B3-A7C2-61A806118945", "4BD9088F-5CC6-89B1-45FC-A2AAFFC7CC0D", "Enabled Lava Commands", "EnabledLavaCommands", "The Lava commands that should be enabled for this action.", 2, @"", "F3E380BF-AAC8-4015-9ADC-0DF56B5462F5" ); // Rock.Workflow.Action.RunLava:Enabled Lava Commands
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute( "BC21E57A-1477-44B3-A7C2-61A806118945", "A75DFC58-7A1B-4799-BF31-451B2BBE38FF", "Order", "Order", "The order that this service should be used (priority)", 0, @"", "1B833F48-EFC2-4537-B1E3-7793F6863EAA" ); // Rock.Workflow.Action.RunLava:Order

            RockMigrationHelper.UpdateWorkflowActionForm( @"
            {{ Workflow | Attribute: 'WarningMessage' }}
            {{ Workflow | Attribute: 'NoEmailWarning' }}
            {% assign p = Workflow | Attribute:'Person','Object' %}
            <p><i>The message below will be included in the emailed request to {{p.NickName}}.     
            Feel free to customize it to better suit your situation.</i> </p>   
            <hr/>  
            Hi {{ p.NickName }}!", @"", "Send^fdc397cd-8b4a-436e-bea1-bce2e6717c03^^Your information has been submitted successfully.|", "", true, "", "4AFAB342-D584-4B79-B038-A99C0C469D74" ); // DISC Request:Launch From Person Profile:Custom Message
            RockMigrationHelper.UpdateWorkflowActionForm( @"
            {{ Workflow | Attribute: 'WarningMessage' }} 
            {{ Workflow | Attribute: 'NoEmailWarning' }}", @"", "", "88C7D1CC-3478-4562-A301-AE7D4D7FFF6D", true, "", "78EB1EC5-D896-4194-A86B-939F95A34555" ); // DISC Request:Launch From Person Profile:Custom Failure Message

            RockMigrationHelper.UpdateWorkflowActionFormAttribute( "4AFAB342-D584-4B79-B038-A99C0C469D74", "9B4576B0-2227-4564-8EB3-0BC33B31A723", 5, false, true, false, false, @"", @"", "5D440583-5F29-4A11-B5CB-8154793E3DB0" ); // DISC Request:Launch From Person Profile:Custom Message:No Email Opted Out
            RockMigrationHelper.UpdateWorkflowActionFormAttribute( "4AFAB342-D584-4B79-B038-A99C0C469D74", "20354F00-F6A8-4EA8-9F05-1DEC378CEEC2", 6, false, true, false, false, @"", @"", "1102FF4C-BA75-4CAA-9E34-97570FF7BA0E" ); // DISC Request:Launch From Person Profile:Custom Message:IsEmailPresent
            RockMigrationHelper.UpdateWorkflowActionFormAttribute( "4AFAB342-D584-4B79-B038-A99C0C469D74", "1917380A-CE12-4253-980A-60EDDD3348B5", 8, false, true, false, false, @"", @"", "E32ED422-8F45-4F52-B4CD-9CA331693DB2" ); // DISC Request:Launch From Person Profile:Custom Message:User Entry Valid
            RockMigrationHelper.UpdateWorkflowActionFormAttribute( "78EB1EC5-D896-4194-A86B-939F95A34555", "C0BC984C-84C3-494B-A861-49840E452F68", 0, false, true, false, false, @"", @"", "D92EE1FA-D537-41A1-BD80-034587808341" ); // DISC Request:Launch From Person Profile:Custom Failure Message:Person
            RockMigrationHelper.UpdateWorkflowActionFormAttribute( "78EB1EC5-D896-4194-A86B-939F95A34555", "840E6A84-9F83-4482-92D1-6F635F062251", 1, false, true, false, false, @"", @"", "31AA045C-CB26-4F1D-AE58-311F96130B3F" ); // DISC Request:Launch From Person Profile:Custom Failure Message:Custom Message
            RockMigrationHelper.UpdateWorkflowActionFormAttribute( "78EB1EC5-D896-4194-A86B-939F95A34555", "D44036C8-2787-4942-A70E-5E85087787EB", 2, false, true, false, false, @"", @"", "B4DAD64D-59DD-4736-91AC-9EC75B281D80" ); // DISC Request:Launch From Person Profile:Custom Failure Message:Sender
            RockMigrationHelper.UpdateWorkflowActionFormAttribute( "78EB1EC5-D896-4194-A86B-939F95A34555", "82B3A401-463C-4AD5-8AD1-23D067E48458", 3, false, true, false, false, @"", @"", "FC755AE5-318C-4530-90AF-368DC1611737" ); // DISC Request:Launch From Person Profile:Custom Failure Message:OptOut
            RockMigrationHelper.UpdateWorkflowActionFormAttribute( "78EB1EC5-D896-4194-A86B-939F95A34555", "74E7BD11-BE21-4256-B02A-E97AD54C7511", 4, false, true, false, false, @"", @"", "D32A7F6D-9179-4A0D-9E57-D12982460D43" ); // DISC Request:Launch From Person Profile:Custom Failure Message:Warning Message
            RockMigrationHelper.UpdateWorkflowActionFormAttribute( "78EB1EC5-D896-4194-A86B-939F95A34555", "BB2B3CF8-7C81-4EBD-9589-A961E341F70C", 5, false, true, false, false, @"", @"", "ABF49299-A554-4F3E-AAE7-F515DCFE4743" ); // DISC Request:Launch From Person Profile:Custom Failure Message:No Email Warning
            RockMigrationHelper.UpdateWorkflowActionFormAttribute( "78EB1EC5-D896-4194-A86B-939F95A34555", "9B4576B0-2227-4564-8EB3-0BC33B31A723", 6, false, true, false, false, @"", @"", "CD8C94C6-75B5-4E4D-968B-71AE7628BBC5" ); // DISC Request:Launch From Person Profile:Custom Failure Message:No Email Opted Out
            RockMigrationHelper.UpdateWorkflowActionFormAttribute( "78EB1EC5-D896-4194-A86B-939F95A34555", "20354F00-F6A8-4EA8-9F05-1DEC378CEEC2", 7, false, true, false, false, @"", @"", "39C1910F-A7E2-4DB0-9C5A-45EC2E427142" ); // DISC Request:Launch From Person Profile:Custom Failure Message:IsEmailPresent
            RockMigrationHelper.UpdateWorkflowActionFormAttribute( "78EB1EC5-D896-4194-A86B-939F95A34555", "1917380A-CE12-4253-980A-60EDDD3348B5", 8, false, true, false, false, @"", @"", "6E3D2903-8CE2-4D3C-A31F-F88391E2BE84" ); // DISC Request:Launch From Person Profile:Custom Failure Message:User Entry Valid
            RockMigrationHelper.AddActionTypeAttributeValue( "666FC137-BC95-49BE-A976-0BFF2417F44C", "4D245B9E-6B03-46E7-8482-A51FBA190E4D", @"{{ 'Global' | Attribute:'EmailHeader' }} <p>Hi {{ Person.NickName }}!</p>  <p>{{ Workflow | Attribute:'CustomMessage' | NewlineToBr }}</p>  <blockquote>     <p><a href=""{{ 'Global' | Attribute:'PublicApplicationRoot' }}DISC/{{ p.UrlEncodedKey }}"">Take Personality Assessment</a></p>     <p><i>- or -</i></p>     <p><a href=""{{ 'Global' | Attribute:'PublicApplicationRoot' }}Unsubscribe/{{ p.UrlEncodedKey }}"">I&#39;m no longer involved with      {{ 'Global' | Attribute :'OrganizationName' }}. Please remove me from all future communications.</a></p> </blockquote>   <p>- {{ Workflow | Attribute:'Sender' }}</p>  {{ 'Global' | Attribute:'EmailFooter' }}" ); // DISC Request:Launch From Person Profile:Send Email Action:Body
            Sql( @"
            UPDATE
                [Attribute]
            SET
                [Description] = ''
            WHERE
                [Guid] = '840E6A84-9F83-4482-92D1-6F635F062251'
            " );

            RockMigrationHelper.UpdateWorkflowActionType( "4A8E2E61-FE0D-4AB3-BA48-9419A20E4541", "Custom Message", 9, "486DC4FA-FCBC-425F-90B0-E606DA8A9F68", true, false, "4AFAB342-D584-4B79-B038-A99C0C469D74", "1917380A-CE12-4253-980A-60EDDD3348B5", 1, "True", "1A08A4EC-B2C6-43B5-926F-2F86CFA35102" ); // DISC Request:Launch From Person Profile:Custom Message
            RockMigrationHelper.UpdateWorkflowActionType( "4A8E2E61-FE0D-4AB3-BA48-9419A20E4541", "Custom Failure Message", 10, "486DC4FA-FCBC-425F-90B0-E606DA8A9F68", true, true, "78EB1EC5-D896-4194-A86B-939F95A34555", "1917380A-CE12-4253-980A-60EDDD3348B5", 1, "False", "2FEE6858-8A63-484F-9848-EAC4697DA1D5" ); // DISC Request:Launch From Person Profile:Custom Failure Message

            RockMigrationHelper.UpdateWorkflowActionFormAttribute( "4AFAB342-D584-4B79-B038-A99C0C469D74", "840E6A84-9F83-4482-92D1-6F635F062251", 1, true, false, false, true, @"", @"", "94B20E87-9C79-4C37-926C-0426496AC722" ); // DISC Request:Launch From Person Profile:Custom Message:Custom Message
            RockMigrationHelper.AddActionTypeAttributeValue( "2FEE6858-8A63-484F-9848-EAC4697DA1D5", "234910F2-A0DB-4D7D-BAF7-83C880EF30AE", @"False" ); // DISC Request:Launch From Person Profile:Custom Failure Message:Active
            RockMigrationHelper.AddActionTypeAttributeValue( "2FEE6858-8A63-484F-9848-EAC4697DA1D5", "C178113D-7C86-4229-8424-C6D0CF4A7E23", @"" ); // DISC Request:Launch From Person Profile:Custom Failure Message:Order
            RockMigrationHelper.AddActionTypeAttributeValue( "A5967573-B263-4003-AED3-36862D4A0251", "F3B9908B-096F-460B-8320-122CF046D1F9", @"
            DECLARE @PersonAliasGuid uniqueidentifier = '{{ Workflow | Attribute:'Person','RawValue' }}'  
            SELECT  
                CASE     WHEN EXISTS (
                    SELECT 1
                    FROM
                        [Person] P
                    INNER JOIN 
                        [PersonAlias] PA ON PA.[Guid] = @PersonAliasGuid AND P.[Id] = PA.[PersonId]         
                    WHERE P.[EmailPreference] = 1 )     
                THEN 
                    'True'
                ELSE 'False'
                END" ); // DISC Request:Launch From Person Profile:Check for Opt Out:SQLQuery

            RockMigrationHelper.AddActionTypeAttributeValue( "080944DB-B45F-41CA-BF5E-1F697B503C16", "E5272B11-A2B8-49DC-860D-8D574E2BC15C", @"<div class=""alert alert-warning"">{{ Workflow | Attribute:'Person' }} has previously opted out from bulk requests.  Make sure you want to override this preference.</div>" ); // DISC Request:Launch From Person Profile:Set Warning:Text Value|Attribute Value
            RockMigrationHelper.UpdateWorkflowTypeAttribute( "885CBA61-44EA-4B4A-B6E1-289041B6A195", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "No Email Opted Out", "NoEmailOptedOut", "Holds a 2 if the person has opted out email requests.", 6, @"", "9B4576B0-2227-4564-8EB3-0BC33B31A723", false ); // DISC Request:No Email Opted Out
            RockMigrationHelper.UpdateWorkflowTypeAttribute( "885CBA61-44EA-4B4A-B6E1-289041B6A195", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "IsEmailPresent", "IsEmailPresent", "Hold if email is found for the person.", 7, @"", "20354F00-F6A8-4EA8-9F05-1DEC378CEEC2", false ); // DISC Request:IsEmailPresent
            RockMigrationHelper.UpdateWorkflowTypeAttribute( "885CBA61-44EA-4B4A-B6E1-289041B6A195", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "User Entry Valid", "UserEntryValid", "Determines if everything is valid to proceed to user entry form.", 8, @"", "1917380A-CE12-4253-980A-60EDDD3348B5", false ); // DISC Request:User Entry Valid
            RockMigrationHelper.AddAttributeQualifier( "9B4576B0-2227-4564-8EB3-0BC33B31A723", "falsetext", @"No", "4F14E70F-9019-4884-9CC3-C4C5B6143AA0" ); // DISC Request:No Email Opted Out:falsetext
            RockMigrationHelper.AddAttributeQualifier( "9B4576B0-2227-4564-8EB3-0BC33B31A723", "truetext", @"Yes", "2ABC15A1-F190-401B-B86D-B29295A65B09" ); // DISC Request:No Email Opted Out:truetext
            RockMigrationHelper.AddAttributeQualifier( "20354F00-F6A8-4EA8-9F05-1DEC378CEEC2", "falsetext", @"No", "DAB866DA-D40D-423A-BCB7-E3A6F350850E" ); // DISC Request:IsEmailPresent:falsetext
            RockMigrationHelper.AddAttributeQualifier( "20354F00-F6A8-4EA8-9F05-1DEC378CEEC2", "truetext", @"Yes", "FAED4973-9065-4D1D-A5B0-BFD142EAC20C" ); // DISC Request:IsEmailPresent:truetext
            RockMigrationHelper.AddAttributeQualifier( "1917380A-CE12-4253-980A-60EDDD3348B5", "falsetext", @"No", "4019CB32-D78F-494C-B6E1-A07118D39031" ); // DISC Request:User Entry Valid:falsetext
            RockMigrationHelper.AddAttributeQualifier( "1917380A-CE12-4253-980A-60EDDD3348B5", "truetext", @"Yes", "32D1791C-7BDE-488F-8BDE-C4CC81C7D113" ); // DISC Request:User Entry Valid:truetext

            RockMigrationHelper.UpdateWorkflowActionFormAttribute( "4AFAB342-D584-4B79-B038-A99C0C469D74", "9B4576B0-2227-4564-8EB3-0BC33B31A723", 6, false, true, false, false, @"", @"", "5D440583-5F29-4A11-B5CB-8154793E3DB0" ); // DISC Request:Launch From Person Profile:Custom Message:No Email Opted Out
            RockMigrationHelper.UpdateWorkflowActionFormAttribute( "4AFAB342-D584-4B79-B038-A99C0C469D74", "20354F00-F6A8-4EA8-9F05-1DEC378CEEC2", 7, false, true, false, false, @"", @"", "1102FF4C-BA75-4CAA-9E34-97570FF7BA0E" ); // DISC Request:Launch From Person Profile:Custom Message:IsEmailPresent
            RockMigrationHelper.UpdateWorkflowActionFormAttribute( "4AFAB342-D584-4B79-B038-A99C0C469D74", "1917380A-CE12-4253-980A-60EDDD3348B5", 8, false, true, false, false, @"", @"", "E32ED422-8F45-4F52-B4CD-9CA331693DB2" ); // DISC Request:Launch From Person Profile:Custom Message:User Entry Valid

            RockMigrationHelper.UpdateWorkflowActionForm( @"{{ Workflow | Attribute: 'WarningMessage' }} {{ Workflow | Attribute: 'NoEmailWarning' }}", @"", "", "88C7D1CC-3478-4562-A301-AE7D4D7FFF6D", true, "", "78EB1EC5-D896-4194-A86B-939F95A34555" ); // DISC Request:Launch From Person Profile:Custom Failure Message
            RockMigrationHelper.UpdateWorkflowActionFormAttribute( "78EB1EC5-D896-4194-A86B-939F95A34555", "C0BC984C-84C3-494B-A861-49840E452F68", 0, false, true, false, false, @"", @"", "D92EE1FA-D537-41A1-BD80-034587808341" ); // DISC Request:Launch From Person Profile:Custom Failure Message:Person
            RockMigrationHelper.UpdateWorkflowActionFormAttribute( "78EB1EC5-D896-4194-A86B-939F95A34555", "840E6A84-9F83-4482-92D1-6F635F062251", 1, false, true, false, false, @"", @"", "31AA045C-CB26-4F1D-AE58-311F96130B3F" ); // DISC Request:Launch From Person Profile:Custom Failure Message:Custom Message
            RockMigrationHelper.UpdateWorkflowActionFormAttribute( "78EB1EC5-D896-4194-A86B-939F95A34555", "D44036C8-2787-4942-A70E-5E85087787EB", 2, false, true, false, false, @"", @"", "B4DAD64D-59DD-4736-91AC-9EC75B281D80" ); // DISC Request:Launch From Person Profile:Custom Failure Message:Sender
            RockMigrationHelper.UpdateWorkflowActionFormAttribute( "78EB1EC5-D896-4194-A86B-939F95A34555", "82B3A401-463C-4AD5-8AD1-23D067E48458", 3, false, true, false, false, @"", @"", "FC755AE5-318C-4530-90AF-368DC1611737" ); // DISC Request:Launch From Person Profile:Custom Failure Message:OptOut
            RockMigrationHelper.UpdateWorkflowActionFormAttribute( "78EB1EC5-D896-4194-A86B-939F95A34555", "74E7BD11-BE21-4256-B02A-E97AD54C7511", 4, false, true, false, false, @"", @"", "D32A7F6D-9179-4A0D-9E57-D12982460D43" ); // DISC Request:Launch From Person Profile:Custom Failure Message:Warning Message
            RockMigrationHelper.UpdateWorkflowActionFormAttribute( "78EB1EC5-D896-4194-A86B-939F95A34555", "BB2B3CF8-7C81-4EBD-9589-A961E341F70C", 5, false, true, false, false, @"", @"", "ABF49299-A554-4F3E-AAE7-F515DCFE4743" ); // DISC Request:Launch From Person Profile:Custom Failure Message:No Email Warning
            RockMigrationHelper.UpdateWorkflowActionFormAttribute( "78EB1EC5-D896-4194-A86B-939F95A34555", "9B4576B0-2227-4564-8EB3-0BC33B31A723", 6, false, true, false, false, @"", @"", "CD8C94C6-75B5-4E4D-968B-71AE7628BBC5" ); // DISC Request:Launch From Person Profile:Custom Failure Message:No Email Opted Out
            RockMigrationHelper.UpdateWorkflowActionFormAttribute( "78EB1EC5-D896-4194-A86B-939F95A34555", "20354F00-F6A8-4EA8-9F05-1DEC378CEEC2", 7, false, true, false, false, @"", @"", "39C1910F-A7E2-4DB0-9C5A-45EC2E427142" ); // DISC Request:Launch From Person Profile:Custom Failure Message:IsEmailPresent
            RockMigrationHelper.UpdateWorkflowActionFormAttribute( "78EB1EC5-D896-4194-A86B-939F95A34555", "1917380A-CE12-4253-980A-60EDDD3348B5", 8, false, true, false, false, @"", @"", "6E3D2903-8CE2-4D3C-A31F-F88391E2BE84" ); // DISC Request:Launch From Person Profile:Custom Failure Message:User Entry Valid

            RockMigrationHelper.AddActionTypeAttributeValue( "940E88E7-1294-4DD7-A626-E1345A41A2D1", "56997192-2545-4EA1-B5B2-313B04588984", @"20354f00-f6a8-4ea8-9f05-1dec378ceec2" ); // DISC Request:Launch From Person Profile:Set No Email Warning:Result Attribute
            RockMigrationHelper.UpdateWorkflowActionType( "4A8E2E61-FE0D-4AB3-BA48-9419A20E4541", "Set No Email Warning", 4, "A41216D6-6FB0-4019-B222-2C29B4519CF4", true, false, "", "", 1, "", "940E88E7-1294-4DD7-A626-E1345A41A2D1" ); // DISC Request:Launch From Person Profile:Set No Email Warning
            RockMigrationHelper.UpdateWorkflowActionType( "4A8E2E61-FE0D-4AB3-BA48-9419A20E4541", "Set No Email Warning Message", 5, "C789E457-0783-44B3-9D8F-2EBAB5F11110", true, false, "", "20354F00-F6A8-4EA8-9F05-1DEC378CEEC2", 1, "False", "3FAC4206-D572-4BB7-84A8-9FF817C96302" ); // DISC Request:Launch From Person Profile:Set No Email Warning Message
            RockMigrationHelper.UpdateWorkflowActionType( "4A8E2E61-FE0D-4AB3-BA48-9419A20E4541", "Set No Email Opted Out", 6, "A41216D6-6FB0-4019-B222-2C29B4519CF4", true, false, "", "", 1, "", "AF0285A4-CA58-4131-BE35-EEB5CF033DE0" ); // DISC Request:Launch From Person Profile:Set No Email Opted Out
            RockMigrationHelper.UpdateWorkflowActionType( "4A8E2E61-FE0D-4AB3-BA48-9419A20E4541", "Set Error", 7, "C789E457-0783-44B3-9D8F-2EBAB5F11110", true, false, "", "9B4576B0-2227-4564-8EB3-0BC33B31A723", 1, "True", "7790C564-B42C-4FFC-B6C0-71E9853E380A" ); // DISC Request:Launch From Person Profile:Set Error
            RockMigrationHelper.UpdateWorkflowActionType( "4A8E2E61-FE0D-4AB3-BA48-9419A20E4541", "Set User Entry Valid", 8, "BC21E57A-1477-44B3-A7C2-61A806118945", true, false, "", "", 1, "", "36DE9E26-DC5C-43E7-866A-B61CF37025FC" ); // DISC Request:Launch From Person Profile:Set User Entry Valid
            Sql( @"
                UPDATE
                    [WorkflowActionType]
                SET
                    [Order] =[Order] + 3
                WHERE
                    [Guid] IN(
    '1A08A4EC-B2C6-43B5-926F-2F86CFA35102',
    '555A729C-5EC4-4C83-B35F-036234E5EFCC',
    '666FC137-BC95-49BE-A976-0BFF2417F44C',
    '42401767-F313-4AAE-BE46-AD557F970847',
    'EA4FF8DD-0E66-4C98-84FC-845DEAB76A61' )
            " );
            RockMigrationHelper.UpdateWorkflowActionType( "4A8E2E61-FE0D-4AB3-BA48-9419A20E4541", "Custom Message", 9, "486DC4FA-FCBC-425F-90B0-E606DA8A9F68", true, false, "4AFAB342-D584-4B79-B038-A99C0C469D74", "1917380A-CE12-4253-980A-60EDDD3348B5", 1, "True", "1A08A4EC-B2C6-43B5-926F-2F86CFA35102" ); // DISC Request:Launch From Person Profile:Custom Message
            RockMigrationHelper.UpdateWorkflowActionType( "4A8E2E61-FE0D-4AB3-BA48-9419A20E4541", "Custom Failure Message", 10, "486DC4FA-FCBC-425F-90B0-E606DA8A9F68", true, true, "78EB1EC5-D896-4194-A86B-939F95A34555", "1917380A-CE12-4253-980A-60EDDD3348B5", 1, "False", "2FEE6858-8A63-484F-9848-EAC4697DA1D5" ); // DISC Request:Launch From Person Profile:Custom Failure Message
            RockMigrationHelper.AddActionTypeAttributeValue( "AF0285A4-CA58-4131-BE35-EEB5CF033DE0", "FA7C685D-8636-41EF-9998-90FFF3998F76", @"" ); // DISC Request:Launch From Person Profile:Set No Email Opted Out:Order
            RockMigrationHelper.AddActionTypeAttributeValue( "AF0285A4-CA58-4131-BE35-EEB5CF033DE0", "F3B9908B-096F-460B-8320-122CF046D1F9", @"
            DECLARE @PersonAliasGuid uniqueidentifier = '{{ Workflow | Attribute:'Person','RawValue' }}'
            SELECT  
                CASE     WHEN EXISTS 
                    ( SELECT 1         
                     FROM
                        [Person] P  
                     INNER JOIN 
                        [PersonAlias] PA ON PA.[Guid] = @PersonAliasGuid AND P.[Id] = PA.[PersonId]         
                    WHERE P.[EmailPreference] = 2 )
                THEN 'True'
                ELSE 'False'
            END" ); // DISC Request:Launch From Person Profile:Set No Email Opted Out:SQLQuery
            RockMigrationHelper.AddActionTypeAttributeValue( "AF0285A4-CA58-4131-BE35-EEB5CF033DE0", "A18C3143-0586-4565-9F36-E603BC674B4E", @"False" ); // DISC Request:Launch From Person Profile:Set No Email Opted Out:Active
            RockMigrationHelper.AddActionTypeAttributeValue( "AF0285A4-CA58-4131-BE35-EEB5CF033DE0", "59F8F913-3969-46F3-B6E9-3613DCC94388", @"" ); // DISC Request:Launch From Person Profile:Set No Email Opted Out:Parameters
            RockMigrationHelper.AddActionTypeAttributeValue( "AF0285A4-CA58-4131-BE35-EEB5CF033DE0", "56997192-2545-4EA1-B5B2-313B04588984", @"9b4576b0-2227-4564-8eb3-0bc33b31a723" ); // DISC Request:Launch From Person Profile:Set No Email Opted Out:Result Attribute
            RockMigrationHelper.AddActionTypeAttributeValue( "AF0285A4-CA58-4131-BE35-EEB5CF033DE0", "1C419D69-E39E-4CCF-B9DB-60C803055146", @"False" ); // DISC Request:Launch From Person Profile:Set No Email Opted Out:Continue On Error
            RockMigrationHelper.AddActionTypeAttributeValue( "7790C564-B42C-4FFC-B6C0-71E9853E380A", "D7EAA859-F500-4521-9523-488B12EAA7D2", @"False" ); // DISC Request:Launch From Person Profile:Set Error:Active
            RockMigrationHelper.AddActionTypeAttributeValue( "7790C564-B42C-4FFC-B6C0-71E9853E380A", "44A0B977-4730-4519-8FF6-B0A01A95B212", @"74e7bd11-be21-4256-b02a-e97ad54c7511" ); // DISC Request:Launch From Person Profile:Set Error:Attribute
            RockMigrationHelper.AddActionTypeAttributeValue( "7790C564-B42C-4FFC-B6C0-71E9853E380A", "57093B41-50ED-48E5-B72B-8829E62704C8", @"" ); // DISC Request:Launch From Person Profile:Set Error:Order
            RockMigrationHelper.AddActionTypeAttributeValue( "7790C564-B42C-4FFC-B6C0-71E9853E380A", "E5272B11-A2B8-49DC-860D-8D574E2BC15C", @"<div class=""alert alert-danger"">{{ Workflow | Attribute:'Person' }} has previously opted out from emails.  Make sure you want to override this preference.</div>" ); // DISC Request:Launch From Person Profile:Set Error:Text Value|Attribute Value
            RockMigrationHelper.AddActionTypeAttributeValue( "555A729C-5EC4-4C83-B35F-036234E5EFCC", "E22BE348-18B1-4420-83A8-6319B35416D2", @"True" ); // DISC Request:Launch From Person Profile:Persist Workflow:Persist Immediately

            RockMigrationHelper.AddActionTypeAttributeValue( "36DE9E26-DC5C-43E7-866A-B61CF37025FC", "F1F6F9D6-FDC5-489C-8261-4B9F45B3EED4", @"
           {% assign optOut = Workflow | Attribute:'OptOut'  | AsBoolean %}
           {% assign isEmailpresent = Workflow | Attribute:'IsEmailPresent'  | AsBoolean %} 
           {% assign noEmailOptedOut = Workflow | Attribute:'NoEmailOptedOut' | AsBoolean %} 
           {% if optOut == false and isEmailpresent and noEmailOptedOut == false  %} 
           {{ true }}
           {% else %} 
           {{ false }} 
           {% endif %}" ); // DISC Request:Launch From Person Profile:Set User Entry Valid:Lava
            RockMigrationHelper.AddActionTypeAttributeValue( "36DE9E26-DC5C-43E7-866A-B61CF37025FC", "F1924BDC-9B79-4018-9D4A-C3516C87A514", @"False" ); // DISC Request:Launch From Person Profile:Set User Entry Valid:Active
            RockMigrationHelper.AddActionTypeAttributeValue( "36DE9E26-DC5C-43E7-866A-B61CF37025FC", "1B833F48-EFC2-4537-B1E3-7793F6863EAA", @"" ); // DISC Request:Launch From Person Profile:Set User Entry Valid:Order
            RockMigrationHelper.AddActionTypeAttributeValue( "36DE9E26-DC5C-43E7-866A-B61CF37025FC", "431273C6-342D-4030-ADC7-7CDEDC7F8B27", @"1917380a-ce12-4253-980a-60eddd3348b5" ); // DISC Request:Launch From Person Profile:Set User Entry Valid:Attribute
            RockMigrationHelper.AddActionTypeAttributeValue( "36DE9E26-DC5C-43E7-866A-B61CF37025FC", "F3E380BF-AAC8-4015-9ADC-0DF56B5462F5", @"" ); // DISC Request:Launch From Person Profile:Set User Entry Valid:Enabled Lava Commands
            RockMigrationHelper.AddActionTypeAttributeValue( "2FEE6858-8A63-484F-9848-EAC4697DA1D5", "234910F2-A0DB-4D7D-BAF7-83C880EF30AE", @"False" ); // DISC Request:Launch From Person Profile:Custom Failure Message:Active
            RockMigrationHelper.AddActionTypeAttributeValue( "2FEE6858-8A63-484F-9848-EAC4697DA1D5", "C178113D-7C86-4229-8424-C6D0CF4A7E23", @"" ); // DISC Request:Launch From Person Profile:Custom Failure Message:Order

        }

        /// <summary>
        /// SK: "Rock Update Helper v9.0 - DISC Score Conversion
        /// </summary>
        private void UpdateDiscScoreConversion()
        {
            Sql( @"IF NOT EXISTS (
  SELECT[Id]
  FROM[ServiceJob]
  WHERE[Class] = 'Rock.Jobs.PostV90DataMigrationsForDISC'
   AND[Guid] = 'A839DFEC-B1A3-499C-9BB3-03241E8E5305'
  )
BEGIN
 INSERT INTO[ServiceJob](
  [IsSystem]
  ,[IsActive]
  ,[Name]
  ,[Description]
  ,[Class]
  ,[CronExpression]
  ,[NotificationStatus]
  ,[Guid]
  )
 VALUES(
  0
  ,1
  ,'Rock Update Helper v9.0 - DISC'
  ,'This job will take care of any data migrations that need to occur after updating to v9.0. After all the operations are done, this job will delete itself.'
  ,'Rock.Jobs.PostV90DataMigrationsForDISC'
  ,'0 0 21 1/1 * ? *'
  ,1
  ,'A839DFEC-B1A3-499C-9BB3-03241E8E5305'
  );
        END" );
        }


        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
        }
    }
}
