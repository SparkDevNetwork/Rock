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
namespace Rock.Plugin.HotFixes
{
    /// <summary>
    /// This hotfix update is to address security concerns in the file uploader and is being retroactivly applied to v7.6 and >.
    /// </summary>
    /// <seealso cref="Rock.Plugin.Migration" />
    [MigrationNumber(64,"1.8.6")]
    public class MigrationRollupsForV8_7_2 :Migration
    {
        /// <summary>
        /// The commands to run to migrate plugin to the specific version
        /// </summary>
        public override void Up()
        {
            //UpdateCommunicationSettings();
            //UpdatedCheckInSuccessTemplate();
            //UpdatedAttendanceSummaryEmail();
            //UpdateContentChannelView();
            //RemoveDuplicateIndexes();
        }

        /// <summary>
        /// The commands to undo a migration from a specific version
        /// </summary>
        public override void Down()
        {
            
        }

        /// <summary>Updates the communication settings.
        ///SK: Updated Communication Settings Name and description
        /// </summary>
        private void UpdateCommunicationSettings()
        {
            Sql( @"
UPDATE 
	[Attribute]
SET
	[Name]='Enable Do Not Disturb'
	, [Description] = 'When enabled, communications will not be sent between the starting and ending ''Do Not Disturb'' hours.'
WHERE
	[Guid]='1BE30413-5C90-4B78-B324-BD31AA83C002'

UPDATE 
	[Attribute]
SET
	[Name]='Do Not Disturb Start'
	, [Description] = 'The hour that starts the Do Not Disturb window.'
WHERE
	[Guid]='4A558666-32C7-4490-B860-0F41358E14CA'

UPDATE 
	[Attribute]
SET
	[Name]='Do Not Disturb End'
	, [Description] = 'The hour that ends the Do Not Disturb window.'
WHERE
	[Guid]='661802FC-E636-4CE2-B75A-4AC05595A347'
" );
        }

        /// <summary>Updateds the check in success template.
        ///SK: Updated Check-in Success Template to include error messages when capacity increases threshold value
        /// </summary>
        private void UpdatedCheckInSuccessTemplate()
        {
            Sql( @"
DECLARE @CheckInSuccessTemplateId INT = (
        SELECT TOP 1 Id
        FROM Attribute
        WHERE [Guid] = 'F5BA6DCC-0A4D-4616-871D-ECBA7082C45F'
        )

UPDATE Attribute
SET [DefaultValue] = '<ol class=''checkin-messages checkin-body-container''>
{% for checkinMessage in Messages %}
    <li><div class=''alert alert-{{ checkinMessage.MessageType }}''>
        {{ checkinMessage.MessageText }}
        </div>
    </li>
    {% endfor %}
</ol>
' + [DefaultValue]
WHERE [Id] = @CheckInSuccessTemplateId AND [DefaultValue] NOT LIKE '%checkinMessage in Messages%'

UPDATE
	[AttributeValue]
SET [Value] = '<ol class=''checkin-messages checkin-body-container''>
{% for checkinMessage in Messages %}
    <li><div class=''alert alert-{{ checkinMessage.MessageType }}''>
        {{ checkinMessage.MessageText }}
        </div>
    </li>
    {% endfor %}
</ol>
' + [Value]
WHERE
[AttributeId] =@CheckInSuccessTemplateId AND [Value] NOT LIKE '%checkinMessage in Messages%'
" );

        }

        /// <summary>Updateds the attendance summary email.
        ///SK: Updated Attendance Summary Email to always include the notes if exists.
        /// </summary>
        private void UpdatedAttendanceSummaryEmail()
        {
            Sql( @"
UPDATE
	 SystemEmail
SET [Body] = Replace([Body],'{%- if AttendanceOccurrence.DidNotOccur == true -%}
The {{ Group.Name }} group did not meet.
{%- else -%}
{%- if AttendanceOccurrence.Notes -%}
<p>{{ AttendanceNoteLabel }}: <br/> {{ AttendanceOccurrence.Notes }}</p>
{%- endif -%}','{%- if AttendanceOccurrence.DidNotOccur == true -%}
The {{ Group.Name }} group did not meet.
{%- if AttendanceOccurrence.Notes -%}
<p>{{ AttendanceNoteLabel }}: <br/> {{ AttendanceOccurrence.Notes }}</p>
{%- endif -%}
{%- else -%}
{%- if AttendanceOccurrence.Notes -%}
<p>{{ AttendanceNoteLabel }}: <br/> {{ AttendanceOccurrence.Notes }}</p>
{%- endif -%}')
WHERE
	 [Guid]='CA794BD8-25C5-46D9-B7C2-AD8190AC27E6'
	 AND [Body] LIKE '%{%- if AttendanceOccurrence.DidNotOccur == true -%}
The {{ Group.Name }} group did not meet.
{%- else -%}
{%- if AttendanceOccurrence.Notes -%}
<p>{{ AttendanceNoteLabel }}: <br/> {{ AttendanceOccurrence.Notes }}</p>
{%- endif -%}%'
" );
        }

        /// <summary>Updates the content channel view.
        ///SK: Updated Content Channel view detail to use Page picker for detail page setting instead of directly providing Id
        /// </summary>
        private void UpdateContentChannelView()
        {
            Sql( @"
DECLARE @BlockTypeId int, @AttributeId int

SET @BlockTypeId = (SELECT [Id] FROM [BlockType] WHERE [Guid] = '63659EBE-C5AF-4157-804A-55C7D565110E')
SET @AttributeId = (SELECT [Id] FROM [Attribute] WHERE [Key]='DetailPage' AND [EntityTypeQualifierColumn]='BlockTypeId' AND [EntityTypeQualifierValue]=@BlockTypeId)

UPDATE [AttributeValue]
SET [AttributeValue].[Value] = [Page].[Guid]
FROM [AttributeValue]
INNER JOIN [Page] ON [AttributeValue].[Value] = CONVERT(nvarchar(10), [Page].[Id])
WHERE [AttributeValue].[AttributeId] = @AttributeId
" );
        }

        /// <summary>Removes the duplicate indexes.
        ///SK:  Remove duplicate indexes from BinaryFileData and FinancialTransactionRefund
        /// </summary>
        private void RemoveDuplicateIndexes()
        {
            Sql( @"
IF EXISTS( SELECT * FROM sys.indexes WHERE name = 'IX_Id' AND object_id = OBJECT_ID( N'[dbo].[BinaryFileData]' ) )
DROP INDEX[IX_Id] ON[dbo].[BinaryFileData]

IF EXISTS( SELECT * FROM sys.indexes WHERE name = 'IX_Id' AND object_id = OBJECT_ID( N'[dbo].[FinancialTransactionRefund]' ) )
DROP INDEX[IX_Id] ON[dbo].[FinancialTransactionRefund]
" );
        }

    }
}
