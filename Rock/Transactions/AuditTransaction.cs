//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//
using System.Collections.Generic;

using Rock.Model;

namespace Rock.Transactions
{
    /// <summary>
    /// Writes entity audits 
    /// </summary>
    public class AuditTransaction : ITransaction
    {
        /// <summary>
        /// Gets or sets the audits.
        /// </summary>
        /// <value>
        /// The audits.
        /// </value>
        public List<Audit> Audits { get; set; }

        /// <summary>
        /// Execute method to write transaction to the database.
        /// </summary>
        public void Execute()
        {
            if ( Audits != null && Audits.Count > 0 )
            {
                var auditService = new AuditService();

                foreach ( var audit in Audits )
                {
                    auditService.Add( audit, audit.PersonId );
                    auditService.Save( audit, audit.PersonId );
                }
            }
        }
    }
}