// <copyright>
// Copyright Southeast Christian Church
//
// Licensed under the  Southeast Christian Church License (the "License");
// you may not use this file except in compliance with the License.
// A copy of the License shoud be included with this file.
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
// </copyright>
//

using Rock.Data;

namespace org.secc.RecurringCommunications.Data
{
    public class RecurringCommunicationsService<T> : Rock.Data.Service<T> where T : Rock.Data.Entity<T>, new()
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="RecurringCommunicationsService{T}"/> class.
        /// </summary>
        public RecurringCommunicationsService( RockContext context )
            : base( context )
        {
        }

        /// <summary>
        /// Determines whether this instance can delete the specified item.
        /// </summary>
        /// <param name="item">The item.</param>
        /// <param name="errorMessage">The error message.</param>
        /// <returns></returns>
        public virtual bool CanDelete( T item, out string errorMessage )
        {
            errorMessage = string.Empty;
            return true;
        }
    }
}
