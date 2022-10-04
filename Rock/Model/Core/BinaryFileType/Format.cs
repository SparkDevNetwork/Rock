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

namespace Rock.Model
{
    /// <summary>
    /// Represents the preferred format of the binary file type.
    /// </summary>
    public enum Format
    {
        /// <summary>
        /// The undefined.
        /// </summary>
        Undefined = -1,

        /// <summary>
        /// The preferred format is as a .JPG file.
        /// </summary>
        JPG = 0,

        /// <summary>
        /// The preferred format is as a .GIF file.
        /// </summary>
        GIF = 1,

        /// <summary>
        /// The preferred format is as a .PNG file.
        /// </summary>
        PNG = 2,

        /// <summary>
        /// The preferred format is as a .PDF file.
        /// </summary>
        PDF = 3,

        /// <summary>
        /// The preferred format is as a Word document.
        /// </summary>
        Word = 4,

        /// <summary>
        /// The preferred format is as an Excel document.
        /// </summary>
        Excel = 5,

        /// <summary>
        /// The preferred format is as a text file.
        /// </summary>
        Text = 6,

        /// <summary>
        /// The preferred format is as an HTML document.
        /// </summary>
        HTML = 7
    }
}
