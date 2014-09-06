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
    public partial class WorkflowUpdates : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {

            Sql( @"
UPDATE [SystemEmail]
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
        {% for button in Action.ActionType.WorkflowForm.Buttons %}

            {% capture ButtonLinkSearch %}{% raw %}{{ ButtonLink }}{% endraw %}{% endcapture %}
            {% capture ButtonLinkReplace %}{{ GlobalAttribute.InternalApplicationRoot }}WorkflowEntry/{{ Workflow.WorkflowTypeId }}?WorkflowGuid={{ Workflow.Guid }}&action={{ button.Name }}{% endcapture %}
            {% capture ButtonHtml %}{{ button.Html | Replace: ButtonLinkSearch, ButtonLinkReplace }}{% endcapture %}

            {% capture ButtonTextSearch %}{% raw %}{{ ButtonText }}{% endraw %}{% endcapture %}
            {% capture ButtonTextReplace %}{{ button.Name }}{% endcapture %}
            {{ ButtonHtml | Replace: ButtonTextSearch, ButtonTextReplace }}

        {% endfor %}
        </p>

    {% endif %}

{% endif %}

{{ GlobalAttribute.EmailFooter }}'
WHERE [Guid] = '88C7D1CC-3478-4562-A301-AE7D4D7FFF6D'
" );

            try
            {
                Sql( @"
    DELETE FROM [Auth]
    WHERE [EntityTypeId] IN (
        SELECT [Id]
        FROM [EntityType]
        WHERE [Guid] IN (
            'B60D2D0D-A527-4623-9D9C-6960CA2D84CD'
            ,'C6BC3BF4-0DD5-46C6-87D0-24681A48F5A2'
            ,'82186B56-4ECD-4A51-9D72-EC81C2BED171'
            ,'1FA9042F-EC15-411C-A25C-71F6323B3005'
            ,'E5614E98-7CAD-46A0-B885-BD1EA92F3D36'
            ,'1C2ADB38-71F0-44C0-9795-0FE73D45D814'
        )
    )

    DELETE FROM [Attribute]
    WHERE [EntityTypeId] IN (
        SELECT [Id]
        FROM [EntityType]
        WHERE [Guid] IN (
            'B60D2D0D-A527-4623-9D9C-6960CA2D84CD'
            ,'C6BC3BF4-0DD5-46C6-87D0-24681A48F5A2'
            ,'82186B56-4ECD-4A51-9D72-EC81C2BED171'
            ,'1FA9042F-EC15-411C-A25C-71F6323B3005'
            ,'E5614E98-7CAD-46A0-B885-BD1EA92F3D36'
            ,'1C2ADB38-71F0-44C0-9795-0FE73D45D814'
        )
    )

    DELETE FROM EntityType
    WHERE [Guid] IN (
        'B60D2D0D-A527-4623-9D9C-6960CA2D84CD'
        ,'C6BC3BF4-0DD5-46C6-87D0-24681A48F5A2'
        ,'82186B56-4ECD-4A51-9D72-EC81C2BED171'
        ,'1FA9042F-EC15-411C-A25C-71F6323B3005'
        ,'E5614E98-7CAD-46A0-B885-BD1EA92F3D36'
        ,'1C2ADB38-71F0-44C0-9795-0FE73D45D814'
    )
" );
            }
            catch { }

        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
        }
    }
}
