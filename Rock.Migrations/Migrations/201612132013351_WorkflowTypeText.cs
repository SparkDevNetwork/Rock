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
<p>This {{ Workflow.WorkflowType.WorkTerm }} was started {{ Workflow.ActivatedDateTime | Date:''dddd, MMMM d, yyyy'' }} at {{ Workflow.ActivatedDateTime | Date:''hh:mm tt'' }} 
{% if Workflow.InitiatorPersonAlias %}by <strong><i>{{ Workflow.InitiatorPersonAlias.Person.FullName }}</i></strong>{% endif %} and
{% if Workflow.IsActive %}is still active with a status of <i>{{ Workflow.Status }}</i>{% else %}was completed {{ Workflow.CompletedDateTime | Date:''dddd, MMMM d, yyyy'' }} at {{ Workflow.CompletedDateTime | Date:''hh:mm tt'' }}{% endif %}.</p>

<p>The following activities have been started:
<ul>
    {% for activity in Workflow.Activities %}
        <li>
            <strong><i>{{ activity.ActivityType.Name }}</i></strong> was started {{ activity.ActivatedDateTime | Date:''dddd, MMMM d, yyyy'' }} at {{ activity.ActivatedDateTime | Date:''hh:mm tt'' }} and
            {% if activity.IsActive %}is still active{% else %}was completed {{ activity.CompletedDateTime | Date:''dddd, MMMM d, yyyy'' }} at {{ activity.CompletedDateTime | Date:''hh:mm tt'' }}{% endif %}.
            {% if activity.IsActive and activity.AssignedPersonAlias %}
                It is assigned to <strong><i>{{ activity.AssignedPersonAlias.Person.FullName }}</i></strong>.
            {% endif %}
            {% if activity.IsActive and activity.AssignedGroup %}
                It is assigned to the <strong><i>{{ activity.AssignedGroup.Name }}</i></strong> group.
            {% endif %}
        </li>
    {% endfor %}
</ul>
</p>

{% assign attributeList = '''' %}
{% for attribute in Workflow.AttributeValues %}
    {% if attribute.AttributeIsGridColumn %}
        {% assign attributeValue = attribute.ValueFormatted %}
        {% if attributeValue != '''' %}
            {% capture item %}<li><strong>{{ attribute.AttributeName }}</strong>: {{ attributeValue }}</li>{% endcapture %}
            {% assign attributeList = attributeList | Append:item %}
        {% endif %}
    {% endif %}
{% endfor %}

{% if attributeList != '''' %}
    <p>Below are values specific to this {{ Workflow.WorkflowType.WorkTerm }}:
    <ul>
        {{ attributeList }}
    </ul>
    </p>
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
