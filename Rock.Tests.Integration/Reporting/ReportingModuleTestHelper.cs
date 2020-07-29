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
using System.Diagnostics;
using System.Linq;
using Rock.Data;
using Rock.Model;

namespace Rock.Tests.Integration
{
    /// <summary>
    /// Functions to assist with testing the reporting module.
    /// </summary>
    public class ReportingModuleTestHelper
    {
        private string _RecordTag = null;

        /// <summary>
        /// Constuctor
        /// </summary>
        /// <param name="recordTag">A tag that is added to the ForeignKey property of each record created by this helper instance.</param>
        public ReportingModuleTestHelper( string recordTag )
        {
            _RecordTag = recordTag;
        }

        /// <summary>
        /// Add or update a DataView.
        /// </summary>
        /// <param name="dataContext"></param>
        /// <param name="dataView"></param>
        public void AddOrUpdateDataView( RockContext dataContext, DataView dataView )
        {
            var dataViewService = new DataViewService( dataContext );

            var existingDataView = dataViewService.Queryable().FirstOrDefault( x => x.Guid == dataView.Guid );

            if ( existingDataView == null )
            {
                dataViewService.Add( dataView );

                existingDataView = dataView;
            }
            else
            {
                existingDataView.CopyPropertiesFrom( dataView );
            }
        }

        /// <summary>
        /// Create a new DataView
        /// </summary>
        /// <param name="name"></param>
        /// <param name="guid"></param>
        /// <param name="appliesToEntityTypeId"></param>
        /// <param name="order"></param>
        /// <returns></returns>
        public DataView CreateDataView( string name, Guid guid, int appliesToEntityTypeId, int order = 0 )
        {
            var newDataView = new DataView();

            newDataView.Name = name;
            newDataView.Guid = guid;
            newDataView.IsSystem = true;
            newDataView.EntityTypeId = appliesToEntityTypeId;
            //newDataView.Order = order;

            newDataView.ForeignKey = _RecordTag;

            return newDataView;
        }

        /// <summary>
        /// Remove DataViews flagged with the current test record tag.
        /// </summary>
        /// <param name="dataContext"></param>
        /// <returns></returns>
        public int DeleteDataViewsByRecordTag( RockContext dataContext )
        {
            // Remove DataViews associated with the current test record tag.
            var recordsDeleted = dataContext.Database.ExecuteSqlCommand( $"delete from [DataView] where [ForeignKey] = '{_RecordTag}'" );

            Debug.Print( $"Delete Test Data: {recordsDeleted} DataViews deleted." );

            return recordsDeleted;
        }
    }
}
