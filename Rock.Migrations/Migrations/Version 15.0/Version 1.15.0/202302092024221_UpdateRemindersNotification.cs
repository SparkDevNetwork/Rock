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
    public partial class UpdateRemindersNotification : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            RemoveUnusedRoute();
            UpdateReminderCommunicationTemplate();
            AddDefaultReminderTypes();
        }

        /// <summary>
        /// Moved from plugin migration 162
        /// </summary>
        private void RemoveUnusedRoute()
        {
            Sql( "DELETE FROM [PageRoute] WHERE [Guid] = 'ADDFC440-A803-4F97-8E11-20C62519B1D4'" );
        }

        private void UpdateReminderCommunicationTemplate()
        {
            Sql( @"
UPDATE [SystemCommunication]
SET
	  [SMSMessage] = NULL
	, [PushTitle] = NULL
	, [PushMessage] = NULL
	, [PushSound] = NULL
	, [Subject] = 'Reminders for {{ ''Now'' | Date: ''ddd, MMMM d, yyyy'' }}'
	, [Body] = '{{ ''Global'' | Attribute:''EmailHeader'' }}

<h1 style=""margin:0;"">Your Reminders</h1>

<p>
    Below are your reminders for {{ ''Now'' |  Date:''MMMM d, yyyy'' }}.
</p>

{% assign lastEntityType = '''' %}
{% for reminder in Reminders %}
    {% if lastEntityType != reminder.EntityTypeName %}
        {% if lastEntityType != '''' %}
</table><hr/>
        {% endif %}
<h2>{{ reminder.EntityTypeName }}</h2>
<table role=""presentation"" cellspacing=""0"" cellpadding=""0"" border=""0"" width=""100%"">

        {% assign lastEntityType = reminder.EntityTypeName %}
    {% endif %}
    {% if reminder.IsPersonReminder %}
        <tr>
            <td valign=""middle"" style=""vertical-align:middle;width:50px !important;"" width=""50px""><img src=""{{ reminder.PersonProfilePictureUrl }}&w=50"" width=""50"" height=""50"" alt="""" style=""display:block;width:50px;height:50px;border-radius:6px;""></td>
            <td style=""vertical-align:middle;padding-left:12px;"">
                {% if reminder.EntityUrl == '''' %}
                    <a href=""{{ reminder.EntityUrl }}"" style=""font-weight:700"">{{ reminder.EntityDescription }}</a>
                {% else %}
                    <span style=""font-weight:700"">{{ reminder.EntityDescription }}</span>
                {% endif %}
                <br />
                <span style=""font-size:12px;""><span style=""color:blueviolet"">&#11044;</span> {{ reminder.ReminderTypeName }}</span>
            </td>
        </tr>
        <tr>
            <td valign=""middle"" style=""vertical-align:middle;width:50px !important;"" width=""50px""></td>
            <td style=""padding-left:12px;padding-right:12px;padding-bottom:24px;"">
                <p>{{reminder.Note}}</p>
            </td>
        </tr>
    {% else %}
        <tr>
            <td style=""padding-bottom:24px;"">
                {% if reminder.EntityUrl == """" %}
                    <a href=""{{ reminder.EntityUrl }}"" style=""font-weight:700"">{{ reminder.EntityDescription }}</a>
                {% else %}
                    <span style=""font-weight:700"">{{ reminder.EntityDescription }}</span>
                {% endif %}
                <br />
                <span style=""font-size:14px;""><span style=""color:blueviolet"">&#11044;</span> {{ reminder.ReminderTypeName }}</span>
                <p>{{reminder.Note}}</p>
            </td>
        </tr>
    {% endif %}
{% endfor %}

{{ ''Global'' | Attribute:''EmailFooter'' }}'
WHERE
	[Guid] = '7899958C-BC2F-499E-A5CC-11DE1EF8DF20'
" );
        }

        private void AddDefaultReminderTypes()
        {
            Sql( @"
DECLARE @personEntityTypeId INT = (SELECT [Id] FROM [EntityType] WHERE [Name] = 'Rock.Model.Person');
IF NOT EXISTS(SELECT [Id] FROM [ReminderType] WHERE [Guid] = 'A9BAAA29-F306-4E35-9273-4B299676B252')
INSERT INTO [ReminderType] ([Name] ,[Description] ,[IsActive] ,[NotificationType] ,[ShouldShowNote] ,[Order] ,[EntityTypeId] ,[ShouldAutoCompleteWhenNotified] ,[HighlightColor] ,[Guid])
VALUES ('Personal Reminder', 'Used for general reminders for people.', 1, 0, 1, 0, @personEntityTypeId, 0, '#fff', 'A9BAAA29-F306-4E35-9273-4B299676B252');

DECLARE @groupEntityTypeId INT = (SELECT [Id] FROM [EntityType] WHERE [Name] = 'Rock.Model.Group');
IF NOT EXISTS(SELECT [Id] FROM [ReminderType] WHERE [Guid] = 'CF5D8F88-8BF0-4880-88BC-102B2AE6159D')
INSERT INTO [ReminderType] ([Name] ,[Description] ,[IsActive] ,[NotificationType] ,[ShouldShowNote] ,[Order] ,[EntityTypeId] ,[ShouldAutoCompleteWhenNotified] ,[HighlightColor] ,[Guid])
VALUES ('Group Reminder', 'General reminder type for groups.', 1, 0, 1, 0, @groupEntityTypeId, 0, '#fff', 'CF5D8F88-8BF0-4880-88BC-102B2AE6159D');
" );
        }

        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            // Down migrations are not yet supported in plug-in migrations.
        }
    }
}
