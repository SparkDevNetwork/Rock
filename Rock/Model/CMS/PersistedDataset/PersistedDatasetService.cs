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
using System.Data;
using System.Data.Entity;
using System.Linq;

namespace Rock.Model
{
    /// <summary>
    /// Data Access/Service class for <see cref="Rock.Model.PersistedDataset"/> entities.
    /// </summary>
    public partial class PersistedDatasetService
    {
        /// <summary>
        /// Gets from access key no tracking.
        /// </summary>
        /// <param name="accessKey">The access key.</param>
        /// <returns></returns>
        public PersistedDataset GetFromAccessKeyNoTracking( string accessKey )
        {
            return this.Queryable().Where( a => a.AccessKey == accessKey ).AsNoTracking().FirstOrDefault();
        }
    }
}
