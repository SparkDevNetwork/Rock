using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.IO;

namespace Rock.Transactions
{
    /// <summary>
    /// Tracks when a page is viewed.
    /// </summary>
    public class PageViewTransaction : ITransaction
    {

        /// <summary>
        /// Gets or sets the Page Id.
        /// </summary>
        /// <value>
        /// Page Id.
        /// </value>
        public int PageId { get; set; }

        /// <summary>
        /// Gets or sets the Person Id.
        /// </summary>
        /// <value>
        /// Person Id.
        /// </value>
        public int PersonId { get; set; }

        /// <summary>
        /// Gets or sets the DateTime the page was viewed.
        /// </summary>
        /// <value>
        /// Date Viewed.
        /// </value>
        public DateTime DateViewed { get; set; }

        /// <summary>
        /// Gets or sets the IP address that requested the page.
        /// </summary>
        /// <value>
        /// IP Address.
        /// </value>
        public string IPAddress { get; set; }

        /// <summary>
        /// Gets or sets the browser vendor and version.
        /// </summary>
        /// <value>
        /// IP Address.
        /// </value>
        public string UserAgent { get; set; }
        
        
        /// <summary>
        /// Execute method to write transaction to the database.
        /// </summary>
        public void Execute()
        {
            using ( StreamWriter w = File.AppendText( "pageviews.csv" ) )
            {
                w.Write( "{0},{1},{2},{3},{4}\r\n", DateViewed.ToString(),  PageId.ToString(), PersonId.ToString(), IPAddress, UserAgent);
                w.Close();
            }
        }
    }
}