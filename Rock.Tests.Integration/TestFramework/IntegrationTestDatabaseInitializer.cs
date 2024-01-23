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

using System;
using System.Collections.Generic;
using Rock.Model;
using Rock.Tests.Shared;
using Rock.Utility;
using Rock.Web;

namespace Rock.Tests.Integration.Database
{
    public interface ITestDatabaseInitializer
    {
        void InitializeSnapshot( string snapshotName, string sampleDataUrl );
    }

    public static class TestDatabaseSnapshotNames
    {
        /// <summary>
        /// Specifies a database with standard sample data.
        /// </summary>
        public static string Default = "default";

        /// <summary>
        /// Species a database with no sample data.
        /// </summary>
        public static string Empty = "empty";
    }

    /// <summary>
    /// Initializes an empty database with the specified Rock sample data set.
    /// </summary>
    public class IntegrationTestDatabaseInitializer : ITestDatabaseInitializer
    {
        public void InitializeSnapshot( string snapshotName, string sampleDataUrl )
        {
            if ( snapshotName == TestDatabaseSnapshotNames.Empty )
            {
                // Do not load any additional data.
            }
            else
            {
                // Load the standard Rock sample data set.
                AddSampleDataForActiveDatabase( sampleDataUrl );
            }
        }

        private static bool AddSampleDataForActiveDatabase( string sampleDataUrl )
        {
            TestHelper.Log( "Loading sample data..." );

            // Make sure all Entity Types are registered.
            // This is necessary because some components are only registered at runtime,
            // including the Rock.Bus.Transport.InMemory Type that is required to start the Rock Message Bus.
            EntityTypeService.RegisterEntityTypes();

            var factory = new SampleDataManager();
            var args = new SampleDataManager.SampleDataImportActionArgs
            {
                FabricateAttendance = true,
                EnableGiving = true
            };

            factory.CreateFromXmlDocumentFile( sampleDataUrl, args );

            //
            // Run Rock Jobs to ensure calculated fields are updated.
            //

            // Rock Cleanup
            var jobCleanup = new Rock.Jobs.RockCleanup();

            ExecuteRockJob( "RockCleanup", () => { jobCleanup.ExecuteInternal( new Dictionary<string, string>() ); } );

            // Calculate Family Analytics
            var jobFamilyAnalytics = new Rock.Jobs.CalculateFamilyAnalytics();

            ExecuteRockJob( "CalculateFamilyAnalytics", () => { jobFamilyAnalytics.ExecuteInternal( new Dictionary<string, string>() ); } );

            // Process BI Analytics
            var jobBIAnalytics = new Rock.Jobs.ProcessBIAnalytics();

            var biAnalyticsSettings = new Dictionary<string, string>();
            biAnalyticsSettings.AddOrReplace( Rock.Jobs.ProcessBIAnalytics.AttributeKey.ProcessPersonBIAnalytics, "true" );
            biAnalyticsSettings.AddOrReplace( Rock.Jobs.ProcessBIAnalytics.AttributeKey.ProcessFamilyBIAnalytics, "true" );
            biAnalyticsSettings.AddOrReplace( Rock.Jobs.ProcessBIAnalytics.AttributeKey.ProcessAttendanceBIAnalytics, "true" );

            ExecuteRockJob( "ProcessBiAnalytics", () => { jobBIAnalytics.ExecuteInternal( biAnalyticsSettings ); } );

            // Calculate Attribute "ValueAs..." columns.
            var jobUpdateAttributeValueAs = new Rock.Jobs.PostV141UpdateValueAsColumns();
            jobUpdateAttributeValueAs.ExecuteInternal( new Dictionary<string, string>() );

            // Set the sample data identifiers.
            SystemSettings.SetValue( SystemKey.SystemSetting.SAMPLEDATA_DATE, RockDateTime.Now.ToString() );
            SystemSettings.SetValue( TestDatabaseHelper.SampleDataSourceKey, sampleDataUrl );

            TestHelper.Log( $"Sample Data loaded." );

            return true;
        }

        private static void ExecuteRockJob( string jobName, Action action )
        {
            try
            {
                TestHelper.Log( $"Job Started: {jobName}..." );
                action();
                TestHelper.Log( $"Job Completed: {jobName}..." );
            }
            catch ( Exception ex )
            {
                TestHelper.Log( $"WARNING: Job failed. [Job={jobName}]\n{ex.ToString()}" );
            }
        }
    }
}
