using Rock.Transactions;

namespace Rock
{
    public static partial class ExtensionMethods
    {
        /// <summary>
        /// Adds the ITransaction to the Rock TransactionQueue
        /// </summary>
        /// <param name="transaction">The transaction.</param>
        public static void Enqueue( this ITransaction transaction )
        {
            RockQueue.TransactionQueue.Enqueue( transaction );
        }
    }
}
