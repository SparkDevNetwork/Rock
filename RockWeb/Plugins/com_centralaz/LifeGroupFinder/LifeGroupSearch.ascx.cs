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
using System.IO;
using System.Linq;
using System.Web.UI;
using System.Web.UI.WebControls;

using Rock;
using Rock.Data;
using Rock.Model;
using Rock.Web.Cache;
using Rock.Web.UI.Controls;
using Rock.Attribute;

namespace RockWeb.Plugins.com_centralaz.LifeGroupFinder
{
    /// <summary>
    /// Template block for developers to use to start a new block.
    /// </summary>
    [DisplayName( "Life Group Search" )]
    [Category( "com_centralaz > Groups" )]
    [Description( "Central custom group search block." )]
    [LinkedPage( "Life Group List Page", "The page to navigate to for group details.", false, "", "", 0 )]
    [LinkedPage( "Life Group Map Page", "The page to navigate to for group details.", false, "", "", 0 )]
    [LinkedPage( "Information Security Page", "The page to navigate to for group details.", false, "", "", 0 )]

    public partial class LifeGroupSearch : Rock.Web.UI.RockBlock
    {
        #region ViewState and Dynamic Controls
        public Dictionary<string, string> ParameterState
        {
            get
            {
                var parameterState = Session["ParameterState"] as Dictionary<string, string>;
                if ( parameterState == null )
                {
                    parameterState = new Dictionary<string, string>();

                    Session["ParameterState"] = parameterState;
                }
                return parameterState;
            }

            set
            {
                Session["ParameterState"] = value;
            }
        }

        public bool Expanded
        {
            get
            {
                EnsureChildControls();
                return _hfExpanded.Value.AsBooleanOrNull() ?? false;
            }

            set
            {
                EnsureChildControls();
                _hfExpanded.Value = value.ToString();
            }
        }

        #endregion

        #region Base Control Methods

        //  overrides of the base RockBlock methods (i.e. OnInit, OnLoad)

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );

            // this event gets fired after block settings are updated. it's nice to repaint the screen if these settings would alter it
            this.BlockUpdated += Block_BlockUpdated;
            this.AddConfigurationUpdateTrigger( upnlContent );
            string script = @"
// groupsearch-filter animation
$('.groupsearch-filter > header').click(function () {
    $(this).siblings('.panel-body').slideToggle();

    $expanded = $(this).children('input.filter-expanded');
    $expanded.val($expanded.val() == 'True' ? 'False' : 'True');

    $('i.groupsearch-filter-state', this).toggleClass('fa-chevron-right');
    $('i.groupsearch-filter-state', this).toggleClass('fa-chevron-down');
});

// fix so that the Remove button will fire its event, but not the parent event 
$('.groupsearch-filter a.btn-danger').click(function (event) {
    event.stopImmediatePropagation();
});

$('.groupsearch-filter > .panel-body').on('validation-error', function() {
    var $header = $(this).siblings('header');
    $(this).slideDown();

    $expanded = $header.children('input.filter-expanded');
    $expanded.val('True');

    $('i.groupsearch-filter-state', $header).removeClass('fa-chevron-right');
    $('i.groupsearch-filter-state', $header).addClass('fa-chevron-down');

    return false;
});

";

            ScriptManager.RegisterStartupScript( this.Page, this.Page.GetType(), "GroupSearchFilterScript", script, true );
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
                if ( CurrentPerson != null )
                {
                    pnlLogin.Visible = false;
                    lTitle.Text = String.Format( "Hello {0}, Looking for a Life Group?", CurrentPerson.NickName );
                    acAddress.SetValues( CurrentPerson.GetHomeLocation() );
                }
                LoadDropDowns();
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

        }

        protected void btnSearch_Click( object sender, EventArgs e )
        {
            ParameterState.AddOrReplace( "Children", cblChildren.SelectedValues.AsDelimited( ";" ) );
            ParameterState.AddOrReplace( "Days", cblDays.SelectedValues.AsDelimited( ";" ) );
            ParameterState.AddOrReplace( "Pets", cbPets.Checked.ToTrueFalse() );
            ParameterState.AddOrReplace( "Campus", ddlCampus.SelectedValue );
            ParameterState.AddOrReplace( "StreetAddress1", acAddress.Street1 );
            ParameterState.AddOrReplace( "StreetAddress2", acAddress.Street2 );
            ParameterState.AddOrReplace( "City", acAddress.City );
            ParameterState.AddOrReplace( "State", acAddress.State );
            ParameterState.AddOrReplace( "PostalCode", acAddress.PostalCode );
            ParameterState.AddOrReplace( "Country", acAddress.Country );
            NavigateToLinkedPage( "LifeGroupListPage" );
        }

        protected void lbLogin_Click( object sender, EventArgs e )
        {
            var site = RockPage.Layout.Site;
            if ( site.LoginPageId.HasValue )
            {
                site.RedirectToLoginPage( true );
            }
        }

        protected void lbMap_Click( object sender, EventArgs e )
        {
            NavigateToLinkedPage( "LifeGroupMapPage", "Campus", ddlCampus.SelectedValue.AsInteger() );
        }

        protected void lbSecurity_Click( object sender, EventArgs e )
        {
            NavigateToLinkedPage( "InformationSecurityPage" );
        }

        #endregion

        #region Private Methods
        private void LoadDropDowns()
        {
            if ( ParameterState.ContainsKey("Pets") )
            {
                cbPets.Checked = ParameterState["Pets"].AsBoolean();
            }
            cblDays.Items.Clear();
            foreach ( var dow in Enum.GetValues( typeof( DayOfWeek ) ).OfType<DayOfWeek>().ToList() )
            {
                cblDays.Items.Add( new ListItem( dow.ConvertToString().Substring( 0, 3 ), dow.ConvertToInt().ToString() ) );
            }
            if ( ParameterState.ContainsKey( "Days" ) )
            {
                cblDays.SetValues( ParameterState["Days"].Split( ';' ).ToList() );
            }
            cblChildren.Items.Clear();
            cblChildren.BindToDefinedType( DefinedTypeCache.Read( "512F355E-9441-4C47-BE47-7FFE19209496".AsGuid() ) );
            if ( ParameterState.ContainsKey( "Children" ) )
            {
                cblChildren.SetValues( ParameterState["Children"].Split( ';' ).ToList() );
            }
            ddlCampus.DataSource = CampusCache.All();
            ddlCampus.DataBind();
        }
        #endregion

    }
}