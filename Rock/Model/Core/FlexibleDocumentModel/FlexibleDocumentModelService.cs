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

namespace Rock.Model
{
    /// <summary>
    /// Data access and service class for <see cref="Rock.Model.FlexibleDocumentModel"/> entity objects.
    /// </summary>
    public partial class FlexibleDocumentModelService
    {
        /// <summary>
        /// Returns the <see cref="Rock.Model.FlexibleDocumentModel"/> with the specified unique key.
        /// </summary>
        /// <param name="key">The unique key of the model (e.g. <c>AgentMemory</c>).</param>
        /// <returns>The matching <see cref="Rock.Model.FlexibleDocumentModel"/>, or <c>null</c> if no model has that key.</returns>
        public FlexibleDocumentModel GetByKey( string key )
        {
            return Queryable().FirstOrDefault( m => m.Key == key );
        }
    }
}
