// <copyright>
// Copyright by the Spark Development Network
//
// Licensed under the Rock Community License (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.rockrms.com/license
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
// </copyright>
//
using System;
using System.ComponentModel;
using System.Web.UI;
using System.Linq;
using Rock;
using Rock.Attribute;
using Rock.Data;
using Rock.Model;
using Rock.Web;
using Rock.Web.Cache;
using System.Collections.Generic;

namespace com.bemaservices.MailChimp
{
    /// <summary>
    /// Template block for developers to use to start a new block.
    /// </summary>
    [DisplayName( "Mail Chimp Audience Detail" )]
    [Category( "BEMA Services > MailChimp" )]
    [Description( "A block for people to edit the details of a MailChimp audience." )]

    public partial class MailChimpAudienceDetail : Rock.Web.UI.RockBlock
    {
        #region Base Control Methods

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );

            var definedValueId = PageParameter( "ListId" ).AsInteger();
            hfDefinedValueId.Value = definedValueId.ToString();

            // this event gets fired after block settings are updated. it's nice to repaint the screen if these settings would alter it
            this.BlockUpdated += Block_BlockUpdated;
            this.AddConfigurationUpdateTrigger( upnlContent );
        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Load" /> event.
        /// </summary>
        /// <param name="e">The <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnLoad( EventArgs e )
        {
            base.OnLoad( e );


            if ( !Page.IsPostBack )
            {
                var listId = PageParameter( "ListId" ).AsInteger();
                if ( listId > 0 )
                {
                    ShowDetails( listId );
                }
                else
                {
                   // ShowDefinedValueEdit();
                }
            }
        }

        #endregion

        #region Events

        // handlers called by the controls on your block

        /// <summary>
        /// Handles the BlockUpdated event of the control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void Block_BlockUpdated( object sender, EventArgs e )
        {
            var listId = PageParameter( "ListId" ).AsInteger();
            if ( listId > 0 )
            {
                ShowDetails( listId );
            }
            else
            {
               // ShowDefinedValueEdit();
            }
        }

        #endregion

        #region Methods

        private void ShowDetails( int listId )
        {
            pnlEdit.Visible = false;
            pnlView.Visible = true;
            var definedValue = new DefinedValueService( new RockContext() ).Get( listId );
            if ( definedValue != null )
            {
                hfDefinedValueId.SetValue( definedValue.Id );

                Rock.Web.DescriptionList descriptionList = new DescriptionList()
                .Add( "List", definedValue.Value ?? string.Empty )
                .Add( "Description", definedValue.Description ?? string.Empty );

                lblMainDetails.Text = descriptionList.Html;

                definedValue.LoadAttributes();
                Rock.Attribute.Helper.AddDisplayControls( definedValue, phDisplayAttributes, null, false, false );
                this.HideSecondaryBlocks( false );

            }
            else
            {
               // ShowDefinedValueEdit();
            }
        }

        private void ShowDefinedValueEdit()
        {
            var valueId = hfDefinedValueId.ValueAsInt();
            var definedTypeCache = DefinedTypeCache.Get( com.bemaservices.MailChimp.SystemGuid.SystemDefinedTypes.MAIL_CHIMP_AUDIENCES );
            DefinedValue definedValue;

            if ( !valueId.Equals( 0 ) )
            {
                definedValue = new DefinedValueService( new RockContext() ).Get( valueId );
            }
            else
            {
                definedValue = new DefinedValue { Id = 0 };
                definedValue.DefinedTypeId = definedTypeCache.Id;
            }


            hfDefinedValueId.SetValue( definedValue.Id );
            tbValueName.Text = definedValue.Value;
            tbValueDescription.Text = definedValue.Description;

            avcEditAttributes.AddEditControls( definedValue );

            pnlEdit.Visible = true;
            pnlView.Visible = false;
            this.HideSecondaryBlocks( true );
        }

        #endregion

        protected void btnEdit_Click( object sender, EventArgs e )
        {
            //ShowDefinedValueEdit();
        }

        protected void btnSave_Click( object sender, EventArgs e )
        {
            DefinedValue definedValue;
            var rockContext = new RockContext();
            DefinedValueService definedValueService = new DefinedValueService( rockContext );
            var definedTypeCache = DefinedTypeCache.Get( com.bemaservices.MailChimp.SystemGuid.SystemDefinedTypes.MAIL_CHIMP_AUDIENCES );

            int definedValueId = hfDefinedValueId.ValueAsInt();

            if ( definedValueId.Equals( 0 ) )
            {
                definedValue = new DefinedValue { Id = 0 };
                definedValue.DefinedTypeId = definedTypeCache.Id;
                definedValue.IsSystem = false;

                var orders = definedValueService.Queryable()
                    .Where( d => d.DefinedTypeId == definedTypeCache.Id )
                    .Select( d => d.Order )
                    .ToList();

                definedValue.Order = orders.Any() ? orders.Max() + 1 : 0;
            }
            else
            {
                definedValue = definedValueService.Get( definedValueId );
            }

            definedValue.Value = tbValueName.Text;
            definedValue.Description = tbValueDescription.Text;
            avcEditAttributes.GetEditValues( definedValue );

            if ( !Page.IsValid )
            {
                return;
            }

            if ( !definedValue.IsValid )
            {
                // Controls will render the error messages                    
                return;
            }

            rockContext.WrapTransaction( () =>
            {
                if ( definedValue.Id.Equals( 0 ) )
                {
                    definedValueService.Add( definedValue );
                }

                rockContext.SaveChanges();

                definedValue.SaveAttributeValues( rockContext );

            } );

            hfDefinedValueId.Value = string.Empty;


            var qryParams = new Dictionary<string, string>();
            qryParams.Add( "ListId", definedValue.Id.ToString() );
            NavigateToCurrentPage( qryParams );
        }
    }
}