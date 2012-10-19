//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//
using System;
using Rock.Crm;

namespace Rock.Transactions
{
    /// <summary>
    /// Tracks when a person is viewed.
    /// </summary>
    public class PersonViewTransaction : ITransaction
    {

        /// <summary>
        /// Gets or sets the viewer person id.
        /// </summary>
        /// <value>
        /// The viewer person id.
        /// </value>
        public int ViewerPersonId { get; set; }

        /// <summary>
        /// Gets or sets the target person id.
        /// </summary>
        /// <value>
        /// The target person id.
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
        public DateTimeOffset DateViewed { get; set; }
        
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

                    PersonViewedService pvService = new PersonViewedService();

                    PersonViewed pvRecord = new PersonViewed();
                    pvService.Add( pvRecord, null );

                    pvRecord.IpAddress = IPAddress;
                    pvRecord.TargetPersonId = TargetPersonId;
                    pvRecord.ViewerPersonId = ViewerPersonId;
                    pvRecord.ViewDateTime = DateViewed;
                    pvRecord.Source = Source;

                    pvService.Save( pvRecord, null );
                }
            }
        }
    }
}