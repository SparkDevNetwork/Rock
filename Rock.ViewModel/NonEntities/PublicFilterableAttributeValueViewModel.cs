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

namespace Rock.ViewModel.NonEntities
{
    /// <summary>
    /// Describes the data sent to and from remote systems to allow editing of
    /// attribute filter values as well as the current value.
    /// </summary>
    public class PublicFilterableAttributeValueViewModel : PublicFilterableAttributeViewModel
    {
        /// <summary>
        /// Gets or sets the current value used for comparison.
        /// </summary>
        /// <value>The current value used for comparison.</value>
        public PublicComparisonValueViewModel Value { get; set; }
    }
}
