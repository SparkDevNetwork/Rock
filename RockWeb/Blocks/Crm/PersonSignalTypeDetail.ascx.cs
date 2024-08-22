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
using System.Linq;
using System.Web.UI;

using Rock;
using Rock.Constants;
using Rock.Data;
using Rock.Model;
using Rock.Web.UI;
using Rock.Security;
using Rock.Tasks;

namespace RockWeb.Blocks.Crm
{
    [DisplayName( "Person Signal Type Detail" )]
    [Category( "CRM" )]
    [Description( "Shows the details of a particular person signal type." )]

    [Rock.SystemGuid.BlockTypeGuid( "E9AB79D9-429F-410D-B4A8-327829FC7C63" )]
    public partial class PersonSignalTypeDetail : RockBlock
    {
        #region Properties

        /// <summary>
        /// The identifier of the signal type that we are currently editing.
        /// </summary>
        public int SignalTypeId
        {
            get { return ViewState["SignalTypeId"] as int? ?? 0; }
            set { ViewState["SignalTypeId"] = value; }
        }

        #endregion

        #region Base Control Methods

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Load" /> event.
        /// </summary>
        /// <param name="e">The <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnLoad( EventArgs e )
        {
            if ( !Page.IsPostBack )
            {
                lActionTitle.Text = ActionTitle.Add( SignalType.FriendlyTypeName ).FormatAsHtmlTitle();

                SignalTypeId = PageParameter( "SignalTypeId" ).AsInteger();
                ShowDetail();
            }

            base.OnLoad( e );
        }

        #endregion

        #region Core Methods

        /// <summary>
        /// Shows all the edit details for this signal type.
        /// </summary>
        private void ShowDetail()
        {
            var rockContext = new RockContext();
            var signalType = new SignalTypeService( rockContext ).Get( SignalTypeId );

            if ( signalType == null )
            {
                signalType = new SignalType();
                pdAuditDetails.Visible = false;
                lActionTitle.Text = ActionTitle.Add( SignalType.FriendlyTypeName ).FormatAsHtmlTitle();
            }
            else
            {
                lActionTitle.Text = ActionTitle.Edit( signalType.Name ).FormatAsHtmlTitle();
            }

            //
            // Switch everything to read-only if user not allowed to administrate.
            //
            if ( !signalType.IsAuthorized( Authorization.ADMINISTRATE, CurrentPerson ) || !IsUserAuthorized( Authorization.EDIT ) )
            {
                tbName.Enabled = false;
                tbDescription.Enabled = false;
                cpColor.Enabled = false;
                tbIconCssClass.Enabled = false;

                nbEditModeMessage.Text = EditModeMessage.NotAuthorizedToEdit( SignalType.FriendlyTypeName );
            }

            tbName.Text = signalType.Name;
            tbDescription.Text = signalType.Description;
            cpColor.Text = signalType.SignalColor;
            tbIconCssClass.Text = signalType.SignalIconCssClass;

            pdAuditDetails.SetEntity( signalType, ResolveRockUrl( "~" ) );
        }

        #endregion

        #region Events

        /// <summary>
        /// Handles the Click event of the btnSave control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void btnSave_Click( object sender, EventArgs e )
        {
            SignalType signalType = null;
            var rockContext = new RockContext();
            var signalTypeService = new SignalTypeService( rockContext );

            if ( SignalTypeId != 0 )
            {
                signalType = signalTypeService.Get( SignalTypeId );
            }

            if ( signalType == null )
            {
                signalType = new SignalType();
                signalTypeService.Add( signalType );
            }

            signalType.Name = tbName.Text;
            signalType.Description = tbDescription.Text;
            signalType.SignalColor = cpColor.Text;
            signalType.SignalIconCssClass = tbIconCssClass.Text;

            if ( !Page.IsValid || !signalType.IsValid )
            {
                return;
            }

            rockContext.SaveChanges();

            var people = new PersonSignalService( rockContext ).Queryable()
                .Where( s => s.SignalTypeId == signalType.Id )
                .Select( s => s.PersonId )
                .Distinct()
                .ToList();

            //
            // If less than 250 people with this signal type then just update them all now,
            // otherwise put something in the rock queue to take care of it.
            //
            if ( people.Count < 250 )
            {
                new PersonService( rockContext ).Queryable()
                    .Where( p => people.Contains( p.Id ) )
                    .ToList()
                    .ForEach( p => p.CalculateSignals() );

                rockContext.SaveChanges();
            }
            else
            {
                var updatePersonSignalTypesMsg = new UpdatePersonSignalTypes.Message()
                {
                    PersonIds = people
                };

                updatePersonSignalTypesMsg.Send();
            }

            NavigateToParentPage();
        }

        /// <summary>
        /// Handles the Click event of the btnCancel control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void btnCancel_Click( object sender, EventArgs e )
        {
            NavigateToParentPage();
        }

        #endregion
    }
}