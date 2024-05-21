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

namespace Rock.Tests.Shared
{
    /// <summary>
    /// Unique String identifiers 
    /// </summary>
    public static class RecordTag
    {
        public const string PrayerRequestFeature = "PrayerRequestFeatureTest";
    }

    public static class TestCategories
    {
        // This Test Category contains methods for adding test data to the current database.
        public const string AddData = "Rock.Setup.AddData";
        // This Test Category contains methods for removing test data from the current database.
        public const string RemoveData = "Rock.Setup.RemoveData";
        // This Test Category contains methods for maintaining specific elements of the test data during the development process.
        public const string DeveloperSetup = "Rock.Setup.Dev";
    }

    public static class TestFeatures
    {
        // Tests having this property relate to data setup for integration tests.
        public const string DataSetup = "Data Setup (Basic)";
        public const string DataSetupBulk = "Data Setup (Bulk)";

        // Tests having this property relate to data maintenance that may be needed during the development process.
        public const string DataMaintenance = "Data Maintenance";

        // Tests having this property relate to the Reporting feature of Rock.
        public const string Reporting = "Reporting";

        // Tests having this property relate to the Steps feature of Rock.
        public const string Steps = "Steps";

        public const string Communications = "Communications";
        public const string Groups = "Groups";
        public const string Personalization = "Personalization";

        public const string Lava = "Core.Lava";
        public const string DataModelValidation = "Core.Model.Validation";
    }

    public static class TestPurposes
    {
        // Tests having this purpose are intended to measure the performance of the function being tested.
        public const string Performance = "Performance";
        // Tests having this purpose are intended to validate the function being tested.
        public const string Validation = "Validation";
    }

}

