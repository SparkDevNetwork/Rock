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

namespace Rock.Financial
{
    /// <summary>
    /// A Financial Gateway that provides a transaction status.
    /// Note:  This was added to Rock core to support Gift and ScheduledGift bus event messages
    /// that were adapted from an external solution.  (See <see cref="Rock.Bus.Message.GiftWasGivenMessage"/>
    /// and <see cref="Rock.Bus.Message.ScheduledGiftWasModifiedMessage"/>.)
    /// </summary>
    public interface IStatusProvidingGateway
    {
        /// <summary>
        /// Determines whether the specified status is success.
        /// </summary>
        /// <param name="status">The status.</param>
        /// <returns>
        ///   <c>true</c> if the specified status is success; otherwise, <c>false</c>.
        /// </returns>
        bool IsSuccessStatus( string status );

        /// <summary>
        /// Determines whether the specified status is failure.
        /// </summary>
        /// <param name="status">The status.</param>
        /// <returns>
        ///   <c>true</c> if the specified status is failure; otherwise, <c>false</c>.
        /// </returns>
        bool IsFailureStatus( string status );

        /// <summary>
        /// Determines whether the specified status is pending.
        /// </summary>
        /// <param name="status">The status.</param>
        /// <returns>
        ///   <c>true</c> if the specified status is pending; otherwise, <c>false</c>.
        /// </returns>
        bool IsPendingStatus( string status );
    }
}
