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
using System.Linq;
using System.Text.RegularExpressions;
using Rock.Data;
using Rock.Web.Cache;

namespace Rock.Model
{
    /// <summary>
    /// Data access/service class for <see cref="Rock.Model.ContentChannelItemSlug"/> objects.
    /// </summary>
    public partial class ContentChannelItemSlugService
    {
        /// <summary>
        /// Gets a unique slug for the content channel.
        /// </summary>
        /// <param name="slug">The slug.</param>
        /// <param name="contentChannelId">The content channel identifier.</param>
        /// <param name="contentChannelItemSlugId">The content channel item slug identifier.</param>
        /// <returns></returns>
        public string GetUniqueSlugForContentChannel( string slug, int contentChannelId, int? contentChannelItemSlugId)
        {
            return GetUniqueSlug( slug, contentChannelItemSlugId, contentChannelId );
        }

        /// <summary>
        /// Gets a globally unique slug. In most cases use the override with the content channel item ID to get a unique
        /// slug for the content channel.
        /// </summary>
        /// <param name="slug">The slug.</param>
        /// <param name="contentChannelItemSlugId">The content channel item slug identifier.</param>
        /// <returns></returns>
        public string GetUniqueContentSlug( string slug, int? contentChannelItemSlugId )
        {
            return GetUniqueContentSlug( slug, contentChannelItemSlugId, null );
        }

        /// <summary>
        /// Gets the unique slug
        /// </summary>
        /// <param name="slug">The slug.</param>
        /// <param name="contentChannelItemSlugId">The content channel item slug identifier.</param>
        /// <param name="contentChannelItemId">The content channel item identifier.</param>
        /// <returns></returns>
        public string GetUniqueContentSlug( string slug, int? contentChannelItemSlugId, int? contentChannelItemId )
        {
            // Slug must be unique within a content channel but may be duplicated across content channels

            // Get ContentChannel.Id
            int? contentChannelId = null;
            if ( contentChannelItemId != null )
            {
                var contentChannelItem = ContentChannelItemCache.Get( contentChannelItemId.Value );
                contentChannelId = contentChannelItem?.ContentChannelId;
            }

            return GetUniqueSlug( slug, contentChannelItemSlugId, contentChannelId );
        }

        /// <summary>
        /// Gets a unique slug
        /// </summary>
        /// <param name="slug">The slug.</param>
        /// <param name="contentChannelItemSlugId">The content channel item slug identifier.</param>
        /// <param name="contentChannelId">The content channel identifier.</param>
        /// <returns></returns>
        private string GetUniqueSlug( string slug, int? contentChannelItemSlugId, int? contentChannelId )
        {
            bool isValid = false;

            slug = MakeSlugValid( slug );

            // If MakeSlugValid removes all the characters then just return null.
            if ( slug.IsNullOrWhiteSpace() )
            {
                return null;
            }

            int intialSlugLength = slug.Length;
            int paddedNumber = 0;
            do
            {
                string customSlug = slug;
                if ( paddedNumber > 0 )
                {
                    string paddedString = string.Format( "-{0}", paddedNumber );
                    if ( intialSlugLength + paddedString.Length > 200 )
                    {
                        customSlug = slug.Left( intialSlugLength + paddedString.Length - 200 ) + paddedString;
                    }
                    else
                    {
                        customSlug = slug + paddedString;
                    }
                }

                var qry = this
                    .Queryable()
                    .Where( b => ( ( contentChannelItemSlugId.HasValue && b.Id != contentChannelItemSlugId.Value ) || !contentChannelItemSlugId.HasValue ) )
                    .Where( b => b.Slug == customSlug );

                if ( contentChannelId != null )
                {
                    qry = qry.Where( b => b.ContentChannelItem.ContentChannelId == contentChannelId );
                }

                isValid = !qry.Any();
                if ( !isValid )
                {
                    paddedNumber += 1;
                }
                else
                {
                    slug = customSlug;
                }
            } while ( !isValid );

            return slug;
        }

        /// <summary>
        /// Saves the slug
        /// </summary>
        /// <param name="contentChannelItemId">The content channel item identifier.</param>
        /// <param name="slug">The slug.</param>
        /// <param name="contentChannelItemSlugId">The content channel item slug identifier.</param>
        /// <returns></returns>
        public ContentChannelItemSlug SaveSlug( int contentChannelItemId, string slug, int? contentChannelItemSlugId )
        {
            var uniqueSlug = this.GetUniqueContentSlug( slug, contentChannelItemSlugId, contentChannelItemId );
            return SaveSlug( contentChannelItemId, contentChannelItemSlugId, uniqueSlug );
        }

        /// <summary>
        /// Saves the slug using the given contentChannelId to guarantee the slug is unique for that channel,
        /// which also works when the item is unsaved (contentChannelItemId of 0) or not yet committed.
        /// </summary>
        /// <param name="contentChannelItemId">The content channel item identifier.</param>
        /// <param name="contentChannelId">The content channel identifier.</param>
        /// <param name="slug">The slug.</param>
        /// <param name="contentChannelItemSlugId">The content channel item slug identifier.</param>
        /// <returns></returns>
        public ContentChannelItemSlug SaveSlug( int contentChannelItemId, int contentChannelId, string slug, int? contentChannelItemSlugId )
        {
            /*
                6/10/26 - MSE

                The caller-supplied content channel id now scopes the uniqueness
                check for saved items too, not only unsaved ones. The previous
                else branch derived the channel through ContentChannelItemCache,
                which loads on its own context: inside a wrapped transaction a
                just-inserted item is not yet visible there, the lookup returned
                null, and the check silently degraded to global scope, renaming
                slugs that only collided across channels.

                Reason: The cache cannot see an uncommitted item; the caller
                already knows the channel.
            */
            var uniqueSlug = this.GetUniqueSlugForContentChannel( slug, contentChannelId, contentChannelItemSlugId );

            return SaveSlug( contentChannelItemId, contentChannelItemSlugId, uniqueSlug );
        }

        /// <summary>
        /// Saves the slug
        /// </summary>
        /// <param name="contentChannelItemId"></param>
        /// <param name="contentChannelItemSlugId"></param>
        /// <param name="uniqueSlug"></param>
        /// <returns></returns>
        private ContentChannelItemSlug SaveSlug( int contentChannelItemId, int? contentChannelItemSlugId, string uniqueSlug )
        {
            if ( uniqueSlug.IsNullOrWhiteSpace() )
            {
                return null;
            }

            var rockContext = ( RockContext ) this.Context;

            ContentChannelItemSlug contentChannelItemSlug = null;
            if ( contentChannelItemSlugId.HasValue )
            {
                contentChannelItemSlug = this.Get( contentChannelItemSlugId.Value );
            }
            else
            {
                contentChannelItemSlug = new ContentChannelItemSlug();
                contentChannelItemSlug.ContentChannelItemId = contentChannelItemId;
                this.Add( contentChannelItemSlug );
            }

            contentChannelItemSlug.Slug = uniqueSlug;

            rockContext.SaveChanges();
            return contentChannelItemSlug;
        }

        /// <summary>
        /// Replace space with dash
        /// Remove unsafe and reserved characters ; / ? : @ = &amp; &gt; &lt; # % " { } | \ ^ [ ] `
        /// Limit to 75 characters.
        /// </summary>
        /// <param name="slug">The slug.</param>
        /// <returns></returns>
        internal static string MakeSlugValid( string slug )
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
            slug = Regex.Replace( slug, @"-+", "-" );

            // Remove any none alphanumeric characters, this negates the need
            // for .RemoveInvalidReservedUrlChars()
            slug = Regex.Replace( slug, @"[^a-zA-Z0-9 -]", string.Empty );

            return slug
                    .Left( 200 )
                    .TrimEnd( '-' );
        }
    }
}
