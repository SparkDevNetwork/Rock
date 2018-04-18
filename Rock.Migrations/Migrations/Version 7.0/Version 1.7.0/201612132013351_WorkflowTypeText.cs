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
    public partial class WorkflowTypeText : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            AddColumn("dbo.WorkflowType", "SummaryViewText", c => c.String());
            AddColumn("dbo.WorkflowType", "NoActionMessage", c => c.String());

            Sql( @"
    UPDATE [WorkflowType] SET
        [SummaryViewText] = '
<div class=''row''>
    <div class=''col-sm-6''>
        <dl><dt>Started By</dt><dd>{{ Workflow.InitiatorPersonAlias.Person.FullName }}</dd></dl>
    </div>
    <div class=''col-sm-6''>
        <dl><dt>Started On</dt><dd>{{ Workflow.ActivatedDateTime | Date:''MM/dd/yyyy'' }} at {{ Workflow.ActivatedDateTime | Date:''hh:mm:ss tt'' }}</dd></dl>
    </div>
</div>

{% assign attributeList = '''' %}
{% for attribute in Workflow.AttributeValues %}
    {% if attribute.AttributeIsGridColumn %}
        {% assign attributeValue = attribute.ValueFormatted %}
        {% if attributeValue != '''' %}
            {% capture item %}<dt>{{ attribute.AttributeName }}</dt><dd>{{ attributeValue }}</dd>{% endcapture %}
            {% assign attributeList = attributeList | Append:item %}
        {% endif %}
    {% endif %}
{% endfor %}

{% if attributeList != '''' %}
    <div class=''row''>
        <div class=''col-sm-6''>
            <dl>
                {{ attributeList }}
            </dl>
        </div>
    </div>
{% endif %}
',
        [NoActionMessage] = '
This {{ Workflow.WorkflowType.WorkTerm }} does not currently require your attention.
'
" );
            // V5.2 Hotfix for adding birthdate filter to check-in
            Sql( @"
    UPDATE [Attribute] SET [EntityTypeQualifierValue] = '' WHERE [Guid] = 'F1A43EAB-D682-403F-A05E-CCFFBF879F32'
" );
            RockMigrationHelper.UpdateEntityAttribute( "Rock.Model.Group", "9C7D431C-875C-4792-9E76-93F3A32BB850", "GroupTypeId", "", "Birthdate Range", "The birth date range allowed to check in to these group types.", 0, "", "F1A43EAB-D682-403F-A05E-CCFFBF879F32" );
            Sql( @"
    DECLARE @GroupTypeId int = ( SELECT TOP 1 [Id] FROM [GroupType] WHERE [Guid] = '0572A5FE-20A4-4BF1-95CD-C71DB5281392' )
    DECLARE @AttributeId int = ( SELECT TOP 1 [Id] FROM [Attribute] WHERE [Guid] = 'F1A43EAB-D682-403F-A05E-CCFFBF879F32' )
    DECLARE @CategoryId int = ( SELECT TOP 1 [Id] FROM [Category] WHERE [Guid] = 'C8E0FD8D-3032-4ACD-9DB9-FF70B11D6BCC' )

    IF @GroupTypeId IS NOT NULL AND @AttributeId IS NOT NULL AND @CategoryId IS NOT NULL
    BEGIN
        UPDATE [Attribute] SET [EntityTypeQualifierValue] = CAST(@GroupTypeId AS VARCHAR) WHERE [Id] = @AttributeId
        IF NOT EXISTS ( SELECT * FROM [AttributeCategory] WHERE [AttributeId] = @AttributeId AND [CategoryId] = @CategoryId )
        BEGIN
            INSERT INTO [AttributeCategory] ( [AttributeId], [CategoryId] )
            VALUES ( @AttributeId, @CategoryId )
        END
    END
" );

        }

        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            DropColumn("dbo.WorkflowType", "NoActionMessage");
            DropColumn("dbo.WorkflowType", "SummaryViewText");
        }
    }
}
