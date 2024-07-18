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
    /// A component that prepares an empty Rock database template for specific testing by adding relevant sample data.
    /// </summary>
    /// <remarks>
    /// The initializer should add all datasets that are needed for the current test run.
    /// It should also execute any post-migration tasks that are necessary to finalize the data, such as Rock Jobs.
    /// </remarks>
    public interface ITestDatabaseInitializer
    {
        /// <summary>
        /// Initialize the database.
        /// </summary>
        void Initialize();

        /// <summary>
        /// A descriptive key that identifies the dataset.
        /// This key is added to the archive file name to identify the data initializer used to create the test database image.
        /// </summary>
        string DatasetIdentifier { get; }
    }
}
