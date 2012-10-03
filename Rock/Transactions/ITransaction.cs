//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//

namespace Rock.Transactions
{
    /// <summary>
    /// Represents a Transaction class
    /// </summary>
    public interface ITransaction
    {
        /// <summary>
        /// Executes this instance.
        /// </summary>
        void Execute();
    }

    
}
