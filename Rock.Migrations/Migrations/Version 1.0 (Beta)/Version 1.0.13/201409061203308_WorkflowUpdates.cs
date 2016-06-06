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

            // try to delete dead report entities, but ignore if there was an exception just in case one of these entities somehow got used in production
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

            // try to delete a few more dead entities, but ignore if there was an exception just in case one of these entities somehow got used in production
            try
            {
                Sql( @"
DELETE FROM [Auth]
    WHERE [EntityTypeId] IN (
        SELECT [Id]
        FROM [EntityType]
        WHERE [Guid] IN (
            '5222D370-20D5-4D18-A6BF-4F1FA571321D',--Rock.Address.Geocode.ServiceObjects
            'E5C50259-609E-4294-837B-26ADE92BDF4E',--Rock.Address.Geocode.StrikeIron
            'DD23B7D2-3A12-432C-B1EE-722379AD808D',--Rock.Address.Geocode.TelaAtlas
            '51C31F4E-943B-4B83-956F-41685A16FCBC',--Rock.Address.Standardize.MelissaData
            'C4CE3228-EF10-4B4D-9FBC-95466134F4AC',--Rock.Address.Standardize.StrikeIron
            '049FAF56-E816-447A-AA60-BB22CE18BF87',--Rock.Component.Geocode.ServiceObjects
            '23EA1D4F-3F7A-48E4-BB06-03AE42C669BE',--Rock.Component.Geocode.StrikeIron
            '4AD002E9-A577-475E-93A8-98084CD10CBB',--Rock.Component.Geocode.TelaAtlas
            '1AEB3602-0AEC-49A3-B6A5-EF43900ADF05',--Rock.Component.Standardize.MelissaData
            'EB81C9AE-7122-4F7F-AF2B-414E4B53067C',--Rock.Component.Standardize.StrikeIron
            'B90973E1-AFC5-49DD-9145-79058C025811',--Rock.MEF.Geocode.ServiceObjects
            'B4C00A2D-06CA-4B62-8BBD-1D4FA8A3CDF4',--Rock.MEF.Geocode.StrikeIron
            'F71FDAB3-5E7F-4DB4-AE4C-F60D5F74463A',--Rock.MEF.Geocode.TelaAtlas
            '0AD91B7F-860E-4AE6-8D4D-1C12BF134523',--Rock.MEF.Standardize.MelissaData
            'E71993C1-9476-47D3-BF88-EAC28D9FE65B',--Rock.MEF.Standardize.StrikeIron
            '8E8792EC-8378-437D-9213-1DE4E52ABA9F',--Rock.Model.EmailTemplate
            '863DB29A-BBF4-4B22-83EF-3E24020669D0',--Rock.Model.FinancialGateway
            '9969FC04-2985-46C7-B10D-BF3835A9D931'--Rock.Model.PersonMerged
        )
    )

    DELETE FROM [Attribute]
    WHERE [EntityTypeId] IN (
        SELECT [Id]
        FROM [EntityType]
        WHERE [Guid] IN (
             '5222D370-20D5-4D18-A6BF-4F1FA571321D',--Rock.Address.Geocode.ServiceObjects
            'E5C50259-609E-4294-837B-26ADE92BDF4E',--Rock.Address.Geocode.StrikeIron
            'DD23B7D2-3A12-432C-B1EE-722379AD808D',--Rock.Address.Geocode.TelaAtlas
            '51C31F4E-943B-4B83-956F-41685A16FCBC',--Rock.Address.Standardize.MelissaData
            'C4CE3228-EF10-4B4D-9FBC-95466134F4AC',--Rock.Address.Standardize.StrikeIron
            '049FAF56-E816-447A-AA60-BB22CE18BF87',--Rock.Component.Geocode.ServiceObjects
            '23EA1D4F-3F7A-48E4-BB06-03AE42C669BE',--Rock.Component.Geocode.StrikeIron
            '4AD002E9-A577-475E-93A8-98084CD10CBB',--Rock.Component.Geocode.TelaAtlas
            '1AEB3602-0AEC-49A3-B6A5-EF43900ADF05',--Rock.Component.Standardize.MelissaData
            'EB81C9AE-7122-4F7F-AF2B-414E4B53067C',--Rock.Component.Standardize.StrikeIron
            'B90973E1-AFC5-49DD-9145-79058C025811',--Rock.MEF.Geocode.ServiceObjects
            'B4C00A2D-06CA-4B62-8BBD-1D4FA8A3CDF4',--Rock.MEF.Geocode.StrikeIron
            'F71FDAB3-5E7F-4DB4-AE4C-F60D5F74463A',--Rock.MEF.Geocode.TelaAtlas
            '0AD91B7F-860E-4AE6-8D4D-1C12BF134523',--Rock.MEF.Standardize.MelissaData
            'E71993C1-9476-47D3-BF88-EAC28D9FE65B',--Rock.MEF.Standardize.StrikeIron
            '8E8792EC-8378-437D-9213-1DE4E52ABA9F',--Rock.Model.EmailTemplate
            '863DB29A-BBF4-4B22-83EF-3E24020669D0',--Rock.Model.FinancialGateway
            '9969FC04-2985-46C7-B10D-BF3835A9D931'--Rock.Model.PersonMerged       
            )
    )

    DELETE FROM EntityType
    WHERE [Guid] IN (
                    '5222D370-20D5-4D18-A6BF-4F1FA571321D',--Rock.Address.Geocode.ServiceObjects
            'E5C50259-609E-4294-837B-26ADE92BDF4E',--Rock.Address.Geocode.StrikeIron
            'DD23B7D2-3A12-432C-B1EE-722379AD808D',--Rock.Address.Geocode.TelaAtlas
            '51C31F4E-943B-4B83-956F-41685A16FCBC',--Rock.Address.Standardize.MelissaData
            'C4CE3228-EF10-4B4D-9FBC-95466134F4AC',--Rock.Address.Standardize.StrikeIron
            '049FAF56-E816-447A-AA60-BB22CE18BF87',--Rock.Component.Geocode.ServiceObjects
            '23EA1D4F-3F7A-48E4-BB06-03AE42C669BE',--Rock.Component.Geocode.StrikeIron
            '4AD002E9-A577-475E-93A8-98084CD10CBB',--Rock.Component.Geocode.TelaAtlas
            '1AEB3602-0AEC-49A3-B6A5-EF43900ADF05',--Rock.Component.Standardize.MelissaData
            'EB81C9AE-7122-4F7F-AF2B-414E4B53067C',--Rock.Component.Standardize.StrikeIron
            'B90973E1-AFC5-49DD-9145-79058C025811',--Rock.MEF.Geocode.ServiceObjects
            'B4C00A2D-06CA-4B62-8BBD-1D4FA8A3CDF4',--Rock.MEF.Geocode.StrikeIron
            'F71FDAB3-5E7F-4DB4-AE4C-F60D5F74463A',--Rock.MEF.Geocode.TelaAtlas
            '0AD91B7F-860E-4AE6-8D4D-1C12BF134523',--Rock.MEF.Standardize.MelissaData
            'E71993C1-9476-47D3-BF88-EAC28D9FE65B',--Rock.MEF.Standardize.StrikeIron
            '8E8792EC-8378-437D-9213-1DE4E52ABA9F',--Rock.Model.EmailTemplate
            '863DB29A-BBF4-4B22-83EF-3E24020669D0',--Rock.Model.FinancialGateway
            '9969FC04-2985-46C7-B10D-BF3835A9D931'--Rock.Model.PersonMerged
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
