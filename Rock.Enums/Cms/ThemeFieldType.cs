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

namespace Rock.Enums.Cms
{
    /// <summary>
    /// The type of field to be displayed in the UI.
    /// </summary>
    public enum ThemeFieldType
    {
        /// <summary>
        /// A CSS variable with a text input that will output the literal value
        /// without any formatting.
        /// </summary>
        Literal = 0,

        /// <summary>
        /// A CSS variable with a color input.
        /// </summary>
        Color = 1,

        /// <summary>
        /// A CSS variable with an image input.
        /// </summary>
        Image = 2,

        /// <summary>
        /// A CSS variable with a text input. The value will be enclosed in
        /// quotes and escaped.
        /// </summary>
        Text = 3,

        /// <summary>
        /// A CSS variable with a file input.
        /// </summary>
        File = 4,

        /// <summary>
        /// A field that will include one of two custom CSS content templates.
        /// The CSS variable will contain the text 'on' or 'off'.
        /// </summary>
        Switch = 5,

        /// <summary>
        /// A heading above other fields.
        /// </summary>
        Heading = 100,

        /// <summary>
        /// A spacer between fields.
        /// </summary>
        Spacer = 101,

        /// <summary>
        /// A panel that wraps other fields.
        /// </summary>
        Panel = 102
    }
}
