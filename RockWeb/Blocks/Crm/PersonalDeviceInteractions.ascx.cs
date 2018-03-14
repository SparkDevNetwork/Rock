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
using Rock.Attribute;

using Rock;
using Rock.Data;
using Rock.Model;
using Rock.Web.UI;
using Rock.Web.Cache;
using Rock.Web.UI.Controls;
using Rock.PersonProfile;
using Rock.Security;
using System.Data.Entity;

namespace RockWeb.Blocks.Crm
{
    [DisplayName( "Personal Device Interactions" )]
    [Category( "CRM" )]
    [Description( "Shows a list of all interactions for a personal device." )]

    public partial class PersonalDeviceInteractions : RockBlock
    {
        #region Fields

        private int? _personalDeviceId = null;

        #endregion

        #region Base Control Methods

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );

            gInteractions.DataKeyNames = new string[] { "Id" };
            gInteractions.GridRebind += gInteractions_GridRebind;

            _personalDeviceId = PageParameter( "personalDeviceId" ).AsIntegerOrNull();
        }


        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Load" /> event.
        /// </summary>
        /// <param name="e">The <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnLoad( EventArgs e )
        {
            if ( !Page.IsPostBack )
            {
                if ( _personalDeviceId.HasValue )
                {
                    hfPersonalDevice.SetValue( _personalDeviceId.Value );
                    BindGrid();
                }
                else
                {
                    nbError.Text =  "No Personal Device Found to show the interations";
                    nbError.Visible = true;
                    return;
                }
            }
            else
            {
                _personalDeviceId = hfPersonalDevice.Value.AsIntegerOrNull();
            }

            base.OnLoad( e );
        }

        #endregion

        #region Grid Events

        /// <summary>
        /// Handles the GridRebind event of the gInteractions control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        private void gInteractions_GridRebind( object sender, EventArgs e )
        {
            BindGrid();
        }

        #endregion

        #region Internal Methods

        /// <summary>
        /// Binds the grid.
        /// </summary>
        private void BindGrid()
        {
            if ( _personalDeviceId.HasValue )
            {
                using ( var rockContext = new RockContext() )
                {
                    var personalDevice = new PersonalDeviceService( rockContext )
                        .Queryable().AsNoTracking()
                        .Where( d => d.Id == _personalDeviceId.Value )
                        .FirstOrDefault();
                    if ( personalDevice.PersonAlias != null )
                    {
                        lblHeading.Text = string.Format( "{0} Device Interactions", personalDevice.PersonAlias.Person.FullName );
                    }
                    else
                    {
                        lblHeading.Text = "Personal Device Interactions";
                    }

                    var interactions = new InteractionService( rockContext )
                        .Queryable().AsNoTracking()
                        .Where( a => a.PersonalDeviceId == _personalDeviceId );

                    SortProperty sortProperty = gInteractions.SortProperty;
                    if ( sortProperty != null )
                    {
                        interactions = interactions.Sort( sortProperty );
                    }
                    else
                    {
                        interactions = interactions.OrderByDescending( p => p.InteractionDateTime );
                    }

                    gInteractions.SetLinqDataSource<Interaction>( interactions );
                    gInteractions.DataBind();
                }
            }
        }

        #endregion
    }
}