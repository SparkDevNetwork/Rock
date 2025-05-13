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
using Rock.Security;
using Rock.Web.Cache;

namespace Rock.Model
{
    public partial class RegistrationTemplatePlacement
    {
        #region ISecured

        /// <inheritdoc/>
        public override ISecured ParentAuthority => RegistrationTemplate ?? base.ParentAuthority;

        #endregion

        #region Methods

        /// <summary>
        /// Gets the Icon CSS class from either <see cref="IconCssClass"/> or from <see cref="GroupType.IconCssClass" />
        /// </summary>
        /// <returns></returns>
        public string GetIconCssClass()
        {
            if ( this.IconCssClass.IsNotNullOrWhiteSpace() )
            {
                return this.IconCssClass;
            }
            else
            {
                return GroupTypeCache.Get( this.GroupTypeId ).IconCssClass;
            }
        }

        #endregion Methods
    }
}
