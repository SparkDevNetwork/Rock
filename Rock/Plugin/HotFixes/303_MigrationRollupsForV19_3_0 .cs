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

using System;

namespace Rock.Plugin.HotFixes
{
    /// <summary>
    /// Plug-in migration
    /// </summary>
    /// <seealso cref="Rock.Plugin.Migration" />
    [MigrationNumber( 303, "19.3" )]
    public class MigrationRollupsForV19_3_0 : Migration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            JE_TablerIconReplaceInAttributeDefaultValues_Up();
        }

        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            // Down migrations are not yet supported in plug-in migrations.
        }

        private void JE_TablerIconReplaceInAttributeDefaultValues_Up()
        {
            Sql( @"
UPDATE [Attribute]
SET
    [DefaultValue] = REPLACE( [DefaultValue], 'fa fa-gear', 'ti ti-settings' ),
    [ModifiedDateTime] = GETDATE()
WHERE [DefaultValue] LIKE '%fa fa-gear%'

UPDATE [Attribute]
SET
    [DefaultValue] = REPLACE( [DefaultValue], 'fa fa-arrow-circle-right', 'ti ti-circle-arrow-right' ),
    [ModifiedDateTime] = GETDATE()
WHERE [DefaultValue] LIKE '%fa fa-arrow-circle-right%'

UPDATE [Attribute]
SET
    [DefaultValue] = REPLACE( [DefaultValue], 'fa fa-user-lock', 'ti ti-lock' ),
    [ModifiedDateTime] = GETDATE()
WHERE [DefaultValue] LIKE '%fa fa-user-lock%'

UPDATE [Attribute]
SET
    [DefaultValue] = REPLACE( [DefaultValue], 'fas fa-user-lock', 'ti ti-lock' ),
    [ModifiedDateTime] = GETDATE()
WHERE [DefaultValue] LIKE '%fas fa-user-lock%'

UPDATE [Attribute]
SET
    [DefaultValue] = REPLACE( [DefaultValue], 'fa fa-flag', 'ti ti-flag' ),
    [ModifiedDateTime] = GETDATE()
WHERE [DefaultValue] LIKE '%fa fa-flag%'

UPDATE [Attribute]
SET
    [DefaultValue] = REPLACE( [DefaultValue], 'fa fa-calendar-alt', 'ti ti-calendar' ),
    [ModifiedDateTime] = GETDATE()
WHERE [DefaultValue] LIKE '%fa fa-calendar-alt%'

UPDATE [Attribute]
SET
    [DefaultValue] = REPLACE( [DefaultValue], 'fa fa-shield-alt', 'ti ti-shield' ),
    [ModifiedDateTime] = GETDATE()
WHERE [DefaultValue] LIKE '%fa fa-shield-alt%'

UPDATE [Attribute]
SET
    [DefaultValue] = REPLACE( [DefaultValue], 'fas fa-shield-alt', 'ti ti-shield' ),
    [ModifiedDateTime] = GETDATE()
WHERE [DefaultValue] LIKE '%fas fa-shield-alt%'

UPDATE [Attribute]
SET
    [DefaultValue] = REPLACE( [DefaultValue], 'fa fa-print', 'ti ti-printer' ),
    [ModifiedDateTime] = GETDATE()
WHERE [DefaultValue] LIKE '%fa fa-print%'

UPDATE [Attribute]
SET
    [DefaultValue] = REPLACE( [DefaultValue], 'fa fa-calendar', 'ti ti-calendar' ),
    [ModifiedDateTime] = GETDATE()
WHERE [DefaultValue] LIKE '%fa fa-calendar%'

UPDATE [Attribute]
SET
    [DefaultValue] = REPLACE( [DefaultValue], 'fa fa-plus', 'ti ti-plus' ),
    [ModifiedDateTime] = GETDATE()
WHERE [DefaultValue] LIKE '%fa fa-plus%'

UPDATE [Attribute]
SET
    [DefaultValue] = REPLACE( [DefaultValue], 'fa-plus', 'ti-plus' ),
    [ModifiedDateTime] = GETDATE()
WHERE [DefaultValue] LIKE '%fa-plus%'

UPDATE [Attribute]
SET
    [DefaultValue] = REPLACE( [DefaultValue], 'fa-minus', 'ti-minus' ),
    [ModifiedDateTime] = GETDATE()
WHERE [DefaultValue] LIKE '%fa-minus%'
" );
        }
    }
}
