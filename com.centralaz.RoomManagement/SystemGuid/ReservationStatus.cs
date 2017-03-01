// <copyright>
// Copyright by the Central Christian Church
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

namespace com.centralaz.RoomManagement.SystemGuid
{
    public static class ReservationStatus
    {
        /// <summary>
        /// A denied reservation
        /// </summary>
        public const string DENIED = "79A4347E-C399-403A-9053-8FB836354D77";

        /// <summary>
        /// An approved reservation
        /// </summary>
        public const string APPROVED = "D11163C8-4684-471F-9043-E976C75091E8";

        /// <summary>
        /// A pending reservation
        /// </summary>
        public const string NEEDS_APPROVAL = "E739F883-8B84-4755-92C0-3DB6606381F1";
    }
}
