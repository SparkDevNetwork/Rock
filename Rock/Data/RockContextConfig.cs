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
using System.Data.Entity;

namespace Rock.Data
{
    /// <summary>
    /// 
    /// </summary>
    /// <seealso cref="System.Data.Entity.DbConfiguration" />
    public class RockContextConfig : DbConfiguration
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="RockContextConfig"/> class.
        /// </summary>
        public RockContextConfig()
        {
            // default Initializer is CreateDatabaseIfNotExists, so set it to NULL so that nothing happens if there isn't a database yet
            this.SetDatabaseInitializer<RockContext>( null );
        }
    }
}