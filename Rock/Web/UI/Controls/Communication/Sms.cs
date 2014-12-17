﻿// <copyright>
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
    /// SMS Communication Medium control
    /// </summary>
    public class Sms : MediumControl
    {
        #region UI Controls

        private RockDropDownList ddlFrom;
        private RockControlWrapper rcwMessage;
        private MergeFieldPicker mfpMessage;
        private RockTextBox tbMessage;

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets the medium data.
        /// </summary>
        /// <value>
        /// The medium data.
        /// </value>
        public override Dictionary<string, string> MediumData
        {
            get
            {
                EnsureChildControls();
                var data = new Dictionary<string, string>();
                data.Add( "FromValue", ddlFrom.SelectedValue );
                data.Add( "Subject", ddlFrom.SelectedItem != null ? ( "From: " + ddlFrom.SelectedItem.Text ) : string.Empty );
                data.Add( "Message", tbMessage.Text );
                return data;
            }

            set
            {
                EnsureChildControls();
                ddlFrom.SelectedValue = GetDataValue( value, "FromValue" );
                tbMessage.Text = GetDataValue( value, "Message" );
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

            ddlFrom = new RockDropDownList();
            ddlFrom.ID = string.Format( "ddlFrom_{0}", this.ID );
            ddlFrom.Label = "From";
            ddlFrom.Help = "The number to originate message from (configured under Admin Tools > General Settings > Defined Types > SMS From Values).";
            ddlFrom.BindToDefinedType( DefinedTypeCache.Read( new Guid( Rock.SystemGuid.DefinedType.COMMUNICATION_SMS_FROM ) ), false, true );
            Controls.Add( ddlFrom );

            rcwMessage = new RockControlWrapper();
            rcwMessage.ID = string.Format( "rcwMessage_{0}", this.ID );
            rcwMessage.Label = "Message";
            rcwMessage.Help = "<span class='tip tip-lava'></span>";
            Controls.Add( rcwMessage );

            mfpMessage = new MergeFieldPicker();
            mfpMessage.ID = string.Format( "mfpMergeFields_{0}", this.ID );
            mfpMessage.MergeFields.Clear();
            mfpMessage.MergeFields.Add( "GlobalAttribute" );
            mfpMessage.MergeFields.Add( "Rock.Model.Person" );
            mfpMessage.CssClass += " pull-right margin-b-sm"; 
            mfpMessage.SelectItem += mfpMergeFields_SelectItem;
            rcwMessage.Controls.Add( mfpMessage );

            tbMessage = new RockTextBox();
            tbMessage.ID = string.Format( "tbTextMessage_{0}", this.ID );
            tbMessage.TextMode = TextBoxMode.MultiLine;
            tbMessage.Rows = 3;
            rcwMessage.Controls.Add( tbMessage );
        }

        /// <summary>
        /// Gets or sets the validation group.
        /// </summary>
        /// <value>
        /// The validation group.
        /// </value>
        public override string ValidationGroup
        {
            get
            {
                EnsureChildControls();
                return tbMessage.ValidationGroup;
            }
            set
            {
                EnsureChildControls();
                ddlFrom.ValidationGroup = value;
                mfpMessage.ValidationGroup = value;
                tbMessage.ValidationGroup = value;
            }
        }
        
        /// <summary>
        /// On new communicaiton, initializes controls from sender values
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <exception cref="System.NotImplementedException"></exception>
        public override void InitializeFromSender( Person sender )
        {
        }

        /// <summary>
        /// Outputs server control content to a provided <see cref="T:System.Web.UI.HtmlTextWriter" /> object and stores tracing information about the control if tracing is enabled.
        /// </summary>
        /// <param name="writer">The <see cref="T:System.Web.UI.HtmlTextWriter" /> object that receives the control content.</param>
        public override void RenderControl( HtmlTextWriter writer )
        {
            ddlFrom.RenderControl( writer );
            rcwMessage.RenderControl( writer );
        }

        #endregion

        #region Events

        void mfpMergeFields_SelectItem( object sender, EventArgs e )
        {
            EnsureChildControls();
            tbMessage.Text += mfpMessage.SelectedMergeField;
            mfpMessage.SetValue( string.Empty );
        }

        #endregion
    }

}
