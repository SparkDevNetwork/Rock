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

using Rock.ViewModels.Utility;

namespace Rock.ViewModels.Blocks.Security.RestKeyDetail
{
    /// <summary>
    /// 
    /// </summary>
    /// <seealso cref="Rock.ViewModels.Utility.EntityBagBase" />
    public class RestKeyBag : EntityBagBase
    {
        /// <summary>
        /// Gets or sets the API key associated with the UserLogin
        /// </summary>
        public string ApiKey { get; set; }

        /// <summary>
        /// Gets or sets the Rock.Model.EntityType for the authentication service that this Rest Key.
        /// </summary>
        public ListItemBag EntityType { get; set; }

        /// <summary>
        /// Gets or sets the Name that this UserLogin is associated with.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the Description that is associated with this Rest Key. This property is required.
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Gets or sets a flag indicating if this Rest Key is active. This property is required.
        /// </summary>
        public bool IsActive { get; set; }
    }
}
