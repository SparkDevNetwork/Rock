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

using Rock.ViewModel;

namespace Rock.Model
{
    /// <summary>
    /// Service/Data access class for Rock.Model.RegistrationTemplate
    /// </summary>
    public partial class RegistrationTemplateService
    {
    }

    /// <summary>
    /// RegistrationTemplateViewModelHelper
    /// </summary>
    public partial class RegistrationTemplateViewModelHelper
    {
        /// <summary>
        /// Applies the additional properties and security to view model.
        /// </summary>
        /// <param name="model">The model.</param>
        /// <param name="viewModel">The view model.</param>
        /// <param name="currentPerson">The current person.</param>
        /// <param name="loadAttributes">if set to <c>true</c> [load attributes].</param>
        public override void ApplyAdditionalPropertiesAndSecurityToViewModel( RegistrationTemplate model, RegistrationTemplateViewModel viewModel, Person currentPerson = null, bool loadAttributes = true )
        {
            viewModel.PluralRegistrantTerm = model.RegistrantTerm.Pluralize();
        }
    }
}
