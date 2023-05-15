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
using Rock.Tests.Integration.Crm.Prayer;
using Rock.Tests.Integration.Crm.Steps;

namespace Rock.Tests.Integration
{
    /// <summary>
    /// Manages feature-based sets of sample data in a Rock database for the purpose of integration testing.
    /// </summary>
    public static class SampleDataHelper
    {
        private static List<string> _loadedFeatureDataSets = new List<string>();

        public static class DataSetIdentifiers
        {
            public static string PrayerSampleData = "PrayerSampleData";
            public static string StepsSampleData = "StepsSampleData";
        }

        /// <summary>
        /// Add a well-known set of test data.
        /// </summary>
        /// <param name="datasetIdentifier">A <c>SampleDataHelper.DataSetIdentifiers</c> value that uniquely identifies the data set.</param>
        public static void AddTestDataSet( string datasetIdentifier )
        {
            if ( GetFeatureDataLoadState( datasetIdentifier ) )
            {
                return;
            }

            var isValid = false;

            if ( datasetIdentifier == DataSetIdentifiers.PrayerSampleData )
            {
                PrayerFeatureDataHelper.AddSampleData();
                isValid = true;
            }
            else if ( datasetIdentifier == DataSetIdentifiers.StepsSampleData )
            {
                StepsFeatureDataHelper.AddSampleTestData();
                isValid = true;
            }

            else
            {
                
            }

            if (!isValid)
            {
                throw new Exception( $"Invalid Data Set. The data set \"{datasetIdentifier}\" could not be loaded." );
            }

            SetFeatureDataLoadState( datasetIdentifier );
        }

        public static void RemoveTestDataSet( string datasetIdentifier )
        {
            if ( !GetFeatureDataLoadState( datasetIdentifier ) )
             {
                return;
            }

            var isValid = false;

            if ( datasetIdentifier == DataSetIdentifiers.PrayerSampleData )
            {
                PrayerFeatureDataHelper.RemoveSampleData();
                isValid = true;
            }
            else if ( datasetIdentifier == DataSetIdentifiers.StepsSampleData )
            {
                StepsFeatureDataHelper.RemoveStepsFeatureTestData();
                isValid = true;
            }

            if ( !isValid )
            {
                throw new Exception( $"Invalid Data Set. The data set \"{datasetIdentifier}\" could not be loaded." );
            }

            SetFeatureDataLoadState( datasetIdentifier );
        }

        //public static void RemoveSampleData()
        //{
        //    _factory.RemovePrayerRequestTestData();
        //}

        //public static void AddSampleData( bool removeExistingData = false )
        //{
        //    if ( removeExistingData )
        //    {
        //        RemoveSampleData();
        //    }

        //    _factory.AddPrayerCategories();
        //    _factory.AddPrayerRequestKnownData();
        //    _factory.AddPrayerRequestCommentsEmailTemplate();
        //}

        //private static bool AddPrayerFeatureData( string datasetIdentifier )
        //{
        //    var isValid = true;
        //    var factory = new StepsFeatureDataFactory();

        //    if ( datasetIdentifier == DataSetIdentifiers.PrayerSampleData )
        //    {
        //        factory.AddTestDataStepPrograms();
        //    }
        //    else if ( datasetIdentifier == DataSetIdentifiers.StepParticipations )
        //    {
        //        factory.AddKnownStepParticipations();
        //    }
        //    else if ( datasetIdentifier == DataSetIdentifiers.StepDataViews )
        //    {
        //        factory.AddStepDataViews();
        //    }
        //    else
        //    {
        //        isValid = false;
        //    }

        //    return isValid;
        //}


        //private static bool AddStepsFeatureData( string datasetIdentifier )
        //{
        //    var isValid = true;
        //    var factory = new StepsFeatureDataFactory();

        //    if ( datasetIdentifier == DataSetIdentifiers.StepsSampleData )
        //    {
        //        factory.AddTestDataStepPrograms();
        //    }
        //    else if ( datasetIdentifier == DataSetIdentifiers.StepParticipations )
        //    {
        //        factory.AddKnownStepParticipations();
        //    }
        //    else if ( datasetIdentifier == DataSetIdentifiers.StepDataViews )
        //    {
        //        factory.AddStepDataViews();
        //    }
        //    else
        //    {
        //        isValid = false;
        //    }

        //    return isValid;
        //}

        private static bool GetFeatureDataLoadState( string datasetName )
        {
            return _loadedFeatureDataSets.Contains( datasetName );
        }

        private static void SetFeatureDataLoadState( string datasetName, bool isLoaded = true )
        {
            if ( _loadedFeatureDataSets.Contains(datasetName) )
            {
                if ( !isLoaded )
                {
                    _loadedFeatureDataSets.Remove( datasetName );
                }
            }
            else
            {
                if (isLoaded)
                {
                    _loadedFeatureDataSets.Add( datasetName );
                }
            }
        }
    }
}