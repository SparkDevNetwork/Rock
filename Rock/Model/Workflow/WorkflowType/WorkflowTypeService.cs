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
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

using Rock.Data;
using Rock.Web.Cache;

namespace Rock.Model
{
    /// <summary>
    /// Service/Data access class for <see cref="Rock.Model.WorkflowType"/> entity objects
    /// </summary>
    public partial class WorkflowTypeService
    {
        private static int WorkflowTypeSlugMaxLength = 400;

        #region Public Methods

        /// <summary>
        /// 
        /// </summary>
        /// <param name="slug"></param>
        /// <returns></returns>
        public string GetUniqueSlug( string slug )
        {
            return GetUniqueSlugInternal( slug, ( candidateSlug ) =>
                !this.Queryable().Any( b => b.Slug == candidateSlug )
            );
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="slug"></param>
        /// <param name="workflowTypeId"></param>
        /// <returns></returns>
        public string GetUniqueSlug( string slug, int workflowTypeId )
        {
            return GetUniqueSlugInternal( slug, ( candidateSlug ) =>
                !this.Queryable().Any( b => b.Slug == candidateSlug && b.Id != workflowTypeId )
            );
        }

        #endregion Public Methods

        #region Private Methods

        private string GetUniqueSlugInternal( string slug, Func<string, bool> isUnique )
        {
            slug = MakeSlugValid( slug );

            if ( slug.IsNullOrWhiteSpace() )
            {
                return null;
            }

            var initialSlugLength = slug.Length;
            var paddedNumber = 0;
            string candidateSlug;

            do
            {
                var suffix = paddedNumber > 0 ? $"-{paddedNumber}" : string.Empty;

                if ( initialSlugLength + suffix.Length > WorkflowTypeSlugMaxLength )
                {
                    candidateSlug = slug.Left( WorkflowTypeSlugMaxLength - suffix.Length ) + suffix;
                }
                else
                {
                    candidateSlug = slug + suffix;
                }

                paddedNumber++;
            }
            while ( !isUnique( candidateSlug ) );

            return candidateSlug;
        }

#pragma warning disable CS1570 // XML comment has badly formed XML
        /// <summary>
        /// Normalizes and sanitizes a slug string to ensure it conforms to URL-safe and Rock RMS-friendly standards.
        /// </summary>
        /// <param name="slug">The raw slug string to be cleaned and standardized.</param>
        /// <returns>
        /// A valid slug string containing only lowercase alphanumeric characters and hyphens,
        /// with all special characters and whitespace removed or replaced.
        /// The resulting string is also truncated to the maximum allowed length defined by <c>WorkflowTypeSlugMaxLength</c>.
        /// </returns>
        /// <remarks>
        /// This method performs several transformations:
        /// <list type="bullet">
        ///   <item><description>Trims and converts the input to lowercase.</description></item>
        ///   <item><description>Replaces HTML character entities (e.g., &nbsp;, &mdash;) with hyphens.</description></item>
        ///   <item><description>Converts spaces and underscores to hyphens.</description></item>
        ///   <item><description>Collapses multiple consecutive hyphens into a single one.</description></item>
        ///   <item><description>Removes all non-alphanumeric characters except hyphens.</description></item>
        ///   <item><description>Ensures the final slug is within length constraints and does not end with a hyphen.</description></item>
        /// </list>
        /// </remarks>
#pragma warning restore CS1570 // XML comment has badly formed XML
        private static string MakeSlugValid( string slug )
        {
            slug = slug
                .Trim()
                .ToLower()
                .Replace( "&nbsp;", "-" )
                .Replace( "&#160;", "-" )
                .Replace( "&ndash;", "-" )
                .Replace( "&#8211;", "-" )
                .Replace( "&mdash;", "-" )
                .Replace( "&#8212;", "-" )
                .Replace( "_", "-" )
                .Replace( " ", "-" );

            // Remove multiple -- in a row from the slug.
            slug = Regex.Replace( slug, "-+", "-" );

            // Remove any none alphanumeric characters, this negates the need
            // for .RemoveInvalidReservedUrlChars()
            slug = Regex.Replace( slug, @"[^a-zA-Z0-9-]", string.Empty );

            // Ensure the slug does not exceed the max length and doesn't end in a dash.
            return slug.Left( WorkflowTypeSlugMaxLength ).TrimEnd( '-' );
        }

        #endregion Private Methods
    }
}