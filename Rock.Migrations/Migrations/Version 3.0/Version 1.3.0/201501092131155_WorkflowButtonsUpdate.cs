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
    public partial class WorkflowButtonsUpdate : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            // update the name of the primary button
            Sql( @"UPDATE [DefinedValue]
  SET [Value] = 'Primary', [Description] = 'Used for the primary button on the form. Styled to use the Rock brand color (orange).'
  WHERE [Guid] = 'FDC397CD-8B4A-436E-BEA1-BCE2E6717C03'" );

            // update the name of the secondary button
            Sql( @"UPDATE [DefinedValue]
  SET [Value] = 'Secondary', [Description] = 'Used for the secondary button on the form. Styled to use the Bootstrap default styling.'
  WHERE [Guid] = '8CF6E927-4FA5-4241-991C-391038B79631'" );

            // add the new attribute to the defined value
            RockMigrationHelper.AddDefinedTypeAttribute( "407A3A73-A3EF-4970-B856-2A33F62AC72E", "1D0D3794-C210-48A8-8C68-3FBEC08A6BA5", "Button Email HTML", "ButtonEmailHTML", "The HTML to use for displaying a button in an HTML email. {{ ButtonLink }}, {{ ButtonClick }} and {{ ButtonText }} merge fields are available to be used in the HTML as placeholders.  {{ ButtonLink }} will be replaced with the url that user should be redirected to when they select that button.  {{ ButtonClick }} is replaced with any necessary client script (e.g. validation and animation).  {{ ButtonText }} is replaced with the name to be displayed.", 35, "", "8DA98984-BA25-473C-93BA-8BA2FD58C4C7" );

            // update the attribute qualifiers for the new attribute
            RockMigrationHelper.AddAttributeQualifier( "8DA98984-BA25-473C-93BA-8BA2FD58C4C7", "editorMode", "2", "94A24055-9866-4AC8-8E57-278D232C935B" );
            RockMigrationHelper.AddAttributeQualifier( "8DA98984-BA25-473C-93BA-8BA2FD58C4C7", "editorHeight", "", "73B8DA44-9D4B-4C93-BCEA-632B6B43426E" );
            RockMigrationHelper.AddAttributeQualifier( "8DA98984-BA25-473C-93BA-8BA2FD58C4C7", "editorTheme", "0", "3446ECAC-9273-449C-B933-F0F6506F5B19" );


            //  primary html
            RockMigrationHelper.AddDefinedValueAttributeValue( "FDC397CD-8B4A-436E-BEA1-BCE2E6717C03", "6FF59F53-28EA-4BFE-AFE1-A459CC588495", @"<a  href=""{{ ButtonLink }}"" 
    onclick=""{{ ButtonClick }}""
    data-loading-text=""<i class='fa fa-refresh fa-spin'></i> {{ ButtonText }}""
    class=""btn btn-primary""
    >
    {{ ButtonText }}
</a>" );

            // primary email html
            RockMigrationHelper.AddDefinedValueAttributeValue( "FDC397CD-8B4A-436E-BEA1-BCE2E6717C03", "8DA98984-BA25-473C-93BA-8BA2FD58C4C7", @"<table align=""left"" style=""width: 29%; min-width: 190px; margin-bottom: 12px;"" cellpadding=""0"" cellspacing=""0"">
 <tr>
   <td>

		<div><!--[if mso]>
		  <v:roundrect xmlns:v=""urn:schemas-microsoft-com:vml"" xmlns:w=""urn:schemas-microsoft-com:office:word"" href=""{{ ButtonLink }}"" style=""height:38px;v-text-anchor:middle;width:175px;"" arcsize=""11%"" strokecolor=""#e76812"" fillcolor=""#ee7624"">
			<w:anchorlock/>
			<center style=""color:#ffffff;font-family:sans-serif;font-size:13px;font-weight:normal;"">{{ ButtonText }}</center>
		  </v:roundrect>
		<![endif]--><a href=""{{ ButtonLink }}""
		style=""background-color:#ee7624;border:1px solid #e76812;border-radius:4px;color:#ffffff;display:inline-block;font-family:sans-serif;font-size:13px;font-weight:normal;line-height:38px;text-align:center;text-decoration:none;width:175px;-webkit-text-size-adjust:none;mso-hide:all;"">{{ ButtonText }}</a></div>

	</td>
 </tr>
</table>" );


            // **********************************************


            //  secondary html
            RockMigrationHelper.AddDefinedValueAttributeValue( "8CF6E927-4FA5-4241-991C-391038B79631", "6FF59F53-28EA-4BFE-AFE1-A459CC588495", @"<a  href=""{{ ButtonLink }}"" 
    onclick=""{{ ButtonClick }}"" 
    data-loading-text=""<i class='fa fa-refresh fa-spin'></i> {{ ButtonText }}""
    class=""btn btn-default""
    >
    {{ ButtonText }}
</a>" );

            // secondary email html
            RockMigrationHelper.AddDefinedValueAttributeValue( "8CF6E927-4FA5-4241-991C-391038B79631", "8DA98984-BA25-473C-93BA-8BA2FD58C4C7", @"<table align=""left"" style=""width: 29%; min-width: 190px; margin-bottom: 12px;"" cellpadding=""0"" cellspacing=""0"">
 <tr>
   <td>

		<div><!--[if mso]>
		  <v:roundrect xmlns:v=""urn:schemas-microsoft-com:vml"" xmlns:w=""urn:schemas-microsoft-com:office:word"" href=""{{ ButtonLink }}"" style=""height:38px;v-text-anchor:middle;width:175px;"" arcsize=""11%"" strokecolor=""#adadad"" fillcolor=""#e6e6e6"">
			<w:anchorlock/>
			<center style=""color:#333333;font-family:sans-serif;font-size:13px;font-weight:normal;"">{{ ButtonText }}</center>
		  </v:roundrect>
		<![endif]--><a href=""{{ ButtonLink }}""
		style=""background-color:#e6e6e6;border:1px solid #adadad;border-radius:4px;color:#333333;display:inline-block;font-family:sans-serif;font-size:13px;font-weight:normal;line-height:38px;text-align:center;text-decoration:none;width:175px;-webkit-text-size-adjust:none;mso-hide:all;"">{{ ButtonText }}</a></div>

	</td>
 </tr>
</table>" );



            // **********************************************



            // deny html
            RockMigrationHelper.AddDefinedValueAttributeValue( "D6B809A9-C1CC-4EBB-816E-33D8C1E53EA4", "6FF59F53-28EA-4BFE-AFE1-A459CC588495", @"<a  href=""{{ ButtonLink }}"" 
    onclick=""{{ ButtonClick }}"" 
    data-loading-text=""<i class='fa fa-refresh fa-spin'></i> {{ ButtonText }}""
    class=""btn btn-danger"" >
    <i class=""fa fa-ban""></i> {{ ButtonText }}
</a>" );

            // deny email html
            RockMigrationHelper.AddDefinedValueAttributeValue( "D6B809A9-C1CC-4EBB-816E-33D8C1E53EA4", "8DA98984-BA25-473C-93BA-8BA2FD58C4C7", @"<table align=""left"" style=""width: 29%; min-width: 190px; margin-bottom: 12px;"" cellpadding=""0"" cellspacing=""0"">
 <tr>
   <td>

		<div><!--[if mso]>
		  <v:roundrect xmlns:v=""urn:schemas-microsoft-com:vml"" xmlns:w=""urn:schemas-microsoft-com:office:word"" href=""{{ ButtonLink }}"" style=""height:38px;v-text-anchor:middle;width:175px;"" arcsize=""11%"" strokecolor=""#ac2925"" fillcolor=""#c9302c"">
			<w:anchorlock/>
			<center style=""color:#ffffff;font-family:sans-serif;font-size:13px;font-weight:normal;"">{{ ButtonText }}</center>
		  </v:roundrect>
		<![endif]--><a href=""{{ ButtonLink }}"" 
		style=""background-color:#c9302c;border:1px solid #ac2925;border-radius:4px;color:#ffffff;display:inline-block;font-family:sans-serif;font-size:13px;font-weight:normal;line-height:38px;text-align:center;text-decoration:none;width:175px;-webkit-text-size-adjust:none;mso-hide:all;"">{{ ButtonText }}</a></div>

	</td>
 </tr>
</table>" );


            // **********************************************



            // approve html
            RockMigrationHelper.AddDefinedValueAttributeValue( "C88FEF94-95B9-444A-BC93-58E983F3C047", "6FF59F53-28EA-4BFE-AFE1-A459CC588495", @"<a  href=""{{ ButtonLink }}"" 
    onclick=""{{ ButtonClick }}"" 
    data-loading-text=""<i class='fa fa-refresh fa-spin'></i> {{ ButtonText }}""
    class=""btn btn-success"" >
    <i class=""fa fa-check""></i> {{ ButtonText }}
</a>" );


            // approve email html
            RockMigrationHelper.AddDefinedValueAttributeValue( "C88FEF94-95B9-444A-BC93-58E983F3C047", "8DA98984-BA25-473C-93BA-8BA2FD58C4C7", @"<table align=""left"" style=""width: 29%; min-width: 190px; margin-bottom: 12px;"" cellpadding=""0"" cellspacing=""0"">
 <tr>
   <td>

		<div><!--[if mso]>
		  <v:roundrect xmlns:v=""urn:schemas-microsoft-com:vml"" xmlns:w=""urn:schemas-microsoft-com:office:word"" href=""{{ ButtonLink }}"" style=""height:38px;v-text-anchor:middle;width:175px;"" arcsize=""11%"" strokecolor=""#398439"" fillcolor=""#449d44"">
			<w:anchorlock/>
			<center style=""color:#ffffff;font-family:sans-serif;font-size:13px;font-weight:normal;"">{{ ButtonText }}</center>
		  </v:roundrect>
		<![endif]--><a href=""{{ ButtonLink }}"" 
		style=""background-color:#449d44;border:1px solid #398439;border-radius:4px;color:#ffffff;display:inline-block;font-family:sans-serif;font-size:13px;font-weight:normal;line-height:38px;text-align:center;text-decoration:none;width:175px;-webkit-text-size-adjust:none;mso-hide:all;"">{{ ButtonText }}</a></div>

	</td>
 </tr>
</table>" );


            // **********************************************



            // success html
            RockMigrationHelper.AddDefinedValueAttributeValue( "53CA2CB9-8BFA-450C-A3AA-FD3F3FD3BC8A", "6FF59F53-28EA-4BFE-AFE1-A459CC588495", @"<a  href=""{{ ButtonLink }}"" 
    onclick=""{{ ButtonClick }}"" 
    data-loading-text=""<i class='fa fa-refresh fa-spin'></i> {{ ButtonText }}""
    class=""btn btn-success"" >
    {{ ButtonText }}
</a>" );


            // success email html
            RockMigrationHelper.AddDefinedValueAttributeValue( "53CA2CB9-8BFA-450C-A3AA-FD3F3FD3BC8A", "8DA98984-BA25-473C-93BA-8BA2FD58C4C7", @"<table align=""left"" style=""width: 29%; min-width: 190px; margin-bottom: 12px;"" cellpadding=""0"" cellspacing=""0"">
 <tr>
   <td>

		<div><!--[if mso]>
		  <v:roundrect xmlns:v=""urn:schemas-microsoft-com:vml"" xmlns:w=""urn:schemas-microsoft-com:office:word"" href=""{{ ButtonLink }}"" style=""height:38px;v-text-anchor:middle;width:175px;"" arcsize=""11%"" strokecolor=""#398439"" fillcolor=""#449d44"">
			<w:anchorlock/>
			<center style=""color:#ffffff;font-family:sans-serif;font-size:13px;font-weight:normal;"">{{ ButtonText }}</center>
		  </v:roundrect>
		<![endif]--><a href=""{{ ButtonLink }}""
		style=""background-color:#449d44;border:1px solid #398439;border-radius:4px;color:#ffffff;display:inline-block;font-family:sans-serif;font-size:13px;font-weight:normal;line-height:38px;text-align:center;text-decoration:none;width:175px;-webkit-text-size-adjust:none;mso-hide:all;"">{{ ButtonText }}</a></div>

	</td>
 </tr>
</table>" );


            // **********************************************



            // info html
            RockMigrationHelper.AddDefinedValueAttributeValue( "3C026B37-29D4-47CB-BB6E-DA43AFE779FE", "6FF59F53-28EA-4BFE-AFE1-A459CC588495", @"<a  href=""{{ ButtonLink }}"" 
    onclick=""{{ ButtonClick }}"" 
    data-loading-text=""<i class='fa fa-refresh fa-spin'></i> {{ ButtonText }}""
    class=""btn btn-info"" >
    {{ ButtonText }}
</a>" );


            // info email html
            RockMigrationHelper.AddDefinedValueAttributeValue( "3C026B37-29D4-47CB-BB6E-DA43AFE779FE", "8DA98984-BA25-473C-93BA-8BA2FD58C4C7", @"<table align=""left"" style=""width: 29%; min-width: 190px; margin-bottom: 12px;"" cellpadding=""0"" cellspacing=""0"">
 <tr>
   <td>

		<div><!--[if mso]>
		  <v:roundrect xmlns:v=""urn:schemas-microsoft-com:vml"" xmlns:w=""urn:schemas-microsoft-com:office:word"" href=""{{ ButtonLink }}"" style=""height:38px;v-text-anchor:middle;width:175px;"" arcsize=""11%"" strokecolor=""#269abc"" fillcolor=""#31b0d5"">
			<w:anchorlock/>
			<center style=""color:#ffffff;font-family:sans-serif;font-size:13px;font-weight:normal;"">{{ ButtonText }}</center>
		  </v:roundrect>
		<![endif]--><a href=""{{ ButtonLink }}""
		style=""background-color:#31b0d5;border:1px solid #269abc;border-radius:4px;color:#ffffff;display:inline-block;font-family:sans-serif;font-size:13px;font-weight:normal;line-height:38px;text-align:center;text-decoration:none;width:175px;-webkit-text-size-adjust:none;mso-hide:all;"">{{ ButtonText }}</a></div>

	</td>
 </tr>
</table>" );


            // **********************************************



            // warning html
            RockMigrationHelper.AddDefinedValueAttributeValue( "F03C9591-C497-4E27-A714-6A482E745141", "6FF59F53-28EA-4BFE-AFE1-A459CC588495", @"<a  href=""{{ ButtonLink }}"" 
    onclick=""{{ ButtonClick }}"" 
    data-loading-text=""<i class='fa fa-refresh fa-spin'></i> {{ ButtonText }}""
    class=""btn btn-warning"" >
    {{ ButtonText }}
</a>" );


            // warning email html
            RockMigrationHelper.AddDefinedValueAttributeValue( "F03C9591-C497-4E27-A714-6A482E745141", "8DA98984-BA25-473C-93BA-8BA2FD58C4C7", @"<table align=""left"" style=""width: 29%; min-width: 190px; margin-bottom: 12px;"" cellpadding=""0"" cellspacing=""0"">
 <tr>
   <td>

		<div><!--[if mso]>
		  <v:roundrect xmlns:v=""urn:schemas-microsoft-com:vml"" xmlns:w=""urn:schemas-microsoft-com:office:word"" href=""{{ ButtonLink }}"" style=""height:38px;v-text-anchor:middle;width:175px;"" arcsize=""11%"" strokecolor=""#d58512"" fillcolor=""#ec971f"">
			<w:anchorlock/>
			<center style=""color:#ffffff;font-family:sans-serif;font-size:13px;font-weight:normal;"">{{ ButtonText }}</center>
		  </v:roundrect>
		<![endif]--><a href=""{{ ButtonLink }}""
		style=""background-color:#ec971f;border:1px solid #d58512;border-radius:4px;color:#ffffff;display:inline-block;font-family:sans-serif;font-size:13px;font-weight:normal;line-height:38px;text-align:center;text-decoration:none;width:175px;-webkit-text-size-adjust:none;mso-hide:all;"">{{ ButtonText }}</a></div>

	</td>
 </tr>
</table>" );


            // **********************************************



            // danger html
            RockMigrationHelper.AddDefinedValueAttributeValue( "9B329020-E074-4326-8831-9DD534F491DF", "6FF59F53-28EA-4BFE-AFE1-A459CC588495", @"<a  href=""{{ ButtonLink }}"" 
    onclick=""{{ ButtonClick }}"" 
    data-loading-text=""<i class='fa fa-refresh fa-spin'></i> {{ ButtonText }}""
    class=""btn btn-danger"" >
    {{ ButtonText }}
</a>" );


            // danger email html
            RockMigrationHelper.AddDefinedValueAttributeValue( "9B329020-E074-4326-8831-9DD534F491DF", "8DA98984-BA25-473C-93BA-8BA2FD58C4C7", @"<table align=""left"" style=""width: 29%; min-width: 190px; margin-bottom: 12px;"" cellpadding=""0"" cellspacing=""0"">
 <tr>
   <td>

		<div><!--[if mso]>
		  <v:roundrect xmlns:v=""urn:schemas-microsoft-com:vml"" xmlns:w=""urn:schemas-microsoft-com:office:word"" href=""{{ ButtonLink }}"" style=""height:38px;v-text-anchor:middle;width:175px;"" arcsize=""11%"" strokecolor=""#ac2925"" fillcolor=""#c9302c"">
			<w:anchorlock/>
			<center style=""color:#ffffff;font-family:sans-serif;font-size:13px;font-weight:normal;"">{{ ButtonText }}</center>
		  </v:roundrect>
		<![endif]--><a href=""{{ ButtonLink }}""
		style=""background-color:#c9302c;border:1px solid #ac2925;border-radius:4px;color:#ffffff;display:inline-block;font-family:sans-serif;font-size:13px;font-weight:normal;line-height:38px;text-align:center;text-decoration:none;width:175px;-webkit-text-size-adjust:none;mso-hide:all;"">{{ ButtonText }}</a></div>

	</td>
 </tr>
</table>" );


            // **********************************************



            // default html
            RockMigrationHelper.AddDefinedValueAttributeValue( "638BEEE0-2F8F-4706-B9A4-5BAB70386697", "6FF59F53-28EA-4BFE-AFE1-A459CC588495", @"<a  href=""{{ ButtonLink }}"" 
    onclick=""{{ ButtonClick }}"" 
    data-loading-text=""<i class='fa fa-refresh fa-spin'></i> {{ ButtonText }}""
    class=""btn btn-default"" >
    {{ ButtonText }}
</a>" );


            // default email html
            RockMigrationHelper.AddDefinedValueAttributeValue( "638BEEE0-2F8F-4706-B9A4-5BAB70386697", "8DA98984-BA25-473C-93BA-8BA2FD58C4C7", @"<table align=""left"" style=""width: 29%; min-width: 190px; margin-bottom: 12px;"" cellpadding=""0"" cellspacing=""0"">
 <tr>
   <td>

		<div><!--[if mso]>
		  <v:roundrect xmlns:v=""urn:schemas-microsoft-com:vml"" xmlns:w=""urn:schemas-microsoft-com:office:word"" href=""{{ ButtonLink }}"" style=""height:38px;v-text-anchor:middle;width:175px;"" arcsize=""11%"" strokecolor=""#adadad""  fillcolor=""#e6e6e6"">
			<w:anchorlock/>
			<center style=""color:#333333;font-family:sans-serif;font-size:13px;font-weight:normal;"">{{ ButtonText }}</center>
		  </v:roundrect>
		<![endif]--><a href=""{{ ButtonLink }}""
		style=""background-color:#e6e6e6;border:1px solid #adadad;border-radius:4px;color:#333333;display:inline-block;font-family:sans-serif;font-size:13px;font-weight:normal;line-height:38px;text-align:center;text-decoration:none;width:175px;-webkit-text-size-adjust:none;mso-hide:all;"">{{ ButtonText }}</a></div>

	</td>
 </tr>
</table>" );


            // **********************************************





            // update system email
            Sql( @"
UPDATE [SystemEmail]
SET [Body] = '{{ GlobalAttribute.EmailHeader }}

<p>{{ Person.FirstName }},</p>
<p>The following {{ Workflow.WorkflowType.Name }} requires action:<p>
<p>{{ Workflow.WorkflowType.WorkTerm}}: <a href=''{{ GlobalAttribute.InternalApplicationRoot }}WorkflowEntry/{{ Workflow.WorkflowTypeId }}?WorkflowGuid={{ Workflow.Guid }}''>{{ Workflow.Name }}</a></p>

{% assign RequiredFields = false %}

<h3 class=""separator"">Details</h3>


<table>
{% for attribute in Action.FormAttributes %}

    
    {% if attribute.IsVisible and attribute.Value != '''' %}
        <tr>
            <td align=""right""><strong>{{ attribute.Name }}:</strong></td>
        
            <td>
                {% if attribute.Url and attribute.Url != '''' %}
                    <a href=''{{ attribute.Url }}''>{{ attribute.Value }}</a>
                {% else %}
                    {{ attribute.Value }}
                {% endif %}
            </td>

        </tr>
    {% endif %}
    
        
    {% if attribute.IsRequired && attribute.Value == Empty %}
        {% assign RequiredFields = true %}
    {% endif %}

{% endfor %}
</table>


<table width=""100%"">
    <tr>
        <td>

    <table align=""left"" style=""width: 29%; min-width: 190px; margin-bottom: 12px;"" cellpadding=""0"" cellspacing=""0"">
     <tr>
       <td>
    
    		<div><!--[if mso]>
    		  <v:roundrect xmlns:v=""urn:schemas-microsoft-com:vml"" xmlns:w=""urn:schemas-microsoft-com:office:word"" href=""{{ GlobalAttribute.InternalApplicationRoot }}WorkflowEntry/{{ Workflow.WorkflowTypeId }}?WorkflowGuid={{ Workflow.Guid }}"" style=""height:38px;v-text-anchor:middle;width:175px;"" arcsize=""11%"" strokecolor=""#269abc"" fillcolor=""#31b0d5"">
    			<w:anchorlock/>
    			<center style=""color:#ffffff;font-family:sans-serif;font-size:14px;font-weight:normal;"">View Details</center>
    		  </v:roundrect>
    		<![endif]--><a href=""{{ GlobalAttribute.InternalApplicationRoot }}WorkflowEntry/{{ Workflow.WorkflowTypeId }}?WorkflowGuid={{ Workflow.Guid }}""
    		style=""background-color:#31b0d5;border:1px solid #269abc;border-radius:4px;color:#ffffff;display:inline-block;font-family:sans-serif;font-size:14px;font-weight:normal;line-height:38px;text-align:center;text-decoration:none;width:175px;-webkit-text-size-adjust:none;mso-hide:all;"">View Details</a></div>
    
    	</td>
     </tr>
    </table>

    {% if Action.ActionType.WorkflowForm.IncludeActionsInNotification == true %}

        {% if RequiredFields != true %}
    
            {% for button in Action.ActionType.WorkflowForm.Buttons %}
                {% capture ButtonLinkSearch %}{% raw %}{{ ButtonLink }}{% endraw %}{% endcapture %}
                {% capture ButtonLinkReplace %}{{ GlobalAttribute.InternalApplicationRoot }}WorkflowEntry/{{ Workflow.WorkflowTypeId }}?WorkflowGuid={{ Workflow.Guid }}&action={{ button.Name }}{% endcapture %}
                {% capture ButtonHtml %}{{ button.EmailHtml | Replace: ButtonLinkSearch, ButtonLinkReplace }}{% endcapture %}

                {% capture ButtonTextSearch %}{% raw %}{{ ButtonText }}{% endraw %}{% endcapture %}
                {% capture ButtonTextReplace %}{{ button.Name }}{% endcapture %}
                {{ ButtonHtml | Replace: ButtonTextSearch, ButtonTextReplace }}
            {% endfor %}
        {% endif %}

    {% endif %}

        </td>
    </tr>
</table>


{{ GlobalAttribute.EmailFooter }}'
WHERE [Guid] = '88C7D1CC-3478-4562-A301-AE7D4D7FFF6D'
" );


        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
        }
    }
}
