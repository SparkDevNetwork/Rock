using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Rock.Transactions
{
    static public class RockQueue
    {

        /// <summary>
        /// Gets or sets the Transaction Queue.
        /// </summary>
        /// <value>
        /// Page Id.
        /// </value>
        public static Queue<ITransaction> TransactionQueue { get; set; }


        static RockQueue()
        {
            TransactionQueue = new Queue<ITransaction>();
        }
    }
}