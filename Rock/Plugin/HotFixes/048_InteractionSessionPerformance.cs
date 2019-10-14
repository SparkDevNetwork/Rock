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
    /// 
    /// </summary>
    /// <seealso cref="Rock.Plugin.Migration" />
    [MigrationNumber( 48, "1.7.0" )]
    public class InteractionSessionPerformance : Migration
    {
        /// <summary>
        /// The commands to run to migrate plugin to the specific version
        /// </summary>
        public override void Up()
        {
            // Moved to 201803201617508_DaysUntilAnniversary
            //            // JE: Fix missing period in confirm message
            //            Sql( @"
            //    UPDATE [Attribute]
            //        SET
            //            [DefaultValue] = '{0}, Your account has been confirmed.  Thank you for creating the account.'
            //    WHERE
            //        [Guid] = '22FCB059-AA40-45D9-BECB-C22D15A3D41A'
            //" );

            //            // Fix the Interaction Detail page setting on Interaction Session List Block
            //            RockMigrationHelper.AddBlockAttributeValue( "0FCDA1B8-B3F1-4E78-8FCF-E81F2CA77D05", "922BDC9E-D1ED-4553-B84D-1C301B291F5F", @"b6f6ab6f-a572-45fe-a143-2e4b8f192c8d" );

            //            // Add an index to help with performance of the Session List block
            //            Sql( @"
            //    IF EXISTS ( SELECT * FROM sys.indexes WHERE NAME = 'IX_PersonAliasId_InteractionSessionId' AND object_id = OBJECT_ID('Interaction') )
            //	DROP INDEX [IX_PersonAliasId_InteractionSessionId] ON [dbo].[Interaction]

            //    CREATE NONCLUSTERED INDEX [IX_PersonAliasId_InteractionSessionId]
            //    ON [dbo].[Interaction] ([PersonAliasId],[InteractionSessionId])
            //    INCLUDE ([InteractionDateTime],[InteractionComponentId])
            //" );

            //            // Update the default session list template to link pages directly to the page that was viewed
            //            RockMigrationHelper.UpdateBlockTypeAttribute( "EA90EF4F-C783-48CD-B575-AD785DE896E9", "1D0D3794-C210-48A8-8C68-3FBEC08A6BA5", "Default Template", "DefaultTemplate", "", @"The Lava template to use as default.", 2, @"
            //{% if InteractionChannel != null and InteractionChannel != '' %}
            //    {% for session in WebSessions %}
            //        <div class='panel panel-widget'>
            //	        <header class='panel-heading clearfix'>
            //	        <div class='pull-left'>
            //		        <h4 class='panel-title'>
            //		            {{ session.PersonAlias.Person.FullName }}
            //			        <small>
            //			            Started {{ session.StartDateTime }} / 
            //			            Duration: {{ session.StartDateTime | HumanizeTimeSpan:session.EndDateTime, 1 }}
            //			        </small>
            //		        </h4>
            //		        <span class='label label-primary'></span>
            //		        <span class='label label-info'>{{ InteractionChannel.Name }}</span>
            //		        </div> 
            //		        {% assign icon = '' %}
            //		        {% case session.InteractionSession.DeviceType.ClientType %}
            //			        {% when 'Desktop' %}{% assign icon = 'fa-desktop' %}
            //			        {% when 'Tablet' %}{% assign icon = 'fa-tablet' %}
            //			        {% when 'Mobile' %}{% assign icon = 'fa-mobile-phone' %}
            //			        {% else %}{% assign icon = '' %}
            //		        {% endcase %}
            //		        {% if icon != '' %}
            //    		        <div class='pageviewsession-client pull-right'>
            //                        <div class='pull-left'>
            //                            <small>{{ session.InteractionSession.DeviceType.Application }} <br>
            //                            {{ session.InteractionSession.DeviceType.OperatingSystem }} </small>
            //                        </div>
            //                        <i class='fa {{ icon }} fa-2x pull-right'></i>
            //                    </div>
            //                {% endif %}
            //	        </header>
            //	        <div class='panel-body'>
            //		        <ol>
            //		        {% for interaction in session.Interactions %}
            //    			    <li><a href = '{{ interaction.InteractionData }}'>{{ interaction.InteractionComponent.Name }}</a></li>
            //		        {% endfor %}				
            //		        </ol>
            //	        </div>
            //        </div>
            //    {% endfor %}
            //{% endif %}", "DF74EDE5-3B7D-4A79-B1F2-499D18FE6F2C" );

        }


        /// <summary>
        /// The commands to undo a migration from a specific version
        /// </summary>
        public override void Down()
        {

        }
    }
}
