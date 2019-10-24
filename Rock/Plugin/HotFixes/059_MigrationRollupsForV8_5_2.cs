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
    [MigrationNumber( 59, "1.8.4" )]
    public class MigrationRollupsForV8_5_2 : Migration
    {
        /// <summary>
        /// The commands to run to migrate plugin to the specific version
        /// </summary>
        public override void Up()
        {
            //FixTextToWorkflowKeywordExpression();
            //FixTrueWiringIcon();
            //FixCurrentDayDateMergeField();
            //FixspCheckin_AttendanceAnalyticsQuery_NonAttendees();
        }


        /// <summary>
        /// The commands to undo a migration from a specific version
        /// </summary>
        public override void Down()
        {
        }

        /// <summary>
        /// NA: Fix KeywordExpression attribute description of the 'Text To Workflow' WF
        /// </summary>
        private void FixTextToWorkflowKeywordExpression()
        {
            Sql( @"UPDATE [Attribute] SET
                [Description] = 'The incoming keyword regular expression to match to (e.g. ''regist.*'' would match ''register'' and ''registration'')'
                WHERE [Guid] = '3A526D6C-06FC-46CD-A447-9A6D9A74BB4F'" );
        }

        /// <summary>
        /// NA: Fix Spiritual Gifts "TrueWiring" icon on the extended attributes
        /// </summary>
        private void FixTrueWiringIcon()
        {
            RockMigrationHelper.UpdatePersonAttributeCategory( "TrueWiring", "fa fa-directions", string.Empty, "B08A3096-FCFA-4DA0-B95D-1F3F11CC9969" );
        }

        /// <summary>
        /// NA: Fix the broken 'MergeField' attribute value for the "Current Day/Date" Label Merge Field (Defined Type) value
        /// </summary>
        private void FixCurrentDayDateMergeField()
        {
            Sql( @"DECLARE @MergeFieldAttribId INT = ( SELECT [Id] FROM [Attribute] WHERE [Guid] = '51EB8583-55EA-4431-8B66-B5BD0F83D81E' )
                DECLARE @CurrDayDefinedValueId INT = ( SELECT [Id] FROM DefinedValue WHERE [Guid] = '23502286-D921-4455-BABC-D8D6CB8FFB3D' )

                UPDATE [AttributeValue]
                SET [Value] = '{{ ''Now'' | Date:''ddd M/d'' }}'
                WHERE [AttributeId] = @MergeFieldAttribId
                    AND [EntityId] = @CurrDayDefinedValueId
                    AND [Value] = '{{ ''''Now'''' | Date:''''ddd M/d'''' }}'" );
        }

        /// <summary>
        /// MP: Fix Invalid Column error in spCheckin_AttendanceAnalyticsQuery_NonAttendees
        /// </summary>
        private void FixspCheckin_AttendanceAnalyticsQuery_NonAttendees()
        {
            Sql( HotFixMigrationResource._059_MigrationRollupsForV8_5_2_spCheckin_AttendanceAnalyticsQuery_NonAttendees );
        }
    }
}
