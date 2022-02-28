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
namespace Rock.Web.UI.Controls
{
    /// <summary>
    /// Indicates that the control using this interface can be notified of the attribute ID it is being used for. This is usefull
    /// for controls that can change the attribute configuration outside of the AttribueEditor. e.g. DefinedValueWithAdd and the list of SelectableValues.
    /// </summary>
    public interface IHasAttributeId
    {

        /// <summary>
        /// Gets or sets the attribute identifier. Will be used to update the attribute/fieldtype configuration if it is not null and implemented by the control.
        /// </summary>
        /// <value>
        /// The attribute identifier.
        /// </value>
        int? AttributeId { get; set; }
    }
}
