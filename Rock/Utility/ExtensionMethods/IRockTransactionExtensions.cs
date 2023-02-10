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
using Rock.Transactions;

namespace Rock
{
    public static partial class ExtensionMethods
    {
        /// <summary>
        /// Adds the <see cref="ITransaction"/> to the standard Rock transaction queue.
        /// </summary>
        /// <param name="transaction">The transaction to be added to the queue.</param>
        public static void Enqueue( this ITransaction transaction )
        {
            RockQueue.Enqueue( transaction );
        }

        /// <summary>
        /// Adds the <see cref="ITransaction"/> to the Rock transaction queue.
        /// </summary>
        /// <param name="transaction">The transaction to be added to the queue.</param>
        /// <param name="useFastQueue">
        ///     <para>
        ///         <c>true</c> if the fast queue should be used.
        ///     </para>
        ///     <para>
        ///         This queue drains every second so it should only be used with transactions
        ///         that execute extremely fast and need to execute quickly.
        ///     </para>
        /// </param>
        public static void Enqueue( this ITransaction transaction, bool useFastQueue )
        {
            RockQueue.Enqueue( transaction, useFastQueue );
        }
    }
}
