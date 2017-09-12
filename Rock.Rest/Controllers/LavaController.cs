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
using System.Web.Http;
using System.Collections.Generic;
using System.Linq;
using Rock.Web.Cache;
using Rock.Web.UI.Controls;
using Rock.Rest.Filters;

namespace Rock.Rest.Controllers
{
    /// <summary>
    /// Controller of misc utility functions that are used by Rock controls
    /// </summary>
    public class LavaController : ApiControllerBase
    {

        /// <summary>
        /// Renders the template.
        /// </summary>
        /// <param name="template">The template.</param>
        /// <returns></returns>
        [System.Web.Http.Route( "api/Lava/RenderTemplate" )]
        [HttpPost]
        [Authenticate, Secured]
        public string RenderTemplate( [NakedBody] string template )
        {
            Rock.Lava.CommonMergeFieldsOptions lavaOptions = new Lava.CommonMergeFieldsOptions();
            lavaOptions.GetPageContext = false;
            lavaOptions.GetPageParameters = false;
            lavaOptions.GetCurrentPerson = true;
            lavaOptions.GetCampuses = true;
            lavaOptions.GetLegacyGlobalMergeFields = false;

            Dictionary<string, object> mergeFields = Rock.Lava.LavaHelper.GetCommonMergeFields(null, GetPerson(), lavaOptions);
            
            return template.ResolveMergeFields( mergeFields );
        }
    }
}
