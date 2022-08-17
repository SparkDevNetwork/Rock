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
    public partial class Rollup_20220718 : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            ChartFix();
            GroupViewLavaTemplateUpdate();
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
        }

        /// <summary>
        /// GJ: 13.7 Chart Fix
        /// </summary>
        private void ChartFix()
        {
            Sql( MigrationSQL._202207182110401_Rollup_20220718_updatechart );
        }

        /// <summary>
        /// ED: Add check for GroupCapacityRule to GroupViewLavaTemplate
        /// </summary>
        private void GroupViewLavaTemplateUpdate()
        {
            Sql( @"
                UPDATE [GroupType]
                SET [GroupViewLavaTemplate] = REPLACE(
	                    [GroupViewLavaTemplate]
	                , '{% if Group.GroupCapacity != null and Group.GroupCapacity != '''' %}'
	                , '{% if Group.GroupType.GroupCapacityRule != ''None'' and Group.GroupCapacity != null and Group.GroupCapacity != '''' %}')
                WHERE [GroupViewLavaTemplate] LIKE '%{% if Group.GroupCapacity != null and Group.GroupCapacity != '''' %}%'

                UPDATE [Attribute]
                SET [DefaultValue] = REPLACE(
	                [DefaultValue]
	                , '{% if Group.GroupCapacity != null and Group.GroupCapacity != '''' %}'
	                , '{% if Group.GroupType.GroupCapacityRule != ''None'' and Group.GroupCapacity != null and Group.GroupCapacity != '''' %}')
                WHERE [Key] = 'core_templates_GroupViewTemplate'" );
        }
    }
}
