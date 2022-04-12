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

using System.Linq;
using Rock.ViewModels.Entities;
using Rock.Web.Cache;

namespace Rock.Model
{
    /// <summary>
    /// Attribute View Model Helper
    /// </summary>
    public partial class AttributeViewModelHelper
    {
        /// <summary>
        /// Applies the additional properties and security to view model.
        /// </summary>
        /// <param name="model">The model.</param>
        /// <param name="viewModel">The view model.</param>
        /// <param name="currentPerson">The current person.</param>
        /// <param name="loadAttributes">if set to <c>true</c> [load attributes].</param>
        public override void ApplyAdditionalPropertiesAndSecurityToViewModel( Attribute model, AttributeBag viewModel, Person currentPerson = null, bool loadAttributes = true )
        {
            var attributeCache = AttributeCache.Get( model.Id );

            viewModel.FieldTypeGuid = FieldTypeCache.Get( attributeCache.FieldTypeId ).Guid;
            viewModel.CategoryGuids = attributeCache.Categories.Select( c => c.Guid ).ToArray();
            viewModel.QualifierValues = attributeCache.QualifierValues
                .ToDictionary(
                kvp => kvp.Key,
                kvp => kvp.Value.Value );
        }
    }
}
