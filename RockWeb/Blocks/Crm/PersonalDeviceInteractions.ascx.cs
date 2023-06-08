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
using Rock.Security;
using System.Data.Entity;

namespace RockWeb.Blocks.Crm
{
    [DisplayName( "Personal Device Interactions" )]
    [Category( "CRM" )]
    [Description( "Shows a list of all interactions for a personal device." )]

    [IntegerField(
        "Currently Present Interval",
        Key = AttributeKey.CurrentlyPresentInterval,
        Description = "The number of minutes to use to determine is someone is still present. For example if set to 5 the system will consider the device present if their interaction records end date/time is within the last 5 minutes.",
        IsRequired = true,
        DefaultIntegerValue = 5,
        Order = 0 )]

    [Rock.SystemGuid.BlockTypeGuid( "D6224911-2590-427F-9DCE-6D14E79806BA" )]
    public partial class PersonalDeviceInteractions : RockBlock
    {
        #region Attribute Keys
        private static class AttributeKey
        {
            public const string CurrentlyPresentInterval = "CurrentlyPresentInterval";
        }
        #endregion Attribute Keys

        #region Fields

        private int? _personalDeviceId = null;
        private int _currentlyPresentInterval;

        #endregion

        #region Filter's User Preference Setting Keys
        /// <summary>
        /// Constant like string-key-settings that are tied to user saved filter preferences.
        /// </summary>
        public static class FilterSetting
        {
            public static readonly string DateRange = "Date Range";
            public static readonly string ShowUnassignedDevices = "Show Unassigned Devices";
            public static readonly string PresentDevices = "Present Devices";
        }
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

            gfInteractions.ClearFilterClick += gfInteractions_ClearFilterClick;
            gfInteractions.ApplyFilterClick += gfInteractions_ApplyFilterClick;
            gfInteractions.DisplayFilterValue += gfInteractions_DisplayFilterValue;

            _personalDeviceId = PageParameter( "PersonalDeviceId" ).AsIntegerOrNull();
            if ( _personalDeviceId.HasValue )
            {
                gfInteractions.SetFilterPreference( FilterSetting.PresentDevices, string.Empty );
                gfInteractions.SetFilterPreference( FilterSetting.ShowUnassignedDevices, string.Empty );
                cbPresentDevices.Visible = false;
                cbShowUnassignedDevices.Visible = false;
            }
            
            _currentlyPresentInterval = GetAttributeValue( AttributeKey.CurrentlyPresentInterval ).AsInteger();

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
                }
                BindFilter();
                BindGrid();
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
        /// Handles the ClearFilterClick event of the gfInteractions control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void gfInteractions_ClearFilterClick( object sender, EventArgs e )
        {
            gfInteractions.DeleteFilterPreferences();
            BindFilter();
        }

        /// <summary>
        /// Handles the ApplyFilterClick event of the gfInteractions control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void gfInteractions_ApplyFilterClick( object sender, EventArgs e )
        {
            gfInteractions.SetFilterPreference( FilterSetting.DateRange, sdpDateRange.DelimitedValues );
            gfInteractions.SetFilterPreference( FilterSetting.ShowUnassignedDevices, cbShowUnassignedDevices.Checked.ToString() );
            gfInteractions.SetFilterPreference( FilterSetting.PresentDevices, cbPresentDevices.Checked.ToString() );

            BindGrid();
        }

        /// <summary>
        /// Handles displaying the stored filter values.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The e as DisplayFilterValueArgs (hint: e.Key and e.Value).</param>
        protected void gfInteractions_DisplayFilterValue( object sender, GridFilter.DisplayFilterValueArgs e )
        {
            switch ( e.Key )
            {
                case "Date Range":
                    e.Value = SlidingDateRangePicker.FormatDelimitedValues( e.Value );
                    break;
                case "Show Unassigned Devices":
                    var showUnassignedDevices = e.Value.AsBooleanOrNull();
                    if ( showUnassignedDevices.HasValue && showUnassignedDevices.Value )
                    {
                        e.Value = showUnassignedDevices.Value.ToYesNo();
                    }
                    else
                    {
                        e.Value = string.Empty;
                    }
                    break;
                case "Present Devices":
                    var presentDevices = e.Value.AsBooleanOrNull();
                    if ( presentDevices.HasValue && presentDevices.Value )
                    {
                        e.Value = presentDevices.Value.ToYesNo();
                    }
                    else
                    {
                        e.Value = string.Empty;
                    }
                    break;
            }
        }

        /// <summary>
        /// Handles the GridRebind event of the gInteractions control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        private void gInteractions_GridRebind( object sender, EventArgs e )
        {
            BindGrid();
        }

        protected bool IsCurrentlyPresent( DateTime? interactionEndDateTime )
        {
            var startDateTime = RockDateTime.Now.AddMinutes( -_currentlyPresentInterval );
            if ( interactionEndDateTime.HasValue && interactionEndDateTime.Value.CompareTo( startDateTime ) >= 0 )
            {
                return true;
            }
            return false;
        }

        #endregion

        #region Internal Methods

        /// <summary>
        /// Binds any needed data to the Grid Filter also using the user's stored
        /// preferences.
        /// </summary>
        private void BindFilter()
        {
            sdpDateRange.DelimitedValues = gfInteractions.GetFilterPreference( FilterSetting.DateRange );
            cbShowUnassignedDevices.Checked = gfInteractions.GetFilterPreference( FilterSetting.ShowUnassignedDevices ).AsBooleanOrNull() ?? false;
            cbPresentDevices.Checked = gfInteractions.GetFilterPreference( FilterSetting.PresentDevices ).AsBooleanOrNull() ?? false;
        }

        /// <summary>
        /// Binds the grid.
        /// </summary>
        private void BindGrid()
        {
            using ( var rockContext = new RockContext() )
            {
                if ( _personalDeviceId.HasValue )
                {
                    var personalDevice = new PersonalDeviceService( rockContext )
                        .Queryable().AsNoTracking()
                        .Where( d => d.Id == _personalDeviceId.Value )
                        .FirstOrDefault();
                    if ( personalDevice.PersonAlias != null )
                    {
                        lblHeading.Text = string.Format( "{0} Device Interactions", personalDevice.PersonAlias.Person.FullName.ToPossessive() );
                    }
                    else
                    {
                        lblHeading.Text = "Personal Device Interactions";
                    }
                }
                else
                {
                    lblHeading.Text = "Personal Device Interactions";
                }

                var interactionsQry = new InteractionService( rockContext )
                    .Queryable().AsNoTracking()
                    .Where( a => a.PersonalDeviceId != null );

                if ( _personalDeviceId.HasValue )
                {
                    interactionsQry = interactionsQry.Where( a => a.PersonalDeviceId == _personalDeviceId.Value );
                }
                gInteractions.ColumnsOfType<RockTemplateField>().First( a => a.HeaderText == "Assigned Individual" ).Visible = !_personalDeviceId.HasValue;

                var dateRange = SlidingDateRangePicker.CalculateDateRangeFromDelimitedValues( gfInteractions.GetFilterPreference( FilterSetting.DateRange ) );

                if ( dateRange.Start.HasValue )
                {
                    interactionsQry = interactionsQry.Where( e => e.InteractionDateTime >= dateRange.Start.Value );
                }

                if ( dateRange.End.HasValue )
                {
                    interactionsQry = interactionsQry.Where( e => e.InteractionDateTime < dateRange.End.Value );
                }

                if ( cbShowUnassignedDevices.Checked )
                {
                    interactionsQry = interactionsQry.Where( e => !e.PersonalDevice.PersonAliasId.HasValue );
                }

                if ( cbPresentDevices.Checked )
                {
                    var startDateTime = RockDateTime.Now.AddMinutes(-_currentlyPresentInterval );
                    interactionsQry = interactionsQry.Where( e => e.InteractionEndDateTime.HasValue && e.InteractionEndDateTime.Value.CompareTo(startDateTime) >= 0 );
                }

                SortProperty sortProperty = gInteractions.SortProperty;
                if ( sortProperty != null )
                {
                    interactionsQry = interactionsQry.Sort( sortProperty );
                }
                else
                {
                    interactionsQry = interactionsQry.OrderByDescending( p => p.InteractionDateTime );
                }

                gInteractions.SetLinqDataSource<Interaction>( interactionsQry );
                gInteractions.DataBind();
            }
        }

        #endregion
    }
}