using System;
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
using System.Collections.Generic;
using System.Linq;
using System.Web.UI.WebControls;
using Rock.Model;

namespace Rock.Web.UI.Controls
{
    /// <summary>
    /// 
    /// </summary>
    public class MetricsPicker : RockCheckBoxList
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MetricsPicker" /> class.
        /// </summary>
        public MetricsPicker()
            : base()
        {
            Label = "Metrics";
        }

        /// <summary>
        /// Sets the metrics.
        /// </summary>
        /// <value>
        /// The metrics.
        /// </value>
        public List<Metric> Metrics
        {
            set
            {
                this.Items.Clear();
                foreach ( Metric metric in value )
                {
                    ListItem metricItem = new ListItem();
                    metricItem.Value = metric.Guid.ToString();
                    metricItem.Text = metric.Title;
                    this.Items.Add( metricItem );
                }
            }
        }

        /// <summary>
        /// Gets the available metric Guids.
        /// </summary>
        /// <value>
        /// The available metric Guids.
        /// </value>
        public List<Guid> AvailableMetricGuids
        {
            get
            {
                return this.Items.OfType<ListItem>().Select( a => a.Value.AsGuid() ).ToList();
            }
        }

        /// <summary>
        /// Gets or sets the selected metric Guids.
        /// </summary>
        /// <value>
        /// The selected metric Guids.
        /// </value>
        public List<Guid> SelectedMetricGuids
        {
            get
            {
                return this.Items.OfType<ListItem>().Where( l => l.Selected ).Select( a => a.Value.AsGuid() ).ToList();
            }

            set
            {
                foreach ( ListItem item in this.Items )
                {
                    item.Selected = value.Exists( a => a.Equals( item.Value ) );
                }
            }
        }
    }
}