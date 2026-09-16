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

namespace Rock.ViewModels.Blocks.Mobile.MobilePageDetail
{
    /// <summary>
    /// A single context parameter for a page, mapping a context entity type to the
    /// page/route parameter name that carries its identifier.
    /// </summary>
    public class MobilePageContextParameterBag
    {
        /// <summary>
        /// Gets or sets the fully qualified name of the context entity type.
        /// </summary>
        public string EntityTypeName { get; set; }

        /// <summary>
        /// Gets or sets the friendly name of the context entity type, shown as the field label.
        /// </summary>
        public string EntityTypeFriendlyName { get; set; }

        /// <summary>
        /// Gets or sets the page/route parameter name that contains the identifier of
        /// the context entity.
        /// </summary>
        public string ParameterName { get; set; }
    }
}
