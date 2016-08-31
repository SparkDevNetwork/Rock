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
    public partial class WorkflowFormNotification : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            // Add notification option fields to Workflow Action Type Form
            AddColumn("dbo.WorkflowActionForm", "NotificationSystemEmailId", c => c.Int());
            AddColumn("dbo.WorkflowActionForm", "IncludeActionsInNotification", c => c.Boolean(nullable: false));
            CreateIndex("dbo.WorkflowActionForm", "NotificationSystemEmailId");
            AddForeignKey("dbo.WorkflowActionForm", "NotificationSystemEmailId", "dbo.SystemEmail", "Id");

            // Add/Update Defined Type for ButtonHTML
            RockMigrationHelper.AddDefinedType_pre201409101843015( "Global", "Button HTML", "Contains HTML for common button styles.", "407A3A73-A3EF-4970-B856-2A33F62AC72E", @"" );

            Sql( @"
    DECLARE @AttributeId int = (SELECT [Id] FROM [Attribute] WHERE [Guid] = '6FF59F53-28EA-4BFE-AFE1-A459CC588495' )

    DECLARE @FieldTypeId int = (SELECT [Id] FROM [FieldType] WHERE [Guid] = '1D0D3794-C210-48A8-8C68-3FBEC08A6BA5')
    UPDATE [Attribute] SET
         [Name] = 'Button HTML'
        ,[Key] = 'ButtonHTML'
        ,[Description] = 'The HTML to use for displaying a button.  Both  ""{{ ButtonLink }}"" and ""{{ ButtonText }}"" merge fields are available to be used in the HTML as placeholder for the text and link values that will be rendered for the button.'
        ,[FieldTypeId] = @FieldTypeId
    WHERE [Id] = @AttributeId

    DECLARE @DefinedValueId int = (SELECT [ID] FROM [DefinedValue] WHERE [Guid] = 'FDC397CD-8B4A-436E-BEA1-BCE2E6717C03')
    UPDATE [AttributeValue] SET [Value] = '<a href=""{{ ButtonLink }}"" class=""btn btn-primary"">{{ ButtonText }}</a>'
	WHERE [AttributeId] = @AttributeId AND [EntityId] = CAST( @DefinedValueId AS varchar )

    SET @DefinedValueId = (SELECT [ID] FROM [DefinedValue] WHERE [Guid] = 'FDEB8E6C-70C3-4033-B307-7D0DEE1AC29D')
    UPDATE [AttributeValue] SET [Value] = '<a href=""{{ ButtonLink }}"" class=""btn btn-danger"">{{ ButtonText }}</a>'
	WHERE [AttributeId] = @AttributeId AND [EntityId] = CAST( @DefinedValueId AS varchar )
" );
            RockMigrationHelper.AddAttributeQualifier( "6FF59F53-28EA-4BFE-AFE1-A459CC588495", "editorMode", "2", "203785F4-60C1-4BD4-B2F4-CC125857A360" );
            RockMigrationHelper.AddAttributeQualifier( "6FF59F53-28EA-4BFE-AFE1-A459CC588495", "editorTheme", "0", "AFD572CD-B751-474F-945D-97DD1A3C7EDF" );
            RockMigrationHelper.AddAttributeQualifier( "6FF59F53-28EA-4BFE-AFE1-A459CC588495", "editorHeight", "", "7E21A104-50F6-4C93-943B-D1EBEE3444EE" );


            // Create Default System Email for workflow form notifications
            RockMigrationHelper.UpdateSystemEmail_pre201409101843015( "Workflow", "Workflow Form Notification", "", "", "", "", "", "{{ Workflow.WorkflowType.Name }}: {{ Workflow.Name }}", @"
{{ GlobalAttribute.EmailHeader }}

<p>{{ Person.FirstName }},</p>
<p>The following {{ Workflow.WorkflowType.Name }} requires action:<p>
<p>{{ Workflow.WorkflowType.WorkTerm}}: <a href='{{ GlobalAttribute.InternalApplicationRoot }}WorkflowEntry/{{ Workflow.WorkflowTypeId }}/{{ Workflow.Id }}'>{{ Workflow.Name }}</a></p>

{% assign RequiredFields = false %}

<h4>Details:</h4>
<p>
{% for attribute in Action.FormAttributes %}

    <strong>{{ attribute.Name }}:</strong> {{ attribute.Value }}<br/>

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

            // Add/update routes for workflow entry page
            RockMigrationHelper.AddPageRoute( "0550D2AA-A705-4400-81FF-AB124FDF83D7", "WorkflowEntry/{WorkflowTypeId}/{WorkflowId}" );
            RockMigrationHelper.AddPageRoute( "0550D2AA-A705-4400-81FF-AB124FDF83D7", "WorkflowEntry/{WorkflowTypeId}" );
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            DropForeignKey("dbo.WorkflowActionForm", "NotificationSystemEmailId", "dbo.SystemEmail");
            DropIndex("dbo.WorkflowActionForm", new[] { "NotificationSystemEmailId" });
            DropColumn("dbo.WorkflowActionForm", "IncludeActionsInNotification");
            DropColumn("dbo.WorkflowActionForm", "NotificationSystemEmailId");
        }
    }
}
