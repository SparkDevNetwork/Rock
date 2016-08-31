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
    public partial class HtmlButtonsForWorkflows : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            // delete the 'Red' button
            Sql( @"  DELETE FROM [DefinedValue]
                    WHERE [Guid] = 'FDEB8E6C-70C3-4033-B307-7D0DEE1AC29D'" );

            // update primary and secondary buttons
            Sql( @"
                DECLARE @AttributeButtonHtmlId int
                SET @AttributeButtonHtmlId = (SELECT TOP 1 [Id] FROM [Attribute] WHERE [Guid] = '6FF59F53-28EA-4BFE-AFE1-A459CC588495')

                DECLARE @PrimaryButtonId int
                SET @PrimaryButtonId = (SELECT TOP 1 [Id] FROM [DefinedValue] WHERE [Guid] = 'FDC397CD-8B4A-436E-BEA1-BCE2E6717C03')

                DECLARE @SecondaryButtonId int
                SET @SecondaryButtonId = (SELECT TOP 1 [Id] FROM [DefinedValue] WHERE [Guid] = '8CF6E927-4FA5-4241-991C-391038B79631')

                UPDATE [AttributeValue] 
	                SET [Value] = '
	                <div class=""btn-tabled"" style=""float: left; margin: 0 6px 12px 0"">
                <table width=""100%"" border=""0"" cellspacing=""0"" cellpadding=""0"">
                  <tr>
                    <td>
                      <table border=""0"" cellspacing=""0"" cellpadding=""0"">
                        <tr>
                          <td align=""center"" style=""
			                -webkit-border-radius: 4px; 
			                -moz-border-radius: 4px; 
			                border-radius: 4px;"" 
			                bgcolor=""#cf5e10"">
								
				                <a href=""{{ ButtonLink }}"" onclick=""{{ ButtonClick }}"" data-loading-text=""<i class=''fa fa-refresh fa-spin''></i> {{ ButtonText }}""
					                style=""	
						                font-size: 14px; 
						                font-family: OpenSans, ''Helvetica Neue'', Helvetica, Arial, sans-serif; 
						                color: #ffffff; 
						                text-decoration: none; 
						                -webkit-border-radius: 4px; 
						                -moz-border-radius: 4px; 
						                border-radius: 4px; 
						                padding: 6px 12px; 
						                border: 1px solid #ae4f0d; 
						                display: inline-block;"">
								                {{ ButtonText }}
				                </a>
			                </td>
                        </tr>
                      </table>
                    </td>
                  </tr>
                </table>
                </div>
	                '
	                WHERE [AttributeId] = @AttributeButtonHtmlId AND [EntityId] = @PrimaryButtonId

                UPDATE [AttributeValue] 
	                SET [Value] = '
                <div class=""btn-tabled"" style=""float: left; margin: 0 6px 12px 0"">
                <table width=""100%"" border=""0"" cellspacing=""0"" cellpadding=""0"">
                  <tr>
                    <td>
                      <table border=""0"" cellspacing=""0"" cellpadding=""0"">
                        <tr>
                          <td align=""center"" style="" 
			                bgcolor=""#ffffff"">
								
				                <a href=""{{ ButtonLink }}"" onclick=""{{ ButtonClick }}"" data-loading-text=""<i class=''fa fa-refresh fa-spin''></i> {{ ButtonText }}""
					                style=""	
						                font-size: 14px; 
						                font-family: OpenSans, ''Helvetica Neue'', Helvetica, Arial, sans-serif; 
						                color: #316078; 					
						                padding: 6px 12px; 
						                display: inline-block;"">
								                {{ ButtonText }}
				                </a>
			                </td>
                        </tr>
                      </table>
                    </td>
                  </tr>
                </table>
                </div>	
                '
	                WHERE [AttributeId] = @AttributeButtonHtmlId AND [EntityId] = @SecondaryButtonId

" );
            // add defined values
            RockMigrationHelper.AddDefinedValue( "407A3A73-A3EF-4970-B856-2A33F62AC72E", "Deny", "Used for as a denying or negative commands.", "D6B809A9-C1CC-4EBB-816E-33D8C1E53EA4", false );
            RockMigrationHelper.AddDefinedValueAttributeValue( "D6B809A9-C1CC-4EBB-816E-33D8C1E53EA4", "6FF59F53-28EA-4BFE-AFE1-A459CC588495", @"<div class=""btn-tabled"" style=""float: left; margin: 0 6px 12px 0"">
<table width=""100%"" border=""0"" cellspacing=""0"" cellpadding=""0"">
  <tr>
    <td>
      <table border=""0"" cellspacing=""0"" cellpadding=""0"">
        <tr>
          <td align=""center"" style=""
			-webkit-border-radius: 4px; 
			-moz-border-radius: 4px; 
			border-radius: 4px;"" 
			bgcolor=""#ac3434"">
								
				<a href=""{{ ButtonLink }}"" onclick=""{{ ButtonClick }}"" data-loading-text=""<i class='fa fa-refresh fa-spin'></i> {{ ButtonText }}""
					style=""	
						font-size: 14px; 
						font-family: OpenSans, 'Helvetica Neue', Helvetica, Arial, sans-serif; 
						color: #ffffff; 
						text-decoration: none; 
						-webkit-border-radius: 4px; 
						-moz-border-radius: 4px; 
						border-radius: 4px; 
						padding: 6px 12px; 
						border: 1px solid #902c2c; 
						display: inline-block;"">
							<i class=""fa fa-ban""></i> {{ ButtonText }}
				</a>
			</td>
        </tr>
      </table>
    </td>
  </tr>
</table>
</div>" );

            // approve button
            RockMigrationHelper.AddDefinedValue( "407A3A73-A3EF-4970-B856-2A33F62AC72E", "Approve", "Used for as a approving or positive commands.", "C88FEF94-95B9-444A-BC93-58E983F3C047", false );
            RockMigrationHelper.AddDefinedValueAttributeValue( "C88FEF94-95B9-444A-BC93-58E983F3C047", "6FF59F53-28EA-4BFE-AFE1-A459CC588495", @"<div class=""btn-tabled"" style=""float: left; margin: 0 6px 12px 0"">
<table width=""100%"" border=""0"" cellspacing=""0"" cellpadding=""0"">
  <tr>
    <td>
      <table border=""0"" cellspacing=""0"" cellpadding=""0"">
        <tr>
          <td align=""center"" style=""
			-webkit-border-radius: 4px; 
			-moz-border-radius: 4px; 
			border-radius: 4px;"" 
			bgcolor=""#359967"">
								
				<a href=""{{ ButtonLink }}"" onclick=""{{ ButtonClick }}"" data-loading-text=""<i class='fa fa-refresh fa-spin'></i> {{ ButtonText }}""
					style=""	
						font-size: 14px; 
						font-family: OpenSans, 'Helvetica Neue', Helvetica, Arial, sans-serif; 
						color: #ffffff; 
						text-decoration: none; 
						-webkit-border-radius: 4px; 
						-moz-border-radius: 4px; 
						border-radius: 4px; 
						padding: 6px 12px; 
						border: 1px solid #2c7f55; 
						display: inline-block;"">
							<i class=""fa fa-check""></i> {{ ButtonText }}
				</a>
			</td>
        </tr>
      </table>
    </td>
  </tr>
</table>
</div>" );

            // success button
            RockMigrationHelper.AddDefinedValue( "407A3A73-A3EF-4970-B856-2A33F62AC72E", "Success", "Styled like the Bootstrap success button.", "53CA2CB9-8BFA-450C-A3AA-FD3F3FD3BC8A", false );
            RockMigrationHelper.AddDefinedValueAttributeValue( "53CA2CB9-8BFA-450C-A3AA-FD3F3FD3BC8A", "6FF59F53-28EA-4BFE-AFE1-A459CC588495", @"<div class=""btn-tabled"" style=""float: left; margin: 0 6px 12px 0"">
<table width=""100%"" border=""0"" cellspacing=""0"" cellpadding=""0"">
  <tr>
    <td>
      <table border=""0"" cellspacing=""0"" cellpadding=""0"">
        <tr>
          <td align=""center"" style=""
			-webkit-border-radius: 4px; 
			-moz-border-radius: 4px; 
			border-radius: 4px;"" 
			bgcolor=""#359967"">
								
				<a href=""{{ ButtonLink }}"" onclick=""{{ ButtonClick }}"" data-loading-text=""<i class='fa fa-refresh fa-spin'></i> {{ ButtonText }}""
					style=""	
						font-size: 14px; 
						font-family: OpenSans, 'Helvetica Neue', Helvetica, Arial, sans-serif; 
						color: #ffffff; 
						text-decoration: none; 
						-webkit-border-radius: 4px; 
						-moz-border-radius: 4px; 
						border-radius: 4px; 
						padding: 6px 12px; 
						border: 1px solid #2c7f55; 
						display: inline-block;"">
							{{ ButtonText }}
				</a>
			</td>
        </tr>
      </table>
    </td>
  </tr>
</table>
</div>" );

            // info button
            RockMigrationHelper.AddDefinedValue( "407A3A73-A3EF-4970-B856-2A33F62AC72E", "Info", "Styled like the Bootstrap info button.", "3C026B37-29D4-47CB-BB6E-DA43AFE779FE", false );
            RockMigrationHelper.AddDefinedValueAttributeValue( "3C026B37-29D4-47CB-BB6E-DA43AFE779FE", "6FF59F53-28EA-4BFE-AFE1-A459CC588495", @"<div class=""btn-tabled"" style=""float: left; margin: 0 6px 12px 0"">
<table width=""100%"" border=""0"" cellspacing=""0"" cellpadding=""0"">
  <tr>
    <td>
      <table border=""0"" cellspacing=""0"" cellpadding=""0"">
        <tr>
          <td align=""center"" style=""
			-webkit-border-radius: 4px; 
			-moz-border-radius: 4px; 
			border-radius: 4px;"" 
			bgcolor=""#20a9c7"">
								
				<a href=""{{ ButtonLink }}"" onclick=""{{ ButtonClick }}"" data-loading-text=""<i class='fa fa-refresh fa-spin'></i> {{ ButtonText }}""
					style=""	
						font-size: 14px; 
						font-family: OpenSans, 'Helvetica Neue', Helvetica, Arial, sans-serif; 
						color: #ffffff; 
						text-decoration: none; 
						-webkit-border-radius: 4px; 
						-moz-border-radius: 4px; 
						border-radius: 4px; 
						padding: 6px 12px; 
						border: 1px solid #1b8fa8; 
						display: inline-block;"">
							<i class=""fa fa-check""></i> {{ ButtonText }}
				</a>
			</td>
        </tr>
      </table>
    </td>
  </tr>
</table>
</div>" );

            // warning button
            RockMigrationHelper.AddDefinedValue( "407A3A73-A3EF-4970-B856-2A33F62AC72E", "Warning", "Styled like the Bootstrap warning button.", "F03C9591-C497-4E27-A714-6A482E745141", false );
            RockMigrationHelper.AddDefinedValueAttributeValue( "F03C9591-C497-4E27-A714-6A482E745141", "6FF59F53-28EA-4BFE-AFE1-A459CC588495", @"<div class=""btn-tabled"" style=""float: left; margin: 0 6px 12px 0"">
<table width=""100%"" border=""0"" cellspacing=""0"" cellpadding=""0"">
  <tr>
    <td>
      <table border=""0"" cellspacing=""0"" cellpadding=""0"">
        <tr>
          <td align=""center"" style=""
			-webkit-border-radius: 4px; 
			-moz-border-radius: 4px; 
			border-radius: 4px;"" 
			bgcolor=""#efc137"">
								
				<a href=""{{ ButtonLink }}"" onclick=""{{ ButtonClick }}"" data-loading-text=""<i class='fa fa-refresh fa-spin'></i> {{ ButtonText }}""
					style=""	
						font-size: 14px; 
						font-family: OpenSans, 'Helvetica Neue', Helvetica, Arial, sans-serif; 
						color: #ffffff; 
						text-decoration: none; 
						-webkit-border-radius: 4px; 
						-moz-border-radius: 4px; 
						border-radius: 4px; 
						padding: 6px 12px; 
						border: 1px solid #edb716; 
						display: inline-block;"">
							<i class=""fa fa-check""></i> {{ ButtonText }}
				</a>
			</td>
        </tr>
      </table>
    </td>
  </tr>
</table>
</div>" );

            // danger button
            RockMigrationHelper.AddDefinedValue( "407A3A73-A3EF-4970-B856-2A33F62AC72E", "Danger", "Styled like the Bootstrap danger button.", "9B329020-E074-4326-8831-9DD534F491DF", false );
            RockMigrationHelper.AddDefinedValueAttributeValue( "9B329020-E074-4326-8831-9DD534F491DF", "6FF59F53-28EA-4BFE-AFE1-A459CC588495", @"<div class=""btn-tabled"" style=""float: left; margin: 0 6px 12px 0"">
<table width=""100%"" border=""0"" cellspacing=""0"" cellpadding=""0"">
  <tr>
    <td>
      <table border=""0"" cellspacing=""0"" cellpadding=""0"">
        <tr>
          <td align=""center"" style=""
			-webkit-border-radius: 4px; 
			-moz-border-radius: 4px; 
			border-radius: 4px;"" 
			bgcolor=""#c84b4b"">
								
				<a href=""{{ ButtonLink }}"" onclick=""{{ ButtonClick }}"" data-loading-text=""<i class='fa fa-refresh fa-spin'></i> {{ ButtonText }}""
					style=""	
						font-size: 14px; 
						font-family: OpenSans, 'Helvetica Neue', Helvetica, Arial, sans-serif; 
						color: #ffffff; 
						text-decoration: none; 
						-webkit-border-radius: 4px; 
						-moz-border-radius: 4px; 
						border-radius: 4px; 
						padding: 6px 12px; 
						border: 1px solid #bf3a3a; 
						display: inline-block;"">
								{{ ButtonText }}
				</a>
			</td>
        </tr>
      </table>
    </td>
  </tr>
</table>
</div>" );

            // default button
            RockMigrationHelper.AddDefinedValue( "407A3A73-A3EF-4970-B856-2A33F62AC72E", "Default", "Styled like the Bootstrap default button.", "638BEEE0-2F8F-4706-B9A4-5BAB70386697", false );
            RockMigrationHelper.AddDefinedValueAttributeValue( "638BEEE0-2F8F-4706-B9A4-5BAB70386697", "6FF59F53-28EA-4BFE-AFE1-A459CC588495", @"<div class=""btn-tabled"" style=""float: left; margin: 0 6px 12px 0"">
<table width=""100%"" border=""0"" cellspacing=""0"" cellpadding=""0"">
  <tr>
    <td>
      <table border=""0"" cellspacing=""0"" cellpadding=""0"">
        <tr>
          <td align=""center"" style=""
			-webkit-border-radius: 4px; 
			-moz-border-radius: 4px; 
			border-radius: 4px;"" 
			bgcolor=""#ffffff"">
								
				<a href=""{{ ButtonLink }}"" onclick=""{{ ButtonClick }}"" data-loading-text=""<i class='fa fa-refresh fa-spin'></i> {{ ButtonText }}""
					style=""	
						font-size: 14px; 
						font-family: OpenSans, 'Helvetica Neue', Helvetica, Arial, sans-serif; 
						color: #000; 
						text-decoration: none; 
						-webkit-border-radius: 4px; 
						-moz-border-radius: 4px; 
						border-radius: 4px; 
						padding: 6px 12px; 
						border: 1px solid #ccc; 
						display: inline-block;"">
								{{ ButtonText }}
				</a>
			</td>
        </tr>
      </table>
    </td>
  </tr>
</table>
</div>" );

            // update the system email with new template
            Sql( @" UPDATE [SystemEmail]
  SET [Body] = '{{ GlobalAttribute.EmailHeader }}

<p>{{ Person.FirstName }},</p>
<p>The following {{ Workflow.WorkflowType.Name }} requires action:<p>
<p>{{ Workflow.WorkflowType.WorkTerm}}: <a href=''{{ GlobalAttribute.InternalApplicationRoot }}WorkflowEntry/{{ Workflow.WorkflowTypeId }}?WorkflowGuid={{ Workflow.Guid }}''>{{ Workflow.Name }}</a></p>

{% assign RequiredFields = false %}

<h4>Details:</h4>
<p>
{% for attribute in Action.FormAttributes %}

    {% if attribute.Url != Empty || attribute.Value != Empty %}
    
        <strong>{{ attribute.Name }}:</strong> 
    
        {% if attribute.Url != Empty %}
            <a href=''{{ attribute.Url }}''>{{ attribute.Value }}</a>
        {% else %}
            {{ attribute.Value }}
        {% endif %}
        <br/>

    {% endif %}
    
    {% if attribute.IsRequired && attribute.Value == Empty %}
        {% assign RequiredFields = true %}
    {% endif %}

{% endfor %}
</p>


{% if Action.ActionType.WorkflowForm.IncludeActionsInNotification == true %}

    {% if RequiredFields != true %}

        <p>
        <table>
            <tr>
                {% for button in Action.ActionType.WorkflowForm.Buttons %}
                    <td>

                    {% capture ButtonLinkSearch %}{% raw %}{{ ButtonLink }}{% endraw %}{% endcapture %}
                    {% capture ButtonLinkReplace %}{{ GlobalAttribute.InternalApplicationRoot }}WorkflowEntry/{{ Workflow.WorkflowTypeId }}?WorkflowGuid={{ Workflow.Guid }}&action={{ button.Name }}{% endcapture %}
                    {% capture ButtonHtml %}{{ button.Html | Replace: ButtonLinkSearch, ButtonLinkReplace }}{% endcapture %}

                    {% capture ButtonTextSearch %}{% raw %}{{ ButtonText }}{% endraw %}{% endcapture %}
                    {% capture ButtonTextReplace %}{{ button.Name }}{% endcapture %}
                    {{ ButtonHtml | Replace: ButtonTextSearch, ButtonTextReplace }}
                    
                    </td>
                {% endfor %}
            </tr>
        </table>
        </p>

    {% endif %}

{% endif %}

{{ GlobalAttribute.EmailFooter }}'
  WHERE [Guid] = '88C7D1CC-3478-4562-A301-AE7D4D7FFF6D'" );
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            RockMigrationHelper.DeleteDefinedValue("638BEEE0-2F8F-4706-B9A4-5BAB70386697");
            RockMigrationHelper.DeleteDefinedValue( "9B329020-E074-4326-8831-9DD534F491DF" );
            RockMigrationHelper.DeleteDefinedValue( "F03C9591-C497-4E27-A714-6A482E745141" );
            RockMigrationHelper.DeleteDefinedValue( "3C026B37-29D4-47CB-BB6E-DA43AFE779FE" );
            RockMigrationHelper.DeleteDefinedValue( "53CA2CB9-8BFA-450C-A3AA-FD3F3FD3BC8A" );
            RockMigrationHelper.DeleteDefinedValue( "C88FEF94-95B9-444A-BC93-58E983F3C047" );
            RockMigrationHelper.DeleteDefinedValue( "D6B809A9-C1CC-4EBB-816E-33D8C1E53EA4" );
        }
    }
}
