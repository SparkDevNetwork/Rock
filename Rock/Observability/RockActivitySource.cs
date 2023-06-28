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

using System.Diagnostics;
using Rock.SystemKey;

namespace Rock.Observability
{
    /// <summary>
    /// Global ActivitySource for Rock's observability framework.
    /// </summary>
    public static class RockActivitySource
    {
        #region Private Methods
        private static ActivitySource _activitySource = null;
        private const string _sourceVersion = "1.0.0";
        #endregion

        #region Properties
        /// <summary>
        /// Returns the global ActivitySource
        /// </summary>
        public static ActivitySource ActivitySource
        {
            get
            {
                return _activitySource;
            }
        }
        #endregion

        /// <summary>
        /// Sets up the OpenTelemetry ActivitySource
        /// </summary>
        static RockActivitySource()
        {
            // The values on the start must match those in the start-up
            _activitySource = new ActivitySource( Rock.Web.SystemSettings.GetValue( SystemSetting.OBSERVABILITY_SERVICE_NAME ), _sourceVersion );
        }

        /// <summary>
        /// This method is called to refresh the activity source in the case where an administrator resets the service name.
        /// </summary>
        public static void RefreshActivitySource()
        {
            _activitySource = new ActivitySource( Rock.Web.SystemSettings.GetValue( SystemSetting.OBSERVABILITY_SERVICE_NAME ), _sourceVersion );
        }
    }
}
