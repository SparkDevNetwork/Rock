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
namespace Rock.ViewModels.Blocks.Security.AccountEntry
{
    /// <summary>
    /// A bag that contains the required information to render an account entry block's duplicate person item.
    /// </summary>
    public class AccountEntryDuplicatePersonItemBag
    {
        /// <summary>
        /// Gets or sets the person identifier.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Gets or sets the person's full name.
        /// </summary>
        /// <remarks>May be obscured if the potential duplicate person doesn't match by exact name.</remarks>
        public string FullName { get; set; }
    }
}
