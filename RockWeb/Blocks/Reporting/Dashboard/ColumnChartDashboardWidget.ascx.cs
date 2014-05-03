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
using System.ComponentModel;
using System.Linq;
using System.Web;
using System.Web.UI;
using Newtonsoft.Json;

using Rock;
using Rock.Data;
using Rock.Model;
using Rock.Reporting.Dashboard;

namespace RockWeb.Blocks.Reporting.Dashboard
{
    /// <summary>
    /// Template block for developers to use to start a new block.
    /// </summary>
    [DisplayName( "Column Chart DashboardWidget" )]
    [Category( "Dashboard" )]
    [Description( "Column Chart dashboard widget example" )]
    public partial class ColumnChartDashboardWidget : DashboardWidget
    {
        #region Base Control Methods

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Load" /> event.
        /// </summary>
        /// <param name="e">The <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnLoad( EventArgs e )
        {
            base.OnLoad( e );

            if ( !Page.IsPostBack )
            {
                Guid attendanceMetricGuid = new Guid( "D4752628-DFC9-4681-ADB3-01936B8F38CA" );
                colchrtExample.MetricId = new MetricService( new RockContext()).Get(attendanceMetricGuid).Id;
                colchrtExample.StartDate = new DateTime( 2013, 1, 1 );
                colchrtExample.EndDate = new DateTime( 2014, 1, 1 );
                
            }
        }

        #endregion
    }
}