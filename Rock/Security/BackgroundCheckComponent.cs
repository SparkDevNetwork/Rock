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
using System.Collections.Generic;

using Rock.Data;
using Rock.Extension;
using Rock.Web.Cache;

namespace Rock.Security
{
    /// <summary>
    /// The base class for all background check requests
    /// </summary>
    public abstract class BackgroundCheckComponent : Component
    {
        /// <summary>
        /// Abstract method for sending a background request. Derived classes should implement
        /// this method to initiate a background request. Because requests will be made through a
        /// workflow, the workflow is passed to this method. It is up to the derived class to
        /// evaluate/update any workflow attributes it needs
        /// </summary>
        /// <param name="rockContext">The rock context.</param>
        /// <param name="workflow">The Workflow initiating the request.</param>
        /// <param name="personAttribute">The person attribute.</param>
        /// <param name="ssnAttribute">The SSN attribute.</param>
        /// <param name="requestTypeAttribute">The request type attribute.</param>
        /// <param name="billingCodeAttribute">The billing code attribute.</param>
        /// <param name="errorMessages">The error messages.</param>
        /// <returns>
        /// True/False value of whether the request was successfully sent or not
        /// </returns>
        public abstract bool SendRequest( RockContext rockContext, Rock.Model.Workflow workflow, 
            AttributeCache personAttribute, AttributeCache ssnAttribute, AttributeCache requestTypeAttribute, 
            AttributeCache billingCodeAttribute, out List<string> errorMessages );

        /// <summary>
        /// Gets the URL to the background check report.
        /// Note: Also used by GetBackgroundCheck.ashx.cs, ProcessRequest( HttpContext context )
        /// </summary>
        /// <param name="reportKey">The report key.</param>
        /// <returns></returns>
        public abstract string GetReportUrl( string reportKey );
    }

}
