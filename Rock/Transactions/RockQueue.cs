//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//
using System.Collections.Generic;

namespace Rock.Transactions
{
    /// <summary>
    /// 
    /// </summary>
    static public class RockQueue
    {
        /// <summary>
        /// Gets or sets the transaction queue.
        /// </summary>
        /// <value>
        /// The transaction queue.
        /// </value>
        public static Queue<ITransaction> TransactionQueue { get; set; }
        
        /// <summary>
        /// Initializes the <see cref="RockQueue" /> class.
        /// </summary>
        static RockQueue()
        {
            TransactionQueue = new Queue<ITransaction>();
        }
    }
}