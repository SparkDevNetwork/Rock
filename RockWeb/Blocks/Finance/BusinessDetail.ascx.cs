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
using System.Web.UI.WebControls;
using Rock.Constants;
using Rock.Model;
using Rock.Security;
using Rock.Web.UI;

namespace RockWeb.Blocks.Finance
{
    [DisplayName( "Business Detail" )]
    [Category( "Finance" )]
    [Description( "Displays the details of the given business." )]
    public partial class BusinessDetail : Rock.Web.UI.RockBlock, IDetailBlock
    {
        #region Control Methods

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Load" /> event.
        /// </summary>
        /// <param name="e">The <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnLoad( EventArgs e )
        {
            base.OnLoad( e );

            if ( !Page.IsPostBack )
            {
                string itemId = PageParameter( "businessId" );
                if ( !string.IsNullOrWhiteSpace( itemId ) )
                {
                    ShowDetail( "businessId", int.Parse( itemId ) );
                }
                else
                {
                    pnlDetails.Visible = false;
                }
            }
        }

        #endregion

        #region Events

        protected void lbSave_Click( object sender, EventArgs e )
        {

        }

        protected void lbCancel_Click( object sender, EventArgs e )
        {
            if ( !string.IsNullOrWhiteSpace( PhoneNumber.CleanNumber( tbPhone.Text ) ) )
            {
                var phoneNumber = new PhoneNumber();
                phoneNumber.Number = PhoneNumber.CleanNumber( tbPhone.Text );
            }
        }

        protected void lbEdit_Click( object sender, EventArgs e )
        {

        }

        #endregion

        #region Internal Methods

        /// <summary>
        /// Shows the detail.
        /// </summary>
        /// <param name="itemKey">The item key.</param>
        /// <param name="itemKeyValue">The item key value.</param>
        public void ShowDetail( string itemKey, int itemKeyValue )
        {
            pnlDetails.Visible = false;

            if ( !itemKey.Equals( "businessId" ) )
            {
                return;
            }

            bool editAllowed = true;

            Person business = null;     // A business is a person

            if ( !itemKeyValue.Equals( 0 ) )
            {
                business = new PersonService().Get( itemKeyValue );
                editAllowed = business.IsAuthorized( Authorization.EDIT, CurrentPerson );
            }
            else
            {
                business = new Person { Id = 0 };
            }

            if ( business == null )
            {
                return;
            }

            pnlDetails.Visible = true;
            hfBusinessId.Value = business.Id.ToString();

            bool readOnly = false;

            nbEditModeMessage.Text = string.Empty;
            if ( !editAllowed || !IsUserAuthorized( Authorization.EDIT ) )
            {
                readOnly = true;
                nbEditModeMessage.Text = EditModeMessage.ReadOnlyEditActionNotAllowed( Person.FriendlyTypeName );
            }

            if ( readOnly )
            {
                ShowReadonlyDetails( business );
            }
            else
            {
                lbEdit.Visible = true;
                if ( business.Id > 0 )
                {
                    ShowReadonlyDetails( business );
                }
                else
                {
                    ShowEditDetails( business );
                }
            }
        }

        private void ShowReadonlyDetails( Person business )
        {
            SetEditMode( false );

            //hfAccountId.SetValue( account.Id );
            //lActionTitle.Text = account.Name.FormatAsHtmlTitle();
            //hlInactive.Visible = !account.IsActive;
            //lAccountDescription.Text = account.Description;

            //DescriptionList descriptionList = new DescriptionList();
            //descriptionList.Add( "", "" );
            //lblMainDetails.Text = descriptionList.Html;
        }

        private void SetEditMode( bool editable )
        {
            pnlEditDetails.Visible = editable;
            fieldsetViewSummary.Visible = !editable;
            this.HideSecondaryBlocks( editable );
        }

        #endregion
}
}