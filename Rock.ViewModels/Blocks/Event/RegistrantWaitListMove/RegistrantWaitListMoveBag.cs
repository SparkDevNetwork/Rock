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
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rock.ViewModels.Blocks.Event.RegistrantWaitListMove
{
    /// <summary>
    /// 
    /// </summary>
    public class RegistrantWaitListMoveBag
    {
        /// <summary>
        /// 
        /// </summary>
        public List<RegistrantWaitListMoveRecipientBag> Recipients { get; set; }

        /// <summary>
        /// Gets or sets the email bag containing the pre-populated and Lava-resolved
        /// email template fields shown to the user before sending.
        /// </summary>
        public RegistrantWaitListMoveEmailBag EmailBag { get; set; }

        /// <summary>
        /// Gets or sets the confirmation message shown after registrants are moved,
        /// indicating how many individuals were transitioned off the wait list.
        /// </summary>
        public string UpdateMessage { get; set; }

    }
}
