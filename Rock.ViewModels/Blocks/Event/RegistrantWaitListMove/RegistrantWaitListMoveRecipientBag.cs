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
    public class RegistrantWaitListMoveRecipientBag
    {
        /// <summary>
        /// 
        /// </summary>
        public int Id { get; set; }
        /// <summary>
        /// Name of person who registered
        /// </summary>
        public string RegistrantName { get; set; }

        /// <summary>
        /// Indicates if the recipient is selected to receive the email notification about being moved off the wait-list.
        /// </summary>
        public bool IsSelected { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string Email { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public List<string> RegisteredNames { get; set; }

    }
}
