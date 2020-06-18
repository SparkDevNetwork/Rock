﻿// <copyright>
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
    /// Plugin Migration. The migration number jumps to 83 because 75-82 were moved to EF migrations and deleted.
    /// </summary>
    [MigrationNumber( 84, "1.9.0" )]
    public class FixSpiritualGiftsAssessmentBadge : Migration
    {
        /// <summary>
        /// The commands to run to migrate plugin to the specific version
        /// </summary>
        public override void Up()
        {
//            Sql( $@"UPDATE [dbo].[AssessmentType]
//SET [BadgeSummaryLava] = '{{% assign gifts = Person | Attribute:''core_DominantGifts'' %}}
//{{% if gifts contains '','' %}}
//  {{% assign gifts = Person | Attribute:''core_DominantGifts'',''Object'' %}}
//  {{{{ gifts | Map:''Value'' | Join:'', '' | ReplaceLast:'','','' and'' }}}}
//{{% else %}}
//  {{{{ gifts }}}}
//{{% endif %}}'
//WHERE [Guid] = '{Rock.SystemGuid.AssessmentType.GIFTS}'" );
        }

        /// <summary>
        /// The commands to undo a migration from a specific version
        /// </summary>
        public override void Down()
        {
            // Not yet used by hotfix migrations.
        }
    }
}
