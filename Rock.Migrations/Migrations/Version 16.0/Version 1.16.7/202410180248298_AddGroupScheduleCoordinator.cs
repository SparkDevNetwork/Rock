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
    public partial class AddGroupScheduleCoordinator : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            DropForeignKey( "dbo.Group", "ScheduleCancellationPersonAliasId", "dbo.PersonAlias" );

            RenameColumn( "dbo.Group", "ScheduleCancellationPersonAliasId", "ScheduleCoordinatorPersonAliasId" );
            RenameIndex( "dbo.Group", "IX_ScheduleCancellationPersonAliasId", "IX_ScheduleCoordinatorPersonAliasId" );

            AddForeignKey( "dbo.Group", "ScheduleCoordinatorPersonAliasId", "dbo.PersonAlias", "Id" );

            AddColumn( "dbo.Group", "ScheduleCoordinatorNotificationTypes", c => c.Int() );
            AddColumn( "dbo.GroupType", "ScheduleCoordinatorNotificationTypes", c => c.Int() );

            var multilineBodyColumn = RockMigrationHelper.NormalizeColumnCRLF( "Body" );

            var multilineScheduledItemTemplate = @"{{ Group.Name }}
{{ ScheduledItem.Location.Name }} - {{ ScheduledItem.Schedule.Name }}";

            Sql( $@"
-- Update existing group types to have a value of 'Decline' to match current behavior.
UPDATE [GroupType]
SET [ScheduleCoordinatorNotificationTypes] = 2;

-- Update Scheduling Response email subject:
UPDATE [SystemCommunication]
SET [Subject] = 'Schedule Response: {{%- assign rsvp = ScheduledItem.RSVP | AsBoolean -%}} {{% if rsvp %}} Accepted{{% else %}} Declined{{% endif %}} - {{{{ Person.FullName }}}} to {{{{ Group.Name }}}}'
WHERE [Guid] = 'D095F78D-A5CF-4EF6-A038-C7B07E250611'
    AND [Subject] = '{{%- assign rsvp = ScheduledItem.RSVP | AsBoolean -%}} {{% if rsvp %}}Accepted{{% else %}}Declined{{% endif %}}';

-- Update Scheduling Response email body:
UPDATE [SystemCommunication]
SET [Body] = REPLACE([Body], '{{{{ ''Global'' | Attribute:''EmailHeader'' }}}}', '{{%- assign selfScheduled = false -%}}
{{%- if Person.Id == Scheduler.Id %}}{{%- assign selfScheduled = true -%}}{{%- endif -%}}
{{{{ ''Global'' | Attribute:''EmailHeader'' }}}}')
WHERE [Guid] = 'D095F78D-A5CF-4EF6-A038-C7B07E250611'
    AND [Body] LIKE '{{{{ ''Global'' | Attribute:''EmailHeader'' }}}}%'
    AND [Body] LIKE '%{{{{ Person.FullName }}}}%';

UPDATE [SystemCommunication]
SET [Body] = REPLACE([Body], '{{{{ Person.FullName }}}}', '{{{{ Person.FullName }}}}{{% if selfScheduled %}} (self-scheduled){{% endif %}}')
WHERE [Guid] = 'D095F78D-A5CF-4EF6-A038-C7B07E250611'
    AND [Body] LIKE '%assign selfScheduled = false%'
    AND [Body] NOT LIKE '%{{\% if selfScheduled \%}} (self-scheduled){{\% endif \%}}%'
    ESCAPE '\';

UPDATE [SystemCommunication]
SET [Body] = REPLACE({multilineBodyColumn}, '{multilineScheduledItemTemplate}', '{{{{ Group.Name }}}}
<br/>
{{% if ScheduledItem.Occurrence.Location.Name %}}{{{{ ScheduledItem.Occurrence.Location.Name }}}} - {{% endif %}}{{{{ ScheduledItem.Occurrence.Schedule.Name }}}}')
WHERE [Guid] = 'D095F78D-A5CF-4EF6-A038-C7B07E250611'
    AND [Body] LIKE '%{{{{ ScheduledItem.Location.Name }}}} - {{{{ ScheduledItem.Schedule.Name }}}}%';
" );
        }

        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            var multilineBodyColumn = RockMigrationHelper.NormalizeColumnCRLF( "Body" );

            var multilineVariableTemplate = @"{%- assign selfScheduled = false -%}
{%- if Person.Id == Scheduler.Id %}{%- assign selfScheduled = true -%}{%- endif -%}
{{ ''Global'' | Attribute:''EmailHeader'' }}";

            var multilineScheduledItemTemplate = @"{{ Group.Name }}
<br/>
{% if ScheduledItem.Occurrence.Location.Name %}{{ ScheduledItem.Occurrence.Location.Name }} - {% endif %}{{ ScheduledItem.Occurrence.Schedule.Name }}";

            Sql( $@"
-- Update Scheduling Response email subject:
UPDATE [SystemCommunication]
SET [Subject] = '{{%- assign rsvp = ScheduledItem.RSVP | AsBoolean -%}} {{% if rsvp %}}Accepted{{% else %}}Declined{{% endif %}}'
WHERE [Guid] = 'D095F78D-A5CF-4EF6-A038-C7B07E250611'
    AND [Subject] = 'Schedule Response: {{%- assign rsvp = ScheduledItem.RSVP | AsBoolean -%}} {{% if rsvp %}} Accepted{{% else %}} Declined{{% endif %}} - {{{{ Person.FullName }}}} to {{{{ Group.Name }}}}';

-- Update Scheduling Response email body:
UPDATE [SystemCommunication]
SET [Body] = REPLACE({multilineBodyColumn}, '{multilineVariableTemplate}', '{{{{ ''Global'' | Attribute:''EmailHeader'' }}}}')
WHERE [Guid] = 'D095F78D-A5CF-4EF6-A038-C7B07E250611'
    AND [Body] LIKE '%assign selfScheduled = false%';

UPDATE [SystemCommunication]
SET [Body] = REPLACE([Body], '{{{{ Person.FullName }}}}{{% if selfScheduled %}} (self-scheduled){{% endif %}}', '{{{{ Person.FullName }}}}')
WHERE [Guid] = 'D095F78D-A5CF-4EF6-A038-C7B07E250611'
    AND [Body] LIKE '%{{{{ Person.FullName }}}}{{\% if selfScheduled \%}} (self-scheduled){{\% endif \%}}%'
    ESCAPE '\';

UPDATE [SystemCommunication]
SET [Body] = REPLACE({multilineBodyColumn}, '{multilineScheduledItemTemplate}', '{{{{ Group.Name }}}}
{{{{ ScheduledItem.Location.Name }}}} - {{{{ ScheduledItem.Schedule.Name }}}}')
WHERE [Guid] = 'D095F78D-A5CF-4EF6-A038-C7B07E250611'
    AND [Body] LIKE '%{{\% if ScheduledItem.Occurrence.Location.Name \%}}{{{{ ScheduledItem.Occurrence.Location.Name }}}} - {{\% endif \%}}{{{{ ScheduledItem.Occurrence.Schedule.Name }}}}%'
    ESCAPE '\';
" );

            DropColumn( "dbo.GroupType", "ScheduleCoordinatorNotificationTypes" );
            DropColumn( "dbo.Group", "ScheduleCoordinatorNotificationTypes" );

            DropForeignKey( "dbo.Group", "ScheduleCoordinatorPersonAliasId", "dbo.PersonAlias" );

            RenameColumn( "dbo.Group", "ScheduleCoordinatorPersonAliasId", "ScheduleCancellationPersonAliasId" );
            RenameIndex( "dbo.Group", "IX_ScheduleCoordinatorPersonAliasId", "IX_ScheduleCancellationPersonAliasId" );

            AddForeignKey( "dbo.Group", "ScheduleCancellationPersonAliasId", "dbo.PersonAlias", "Id" );
        }
    }
}
