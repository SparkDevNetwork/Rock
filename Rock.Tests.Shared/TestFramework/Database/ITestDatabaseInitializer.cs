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

namespace Rock.Tests.Shared.TestFramework
{
    /// <summary>
    /// A component that can initialize an empty Rock database with a specified set of sample data.
    /// </summary>
    public interface ITestDatabaseInitializer
    {
        void InitializeSnapshot( string snapshotName, string sampleDataUrl );
    }
}
