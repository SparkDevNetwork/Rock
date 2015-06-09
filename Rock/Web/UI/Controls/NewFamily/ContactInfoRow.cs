// <copyright>
// Copyright 2013 by the Spark Development Network
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
// </copyright>
//
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;

using Rock.Model;
using Rock.Web.Cache;

namespace Rock.Web.UI.Controls
{
    /// <summary>
    /// Displays a bootstrap badge
    /// </summary>
    public class NewFamilyContactInfoRow : CompositeControl
    {
        /// <summary>
        /// The Family role key
        /// </summary>
        /// 
        private PhoneNumberBox _pnbHomePhone;
        private PhoneNumberBox _pnbCellPhone;
        private RockCheckBox _cbIsMessagingEnabled;
        private EmailBox _ebEmail;

        /// <summary>
        /// Gets or sets the person GUID.
        /// </summary>
        /// <value>
        /// The person GUID.
        /// </value>
        public Guid? PersonGuid
        {
            get
            {
                if ( ViewState["PersonGuid"] != null )
                {
                    return (Guid)ViewState["PersonGuid"];
                }
                else
                {
                    return Guid.Empty;
                }
            }
            set { ViewState["PersonGuid"] = value; }
        }

        /// <summary>
        /// Gets or sets the name of the person.
        /// </summary>
        /// <value>
        /// The name of the person.
        /// </value>
        public string PersonName
        {
            get { return ViewState["PersonName"] as string ?? string.Empty; }
            set { ViewState["PersonName"] = value; }
        }

        /// <summary>
        /// Gets or sets the home phone country code.
        /// </summary>
        /// <value>
        /// The home phone country code.
        /// </value>
        public string HomePhoneCountryCode
        {
            get 
            {
                EnsureChildControls();
                return _pnbHomePhone.CountryCode; 
            }
            set 
            {
                EnsureChildControls();
                _pnbHomePhone.CountryCode = value; 
            }
        }

        /// <summary>
        /// Gets or sets the home phone number.
        /// </summary>
        /// <value>
        /// The home phone number.
        /// </value>
        public string HomePhoneNumber
        {
            get 
            {
                EnsureChildControls();
                return _pnbHomePhone.Number; 
            }
            set 
            {
                EnsureChildControls();
                _pnbHomePhone.Number = value; 
            }
        }

        /// <summary>
        /// Gets or sets the cell phone country code.
        /// </summary>
        /// <value>
        /// The cell phone country code.
        /// </value>
        public string CellPhoneCountryCode
        {
            get 
            {
                EnsureChildControls();
                return _pnbCellPhone.CountryCode; 
            }
            set 
            { 
                _pnbCellPhone.CountryCode = value;
                EnsureChildControls();
            }
        }

        /// <summary>
        /// Gets or sets the cell phone number.
        /// </summary>
        /// <value>
        /// The cell phone number.
        /// </value>
        public string CellPhoneNumber
        {
            get 
            {
                EnsureChildControls();
                return _pnbCellPhone.Number; 
            }
            set 
            {
                EnsureChildControls();
                _pnbCellPhone.Number = value; 
            }
        }

        /// <summary>
        /// Gets or sets the email.
        /// </summary>
        /// <value>
        /// The email.
        /// </value>
        public string Email
        {
            get 
            {
                EnsureChildControls();
                return _ebEmail.Text; 
            }
            set 
            {
                EnsureChildControls();
                _ebEmail.Text = value; 
            }
        }

        /// <summary>
        /// Gets or sets the Is Messaging Enabled bool.
        /// </summary>
        /// <value>
        /// True/False.
        /// </value>
        public bool IsMessagingEnabled
        {
            get
            {
                EnsureChildControls();
                return _cbIsMessagingEnabled.Checked;
            }
            set
            {
                EnsureChildControls();
                _cbIsMessagingEnabled.Checked = value;
            }
        }

        /// <summary>
        /// Gets or sets the validation group.
        /// </summary>
        /// <value>
        /// The validation group.
        /// </value>
        public string ValidationGroup
        {
            get
            {
                EnsureChildControls();
                return _pnbHomePhone.ValidationGroup;
            }
            set
            {
                EnsureChildControls();
                _pnbHomePhone.ValidationGroup = value;
                _pnbCellPhone.ValidationGroup = value;
                _cbIsMessagingEnabled.ValidationGroup = value;
                _ebEmail.ValidationGroup = value;
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="NewFamilyContactInfoRow" /> class.
        /// </summary>
        public NewFamilyContactInfoRow()
            : base()
        {
            _pnbHomePhone = new PhoneNumberBox();
            _pnbCellPhone = new PhoneNumberBox();
            _cbIsMessagingEnabled = new RockCheckBox();
            _ebEmail = new EmailBox();
        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );

            string script = @"
    $('input[id$=""_pnbHomePhone""]').on('change', function(e) {
        if ($(this).val() != '') {
            var masterId = $(this).prop('id');
            var masterVal = $(this).val();
            $('input[id$=""_pnbHomePhone""]').each( function(e) {
                if ($(this).prop('id') != masterId && $(this).val() == '') {
                    $(this).val(masterVal);
                    phoneNumberBoxFormatNumber($(this));
                }
            });
        }
    });
";
            ScriptManager.RegisterStartupScript( this, this.GetType(), "home-phone", script, true );

        }
        /// <summary>
        /// Called by the ASP.NET page framework to notify server controls that use composition-based implementation to create any child controls they contain in preparation for posting back or rendering.
        /// </summary>
        protected override void CreateChildControls()
        {
            base.CreateChildControls();
            Controls.Clear();

            _pnbHomePhone.ID = "_pnbHomePhone";
            _pnbCellPhone.ID = "_pnbCellPhone";
            _cbIsMessagingEnabled.ID = "_cbIsMessagingEnabled";
            _ebEmail.ID = "_ebEmail";
            

            Controls.Add( _pnbHomePhone );
            Controls.Add( _pnbCellPhone );
            Controls.Add(_cbIsMessagingEnabled);
            Controls.Add(_ebEmail);

            var homePhone = DefinedValueCache.Read( Rock.SystemGuid.DefinedValue.PERSON_PHONE_TYPE_HOME );
            _pnbHomePhone.Placeholder = homePhone != null ? homePhone.Value.EndsWith("Phone") ? homePhone.Value : homePhone.Value + " Phone" : "Home Phone";
            _pnbHomePhone.Required = false;

            var cellPhone = DefinedValueCache.Read( Rock.SystemGuid.DefinedValue.PERSON_PHONE_TYPE_MOBILE );
            _pnbCellPhone.Placeholder = cellPhone != null ? cellPhone.Value.EndsWith( "Phone" ) ? cellPhone.Value : cellPhone.Value + " Phone" : "Cell Phone";
            _pnbCellPhone.Required = false;

            _cbIsMessagingEnabled.Checked = true;
           
            _ebEmail.Placeholder = "Email";
            _ebEmail.Required = false;
        }

        /// <summary>
        /// Outputs server control content to a provided <see cref="T:System.Web.UI.HtmlTextWriter" /> object and stores tracing information about the control if tracing is enabled.
        /// </summary>
        /// <param name="writer">The <see cref="T:System.Web.UI.HtmlTextWriter" /> object that receives the control content.</param>
        public override void RenderControl( HtmlTextWriter writer )
        {
            if ( this.Visible )
            {
                writer.AddAttribute( "rowid", ID );
                writer.RenderBeginTag( HtmlTextWriterTag.Tr );

                writer.RenderBeginTag( HtmlTextWriterTag.Td );
                writer.Write( PersonName );
                writer.RenderEndTag();

                writer.RenderBeginTag( HtmlTextWriterTag.Td );
                writer.AddAttribute( "class", "form-group" );
                writer.RenderBeginTag( HtmlTextWriterTag.Div );
                _pnbHomePhone.RenderControl( writer );
                writer.RenderEndTag();
                writer.RenderEndTag();

                writer.RenderBeginTag( HtmlTextWriterTag.Td );
                writer.AddAttribute( "class", "form-group" );
                writer.RenderBeginTag( HtmlTextWriterTag.Div );
                _pnbCellPhone.RenderControl( writer );
                writer.RenderEndTag();
                writer.RenderEndTag();

                writer.RenderBeginTag(HtmlTextWriterTag.Td);
                writer.AddAttribute("class", "text-center");
                writer.RenderBeginTag(HtmlTextWriterTag.Div);
                _cbIsMessagingEnabled.RenderControl(writer);
                writer.RenderEndTag();
                writer.RenderEndTag();

                writer.RenderBeginTag( HtmlTextWriterTag.Td );
                writer.AddAttribute( "class", "form-group" );
                writer.RenderBeginTag( HtmlTextWriterTag.Div );
                _ebEmail.RenderControl( writer );
                writer.RenderEndTag();
                writer.RenderEndTag();

                writer.RenderEndTag();  // Tr
            }
        }
    }

}