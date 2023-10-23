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

namespace Rock.Model
{
    public partial class ContentCollectionService
    {
        /// <summary>
        /// Gets a globally unique slug for a content collection.
        /// </summary>
        /// <param name="slug">The slug requested by the person.</param>
        /// <param name="existingCollectionId">The identifier of the existing collection to ignore when ensuring uniqueness.</param>
        /// <returns>A slug that is unique in the system or <c>null</c> if it wasn't valid.</returns>
        public string GetUniqueSlug( string slug, int? existingCollectionId = null )
        {
            bool isValid = false;

            slug = ContentChannelItemSlugService.MakeSlugValid( slug );

            // If MakeSlugValid removes all the characters then just return null.
            if ( slug.IsNullOrWhiteSpace() )
            {
                return null;
            }

            // Get the existing slugs in lower case to save on later
            // comparison costs.
            var existingSlugs = Queryable()
                .Where( cl => !existingCollectionId.HasValue || cl.Id != existingCollectionId.Value )
                .Select( cl => cl.CollectionKey )
                .ToList()
                .Select( s => s.ToLower() )
                .ToList();

            int intialSlugLength = slug.Length;
            for ( int paddedNumber = 0; !isValid; paddedNumber++ )
            {
                var customSlug = slug;

                // If this isn't the first attempt, then append the attempt
                // number to the slug while ensuring we stay under 200 characters.
                if ( paddedNumber > 0 )
                {
                    var paddedString = $"-{paddedNumber}";

                    if ( intialSlugLength + paddedString.Length > 200 )
                    {
                        customSlug = slug.Left( intialSlugLength + paddedString.Length - 200 ) + paddedString;
                    }
                    else
                    {
                        customSlug = slug + paddedString;
                    }
                }

                if ( !existingSlugs.Contains( customSlug.ToLower() ) )
                {
                    return customSlug;
                }
            }

            return null;
        }
    }
}
