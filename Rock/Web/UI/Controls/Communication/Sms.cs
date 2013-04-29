//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.UI;
using System.Web.UI.WebControls;

using Rock.Field;
using Rock.Model;
using Rock.Web.Cache;

namespace Rock.Web.UI.Controls.Communication
{
    /// <summary>
    /// SMS Communication Channel control
    /// </summary>
    public class Sms : ChannelControl
    {
        #region UI Controls

        private LabeledTextBox tbFromPhone;
        private LabeledTextBox tbTextMessage;

        #endregion

        #region Constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="Email" /> class.
        /// </summary>
        public Sms()
        {
            tbFromPhone = new LabeledTextBox();
            tbTextMessage = new LabeledTextBox();
        }

        #endregion

        #region Properties

        public override Dictionary<string, string> ChannelData
        {
            get
            {
                var data = new Dictionary<string, string>();
                data.Add( "FromPhone", tbFromPhone.Text );
                data.Add( "Subject", tbTextMessage.Text );
                return data;
            }

            set
            {
                tbFromPhone.Text = GetDataValue( value, "FromPhone" );
                tbTextMessage.Text = GetDataValue( value, "Subject" );
            }
        }

        #endregion

        #region CompositeControl Methods

        /// <summary>
        /// Called by the ASP.NET page framework to notify server controls that use composition-based implementation to create any child controls they contain in preparation for posting back or rendering.
        /// </summary>
        protected override void CreateChildControls()
        {
            base.CreateChildControls();
            Controls.Clear();

            tbFromPhone.ID = string.Format( "tbFromName_{0}", this.ID );
            tbFromPhone.LabelText = "From Phone Number";
            Controls.Add( tbFromPhone );

            tbTextMessage.ID = string.Format( "tbTextMessage_{0}", this.ID );
            tbTextMessage.LabelText = "Message";
            tbTextMessage.TextMode = TextBoxMode.MultiLine;
            tbTextMessage.Rows = 3;
            Controls.Add( tbTextMessage );
        }

        /// <summary>
        /// Outputs server control content to a provided <see cref="T:System.Web.UI.HtmlTextWriter" /> object and stores tracing information about the control if tracing is enabled.
        /// </summary>
        /// <param name="writer">The <see cref="T:System.Web.UI.HtmlTextWriter" /> object that receives the control content.</param>
        public override void RenderControl( HtmlTextWriter writer )
        {
            tbFromPhone.RenderControl( writer );
            tbTextMessage.RenderControl( writer );
        }

        #endregion

    }

}
