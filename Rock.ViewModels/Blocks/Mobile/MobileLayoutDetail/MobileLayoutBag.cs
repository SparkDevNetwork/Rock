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

namespace Rock.ViewModels.Blocks.Mobile.MobileLayoutDetail
{
    /// <summary>
    /// 
    /// </summary>
    public class MobileLayoutBag : EntityBagBase
    {

        /// <summary>
        /// Gets or sets the logical name of the Layout.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the user defined description of the Layout.
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Gets or sets the layout mobile phone.
        /// </summary>
        public string LayoutMobilePhone { get; set; }


        /// <summary>
        /// Gets or sets the layout mobile tablet.
        /// </summary>
        public string LayoutMobileTablet { get; set; }
    }
}
