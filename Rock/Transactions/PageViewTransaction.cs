using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.IO;

namespace Rock.Transactions
    
    /// <summary>
    /// Tracks when a page is viewed.
    /// </summary>
    public class PageViewTransaction : ITransaction
        

        /// <summary>
        /// Gets or sets the Page Id.
        /// </summary>
        /// <value>
        /// Page Id.
        /// </value>
        public int PageId      get; set; }

        /// <summary>
        /// Gets or sets the Site Id.
        /// </summary>
        /// <value>
        /// Site Id.
        /// </value>
        public int SiteId      get; set; }

        /// <summary>
        /// Gets or sets the Person Id.
        /// </summary>
        /// <value>
        /// Person Id.
        /// </value>
        public int PersonId      get; set; }

        /// <summary>
        /// Gets or sets the DateTime the page was viewed.
        /// </summary>
        /// <value>
        /// Date Viewed.
        /// </value>
        public DateTime DateViewed      get; set; }

        /// <summary>
        /// Gets or sets the IP address that requested the page.
        /// </summary>
        /// <value>
        /// IP Address.
        /// </value>
        public string IPAddress      get; set; }

        /// <summary>
        /// Gets or sets the browser vendor and version.
        /// </summary>
        /// <value>
        /// IP Address.
        /// </value>
        public string UserAgent      get; set; }
        
        
        /// <summary>
        /// Execute method to write transaction to the database.
        /// </summary>
        public void Execute()
            
            string directory = AppDomain.CurrentDomain.BaseDirectory;
            directory = Path.Combine( directory, "Logs" );

            // check that directory exists
            if ( !Directory.Exists( directory ) )
                Directory.CreateDirectory( directory );

            // create full path to the fie
            string filePath = Path.Combine( directory, "pageviews.csv" );
            
            // write to the file
            StreamWriter w = new StreamWriter( filePath, true );
            w.Write( "    0},    1},    2},    3},    4},    5}\r\n", DateViewed.ToString(),  PageId.ToString(), SiteId.ToString(), PersonId.ToString(), IPAddress, UserAgent);
            w.Close();
        }
    }
}