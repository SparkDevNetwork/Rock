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
using System.ComponentModel;
using System.ComponentModel.Composition;

namespace Rock.Reporting.DataFilter.BenevolenceRequest
{
    /// <summary>
    /// 
    /// </summary>
    [Description("Filter benevolence requests that are associated with a specific active campus.")]
    [Export(typeof(DataFilterComponent))]
    [ExportMetadata("ComponentName", "Benevolence Request Active Campus Filter")]
    public class CampusActiveFilter : CampusFilter
    {
        #region Properties

        /// <summary>
        /// Gets the control class name.
        /// </summary>
        /// <value>
        /// The name of the control class.
        /// </value>
        internal override string ControlClassName
        {
            get { return "js-campus-active-picker"; }
        }

        /// <summary>
        /// Gets a value indicating whether to include inactive campuses.
        /// </summary>
        /// <value>
        ///   <c>true</c> if [include inactive]; otherwise, <c>false</c>.
        /// </value>
        internal override bool IncludeInactive
        {
            get { return false; }
        }

        #endregion
        #region Public Methods

        /// <summary>
        /// Gets the title.
        /// </summary>
        /// <param name="entityType"></param>
        /// <returns></returns>
        /// <value>
        /// The title.
        /// </value>
        public override string GetTitle(Type entityType)
        {
            return "Campus (Active Only)";
        }

        #endregion
    }
}