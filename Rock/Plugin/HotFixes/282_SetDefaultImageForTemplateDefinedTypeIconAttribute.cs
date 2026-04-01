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

using Mono.CSharp.Linq;

using static Rock.WebFarm.RockWebFarm;

namespace Rock.Plugin.HotFixes
{
    /// <summary>
    /// Plug-in migration
    /// </summary>
    /// <seealso cref="Rock.Plugin.Migration" />
    [MigrationNumber( 282, "18.2" )]
    public class SetDefaultImageForTemplateDefinedTypeIconAttribute : Migration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            NA_SetDefaultImageForTemplateDefinedTypeIconAttribute_Up();
        }

        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            // Down migrations are not yet supported in plug-in migrations.
        }

        /// <summary>
        /// We need to add a default image to this attribute "Icon" attribute which is
        /// defined on DefinedType called "Template".  The Icon attribute was originally
        /// via 202001081542541_Rollup_01081.cs but no default image was set at that time.
        /// See https://app.asana.com/1/20866866924293/project/1208321217019996/task/1211303414636849?focus=true
        /// </summary>
        private void NA_SetDefaultImageForTemplateDefinedTypeIconAttribute_Up()
        {
            string newBinaryFileGuid = "3FC8FF25-13FC-4CED-81F1-E7C73205C18A";
            string standardIconSvg = "PD94bWwgdmVyc2lvbj0iMS4wIiBlbmNvZGluZz0iVVRGLTgiIHN0YW5kYWxvbmU9Im5vIj8+CjwhRE9DVFlQRSBzdmcgUFVCTElDICItLy9XM0MvL0RURCBTVkcgMS4xLy9FTiIgImh0dHA6Ly93d3cudzMub3JnL0dyYXBoaWNzL1NWRy8xLjEvRFREL3N2ZzExLmR0ZCI+Cjxzdmcgd2lkdGg9IjEwMCUiIGhlaWdodD0iMTAwJSIgdmlld0JveD0iMCAwIDY0MCAyNDAiIHZlcnNpb249IjEuMSIgeG1sbnM9Imh0dHA6Ly93d3cudzMub3JnLzIwMDAvc3ZnIiB4bWxuczp4bGluaz0iaHR0cDovL3d3dy53My5vcmcvMTk5OS94bGluayIgeG1sOnNwYWNlPSJwcmVzZXJ2ZSIgeG1sbnM6c2VyaWY9Imh0dHA6Ly93d3cuc2VyaWYuY29tLyIgc3R5bGU9ImZpbGwtcnVsZTpldmVub2RkO2NsaXAtcnVsZTpldmVub2RkO3N0cm9rZS1saW5lam9pbjpyb3VuZDtzdHJva2UtbWl0ZXJsaW1pdDoyOyI+CiAgICA8ZyB0cmFuc2Zvcm09Im1hdHJpeCgxLjEwMTU1LDAsMCwxLC0zMC44NDM0LC0zMSkiPgogICAgICAgIDxyZWN0IHg9IjI4IiB5PSIzMSIgd2lkdGg9IjU4MSIgaGVpZ2h0PSIxOCIgc3R5bGU9ImZpbGw6cmdiKDIzMSwyMzEsMjMxKTsiLz4KICAgIDwvZz4KICAgIDxnIHRyYW5zZm9ybT0ibWF0cml4KDAuOTY1NTc3LDAsMCwxLC0yNy4wMzYxLDEyKSI+CiAgICAgICAgPHJlY3QgeD0iMjgiIHk9IjMxIiB3aWR0aD0iNTgxIiBoZWlnaHQ9IjE4IiBzdHlsZT0iZmlsbDpyZ2IoMjMxLDIzMSwyMzEpOyIvPgogICAgPC9nPgogICAgPGcgdHJhbnNmb3JtPSJtYXRyaXgoMS4wMjA2NSwwLDAsMSwtMjguNTc4Myw1NSkiPgogICAgICAgIDxyZWN0IHg9IjI4IiB5PSIzMSIgd2lkdGg9IjU4MSIgaGVpZ2h0PSIxOCIgc3R5bGU9ImZpbGw6cmdiKDIzMSwyMzEsMjMxKTsiLz4KICAgIDwvZz4KICAgIDxnIHRyYW5zZm9ybT0ibWF0cml4KDAuOTg0NTA5LDAsMCwxLC0yNy41NjYzLDk4KSI+CiAgICAgICAgPHJlY3QgeD0iMjgiIHk9IjMxIiB3aWR0aD0iNTgxIiBoZWlnaHQ9IjE4IiBzdHlsZT0iZmlsbDpyZ2IoMjMxLDIzMSwyMzEpOyIvPgogICAgPC9nPgogICAgPGcgdHJhbnNmb3JtPSJtYXRyaXgoMS4wNTY4LDAsMCwxLC0yOS41OTA0LDE0MSkiPgogICAgICAgIDxyZWN0IHg9IjI4IiB5PSIzMSIgd2lkdGg9IjU4MSIgaGVpZ2h0PSIxOCIgc3R5bGU9ImZpbGw6cmdiKDIzMSwyMzEsMjMxKTsiLz4KICAgIDwvZz4KICAgIDxnIHRyYW5zZm9ybT0ibWF0cml4KDEuMDc5MTcsMCwwLDEsLTMwLjIxNjksMTg0KSI+CiAgICAgICAgPHJlY3QgeD0iMjgiIHk9IjMxIiB3aWR0aD0iNTgxIiBoZWlnaHQ9IjE4IiBzdHlsZT0iZmlsbDpyZ2IoMjMxLDIzMSwyMzEpOyIvPgogICAgPC9nPgo8L3N2Zz4K";
            RockMigrationHelper.AddOrUpdateBinaryFileForDatabaseStorage( Rock.SystemGuid.BinaryFiletype.DEFAULT, standardIconSvg, "standard-template.svg", "image/svg+xml", "", newBinaryFileGuid );

            // Also, add this other possible missing BinaryFile for Guid 8B53F981-6FF6-4657-9CD5-01E36EB0DF51
            RockMigrationHelper.AddOrUpdateBinaryFileForDatabaseStorage( Rock.SystemGuid.BinaryFiletype.DEFAULT, standardIconSvg, "standard-template.svg", "image/svg+xml", "", "8B53F981-6FF6-4657-9CD5-01E36EB0DF51" );

            // Now we need to set the default value of the Icon attribute for the Template DefinedType to be the image we just added.
            Sql( $@"
DECLARE @IconAttributeId int = (SELECT TOP 1 [Id] FROM [Attribute] WHERE [Guid] = '831403eb-262e-4bc5-8b5e-f16153493bf5')
IF @IconAttributeId IS NOT NULL
BEGIN
    UPDATE [Attribute] SET [DefaultValue] = '{newBinaryFileGuid}', IsDefaultPersistedValueDirty = 1 WHERE Id = @IconAttributeId
END
            " );

            // We also need to do some other minor data maintenance on certain defined values.
            Sql( @"
UPDATE [DefinedValue]
    SET [Value] = 'Legacy 2'
    , [Order] = 3
    WHERE [Guid] = '74760472-3516-480d-b96b-1f77aaef0862'
    
UPDATE [DefinedValue]
    SET [Order] = 2
    WHERE [Guid] = '74760472-3516-480d-b96b-1f77aaef0862'
    
UPDATE [DefinedValue]
    SET [Order] = 1
    WHERE [Guid] = '5114db6e-40cc-4455-99b8-c109b7bb52d1'
" );
        }
    }
}
