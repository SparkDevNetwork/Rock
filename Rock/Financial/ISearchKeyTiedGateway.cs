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
using Rock.Model;
using System;

namespace Rock.Financial
{
    /// <summary>
    /// A Financial Gateway that uses a particular person search key.
    /// Note:  This was added to Rock core to support Gift and ScheduledGift bus event messages
    /// that were adapted from an external solution.  (See <see cref="Rock.Bus.Message.GiftWasGivenMessage"/>
    /// and <see cref="Rock.Bus.Message.ScheduledGiftWasModifiedMessage"/>.)
    /// </summary>
    public interface ISearchKeyTiedGateway
    {
        /// <summary>
        /// Gets the type of the person search key.
        /// </summary>
        /// <param name="financialGateway">The financial gateway.</param>
        /// <returns></returns>
        Guid? GetPersonSearchKeyTypeGuid( FinancialGateway financialGateway );
    }
}
