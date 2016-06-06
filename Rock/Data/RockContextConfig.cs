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
using System.Data.Entity.SqlServer;

namespace Rock.Data
{
    /// <summary>
    /// Create a DbConfiguration that uses an execution strategy that will retry exceptions that are 
    /// known to be possibley transient when working with SqlAzure
    /// </summary>
    public class RockContextConfig : DbConfiguration
    {
        public RockContextConfig()
        {
            SetExecutionStrategy( "System.Data.SqlClient", () => new SqlAzureExecutionStrategy() );
        }
    }
}