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

using Rock.Model;

namespace Rock.Plugin.HotFixes
{
    /// <summary>
    /// Plug-in migration
    /// </summary>
    /// <seealso cref="Rock.Plugin.Migration" />
    [MigrationNumber( 273, "18.1" )]
    public class UpdateCommunicationTemplatesAndShortcodeForV18_2 : Migration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            Sql( @"
                -- Fix typo in TextBox LavaShortcode
                UPDATE [LavaShortcode]
                SET [Documentation] = REPLACE([Documentation], 'preeaddon', 'preaddon')
                WHERE [Guid] = 'BE829889-A775-4170-9A88-591BB82C86DD'

                -- Fix isrequire issue in the Checkbox list LavaShortcode
                UPDATE [LavaShortcode]
                SET [Markup] =
                    REPLACE(
                        REPLACE(
                            [Markup],
                            '{% assign sc-values =  value | Split:'','',true %}',
                            '{% assign isrequired = isrequired | AsBoolean %}' + CHAR(13) + CHAR(10) +
                            '{% assign sc-values =  value | Split:'','',true %}'
                        ),
                        '{% if isrequired %}',
                        '{% if isrequired == true %}'
                    )
                WHERE [Guid] = 'D052824F-E514-47D9-953E-2C9B55FF72D0'
                  AND [Markup] NOT LIKE '% assign isrequired = isrequired | AsBoolean %'


                -- Rename the 'Blank' template to Legacy
                UPDATE [CommunicationTemplate]
                SET [Name] = 'Blank (Legacy)'
                WHERE [Guid] = 'A3C7F623-7F6F-4C48-B66F-CBEE2DF30B6A'
                AND [Name] = 'Blank'

                -- Rename the preview template to just 'Blank'
                UPDATE [CommunicationTemplate]
                SET [Name] = 'Blank'
                WHERE [Guid] = '6280214C-404E-4F4E-BC33-7A5D4CDF8DBC'
                AND [Name] = 'Blank (Preview)'
            " );

        }

        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            Sql( @"
                -- Don't undo typo in TextBox LavaShortcode
                -- no op

                -- Undo the fix for the isrequire issue in the Checkbox list LavaShortcode
                UPDATE [LavaShortcode]
                SET [Markup] =
                    REPLACE(
                        REPLACE(
                            [Markup],
                            '{% if isrequired == true %}',
                            '{% if isrequired %}'
                        ),
                        '{% assign isrequired = isrequired | AsBoolean %}' + CHAR(13) + CHAR(10),
                        ''
                    )
                WHERE [Guid] = 'D052824F-E514-47D9-953E-2C9B55FF72D0'
                  AND  [Markup] LIKE '% assign isrequired = isrequired | AsBoolean %'

                -- Rename the 'Blank' template back to Legacy
                UPDATE [CommunicationTemplate]
                SET [Name] = 'Blank'
                WHERE [Guid] = 'A3C7F623-7F6F-4C48-B66F-CBEE2DF30B6A'
                AND [Name] = 'Blank (Legacy)'

                -- Rename the preview template back to preview
                UPDATE [CommunicationTemplate]
                SET [Name] = 'Blank (Preview)'
                WHERE [Guid] = '6280214C-404E-4F4E-BC33-7A5D4CDF8DBC'
                AND [Name] = 'Blank'
            " );
        }

    }
}