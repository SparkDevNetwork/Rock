//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//

using Rock.Model;

namespace Rock.Transactions
{
    /// <summary>
    /// Writes entity audits 
    /// </summary>
    public class SendCommunicationTransaction : ITransaction
    {
        /// <summary>
        /// Gets or sets the communication id
        /// </summary>
        /// <value>
        /// The communication id.
        /// </value>
        public int CommunicationId { get; set; }

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
            var communication = new CommunicationService().Get( CommunicationId );

            if ( communication != null && communication.Status == CommunicationStatus.Approved )
            {
                var channel = communication.Channel;
                if ( channel != null )
                {
                    var transport = channel.Transport;
                    if ( transport != null )
                    {
                        transport.Send( communication, PersonId );
                    }
                }
            }
        }
    }
}