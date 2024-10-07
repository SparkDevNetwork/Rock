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

using Newtonsoft.Json.Linq;

using Rock.Configuration;
using Rock.Enums.Cms;

namespace Rock.Cms.ThemeFields
{
    /// <summary>
    /// A theme field which represent a CSS variable that takes a file
    /// upload. Output is a <c>url('...')</c> value.
    /// </summary>
    internal class ImageThemeField : VariableThemeField
    {
        /// <summary>
        /// The handler that will take a guid and load the file on the browser.
        /// </summary>
        protected virtual string FileLoaderHandler => "~/GetImage.ashx";

        /// <summary>
        /// Creates a new instance of <see cref="ImageThemeField"/>.
        /// </summary>
        /// <param name="jField">The JSON object to be parsed.</param>
        /// <param name="type">The type of field of this instance.</param>
        public ImageThemeField( JObject jField, ThemeFieldType type )
            : base( jField, type )
        {
        }

        /// <inheritdoc/>
        public override void AddCssOverrides( IThemeOverrideBuilder builder )
        {
            var rawValue = GetValueOrDefault( builder );

            // Don't emit the variable at all.
            if ( rawValue.IsNullOrWhiteSpace() )
            {
                return;
            }

            // An image could either be an absolute path (possibly
            // with ~ or ~~ prefix), or a guid that represents a binary
            // file.
            if ( Guid.TryParse( rawValue, out var fileGuid ) )
            {
                var url = RockApp.Current.ResolveRockUrl( $"{FileLoaderHandler}?guid={fileGuid}" );

                builder.AddVariable( Variable, $"url('{url}')" );
            }
            else if ( rawValue.StartsWith( "~" ) )
            {
                var url = RockApp.Current.ResolveRockUrl( rawValue, builder.ThemeName );

                builder.AddVariable( Variable, $"url('{url}')" );
            }
            else
            {
                builder.AddVariable( Variable, $"url('{rawValue}')" );
            }
        }
    }
}
