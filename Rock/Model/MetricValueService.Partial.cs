// <copyright>
// Copyright 2013 by the Spark Development Network
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.apache.org/licenses/LICENSE-2.0
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
using System.Linq;

using Rock.Data;

namespace Rock.Model
{
    /// <summary>
    /// Service/Data Access class for <see cref="Rock.Model.MetricValue"/> entity objects.
    /// </summary>
    public partial class MetricValueService 
    {
        /// <summary>
        /// Gets MetricValues by MetricId
        /// Returns an enumerable collection of <see cref="Rock.Model.MetricValue">MetricValues</see> by the MetricId of the <see cref="Rock.Model.Metric"/>
        /// that they are associated with.
        /// </summary>
        /// <param name="metricId">A <see cref="System.Int32"/> representing the MetricId to retreive <see cref="Rock.Model.MetricValue">MetricvValues</see> for.</param>
        /// <returns>An enumerable collection of <see cref="Rock.Model.MetricValue">MetricValues</see> that belong to the specified <see cref="Rock.Model.Metric"/></returns>
        public IEnumerable<Rock.Model.MetricValue> GetByMetricId( int? metricId )
        {
            return Repository.Find( t => ( t.MetricId == metricId || ( metricId == null && t.MetricId == null ) ) ).OrderBy( t => t.Order );
        }

        /// <summary>
        /// Returns a <see cref="Rock.Model.MetricValue"/> by it's GUID identifier.
        /// </summary>
        /// <param name="guid">The GUID value to search by.</param>
        /// <returns>The <see cref="Rock.Model.MetricValue"/> that contains the provided Guid. If no match is found, this value will be null.</returns>
        public MetricValue GetByGuid( Guid guid )
        {
            return Repository.FirstOrDefault( t => t.Guid == guid );
        }
    }
}
