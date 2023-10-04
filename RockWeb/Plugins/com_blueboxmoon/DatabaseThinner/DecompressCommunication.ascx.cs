using System;
using System.ComponentModel;
using System.Web;

using com.blueboxmoon.DatabaseThinner;
using Rock;
using Rock.Model;
using Rock.Web;
using Rock.Web.UI;

namespace RockWeb.Plugins.com_blueboxmoon.DatabaseThinner
{
    [DisplayName( "Decompress Communication" )]
    [Category( "Blue Box Moon > Database Thinner" )]
    [Description( "Decompresses a communication so it can be viewed normally." )]
    public partial class DecompressCommunication : RockBlock
    {
        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Load" /> event.
        /// </summary>
        /// <param name="e">The <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnLoad( EventArgs e )
        {
            base.OnLoad( e );

            if ( !IsPostBack )
            {
                int? communicationId = PageParameter( "CommunicationId" ).AsIntegerOrNull();

                if ( communicationId.HasValue )
                {
                    DecompressCommunicationId( communicationId.Value );
                }
            }
        }

        /// <summary>
        /// Decompresses the communication identifier.
        /// </summary>
        /// <param name="communicationId">The communication identifier.</param>
        protected void DecompressCommunicationId( int communicationId )
        {
            var data = Helper.DecompressCommunication( communicationId );

            var uri = new Uri( ResolveRockUrlIncludeRoot( string.Format( "~/Communication/{0}", communicationId ) ) );
            var pageReference = new PageReference( uri, HttpContext.Current.Request.ApplicationPath );

            NavigateToPage( pageReference );
        }
    }
}