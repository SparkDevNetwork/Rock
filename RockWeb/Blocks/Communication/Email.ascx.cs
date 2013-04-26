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
            tbFromName.Text = communication.GetChannelDataValue("FromName");
            tbFromAddress.Text = communication.GetChannelDataValue("FromAddress");
            tbReplyToAddress.Text = communication.GetChannelDataValue("ReplyTo");
            tbSubject.Text = communication.Subject;
            htmlMessage.Text = communication.GetChannelDataValue("HtmlMessage");
            tbTextMessage.Text = communication.GetChannelDataValue("TextMessage");
        }

        public override void GetControlProperties( Rock.Model.Communication communication )
        {
            communication.SetChannelDataValue("FromName", tbFromName.Text);
            communication.SetChannelDataValue("FromAddress", tbFromAddress.Text);
            communication.SetChannelDataValue("ReplyTo", tbReplyToAddress.Text);
            communication.Subject = tbSubject.Text;
            communication.SetChannelDataValue("HtmlMessage", htmlMessage.Text);
            communication.SetChannelDataValue("TextMessage", tbTextMessage.Text);
        }
    }
}