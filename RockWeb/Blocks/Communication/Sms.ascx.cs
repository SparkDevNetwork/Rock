//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//
using Rock.Communication;

namespace RockWeb.Blocks.Communication
{
    public partial class Email : CommunicationChannelControl
    {
        public override void SetControlProperties( Rock.Model.Communication communication )
        {
            tbFromPhone.Text = communication.GetChannelDataValue( "FromPhone" );
            tbMessage.Text = communication.GetChannelDataValue( "Message" );
        }

        public override void GetControlProperties( Rock.Model.Communication communication )
        {
            communication.SetChannelDataValue( "FromPhone", tbFromPhone.Text );
            communication.SetChannelDataValue( "Message", tbMessage.Text );
        }
    }
}