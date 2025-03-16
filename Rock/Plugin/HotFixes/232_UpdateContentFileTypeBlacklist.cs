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
    [MigrationNumber( 232, "1.15.5" )]
    public class UpdateContentFileTypeBlacklist : Migration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            Sql( $@"
                IF EXISTS (
                    SELECT [Id]
                    FROM dbo.[Attribute]
                    WHERE [Guid] = '9ffb15c1-aa53-4fba-a480-64c9b348c5e5' )
                BEGIN
                    UPDATE [Attribute] SET
                        [DefaultValue] = 'ascx, ashx, aspx, ascx.cs, ashx.cs, aspx.cs, cs, aspx.cs, php, exe, dll, config, asmx'
                    WHERE [Guid] = '9ffb15c1-aa53-4fba-a480-64c9b348c5e5'
                END
                ELSE
                BEGIN
                    INSERT [dbo].[Attribute] (
                        [IsSystem]
                        , [FieldTypeId]
                        , [EntityTypeId]
                        , [EntityTypeQualifierColumn]
                        , [EntityTypeQualifierValue]
                        , [Key]
                        , [Name]
                        , [Description]
                        , [Order]
                        , [IsGridColumn]
                        , [DefaultValue]
                        , [IsMultiValue]
                        , [IsRequired]
                        , [Guid]
                        , [CreatedDateTime]
                        , [ModifiedDateTime]
                        , [CreatedByPersonAliasId]
                        , [ModifiedByPersonAliasId]
                        , [ForeignKey]
                        , [IconCssClass]
                        , [AllowSearch]
                        , [ForeignGuid]
                        , [ForeignId]
                        , [IsIndexEnabled]
                        , [IsAnalytic]
                        , [IsAnalyticHistory]
                        , [IsActive]
                        , [EnableHistory]
                        , [PreHtml]
                        , [PostHtml]
                        , [AbbreviatedName]
                        , [ShowOnBulk]
                        , [IsPublic])
                    VALUES (
                        1
                        , 1
                        , NULL
                        , ''
                        , ''
                        , 'ContentFiletypeBlacklist'
                        , 'Content Filetype Blacklist'
                        , 'List of file types are not allowed to be uploaded in the HTML Editor.'
                        , 0
                        , 0
                        , 'ascx, ashx, aspx, ascx.cs, ashx.cs, aspx.cs, cs, aspx.cs, php, exe, dll, config, asmx'
                        , 0
                        , 0
                        , '9ffb15c1-aa53-4fba-a480-64c9b348c5e5'
                        , NULL
                        , NULL
                        , NULL
                        , NULL
                        , NULL
                        , NULL
                        , 0
                        , NULL
                        , NULL
                        , 0
                        , 0
                        , 0
                        , 1
                        , 0
                        , NULL
                        , NULL
                        , NULL
                        , 0
                        , 0)
                END" );
        }

        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            // Down migrations are not yet supported in plug-in migrations.
        }
    }
}
