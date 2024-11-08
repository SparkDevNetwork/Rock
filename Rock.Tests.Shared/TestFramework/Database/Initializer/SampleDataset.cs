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
using System.Configuration;
using Rock.Lava.Fluid;
using Rock.Model;
using Rock.Tests.Shared.Lava;
using Rock.Utility;
using Rock.Web;

namespace Rock.Tests.Shared.TestFramework.Database.Initializer
{
    /// <summary>
    /// Initializes an empty database with the specified Rock sample data set.
    /// </summary>
    public class SampleDataset : ITestDatabaseInitializer
    {
        public const string SampleDataSourceKey = "com.rockrms.test.SampleDataSource";
        public string SampleDataUrl = null;

        /// <summary>
        /// Override this method to set a unique identifier for this dataset.
        /// </summary>
        public virtual string DatasetIdentifier => "sample14";

        public void Initialize()
        {
            // Get the sample data source.
            var sampleDataUrl = ConfigurationManager.AppSettings["SampleDataUrl"].ToStringSafe();

            AddSampleDataForActiveDatabase( sampleDataUrl );
            OnSampleDataAdded();

            RunRockJobsForDataInitialization();
            OnPostUpgradeJobsCompleted();

            // Set the sample data identifiers.
            SystemSettings.SetValue( SystemKey.SystemSetting.SAMPLEDATA_DATE, RockDateTime.Now.ToString() );
            SystemSettings.SetValue( SampleDataSourceKey, sampleDataUrl );
        }

        private void AddSampleDataForActiveDatabase( string sampleDataUrl )
        {
            // Load the standard Rock sample data set.
            TestHelper.Log( "Loading Sample Data..." );

            // Make sure all Entity Types are registered.
            // This is necessary because some components are only registered at runtime,
            // including the Rock.Bus.Transport.InMemory Type that is required to start the Rock Message Bus.
            EntityTypeService.RegisterEntityTypes();

            LavaIntegrationTestHelper.Initialize( testRockLiquidEngine: true, testDotLiquidEngine: false, testFluidEngine: true, loadShortcodes: false );

            var lavaEngine = LavaIntegrationTestHelper.GetEngineInstance<FluidEngine>();

            var factory = new SampleDataManager( lavaEngine );
            var args = new SampleDataManager.SampleDataImportActionArgs
            {
                FabricateAttendance = true,
                EnableGiving = true,
                Password = "password"
            };

            factory.CreateFromXmlDocumentFile( sampleDataUrl, args );

            TestHelper.Log( $"Loading Sample Data: completed." );
        }

        /// <summary>
        /// Run Rock Jobs to ensure calculated fields are updated.
        /// </summary>
        private void RunRockJobsForDataInitialization()
        {
            TestHelper.Log( "Executing Rock Jobs..." );

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

            TestHelper.Log( "Executing Rock Jobs: completed." );
        }

        protected void ExecuteRockJob( string jobName, Action action )
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

        #region Extensibility

        /// <summary>
        /// Override this method to modify the test database after the sample dataset has been added.
        /// </summary>
        protected virtual void OnSampleDataAdded()
        {
            return;
        }

        /// <summary>
        /// Override this method to perform additional actions to prepare the test database
        /// after the post-upgrade jobs associated with the standard sample data are completed.
        /// </summary>
        protected virtual void OnPostUpgradeJobsCompleted()
        {
            return;
        }

        #endregion

    }
}
