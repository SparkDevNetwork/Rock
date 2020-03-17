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

namespace Rock.Plugin.HotFixes
{
    /// <summary>
    /// Plug-in migration
    /// </summary>
    /// <seealso cref="Rock.Plugin.Migration" />
    [MigrationNumber( 107, "1.10.0" )]
    public class MigrationRollupsFor10_3_1 : Migration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            //RemoveCommunicationPreference();
            //UpdateEmailMediumAttributeNonHtmlContent();
            //UpdateCheckinSuccessMessage();
            //UpdateEventListTemplateType();
            //UpdatePrayerSessionTemplateType();

        }

        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            // Down migrations are not yet supported in plug-in migrations.
        }

        /// <summary>
        /// MB: Remove Communication Preference Attribute from Communication List.
        /// </summary>
        private void RemoveCommunicationPreference()
        {
            Sql( @"
                DECLARE @attributeId AS INT
                SELECT @attributeId = Id
                FROM [dbo].[Attribute] 
                WHERE [Guid] = 'D7941908-1F65-CC9B-416C-CCFABE4221B9'

                IF @attributeId IS NOT NULL
                BEGIN
	                UPDATE GroupMember
	                SET GroupMember.CommunicationPreference = CASE 
		                WHEN ValueAsNumeric = 1 THEN 1
		                WHEN ValueAsNumeric = 2 THEN 2
		                ELSE 0 END
	                FROM [dbo].[AttributeValue] 
	                WHERE AttributeId = @attributeId 
			                AND [AttributeValue].EntityId = GroupMember.Id
			                AND GroupMember.CommunicationPreference <> ValueAsNumeric

	                WHILE (SELECT COUNT(1) FROM [dbo].[AttributeValue]
			                WHERE AttributeId = @attributeId) > 0
	                BEGIN
		                DELETE TOP(100000) [dbo].[AttributeValue]
		                WHERE AttributeId = @attributeId
	                END
	
	                DELETE [dbo].[Attribute]
	                WHERE Id = @attributeId
                END" );
        }

        /// <summary>
        /// ED: Update Email Medium Attribute NonHtmlContent
        /// </summary>
        private void UpdateEmailMediumAttributeNonHtmlContent()
        {
            Sql( @"
                UPDATE [Attribute]
                SET [DefaultValue] = REPLACE([DefaultValue], 'GetCommunication.ashx?c={{ Communication.Id }}', 'GetCommunication.ashx?c={{ Communication.Guid }}')
                WHERE [Guid] = 'FDB3E4EB-DE16-4A43-AE92-B4EAA3D5DF88'
                UPDATE [AttributeValue]
                SET [Value] = REPLACE([Value], 'GetCommunication.ashx?c={{ Communication.Id }}', 'GetCommunication.ashx?c={{ Communication.Guid }}')
                WHERE [AttributeId] IN (SELECT [Id] FROM [Attribute] WHERE [Guid] = 'FDB3E4EB-DE16-4A43-AE92-B4EAA3D5DF88')" );
        }

        #region MB: Update CheckIn Success Message

        private string GetNewLavaTemplate()
        {
            return @"<ol class='checkin-summary checkin-body-container'>
                {% for checkinResult in CheckinResultList %}
                    {% if RegistrationModeEnabled == true %}
                        <li>{{ checkinResult.DetailMessage }}</li>
                    {% else %}
                        <li>{{ checkinResult.DetailMessage }}</li>
                    {% endif %}
                {% endfor %}
            </ol>
            <ol class='checkin-error'>
                {% comment %}Display any error messages from the label printer{% endcomment %}
                {% for message in ZebraPrintMessageList %}
                    <li>{{ message }}</li>
                {% endfor %}
            </ol>";
        }

        private string GetPreviousLavaTemplate()
        {
            return @"<ol class=""checkin-summary checkin-body-container"">

            {% for checkinResult in CheckinResultList %}
                {% if RegistrationModeEnabled == true %}
                    <li>{{ checkinResult.DetailMessage }}</li>
                {% else %}
                    <li>{{ checkinResult.DetailMessage }}</li>
                {% endif %}
            {% endfor %}

            {% comment %}Display any error messages from the label printer{% endcomment %}
            {% for message in ZebraPrintMessageList %}
                <br/>{{ message }}
            {% endfor %}

            </ol>";
        }

        /// <summary>
        /// MB: Update CheckIn Success Message
        /// </summary>
        private void UpdateCheckinSuccessMessage()
        {
            var newLavaTemplate = GetNewLavaTemplate();
            var previousLavaTemplate = GetPreviousLavaTemplate();

            newLavaTemplate = newLavaTemplate.Replace( "'", "''" );
            previousLavaTemplate = previousLavaTemplate.Replace( "'", "''" );

            // Use NormalizeColumnCRLF when attempting to do a WHERE clause or REPLACE using multi line strings!
            var targetColumn = RockMigrationHelper.NormalizeColumnCRLF( "DefaultValue" );

            Sql( $@"UPDATE [dbo].[Attribute] 
                    SET [DefaultValue] = REPLACE({targetColumn}, '{previousLavaTemplate}', '{newLavaTemplate}')
                    WHERE {targetColumn} LIKE '%{previousLavaTemplate}%'
                            AND [Key] = 'core_checkin_SuccessLavaTemplate'" );
        }

        #endregion MB: Update CheckIn Success Message

        #region DH: Mobile Prayer and Event List block templates

        /// <summary>
        /// Updates the Defined Value of the event list template.
        /// </summary>
        private void UpdateEventListTemplateType()
        {
            RockMigrationHelper.UpdateDefinedValue(
                Rock.SystemGuid.DefinedType.TEMPLATE_BLOCK,
                "Mobile Calendar Event List",
                string.Empty,
                Rock.SystemGuid.DefinedValue.BLOCK_TEMPLATE_MOBILE_CALENDAR_EVENT_LIST );
        }

        /// <summary>
        /// Updates the Defined Value of the prayer session template.
        /// </summary>
        private void UpdatePrayerSessionTemplateType()
        {
            RockMigrationHelper.UpdateDefinedValue(
                Rock.SystemGuid.DefinedType.TEMPLATE_BLOCK,
                "Mobile Prayer Session",
                string.Empty,
                Rock.SystemGuid.DefinedValue.BLOCK_TEMPLATE_MOBILE_PRAYER_SESSION );
        }

        #endregion DH: Mobile Prayer and Event List block templates
    }
}
