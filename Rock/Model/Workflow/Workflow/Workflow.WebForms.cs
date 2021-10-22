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
using System.Web;

namespace Rock.Model
{
    /// <summary>
    /// Workflow WebForms References
    /// </summary>
    public partial class Workflow
    {
        /// <summary>
        /// Sets the initiator.
        /// </summary>
        internal void SetInitiator()
        {
            if ( !InitiatorPersonAliasId.HasValue &&
                HttpContext.Current != null &&
                HttpContext.Current.Items.Contains( "CurrentPerson" ) )
            {
                var currentPerson = HttpContext.Current.Items["CurrentPerson"] as Person;
                if ( currentPerson != null )
                {
                    InitiatorPersonAliasId = currentPerson.PrimaryAliasId;
                }
            }
        }
    }
}

