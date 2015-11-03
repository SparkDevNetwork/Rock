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
    public partial class CreateWorkflows : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            #region Workflow Support Data

            AddColumn( "dbo.WorkflowActionForm", "ActionAttributeGuid", c => c.Guid() );
            DropColumn( "dbo.WorkflowActionForm", "InactiveMessage" );

            RockMigrationHelper.AddPage( "6510AB6B-DFB4-4DBF-9F0F-7EA598E4AC54", "D65F783D-87A9-4CC9-8110-E83466A0EADB", "LaunchWorkflow", "", "5DA89BC9-A185-4749-A843-314B72170D82", "" ); // Site:Rock RMS
            RockMigrationHelper.AddPageRoute( "5DA89BC9-A185-4749-A843-314B72170D82", "LaunchWorkflow/{WorkflowTypeId}" );

            RockMigrationHelper.AddPage( "7625A63E-6650-4886-B605-53C2234FA5E1", "325B7BFD-8B80-44FD-A951-4E4763DA6C0D", "Contact Us", "", "B1E63FE3-779C-4388-AFE4-FD6DFC034932", "" ); // Site:Rock Solid Church
            RockMigrationHelper.AddPageRoute( "B1E63FE3-779C-4388-AFE4-FD6DFC034932", "ContactUs" );

            // Add Block to Page: LaunchWorkflow, Site: Rock RMS
            RockMigrationHelper.AddBlock( "5DA89BC9-A185-4749-A843-314B72170D82", "", "83CB0C72-4F0A-44A7-98D0-260CE33788E9", "Activate Workflow", "Main", "", "", 0, "C0717EB6-80B2-48AF-AD86-EF46678F6780" );
            // Add Block to Page: Contact Us, Site: Rock Solid Church
            RockMigrationHelper.AddBlock( "B1E63FE3-779C-4388-AFE4-FD6DFC034932", "", "A8BD05C8-6F89-4628-845B-059E686F089A", "Workflow Entry", "Main", "", "", 0, "CA7D13BB-6781-4908-9198-CF89E915F9D7" );
            // Add Block to Page: Contact Us, Site: Rock Solid Church
            RockMigrationHelper.AddBlock( "B1E63FE3-779C-4388-AFE4-FD6DFC034932", "", "CACB9D1A-A820-4587-986A-D66A69EE9948", "SubNav", "Sidebar1", "", "", 0, "240E241A-874B-453C-A98E-AEBADA722EC4" );

            // Attrib for BlockType: Person Bio:Actions
            RockMigrationHelper.AddBlockTypeAttribute( "0F5922BB-CD68-40AC-BF3C-4AAB1B98760C", "1D0D3794-C210-48A8-8C68-3FBEC08A6BA5", "Actions", "Actions", "", @"
Custom html content to display as a list of actions. Any instance of '{0}' will be replaced with the current person's id.
Because the contents of this setting will be rendered inside a &lt;ul&gt; element, it is recommended to use an 
&lt;li&gt; element for each available action.  Example:
<pre>
    &lt;li&gt;
        &lt;a href='~/LaunchWorkflow/1?PersonId={0}' tabindex='0'&gt;First Action&lt;/a&gt;
        &lt;a href='~/LaunchWorkflow/2?PersonId={0}' tabindex='0'&gt;Second Action&lt;/a&gt;
        &lt;a href='~/LaunchWorkflow/3?PersonId={0}' tabindex='0'&gt;Third Action&lt;/a&gt;
    &lt;/li&gt;
    &lt;li class='divider'&gt;&lt;/li&gt;
    &lt;li&gt;&lt;a href='~/LaunchWorkflow/4?PersonId={0}' tabindex='0'&gt;Fourth Action&lt;/a&gt;&lt;/li&gt;
</pre>
", 1, @"", "35F69669-48DE-4182-B828-4EC9C1C31B08" );
            // Attrib for BlockType: Activate Workflow:Workflow Entry Page
            RockMigrationHelper.AddBlockTypeAttribute( "83CB0C72-4F0A-44A7-98D0-260CE33788E9", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Workflow Entry Page", "WorkflowEntryPage", "", "", 0, @"", "031DD6B2-7131-4BC7-9609-4FEED6D8AEA1" );

            // Attrib Value for Block:Bio, Attribute:Actions Layout: PersonDetail, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "B5C1FDB6-0224-43E4-8E26-6B2EAF86253A", "35F69669-48DE-4182-B828-4EC9C1C31B08", @"" );
            // Attrib Value for Block:Activate Workflow, Attribute:Workflow Entry Page Page: LaunchWorkflow, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "C0717EB6-80B2-48AF-AD86-EF46678F6780", "031DD6B2-7131-4BC7-9609-4FEED6D8AEA1", @"0550d2aa-a705-4400-81ff-ab124fdf83d7,f62c0ff5-7132-43da-a23b-a4fcd8077000" );
            // Attrib Value for Block:Workflow Entry, Attribute:Workflow Type Page: Contact Us, Site: Rock Solid Church
            RockMigrationHelper.AddBlockAttributeValue( "CA7D13BB-6781-4908-9198-CF89E915F9D7", "2F1D98C4-A8EF-4680-9F64-11BFC28D5597", @"236ab611-ede8-42b5-b559-6b6a88adddcb" );

            RockMigrationHelper.AddBlockAttributeValue( "240E241A-874B-453C-A98E-AEBADA722EC4", "7A2010F0-0C0C-4CC5-A29B-9CBAE4DE3A22", "" );
            RockMigrationHelper.AddBlockAttributeValue( "240E241A-874B-453C-A98E-AEBADA722EC4", "EEE71DDE-C6BC-489B-BAA5-1753E322F183", "False" );
            RockMigrationHelper.AddBlockAttributeValue( "240E241A-874B-453C-A98E-AEBADA722EC4", "1322186A-862A-4CF1-B349-28ECB67229BA", "{% include 'PageSubNav' %}" );
            RockMigrationHelper.AddBlockAttributeValue( "240E241A-874B-453C-A98E-AEBADA722EC4", "41F1C42E-2395-4063-BD4F-031DF8D5B231", "7625a63e-6650-4886-b605-53c2234fa5e1" );
            RockMigrationHelper.AddBlockAttributeValue( "240E241A-874B-453C-A98E-AEBADA722EC4", "6C952052-BC79-41BA-8B88-AB8EA3E99648", "3" );
            RockMigrationHelper.AddBlockAttributeValue( "240E241A-874B-453C-A98E-AEBADA722EC4", "E4CF237D-1D12-4C93-AFD7-78EB296C4B69", "False" );
            RockMigrationHelper.AddBlockAttributeValue( "240E241A-874B-453C-A98E-AEBADA722EC4", "2EF904CD-976E-4489-8C18-9BA43885ACD9", "False" );
            RockMigrationHelper.AddBlockAttributeValue( "240E241A-874B-453C-A98E-AEBADA722EC4", "C80209A8-D9E0-4877-A8E3-1F7DBF64D4C2", "False" );

            // Attrib Value for Block:My Workflows Liquid, Attribute:Contents Page: Internal Homepage, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "2C90BDF8-48FF-4A7C-AA70-97B7E3780177", "D2149BA3-7AE8-4FE8-AF7C-4EF40DBEB4B2", @"
{% if Actions.size > 0 %}
    <div class='panel panel-info'> 
        <div class='panel-heading'>
            <h4 class='panel-title'>My Tasks</h4>
        </div>
        <div class='panel-body'>
            <ul class='fa-ul'>
                {% for action in Actions %}
                    <li>
                        <i class='fa-li {{ action.Activity.Workflow.WorkflowType.IconCssClass }}'></i>
                        <a href='~/WorkflowEntry/{{ action.Activity.Workflow.WorkflowTypeId }}/{{ action.Activity.Workflow.Id }}'>{{ action.Activity.Workflow.WorkflowType.Name }}: {{ action.Activity.Workflow.Name }} ({{ action.Activity.ActivityType.Name }})</a>
                    </li>
                {% endfor %}
            </ul>
        </div>
    </div>
{% endif %}
" );

            RockMigrationHelper.UpdateEntityType( "Rock.Model.WorkflowType", "C9F3C4A5-1526-474D-803F-D6C7A45CBBAE", true, true );
            RockMigrationHelper.UpdateEntityType( "Rock.Storage.Provider.Database", "0AA42802-04FD-4AEC-B011-FEB127FC85CD", false, true );
            RockMigrationHelper.UpdateCategory( "C9F3C4A5-1526-474D-803F-D6C7A45CBBAE", "Person", "fa fa-user", "", "BBAE05FD-8192-4616-A71E-903A927E0D90" );
            RockMigrationHelper.UpdateCategory( "C9F3C4A5-1526-474D-803F-D6C7A45CBBAE", "Requests", "fa fa-question-circle", "", "78E38655-D951-41DB-A0FF-D6474775CFA1" );

            // Attrib Value for Block:Workflow Navigation, Attribute:Categories Page: Workflows, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "2D20CEC4-328E-4C2B-8059-78DFC49D8E35", "FB420F14-3D9D-4304-878F-124902E2CEAB", @"bbae05fd-8192-4616-a71e-903a927e0d90,78e38655-d951-41db-a0ff-d6474775cfa1" );

            RockMigrationHelper.UpdateDefinedValue_pre20140819( "407A3A73-A3EF-4970-B856-2A33F62AC72E", "Secondary Button", "", "8CF6E927-4FA5-4241-991C-391038B79631", false );
            RockMigrationHelper.AddDefinedValueAttributeValue( "8CF6E927-4FA5-4241-991C-391038B79631", "6FF59F53-28EA-4BFE-AFE1-A459CC588495", @"<a href=""{{ ButtonLink }}"" onclick=""{{ ButtonClick }}"" class=""btn btn-link"" data-loading-text=""<i class='fa fa-refresh fa-spin'></i> {{ ButtonText }}"">{{ ButtonText }}</a>" );

            Sql( @"
UPDATE [Category] SET [Order] = 1 WHERE [Guid] = 'BBAE05FD-8192-4616-A71E-903A927E0D90'
UPDATE [Category] SET [Order] = 2 WHERE [Guid] = '78E38655-D951-41DB-A0FF-D6474775CFA1'

DECLARE @EntityTypeId int = (SELECT TOP 1 [Id] FROM [EntityType] WHERE [Name] = 'Rock.Storage.Provider.Database')
INSERT INTO [BinaryFileType]
           ([IsSystem]
           ,[Name]
           ,[Description]
           ,[IconCssClass]
           ,[StorageEntityTypeId]
           ,[AllowCaching]
           ,[Guid])
     VALUES
           (1
           ,'Location Image'
           ,'Image associated with a named location (e.g. Campus, Room)'
           ,'fa fa-building'
           ,@EntityTypeId
           ,1
           ,'DAB74416-3272-4411-BA69-70944B549A4B')


    DECLARE @AttributeId int = (SELECT [Id] FROM [Attribute] WHERE [Guid] = '6FF59F53-28EA-4BFE-AFE1-A459CC588495' )

    DECLARE @FieldTypeId int = (SELECT [Id] FROM [FieldType] WHERE [Guid] = '1D0D3794-C210-48A8-8C68-3FBEC08A6BA5')
    UPDATE [Attribute] SET
         [Name] = 'Button HTML'
        ,[Key] = 'ButtonHTML'
        ,[Description] = 'The HTML to use for displaying a button. ""{{ ButtonLink }}"", ""{{ ButtonClick }}"" and ""{{ ButtonText }}"" merge fields are available to be used in the HTML as placeholders.  {{ ButtonLink }} will be replaced with the url that user should be redirected to when they select that button.  {{ ButtonClick }} is replaced with any necessary client script (e.g. validation and animation).  {{ ButtonText }} is replaced with the name to be displayed.'
        ,[FieldTypeId] = @FieldTypeId
    WHERE [Id] = @AttributeId

    DECLARE @DefinedValueId int = (SELECT [ID] FROM [DefinedValue] WHERE [Guid] = 'FDC397CD-8B4A-436E-BEA1-BCE2E6717C03')
    UPDATE [DefinedValue] SET [Order] = 0 WHERE [Id] = @DefinedValueID
    UPDATE [AttributeValue] SET [Value] = '<a href=""{{ ButtonLink }}"" onclick=""{{ ButtonClick }}"" class=""btn btn-primary"" data-loading-text=""<i class=''fa fa-refresh fa-spin''></i> {{ ButtonText }}"">{{ ButtonText }}</a>'
	WHERE [AttributeId] = @AttributeId AND [EntityId] = CAST( @DefinedValueId AS varchar )

    SET @DefinedValueId = (SELECT [ID] FROM [DefinedValue] WHERE [Guid] = 'FDEB8E6C-70C3-4033-B307-7D0DEE1AC29D')
    UPDATE [DefinedValue] SET [Order] = 1 WHERE [Id] = @DefinedValueID
    UPDATE [AttributeValue] SET [Value] = '<a href=""{{ ButtonLink }}"" onclick=""{{ ButtonClick }}"" class=""btn btn-danger"" data-loading-text=""<i class=''fa fa-refresh fa-spin''></i> {{ ButtonText }}"">{{ ButtonText }}</a>'
	WHERE [AttributeId] = @AttributeId AND [EntityId] = CAST( @DefinedValueId AS varchar )

    SET @DefinedValueId = (SELECT [ID] FROM [DefinedValue] WHERE [Guid] = '8CF6E927-4FA5-4241-991C-391038B79631')
    UPDATE [DefinedValue] SET [Order] = 2 WHERE [Id] = @DefinedValueID
" );

            RockMigrationHelper.UpdateSystemEmail_pre201409101843015( "Workflow", "Workflow Form Notification", "", "", "", "", "", "{{ Workflow.WorkflowType.Name }}: {{ Workflow.Name }}", @"
{{ GlobalAttribute.EmailHeader }}

<p>{{ Person.FirstName }},</p>
<p>The following {{ Workflow.WorkflowType.Name }} requires action:<p>
<p>{{ Workflow.WorkflowType.WorkTerm}}: <a href='{{ GlobalAttribute.InternalApplicationRoot }}WorkflowEntry/{{ Workflow.WorkflowTypeId }}/{{ Workflow.Id }}'>{{ Workflow.Name }}</a></p>

{% assign RequiredFields = false %}

<h4>Details:</h4>
<p>
{% for attribute in Action.FormAttributes %}

    <strong>{{ attribute.Name }}:</strong> 

    {% if attribute.Url != Empty %}
        <a href='{{ attribute.Url }}'>{{ attribute.Value }}</a>
    {% else %}
        {{ attribute.Value }}
    {% endif %}
    <br/>

    {% if attribute.IsRequired %}
        {% assign RequiredFields = true %}
    {% endif %}

{% endfor %}
</p>


{% if Action.ActionType.WorkflowForm.IncludeActionsInNotification == true %}

    {% if RequiredFields != true %}

        <p>
        {% for button in Action.ActionType.WorkflowForm.Buttons %}

            {% capture ButtonLinkSearch %}{% raw %}{{ ButtonLink }}{% endraw %}{% endcapture %}
            {% capture ButtonLinkReplace %}{{ GlobalAttribute.InternalApplicationRoot }}WorkflowEntry/{{ Workflow.WorkflowTypeId }}/{{ Workflow.Id }}?action={{ button.Name }}{% endcapture %}
            {% capture ButtonHtml %}{{ button.Html | Replace: ButtonLinkSearch, ButtonLinkReplace }}{% endcapture %}

            {% capture ButtonTextSearch %}{% raw %}{{ ButtonText }}{% endraw %}{% endcapture %}
            {% capture ButtonTextReplace %}{{ button.Name }}{% endcapture %}
            {{ ButtonHtml | Replace: ButtonTextSearch, ButtonTextReplace }}

        {% endfor %}
        </p>

    {% endif %}

{% endif %}

{{ GlobalAttribute.EmailFooter }}
", "88C7D1CC-3478-4562-A301-AE7D4D7FFF6D" );

            #endregion

            #region Add Workflows

            RockMigrationHelper.UpdateEntityType( "Rock.Model.Workflow", "3540E9A7-FE30-43A9-8B0A-A372B63DFC93", true, true );
            RockMigrationHelper.UpdateEntityType( "Rock.Model.WorkflowActivity", "2CB52ED0-CB06-4D62-9E2C-73B60AFA4C9F", true, true );
            RockMigrationHelper.UpdateEntityType( "Rock.Model.WorkflowActionType", "23E3273A-B137-48A3-9AFF-C8DC832DDCA6", true, true );
            RockMigrationHelper.UpdateEntityType( "Rock.Workflow.Action.ActivateActions", "699756EF-28EB-444B-BD28-15F0A167E614", false, true );
            RockMigrationHelper.UpdateEntityType( "Rock.Workflow.Action.ActivateActivity", "38907A90-1634-4A93-8017-619326A4A582", false, true );
            RockMigrationHelper.UpdateEntityType( "Rock.Workflow.Action.AssignActivityToAttributeValue", "F100A31F-E93A-4C7A-9E55-0FAF41A101C4", false, true );
            RockMigrationHelper.UpdateEntityType( "Rock.Workflow.Action.AssignActivityToPerson", "FB2981B7-7922-42E1-8ACF-7F63BB7989E6", false, true );
            RockMigrationHelper.UpdateEntityType( "Rock.Workflow.Action.CompleteWorkflow", "EEDA4318-F014-4A46-9C76-4C052EF81AA1", false, true );
            RockMigrationHelper.UpdateEntityType( "Rock.Workflow.Action.PersistWorkflow", "F1A39347-6FE0-43D4-89FB-544195088ECF", false, true );
            RockMigrationHelper.UpdateEntityType( "Rock.Workflow.Action.SendEmail", "66197B01-D1F0-4924-A315-47AD54E030DE", false, true );
            RockMigrationHelper.UpdateEntityType( "Rock.Workflow.Action.SetAttributeToCurrentPerson", "24B7D5E6-C30F-48F4-9D7E-AF45A342CF3A", false, true );
            RockMigrationHelper.UpdateEntityType( "Rock.Workflow.Action.SetAttributeToEntity", "972F19B9-598B-474B-97A4-50E56E7B59D2", false, true );
            RockMigrationHelper.UpdateEntityType( "Rock.Workflow.Action.SetAttributeValue", "C789E457-0783-44B3-9D8F-2EBAB5F11110", false, true );
            RockMigrationHelper.UpdateEntityType( "Rock.Workflow.Action.SetName", "36005473-BD5D-470B-B28D-98E6D7ED808D", false, true );
            RockMigrationHelper.UpdateEntityType( "Rock.Workflow.Action.SetPersonAttributeValue", "17962C23-2E94-4E06-8461-0FB8B94E2FEA", false, true );
            RockMigrationHelper.UpdateEntityType( "Rock.Workflow.Action.UserEntryForm", "486DC4FA-FCBC-425F-90B0-E606DA8A9F68", false, true );
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute( "17962C23-2E94-4E06-8461-0FB8B94E2FEA", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Active", "Active", "Should Service be used?", 0, @"False", "CE28B79D-FBC2-4894-9198-D923D0217549" ); // Rock.Workflow.Action.SetPersonAttributeValue:Active
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute( "17962C23-2E94-4E06-8461-0FB8B94E2FEA", "33E6DF69-BDFA-407A-9744-C175B60643AE", "Attribute", "Attribute", "The attribute to set the value of.", 0, @"", "7AC47975-71AC-4A2F-BF1F-115CF5578D6F" ); // Rock.Workflow.Action.SetPersonAttributeValue:Attribute
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute( "17962C23-2E94-4E06-8461-0FB8B94E2FEA", "A75DFC58-7A1B-4799-BF31-451B2BBE38FF", "Order", "Order", "The order that this service should be used (priority)", 0, @"", "18EF907D-607E-4891-B034-7AA379D77854" ); // Rock.Workflow.Action.SetPersonAttributeValue:Order
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute( "17962C23-2E94-4E06-8461-0FB8B94E2FEA", "E4EAB7B2-0B76-429B-AFE4-AD86D7428C70", "Person", "Person", "The person to set attribute value to", 1, @"", "5C803BD1-40FA-49B1-AE7E-68F43D3687BB" ); // Rock.Workflow.Action.SetPersonAttributeValue:Person
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute( "24B7D5E6-C30F-48F4-9D7E-AF45A342CF3A", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Active", "Active", "Should Service be used?", 0, @"False", "DE9CB292-4785-4EA3-976D-3826F91E9E98" ); // Rock.Workflow.Action.SetAttributeToCurrentPerson:Active
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute( "24B7D5E6-C30F-48F4-9D7E-AF45A342CF3A", "33E6DF69-BDFA-407A-9744-C175B60643AE", "Person Attribute", "PersonAttribute", "The attribute to set to the currently logged in person.", 0, @"", "BBED8A83-8BB2-4D35-BAFB-05F67DCAD112" ); // Rock.Workflow.Action.SetAttributeToCurrentPerson:Person Attribute
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute( "24B7D5E6-C30F-48F4-9D7E-AF45A342CF3A", "A75DFC58-7A1B-4799-BF31-451B2BBE38FF", "Order", "Order", "The order that this service should be used (priority)", 0, @"", "89E9BCED-91AB-47B0-AD52-D78B0B7CB9E8" ); // Rock.Workflow.Action.SetAttributeToCurrentPerson:Order
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute( "36005473-BD5D-470B-B28D-98E6D7ED808D", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Active", "Active", "Should Service be used?", 0, @"False", "0A800013-51F7-4902-885A-5BE215D67D3D" ); // Rock.Workflow.Action.SetName:Active
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute( "36005473-BD5D-470B-B28D-98E6D7ED808D", "3B1D93D7-9414-48F9-80E5-6A3FC8F94C20", "Text Value|Attribute Value", "NameValue", "The value to use for the workflow's name", 1, @"", "93852244-A667-4749-961A-D47F88675BE4" ); // Rock.Workflow.Action.SetName:Text Value|Attribute Value
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute( "36005473-BD5D-470B-B28D-98E6D7ED808D", "A75DFC58-7A1B-4799-BF31-451B2BBE38FF", "Order", "Order", "The order that this service should be used (priority)", 0, @"", "5D95C15A-CCAE-40AD-A9DD-F929DA587115" ); // Rock.Workflow.Action.SetName:Order
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute( "38907A90-1634-4A93-8017-619326A4A582", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Active", "Active", "Should Service be used?", 0, @"False", "E8ABD802-372C-47BE-82B1-96F50DB5169E" ); // Rock.Workflow.Action.ActivateActivity:Active
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute( "38907A90-1634-4A93-8017-619326A4A582", "739FD425-5B8C-4605-B775-7E4D9D4C11DB", "Activity", "Activity", "The activity type to activate", 0, @"", "02D5A7A5-8781-46B4-B9FC-AF816829D240" ); // Rock.Workflow.Action.ActivateActivity:Activity
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute( "38907A90-1634-4A93-8017-619326A4A582", "A75DFC58-7A1B-4799-BF31-451B2BBE38FF", "Order", "Order", "The order that this service should be used (priority)", 0, @"", "3809A78C-B773-440C-8E3F-A8E81D0DAE08" ); // Rock.Workflow.Action.ActivateActivity:Order
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute( "486DC4FA-FCBC-425F-90B0-E606DA8A9F68", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Active", "Active", "Should Service be used?", 0, @"False", "234910F2-A0DB-4D7D-BAF7-83C880EF30AE" ); // Rock.Workflow.Action.UserEntryForm:Active
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute( "486DC4FA-FCBC-425F-90B0-E606DA8A9F68", "A75DFC58-7A1B-4799-BF31-451B2BBE38FF", "Order", "Order", "The order that this service should be used (priority)", 0, @"", "C178113D-7C86-4229-8424-C6D0CF4A7E23" ); // Rock.Workflow.Action.UserEntryForm:Order
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute( "66197B01-D1F0-4924-A315-47AD54E030DE", "1D0D3794-C210-48A8-8C68-3FBEC08A6BA5", "Body", "Body", "The body of the email that should be sent", 3, @"", "4D245B9E-6B03-46E7-8482-A51FBA190E4D" ); // Rock.Workflow.Action.SendEmail:Body
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute( "66197B01-D1F0-4924-A315-47AD54E030DE", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Active", "Active", "Should Service be used?", 0, @"False", "36197160-7D3D-490D-AB42-7E29105AFE91" ); // Rock.Workflow.Action.SendEmail:Active
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute( "66197B01-D1F0-4924-A315-47AD54E030DE", "3B1D93D7-9414-48F9-80E5-6A3FC8F94C20", "Send To Email Address|Attribute Value", "To", "The email address or an attribute that contains the person or email address that email should be sent to", 0, @"", "0C4C13B8-7076-4872-925A-F950886B5E16" ); // Rock.Workflow.Action.SendEmail:Send To Email Address|Attribute Value
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute( "66197B01-D1F0-4924-A315-47AD54E030DE", "9C204CD0-1233-41C5-818A-C5DA439445AA", "From", "From", "The From address that email should be sent from  (will default to organization email).", 1, @"", "9F5F7CEC-F369-4FDF-802A-99074CE7A7FC" ); // Rock.Workflow.Action.SendEmail:From
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute( "66197B01-D1F0-4924-A315-47AD54E030DE", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Subject", "Subject", "The subject that should be used when sending email.", 2, @"", "5D9B13B6-CD96-4C7C-86FA-4512B9D28386" ); // Rock.Workflow.Action.SendEmail:Subject
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute( "66197B01-D1F0-4924-A315-47AD54E030DE", "A75DFC58-7A1B-4799-BF31-451B2BBE38FF", "Order", "Order", "The order that this service should be used (priority)", 0, @"", "D1269254-C15A-40BD-B784-ADCC231D3950" ); // Rock.Workflow.Action.SendEmail:Order
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute( "699756EF-28EB-444B-BD28-15F0A167E614", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Active", "Active", "Should Service be used?", 0, @"False", "A134F1A7-3824-43E0-9EB1-22C899B795BD" ); // Rock.Workflow.Action.ActivateActions:Active
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute( "699756EF-28EB-444B-BD28-15F0A167E614", "A75DFC58-7A1B-4799-BF31-451B2BBE38FF", "Order", "Order", "The order that this service should be used (priority)", 0, @"", "5DA71523-E8B0-4C4D-89A4-B47945A22A0C" ); // Rock.Workflow.Action.ActivateActions:Order
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute( "972F19B9-598B-474B-97A4-50E56E7B59D2", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Active", "Active", "Should Service be used?", 0, @"False", "9392E3D7-A28B-4CD8-8B03-5E147B102EF1" ); // Rock.Workflow.Action.SetAttributeToEntity:Active
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute( "972F19B9-598B-474B-97A4-50E56E7B59D2", "33E6DF69-BDFA-407A-9744-C175B60643AE", "Attribute", "Attribute", "The attribute to set the value of.", 0, @"", "61E6E1BC-E657-4F00-B2E9-769AAA25B9F7" ); // Rock.Workflow.Action.SetAttributeToEntity:Attribute
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute( "972F19B9-598B-474B-97A4-50E56E7B59D2", "A75DFC58-7A1B-4799-BF31-451B2BBE38FF", "Order", "Order", "The order that this service should be used (priority)", 0, @"", "AD4EFAC4-E687-43DF-832F-0DC3856ABABB" ); // Rock.Workflow.Action.SetAttributeToEntity:Order
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute( "C789E457-0783-44B3-9D8F-2EBAB5F11110", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Active", "Active", "Should Service be used?", 0, @"False", "D7EAA859-F500-4521-9523-488B12EAA7D2" ); // Rock.Workflow.Action.SetAttributeValue:Active
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute( "C789E457-0783-44B3-9D8F-2EBAB5F11110", "33E6DF69-BDFA-407A-9744-C175B60643AE", "Attribute", "Attribute", "The attribute to set the value of.", 0, @"", "44A0B977-4730-4519-8FF6-B0A01A95B212" ); // Rock.Workflow.Action.SetAttributeValue:Attribute
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute( "C789E457-0783-44B3-9D8F-2EBAB5F11110", "3B1D93D7-9414-48F9-80E5-6A3FC8F94C20", "Text Value|Attribute Value", "Value", "The text or attribute to set the value from", 1, @"", "E5272B11-A2B8-49DC-860D-8D574E2BC15C" ); // Rock.Workflow.Action.SetAttributeValue:Text Value|Attribute Value
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute( "C789E457-0783-44B3-9D8F-2EBAB5F11110", "A75DFC58-7A1B-4799-BF31-451B2BBE38FF", "Order", "Order", "The order that this service should be used (priority)", 0, @"", "57093B41-50ED-48E5-B72B-8829E62704C8" ); // Rock.Workflow.Action.SetAttributeValue:Order
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute( "EEDA4318-F014-4A46-9C76-4C052EF81AA1", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Active", "Active", "Should Service be used?", 0, @"False", "0CA0DDEF-48EF-4ABC-9822-A05E225DE26C" ); // Rock.Workflow.Action.CompleteWorkflow:Active
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute( "EEDA4318-F014-4A46-9C76-4C052EF81AA1", "A75DFC58-7A1B-4799-BF31-451B2BBE38FF", "Order", "Order", "The order that this service should be used (priority)", 0, @"", "25CAD4BE-5A00-409D-9BAB-E32518D89956" ); // Rock.Workflow.Action.CompleteWorkflow:Order
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute( "F100A31F-E93A-4C7A-9E55-0FAF41A101C4", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Active", "Active", "Should Service be used?", 0, @"False", "E0F7AB7E-7761-4600-A099-CB14ACDBF6EF" ); // Rock.Workflow.Action.AssignActivityToAttributeValue:Active
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute( "F100A31F-E93A-4C7A-9E55-0FAF41A101C4", "33E6DF69-BDFA-407A-9744-C175B60643AE", "Attribute", "Attribute", "The person or group attribute value to assign this activity to.", 0, @"", "FBADD25F-D309-4512-8430-3CC8615DD60E" ); // Rock.Workflow.Action.AssignActivityToAttributeValue:Attribute
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute( "F100A31F-E93A-4C7A-9E55-0FAF41A101C4", "A75DFC58-7A1B-4799-BF31-451B2BBE38FF", "Order", "Order", "The order that this service should be used (priority)", 0, @"", "7A6B605D-7FB1-4F48-AF35-5A0683FB1CDA" ); // Rock.Workflow.Action.AssignActivityToAttributeValue:Order
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute( "F1A39347-6FE0-43D4-89FB-544195088ECF", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Active", "Active", "Should Service be used?", 0, @"False", "50B01639-4938-40D2-A791-AA0EB4F86847" ); // Rock.Workflow.Action.PersistWorkflow:Active
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute( "F1A39347-6FE0-43D4-89FB-544195088ECF", "A75DFC58-7A1B-4799-BF31-451B2BBE38FF", "Order", "Order", "The order that this service should be used (priority)", 0, @"", "86F795B0-0CB6-4DA4-9CE4-B11D0922F361" ); // Rock.Workflow.Action.PersistWorkflow:Order
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute( "FB2981B7-7922-42E1-8ACF-7F63BB7989E6", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Active", "Active", "Should Service be used?", 0, @"False", "0B768E17-C64A-4212-BAD5-8A16B9F05A5C" ); // Rock.Workflow.Action.AssignActivityToPerson:Active
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute( "FB2981B7-7922-42E1-8ACF-7F63BB7989E6", "A75DFC58-7A1B-4799-BF31-451B2BBE38FF", "Order", "Order", "The order that this service should be used (priority)", 0, @"", "5C5F7DB4-51DE-4293-BD73-CABDEB6564AC" ); // Rock.Workflow.Action.AssignActivityToPerson:Order
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute( "FB2981B7-7922-42E1-8ACF-7F63BB7989E6", "E4EAB7B2-0B76-429B-AFE4-AD86D7428C70", "Person", "Person", "The person to assign this activity to.", 0, @"", "7ED2571D-B1BF-4DB6-9D04-9B5D064F51D8" ); // Rock.Workflow.Action.AssignActivityToPerson:Person
            RockMigrationHelper.UpdateCategory( "C9F3C4A5-1526-474D-803F-D6C7A45CBBAE", "Person", "fa fa-user", "", "BBAE05FD-8192-4616-A71E-903A927E0D90", 1 ); // Person
            RockMigrationHelper.UpdateCategory( "C9F3C4A5-1526-474D-803F-D6C7A45CBBAE", "Requests", "fa fa-question-circle", "", "78E38655-D951-41DB-A0FF-D6474775CFA1", 2 ); // Requests
            RockMigrationHelper.UpdateWorkflowType( false, true, "Person Data Error", "Used to report an issue with a person profile record.", "BBAE05FD-8192-4616-A71E-903A927E0D90", "Report", "fa fa-exclamation", 0, false, 0, "221BF486-A82C-40A7-85B7-BB44DA45582F" ); // Person Data Error
            RockMigrationHelper.UpdateWorkflowType( false, true, "External Inquiry", "Used on public website when visitor selects to \"Contact Us\"", "78E38655-D951-41DB-A0FF-D6474775CFA1", "Inquiry", "fa fa-phone", 0, false, 0, "236AB611-EDE8-42B5-B559-6B6A88ADDDCB" ); // External Inquiry
            RockMigrationHelper.UpdateWorkflowType( false, true, "Facilities Request", "Request the maintenance team to fix a facilities issue.", "78E38655-D951-41DB-A0FF-D6474775CFA1", "Work Request", "fa fa-building", 0, false, 0, "417D8016-92DC-4F25-ACFF-A071B591FA4F" ); // Facilities Request
            RockMigrationHelper.UpdateWorkflowType( false, true, "IT Support", "Request support for a computer hardware and/or software issue.", "78E38655-D951-41DB-A0FF-D6474775CFA1", "Work Request", "fa fa-desktop", 0, false, 0, "51FE9641-FB8F-41BF-B09E-235900C3E53E" ); // IT Support
            RockMigrationHelper.UpdateWorkflowTypeAttribute( "221BF486-A82C-40A7-85B7-BB44DA45582F", "E4EAB7B2-0B76-429B-AFE4-AD86D7428C70", "Person", "Person", "The person that the data error is being reported for.  If this issue was initiated directly from the person profile record (using 'Actions' option), this value will automatically be populated.", 0, @"", "FD89F2C8-CBC8-4ED1-96D1-891AB9616C9E" ); // Person Data Error:Person
            RockMigrationHelper.UpdateWorkflowTypeAttribute( "221BF486-A82C-40A7-85B7-BB44DA45582F", "E4EAB7B2-0B76-429B-AFE4-AD86D7428C70", "Reported By", "ReportedBy", "The person who reported the data error.", 1, @"", "C8FD97B5-ADC0-4EF0-B397-E4010850148C" ); // Person Data Error:Reported By
            RockMigrationHelper.UpdateWorkflowTypeAttribute( "221BF486-A82C-40A7-85B7-BB44DA45582F", "C28C7BF3-A552-4D77-9408-DEDCF760CED0", "Details ( Please be as specific as possible )", "Details", "Please enter as much information as possible regarding the details about the data error for the selected person.", 2, @"", "094C9A07-F5CB-49DB-8B67-9B7616356D2C" ); // Person Data Error:Details ( Please be as specific as possible )
            RockMigrationHelper.UpdateWorkflowTypeAttribute( "221BF486-A82C-40A7-85B7-BB44DA45582F", "C28C7BF3-A552-4D77-9408-DEDCF760CED0", "Resolution", "Resolution", "Optional notes about how the issue was resolved.  This information is not stored with the person's profile. It is only used to communicate back to the requester about what was fixed (if anything).", 3, @"", "83F32DB1-8418-4748-BD52-A9E3FACA3F6E" ); // Person Data Error:Resolution
            RockMigrationHelper.UpdateWorkflowTypeAttribute( "236AB611-EDE8-42B5-B559-6B6A88ADDDCB", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Email Address", "EmailAddress", "Your email address.", 3, @"", "61FBEE01-A087-4060-B5D0-1262308BFAB4" ); // External Inquiry:Email Address
            RockMigrationHelper.UpdateWorkflowTypeAttribute( "236AB611-EDE8-42B5-B559-6B6A88ADDDCB", "9C204CD0-1233-41C5-818A-C5DA439445AA", "First Name", "FirstName", "Your First Name", 1, @"", "5C3EDA60-0791-43ED-8E2B-E98276D543D1" ); // External Inquiry:First Name
            RockMigrationHelper.UpdateWorkflowTypeAttribute( "236AB611-EDE8-42B5-B559-6B6A88ADDDCB", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Last Name", "LastName", "Your Last Name", 2, @"", "2C35E7E1-5831-4C5F-AC50-97ED6EE7D5E0" ); // External Inquiry:Last Name
            RockMigrationHelper.UpdateWorkflowTypeAttribute( "236AB611-EDE8-42B5-B559-6B6A88ADDDCB", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Phone (optional)", "Phone", "Your phone number", 4, @"", "D4D3588B-0A13-432C-BAFB-9964EBB2B9F4" ); // External Inquiry:Phone (optional)
            RockMigrationHelper.UpdateWorkflowTypeAttribute( "236AB611-EDE8-42B5-B559-6B6A88ADDDCB", "7525C4CB-EE6B-41D4-9B64-A08048D5A5C0", "Topic", "Topic", "The nature of your inquiry", 5, @"", "DA61CA95-0106-49EE-962B-F70042E1464E" ); // External Inquiry:Topic
            RockMigrationHelper.UpdateWorkflowTypeAttribute( "236AB611-EDE8-42B5-B559-6B6A88ADDDCB", "1B71FEF4-201F-4D53-8C60-2DF21F1985ED", "Campus", "Campus", "The campus (if any) that this inquiry is related to", 7, @"", "754A0725-29FC-407B-87E9-95D697F49FFE" ); // External Inquiry:Campus
            RockMigrationHelper.UpdateWorkflowTypeAttribute( "236AB611-EDE8-42B5-B559-6B6A88ADDDCB", "C28C7BF3-A552-4D77-9408-DEDCF760CED0", "Inquiry", "Inquiry", "The details of your inquiry", 6, @"", "B7BE8727-31A2-443D-AF78-A9B3A2712143" ); // External Inquiry:Inquiry
            RockMigrationHelper.UpdateWorkflowTypeAttribute( "236AB611-EDE8-42B5-B559-6B6A88ADDDCB", "E4EAB7B2-0B76-429B-AFE4-AD86D7428C70", "Requester", "Requester", "The person who made the request. This is only available if the user was logged into the website when they made the inquiry.", 0, @"", "F5AE12D8-DE33-4C8A-A372-DE87134E3F75" ); // External Inquiry:Requester
            RockMigrationHelper.UpdateWorkflowTypeAttribute( "236AB611-EDE8-42B5-B559-6B6A88ADDDCB", "E4EAB7B2-0B76-429B-AFE4-AD86D7428C70", "Worker", "Worker", "The person responsible to follow up on the inquiry", 8, @"", "04712FBD-715D-412E-96C3-10C748482D6E" ); // External Inquiry:Worker
            RockMigrationHelper.UpdateWorkflowTypeAttribute( "236AB611-EDE8-42B5-B559-6B6A88ADDDCB", "C28C7BF3-A552-4D77-9408-DEDCF760CED0", "Notes", "Notes", "Staff notes about this inquiry", 10, @"", "AB12CA58-0284-4D05-BF5F-4003C04E43B4" ); // External Inquiry:Notes
            RockMigrationHelper.UpdateWorkflowTypeAttribute( "236AB611-EDE8-42B5-B559-6B6A88ADDDCB", "E4EAB7B2-0B76-429B-AFE4-AD86D7428C70", "Assign New Worker", "NewWorker", "If this inquiry needs to be re-assigned to a different person, select that person here.", 9, @"", "56A33DB7-FAC1-41F9-B42D-CC24D7A20466" ); // External Inquiry:Assign New Worker
            RockMigrationHelper.UpdateWorkflowTypeAttribute( "417D8016-92DC-4F25-ACFF-A071B591FA4F", "1B71FEF4-201F-4D53-8C60-2DF21F1985ED", "Campus", "Campus", "", 0, @"", "12EB0CCE-8012-498D-B95E-EB41090F8889" ); // Facilities Request:Campus
            RockMigrationHelper.UpdateWorkflowTypeAttribute( "417D8016-92DC-4F25-ACFF-A071B591FA4F", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Title", "Summary", "Title of the request (please keep short)", 1, @"", "8767527F-C1DC-42E2-B24D-6061C74830EF" ); // Facilities Request:Title
            RockMigrationHelper.UpdateWorkflowTypeAttribute( "417D8016-92DC-4F25-ACFF-A071B591FA4F", "E4EAB7B2-0B76-429B-AFE4-AD86D7428C70", "Requester", "Requester", "The person who made the request", 3, @"", "594D522B-1E69-4175-9C6B-039C6BAF279E" ); // Facilities Request:Requester
            RockMigrationHelper.UpdateWorkflowTypeAttribute( "417D8016-92DC-4F25-ACFF-A071B591FA4F", "E4EAB7B2-0B76-429B-AFE4-AD86D7428C70", "Worker", "Worker", "The person assigned to work on the request", 4, @"", "E0D71111-05F1-4CD3-959E-1D246613942E" ); // Facilities Request:Worker
            RockMigrationHelper.UpdateWorkflowTypeAttribute( "417D8016-92DC-4F25-ACFF-A071B591FA4F", "E4EAB7B2-0B76-429B-AFE4-AD86D7428C70", "New Worker", "NewWorker", "If the request needs to be re-assigned to a different person, select the new person here.", 5, @"", "8F9E8196-087F-49D6-9879-AA139C7A1225" ); // Facilities Request:New Worker
            RockMigrationHelper.UpdateWorkflowTypeAttribute( "417D8016-92DC-4F25-ACFF-A071B591FA4F", "C28C7BF3-A552-4D77-9408-DEDCF760CED0", "Internal Notes", "Notes", "Notes about the request for internal use ( Resolution will be entered when completing the request )", 6, @"", "06912CB0-24A7-45AE-892B-F95A85D76DB2" ); // Facilities Request:Internal Notes
            RockMigrationHelper.UpdateWorkflowTypeAttribute( "417D8016-92DC-4F25-ACFF-A071B591FA4F", "C28C7BF3-A552-4D77-9408-DEDCF760CED0", "Resolution", "Resolution", "How the issue was resolved", 8, @"", "2CB0B49F-453D-4D41-9431-3BE18C2CC842" ); // Facilities Request:Resolution
            RockMigrationHelper.UpdateWorkflowTypeAttribute( "417D8016-92DC-4F25-ACFF-A071B591FA4F", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Waiting on Parts", "WaitingonParts", "Is this request still open due to waiting on equipment", 7, @"False", "1D313E0A-26D7-4A7A-94F1-45E733569C88" ); // Facilities Request:Waiting on Parts
            RockMigrationHelper.UpdateWorkflowTypeAttribute( "417D8016-92DC-4F25-ACFF-A071B591FA4F", "C28C7BF3-A552-4D77-9408-DEDCF760CED0", "Details", "Details", "The details about the facilities issue", 2, @"", "0DCDB750-4789-4A5F-B673-4954E94A81B4" ); // Facilities Request:Details
            RockMigrationHelper.UpdateWorkflowTypeAttribute( "51FE9641-FB8F-41BF-B09E-235900C3E53E", "1B71FEF4-201F-4D53-8C60-2DF21F1985ED", "Campus", "Campus", "", 0, @"", "9DEC1697-1ED6-4CC0-B054-F9E0D975DF12" ); // IT Support:Campus
            RockMigrationHelper.UpdateWorkflowTypeAttribute( "51FE9641-FB8F-41BF-B09E-235900C3E53E", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Title", "Summary", "Title of the request (please keep short)", 1, @"", "59F1FB67-F2B3-441E-ACE3-9B10AB7E674F" ); // IT Support:Title
            RockMigrationHelper.UpdateWorkflowTypeAttribute( "51FE9641-FB8F-41BF-B09E-235900C3E53E", "C28C7BF3-A552-4D77-9408-DEDCF760CED0", "Details", "Details", "Details about the hardware and/or software issue", 2, @"", "9C2E0B04-8319-4967-9E0A-DB062398566C" ); // IT Support:Details
            RockMigrationHelper.UpdateWorkflowTypeAttribute( "51FE9641-FB8F-41BF-B09E-235900C3E53E", "E4EAB7B2-0B76-429B-AFE4-AD86D7428C70", "Requester", "Requester", "The person who made the request", 3, @"", "CE01E29E-4A9C-4BBF-BBCD-235713A81353" ); // IT Support:Requester
            RockMigrationHelper.UpdateWorkflowTypeAttribute( "51FE9641-FB8F-41BF-B09E-235900C3E53E", "E4EAB7B2-0B76-429B-AFE4-AD86D7428C70", "Worker", "Worker", "The person assigned to work on the request", 4, @"", "9D165825-21F2-4B5F-9A64-790A6EBD51AC" ); // IT Support:Worker
            RockMigrationHelper.UpdateWorkflowTypeAttribute( "51FE9641-FB8F-41BF-B09E-235900C3E53E", "E4EAB7B2-0B76-429B-AFE4-AD86D7428C70", "New Worker", "NewWorker", "If the request needs to be re-assigned to a different person, select the new person here.", 5, @"", "91D7D16C-F6A1-4B85-A7EE-A0D20B5A2BC5" ); // IT Support:New Worker
            RockMigrationHelper.UpdateWorkflowTypeAttribute( "51FE9641-FB8F-41BF-B09E-235900C3E53E", "C28C7BF3-A552-4D77-9408-DEDCF760CED0", "Internal Notes", "Notes", "Notes about the request for internal use ( Resolution will be entered when completing the request )", 6, @"", "E616FDCF-A7F7-4EEA-B4E8-35320C24C726" ); // IT Support:Internal Notes
            RockMigrationHelper.UpdateWorkflowTypeAttribute( "51FE9641-FB8F-41BF-B09E-235900C3E53E", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Waiting on Parts", "WaitingonParts", "Is this request still open due to waiting on equipment", 7, @"False", "90E60C82-CD15-4F21-AEBE-6E00EE340685" ); // IT Support:Waiting on Parts
            RockMigrationHelper.UpdateWorkflowTypeAttribute( "51FE9641-FB8F-41BF-B09E-235900C3E53E", "C28C7BF3-A552-4D77-9408-DEDCF760CED0", "Resolution", "Resolution", "How the issue was resolved", 8, @"", "96F7BABE-49AA-46C5-8016-5E7246C97A48" ); // IT Support:Resolution
            RockMigrationHelper.AddAttributeQualifier( "DA61CA95-0106-49EE-962B-F70042E1464E", "values", @"General:General Inquiry,
Login:Login / Username / Password Assistance,
Website:Feedback about the web site,
Finance:Contributions / Finance,
Missions:Missions / Global Trips,
Pastor:Talk to a Pastor", "8FB4B3A9-E5B5-45CC-B677-88E8624E7CE1" ); // External Inquiry:Topic:values
            RockMigrationHelper.AddAttributeQualifier( "DA61CA95-0106-49EE-962B-F70042E1464E", "fieldtype", @"ddl", "71B97287-03A5-4113-9C8B-88012A64D0F3" ); // External Inquiry:Topic:fieldtype
            RockMigrationHelper.AddAttributeQualifier( "1D313E0A-26D7-4A7A-94F1-45E733569C88", "truetext", @"Yes", "D9137614-5AE5-4301-8E18-60FB93955927" ); // Facilities Request:Waiting on Parts:truetext
            RockMigrationHelper.AddAttributeQualifier( "1D313E0A-26D7-4A7A-94F1-45E733569C88", "falsetext", @"No", "55BC12C2-670B-498E-9ACD-860EEF411F3C" ); // Facilities Request:Waiting on Parts:falsetext
            RockMigrationHelper.UpdateWorkflowActivityType( "221BF486-A82C-40A7-85B7-BB44DA45582F", true, "Entry", "Prompts user for specific details about the data error on the selected person's profile.", true, 1, "F238E2D8-AAE0-481F-BABF-955C0A3343CC" ); // Person Data Error:Entry
            RockMigrationHelper.UpdateWorkflowActivityType( "221BF486-A82C-40A7-85B7-BB44DA45582F", true, "Pending", "Reports the error to the person responsible for addressing data errors on person profile records and waits for them to complete the resolution.", false, 2, "31D6A610-9F0B-4136-8FEA-8F4E192D75B8" ); // Person Data Error:Pending
            RockMigrationHelper.UpdateWorkflowActivityType( "221BF486-A82C-40A7-85B7-BB44DA45582F", true, "Complete", "Notifies the person who reported the error that issue has been completed, and marks the workflow complete.", false, 3, "E9F9E0BE-D38B-4017-954A-5B15C1EED2DE" ); // Person Data Error:Complete
            RockMigrationHelper.UpdateWorkflowActivityType( "221BF486-A82C-40A7-85B7-BB44DA45582F", true, "Launch From Person Profile", "When this workflow is initiated from the Person Profile page, the \"Entity\" will have a value so the first action will run successfully, and the workflow will then be persisted.  If the workflow is launched from the Workflows page, the entity will be null so the first action will not run successfully and workflow will not be persisted until user enters information in the next activity.", true, 0, "6AA06B90-5BC1-464B-A660-C481853C1B32" ); // Person Data Error:Launch From Person Profile
            RockMigrationHelper.UpdateWorkflowActivityType( "236AB611-EDE8-42B5-B559-6B6A88ADDDCB", true, "Request", "Prompt the user for the information about their request", true, 0, "D8C39D36-DB70-4200-98C3-A040FA97B53D" ); // External Inquiry:Request
            RockMigrationHelper.UpdateWorkflowActivityType( "236AB611-EDE8-42B5-B559-6B6A88ADDDCB", true, "General Inquiry", "Assign the worker to the person responsible for 'General Inquiry' topic inquiries.", false, 1, "B89148C7-EF85-450D-81CB-F0BF2102E0CB" ); // External Inquiry:General Inquiry
            RockMigrationHelper.UpdateWorkflowActivityType( "236AB611-EDE8-42B5-B559-6B6A88ADDDCB", true, "Login Inquiry", "Assign the worker to the person responsible for 'Login / Username / Password Assistance' topic inquiries", false, 2, "D73B1B06-30F6-4504-B95F-2322AFE96E55" ); // External Inquiry:Login Inquiry
            RockMigrationHelper.UpdateWorkflowActivityType( "236AB611-EDE8-42B5-B559-6B6A88ADDDCB", true, "Website Inquiry", "Assign the worker to the person responsible for 'Feedback about the web site' topic inquiries", false, 3, "ACE58142-04DC-4B87-BC78-F6C9E00C68A9" ); // External Inquiry:Website Inquiry
            RockMigrationHelper.UpdateWorkflowActivityType( "236AB611-EDE8-42B5-B559-6B6A88ADDDCB", true, "Finance Inquiry", "Assign the worker to the person responsible for 'Contributions / Finance' topic inquiries", false, 4, "C216691B-C40F-475F-B146-8C517433BA9A" ); // External Inquiry:Finance Inquiry
            RockMigrationHelper.UpdateWorkflowActivityType( "236AB611-EDE8-42B5-B559-6B6A88ADDDCB", true, "Missions Inquiry", "Assign the worker to the person responsible for 'Missions / Global Trips' topic inquiries", false, 5, "EC0C5563-00F8-4C3A-A085-BCC8D1926DBA" ); // External Inquiry:Missions Inquiry
            RockMigrationHelper.UpdateWorkflowActivityType( "236AB611-EDE8-42B5-B559-6B6A88ADDDCB", true, "Pastoral Inquiry", "Assign the worker to the person responsible for 'Talk to a Pastor' topic inquiries", false, 6, "074B237A-348C-4BD6-BE1E-0EDDC4F376E9" ); // External Inquiry:Pastoral Inquiry
            RockMigrationHelper.UpdateWorkflowActivityType( "236AB611-EDE8-42B5-B559-6B6A88ADDDCB", true, "Complete", "Complete the workflow", false, 8, "E9A017DF-EB72-4DC9-B2A4-5A424383C7E0" ); // External Inquiry:Complete
            RockMigrationHelper.UpdateWorkflowActivityType( "236AB611-EDE8-42B5-B559-6B6A88ADDDCB", true, "Open", "Activity used to process the inquiry.", false, 7, "2B912003-91E7-43B6-8060-A5ED97C4EDEE" ); // External Inquiry:Open
            RockMigrationHelper.UpdateWorkflowActivityType( "236AB611-EDE8-42B5-B559-6B6A88ADDDCB", true, "Re-assign Worker", "Assigns the inquiry to a new worker", false, 9, "679FE471-517B-4278-BD43-5ECA8340F5C5" ); // External Inquiry:Re-assign Worker
            RockMigrationHelper.UpdateWorkflowActivityType( "417D8016-92DC-4F25-ACFF-A071B591FA4F", true, "Request", "Prompt the user for the information about their request", true, 0, "BBFD8CB9-E4B3-46FC-8923-A67FC995EF94" ); // Facilities Request:Request
            RockMigrationHelper.UpdateWorkflowActivityType( "417D8016-92DC-4F25-ACFF-A071B591FA4F", true, "Assign Worker", "Assigns the request to a worker", false, 1, "FCB5C4EC-E25B-45D2-88E1-51F525A073A4" ); // Facilities Request:Assign Worker
            RockMigrationHelper.UpdateWorkflowActivityType( "417D8016-92DC-4F25-ACFF-A071B591FA4F", true, "Open", "Activity used to process the request", false, 2, "8226D12E-654A-41CF-B8B9-E4DFC6470402" ); // Facilities Request:Open
            RockMigrationHelper.UpdateWorkflowActivityType( "417D8016-92DC-4F25-ACFF-A071B591FA4F", true, "Enter Resolution", "Prompts worker for resolution", false, 3, "EBE339D7-53AD-45E8-8610-19C01BA1A0AB" ); // Facilities Request:Enter Resolution
            RockMigrationHelper.UpdateWorkflowActivityType( "417D8016-92DC-4F25-ACFF-A071B591FA4F", true, "Complete", "Complete the workflow", false, 4, "739CEAE9-4FAB-4FE5-A824-6D2E569D637C" ); // Facilities Request:Complete
            RockMigrationHelper.UpdateWorkflowActivityType( "51FE9641-FB8F-41BF-B09E-235900C3E53E", true, "Request", "Prompt the user for the information about their request", true, 0, "4A8FF789-887C-4BD7-8761-6E7D8D59BB5F" ); // IT Support:Request
            RockMigrationHelper.UpdateWorkflowActivityType( "51FE9641-FB8F-41BF-B09E-235900C3E53E", true, "Assign Worker", "Assigns the request to a worker", false, 1, "9AC84ADE-DAD2-4045-B27D-232B248C9457" ); // IT Support:Assign Worker
            RockMigrationHelper.UpdateWorkflowActivityType( "51FE9641-FB8F-41BF-B09E-235900C3E53E", true, "Open", "Activity used to process the request", false, 2, "70B8DEB8-D641-4E64-A327-79D96B8B6756" ); // IT Support:Open
            RockMigrationHelper.UpdateWorkflowActivityType( "51FE9641-FB8F-41BF-B09E-235900C3E53E", true, "Enter Resolution", "Prompts worker for resolution", false, 3, "60D58F02-1D58-40FF-AA40-1637CC8F8DC1" ); // IT Support:Enter Resolution
            RockMigrationHelper.UpdateWorkflowActivityType( "51FE9641-FB8F-41BF-B09E-235900C3E53E", true, "Complete", "Complete the workflow", false, 4, "CAB42A01-6430-42A1-B476-206B5930EA97" ); // IT Support:Complete
            RockMigrationHelper.UpdateWorkflowActivityTypeAttribute( "8226D12E-654A-41CF-B8B9-E4DFC6470402", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Selected Action", "SelectedAction", "The action that was selected by user", 0, @"", "AFE0AC86-88E3-4145-8E0C-421458681A13" ); // Facilities Request:Open:Selected Action
            RockMigrationHelper.UpdateWorkflowActivityTypeAttribute( "70B8DEB8-D641-4E64-A327-79D96B8B6756", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Selected Action", "SelectedAction", "The action that was selected by user", 0, @"", "45E50ED8-6922-41BF-B88F-1960DCEAAE99" ); // IT Support:Open:Selected Action
            RockMigrationHelper.UpdateWorkflowActionForm( @"<h4>Report Data Error</h4>
<p>Please enter as much information as possible about the data error you are reporting.</p>", @"<p>Once you have entered the details, click Submit to report the issue</p>", "Submit^fdc397cd-8b4a-436e-bea1-bce2e6717c03^31D6A610-9F0B-4136-8FEA-8F4E192D75B8^Your information has been submitted successfully.|", "", true, "", "CB8C9C9D-C81D-4B31-92CA-3BCEAE1DBA87" ); // Person Data Error:Entry:Enter Details
            RockMigrationHelper.UpdateWorkflowActionForm( @"<h4>Person Data Error</h4>
<p>A data error has been reported for the person below.</p>", @"<p>Once you have corrected the error, click Complete to close this issue and notify the person who reported the issue.  If you include any 
Resolution notes, those notes will be included in the notification.</p>", "Complete^fdc397cd-8b4a-436e-bea1-bce2e6717c03^E9F9E0BE-D38B-4017-954A-5B15C1EED2DE^Your information has been submitted successfully.|", "88C7D1CC-3478-4562-A301-AE7D4D7FFF6D", true, "", "FF998128-3917-4102-9F82-AD6B706DD173" ); // Person Data Error:Pending:Capture the Resolution
            RockMigrationHelper.UpdateWorkflowActionForm( @"<h2>How Can We Help?</h2>
<p>
Complete the form below and we'll forward your inquiry to the appropriate team member. Please allow 2-3 
business days for a response. {% if GlobalAttribute.OrganizationPhone != Empty %}If you need assistance right 
away, please call <strong>{{ GlobalAttribute.OrganizationPhone }}</strong> to speak with someone.{% endif %}
</p>
<br/>", @"", "Submit^fdc397cd-8b4a-436e-bea1-bce2e6717c03^^Thank You. Your request has been forwarded to a staff member who will be following up with you soon.|", "", true, "", "11D4769F-5B93-4605-8BCA-D21C14B0CEBA" ); // External Inquiry:Request:Prompt User
            RockMigrationHelper.UpdateWorkflowActionForm( @"<h4>{{ Workflow.Topic }} Inquiry from {{ Workflow.FirstName }} {{ Workflow.LastName }}</h4>
<p>The following inquiry has been submitted by a visitor to our website.</p>", @"", "Update^fdc397cd-8b4a-436e-bea1-bce2e6717c03^^The information you entered has been saved.|Complete^fdc397cd-8b4a-436e-bea1-bce2e6717c03^E9A017DF-EB72-4DC9-B2A4-5A424383C7E0^|", "88C7D1CC-3478-4562-A301-AE7D4D7FFF6D", true, "", "EB7034BA-6300-434F-832F-37983B9DF154" ); // External Inquiry:Open:Capture Notes
            RockMigrationHelper.UpdateWorkflowActionForm( @"<h2>Request</h2>
<p>
Complete the form below to request that the maintenance team address a facilities issue.  
</p>
<br/>", @"", "Submit^fdc397cd-8b4a-436e-bea1-bce2e6717c03^FCB5C4EC-E25B-45D2-88E1-51F525A073A4^Thank You. Your request has been forwarded to the maintenance team who will be following up with you soon.|", "", true, "", "445FD357-9948-421B-BB1C-1D1F589112CB" ); // Facilities Request:Request:Prompt User
            RockMigrationHelper.UpdateWorkflowActionForm( @"<h4>{{ Workflow.Title }}</h4>
", @"", "Save^fdc397cd-8b4a-436e-bea1-bce2e6717c03^^The information you entered has been saved.|Done^fdc397cd-8b4a-436e-bea1-bce2e6717c03^^|", "", true, "AFE0AC86-88E3-4145-8E0C-421458681A13", "EB869AA0-C81F-4BA0-9A76-A18A0A544BC6" ); // Facilities Request:Open:Capture Notes
            RockMigrationHelper.UpdateWorkflowActionForm( @"<h4>{{ Workflow.Title }}</h4>", @"<p>Select Submit to close this workflow and notify the requester.</p>", "Submit^fdc397cd-8b4a-436e-bea1-bce2e6717c03^739CEAE9-4FAB-4FE5-A824-6D2E569D637C^Your information has been submitted successfully.|Cancel^8cf6e927-4fa5-4241-991c-391038b79631^8226D12E-654A-41CF-B8B9-E4DFC6470402^|", "", true, "", "A08FC955-BB1A-4747-8FE3-88E3A2DB646E" ); // Facilities Request:Enter Resolution:Enter Resolution
            RockMigrationHelper.UpdateWorkflowActionForm( @"<h2>Request</h2>
<p>
Complete the form below to request support from the IT team. 
</p>
<br/>", @"", "Submit^fdc397cd-8b4a-436e-bea1-bce2e6717c03^9AC84ADE-DAD2-4045-B27D-232B248C9457^Thank You. Your request has been forwarded to the IT team who will be following up with you soon.|", "", true, "", "36D3F2F7-CDA9-4D56-BEF1-21DDAF4E89B3" ); // IT Support:Request:Prompt User
            RockMigrationHelper.UpdateWorkflowActionForm( @"<h4>{{ Workflow.Title }}</h4>
", @"", "Save^fdc397cd-8b4a-436e-bea1-bce2e6717c03^^The information you entered has been saved.|Done^fdc397cd-8b4a-436e-bea1-bce2e6717c03^^|", "", true, "45E50ED8-6922-41BF-B88F-1960DCEAAE99", "B3C205F8-C917-4F45-8F73-CDC8E64B6CF8" ); // IT Support:Open:Capture Notes
            RockMigrationHelper.UpdateWorkflowActionForm( @"<h4>{{ Workflow.Title }}</h4>", @"<p>Select Submit to close this workflow and notify the requester.</p>", "Submit^fdc397cd-8b4a-436e-bea1-bce2e6717c03^CAB42A01-6430-42A1-B476-206B5930EA97^Your information has been submitted successfully.|Cancel^8cf6e927-4fa5-4241-991c-391038b79631^70B8DEB8-D641-4E64-A327-79D96B8B6756^|", "", true, "", "6DE4BBEA-67A2-4B14-973F-6E699228620D" ); // IT Support:Enter Resolution:Enter Resolution
            RockMigrationHelper.UpdateWorkflowActionFormAttribute( "CB8C9C9D-C81D-4B31-92CA-3BCEAE1DBA87", "83F32DB1-8418-4748-BD52-A9E3FACA3F6E", 3, false, true, false, "61B25D4C-0283-4F4A-92FB-C351D52A5799" ); // Person Data Error:Entry:Enter Details:Resolution
            RockMigrationHelper.UpdateWorkflowActionFormAttribute( "CB8C9C9D-C81D-4B31-92CA-3BCEAE1DBA87", "FD89F2C8-CBC8-4ED1-96D1-891AB9616C9E", 0, true, false, false, "8E504295-A9CB-4722-824E-E92D676481F5" ); // Person Data Error:Entry:Enter Details:Person
            RockMigrationHelper.UpdateWorkflowActionFormAttribute( "CB8C9C9D-C81D-4B31-92CA-3BCEAE1DBA87", "C8FD97B5-ADC0-4EF0-B397-E4010850148C", 1, false, true, false, "14DB4521-BD33-4086-816E-EFE068FCE532" ); // Person Data Error:Entry:Enter Details:Reported By
            RockMigrationHelper.UpdateWorkflowActionFormAttribute( "CB8C9C9D-C81D-4B31-92CA-3BCEAE1DBA87", "094C9A07-F5CB-49DB-8B67-9B7616356D2C", 2, true, false, false, "9488AC15-FC4D-46AF-95D5-2C279682B089" ); // Person Data Error:Entry:Enter Details:Details ( Please be as specific as possible )
            RockMigrationHelper.UpdateWorkflowActionFormAttribute( "FF998128-3917-4102-9F82-AD6B706DD173", "FD89F2C8-CBC8-4ED1-96D1-891AB9616C9E", 0, true, true, false, "46ED5DAF-D4BD-42AB-BA07-236422D788F4" ); // Person Data Error:Pending:Capture the Resolution:Person
            RockMigrationHelper.UpdateWorkflowActionFormAttribute( "FF998128-3917-4102-9F82-AD6B706DD173", "C8FD97B5-ADC0-4EF0-B397-E4010850148C", 1, true, true, false, "7D7720D3-724A-494C-B6DB-2004C68471F4" ); // Person Data Error:Pending:Capture the Resolution:Reported By
            RockMigrationHelper.UpdateWorkflowActionFormAttribute( "FF998128-3917-4102-9F82-AD6B706DD173", "094C9A07-F5CB-49DB-8B67-9B7616356D2C", 2, true, true, false, "737DEAC8-AF3A-4CF2-A157-6881BA5B6611" ); // Person Data Error:Pending:Capture the Resolution:Details ( Please be as specific as possible )
            RockMigrationHelper.UpdateWorkflowActionFormAttribute( "FF998128-3917-4102-9F82-AD6B706DD173", "83F32DB1-8418-4748-BD52-A9E3FACA3F6E", 3, true, false, false, "72CA0BE8-79B0-4A7B-A383-8C8727B870F4" ); // Person Data Error:Pending:Capture the Resolution:Resolution
            RockMigrationHelper.UpdateWorkflowActionFormAttribute( "11D4769F-5B93-4605-8BCA-D21C14B0CEBA", "F5AE12D8-DE33-4C8A-A372-DE87134E3F75", 0, false, true, false, "3798DA79-91B2-48C9-8993-193A53F7AA3C" ); // External Inquiry:Request:Prompt User:Requester
            RockMigrationHelper.UpdateWorkflowActionFormAttribute( "11D4769F-5B93-4605-8BCA-D21C14B0CEBA", "5C3EDA60-0791-43ED-8E2B-E98276D543D1", 1, true, false, true, "BE55EB54-A1D0-40F3-92BD-7517D2ECFEF2" ); // External Inquiry:Request:Prompt User:First Name
            RockMigrationHelper.UpdateWorkflowActionFormAttribute( "11D4769F-5B93-4605-8BCA-D21C14B0CEBA", "2C35E7E1-5831-4C5F-AC50-97ED6EE7D5E0", 2, true, false, true, "B8714FA2-CB26-401F-AB0B-D39D080D0714" ); // External Inquiry:Request:Prompt User:Last Name
            RockMigrationHelper.UpdateWorkflowActionFormAttribute( "11D4769F-5B93-4605-8BCA-D21C14B0CEBA", "61FBEE01-A087-4060-B5D0-1262308BFAB4", 3, true, false, true, "1760BB92-DAF1-4102-8544-AC32128B585D" ); // External Inquiry:Request:Prompt User:Email Address
            RockMigrationHelper.UpdateWorkflowActionFormAttribute( "11D4769F-5B93-4605-8BCA-D21C14B0CEBA", "D4D3588B-0A13-432C-BAFB-9964EBB2B9F4", 4, true, false, false, "338D0021-C552-4A28-9DBA-9320BFCECB41" ); // External Inquiry:Request:Prompt User:Phone (optional)
            RockMigrationHelper.UpdateWorkflowActionFormAttribute( "11D4769F-5B93-4605-8BCA-D21C14B0CEBA", "DA61CA95-0106-49EE-962B-F70042E1464E", 5, true, false, true, "5CF07FA7-28FF-4811-9F12-3BF987B538B6" ); // External Inquiry:Request:Prompt User:Topic
            RockMigrationHelper.UpdateWorkflowActionFormAttribute( "11D4769F-5B93-4605-8BCA-D21C14B0CEBA", "B7BE8727-31A2-443D-AF78-A9B3A2712143", 6, true, false, true, "CA9993A7-80FF-4024-94FB-BCE2BBE2CC81" ); // External Inquiry:Request:Prompt User:Inquiry
            RockMigrationHelper.UpdateWorkflowActionFormAttribute( "11D4769F-5B93-4605-8BCA-D21C14B0CEBA", "754A0725-29FC-407B-87E9-95D697F49FFE", 7, true, false, false, "D0AD5A1B-0C35-4D40-85B9-437955F98D2A" ); // External Inquiry:Request:Prompt User:Campus
            RockMigrationHelper.UpdateWorkflowActionFormAttribute( "11D4769F-5B93-4605-8BCA-D21C14B0CEBA", "04712FBD-715D-412E-96C3-10C748482D6E", 8, false, true, false, "FFA7FA9F-B813-4A2E-A412-DBA1503F797A" ); // External Inquiry:Request:Prompt User:Worker
            RockMigrationHelper.UpdateWorkflowActionFormAttribute( "11D4769F-5B93-4605-8BCA-D21C14B0CEBA", "AB12CA58-0284-4D05-BF5F-4003C04E43B4", 9, false, true, false, "914940F9-D49B-423A-B577-C9A9A97F5A69" ); // External Inquiry:Request:Prompt User:Notes
            RockMigrationHelper.UpdateWorkflowActionFormAttribute( "11D4769F-5B93-4605-8BCA-D21C14B0CEBA", "56A33DB7-FAC1-41F9-B42D-CC24D7A20466", 10, false, true, false, "E4F19463-6310-4AEB-BB9A-241F3A6D1873" ); // External Inquiry:Request:Prompt User:Assign New Worker
            RockMigrationHelper.UpdateWorkflowActionFormAttribute( "EB7034BA-6300-434F-832F-37983B9DF154", "F5AE12D8-DE33-4C8A-A372-DE87134E3F75", 0, true, true, false, "BFF5714A-4F16-4E32-A4C3-9061CCCA102E" ); // External Inquiry:Open:Capture Notes:Requester
            RockMigrationHelper.UpdateWorkflowActionFormAttribute( "EB7034BA-6300-434F-832F-37983B9DF154", "5C3EDA60-0791-43ED-8E2B-E98276D543D1", 1, false, true, false, "F2A1D77A-4974-4D50-8960-4BB7EFFA788F" ); // External Inquiry:Open:Capture Notes:First Name
            RockMigrationHelper.UpdateWorkflowActionFormAttribute( "EB7034BA-6300-434F-832F-37983B9DF154", "2C35E7E1-5831-4C5F-AC50-97ED6EE7D5E0", 2, false, true, false, "007945DC-1625-443B-9840-681A262C1916" ); // External Inquiry:Open:Capture Notes:Last Name
            RockMigrationHelper.UpdateWorkflowActionFormAttribute( "EB7034BA-6300-434F-832F-37983B9DF154", "61FBEE01-A087-4060-B5D0-1262308BFAB4", 3, true, true, false, "3427114A-5EFA-41B9-801B-581790D8154A" ); // External Inquiry:Open:Capture Notes:Email Address
            RockMigrationHelper.UpdateWorkflowActionFormAttribute( "EB7034BA-6300-434F-832F-37983B9DF154", "D4D3588B-0A13-432C-BAFB-9964EBB2B9F4", 4, true, true, false, "F3C426C1-9EC4-47FE-8D02-E94A639D4D07" ); // External Inquiry:Open:Capture Notes:Phone (optional)
            RockMigrationHelper.UpdateWorkflowActionFormAttribute( "EB7034BA-6300-434F-832F-37983B9DF154", "DA61CA95-0106-49EE-962B-F70042E1464E", 5, false, true, false, "0C052664-F5C7-4F45-ABDC-4C898E0122B3" ); // External Inquiry:Open:Capture Notes:Topic
            RockMigrationHelper.UpdateWorkflowActionFormAttribute( "EB7034BA-6300-434F-832F-37983B9DF154", "B7BE8727-31A2-443D-AF78-A9B3A2712143", 6, true, true, false, "6A6C1E41-4489-40C4-9B99-172E50DC42D1" ); // External Inquiry:Open:Capture Notes:Inquiry
            RockMigrationHelper.UpdateWorkflowActionFormAttribute( "EB7034BA-6300-434F-832F-37983B9DF154", "754A0725-29FC-407B-87E9-95D697F49FFE", 7, true, true, false, "5A11C889-FA0C-41A1-B7CA-DA2F095D0A50" ); // External Inquiry:Open:Capture Notes:Campus
            RockMigrationHelper.UpdateWorkflowActionFormAttribute( "EB7034BA-6300-434F-832F-37983B9DF154", "04712FBD-715D-412E-96C3-10C748482D6E", 8, false, true, false, "7786B5DE-6731-461B-B59F-9E09A326D625" ); // External Inquiry:Open:Capture Notes:Worker
            RockMigrationHelper.UpdateWorkflowActionFormAttribute( "EB7034BA-6300-434F-832F-37983B9DF154", "56A33DB7-FAC1-41F9-B42D-CC24D7A20466", 9, true, false, false, "5ECD4001-C01C-4AA2-9A5B-0B20475818FC" ); // External Inquiry:Open:Capture Notes:Assign New Worker
            RockMigrationHelper.UpdateWorkflowActionFormAttribute( "EB7034BA-6300-434F-832F-37983B9DF154", "AB12CA58-0284-4D05-BF5F-4003C04E43B4", 10, true, false, false, "F81BB9B3-8223-4120-ABA1-E628E111048D" ); // External Inquiry:Open:Capture Notes:Notes
            RockMigrationHelper.UpdateWorkflowActionFormAttribute( "445FD357-9948-421B-BB1C-1D1F589112CB", "8767527F-C1DC-42E2-B24D-6061C74830EF", 0, true, false, true, "AC72FFA3-9295-48DB-94ED-6D3060E5BAFB" ); // Facilities Request:Request:Prompt User:Title
            RockMigrationHelper.UpdateWorkflowActionFormAttribute( "445FD357-9948-421B-BB1C-1D1F589112CB", "0DCDB750-4789-4A5F-B673-4954E94A81B4", 1, true, false, true, "89CE7031-9B33-4470-BF66-714A2A59C512" ); // Facilities Request:Request:Prompt User:Details
            RockMigrationHelper.UpdateWorkflowActionFormAttribute( "445FD357-9948-421B-BB1C-1D1F589112CB", "12EB0CCE-8012-498D-B95E-EB41090F8889", 2, true, false, false, "E74FBB2B-A470-46D2-86DE-ACFA554E5D9F" ); // Facilities Request:Request:Prompt User:Campus
            RockMigrationHelper.UpdateWorkflowActionFormAttribute( "445FD357-9948-421B-BB1C-1D1F589112CB", "594D522B-1E69-4175-9C6B-039C6BAF279E", 3, false, true, false, "CE2768CA-41B1-483B-8343-61199B6C7FDE" ); // Facilities Request:Request:Prompt User:Requester
            RockMigrationHelper.UpdateWorkflowActionFormAttribute( "445FD357-9948-421B-BB1C-1D1F589112CB", "E0D71111-05F1-4CD3-959E-1D246613942E", 4, false, true, false, "4C7C01A5-9C2A-481D-8EAF-DCD77F862810" ); // Facilities Request:Request:Prompt User:Worker
            RockMigrationHelper.UpdateWorkflowActionFormAttribute( "445FD357-9948-421B-BB1C-1D1F589112CB", "2CB0B49F-453D-4D41-9431-3BE18C2CC842", 5, false, true, false, "46E28713-FECD-426F-8377-466F2F1CCF1D" ); // Facilities Request:Request:Prompt User:Resolution
            RockMigrationHelper.UpdateWorkflowActionFormAttribute( "445FD357-9948-421B-BB1C-1D1F589112CB", "1D313E0A-26D7-4A7A-94F1-45E733569C88", 6, false, true, false, "6F9E7964-718E-4134-ACAE-53D85954B797" ); // Facilities Request:Request:Prompt User:Waiting on Parts
            RockMigrationHelper.UpdateWorkflowActionFormAttribute( "445FD357-9948-421B-BB1C-1D1F589112CB", "8F9E8196-087F-49D6-9879-AA139C7A1225", 7, false, true, false, "FFC35439-79C7-4247-9CC6-51AAE3A46877" ); // Facilities Request:Request:Prompt User:New Worker
            RockMigrationHelper.UpdateWorkflowActionFormAttribute( "445FD357-9948-421B-BB1C-1D1F589112CB", "06912CB0-24A7-45AE-892B-F95A85D76DB2", 8, false, true, false, "D2CEF654-49AD-48D8-9984-04BD33EE24D5" ); // Facilities Request:Request:Prompt User:Internal Notes
            RockMigrationHelper.UpdateWorkflowActionFormAttribute( "EB869AA0-C81F-4BA0-9A76-A18A0A544BC6", "594D522B-1E69-4175-9C6B-039C6BAF279E", 0, true, true, false, "FEA4ADA6-47AF-4053-AF08-D1F862E54AE4" ); // Facilities Request:Open:Capture Notes:Requester
            RockMigrationHelper.UpdateWorkflowActionFormAttribute( "EB869AA0-C81F-4BA0-9A76-A18A0A544BC6", "12EB0CCE-8012-498D-B95E-EB41090F8889", 1, true, true, false, "CEEC3C2A-3799-4A12-B9FA-5CE4D794FC52" ); // Facilities Request:Open:Capture Notes:Campus
            RockMigrationHelper.UpdateWorkflowActionFormAttribute( "EB869AA0-C81F-4BA0-9A76-A18A0A544BC6", "8767527F-C1DC-42E2-B24D-6061C74830EF", 2, false, true, false, "3A5F10DF-17DD-4DC4-83B0-4C478CB05E24" ); // Facilities Request:Open:Capture Notes:Title
            RockMigrationHelper.UpdateWorkflowActionFormAttribute( "EB869AA0-C81F-4BA0-9A76-A18A0A544BC6", "0DCDB750-4789-4A5F-B673-4954E94A81B4", 3, true, true, false, "D0904F6B-BE32-4E99-81CB-2C6C25DB4013" ); // Facilities Request:Open:Capture Notes:Details
            RockMigrationHelper.UpdateWorkflowActionFormAttribute( "EB869AA0-C81F-4BA0-9A76-A18A0A544BC6", "E0D71111-05F1-4CD3-959E-1D246613942E", 4, false, true, false, "C9CBB3B0-61A4-49E2-8321-8FEDF248EBBE" ); // Facilities Request:Open:Capture Notes:Worker
            RockMigrationHelper.UpdateWorkflowActionFormAttribute( "EB869AA0-C81F-4BA0-9A76-A18A0A544BC6", "8F9E8196-087F-49D6-9879-AA139C7A1225", 5, true, false, false, "A6B8B766-56CF-44A0-B2E9-325F225F8E9F" ); // Facilities Request:Open:Capture Notes:New Worker
            RockMigrationHelper.UpdateWorkflowActionFormAttribute( "EB869AA0-C81F-4BA0-9A76-A18A0A544BC6", "06912CB0-24A7-45AE-892B-F95A85D76DB2", 6, true, false, false, "7814FA64-E355-4A01-AB07-63B5559D6B6F" ); // Facilities Request:Open:Capture Notes:Internal Notes
            RockMigrationHelper.UpdateWorkflowActionFormAttribute( "EB869AA0-C81F-4BA0-9A76-A18A0A544BC6", "1D313E0A-26D7-4A7A-94F1-45E733569C88", 7, true, false, false, "7DDE5004-DF5F-4EC5-B1E2-84A0EFB44211" ); // Facilities Request:Open:Capture Notes:Waiting on Parts
            RockMigrationHelper.UpdateWorkflowActionFormAttribute( "EB869AA0-C81F-4BA0-9A76-A18A0A544BC6", "2CB0B49F-453D-4D41-9431-3BE18C2CC842", 8, false, true, false, "6AED38A6-95F6-43A4-84D5-467238739853" ); // Facilities Request:Open:Capture Notes:Resolution
            RockMigrationHelper.UpdateWorkflowActionFormAttribute( "EB869AA0-C81F-4BA0-9A76-A18A0A544BC6", "AFE0AC86-88E3-4145-8E0C-421458681A13", 9, false, true, false, "83C67953-7A3D-465A-B8BD-23DFA3376CE6" ); // Facilities Request:Open:Capture Notes:Selected Action
            RockMigrationHelper.UpdateWorkflowActionFormAttribute( "A08FC955-BB1A-4747-8FE3-88E3A2DB646E", "12EB0CCE-8012-498D-B95E-EB41090F8889", 0, false, true, false, "E9476111-F815-4143-98DA-7EA91A850616" ); // Facilities Request:Enter Resolution:Enter Resolution:Campus
            RockMigrationHelper.UpdateWorkflowActionFormAttribute( "A08FC955-BB1A-4747-8FE3-88E3A2DB646E", "8767527F-C1DC-42E2-B24D-6061C74830EF", 1, false, true, false, "EEB236F2-C971-4445-AED5-BF3491D62E6F" ); // Facilities Request:Enter Resolution:Enter Resolution:Title
            RockMigrationHelper.UpdateWorkflowActionFormAttribute( "A08FC955-BB1A-4747-8FE3-88E3A2DB646E", "0DCDB750-4789-4A5F-B673-4954E94A81B4", 2, true, true, false, "794B0553-845A-407B-8B86-B4ECDD122B8C" ); // Facilities Request:Enter Resolution:Enter Resolution:Details
            RockMigrationHelper.UpdateWorkflowActionFormAttribute( "A08FC955-BB1A-4747-8FE3-88E3A2DB646E", "594D522B-1E69-4175-9C6B-039C6BAF279E", 3, true, true, false, "09F6C4DF-72E5-487A-BAAD-0E657F821F6E" ); // Facilities Request:Enter Resolution:Enter Resolution:Requester
            RockMigrationHelper.UpdateWorkflowActionFormAttribute( "A08FC955-BB1A-4747-8FE3-88E3A2DB646E", "E0D71111-05F1-4CD3-959E-1D246613942E", 4, false, true, false, "26A12876-5C22-4579-810B-B2DD37188EE3" ); // Facilities Request:Enter Resolution:Enter Resolution:Worker
            RockMigrationHelper.UpdateWorkflowActionFormAttribute( "A08FC955-BB1A-4747-8FE3-88E3A2DB646E", "8F9E8196-087F-49D6-9879-AA139C7A1225", 5, false, true, false, "11102D6E-1711-44A5-BCC2-CFE1A7FD3C46" ); // Facilities Request:Enter Resolution:Enter Resolution:New Worker
            RockMigrationHelper.UpdateWorkflowActionFormAttribute( "A08FC955-BB1A-4747-8FE3-88E3A2DB646E", "06912CB0-24A7-45AE-892B-F95A85D76DB2", 6, false, true, false, "31D3F29B-216F-4514-80FF-5A10B2F55C1D" ); // Facilities Request:Enter Resolution:Enter Resolution:Internal Notes
            RockMigrationHelper.UpdateWorkflowActionFormAttribute( "A08FC955-BB1A-4747-8FE3-88E3A2DB646E", "1D313E0A-26D7-4A7A-94F1-45E733569C88", 7, false, true, false, "FE8814ED-9510-4086-8888-02453B80A614" ); // Facilities Request:Enter Resolution:Enter Resolution:Waiting on Parts
            RockMigrationHelper.UpdateWorkflowActionFormAttribute( "A08FC955-BB1A-4747-8FE3-88E3A2DB646E", "2CB0B49F-453D-4D41-9431-3BE18C2CC842", 8, true, false, false, "DC06CF91-6789-493B-9220-95743ED97972" ); // Facilities Request:Enter Resolution:Enter Resolution:Resolution
            RockMigrationHelper.UpdateWorkflowActionFormAttribute( "36D3F2F7-CDA9-4D56-BEF1-21DDAF4E89B3", "59F1FB67-F2B3-441E-ACE3-9B10AB7E674F", 0, true, false, true, "83C6E908-5AC4-4C24-932F-97988B40AE77" ); // IT Support:Request:Prompt User:Title
            RockMigrationHelper.UpdateWorkflowActionFormAttribute( "36D3F2F7-CDA9-4D56-BEF1-21DDAF4E89B3", "9C2E0B04-8319-4967-9E0A-DB062398566C", 1, true, false, true, "F431CC63-05EB-4348-9B1C-C43A639D755C" ); // IT Support:Request:Prompt User:Details
            RockMigrationHelper.UpdateWorkflowActionFormAttribute( "36D3F2F7-CDA9-4D56-BEF1-21DDAF4E89B3", "9DEC1697-1ED6-4CC0-B054-F9E0D975DF12", 2, true, false, false, "130DA728-72EC-4210-A283-5B1C7677507E" ); // IT Support:Request:Prompt User:Campus
            RockMigrationHelper.UpdateWorkflowActionFormAttribute( "36D3F2F7-CDA9-4D56-BEF1-21DDAF4E89B3", "CE01E29E-4A9C-4BBF-BBCD-235713A81353", 3, false, true, false, "07350189-26B0-47EA-8AA6-AA8A9E253541" ); // IT Support:Request:Prompt User:Requester
            RockMigrationHelper.UpdateWorkflowActionFormAttribute( "36D3F2F7-CDA9-4D56-BEF1-21DDAF4E89B3", "9D165825-21F2-4B5F-9A64-790A6EBD51AC", 4, false, true, false, "2F2E3171-F21B-45A3-9B4D-71734D722E7F" ); // IT Support:Request:Prompt User:Worker
            RockMigrationHelper.UpdateWorkflowActionFormAttribute( "36D3F2F7-CDA9-4D56-BEF1-21DDAF4E89B3", "96F7BABE-49AA-46C5-8016-5E7246C97A48", 5, false, true, false, "11951BCE-BBC6-4A8E-8FDF-A9C97E346175" ); // IT Support:Request:Prompt User:Resolution
            RockMigrationHelper.UpdateWorkflowActionFormAttribute( "36D3F2F7-CDA9-4D56-BEF1-21DDAF4E89B3", "90E60C82-CD15-4F21-AEBE-6E00EE340685", 6, false, true, false, "59C2DE4E-F24D-4B46-9F87-11825E74AA5E" ); // IT Support:Request:Prompt User:Waiting on Parts
            RockMigrationHelper.UpdateWorkflowActionFormAttribute( "36D3F2F7-CDA9-4D56-BEF1-21DDAF4E89B3", "91D7D16C-F6A1-4B85-A7EE-A0D20B5A2BC5", 7, false, true, false, "F9DCB1FC-E3CB-4A01-A782-9BF0E7F36895" ); // IT Support:Request:Prompt User:New Worker
            RockMigrationHelper.UpdateWorkflowActionFormAttribute( "36D3F2F7-CDA9-4D56-BEF1-21DDAF4E89B3", "E616FDCF-A7F7-4EEA-B4E8-35320C24C726", 8, false, true, false, "615FB8E8-7DD4-41E4-8F95-27ED5E052422" ); // IT Support:Request:Prompt User:Internal Notes
            RockMigrationHelper.UpdateWorkflowActionFormAttribute( "B3C205F8-C917-4F45-8F73-CDC8E64B6CF8", "CE01E29E-4A9C-4BBF-BBCD-235713A81353", 0, true, true, false, "C23C3EF0-EB48-4272-8F89-CA3472E7B730" ); // IT Support:Open:Capture Notes:Requester
            RockMigrationHelper.UpdateWorkflowActionFormAttribute( "B3C205F8-C917-4F45-8F73-CDC8E64B6CF8", "9DEC1697-1ED6-4CC0-B054-F9E0D975DF12", 1, true, true, false, "FE3A41F8-C304-4983-B78F-DF6AB25511C6" ); // IT Support:Open:Capture Notes:Campus
            RockMigrationHelper.UpdateWorkflowActionFormAttribute( "B3C205F8-C917-4F45-8F73-CDC8E64B6CF8", "59F1FB67-F2B3-441E-ACE3-9B10AB7E674F", 2, false, true, false, "EAEEF9F0-577F-49FA-B23B-F2A6F9E7FBAF" ); // IT Support:Open:Capture Notes:Title
            RockMigrationHelper.UpdateWorkflowActionFormAttribute( "B3C205F8-C917-4F45-8F73-CDC8E64B6CF8", "9C2E0B04-8319-4967-9E0A-DB062398566C", 3, true, true, false, "6BCD0AE7-D9CE-4DFB-9BFD-AB8B3D53BA5F" ); // IT Support:Open:Capture Notes:Details
            RockMigrationHelper.UpdateWorkflowActionFormAttribute( "B3C205F8-C917-4F45-8F73-CDC8E64B6CF8", "9D165825-21F2-4B5F-9A64-790A6EBD51AC", 4, false, true, false, "11243F68-2720-40A1-A846-377A569D6C1C" ); // IT Support:Open:Capture Notes:Worker
            RockMigrationHelper.UpdateWorkflowActionFormAttribute( "B3C205F8-C917-4F45-8F73-CDC8E64B6CF8", "91D7D16C-F6A1-4B85-A7EE-A0D20B5A2BC5", 5, true, false, false, "77726CE9-994D-4F94-9905-5E383C1F95AE" ); // IT Support:Open:Capture Notes:New Worker
            RockMigrationHelper.UpdateWorkflowActionFormAttribute( "B3C205F8-C917-4F45-8F73-CDC8E64B6CF8", "E616FDCF-A7F7-4EEA-B4E8-35320C24C726", 6, true, false, false, "88B66A7E-39F3-48F3-ACDF-BE85B265CCEE" ); // IT Support:Open:Capture Notes:Internal Notes
            RockMigrationHelper.UpdateWorkflowActionFormAttribute( "B3C205F8-C917-4F45-8F73-CDC8E64B6CF8", "90E60C82-CD15-4F21-AEBE-6E00EE340685", 7, true, false, false, "372B917D-2571-4D71-B646-53359F055D10" ); // IT Support:Open:Capture Notes:Waiting on Parts
            RockMigrationHelper.UpdateWorkflowActionFormAttribute( "B3C205F8-C917-4F45-8F73-CDC8E64B6CF8", "96F7BABE-49AA-46C5-8016-5E7246C97A48", 8, false, true, false, "E1AA83D6-78DC-4D2E-BF1C-3A8D1CCAF17D" ); // IT Support:Open:Capture Notes:Resolution
            RockMigrationHelper.UpdateWorkflowActionFormAttribute( "B3C205F8-C917-4F45-8F73-CDC8E64B6CF8", "45E50ED8-6922-41BF-B88F-1960DCEAAE99", 9, false, true, false, "7ED3E760-D93B-4257-AFBB-FEC65A7CED5D" ); // IT Support:Open:Capture Notes:Selected Action
            RockMigrationHelper.UpdateWorkflowActionFormAttribute( "6DE4BBEA-67A2-4B14-973F-6E699228620D", "9DEC1697-1ED6-4CC0-B054-F9E0D975DF12", 0, false, true, false, "68BB38E0-D419-4027-A41B-535BA96E1D68" ); // IT Support:Enter Resolution:Enter Resolution:Campus
            RockMigrationHelper.UpdateWorkflowActionFormAttribute( "6DE4BBEA-67A2-4B14-973F-6E699228620D", "59F1FB67-F2B3-441E-ACE3-9B10AB7E674F", 1, false, true, false, "8D3E2B04-2D7B-485B-9EF2-AF3FC8FD8544" ); // IT Support:Enter Resolution:Enter Resolution:Title
            RockMigrationHelper.UpdateWorkflowActionFormAttribute( "6DE4BBEA-67A2-4B14-973F-6E699228620D", "9C2E0B04-8319-4967-9E0A-DB062398566C", 2, true, true, false, "485E5E9B-41D5-4C59-A580-2CDB3F0CF5A9" ); // IT Support:Enter Resolution:Enter Resolution:Details
            RockMigrationHelper.UpdateWorkflowActionFormAttribute( "6DE4BBEA-67A2-4B14-973F-6E699228620D", "CE01E29E-4A9C-4BBF-BBCD-235713A81353", 3, true, true, false, "F20C8C15-D494-44B9-9788-67D7B60D5438" ); // IT Support:Enter Resolution:Enter Resolution:Requester
            RockMigrationHelper.UpdateWorkflowActionFormAttribute( "6DE4BBEA-67A2-4B14-973F-6E699228620D", "9D165825-21F2-4B5F-9A64-790A6EBD51AC", 4, false, true, false, "64CE2E31-1295-4E28-8CB8-7A736B0C96CE" ); // IT Support:Enter Resolution:Enter Resolution:Worker
            RockMigrationHelper.UpdateWorkflowActionFormAttribute( "6DE4BBEA-67A2-4B14-973F-6E699228620D", "91D7D16C-F6A1-4B85-A7EE-A0D20B5A2BC5", 5, false, true, false, "F54BF849-9C60-461B-A242-8DF6CEC19578" ); // IT Support:Enter Resolution:Enter Resolution:New Worker
            RockMigrationHelper.UpdateWorkflowActionFormAttribute( "6DE4BBEA-67A2-4B14-973F-6E699228620D", "E616FDCF-A7F7-4EEA-B4E8-35320C24C726", 6, false, true, false, "C52A6CC3-FDDF-47DB-8A1E-2DF8CF64809B" ); // IT Support:Enter Resolution:Enter Resolution:Internal Notes
            RockMigrationHelper.UpdateWorkflowActionFormAttribute( "6DE4BBEA-67A2-4B14-973F-6E699228620D", "90E60C82-CD15-4F21-AEBE-6E00EE340685", 7, false, true, false, "ED1BC08A-C8CE-4F27-B516-2594BF9F8E96" ); // IT Support:Enter Resolution:Enter Resolution:Waiting on Parts
            RockMigrationHelper.UpdateWorkflowActionFormAttribute( "6DE4BBEA-67A2-4B14-973F-6E699228620D", "96F7BABE-49AA-46C5-8016-5E7246C97A48", 8, true, false, false, "7004FB2B-7468-441C-BF2C-43B95F9135FB" ); // IT Support:Enter Resolution:Enter Resolution:Resolution
            RockMigrationHelper.UpdateWorkflowActionType( "F238E2D8-AAE0-481F-BABF-955C0A3343CC", "Set the Reported By value", 0, "24B7D5E6-C30F-48F4-9D7E-AF45A342CF3A", true, false, "", "", 1, "", "D56341AE-7A81-445F-B2CC-21C84A767D6C" ); // Person Data Error:Entry:Set the Reported By value
            RockMigrationHelper.UpdateWorkflowActionType( "F238E2D8-AAE0-481F-BABF-955C0A3343CC", "Enter Details", 1, "486DC4FA-FCBC-425F-90B0-E606DA8A9F68", true, false, "CB8C9C9D-C81D-4B31-92CA-3BCEAE1DBA87", "", 1, "", "341569AB-76F6-44E8-A033-CE29FDE8035F" ); // Person Data Error:Entry:Enter Details
            RockMigrationHelper.UpdateWorkflowActionType( "F238E2D8-AAE0-481F-BABF-955C0A3343CC", "Persist the Workflow", 2, "F1A39347-6FE0-43D4-89FB-544195088ECF", true, false, "", "", 1, "", "5169CBED-08DF-4ABD-91AB-83A2637C5515" ); // Person Data Error:Entry:Persist the Workflow
            RockMigrationHelper.UpdateWorkflowActionType( "31D6A610-9F0B-4136-8FEA-8F4E192D75B8", "Assign Worker", 1, "FB2981B7-7922-42E1-8ACF-7F63BB7989E6", true, false, "", "", 1, "", "F6311295-5E9E-4521-A92D-71DAFCCDD4B6" ); // Person Data Error:Pending:Assign Worker
            RockMigrationHelper.UpdateWorkflowActionType( "31D6A610-9F0B-4136-8FEA-8F4E192D75B8", "Capture the Resolution", 2, "486DC4FA-FCBC-425F-90B0-E606DA8A9F68", true, false, "FF998128-3917-4102-9F82-AD6B706DD173", "", 1, "", "6AF4C1E2-461A-432B-9184-068D08DF8F49" ); // Person Data Error:Pending:Capture the Resolution
            RockMigrationHelper.UpdateWorkflowActionType( "31D6A610-9F0B-4136-8FEA-8F4E192D75B8", "Rename the Workflow", 0, "36005473-BD5D-470B-B28D-98E6D7ED808D", true, false, "", "", 1, "", "D8E9AF9C-1280-4ABB-8F96-7BF62F516A5B" ); // Person Data Error:Pending:Rename the Workflow
            RockMigrationHelper.UpdateWorkflowActionType( "E9F9E0BE-D38B-4017-954A-5B15C1EED2DE", "Notify Originator", 0, "66197B01-D1F0-4924-A315-47AD54E030DE", true, false, "", "", 1, "", "84C85F3B-E913-4302-B61E-BE04EB668133" ); // Person Data Error:Complete:Notify Originator
            RockMigrationHelper.UpdateWorkflowActionType( "E9F9E0BE-D38B-4017-954A-5B15C1EED2DE", "Complete the workflow", 1, "EEDA4318-F014-4A46-9C76-4C052EF81AA1", true, false, "", "", 1, "", "0D1D7EB5-EADF-4FC1-87AA-7793202E21F6" ); // Person Data Error:Complete:Complete the workflow
            RockMigrationHelper.UpdateWorkflowActionType( "6AA06B90-5BC1-464B-A660-C481853C1B32", "Set Person", 0, "972F19B9-598B-474B-97A4-50E56E7B59D2", true, false, "", "", 1, "", "87558E26-399D-4EBB-A860-932F8792738C" ); // Person Data Error:Launch From Person Profile:Set Person
            RockMigrationHelper.UpdateWorkflowActionType( "6AA06B90-5BC1-464B-A660-C481853C1B32", "Persist Workflow", 1, "F1A39347-6FE0-43D4-89FB-544195088ECF", true, false, "", "", 1, "", "7E8160EC-1AB1-4D69-8FB3-F7AA08904C15" ); // Person Data Error:Launch From Person Profile:Persist Workflow
            RockMigrationHelper.UpdateWorkflowActionType( "D8C39D36-DB70-4200-98C3-A040FA97B53D", "Prompt User", 0, "486DC4FA-FCBC-425F-90B0-E606DA8A9F68", true, false, "11D4769F-5B93-4605-8BCA-D21C14B0CEBA", "", 1, "", "AEACAE26-20D8-4F24-90FF-3E15475CA8F9" ); // External Inquiry:Request:Prompt User
            RockMigrationHelper.UpdateWorkflowActionType( "D8C39D36-DB70-4200-98C3-A040FA97B53D", "Set Requester", 2, "24B7D5E6-C30F-48F4-9D7E-AF45A342CF3A", true, false, "", "", 1, "", "465B9C7B-BE88-43D1-A5EC-C780E373793D" ); // External Inquiry:Request:Set Requester
            RockMigrationHelper.UpdateWorkflowActionType( "D8C39D36-DB70-4200-98C3-A040FA97B53D", "Persist the Workflow", 3, "F1A39347-6FE0-43D4-89FB-544195088ECF", true, false, "", "", 1, "", "4E59F578-5C2D-4446-A98A-6C908CACFB2B" ); // External Inquiry:Request:Persist the Workflow
            RockMigrationHelper.UpdateWorkflowActionType( "D8C39D36-DB70-4200-98C3-A040FA97B53D", "Test for General Inquiry", 4, "38907A90-1634-4A93-8017-619326A4A582", true, true, "", "DA61CA95-0106-49EE-962B-F70042E1464E", 1, "General", "39E8D405-49B6-4A8F-A100-26538EA21362" ); // External Inquiry:Request:Test for General Inquiry
            RockMigrationHelper.UpdateWorkflowActionType( "D8C39D36-DB70-4200-98C3-A040FA97B53D", "Test for Login Inquiry", 5, "38907A90-1634-4A93-8017-619326A4A582", true, true, "", "DA61CA95-0106-49EE-962B-F70042E1464E", 1, "Login", "73A89BFA-6D25-4933-898A-383D3A9E6BED" ); // External Inquiry:Request:Test for Login Inquiry
            RockMigrationHelper.UpdateWorkflowActionType( "D8C39D36-DB70-4200-98C3-A040FA97B53D", "Test for Website Inquiry", 6, "38907A90-1634-4A93-8017-619326A4A582", true, true, "", "DA61CA95-0106-49EE-962B-F70042E1464E", 1, "Website", "B27137A5-3A9D-4C1B-8D60-8A1A159FE74F" ); // External Inquiry:Request:Test for Website Inquiry
            RockMigrationHelper.UpdateWorkflowActionType( "D8C39D36-DB70-4200-98C3-A040FA97B53D", "Test for Finance Inquiry", 7, "38907A90-1634-4A93-8017-619326A4A582", true, true, "", "DA61CA95-0106-49EE-962B-F70042E1464E", 1, "Finance", "5A1A8A4B-D480-44B3-9C92-5C57C3A1798D" ); // External Inquiry:Request:Test for Finance Inquiry
            RockMigrationHelper.UpdateWorkflowActionType( "D8C39D36-DB70-4200-98C3-A040FA97B53D", "Test for Missions Inquiry", 8, "38907A90-1634-4A93-8017-619326A4A582", true, true, "", "DA61CA95-0106-49EE-962B-F70042E1464E", 1, "Missions", "1F193146-D379-46B7-AEDF-8BBA90E8238B" ); // External Inquiry:Request:Test for Missions Inquiry
            RockMigrationHelper.UpdateWorkflowActionType( "D8C39D36-DB70-4200-98C3-A040FA97B53D", "Test for Pastor Inquiry", 9, "38907A90-1634-4A93-8017-619326A4A582", true, true, "", "DA61CA95-0106-49EE-962B-F70042E1464E", 1, "Pastor", "636F5D27-DAAE-462D-A0ED-666C70EC5F35" ); // External Inquiry:Request:Test for Pastor Inquiry
            RockMigrationHelper.UpdateWorkflowActionType( "D8C39D36-DB70-4200-98C3-A040FA97B53D", "Set Name", 1, "36005473-BD5D-470B-B28D-98E6D7ED808D", true, false, "", "", 1, "", "3DD65F61-6F18-4794-9C05-8FEF69A0DC31" ); // External Inquiry:Request:Set Name
            RockMigrationHelper.UpdateWorkflowActionType( "B89148C7-EF85-450D-81CB-F0BF2102E0CB", "Assign Worker", 0, "17962C23-2E94-4E06-8461-0FB8B94E2FEA", true, false, "", "", 1, "", "4642162E-B893-43B4-9D52-F8221206BAEB" ); // External Inquiry:General Inquiry:Assign Worker
            RockMigrationHelper.UpdateWorkflowActionType( "B89148C7-EF85-450D-81CB-F0BF2102E0CB", "Open", 1, "38907A90-1634-4A93-8017-619326A4A582", true, false, "", "", 1, "", "F46A76F8-BDB4-434F-8A31-88810F8CE2ED" ); // External Inquiry:General Inquiry:Open
            RockMigrationHelper.UpdateWorkflowActionType( "D73B1B06-30F6-4504-B95F-2322AFE96E55", "Open", 1, "38907A90-1634-4A93-8017-619326A4A582", true, false, "", "", 1, "", "49C04C16-D80C-4371-90FD-E065581F5780" ); // External Inquiry:Login Inquiry:Open
            RockMigrationHelper.UpdateWorkflowActionType( "D73B1B06-30F6-4504-B95F-2322AFE96E55", "Assign Worker", 0, "17962C23-2E94-4E06-8461-0FB8B94E2FEA", true, false, "", "", 1, "", "1BEA6BB6-C4C2-4254-B3E6-744BB4249620" ); // External Inquiry:Login Inquiry:Assign Worker
            RockMigrationHelper.UpdateWorkflowActionType( "ACE58142-04DC-4B87-BC78-F6C9E00C68A9", "Assign Worker", 0, "17962C23-2E94-4E06-8461-0FB8B94E2FEA", true, false, "", "", 1, "", "74733C1D-6939-41BC-86DD-CCE5B345CC7C" ); // External Inquiry:Website Inquiry:Assign Worker
            RockMigrationHelper.UpdateWorkflowActionType( "ACE58142-04DC-4B87-BC78-F6C9E00C68A9", "Open", 1, "38907A90-1634-4A93-8017-619326A4A582", true, false, "", "", 1, "", "8B1C0348-76D4-404D-BB5D-665F834FF07E" ); // External Inquiry:Website Inquiry:Open
            RockMigrationHelper.UpdateWorkflowActionType( "C216691B-C40F-475F-B146-8C517433BA9A", "Open", 1, "38907A90-1634-4A93-8017-619326A4A582", true, false, "", "", 1, "", "DCF172EF-DBFD-4749-AC3C-A61E5361AD4E" ); // External Inquiry:Finance Inquiry:Open
            RockMigrationHelper.UpdateWorkflowActionType( "C216691B-C40F-475F-B146-8C517433BA9A", "Assign Worker", 0, "17962C23-2E94-4E06-8461-0FB8B94E2FEA", true, false, "", "", 1, "", "3C57B99B-7591-4B06-8334-BE8098C32B3B" ); // External Inquiry:Finance Inquiry:Assign Worker
            RockMigrationHelper.UpdateWorkflowActionType( "EC0C5563-00F8-4C3A-A085-BCC8D1926DBA", "Assign Worker", 0, "17962C23-2E94-4E06-8461-0FB8B94E2FEA", true, false, "", "", 1, "", "70737135-9FB0-41C5-8470-1BB94BEFEF16" ); // External Inquiry:Missions Inquiry:Assign Worker
            RockMigrationHelper.UpdateWorkflowActionType( "EC0C5563-00F8-4C3A-A085-BCC8D1926DBA", "Open", 1, "38907A90-1634-4A93-8017-619326A4A582", true, false, "", "", 1, "", "DB6A3EE9-9280-4CFB-BC9B-337946FA7A66" ); // External Inquiry:Missions Inquiry:Open
            RockMigrationHelper.UpdateWorkflowActionType( "074B237A-348C-4BD6-BE1E-0EDDC4F376E9", "Open", 1, "38907A90-1634-4A93-8017-619326A4A582", true, false, "", "", 1, "", "BDE13807-217D-4EC6-923E-8536EB5F3443" ); // External Inquiry:Pastoral Inquiry:Open
            RockMigrationHelper.UpdateWorkflowActionType( "074B237A-348C-4BD6-BE1E-0EDDC4F376E9", "Assign Worker", 0, "17962C23-2E94-4E06-8461-0FB8B94E2FEA", true, false, "", "", 1, "", "50D07904-5523-494C-BD11-62015C67CBE6" ); // External Inquiry:Pastoral Inquiry:Assign Worker
            RockMigrationHelper.UpdateWorkflowActionType( "E9A017DF-EB72-4DC9-B2A4-5A424383C7E0", "Complete Workflow", 0, "EEDA4318-F014-4A46-9C76-4C052EF81AA1", true, false, "", "", 1, "", "63CD1086-3505-47C9-9A10-4CDDB99D06B1" ); // External Inquiry:Complete:Complete Workflow
            RockMigrationHelper.UpdateWorkflowActionType( "2B912003-91E7-43B6-8060-A5ED97C4EDEE", "Assign Activity to Worker", 0, "F100A31F-E93A-4C7A-9E55-0FAF41A101C4", true, false, "", "", 1, "", "5B37C3A6-5579-48DE-BDF7-C8F61C96A32A" ); // External Inquiry:Open:Assign Activity to Worker
            RockMigrationHelper.UpdateWorkflowActionType( "2B912003-91E7-43B6-8060-A5ED97C4EDEE", "Capture Notes", 1, "486DC4FA-FCBC-425F-90B0-E606DA8A9F68", true, false, "EB7034BA-6300-434F-832F-37983B9DF154", "", 1, "", "6261A690-CFFF-4CD6-9258-C9BCA78163F1" ); // External Inquiry:Open:Capture Notes
            RockMigrationHelper.UpdateWorkflowActionType( "2B912003-91E7-43B6-8060-A5ED97C4EDEE", "Assign New Worker", 2, "38907A90-1634-4A93-8017-619326A4A582", true, true, "", "56A33DB7-FAC1-41F9-B42D-CC24D7A20466", 64, "", "586D24C8-3BE0-4638-B32F-170C9853F51D" ); // External Inquiry:Open:Assign New Worker
            RockMigrationHelper.UpdateWorkflowActionType( "2B912003-91E7-43B6-8060-A5ED97C4EDEE", "Re-Activate These Actions", 3, "699756EF-28EB-444B-BD28-15F0A167E614", false, false, "", "", 1, "", "3CB3F9B5-C1B5-4E4E-BBFB-0801521E62E1" ); // External Inquiry:Open:Re-Activate These Actions
            RockMigrationHelper.UpdateWorkflowActionType( "679FE471-517B-4278-BD43-5ECA8340F5C5", "Set Worker", 0, "C789E457-0783-44B3-9D8F-2EBAB5F11110", true, false, "", "", 1, "", "0E268839-D6BD-43A7-BAAC-A2ADEA12C5F1" ); // External Inquiry:Re-assign Worker:Set Worker
            RockMigrationHelper.UpdateWorkflowActionType( "679FE471-517B-4278-BD43-5ECA8340F5C5", "Clear New Worker Value", 1, "17962C23-2E94-4E06-8461-0FB8B94E2FEA", true, false, "", "", 1, "", "F4F60ABF-18E2-45EB-803A-E7716522CCC7" ); // External Inquiry:Re-assign Worker:Clear New Worker Value
            RockMigrationHelper.UpdateWorkflowActionType( "679FE471-517B-4278-BD43-5ECA8340F5C5", "Re-Activate Open Activity", 2, "38907A90-1634-4A93-8017-619326A4A582", true, false, "", "", 1, "", "4D4B2DF5-B4CD-4C1A-BEE1-3EBC63552349" ); // External Inquiry:Re-assign Worker:Re-Activate Open Activity
            RockMigrationHelper.UpdateWorkflowActionType( "BBFD8CB9-E4B3-46FC-8923-A67FC995EF94", "Prompt User", 0, "486DC4FA-FCBC-425F-90B0-E606DA8A9F68", true, false, "445FD357-9948-421B-BB1C-1D1F589112CB", "", 1, "", "27838E70-81FE-4867-816B-11BB4611C0AD" ); // Facilities Request:Request:Prompt User
            RockMigrationHelper.UpdateWorkflowActionType( "BBFD8CB9-E4B3-46FC-8923-A67FC995EF94", "Set Requester", 1, "24B7D5E6-C30F-48F4-9D7E-AF45A342CF3A", true, false, "", "", 1, "", "2C7BE93A-071F-4B85-8BD2-A3995D3B7D73" ); // Facilities Request:Request:Set Requester
            RockMigrationHelper.UpdateWorkflowActionType( "BBFD8CB9-E4B3-46FC-8923-A67FC995EF94", "Set Name", 2, "36005473-BD5D-470B-B28D-98E6D7ED808D", true, false, "", "", 1, "", "48A1FD9B-8F52-4EBC-81E3-5C5CEDC77FD3" ); // Facilities Request:Request:Set Name
            RockMigrationHelper.UpdateWorkflowActionType( "BBFD8CB9-E4B3-46FC-8923-A67FC995EF94", "Set Worker", 3, "17962C23-2E94-4E06-8461-0FB8B94E2FEA", true, false, "", "", 1, "", "30D4609E-49ED-464D-891F-DC94D0E51769" ); // Facilities Request:Request:Set Worker
            RockMigrationHelper.UpdateWorkflowActionType( "BBFD8CB9-E4B3-46FC-8923-A67FC995EF94", "Persist the Workflow", 4, "F1A39347-6FE0-43D4-89FB-544195088ECF", true, false, "", "", 1, "", "E91D46A2-C6AE-4BC1-81DB-C3BF49B20413" ); // Facilities Request:Request:Persist the Workflow
            RockMigrationHelper.UpdateWorkflowActionType( "FCB5C4EC-E25B-45D2-88E1-51F525A073A4", "Set Worker", 0, "C789E457-0783-44B3-9D8F-2EBAB5F11110", true, false, "", "", 1, "", "05801A9B-11D0-49A6-9C1D-49552DC80E07" ); // Facilities Request:Assign Worker:Set Worker
            RockMigrationHelper.UpdateWorkflowActionType( "FCB5C4EC-E25B-45D2-88E1-51F525A073A4", "Notify Worker", 1, "66197B01-D1F0-4924-A315-47AD54E030DE", true, false, "", "", 1, "", "7CEC1D71-5676-441A-B499-4A1C63BF0146" ); // Facilities Request:Assign Worker:Notify Worker
            RockMigrationHelper.UpdateWorkflowActionType( "FCB5C4EC-E25B-45D2-88E1-51F525A073A4", "Clear New Worker", 2, "17962C23-2E94-4E06-8461-0FB8B94E2FEA", true, false, "", "", 1, "", "7F8353CE-1F0C-4217-89D4-D478DA8BF9B1" ); // Facilities Request:Assign Worker:Clear New Worker
            RockMigrationHelper.UpdateWorkflowActionType( "FCB5C4EC-E25B-45D2-88E1-51F525A073A4", "Activate Open Activity", 3, "38907A90-1634-4A93-8017-619326A4A582", true, false, "", "", 1, "", "566F71EC-2082-4813-AEB0-C6BA19C002D9" ); // Facilities Request:Assign Worker:Activate Open Activity
            RockMigrationHelper.UpdateWorkflowActionType( "8226D12E-654A-41CF-B8B9-E4DFC6470402", "Assign Activity to Worker", 0, "F100A31F-E93A-4C7A-9E55-0FAF41A101C4", true, false, "", "", 1, "", "D0E5B32D-90EA-44DE-A5ED-CA78C6396B24" ); // Facilities Request:Open:Assign Activity to Worker
            RockMigrationHelper.UpdateWorkflowActionType( "8226D12E-654A-41CF-B8B9-E4DFC6470402", "Capture Notes", 1, "486DC4FA-FCBC-425F-90B0-E606DA8A9F68", true, false, "EB869AA0-C81F-4BA0-9A76-A18A0A544BC6", "", 1, "", "A70C039B-BD86-49BF-9640-51C6B2F99887" ); // Facilities Request:Open:Capture Notes
            RockMigrationHelper.UpdateWorkflowActionType( "8226D12E-654A-41CF-B8B9-E4DFC6470402", "Done", 2, "38907A90-1634-4A93-8017-619326A4A582", true, true, "", "AFE0AC86-88E3-4145-8E0C-421458681A13", 1, "Done", "92E64B7D-5F6D-4B89-9C2B-50038062EB8E" ); // Facilities Request:Open:Done
            RockMigrationHelper.UpdateWorkflowActionType( "8226D12E-654A-41CF-B8B9-E4DFC6470402", "Assign New Worker", 3, "38907A90-1634-4A93-8017-619326A4A582", true, true, "", "8F9E8196-087F-49D6-9879-AA139C7A1225", 64, "", "477FEB2F-8B83-4D89-BAFC-6161678B2069" ); // Facilities Request:Open:Assign New Worker
            RockMigrationHelper.UpdateWorkflowActionType( "8226D12E-654A-41CF-B8B9-E4DFC6470402", "Re-Activate This Activity", 4, "699756EF-28EB-444B-BD28-15F0A167E614", false, false, "", "AFE0AC86-88E3-4145-8E0C-421458681A13", 1, "Save", "66F30491-F718-4DE9-90C6-2CB6F5F8C433" ); // Facilities Request:Open:Re-Activate This Activity
            RockMigrationHelper.UpdateWorkflowActionType( "EBE339D7-53AD-45E8-8610-19C01BA1A0AB", "Assign Activity", 0, "F100A31F-E93A-4C7A-9E55-0FAF41A101C4", true, false, "", "", 1, "", "AD587683-6893-4837-ACE7-4CB44CC646DE" ); // Facilities Request:Enter Resolution:Assign Activity
            RockMigrationHelper.UpdateWorkflowActionType( "EBE339D7-53AD-45E8-8610-19C01BA1A0AB", "Enter Resolution", 1, "486DC4FA-FCBC-425F-90B0-E606DA8A9F68", true, false, "A08FC955-BB1A-4747-8FE3-88E3A2DB646E", "", 1, "", "8D56FFFE-6EF7-4389-97CE-F02BDE8A1074" ); // Facilities Request:Enter Resolution:Enter Resolution
            RockMigrationHelper.UpdateWorkflowActionType( "739CEAE9-4FAB-4FE5-A824-6D2E569D637C", "Notify Requester", 0, "66197B01-D1F0-4924-A315-47AD54E030DE", true, false, "", "", 1, "", "6EC85180-3513-419A-AAA1-E4A37ECB94A1" ); // Facilities Request:Complete:Notify Requester
            RockMigrationHelper.UpdateWorkflowActionType( "739CEAE9-4FAB-4FE5-A824-6D2E569D637C", "Complete the Workflow", 1, "EEDA4318-F014-4A46-9C76-4C052EF81AA1", true, false, "", "", 1, "", "F19E257B-E4AE-4E86-9D54-54B2DEC0A786" ); // Facilities Request:Complete:Complete the Workflow
            RockMigrationHelper.UpdateWorkflowActionType( "4A8FF789-887C-4BD7-8761-6E7D8D59BB5F", "Prompt User", 0, "486DC4FA-FCBC-425F-90B0-E606DA8A9F68", true, false, "36D3F2F7-CDA9-4D56-BEF1-21DDAF4E89B3", "", 1, "", "836BA2F0-9D2A-4B3C-9577-EF693AB85F28" ); // IT Support:Request:Prompt User
            RockMigrationHelper.UpdateWorkflowActionType( "4A8FF789-887C-4BD7-8761-6E7D8D59BB5F", "Set Requester", 1, "24B7D5E6-C30F-48F4-9D7E-AF45A342CF3A", true, false, "", "", 1, "", "490F29C9-9A52-41E3-A0D0-F18EAD8FD5AF" ); // IT Support:Request:Set Requester
            RockMigrationHelper.UpdateWorkflowActionType( "4A8FF789-887C-4BD7-8761-6E7D8D59BB5F", "Set Name", 2, "36005473-BD5D-470B-B28D-98E6D7ED808D", true, false, "", "", 1, "", "420E1EDE-2DEE-4293-89B1-A2415269B002" ); // IT Support:Request:Set Name
            RockMigrationHelper.UpdateWorkflowActionType( "4A8FF789-887C-4BD7-8761-6E7D8D59BB5F", "Set Worker", 3, "17962C23-2E94-4E06-8461-0FB8B94E2FEA", true, false, "", "", 1, "", "42AEC667-9D0B-482D-9490-3F569BA6958E" ); // IT Support:Request:Set Worker
            RockMigrationHelper.UpdateWorkflowActionType( "4A8FF789-887C-4BD7-8761-6E7D8D59BB5F", "Persist the Workflow", 4, "F1A39347-6FE0-43D4-89FB-544195088ECF", true, false, "", "", 1, "", "379C3A30-4FC1-4390-B1C3-0AEAB8D267C0" ); // IT Support:Request:Persist the Workflow
            RockMigrationHelper.UpdateWorkflowActionType( "9AC84ADE-DAD2-4045-B27D-232B248C9457", "Set Worker", 0, "C789E457-0783-44B3-9D8F-2EBAB5F11110", true, false, "", "", 1, "", "B1F0256C-3391-417C-B9EA-61307F2253B9" ); // IT Support:Assign Worker:Set Worker
            RockMigrationHelper.UpdateWorkflowActionType( "9AC84ADE-DAD2-4045-B27D-232B248C9457", "Notify Worker", 1, "66197B01-D1F0-4924-A315-47AD54E030DE", true, false, "", "", 1, "", "89FCD9D3-EB29-4111-8D09-C45CE58DA61B" ); // IT Support:Assign Worker:Notify Worker
            RockMigrationHelper.UpdateWorkflowActionType( "9AC84ADE-DAD2-4045-B27D-232B248C9457", "Clear New Worker", 2, "17962C23-2E94-4E06-8461-0FB8B94E2FEA", true, false, "", "", 1, "", "1A86A352-EE93-4C6C-B752-9F977B8E84F7" ); // IT Support:Assign Worker:Clear New Worker
            RockMigrationHelper.UpdateWorkflowActionType( "9AC84ADE-DAD2-4045-B27D-232B248C9457", "Activate Open Activity", 3, "38907A90-1634-4A93-8017-619326A4A582", true, false, "", "", 1, "", "06DCFDC8-72DE-4C7D-B937-74FE07ED54F1" ); // IT Support:Assign Worker:Activate Open Activity
            RockMigrationHelper.UpdateWorkflowActionType( "70B8DEB8-D641-4E64-A327-79D96B8B6756", "Assign Activity to Worker", 0, "F100A31F-E93A-4C7A-9E55-0FAF41A101C4", true, false, "", "", 1, "", "3CEB73EA-7E14-45D0-8315-49AB784AB088" ); // IT Support:Open:Assign Activity to Worker
            RockMigrationHelper.UpdateWorkflowActionType( "70B8DEB8-D641-4E64-A327-79D96B8B6756", "Capture Notes", 1, "486DC4FA-FCBC-425F-90B0-E606DA8A9F68", true, false, "B3C205F8-C917-4F45-8F73-CDC8E64B6CF8", "", 1, "", "D2316CDF-8EF8-4960-BD98-5332CFAE71A7" ); // IT Support:Open:Capture Notes
            RockMigrationHelper.UpdateWorkflowActionType( "70B8DEB8-D641-4E64-A327-79D96B8B6756", "Done", 2, "38907A90-1634-4A93-8017-619326A4A582", true, true, "", "45E50ED8-6922-41BF-B88F-1960DCEAAE99", 1, "Done", "F90FEBD6-B1E0-445C-AC9F-F4D53EE09227" ); // IT Support:Open:Done
            RockMigrationHelper.UpdateWorkflowActionType( "70B8DEB8-D641-4E64-A327-79D96B8B6756", "Assign New Worker", 3, "38907A90-1634-4A93-8017-619326A4A582", true, true, "", "91D7D16C-F6A1-4B85-A7EE-A0D20B5A2BC5", 64, "", "F6BCA594-3CBE-45F4-A13D-7CE380FF8C9C" ); // IT Support:Open:Assign New Worker
            RockMigrationHelper.UpdateWorkflowActionType( "70B8DEB8-D641-4E64-A327-79D96B8B6756", "Re-Activate This Activity", 4, "699756EF-28EB-444B-BD28-15F0A167E614", false, false, "", "45E50ED8-6922-41BF-B88F-1960DCEAAE99", 1, "Save", "3314F559-79D3-4F88-B1DB-C2E12BAA04A3" ); // IT Support:Open:Re-Activate This Activity
            RockMigrationHelper.UpdateWorkflowActionType( "60D58F02-1D58-40FF-AA40-1637CC8F8DC1", "Assign Activity", 0, "F100A31F-E93A-4C7A-9E55-0FAF41A101C4", true, false, "", "", 1, "", "7379B7B2-EFA1-4D2C-9905-ED6E594FCC05" ); // IT Support:Enter Resolution:Assign Activity
            RockMigrationHelper.UpdateWorkflowActionType( "60D58F02-1D58-40FF-AA40-1637CC8F8DC1", "Enter Resolution", 1, "486DC4FA-FCBC-425F-90B0-E606DA8A9F68", true, false, "6DE4BBEA-67A2-4B14-973F-6E699228620D", "", 1, "", "247BA14B-2313-4ED9-9F3D-F0FBABB67CEC" ); // IT Support:Enter Resolution:Enter Resolution
            RockMigrationHelper.UpdateWorkflowActionType( "CAB42A01-6430-42A1-B476-206B5930EA97", "Notify Requester", 0, "66197B01-D1F0-4924-A315-47AD54E030DE", true, false, "", "", 1, "", "15D058D6-81FC-449E-8090-DD8AAFF533C0" ); // IT Support:Complete:Notify Requester
            RockMigrationHelper.UpdateWorkflowActionType( "CAB42A01-6430-42A1-B476-206B5930EA97", "Complete the Workflow", 1, "EEDA4318-F014-4A46-9C76-4C052EF81AA1", true, false, "", "", 1, "", "A0400B10-3CEA-4124-A646-490E12CEDEC7" ); // IT Support:Complete:Complete the Workflow
            RockMigrationHelper.AddActionTypeAttributeValue( "87558E26-399D-4EBB-A860-932F8792738C", "9392E3D7-A28B-4CD8-8B03-5E147B102EF1", @"False" ); // Person Data Error:Launch From Person Profile:Set Person:Active
            RockMigrationHelper.AddActionTypeAttributeValue( "87558E26-399D-4EBB-A860-932F8792738C", "61E6E1BC-E657-4F00-B2E9-769AAA25B9F7", @"fd89f2c8-cbc8-4ed1-96d1-891ab9616c9e" ); // Person Data Error:Launch From Person Profile:Set Person:Attribute
            RockMigrationHelper.AddActionTypeAttributeValue( "87558E26-399D-4EBB-A860-932F8792738C", "AD4EFAC4-E687-43DF-832F-0DC3856ABABB", @"" ); // Person Data Error:Launch From Person Profile:Set Person:Order
            RockMigrationHelper.AddActionTypeAttributeValue( "27838E70-81FE-4867-816B-11BB4611C0AD", "234910F2-A0DB-4D7D-BAF7-83C880EF30AE", @"False" ); // Facilities Request:Request:Prompt User:Active
            RockMigrationHelper.AddActionTypeAttributeValue( "27838E70-81FE-4867-816B-11BB4611C0AD", "C178113D-7C86-4229-8424-C6D0CF4A7E23", @"" ); // Facilities Request:Request:Prompt User:Order
            RockMigrationHelper.AddActionTypeAttributeValue( "836BA2F0-9D2A-4B3C-9577-EF693AB85F28", "234910F2-A0DB-4D7D-BAF7-83C880EF30AE", @"False" ); // IT Support:Request:Prompt User:Active
            RockMigrationHelper.AddActionTypeAttributeValue( "836BA2F0-9D2A-4B3C-9577-EF693AB85F28", "C178113D-7C86-4229-8424-C6D0CF4A7E23", @"" ); // IT Support:Request:Prompt User:Order
            RockMigrationHelper.AddActionTypeAttributeValue( "AEACAE26-20D8-4F24-90FF-3E15475CA8F9", "234910F2-A0DB-4D7D-BAF7-83C880EF30AE", @"False" ); // External Inquiry:Request:Prompt User:Active
            RockMigrationHelper.AddActionTypeAttributeValue( "AEACAE26-20D8-4F24-90FF-3E15475CA8F9", "C178113D-7C86-4229-8424-C6D0CF4A7E23", @"" ); // External Inquiry:Request:Prompt User:Order
            RockMigrationHelper.AddActionTypeAttributeValue( "490F29C9-9A52-41E3-A0D0-F18EAD8FD5AF", "DE9CB292-4785-4EA3-976D-3826F91E9E98", @"False" ); // IT Support:Request:Set Requester:Active
            RockMigrationHelper.AddActionTypeAttributeValue( "490F29C9-9A52-41E3-A0D0-F18EAD8FD5AF", "89E9BCED-91AB-47B0-AD52-D78B0B7CB9E8", @"" ); // IT Support:Request:Set Requester:Order
            RockMigrationHelper.AddActionTypeAttributeValue( "490F29C9-9A52-41E3-A0D0-F18EAD8FD5AF", "BBED8A83-8BB2-4D35-BAFB-05F67DCAD112", @"ce01e29e-4a9c-4bbf-bbcd-235713a81353" ); // IT Support:Request:Set Requester:Person Attribute
            RockMigrationHelper.AddActionTypeAttributeValue( "2C7BE93A-071F-4B85-8BD2-A3995D3B7D73", "DE9CB292-4785-4EA3-976D-3826F91E9E98", @"False" ); // Facilities Request:Request:Set Requester:Active
            RockMigrationHelper.AddActionTypeAttributeValue( "2C7BE93A-071F-4B85-8BD2-A3995D3B7D73", "89E9BCED-91AB-47B0-AD52-D78B0B7CB9E8", @"" ); // Facilities Request:Request:Set Requester:Order
            RockMigrationHelper.AddActionTypeAttributeValue( "2C7BE93A-071F-4B85-8BD2-A3995D3B7D73", "BBED8A83-8BB2-4D35-BAFB-05F67DCAD112", @"594d522b-1e69-4175-9c6b-039c6baf279e" ); // Facilities Request:Request:Set Requester:Person Attribute
            RockMigrationHelper.AddActionTypeAttributeValue( "7E8160EC-1AB1-4D69-8FB3-F7AA08904C15", "50B01639-4938-40D2-A791-AA0EB4F86847", @"False" ); // Person Data Error:Launch From Person Profile:Persist Workflow:Active
            RockMigrationHelper.AddActionTypeAttributeValue( "7E8160EC-1AB1-4D69-8FB3-F7AA08904C15", "86F795B0-0CB6-4DA4-9CE4-B11D0922F361", @"" ); // Person Data Error:Launch From Person Profile:Persist Workflow:Order
            RockMigrationHelper.AddActionTypeAttributeValue( "3DD65F61-6F18-4794-9C05-8FEF69A0DC31", "0A800013-51F7-4902-885A-5BE215D67D3D", @"False" ); // External Inquiry:Request:Set Name:Active
            RockMigrationHelper.AddActionTypeAttributeValue( "3DD65F61-6F18-4794-9C05-8FEF69A0DC31", "5D95C15A-CCAE-40AD-A9DD-F929DA587115", @"" ); // External Inquiry:Request:Set Name:Order
            RockMigrationHelper.AddActionTypeAttributeValue( "3DD65F61-6F18-4794-9C05-8FEF69A0DC31", "93852244-A667-4749-961A-D47F88675BE4", @"{{ Workflow.FirstName }} {{ Workflow.LastName }} ( {{ Workflow.Topic }} )" ); // External Inquiry:Request:Set Name:Text Value|Attribute Value
            RockMigrationHelper.AddActionTypeAttributeValue( "48A1FD9B-8F52-4EBC-81E3-5C5CEDC77FD3", "0A800013-51F7-4902-885A-5BE215D67D3D", @"False" ); // Facilities Request:Request:Set Name:Active
            RockMigrationHelper.AddActionTypeAttributeValue( "48A1FD9B-8F52-4EBC-81E3-5C5CEDC77FD3", "5D95C15A-CCAE-40AD-A9DD-F929DA587115", @"" ); // Facilities Request:Request:Set Name:Order
            RockMigrationHelper.AddActionTypeAttributeValue( "420E1EDE-2DEE-4293-89B1-A2415269B002", "0A800013-51F7-4902-885A-5BE215D67D3D", @"False" ); // IT Support:Request:Set Name:Active
            RockMigrationHelper.AddActionTypeAttributeValue( "420E1EDE-2DEE-4293-89B1-A2415269B002", "5D95C15A-CCAE-40AD-A9DD-F929DA587115", @"" ); // IT Support:Request:Set Name:Order
            RockMigrationHelper.AddActionTypeAttributeValue( "465B9C7B-BE88-43D1-A5EC-C780E373793D", "DE9CB292-4785-4EA3-976D-3826F91E9E98", @"False" ); // External Inquiry:Request:Set Requester:Active
            RockMigrationHelper.AddActionTypeAttributeValue( "465B9C7B-BE88-43D1-A5EC-C780E373793D", "89E9BCED-91AB-47B0-AD52-D78B0B7CB9E8", @"" ); // External Inquiry:Request:Set Requester:Order
            RockMigrationHelper.AddActionTypeAttributeValue( "465B9C7B-BE88-43D1-A5EC-C780E373793D", "BBED8A83-8BB2-4D35-BAFB-05F67DCAD112", @"f5ae12d8-de33-4c8a-a372-de87134e3f75" ); // External Inquiry:Request:Set Requester:Person Attribute
            RockMigrationHelper.AddActionTypeAttributeValue( "420E1EDE-2DEE-4293-89B1-A2415269B002", "93852244-A667-4749-961A-D47F88675BE4", @"59f1fb67-f2b3-441e-ace3-9b10ab7e674f" ); // IT Support:Request:Set Name:Text Value|Attribute Value
            RockMigrationHelper.AddActionTypeAttributeValue( "48A1FD9B-8F52-4EBC-81E3-5C5CEDC77FD3", "93852244-A667-4749-961A-D47F88675BE4", @"8767527f-c1dc-42e2-b24d-6061c74830ef" ); // Facilities Request:Request:Set Name:Text Value|Attribute Value
            RockMigrationHelper.AddActionTypeAttributeValue( "30D4609E-49ED-464D-891F-DC94D0E51769", "CE28B79D-FBC2-4894-9198-D923D0217549", @"False" ); // Facilities Request:Request:Set Worker:Active
            RockMigrationHelper.AddActionTypeAttributeValue( "30D4609E-49ED-464D-891F-DC94D0E51769", "7AC47975-71AC-4A2F-BF1F-115CF5578D6F", @"8f9e8196-087f-49d6-9879-aa139c7a1225" ); // Facilities Request:Request:Set Worker:Attribute
            RockMigrationHelper.AddActionTypeAttributeValue( "30D4609E-49ED-464D-891F-DC94D0E51769", "18EF907D-607E-4891-B034-7AA379D77854", @"" ); // Facilities Request:Request:Set Worker:Order
            RockMigrationHelper.AddActionTypeAttributeValue( "42AEC667-9D0B-482D-9490-3F569BA6958E", "CE28B79D-FBC2-4894-9198-D923D0217549", @"False" ); // IT Support:Request:Set Worker:Active
            RockMigrationHelper.AddActionTypeAttributeValue( "42AEC667-9D0B-482D-9490-3F569BA6958E", "7AC47975-71AC-4A2F-BF1F-115CF5578D6F", @"91d7d16c-f6a1-4b85-a7ee-a0d20b5a2bc5" ); // IT Support:Request:Set Worker:Attribute
            RockMigrationHelper.AddActionTypeAttributeValue( "42AEC667-9D0B-482D-9490-3F569BA6958E", "18EF907D-607E-4891-B034-7AA379D77854", @"" ); // IT Support:Request:Set Worker:Order
            RockMigrationHelper.AddActionTypeAttributeValue( "4E59F578-5C2D-4446-A98A-6C908CACFB2B", "50B01639-4938-40D2-A791-AA0EB4F86847", @"False" ); // External Inquiry:Request:Persist the Workflow:Active
            RockMigrationHelper.AddActionTypeAttributeValue( "4E59F578-5C2D-4446-A98A-6C908CACFB2B", "86F795B0-0CB6-4DA4-9CE4-B11D0922F361", @"" ); // External Inquiry:Request:Persist the Workflow:Order
            RockMigrationHelper.AddActionTypePersonAttributeValue( "42AEC667-9D0B-482D-9490-3F569BA6958E", "5C803BD1-40FA-49B1-AE7E-68F43D3687BB", @"d565a6c7-b789-4490-8f7f-253671dc2a8e" ); // IT Support:Request:Set Worker:Person
            RockMigrationHelper.AddActionTypePersonAttributeValue( "30D4609E-49ED-464D-891F-DC94D0E51769", "5C803BD1-40FA-49B1-AE7E-68F43D3687BB", @"d565a6c7-b789-4490-8f7f-253671dc2a8e" ); // Facilities Request:Request:Set Worker:Person
            RockMigrationHelper.AddActionTypeAttributeValue( "E91D46A2-C6AE-4BC1-81DB-C3BF49B20413", "50B01639-4938-40D2-A791-AA0EB4F86847", @"False" ); // Facilities Request:Request:Persist the Workflow:Active
            RockMigrationHelper.AddActionTypeAttributeValue( "E91D46A2-C6AE-4BC1-81DB-C3BF49B20413", "86F795B0-0CB6-4DA4-9CE4-B11D0922F361", @"" ); // Facilities Request:Request:Persist the Workflow:Order
            RockMigrationHelper.AddActionTypeAttributeValue( "379C3A30-4FC1-4390-B1C3-0AEAB8D267C0", "50B01639-4938-40D2-A791-AA0EB4F86847", @"False" ); // IT Support:Request:Persist the Workflow:Active
            RockMigrationHelper.AddActionTypeAttributeValue( "379C3A30-4FC1-4390-B1C3-0AEAB8D267C0", "86F795B0-0CB6-4DA4-9CE4-B11D0922F361", @"" ); // IT Support:Request:Persist the Workflow:Order
            RockMigrationHelper.AddActionTypeAttributeValue( "39E8D405-49B6-4A8F-A100-26538EA21362", "E8ABD802-372C-47BE-82B1-96F50DB5169E", @"False" ); // External Inquiry:Request:Test for General Inquiry:Active
            RockMigrationHelper.AddActionTypeAttributeValue( "39E8D405-49B6-4A8F-A100-26538EA21362", "02D5A7A5-8781-46B4-B9FC-AF816829D240", @"b89148c7-ef85-450d-81cb-f0bf2102e0cb" ); // External Inquiry:Request:Test for General Inquiry:Activity
            RockMigrationHelper.AddActionTypeAttributeValue( "39E8D405-49B6-4A8F-A100-26538EA21362", "3809A78C-B773-440C-8E3F-A8E81D0DAE08", @"" ); // External Inquiry:Request:Test for General Inquiry:Order
            RockMigrationHelper.AddActionTypeAttributeValue( "73A89BFA-6D25-4933-898A-383D3A9E6BED", "E8ABD802-372C-47BE-82B1-96F50DB5169E", @"False" ); // External Inquiry:Request:Test for Login Inquiry:Active
            RockMigrationHelper.AddActionTypeAttributeValue( "73A89BFA-6D25-4933-898A-383D3A9E6BED", "02D5A7A5-8781-46B4-B9FC-AF816829D240", @"d73b1b06-30f6-4504-b95f-2322afe96e55" ); // External Inquiry:Request:Test for Login Inquiry:Activity
            RockMigrationHelper.AddActionTypeAttributeValue( "73A89BFA-6D25-4933-898A-383D3A9E6BED", "3809A78C-B773-440C-8E3F-A8E81D0DAE08", @"" ); // External Inquiry:Request:Test for Login Inquiry:Order
            RockMigrationHelper.AddActionTypeAttributeValue( "B27137A5-3A9D-4C1B-8D60-8A1A159FE74F", "E8ABD802-372C-47BE-82B1-96F50DB5169E", @"False" ); // External Inquiry:Request:Test for Website Inquiry:Active
            RockMigrationHelper.AddActionTypeAttributeValue( "B27137A5-3A9D-4C1B-8D60-8A1A159FE74F", "02D5A7A5-8781-46B4-B9FC-AF816829D240", @"ace58142-04dc-4b87-bc78-f6c9e00c68a9" ); // External Inquiry:Request:Test for Website Inquiry:Activity
            RockMigrationHelper.AddActionTypeAttributeValue( "B27137A5-3A9D-4C1B-8D60-8A1A159FE74F", "3809A78C-B773-440C-8E3F-A8E81D0DAE08", @"" ); // External Inquiry:Request:Test for Website Inquiry:Order
            RockMigrationHelper.AddActionTypeAttributeValue( "5A1A8A4B-D480-44B3-9C92-5C57C3A1798D", "E8ABD802-372C-47BE-82B1-96F50DB5169E", @"False" ); // External Inquiry:Request:Test for Finance Inquiry:Active
            RockMigrationHelper.AddActionTypeAttributeValue( "5A1A8A4B-D480-44B3-9C92-5C57C3A1798D", "02D5A7A5-8781-46B4-B9FC-AF816829D240", @"c216691b-c40f-475f-b146-8c517433ba9a" ); // External Inquiry:Request:Test for Finance Inquiry:Activity
            RockMigrationHelper.AddActionTypeAttributeValue( "5A1A8A4B-D480-44B3-9C92-5C57C3A1798D", "3809A78C-B773-440C-8E3F-A8E81D0DAE08", @"" ); // External Inquiry:Request:Test for Finance Inquiry:Order
            RockMigrationHelper.AddActionTypeAttributeValue( "1F193146-D379-46B7-AEDF-8BBA90E8238B", "E8ABD802-372C-47BE-82B1-96F50DB5169E", @"False" ); // External Inquiry:Request:Test for Missions Inquiry:Active
            RockMigrationHelper.AddActionTypeAttributeValue( "1F193146-D379-46B7-AEDF-8BBA90E8238B", "02D5A7A5-8781-46B4-B9FC-AF816829D240", @"ec0c5563-00f8-4c3a-a085-bcc8d1926dba" ); // External Inquiry:Request:Test for Missions Inquiry:Activity
            RockMigrationHelper.AddActionTypeAttributeValue( "1F193146-D379-46B7-AEDF-8BBA90E8238B", "3809A78C-B773-440C-8E3F-A8E81D0DAE08", @"" ); // External Inquiry:Request:Test for Missions Inquiry:Order
            RockMigrationHelper.AddActionTypeAttributeValue( "636F5D27-DAAE-462D-A0ED-666C70EC5F35", "E8ABD802-372C-47BE-82B1-96F50DB5169E", @"False" ); // External Inquiry:Request:Test for Pastor Inquiry:Active
            RockMigrationHelper.AddActionTypeAttributeValue( "636F5D27-DAAE-462D-A0ED-666C70EC5F35", "02D5A7A5-8781-46B4-B9FC-AF816829D240", @"074b237a-348c-4bd6-be1e-0eddc4f376e9" ); // External Inquiry:Request:Test for Pastor Inquiry:Activity
            RockMigrationHelper.AddActionTypeAttributeValue( "636F5D27-DAAE-462D-A0ED-666C70EC5F35", "3809A78C-B773-440C-8E3F-A8E81D0DAE08", @"" ); // External Inquiry:Request:Test for Pastor Inquiry:Order
            RockMigrationHelper.AddActionTypeAttributeValue( "D56341AE-7A81-445F-B2CC-21C84A767D6C", "DE9CB292-4785-4EA3-976D-3826F91E9E98", @"False" ); // Person Data Error:Entry:Set the Reported By value:Active
            RockMigrationHelper.AddActionTypeAttributeValue( "D56341AE-7A81-445F-B2CC-21C84A767D6C", "89E9BCED-91AB-47B0-AD52-D78B0B7CB9E8", @"" ); // Person Data Error:Entry:Set the Reported By value:Order
            RockMigrationHelper.AddActionTypeAttributeValue( "D56341AE-7A81-445F-B2CC-21C84A767D6C", "BBED8A83-8BB2-4D35-BAFB-05F67DCAD112", @"c8fd97b5-adc0-4ef0-b397-e4010850148c" ); // Person Data Error:Entry:Set the Reported By value:Person Attribute
            RockMigrationHelper.AddActionTypeAttributeValue( "4642162E-B893-43B4-9D52-F8221206BAEB", "CE28B79D-FBC2-4894-9198-D923D0217549", @"False" ); // External Inquiry:General Inquiry:Assign Worker:Active
            RockMigrationHelper.AddActionTypeAttributeValue( "4642162E-B893-43B4-9D52-F8221206BAEB", "7AC47975-71AC-4A2F-BF1F-115CF5578D6F", @"04712fbd-715d-412e-96c3-10c748482d6e" ); // External Inquiry:General Inquiry:Assign Worker:Attribute
            RockMigrationHelper.AddActionTypeAttributeValue( "4642162E-B893-43B4-9D52-F8221206BAEB", "18EF907D-607E-4891-B034-7AA379D77854", @"" ); // External Inquiry:General Inquiry:Assign Worker:Order
            RockMigrationHelper.AddActionTypeAttributeValue( "B1F0256C-3391-417C-B9EA-61307F2253B9", "D7EAA859-F500-4521-9523-488B12EAA7D2", @"False" ); // IT Support:Assign Worker:Set Worker:Active
            RockMigrationHelper.AddActionTypeAttributeValue( "B1F0256C-3391-417C-B9EA-61307F2253B9", "44A0B977-4730-4519-8FF6-B0A01A95B212", @"9d165825-21f2-4b5f-9a64-790a6ebd51ac" ); // IT Support:Assign Worker:Set Worker:Attribute
            RockMigrationHelper.AddActionTypeAttributeValue( "B1F0256C-3391-417C-B9EA-61307F2253B9", "57093B41-50ED-48E5-B72B-8829E62704C8", @"" ); // IT Support:Assign Worker:Set Worker:Order
            RockMigrationHelper.AddActionTypeAttributeValue( "05801A9B-11D0-49A6-9C1D-49552DC80E07", "D7EAA859-F500-4521-9523-488B12EAA7D2", @"False" ); // Facilities Request:Assign Worker:Set Worker:Active
            RockMigrationHelper.AddActionTypeAttributeValue( "05801A9B-11D0-49A6-9C1D-49552DC80E07", "44A0B977-4730-4519-8FF6-B0A01A95B212", @"e0d71111-05f1-4cd3-959e-1d246613942e" ); // Facilities Request:Assign Worker:Set Worker:Attribute
            RockMigrationHelper.AddActionTypeAttributeValue( "05801A9B-11D0-49A6-9C1D-49552DC80E07", "57093B41-50ED-48E5-B72B-8829E62704C8", @"" ); // Facilities Request:Assign Worker:Set Worker:Order
            RockMigrationHelper.AddActionTypeAttributeValue( "4642162E-B893-43B4-9D52-F8221206BAEB", "D7EAA859-F500-4521-9523-488B12EAA7D2", @"False" ); // External Inquiry:General Inquiry:Assign Worker:Active
            RockMigrationHelper.AddActionTypeAttributeValue( "4642162E-B893-43B4-9D52-F8221206BAEB", "44A0B977-4730-4519-8FF6-B0A01A95B212", @"d4d3588b-0a13-432c-bafb-9964ebb2b9f4" ); // External Inquiry:General Inquiry:Assign Worker:Attribute
            RockMigrationHelper.AddActionTypeAttributeValue( "4642162E-B893-43B4-9D52-F8221206BAEB", "57093B41-50ED-48E5-B72B-8829E62704C8", @"" ); // External Inquiry:General Inquiry:Assign Worker:Order
            RockMigrationHelper.AddActionTypeAttributeValue( "4642162E-B893-43B4-9D52-F8221206BAEB", "E5272B11-A2B8-49DC-860D-8D574E2BC15C", @"" ); // External Inquiry:General Inquiry:Assign Worker:Text Value|Attribute Value
            RockMigrationHelper.AddActionTypeAttributeValue( "05801A9B-11D0-49A6-9C1D-49552DC80E07", "E5272B11-A2B8-49DC-860D-8D574E2BC15C", @"8f9e8196-087f-49d6-9879-aa139c7a1225" ); // Facilities Request:Assign Worker:Set Worker:Text Value|Attribute Value
            RockMigrationHelper.AddActionTypeAttributeValue( "B1F0256C-3391-417C-B9EA-61307F2253B9", "E5272B11-A2B8-49DC-860D-8D574E2BC15C", @"91d7d16c-f6a1-4b85-a7ee-a0d20b5a2bc5" ); // IT Support:Assign Worker:Set Worker:Text Value|Attribute Value
            RockMigrationHelper.AddActionTypePersonAttributeValue( "4642162E-B893-43B4-9D52-F8221206BAEB", "5C803BD1-40FA-49B1-AE7E-68F43D3687BB", @"d565a6c7-b789-4490-8f7f-253671dc2a8e" ); // External Inquiry:General Inquiry:Assign Worker:Person
            RockMigrationHelper.AddActionTypeAttributeValue( "341569AB-76F6-44E8-A033-CE29FDE8035F", "234910F2-A0DB-4D7D-BAF7-83C880EF30AE", @"False" ); // Person Data Error:Entry:Enter Details:Active
            RockMigrationHelper.AddActionTypeAttributeValue( "341569AB-76F6-44E8-A033-CE29FDE8035F", "C178113D-7C86-4229-8424-C6D0CF4A7E23", @"" ); // Person Data Error:Entry:Enter Details:Order
            RockMigrationHelper.AddActionTypeAttributeValue( "89FCD9D3-EB29-4111-8D09-C45CE58DA61B", "36197160-7D3D-490D-AB42-7E29105AFE91", @"False" ); // IT Support:Assign Worker:Notify Worker:Active
            RockMigrationHelper.AddActionTypeAttributeValue( "89FCD9D3-EB29-4111-8D09-C45CE58DA61B", "D1269254-C15A-40BD-B784-ADCC231D3950", @"" ); // IT Support:Assign Worker:Notify Worker:Order
            RockMigrationHelper.AddActionTypeAttributeValue( "89FCD9D3-EB29-4111-8D09-C45CE58DA61B", "0C4C13B8-7076-4872-925A-F950886B5E16", @"9d165825-21f2-4b5f-9a64-790a6ebd51ac" ); // IT Support:Assign Worker:Notify Worker:Send To Email Address|Attribute Value
            RockMigrationHelper.AddActionTypeAttributeValue( "7CEC1D71-5676-441A-B499-4A1C63BF0146", "36197160-7D3D-490D-AB42-7E29105AFE91", @"False" ); // Facilities Request:Assign Worker:Notify Worker:Active
            RockMigrationHelper.AddActionTypeAttributeValue( "7CEC1D71-5676-441A-B499-4A1C63BF0146", "D1269254-C15A-40BD-B784-ADCC231D3950", @"" ); // Facilities Request:Assign Worker:Notify Worker:Order
            RockMigrationHelper.AddActionTypeAttributeValue( "7CEC1D71-5676-441A-B499-4A1C63BF0146", "0C4C13B8-7076-4872-925A-F950886B5E16", @"e0d71111-05f1-4cd3-959e-1d246613942e" ); // Facilities Request:Assign Worker:Notify Worker:Send To Email Address|Attribute Value
            RockMigrationHelper.AddActionTypeAttributeValue( "F46A76F8-BDB4-434F-8A31-88810F8CE2ED", "E8ABD802-372C-47BE-82B1-96F50DB5169E", @"False" ); // External Inquiry:General Inquiry:Open:Active
            RockMigrationHelper.AddActionTypeAttributeValue( "F46A76F8-BDB4-434F-8A31-88810F8CE2ED", "02D5A7A5-8781-46B4-B9FC-AF816829D240", @"2b912003-91e7-43b6-8060-a5ed97c4edee" ); // External Inquiry:General Inquiry:Open:Activity
            RockMigrationHelper.AddActionTypeAttributeValue( "F46A76F8-BDB4-434F-8A31-88810F8CE2ED", "3809A78C-B773-440C-8E3F-A8E81D0DAE08", @"" ); // External Inquiry:General Inquiry:Open:Order
            RockMigrationHelper.AddActionTypeAttributeValue( "7CEC1D71-5676-441A-B499-4A1C63BF0146", "9F5F7CEC-F369-4FDF-802A-99074CE7A7FC", @"" ); // Facilities Request:Assign Worker:Notify Worker:From
            RockMigrationHelper.AddActionTypeAttributeValue( "89FCD9D3-EB29-4111-8D09-C45CE58DA61B", "9F5F7CEC-F369-4FDF-802A-99074CE7A7FC", @"" ); // IT Support:Assign Worker:Notify Worker:From
            RockMigrationHelper.AddActionTypeAttributeValue( "89FCD9D3-EB29-4111-8D09-C45CE58DA61B", "5D9B13B6-CD96-4C7C-86FA-4512B9D28386", @"IT Support Request: {{ Workflow.Name }}" ); // IT Support:Assign Worker:Notify Worker:Subject
            RockMigrationHelper.AddActionTypeAttributeValue( "7CEC1D71-5676-441A-B499-4A1C63BF0146", "5D9B13B6-CD96-4C7C-86FA-4512B9D28386", @"Facilities Request: {{ Workflow.Name }}" ); // Facilities Request:Assign Worker:Notify Worker:Subject
            RockMigrationHelper.AddActionTypeAttributeValue( "7CEC1D71-5676-441A-B499-4A1C63BF0146", "4D245B9E-6B03-46E7-8482-A51FBA190E4D", @"{{ GlobalAttribute.EmailHeader }}

<p>The following Facilities Request has been submitted by {{ Workflow.Requester }}:</p>

<h4><a href='{{ GlobalAttribute.InternalApplicationRoot }}WorkflowEntry/{{ Workflow.WorkflowTypeId }}?WorkflowGuid={{ Workflow.Guid }}'>{{ Workflow.Name }}</a></h4>
<p>{{ Workflow.Details }}</p>

{{ GlobalAttribute.EmailFooter }}

" ); // Facilities Request:Assign Worker:Notify Worker:Body
            RockMigrationHelper.AddActionTypeAttributeValue( "89FCD9D3-EB29-4111-8D09-C45CE58DA61B", "4D245B9E-6B03-46E7-8482-A51FBA190E4D", @"{{ GlobalAttribute.EmailHeader }}

<p>The following IT Support Request has been submitted by {{ Workflow.Requester }}:</p>

<h4><a href='{{ GlobalAttribute.InternalApplicationRoot }}WorkflowEntry/{{ Workflow.WorkflowTypeId }}?WorkflowGuid={{ Workflow.Guid }}'>{{ Workflow.Name }}</a></h4>
<p>{{ Workflow.Details }}</p>

{{ GlobalAttribute.EmailFooter }}

" ); // IT Support:Assign Worker:Notify Worker:Body
            RockMigrationHelper.AddActionTypeAttributeValue( "1A86A352-EE93-4C6C-B752-9F977B8E84F7", "CE28B79D-FBC2-4894-9198-D923D0217549", @"False" ); // IT Support:Assign Worker:Clear New Worker:Active
            RockMigrationHelper.AddActionTypeAttributeValue( "1A86A352-EE93-4C6C-B752-9F977B8E84F7", "7AC47975-71AC-4A2F-BF1F-115CF5578D6F", @"91d7d16c-f6a1-4b85-a7ee-a0d20b5a2bc5" ); // IT Support:Assign Worker:Clear New Worker:Attribute
            RockMigrationHelper.AddActionTypeAttributeValue( "1A86A352-EE93-4C6C-B752-9F977B8E84F7", "18EF907D-607E-4891-B034-7AA379D77854", @"" ); // IT Support:Assign Worker:Clear New Worker:Order
            RockMigrationHelper.AddActionTypeAttributeValue( "7F8353CE-1F0C-4217-89D4-D478DA8BF9B1", "CE28B79D-FBC2-4894-9198-D923D0217549", @"False" ); // Facilities Request:Assign Worker:Clear New Worker:Active
            RockMigrationHelper.AddActionTypeAttributeValue( "7F8353CE-1F0C-4217-89D4-D478DA8BF9B1", "7AC47975-71AC-4A2F-BF1F-115CF5578D6F", @"8f9e8196-087f-49d6-9879-aa139c7a1225" ); // Facilities Request:Assign Worker:Clear New Worker:Attribute
            RockMigrationHelper.AddActionTypeAttributeValue( "7F8353CE-1F0C-4217-89D4-D478DA8BF9B1", "18EF907D-607E-4891-B034-7AA379D77854", @"" ); // Facilities Request:Assign Worker:Clear New Worker:Order
            RockMigrationHelper.AddActionTypeAttributeValue( "5169CBED-08DF-4ABD-91AB-83A2637C5515", "50B01639-4938-40D2-A791-AA0EB4F86847", @"False" ); // Person Data Error:Entry:Persist the Workflow:Active
            RockMigrationHelper.AddActionTypeAttributeValue( "5169CBED-08DF-4ABD-91AB-83A2637C5515", "86F795B0-0CB6-4DA4-9CE4-B11D0922F361", @"" ); // Person Data Error:Entry:Persist the Workflow:Order
            RockMigrationHelper.AddActionTypePersonAttributeValue( "7F8353CE-1F0C-4217-89D4-D478DA8BF9B1", "5C803BD1-40FA-49B1-AE7E-68F43D3687BB", @"" ); // Facilities Request:Assign Worker:Clear New Worker:Person
            RockMigrationHelper.AddActionTypePersonAttributeValue( "1A86A352-EE93-4C6C-B752-9F977B8E84F7", "5C803BD1-40FA-49B1-AE7E-68F43D3687BB", @"" ); // IT Support:Assign Worker:Clear New Worker:Person
            RockMigrationHelper.AddActionTypeAttributeValue( "06DCFDC8-72DE-4C7D-B937-74FE07ED54F1", "E8ABD802-372C-47BE-82B1-96F50DB5169E", @"False" ); // IT Support:Assign Worker:Activate Open Activity:Active
            RockMigrationHelper.AddActionTypeAttributeValue( "06DCFDC8-72DE-4C7D-B937-74FE07ED54F1", "02D5A7A5-8781-46B4-B9FC-AF816829D240", @"70B8DEB8-D641-4E64-A327-79D96B8B6756" ); // IT Support:Assign Worker:Activate Open Activity:Activity
            RockMigrationHelper.AddActionTypeAttributeValue( "06DCFDC8-72DE-4C7D-B937-74FE07ED54F1", "3809A78C-B773-440C-8E3F-A8E81D0DAE08", @"" ); // IT Support:Assign Worker:Activate Open Activity:Order
            RockMigrationHelper.AddActionTypeAttributeValue( "566F71EC-2082-4813-AEB0-C6BA19C002D9", "E8ABD802-372C-47BE-82B1-96F50DB5169E", @"False" ); // Facilities Request:Assign Worker:Activate Open Activity:Active
            RockMigrationHelper.AddActionTypeAttributeValue( "566F71EC-2082-4813-AEB0-C6BA19C002D9", "02D5A7A5-8781-46B4-B9FC-AF816829D240", @"8226D12E-654A-41CF-B8B9-E4DFC6470402" ); // Facilities Request:Assign Worker:Activate Open Activity:Activity
            RockMigrationHelper.AddActionTypeAttributeValue( "566F71EC-2082-4813-AEB0-C6BA19C002D9", "3809A78C-B773-440C-8E3F-A8E81D0DAE08", @"" ); // Facilities Request:Assign Worker:Activate Open Activity:Order
            RockMigrationHelper.AddActionTypeAttributeValue( "D0E5B32D-90EA-44DE-A5ED-CA78C6396B24", "E0F7AB7E-7761-4600-A099-CB14ACDBF6EF", @"False" ); // Facilities Request:Open:Assign Activity to Worker:Active
            RockMigrationHelper.AddActionTypeAttributeValue( "D0E5B32D-90EA-44DE-A5ED-CA78C6396B24", "FBADD25F-D309-4512-8430-3CC8615DD60E", @"e0d71111-05f1-4cd3-959e-1d246613942e" ); // Facilities Request:Open:Assign Activity to Worker:Attribute
            RockMigrationHelper.AddActionTypeAttributeValue( "D0E5B32D-90EA-44DE-A5ED-CA78C6396B24", "7A6B605D-7FB1-4F48-AF35-5A0683FB1CDA", @"" ); // Facilities Request:Open:Assign Activity to Worker:Order
            RockMigrationHelper.AddActionTypeAttributeValue( "3CEB73EA-7E14-45D0-8315-49AB784AB088", "E0F7AB7E-7761-4600-A099-CB14ACDBF6EF", @"False" ); // IT Support:Open:Assign Activity to Worker:Active
            RockMigrationHelper.AddActionTypeAttributeValue( "3CEB73EA-7E14-45D0-8315-49AB784AB088", "FBADD25F-D309-4512-8430-3CC8615DD60E", @"9d165825-21f2-4b5f-9a64-790a6ebd51ac" ); // IT Support:Open:Assign Activity to Worker:Attribute
            RockMigrationHelper.AddActionTypeAttributeValue( "3CEB73EA-7E14-45D0-8315-49AB784AB088", "7A6B605D-7FB1-4F48-AF35-5A0683FB1CDA", @"" ); // IT Support:Open:Assign Activity to Worker:Order
            RockMigrationHelper.AddActionTypeAttributeValue( "D8E9AF9C-1280-4ABB-8F96-7BF62F516A5B", "0A800013-51F7-4902-885A-5BE215D67D3D", @"False" ); // Person Data Error:Pending:Rename the Workflow:Active
            RockMigrationHelper.AddActionTypeAttributeValue( "D8E9AF9C-1280-4ABB-8F96-7BF62F516A5B", "5D95C15A-CCAE-40AD-A9DD-F929DA587115", @"" ); // Person Data Error:Pending:Rename the Workflow:Order
            RockMigrationHelper.AddActionTypeAttributeValue( "1BEA6BB6-C4C2-4254-B3E6-744BB4249620", "CE28B79D-FBC2-4894-9198-D923D0217549", @"False" ); // External Inquiry:Login Inquiry:Assign Worker:Active
            RockMigrationHelper.AddActionTypeAttributeValue( "1BEA6BB6-C4C2-4254-B3E6-744BB4249620", "7AC47975-71AC-4A2F-BF1F-115CF5578D6F", @"04712fbd-715d-412e-96c3-10c748482d6e" ); // External Inquiry:Login Inquiry:Assign Worker:Attribute
            RockMigrationHelper.AddActionTypeAttributeValue( "1BEA6BB6-C4C2-4254-B3E6-744BB4249620", "18EF907D-607E-4891-B034-7AA379D77854", @"" ); // External Inquiry:Login Inquiry:Assign Worker:Order
            RockMigrationHelper.AddActionTypePersonAttributeValue( "1BEA6BB6-C4C2-4254-B3E6-744BB4249620", "5C803BD1-40FA-49B1-AE7E-68F43D3687BB", @"d565a6c7-b789-4490-8f7f-253671dc2a8e" ); // External Inquiry:Login Inquiry:Assign Worker:Person
            RockMigrationHelper.AddActionTypeAttributeValue( "D8E9AF9C-1280-4ABB-8F96-7BF62F516A5B", "93852244-A667-4749-961A-D47F88675BE4", @"fd89f2c8-cbc8-4ed1-96d1-891ab9616c9e" ); // Person Data Error:Pending:Rename the Workflow:Text Value|Attribute Value
            RockMigrationHelper.AddActionTypeAttributeValue( "F6311295-5E9E-4521-A92D-71DAFCCDD4B6", "0B768E17-C64A-4212-BAD5-8A16B9F05A5C", @"False" ); // Person Data Error:Pending:Assign Worker:Active
            RockMigrationHelper.AddActionTypeAttributeValue( "F6311295-5E9E-4521-A92D-71DAFCCDD4B6", "5C5F7DB4-51DE-4293-BD73-CABDEB6564AC", @"" ); // Person Data Error:Pending:Assign Worker:Order
            RockMigrationHelper.AddActionTypePersonAttributeValue( "F6311295-5E9E-4521-A92D-71DAFCCDD4B6", "7ED2571D-B1BF-4DB6-9D04-9B5D064F51D8", @"d565a6c7-b789-4490-8f7f-253671dc2a8e" ); // Person Data Error:Pending:Assign Worker:Person
            RockMigrationHelper.AddActionTypeAttributeValue( "D2316CDF-8EF8-4960-BD98-5332CFAE71A7", "234910F2-A0DB-4D7D-BAF7-83C880EF30AE", @"False" ); // IT Support:Open:Capture Notes:Active
            RockMigrationHelper.AddActionTypeAttributeValue( "D2316CDF-8EF8-4960-BD98-5332CFAE71A7", "C178113D-7C86-4229-8424-C6D0CF4A7E23", @"" ); // IT Support:Open:Capture Notes:Order
            RockMigrationHelper.AddActionTypeAttributeValue( "A70C039B-BD86-49BF-9640-51C6B2F99887", "234910F2-A0DB-4D7D-BAF7-83C880EF30AE", @"False" ); // Facilities Request:Open:Capture Notes:Active
            RockMigrationHelper.AddActionTypeAttributeValue( "A70C039B-BD86-49BF-9640-51C6B2F99887", "C178113D-7C86-4229-8424-C6D0CF4A7E23", @"" ); // Facilities Request:Open:Capture Notes:Order
            RockMigrationHelper.AddActionTypeAttributeValue( "49C04C16-D80C-4371-90FD-E065581F5780", "E8ABD802-372C-47BE-82B1-96F50DB5169E", @"False" ); // External Inquiry:Login Inquiry:Open:Active
            RockMigrationHelper.AddActionTypeAttributeValue( "49C04C16-D80C-4371-90FD-E065581F5780", "02D5A7A5-8781-46B4-B9FC-AF816829D240", @"074b237a-348c-4bd6-be1e-0eddc4f376e9" ); // External Inquiry:Login Inquiry:Open:Activity
            RockMigrationHelper.AddActionTypeAttributeValue( "49C04C16-D80C-4371-90FD-E065581F5780", "3809A78C-B773-440C-8E3F-A8E81D0DAE08", @"" ); // External Inquiry:Login Inquiry:Open:Order
            RockMigrationHelper.AddActionTypeAttributeValue( "92E64B7D-5F6D-4B89-9C2B-50038062EB8E", "E8ABD802-372C-47BE-82B1-96F50DB5169E", @"False" ); // Facilities Request:Open:Done:Active
            RockMigrationHelper.AddActionTypeAttributeValue( "92E64B7D-5F6D-4B89-9C2B-50038062EB8E", "02D5A7A5-8781-46B4-B9FC-AF816829D240", @"EBE339D7-53AD-45E8-8610-19C01BA1A0AB" ); // Facilities Request:Open:Done:Activity
            RockMigrationHelper.AddActionTypeAttributeValue( "92E64B7D-5F6D-4B89-9C2B-50038062EB8E", "3809A78C-B773-440C-8E3F-A8E81D0DAE08", @"" ); // Facilities Request:Open:Done:Order
            RockMigrationHelper.AddActionTypeAttributeValue( "F90FEBD6-B1E0-445C-AC9F-F4D53EE09227", "E8ABD802-372C-47BE-82B1-96F50DB5169E", @"False" ); // IT Support:Open:Done:Active
            RockMigrationHelper.AddActionTypeAttributeValue( "F90FEBD6-B1E0-445C-AC9F-F4D53EE09227", "02D5A7A5-8781-46B4-B9FC-AF816829D240", @"60D58F02-1D58-40FF-AA40-1637CC8F8DC1" ); // IT Support:Open:Done:Activity
            RockMigrationHelper.AddActionTypeAttributeValue( "F90FEBD6-B1E0-445C-AC9F-F4D53EE09227", "3809A78C-B773-440C-8E3F-A8E81D0DAE08", @"" ); // IT Support:Open:Done:Order
            RockMigrationHelper.AddActionTypeAttributeValue( "6AF4C1E2-461A-432B-9184-068D08DF8F49", "234910F2-A0DB-4D7D-BAF7-83C880EF30AE", @"False" ); // Person Data Error:Pending:Capture the Resolution:Active
            RockMigrationHelper.AddActionTypeAttributeValue( "6AF4C1E2-461A-432B-9184-068D08DF8F49", "C178113D-7C86-4229-8424-C6D0CF4A7E23", @"" ); // Person Data Error:Pending:Capture the Resolution:Order
            RockMigrationHelper.AddActionTypeAttributeValue( "F6BCA594-3CBE-45F4-A13D-7CE380FF8C9C", "E8ABD802-372C-47BE-82B1-96F50DB5169E", @"False" ); // IT Support:Open:Assign New Worker:Active
            RockMigrationHelper.AddActionTypeAttributeValue( "F6BCA594-3CBE-45F4-A13D-7CE380FF8C9C", "02D5A7A5-8781-46B4-B9FC-AF816829D240", @"9AC84ADE-DAD2-4045-B27D-232B248C9457" ); // IT Support:Open:Assign New Worker:Activity
            RockMigrationHelper.AddActionTypeAttributeValue( "F6BCA594-3CBE-45F4-A13D-7CE380FF8C9C", "3809A78C-B773-440C-8E3F-A8E81D0DAE08", @"" ); // IT Support:Open:Assign New Worker:Order
            RockMigrationHelper.AddActionTypeAttributeValue( "477FEB2F-8B83-4D89-BAFC-6161678B2069", "E8ABD802-372C-47BE-82B1-96F50DB5169E", @"False" ); // Facilities Request:Open:Assign New Worker:Active
            RockMigrationHelper.AddActionTypeAttributeValue( "477FEB2F-8B83-4D89-BAFC-6161678B2069", "02D5A7A5-8781-46B4-B9FC-AF816829D240", @"FCB5C4EC-E25B-45D2-88E1-51F525A073A4" ); // Facilities Request:Open:Assign New Worker:Activity
            RockMigrationHelper.AddActionTypeAttributeValue( "477FEB2F-8B83-4D89-BAFC-6161678B2069", "3809A78C-B773-440C-8E3F-A8E81D0DAE08", @"" ); // Facilities Request:Open:Assign New Worker:Order
            RockMigrationHelper.AddActionTypeAttributeValue( "66F30491-F718-4DE9-90C6-2CB6F5F8C433", "A134F1A7-3824-43E0-9EB1-22C899B795BD", @"False" ); // Facilities Request:Open:Re-Activate This Activity:Active
            RockMigrationHelper.AddActionTypeAttributeValue( "66F30491-F718-4DE9-90C6-2CB6F5F8C433", "5DA71523-E8B0-4C4D-89A4-B47945A22A0C", @"" ); // Facilities Request:Open:Re-Activate This Activity:Order
            RockMigrationHelper.AddActionTypeAttributeValue( "3314F559-79D3-4F88-B1DB-C2E12BAA04A3", "A134F1A7-3824-43E0-9EB1-22C899B795BD", @"False" ); // IT Support:Open:Re-Activate This Activity:Active
            RockMigrationHelper.AddActionTypeAttributeValue( "3314F559-79D3-4F88-B1DB-C2E12BAA04A3", "5DA71523-E8B0-4C4D-89A4-B47945A22A0C", @"" ); // IT Support:Open:Re-Activate This Activity:Order
            RockMigrationHelper.AddActionTypeAttributeValue( "7379B7B2-EFA1-4D2C-9905-ED6E594FCC05", "E0F7AB7E-7761-4600-A099-CB14ACDBF6EF", @"False" ); // IT Support:Enter Resolution:Assign Activity:Active
            RockMigrationHelper.AddActionTypeAttributeValue( "7379B7B2-EFA1-4D2C-9905-ED6E594FCC05", "FBADD25F-D309-4512-8430-3CC8615DD60E", @"9d165825-21f2-4b5f-9a64-790a6ebd51ac" ); // IT Support:Enter Resolution:Assign Activity:Attribute
            RockMigrationHelper.AddActionTypeAttributeValue( "7379B7B2-EFA1-4D2C-9905-ED6E594FCC05", "7A6B605D-7FB1-4F48-AF35-5A0683FB1CDA", @"" ); // IT Support:Enter Resolution:Assign Activity:Order
            RockMigrationHelper.AddActionTypeAttributeValue( "AD587683-6893-4837-ACE7-4CB44CC646DE", "E0F7AB7E-7761-4600-A099-CB14ACDBF6EF", @"False" ); // Facilities Request:Enter Resolution:Assign Activity:Active
            RockMigrationHelper.AddActionTypeAttributeValue( "AD587683-6893-4837-ACE7-4CB44CC646DE", "FBADD25F-D309-4512-8430-3CC8615DD60E", @"e0d71111-05f1-4cd3-959e-1d246613942e" ); // Facilities Request:Enter Resolution:Assign Activity:Attribute
            RockMigrationHelper.AddActionTypeAttributeValue( "AD587683-6893-4837-ACE7-4CB44CC646DE", "7A6B605D-7FB1-4F48-AF35-5A0683FB1CDA", @"" ); // Facilities Request:Enter Resolution:Assign Activity:Order
            RockMigrationHelper.AddActionTypeAttributeValue( "84C85F3B-E913-4302-B61E-BE04EB668133", "36197160-7D3D-490D-AB42-7E29105AFE91", @"False" ); // Person Data Error:Complete:Notify Originator:Active
            RockMigrationHelper.AddActionTypeAttributeValue( "84C85F3B-E913-4302-B61E-BE04EB668133", "D1269254-C15A-40BD-B784-ADCC231D3950", @"" ); // Person Data Error:Complete:Notify Originator:Order
            RockMigrationHelper.AddActionTypeAttributeValue( "84C85F3B-E913-4302-B61E-BE04EB668133", "0C4C13B8-7076-4872-925A-F950886B5E16", @"c8fd97b5-adc0-4ef0-b397-e4010850148c" ); // Person Data Error:Complete:Notify Originator:Send To Email Address|Attribute Value
            RockMigrationHelper.AddActionTypeAttributeValue( "74733C1D-6939-41BC-86DD-CCE5B345CC7C", "CE28B79D-FBC2-4894-9198-D923D0217549", @"False" ); // External Inquiry:Website Inquiry:Assign Worker:Active
            RockMigrationHelper.AddActionTypeAttributeValue( "74733C1D-6939-41BC-86DD-CCE5B345CC7C", "7AC47975-71AC-4A2F-BF1F-115CF5578D6F", @"04712fbd-715d-412e-96c3-10c748482d6e" ); // External Inquiry:Website Inquiry:Assign Worker:Attribute
            RockMigrationHelper.AddActionTypeAttributeValue( "74733C1D-6939-41BC-86DD-CCE5B345CC7C", "18EF907D-607E-4891-B034-7AA379D77854", @"" ); // External Inquiry:Website Inquiry:Assign Worker:Order
            RockMigrationHelper.AddActionTypePersonAttributeValue( "74733C1D-6939-41BC-86DD-CCE5B345CC7C", "5C803BD1-40FA-49B1-AE7E-68F43D3687BB", @"d565a6c7-b789-4490-8f7f-253671dc2a8e" ); // External Inquiry:Website Inquiry:Assign Worker:Person
            RockMigrationHelper.AddActionTypeAttributeValue( "84C85F3B-E913-4302-B61E-BE04EB668133", "9F5F7CEC-F369-4FDF-802A-99074CE7A7FC", @"" ); // Person Data Error:Complete:Notify Originator:From
            RockMigrationHelper.AddActionTypeAttributeValue( "84C85F3B-E913-4302-B61E-BE04EB668133", "5D9B13B6-CD96-4C7C-86FA-4512B9D28386", @"Data Error for {{ Workflow.Name }} has been completed" ); // Person Data Error:Complete:Notify Originator:Subject
            RockMigrationHelper.AddActionTypeAttributeValue( "84C85F3B-E913-4302-B61E-BE04EB668133", "4D245B9E-6B03-46E7-8482-A51FBA190E4D", @"{{ GlobalAttribute.EmailHeader }}

<p>{{ Workflow.ReportedBy }},</p>
<p>The data error that you reported for {{ Workflow.Name }} has been completed.<p>

<h4>Details:</h4>
<p>{{ Workflow.Details }}</p>

{% if Workflow.Resolution != Empty %}

    <h4>Resolution:</h4>
    <p>{{ Workflow.Resolution }}</p>

{% endif %}

{{ GlobalAttribute.EmailFooter }}

" ); // Person Data Error:Complete:Notify Originator:Body
            RockMigrationHelper.AddActionTypeAttributeValue( "0D1D7EB5-EADF-4FC1-87AA-7793202E21F6", "0CA0DDEF-48EF-4ABC-9822-A05E225DE26C", @"False" ); // Person Data Error:Complete:Complete the workflow:Active
            RockMigrationHelper.AddActionTypeAttributeValue( "0D1D7EB5-EADF-4FC1-87AA-7793202E21F6", "25CAD4BE-5A00-409D-9BAB-E32518D89956", @"" ); // Person Data Error:Complete:Complete the workflow:Order
            RockMigrationHelper.AddActionTypeAttributeValue( "8D56FFFE-6EF7-4389-97CE-F02BDE8A1074", "234910F2-A0DB-4D7D-BAF7-83C880EF30AE", @"False" ); // Facilities Request:Enter Resolution:Enter Resolution:Active
            RockMigrationHelper.AddActionTypeAttributeValue( "8D56FFFE-6EF7-4389-97CE-F02BDE8A1074", "C178113D-7C86-4229-8424-C6D0CF4A7E23", @"" ); // Facilities Request:Enter Resolution:Enter Resolution:Order
            RockMigrationHelper.AddActionTypeAttributeValue( "247BA14B-2313-4ED9-9F3D-F0FBABB67CEC", "234910F2-A0DB-4D7D-BAF7-83C880EF30AE", @"False" ); // IT Support:Enter Resolution:Enter Resolution:Active
            RockMigrationHelper.AddActionTypeAttributeValue( "247BA14B-2313-4ED9-9F3D-F0FBABB67CEC", "C178113D-7C86-4229-8424-C6D0CF4A7E23", @"" ); // IT Support:Enter Resolution:Enter Resolution:Order
            RockMigrationHelper.AddActionTypeAttributeValue( "8B1C0348-76D4-404D-BB5D-665F834FF07E", "E8ABD802-372C-47BE-82B1-96F50DB5169E", @"False" ); // External Inquiry:Website Inquiry:Open:Active
            RockMigrationHelper.AddActionTypeAttributeValue( "8B1C0348-76D4-404D-BB5D-665F834FF07E", "02D5A7A5-8781-46B4-B9FC-AF816829D240", @"2b912003-91e7-43b6-8060-a5ed97c4edee" ); // External Inquiry:Website Inquiry:Open:Activity
            RockMigrationHelper.AddActionTypeAttributeValue( "8B1C0348-76D4-404D-BB5D-665F834FF07E", "3809A78C-B773-440C-8E3F-A8E81D0DAE08", @"" ); // External Inquiry:Website Inquiry:Open:Order
            RockMigrationHelper.AddActionTypeAttributeValue( "15D058D6-81FC-449E-8090-DD8AAFF533C0", "36197160-7D3D-490D-AB42-7E29105AFE91", @"False" ); // IT Support:Complete:Notify Requester:Active
            RockMigrationHelper.AddActionTypeAttributeValue( "15D058D6-81FC-449E-8090-DD8AAFF533C0", "D1269254-C15A-40BD-B784-ADCC231D3950", @"" ); // IT Support:Complete:Notify Requester:Order
            RockMigrationHelper.AddActionTypeAttributeValue( "15D058D6-81FC-449E-8090-DD8AAFF533C0", "0C4C13B8-7076-4872-925A-F950886B5E16", @"ce01e29e-4a9c-4bbf-bbcd-235713a81353" ); // IT Support:Complete:Notify Requester:Send To Email Address|Attribute Value
            RockMigrationHelper.AddActionTypeAttributeValue( "6EC85180-3513-419A-AAA1-E4A37ECB94A1", "36197160-7D3D-490D-AB42-7E29105AFE91", @"False" ); // Facilities Request:Complete:Notify Requester:Active
            RockMigrationHelper.AddActionTypeAttributeValue( "6EC85180-3513-419A-AAA1-E4A37ECB94A1", "D1269254-C15A-40BD-B784-ADCC231D3950", @"" ); // Facilities Request:Complete:Notify Requester:Order
            RockMigrationHelper.AddActionTypeAttributeValue( "6EC85180-3513-419A-AAA1-E4A37ECB94A1", "0C4C13B8-7076-4872-925A-F950886B5E16", @"594d522b-1e69-4175-9c6b-039c6baf279e" ); // Facilities Request:Complete:Notify Requester:Send To Email Address|Attribute Value
            RockMigrationHelper.AddActionTypeAttributeValue( "3C57B99B-7591-4B06-8334-BE8098C32B3B", "CE28B79D-FBC2-4894-9198-D923D0217549", @"False" ); // External Inquiry:Finance Inquiry:Assign Worker:Active
            RockMigrationHelper.AddActionTypeAttributeValue( "3C57B99B-7591-4B06-8334-BE8098C32B3B", "7AC47975-71AC-4A2F-BF1F-115CF5578D6F", @"04712fbd-715d-412e-96c3-10c748482d6e" ); // External Inquiry:Finance Inquiry:Assign Worker:Attribute
            RockMigrationHelper.AddActionTypeAttributeValue( "3C57B99B-7591-4B06-8334-BE8098C32B3B", "18EF907D-607E-4891-B034-7AA379D77854", @"" ); // External Inquiry:Finance Inquiry:Assign Worker:Order
            RockMigrationHelper.AddActionTypePersonAttributeValue( "3C57B99B-7591-4B06-8334-BE8098C32B3B", "5C803BD1-40FA-49B1-AE7E-68F43D3687BB", @"d565a6c7-b789-4490-8f7f-253671dc2a8e" ); // External Inquiry:Finance Inquiry:Assign Worker:Person
            RockMigrationHelper.AddActionTypeAttributeValue( "6EC85180-3513-419A-AAA1-E4A37ECB94A1", "9F5F7CEC-F369-4FDF-802A-99074CE7A7FC", @"" ); // Facilities Request:Complete:Notify Requester:From
            RockMigrationHelper.AddActionTypeAttributeValue( "15D058D6-81FC-449E-8090-DD8AAFF533C0", "9F5F7CEC-F369-4FDF-802A-99074CE7A7FC", @"" ); // IT Support:Complete:Notify Requester:From
            RockMigrationHelper.AddActionTypeAttributeValue( "15D058D6-81FC-449E-8090-DD8AAFF533C0", "5D9B13B6-CD96-4C7C-86FA-4512B9D28386", @"IT Support Request Completed" ); // IT Support:Complete:Notify Requester:Subject
            RockMigrationHelper.AddActionTypeAttributeValue( "6EC85180-3513-419A-AAA1-E4A37ECB94A1", "5D9B13B6-CD96-4C7C-86FA-4512B9D28386", @"Facilities Request Completed" ); // Facilities Request:Complete:Notify Requester:Subject
            RockMigrationHelper.AddActionTypeAttributeValue( "6EC85180-3513-419A-AAA1-E4A37ECB94A1", "4D245B9E-6B03-46E7-8482-A51FBA190E4D", @"{{ GlobalAttribute.EmailHeader }}

<p>The following Facilities Request has been completed by {{ Workflow.Worker }}:</p>

<h4>{{ Workflow.Name }}</h4>
<p>{{ Workflow.Details }}</p>

<h4>Resolution</h4>
<p>{{ Workflow.Resolution }}</p>

{{ GlobalAttribute.EmailFooter }}

" ); // Facilities Request:Complete:Notify Requester:Body
            RockMigrationHelper.AddActionTypeAttributeValue( "15D058D6-81FC-449E-8090-DD8AAFF533C0", "4D245B9E-6B03-46E7-8482-A51FBA190E4D", @"{{ GlobalAttribute.EmailHeader }}

<p>The following IT Support Request has been completed by {{ Workflow.Worker }}:</p>

<h4>{{ Workflow.Name }}</h4>
<p>{{ Workflow.Details }}</p>

<h4>Resolution</h4>
<p>{{ Workflow.Resolution }}</p>

{{ GlobalAttribute.EmailFooter }}

" ); // IT Support:Complete:Notify Requester:Body
            RockMigrationHelper.AddActionTypeAttributeValue( "A0400B10-3CEA-4124-A646-490E12CEDEC7", "0CA0DDEF-48EF-4ABC-9822-A05E225DE26C", @"False" ); // IT Support:Complete:Complete the Workflow:Active
            RockMigrationHelper.AddActionTypeAttributeValue( "A0400B10-3CEA-4124-A646-490E12CEDEC7", "25CAD4BE-5A00-409D-9BAB-E32518D89956", @"" ); // IT Support:Complete:Complete the Workflow:Order
            RockMigrationHelper.AddActionTypeAttributeValue( "F19E257B-E4AE-4E86-9D54-54B2DEC0A786", "0CA0DDEF-48EF-4ABC-9822-A05E225DE26C", @"False" ); // Facilities Request:Complete:Complete the Workflow:Active
            RockMigrationHelper.AddActionTypeAttributeValue( "F19E257B-E4AE-4E86-9D54-54B2DEC0A786", "25CAD4BE-5A00-409D-9BAB-E32518D89956", @"" ); // Facilities Request:Complete:Complete the Workflow:Order
            RockMigrationHelper.AddActionTypeAttributeValue( "DCF172EF-DBFD-4749-AC3C-A61E5361AD4E", "E8ABD802-372C-47BE-82B1-96F50DB5169E", @"False" ); // External Inquiry:Finance Inquiry:Open:Active
            RockMigrationHelper.AddActionTypeAttributeValue( "DCF172EF-DBFD-4749-AC3C-A61E5361AD4E", "02D5A7A5-8781-46B4-B9FC-AF816829D240", @"2b912003-91e7-43b6-8060-a5ed97c4edee" ); // External Inquiry:Finance Inquiry:Open:Activity
            RockMigrationHelper.AddActionTypeAttributeValue( "DCF172EF-DBFD-4749-AC3C-A61E5361AD4E", "3809A78C-B773-440C-8E3F-A8E81D0DAE08", @"" ); // External Inquiry:Finance Inquiry:Open:Order
            RockMigrationHelper.AddActionTypeAttributeValue( "70737135-9FB0-41C5-8470-1BB94BEFEF16", "CE28B79D-FBC2-4894-9198-D923D0217549", @"False" ); // External Inquiry:Missions Inquiry:Assign Worker:Active
            RockMigrationHelper.AddActionTypeAttributeValue( "70737135-9FB0-41C5-8470-1BB94BEFEF16", "7AC47975-71AC-4A2F-BF1F-115CF5578D6F", @"04712fbd-715d-412e-96c3-10c748482d6e" ); // External Inquiry:Missions Inquiry:Assign Worker:Attribute
            RockMigrationHelper.AddActionTypeAttributeValue( "70737135-9FB0-41C5-8470-1BB94BEFEF16", "18EF907D-607E-4891-B034-7AA379D77854", @"" ); // External Inquiry:Missions Inquiry:Assign Worker:Order
            RockMigrationHelper.AddActionTypePersonAttributeValue( "70737135-9FB0-41C5-8470-1BB94BEFEF16", "5C803BD1-40FA-49B1-AE7E-68F43D3687BB", @"d565a6c7-b789-4490-8f7f-253671dc2a8e" ); // External Inquiry:Missions Inquiry:Assign Worker:Person
            RockMigrationHelper.AddActionTypeAttributeValue( "DB6A3EE9-9280-4CFB-BC9B-337946FA7A66", "E8ABD802-372C-47BE-82B1-96F50DB5169E", @"False" ); // External Inquiry:Missions Inquiry:Open:Active
            RockMigrationHelper.AddActionTypeAttributeValue( "DB6A3EE9-9280-4CFB-BC9B-337946FA7A66", "02D5A7A5-8781-46B4-B9FC-AF816829D240", @"2b912003-91e7-43b6-8060-a5ed97c4edee" ); // External Inquiry:Missions Inquiry:Open:Activity
            RockMigrationHelper.AddActionTypeAttributeValue( "DB6A3EE9-9280-4CFB-BC9B-337946FA7A66", "3809A78C-B773-440C-8E3F-A8E81D0DAE08", @"" ); // External Inquiry:Missions Inquiry:Open:Order
            RockMigrationHelper.AddActionTypeAttributeValue( "50D07904-5523-494C-BD11-62015C67CBE6", "CE28B79D-FBC2-4894-9198-D923D0217549", @"False" ); // External Inquiry:Pastoral Inquiry:Assign Worker:Active
            RockMigrationHelper.AddActionTypeAttributeValue( "50D07904-5523-494C-BD11-62015C67CBE6", "7AC47975-71AC-4A2F-BF1F-115CF5578D6F", @"04712fbd-715d-412e-96c3-10c748482d6e" ); // External Inquiry:Pastoral Inquiry:Assign Worker:Attribute
            RockMigrationHelper.AddActionTypeAttributeValue( "50D07904-5523-494C-BD11-62015C67CBE6", "18EF907D-607E-4891-B034-7AA379D77854", @"" ); // External Inquiry:Pastoral Inquiry:Assign Worker:Order
            RockMigrationHelper.AddActionTypePersonAttributeValue( "50D07904-5523-494C-BD11-62015C67CBE6", "5C803BD1-40FA-49B1-AE7E-68F43D3687BB", @"d565a6c7-b789-4490-8f7f-253671dc2a8e" ); // External Inquiry:Pastoral Inquiry:Assign Worker:Person
            RockMigrationHelper.AddActionTypeAttributeValue( "BDE13807-217D-4EC6-923E-8536EB5F3443", "E8ABD802-372C-47BE-82B1-96F50DB5169E", @"False" ); // External Inquiry:Pastoral Inquiry:Open:Active
            RockMigrationHelper.AddActionTypeAttributeValue( "BDE13807-217D-4EC6-923E-8536EB5F3443", "02D5A7A5-8781-46B4-B9FC-AF816829D240", @"2b912003-91e7-43b6-8060-a5ed97c4edee" ); // External Inquiry:Pastoral Inquiry:Open:Activity
            RockMigrationHelper.AddActionTypeAttributeValue( "BDE13807-217D-4EC6-923E-8536EB5F3443", "3809A78C-B773-440C-8E3F-A8E81D0DAE08", @"" ); // External Inquiry:Pastoral Inquiry:Open:Order
            RockMigrationHelper.AddActionTypeAttributeValue( "5B37C3A6-5579-48DE-BDF7-C8F61C96A32A", "E0F7AB7E-7761-4600-A099-CB14ACDBF6EF", @"False" ); // External Inquiry:Open:Assign Activity to Worker:Active
            RockMigrationHelper.AddActionTypeAttributeValue( "5B37C3A6-5579-48DE-BDF7-C8F61C96A32A", "FBADD25F-D309-4512-8430-3CC8615DD60E", @"04712fbd-715d-412e-96c3-10c748482d6e" ); // External Inquiry:Open:Assign Activity to Worker:Attribute
            RockMigrationHelper.AddActionTypeAttributeValue( "5B37C3A6-5579-48DE-BDF7-C8F61C96A32A", "7A6B605D-7FB1-4F48-AF35-5A0683FB1CDA", @"" ); // External Inquiry:Open:Assign Activity to Worker:Order
            RockMigrationHelper.AddActionTypeAttributeValue( "6261A690-CFFF-4CD6-9258-C9BCA78163F1", "234910F2-A0DB-4D7D-BAF7-83C880EF30AE", @"False" ); // External Inquiry:Open:Capture Notes:Active
            RockMigrationHelper.AddActionTypeAttributeValue( "6261A690-CFFF-4CD6-9258-C9BCA78163F1", "C178113D-7C86-4229-8424-C6D0CF4A7E23", @"" ); // External Inquiry:Open:Capture Notes:Order
            RockMigrationHelper.AddActionTypeAttributeValue( "586D24C8-3BE0-4638-B32F-170C9853F51D", "E8ABD802-372C-47BE-82B1-96F50DB5169E", @"False" ); // External Inquiry:Open:Assign New Worker:Active
            RockMigrationHelper.AddActionTypeAttributeValue( "586D24C8-3BE0-4638-B32F-170C9853F51D", "02D5A7A5-8781-46B4-B9FC-AF816829D240", @"679fe471-517b-4278-bd43-5eca8340f5c5" ); // External Inquiry:Open:Assign New Worker:Activity
            RockMigrationHelper.AddActionTypeAttributeValue( "586D24C8-3BE0-4638-B32F-170C9853F51D", "3809A78C-B773-440C-8E3F-A8E81D0DAE08", @"" ); // External Inquiry:Open:Assign New Worker:Order
            RockMigrationHelper.AddActionTypeAttributeValue( "3CB3F9B5-C1B5-4E4E-BBFB-0801521E62E1", "A134F1A7-3824-43E0-9EB1-22C899B795BD", @"False" ); // External Inquiry:Open:Re-Activate These Actions:Active
            RockMigrationHelper.AddActionTypeAttributeValue( "3CB3F9B5-C1B5-4E4E-BBFB-0801521E62E1", "5DA71523-E8B0-4C4D-89A4-B47945A22A0C", @"" ); // External Inquiry:Open:Re-Activate These Actions:Order
            RockMigrationHelper.AddActionTypeAttributeValue( "63CD1086-3505-47C9-9A10-4CDDB99D06B1", "0CA0DDEF-48EF-4ABC-9822-A05E225DE26C", @"False" ); // External Inquiry:Complete:Complete Workflow:Active
            RockMigrationHelper.AddActionTypeAttributeValue( "63CD1086-3505-47C9-9A10-4CDDB99D06B1", "25CAD4BE-5A00-409D-9BAB-E32518D89956", @"" ); // External Inquiry:Complete:Complete Workflow:Order
            RockMigrationHelper.AddActionTypeAttributeValue( "0E268839-D6BD-43A7-BAAC-A2ADEA12C5F1", "D7EAA859-F500-4521-9523-488B12EAA7D2", @"False" ); // External Inquiry:Re-assign Worker:Set Worker:Active
            RockMigrationHelper.AddActionTypeAttributeValue( "0E268839-D6BD-43A7-BAAC-A2ADEA12C5F1", "44A0B977-4730-4519-8FF6-B0A01A95B212", @"04712fbd-715d-412e-96c3-10c748482d6e" ); // External Inquiry:Re-assign Worker:Set Worker:Attribute
            RockMigrationHelper.AddActionTypeAttributeValue( "0E268839-D6BD-43A7-BAAC-A2ADEA12C5F1", "57093B41-50ED-48E5-B72B-8829E62704C8", @"" ); // External Inquiry:Re-assign Worker:Set Worker:Order
            RockMigrationHelper.AddActionTypeAttributeValue( "0E268839-D6BD-43A7-BAAC-A2ADEA12C5F1", "E5272B11-A2B8-49DC-860D-8D574E2BC15C", @"56a33db7-fac1-41f9-b42d-cc24d7a20466" ); // External Inquiry:Re-assign Worker:Set Worker:Text Value|Attribute Value
            RockMigrationHelper.AddActionTypeAttributeValue( "F4F60ABF-18E2-45EB-803A-E7716522CCC7", "CE28B79D-FBC2-4894-9198-D923D0217549", @"False" ); // External Inquiry:Re-assign Worker:Clear New Worker Value:Active
            RockMigrationHelper.AddActionTypeAttributeValue( "F4F60ABF-18E2-45EB-803A-E7716522CCC7", "7AC47975-71AC-4A2F-BF1F-115CF5578D6F", @"56a33db7-fac1-41f9-b42d-cc24d7a20466" ); // External Inquiry:Re-assign Worker:Clear New Worker Value:Attribute
            RockMigrationHelper.AddActionTypeAttributeValue( "F4F60ABF-18E2-45EB-803A-E7716522CCC7", "18EF907D-607E-4891-B034-7AA379D77854", @"" ); // External Inquiry:Re-assign Worker:Clear New Worker Value:Order
            RockMigrationHelper.AddActionTypePersonAttributeValue( "F4F60ABF-18E2-45EB-803A-E7716522CCC7", "5C803BD1-40FA-49B1-AE7E-68F43D3687BB", @"" ); // External Inquiry:Re-assign Worker:Clear New Worker Value:Person
            RockMigrationHelper.AddActionTypeAttributeValue( "4D4B2DF5-B4CD-4C1A-BEE1-3EBC63552349", "E8ABD802-372C-47BE-82B1-96F50DB5169E", @"False" ); // External Inquiry:Re-assign Worker:Re-Activate Open Activity:Active
            RockMigrationHelper.AddActionTypeAttributeValue( "4D4B2DF5-B4CD-4C1A-BEE1-3EBC63552349", "02D5A7A5-8781-46B4-B9FC-AF816829D240", @"2b912003-91e7-43b6-8060-a5ed97c4edee" ); // External Inquiry:Re-assign Worker:Re-Activate Open Activity:Activity
            RockMigrationHelper.AddActionTypeAttributeValue( "4D4B2DF5-B4CD-4C1A-BEE1-3EBC63552349", "3809A78C-B773-440C-8E3F-A8E81D0DAE08", @"" ); // External Inquiry:Re-assign Worker:Re-Activate Open Activity:Order

            #endregion

            Sql( @"
-- Update the Action attribute on person bio block
DECLARE @BlockId int = ( SELECT [Id] FROM [Block] WHERE [Guid] = 'B5C1FDB6-0224-43E4-8E26-6B2EAF86253A' )
DECLARE @AttributeId int = ( SELECT [Id] FROM [Attribute] WHERE [Guid] = '35F69669-48DE-4182-B828-4EC9C1C31B08' )
DECLARE @WorkflowTypeId int = (	SELECT [Id] FROM [WorkflowType] WHERE [Guid] = '221BF486-A82C-40A7-85B7-BB44DA45582F' )
UPDATE [AttributeValue] SET [Value] = '
	<li>
		<a href=''~/LaunchWorkflow/' + CONVERT(varchar, @WorkflowTypeId) + '?PersonId={0}'' tabindex=''0''>
			<i class=''fa fa-exclamation''></i>
			Report Data Error
		</a>
	</li>'
WHERE [AttributeId] = @AttributeId
AND [EntityId] = @BlockId
" );
        }

        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            Sql( @"
" );
            // Attrib for BlockType: Activate Workflow:Workflow Entry Page
            RockMigrationHelper.DeleteAttribute( "031DD6B2-7131-4BC7-9609-4FEED6D8AEA1" );
            // Attrib for BlockType: Person Bio:Actions
            RockMigrationHelper.DeleteAttribute( "35F69669-48DE-4182-B828-4EC9C1C31B08" );
            // Remove Block: Workflow Entry, from Page: Contact Us, Site: Rock Solid Church
            RockMigrationHelper.DeleteBlock( "CA7D13BB-6781-4908-9198-CF89E915F9D7" );
            // Remove Block: Activate Workflow, from Page: LaunchWorkflow, Site: Rock RMS
            RockMigrationHelper.DeleteBlock( "C0717EB6-80B2-48AF-AD86-EF46678F6780" );
            RockMigrationHelper.DeleteBlock( "240E241A-874B-453C-A98E-AEBADA722EC4" );

            RockMigrationHelper.DeletePage( "B1E63FE3-779C-4388-AFE4-FD6DFC034932" ); // Page: Contact UsLayout: FullWidth, Site: Rock Solid Church
            RockMigrationHelper.DeletePage( "5DA89BC9-A185-4749-A843-314B72170D82" ); // Page: LaunchWorkflowLayout: Full Width, Site: Rock RMS

            AddColumn( "dbo.WorkflowActionForm", "InactiveMessage", c => c.String() );
            DropColumn( "dbo.WorkflowActionForm", "ActionAttributeGuid" );
        }
    }
}
