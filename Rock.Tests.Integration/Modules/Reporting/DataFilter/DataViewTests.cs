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
using System.Diagnostics;
using System.Linq;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Rock.Data;
using Rock.Model;
using Rock.Tests.Shared;
using Rock.Tests.Shared.TestFramework;

namespace Rock.Tests.Integration.Modules.Reporting.DataFilter
{
    [TestClass]
    public class DataViewTests : DatabaseTestsBase
    {
        #region Tests

        /// <summary>
        /// Verify that all Data Views can be processed.
        /// </summary>
        [TestMethod]
        [TestCategory( "Rock.Reporting.Tests" )]
        [TestProperty( "Feature", TestFeatures.Reporting )]
        public void DataView_AllDataViews_CanExecute()
        {
            var dataContext = new RockContext();
            var dataViewService = new DataViewService( dataContext );
            var dataViewIdList = dataViewService.Queryable()
                .Select( dv => dv.Id )
                .ToList();

            int dataViewTotal = dataViewIdList.Count;
            int dataViewCount = 0;
            var exceptions = new List<Exception>();

            foreach ( var dataViewId in dataViewIdList )
            {
                dataContext = new RockContext();
                dataViewService = new DataViewService( dataContext );

                var dataView = dataViewService.Get( dataViewId );

                dataViewCount++;
                LogHelper.Log( $"Evaluating Data View \"{dataView.Name}\"... ({dataViewCount} of {dataViewTotal})" );

                try
                {
                    var args = new DataViewGetQueryArgs
                    {
                        DatabaseTimeoutSeconds = 300,
                        DbContext = dataContext
                    };
                    var query = dataView.GetQuery( args );

                    var stopwatch = Stopwatch.StartNew();
                    var results = query.Select( x => x.Id ).ToList();
                    stopwatch.Stop();

                    LogHelper.Log( $"Data View processed. [ItemCount={results.Count}, Time={stopwatch.Elapsed.TotalSeconds:0.00}s]" );
                }
                catch ( Exception ex )
                {
                    exceptions.Add( new Exception( $"Data View failed. [Name={dataView.Name}]", ex ) );

                    LogHelper.LogError( $"Data View failed. {ex.Message}" );
                }
            }

            if ( exceptions.Any() )
            {
                throw new AggregateException( "One or more Data Views failed to process.", exceptions );
            }
        }

        #endregion
    }
}