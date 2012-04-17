using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.IO;

using Rock.CRM;

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
        public int TargetPersonId { get; set; }

        /// <summary>
        /// Gets or sets the IP address that requested the page.
        /// </summary>
        /// <value>
        /// IP Address.
        /// </value>
        public string IPAddress { get; set; }

        /// <summary>
        /// Gets or sets the source of the view (site id or application name)
        /// </summary>
        /// <value>
        /// Source.
        /// </value>
        public string Source { get; set; }

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
            // store the view to the database if the viewer is NOT the target (don't track looking at your own record)
            if ( ViewerPersonId != TargetPersonId )
            {
                using ( new Rock.Data.UnitOfWorkScope() )
                {

                    PersonViewedRepository pvRepository = new PersonViewedRepository();

                    PersonViewed pvRecord = new PersonViewed();
                    pvRepository.Add( pvRecord, null );

                    pvRecord.IpAddress = IPAddress;
                    pvRecord.TargetPersonId = TargetPersonId;
                    pvRecord.ViewerPersonId = ViewerPersonId;
                    pvRecord.ViewDateTime = DateViewed;
                    pvRecord.Source = Source;

                    pvRepository.Save( pvRecord, null );
                }
            }
        }
    }
}