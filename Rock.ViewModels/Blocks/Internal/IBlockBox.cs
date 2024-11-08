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

namespace Rock.ViewModels.Blocks.Internal
{
    /// <summary>
    /// This defines the structure of the block box. It is used by the framework
    /// and should not be used by plugins as its structure may change without
    /// warning.
    /// </summary>
    public interface IBlockBox
    {
        /// <summary>
        /// Gets or sets the security grant token.
        /// </summary>
        /// <value>The security grant token.</value>
        string SecurityGrantToken { get; set; }

        /// <summary>
        /// Gets or sets the error message that should be displayed instead
        /// of the normal block content.
        /// </summary>
        /// <value>The error message that should be displayed.</value>
        string ErrorMessage { get; set; }
    }
}
