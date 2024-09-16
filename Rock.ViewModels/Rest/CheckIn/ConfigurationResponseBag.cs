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

using Rock.ViewModels.CheckIn;

namespace Rock.ViewModels.Rest.CheckIn
{
    /// <summary>
    /// The response to the request to get all check-in configuration data.
    /// </summary>
    public class ConfigurationResponseBag
    {
        /// <summary>
        /// Gets or sets the templates that are valid for use with check-in.
        /// </summary>
        /// <value>The templates.</value>
        public List<ConfigurationTemplateBag> Templates { get; set; }

        /// <summary>
        /// Gets or sets the areas that are valid for use with check-in.
        /// </summary>
        /// <value>The areas items.</value>
        public List<ConfigurationAreaBag> Areas { get; set; }
    }
}
