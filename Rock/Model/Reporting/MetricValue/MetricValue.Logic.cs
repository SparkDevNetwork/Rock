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

using System.Linq;
using System.Runtime.Serialization;

namespace Rock.Model
{
    public partial class MetricValue
    {
        /// <summary>
        /// Gets the metric value partitions as a comma-delimited list of EntityTypeId|EntityId
        /// </summary>
        /// <value>
        /// The metric value entityTypeId,EntityId partitions
        /// </value>
        [DataMember]
        public virtual string MetricValuePartitionEntityIds
        {
            get
            {
                if ( _metricValuePartitionEntityIds == null )
                {
                    var list = this.MetricValuePartitions.Select( a => new { a.MetricPartition.EntityTypeId, a.EntityId } ).ToList();
                    _metricValuePartitionEntityIds = list.Select( a => string.Format( "{0}|{1}", a.EntityTypeId, a.EntityId ) ).ToList().AsDelimited( "," );
                }

                return _metricValuePartitionEntityIds;
            }
        }

        private string _metricValuePartitionEntityIds;

        /// <summary>
        /// Gets the parent authority.
        /// </summary>
        public override Security.ISecured ParentAuthority
        {
            get
            {
                return this.Metric != null ? this.Metric : base.ParentAuthority;
            }
        }

        /// <summary>
        /// Gets the metric value datetime as a javascript time stamp (handy for chart apis)
        /// </summary>
        /// <value>
        /// The metric value javascript time stamp.
        /// </value>
        [DataMember]
        public long DateTimeStamp
        {
            get
            {
                if ( this.MetricValueDateTime.HasValue )
                {
                    return this.MetricValueDateTime.Value.ToJavascriptMilliseconds();
                }
                else
                {
                    return 0;
                }
            }
        }
    }
}
