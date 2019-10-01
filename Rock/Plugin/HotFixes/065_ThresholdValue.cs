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
    /// Updates the check-in success template to include error messages when capacity increases threshold value
    /// Most systems will have applied this change in 064_MigrationRollupsForV8_7_2, this is for 8.6 systems
    /// that require the functionality before 8.7 is released.
    /// </summary>
    /// <seealso cref="Rock.Plugin.Migration" />
    [MigrationNumber(65,"1.8.5")]
    public class ThresholdValue :Migration
    {
        /// <summary>
        /// The commands to run to migrate plugin to the specific version
        /// </summary>
        public override void Up()
        {
            //UpdatedCheckInSuccessTemplate();
        }

        /// <summary>
        /// The commands to undo a migration from a specific version
        /// </summary>
        public override void Down()
        {
            // This functionality is not yet available for hotfix migrations
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

    }
}
