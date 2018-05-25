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
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Spatial;
using System.Linq;
using Rock.Data;
using Rock.Cache;

namespace Rock.Model
{
    /// <summary>
    /// Data access/service class for <see cref="Rock.Model.ContentChannelItemSlug"/> objects.
    /// </summary>
    public partial class ContentChannelItemSlugService
    {
        /// <summary>
        /// Gets the unique slug
        /// </summary>
        /// <param name="slug">The slug.</param>
        /// <param name="contentChannelItemSlugId">The content channel item slug identifier.</param>
        /// <returns></returns>
        public string GetUniqueContentSlug( string slug, int? contentChannelItemSlugId )
        {
            bool isValid = false;

            slug = MakeSlugValid( slug );

            int intialSlugLength = slug.Length;
            int paddedNumber = 0;
            do
            {
                string customSlug = slug;
                if ( paddedNumber > 0 )
                {
                    string paddedString = string.Format( "-{0}", paddedNumber );
                    if ( intialSlugLength + paddedString.Length > 75 )
                    {
                        customSlug = slug.Left( intialSlugLength + paddedString.Length - 75 ) + paddedString;
                    }
                    else
                    {
                        customSlug = slug + paddedString;
                    }
                }


                isValid = !this.Queryable().Where( b => (( contentChannelItemSlugId.HasValue && b.Id != contentChannelItemSlugId.Value ) || !contentChannelItemSlugId.HasValue) && ( b.Slug == customSlug ) ).Any();
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
            var uniqueSlug = this.GetUniqueContentSlug( slug, contentChannelItemSlugId );

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
        /// Remove unsafe and reserved characters ; / ? : @ = & < > # % " { } | \ ^ [ ] `
        /// Limit to 75 characters.
        /// </summary>
        /// <param name="slug">The slug.</param>
        /// <returns></returns>
        private string MakeSlugValid( string slug )
        {
            return slug
                .ToLower()
                .Replace( " ", "-" )
                .Replace( ";", "" )
                .Replace( "/", "" )
                .Replace( "?", "" )
                .Replace( ":", "" )
                .Replace( "@", "" )
                .Replace( "=", "" )
                .Replace( "&", "" )
                .Replace( "<", "" )
                .Replace( ">", "" )
                .Replace( "#", "" )
                .Replace( "%", "" )
                .Replace( "\"", "" )
                .Replace( "{", "" )
                .Replace( "}", "" )
                .Replace( "|", "" )
                .Replace( "\\", "" )
                .Replace( "^", "" )
                .Replace( "[", "" )
                .Replace( "]", "" )
                .Replace( "`", "" )
                .Left( 75 );
        }
    }
}
