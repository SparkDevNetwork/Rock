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
    [MigrationNumber( 287, "19.0" )]
    public class FixItemsUsingFontAwesomeIssue6766ForV19_1 : Migration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            JE_SetDefaultImageForTemplateDefinedTypeIconAttribute_Up();
        }

        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            // Down migrations are not yet supported in plug-in migrations.
        }

        /// <summary>
        /// Fix for issue https://github.com/SparkDevNetwork/Rock/issues/6766
        /// </summary>
        private void JE_SetDefaultImageForTemplateDefinedTypeIconAttribute_Up()
        {
            Sql( @"
UPDATE [__IconTransition]
SET [TablerClass] = 'ti-map-search', [TablerFull] = 'ti ti-map-search'
WHERE [FontAwesomeFull] = 'fa fa-search-location'

INSERT INTO [__IconTransition]
([FontAwesomeClass], [FontAwesomeFull], [TablerClass], [TablerFull])
VALUES
('fa-openid', 'fa fa-openid', 'ti-cloud-lock', 'ti ti-cloud-lock')

INSERT INTO [__IconTransition]
([FontAwesomeClass], [FontAwesomeFull], [TablerClass], [TablerFull])
VALUES
('fa-video-camera', 'fa fa-video-camera', 'ti-video', 'ti ti-video')

INSERT INTO [__IconTransition]
([FontAwesomeClass], [FontAwesomeFull], [TablerClass], [TablerFull])
VALUES
('fa-file-search', 'fa fa-file-search', 'ti-file-search', 'ti ti-file-search')

INSERT INTO [__IconTransition]
([FontAwesomeClass], [FontAwesomeFull], [TablerClass], [TablerFull])
VALUES
('fa-warning', 'fa fa-warning', 'ti-alert-triangle', 'ti ti-alert-triangle')

INSERT INTO [__IconTransition]
([FontAwesomeClass], [FontAwesomeFull], [TablerClass], [TablerFull])
VALUES
('fa-diamond', 'fa fa-diamond', 'ti-diamond', 'ti ti-diamond')

UPDATE [Page]
SET [IconCssClass] = 'ti ti-file-search'
WHERE [IconCssClass] = 'fa-file-search'

UPDATE [Page]
SET [IconCssClass] = 'ti ti-map-search'
WHERE [IconCssClass] = 'fa fa-search-location'

UPDATE [Page]
SET [IconCssClass] = 'ti ti-cloud-lock'
WHERE [IconCssClass] = 'fa fa-openid'

UPDATE [Page]
SET [IconCssClass] = 'ti ti-video'
WHERE [IconCssClass] = 'fa fa-video-camera'

UPDATE [Page]
SET [IconCssClass] = 'ti ti-video'
WHERE [IconCssClass] = 'fa fa-video-camera'

UPDATE [NoteType]
SET [IconCssClass] = 'ti ti-settings'
WHERE [IconCssClass] = 'fa fa-gogs'

UPDATE [NoteType]
SET [IconCssClass] = 'ti ti-alert-triangle'
WHERE [IconCssClass] = 'fa fa-warning'

UPDATE [Page]
SET [IconCssClass] = 'ti ti-api'
WHERE [Guid] IN ('C132F1D5-9F43-4AEB-9172-CD45138B4CEA', '32551448-8602-4200-9F69-BD4C04770F9F') 

UPDATE [ContentChannel]
SET [IconCssClass] = 'ti ti-video'
WHERE [IconCssClass] = 'fa fa-video-camera'
" );
        }
    }
}
