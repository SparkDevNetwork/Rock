//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.UI.WebControls;

using Rock;
using Rock.Communication;
using Rock.Model;

namespace RockWeb.Blocks.Communication
{
    /// <summary>
    /// UI for Email channel communications
    /// </summary>
    public partial class Email : CommunicationChannelControl
    {
        /// <summary>
        /// Handles the FileUploaded event of the fuAttachments control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs" /> instance containing the event data.</param>
        protected void fuAttachments_FileUploaded( object sender, System.EventArgs e )
        {
            var attachmentList = hfAttachments.Value.SplitDelimitedValues().ToList();
            attachmentList.Add(fuAttachments.BinaryFileId.ToString());
            hfAttachments.Value = attachmentList.AsDelimited(",");

            fuAttachments.BinaryFileId = 0;

            BindAttachments();
        }

        /// <summary>
        /// Handles the ItemCommand event of the rptAttachments control.
        /// </summary>
        /// <param name="source">The source of the event.</param>
        /// <param name="e">The <see cref="System.Web.UI.WebControls.RepeaterCommandEventArgs" /> instance containing the event data.</param>
        protected void rptAttachments_ItemCommand(object source, System.Web.UI.WebControls.RepeaterCommandEventArgs e)
        {
            if ( e.Item.ItemType == ListItemType.Item )
            {
                if (e.Item.DataItem is KeyValuePair<int, string>)
                {
                    var attachment = (KeyValuePair<int, string>)e.Item.DataItem;

                    hfAttachments.Value = hfAttachments.Value.SplitDelimitedValues()
                        .ToList()
                        .Where( i => i != attachment.Key.ToString())
                        .Select( i => i.ToString() )
                        .ToList()
                        .AsDelimited(",");

                    BindAttachments();
                }
            }
        }

        /// <summary>
        /// Sets the control properties.
        /// </summary>
        /// <param name="communication">The communication.</param>
        public override void SetControlProperties( Rock.Model.Communication communication )
        {
            tbFromName.Text = communication.GetChannelDataValue("FromName");
            tbFromAddress.Text = communication.GetChannelDataValue("FromAddress");
            tbReplyToAddress.Text = communication.GetChannelDataValue("ReplyTo");
            tbSubject.Text = communication.Subject;
            htmlMessage.Text = communication.GetChannelDataValue("HtmlMessage");
            tbTextMessage.Text = communication.GetChannelDataValue("TextMessage");
            hfAttachments.Value = communication.GetChannelDataValue("Attachments");

            BindAttachments();

            fuAttachments.FileUploaded += fuAttachments_FileUploaded;
        }

        /// <summary>
        /// Gets the control properties.
        /// </summary>
        /// <param name="communication">The communication.</param>
        public override void GetControlProperties( Rock.Model.Communication communication )
        {
            communication.SetChannelDataValue("FromName", tbFromName.Text);
            communication.SetChannelDataValue("FromAddress", tbFromAddress.Text);
            communication.SetChannelDataValue("ReplyTo", tbReplyToAddress.Text);
            communication.Subject = tbSubject.Text;
            communication.SetChannelDataValue("HtmlMessage", htmlMessage.Text);
            communication.SetChannelDataValue("TextMessage", tbTextMessage.Text);
            communication.SetChannelDataValue("Attachments", hfAttachments.Value);

            if (!string.IsNullOrWhiteSpace(hfAttachments.Value))
            {
                BinaryFile.MakePermanent(hfAttachments.Value);
            }
        }

        /// <summary>
        /// Binds the attachments.
        /// </summary>
        private void BindAttachments()
        {
            var attachments = new Dictionary<int, string>();

            var service = new BinaryFileService();
            foreach(string FileId in hfAttachments.Value.SplitDelimitedValues())
            {
                int binaryFileId = int.MinValue;
                if (int.TryParse(FileId, out binaryFileId))
                {
                    string fileName = service.Queryable()
                        .Where( f => f.Id == binaryFileId)
                        .Select( f => f.FileName)
                        .FirstOrDefault();

                    if (fileName != null)
                    {
                        attachments.Add(binaryFileId, fileName);
                    }
                }
            }

            rptAttachments.DataSource = attachments;
            rptAttachments.DataBind();
        }
    }
}