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
    /// Service/Data access class for <see cref="Rock.Model.Metric"/> entity objects.
    /// </summary>
    public partial class MetricService
    {
        /// <summary>
        /// Returns a <see cref="Rock.Model.Metric"/> by it's Id value.
        /// </summary>
        /// <param name="metricId">A <see cref="System.Int32"/> representing the Id of the <see cref="Rock.Model.Metric"/> to search for/return.</param>
        /// <returns>The <see cref="Rock.Model.Metric"/> with the provided Id value. If a matching <see cref="Rock.Model.Metric"/> is not found, null will be returned.</returns>
        public Metric GetById( int? metricId )
        {
            return Queryable().FirstOrDefault( t => t.Id == metricId );
        }

        /// <summary>
        /// Returns a <see cref="Rock.Model.Metric"/> by it's Guid value.
        /// </summary>
        /// <param name="guid">A <see cref="System.Guid"/> value representing the Guid value of the <see cref="Rock.Model.Metric"/> to search for/return.</param>
        /// <returns>The <see cref="Rock.Model.Metric"/> with a provided Guid value. If a <see cref="Rock.Model.Metric"/> is not found, null will be returned.</returns>
        public Metric GetByGuid( Guid guid )
        {
            return Queryable().FirstOrDefault( t => t.Guid == guid );
        }
    }
}
