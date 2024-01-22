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
using Rock.Model;

namespace Rock.Drawing.Avatar
{
    /// <summary>
    /// Settings required to generate a avatar
    /// </summary>
    public class AvatarSettings
    {
        /// <summary>
        /// Gets or sets the size (width / height) of the avatar image.
        /// </summary>
        /// <value>The size.</value>
        public int Size { get; set; } = 128;

        /// <summary>
        /// Gets or sets the age classification
        /// </summary>
        public AgeClassification AgeClassification { get; set; }

        /// <summary>
        /// Gets or sets the gender for the avatar icon.
        /// </summary>
        /// <value>The gender.</value>
        public Gender Gender { get; set; } = Gender.Unknown;

        /// <summary>
        /// Gets or sets the text for the initials.
        /// </summary>
        /// <value>The text.</value>
        public string Text
        {
            get
            {
                if ( AvatarStyle == AvatarStyle.Icon )
                {
                    return string.Empty;
                }

                return _text;
            }
            set
            {
                _text = value.Truncate( 2, false );
            }
        }
        private string _text = string.Empty;

        /// <summary>
        /// Gets or sets the photo identifier.
        /// </summary>
        /// <value>The photo identifier.</value>
        public int? PhotoId { get; set; }

        /// <summary>
        /// Gets or sets the record type unique identifier.
        /// </summary>
        /// <value>The record type unique identifier.</value>
        public int? RecordTypeId { get; set; }

        /// <summary>
        /// Gets or sets the person unique identifier.
        /// </summary>
        /// <value>The person unique identifier.</value>
        public Guid? PersonGuid { get; set; }

        /// <summary>
        /// Gets or sets the person identifier.
        /// </summary>
        /// <value>The person identifier.</value>
        public int? PersonId { get; set; }

        /// <summary>
        /// Gets or sets the avatar colors (foreground / background).
        /// </summary>
        /// <value>The avatar colors.</value>
        public AvatarColors AvatarColors { get; set; } = new AvatarColors();

        /// <summary>
        /// Gets or sets the avatar style.
        /// </summary>
        /// <value>The avatar style.</value>
        public AvatarStyle AvatarStyle { get; set; } = AvatarStyle.Initials;

        /// <summary>
        /// Gets or sets a value indicating whether this instance is bold.
        /// </summary>
        /// <value><c>true</c> if this instance is bold; otherwise, <c>false</c>.</value>
        public bool IsBold { get; set; } = false;

        /// <summary>
        /// Gets or sets the corner radius.
        /// </summary>
        /// <value>The corner radius.</value>
        public int CornerRadius { get; set; } = 0;

        /// <summary>
        /// Gets or sets a value indicating whether this instance is circle.
        /// </summary>
        /// <value><c>true</c> if this instance is circle; otherwise, <c>false</c>.</value>
        public bool IsCircle { get; set; } = false;

        /// <summary>
        /// Gets or sets a value indicating whether [prefers light].
        /// </summary>
        /// <value><c>true</c> if [prefers light]; otherwise, <c>false</c>.</value>
        public bool PrefersLight { get; set; } = true;

        /// <summary>
        /// The physical path to store the cached avatars
        /// </summary>
        public string CachePath { get; set; }

        /// <summary>
        /// Creates a cache key based on the settings
        /// </summary>
        public string CacheKey
        {
            // Written as is to ensure the key is calculated only once.
            // Note we can't include the person id or guid in the cache key has their profile picture could change and the cache key is
            // used to cache the image both on the server (which could be clustered) and the client.
            get
            {
                if ( _cacheKey.IsNullOrWhiteSpace() )
                {
                    // If the configuration has a photo use a cache key without a style (initials/icon) as the photo will be shown in both cases. This
                    // prevents redundant files
                    if ( PhotoId.HasValue )
                    {
                        _cacheKey = $"{Size}-{Text}-{AvatarColors.ForegroundColor.Replace( "#", "" )}_{AvatarColors.BackgroundColor.Replace( "#", "" )}-{CornerRadius}{IsCircle}-no_style-{AgeClassification}-{Gender}-{IsBold.ToString().Truncate( 1, false )}-{PhotoId}-{PrefersLight.ToString().Truncate( 1, false )}-{RecordTypeId}";
                    }
                    else
                    {
                        _cacheKey = $"{Size}-{Text}-{AvatarColors.ForegroundColor.Replace( "#", "" )}_{AvatarColors.BackgroundColor.Replace( "#", "" )}-{CornerRadius}{IsCircle}-{AvatarStyle}-{AgeClassification}-{Gender}-{IsBold.ToString().Truncate( 1, false )}-{PhotoId}-{PrefersLight.ToString().Truncate( 1, false )}-{RecordTypeId}";
                    }
                }

                return _cacheKey;
            }
        }
        private string _cacheKey = string.Empty;
    }
}
