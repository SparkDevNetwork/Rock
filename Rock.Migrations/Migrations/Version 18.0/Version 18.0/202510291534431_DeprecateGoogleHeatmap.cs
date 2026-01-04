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
    public partial class DeprecateGoogleHeatmap : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            Sql( @"
UPDATE
    [LavaShortcode]
SET 
    [Name] = 'Google Heatmap (Deprecated)',
    [Documentation] = '<div class=""alert alert-warning"">This shortcode is deprecated due to the upcoming removal of the Heatmap Layer in the Google Maps JavaScript API. This shortcode will be removed in a future release. Please begin transitioning to alternative solutions.</div>' + [Documentation]
WHERE [Guid] = '9969a52b-01f8-4597-8c5a-1842bfa1e482'"
);

            Sql( @"
UPDATE
   [BlockType]
SET
   [Name] = 'Dynamic Heat Map (Legacy) (Deprecated)',
   [Description] = 'NOTE: This block is deprecated due to the upcoming removal of the Heatmap Layer in the Google Maps JavaScript API.'
WHERE [Guid] = 'FAFBB883-D0B4-498E-91EE-CAC5652E5095'"
);
        }

        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            Sql( @"
UPDATE
    [LavaShortcode]
SET 
    [Name] = 'Google Heatmap',
    [Documentation] = REPLACE( [Documentation], '<div class=""alert alert-warning"">This shortcode is deprecated due to the upcoming removal of the Heatmap Layer in the Google Maps JavaScript API. This shortcode will be removed in a future release. Please begin transitioning to alternative solutions.</div>', '')
WHERE [Guid] = '9969a52b-01f8-4597-8c5a-1842bfa1e482'"
);

            Sql( @"
UPDATE
   [BlockType]
SET
   [Name] = 'Dynamic Heat Map',
   [Description] = 'Block to a map of the locations of people'
WHERE [Guid] = 'FAFBB883-D0B4-498E-91EE-CAC5652E5095'"
);
        }
    }
}
