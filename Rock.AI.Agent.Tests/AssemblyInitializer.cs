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

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Rock.AI.Agent.Tests.TestFramework;

namespace Rock.AI.Agent.Tests
{
    /// <summary>
    /// Assembly-wide setup for the test run.
    /// </summary>
    [TestClass]
    public static class AssemblyInitializer
    {
        /// <summary>
        /// Loads the SQL Server spatial native libraries (on supported
        /// architectures) so that <see cref="System.Data.Entity.Spatial.DbGeography"/>
        /// operations can be evaluated client-side against the mocked database.
        /// Without this, calls such as <c>DbGeography.Distance</c> throw a
        /// <c>DllNotFoundException</c> for <c>SqlServerSpatial110.dll</c> when a
        /// query is executed as LINQ-to-Objects rather than translated to SQL. On
        /// ARM the load is skipped and the spatial tests are ignored instead.
        /// </summary>
        /// <param name="context">The test context supplied by MSTest.</param>
        [AssemblyInitialize]
        public static void Initialize( TestContext context )
        {
            SqlServerSpatialSupport.EnsureNativeAssembliesLoaded();
        }
    }
}
