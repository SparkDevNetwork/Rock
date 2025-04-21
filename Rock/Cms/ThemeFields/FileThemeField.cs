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

using Newtonsoft.Json.Linq;

using Rock.Enums.Cms;

namespace Rock.Cms.ThemeFields
{
    /// <summary>
    /// A theme field which represent a CSS variable that takes a file
    /// upload. Output is a <c>url('...')</c> value.
    /// </summary>
    internal class FileThemeField : ImageThemeField
    {
        /// <inheritdoc/>
        protected override string FileLoaderHandler => "~/GetFile.ashx";

        /// <summary>
        /// Creates a new instance of <see cref="FileThemeField"/>.
        /// </summary>
        /// <param name="jField">The JSON object to be parsed.</param>
        /// <param name="type">The type of field of this instance.</param>
        public FileThemeField( JObject jField, ThemeFieldType type )
            : base( jField, type )
        {
        }
    }
}
