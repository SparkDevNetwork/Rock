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
    /// Fixes the article image URL in the "Internal Article List" attribute matrix template.
    ///
    /// The shipped template built the article image with a relative "./GetImage.ashx" path, which
    /// resolves against the current page path and 404s on any route deeper than the site root
    /// (e.g. /page/12/). The original line was also malformed (the alt attribute sat inside the
    /// url()/style and the quotes were left unclosed).
    ///
    /// Two replacements are applied. The first rewrites the entire original broken line to a
    /// well-formed, application-root-aware version ( {{ '~' | ResolveRockUrl }}GetImage.ashx with
    /// the alt moved out and the quotes closed ). The second is a fallback that rewrites just the
    /// relative-URL token, so the functional fix still applies when an administrator has already
    /// corrected the surrounding markup but left the relative path. Any template whose image URL no
    /// longer matches the broken form is left untouched.
    /// </summary>
    /// <seealso cref="Rock.Plugin.Migration" />
    [MigrationNumber( 322, "21.0" )]
    public class FixInternalArticleListTemplateArticleImageUrl : Migration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            NA_FixInternalArticleListImageUrl_Up();
        }

        /// <summary>
        /// Rewrites the "Internal Article List" attribute matrix template's article image URL to a
        /// well-formed, application-root-aware form, only when it still matches the broken version.
        /// </summary>
        private void NA_FixInternalArticleListImageUrl_Up()
        {
            Sql( @"
UPDATE [AttributeMatrixTemplate]
SET [FormattedLava] = REPLACE(
    REPLACE(
        [FormattedLava],
        '<div class=""photo"" style=""background-image: url(''./GetImage.ashx?Guid={{ attributeMatrixItem | Attribute:''ArticleImage'',''RawValue'' }}"" alt=""{{ attributeMatrixItem | Attribute:''ArticleTitle'' }}'');""></div>',
        '<div class=""photo"" style=""background-image: url(''{{ ''~'' | ResolveRockUrl }}GetImage.ashx?Guid={{ attributeMatrixItem | Attribute:''ArticleImage'',''RawValue'' }}'');"" alt=""{{ attributeMatrixItem | Attribute:''ArticleTitle'' }}""></div>' ),
    'url(''./GetImage.ashx?Guid={{ attributeMatrixItem | Attribute:''ArticleImage'',''RawValue'' }}',
    'url(''{{ ''~'' | ResolveRockUrl }}GetImage.ashx?Guid={{ attributeMatrixItem | Attribute:''ArticleImage'',''RawValue'' }}' )
WHERE [Guid] = '1D24694E-445C-4852-B5BC-64CDEA6F7175'
    AND [FormattedLava] LIKE '%url(''./GetImage.ashx?Guid={{ attributeMatrixItem | Attribute:''ArticleImage'',''RawValue'' }}%';
" );
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
