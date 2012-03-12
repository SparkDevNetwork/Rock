using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.IO;

namespace Rock.Transactions
{
    /// <summary>
    /// Tracks when a person is viewed.
    /// </summary>
    public class PersonViewTransaction : ITransaction
    {

        /// <summary>
        /// Gets or sets the Person Id of the viewer.
        /// </summary>
        /// <value>
        /// Page Id.
        /// </value>
        public int ViewerPersonId { get; set; }

        /// <summary>
        /// Gets or sets the Person Id of the person who was viewed.
        /// </summary>
        /// <value>
        /// Site Id.
        /// </value>
        public int VieweePersonId { get; set; }

        

        /// <summary>
        /// Gets or sets the DateTime the person was viewed.
        /// </summary>
        /// <value>
        /// Date Viewed.
        /// </value>
        public DateTime DateViewed { get; set; }
        
        /// <summary>
        /// Execute method to write transaction to the database.
        /// </summary>
        public void Execute()
        {
            
        }
    }
}