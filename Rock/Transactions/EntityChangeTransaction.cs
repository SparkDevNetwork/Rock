using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.IO;

using Rock.Cms;

namespace Rock.Transactions
{
    /// <summary>
    /// Writes any entity chnages that are configured to be tracked
    /// </summary>
    public class EntityChangeTransaction : ITransaction
    {
        /// <summary>
        /// Gets or sets the changes.
        /// </summary>
        /// <value>
        /// The changes.
        /// </value>
        public List<Rock.Core.EntityChange> Changes { get; set; }

        /// <summary>
        /// Gets or sets the person id.
        /// </summary>
        /// <value>
        /// The person id.
        /// </value>
        public int? PersonId { get; set; }

        /// <summary>
        /// Execute method to write transaction to the database.
        /// </summary>
        public void Execute()
        {
            if ( Changes != null && Changes.Count > 0 )
            {
                Core.EntityChangeService entityChangeService = new Core.EntityChangeService();

                foreach ( var entityChange in Changes )
                {
                    entityChangeService.Add( entityChange, PersonId );
                    entityChangeService.Save( entityChange, PersonId );
                }
            }
        }
    }
}