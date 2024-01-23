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
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.Entity;
using System.Linq;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;

using Rock;
using Rock.Attribute;
using Rock.CheckIn;
using Rock.Model;
using Rock.Web.UI.Controls;

namespace RockWeb.Blocks.CheckIn
{
    [DisplayName( "Person Select (Family Check-in)" )]
    [Category( "Check-in" )]
    [Description( "Lists people who match the selected family and provides option of selecting multiple." )]

    #region Attribute Keys

    [LinkedPage( "Auto Select Next Page",
        Key = AttributeKey.AutoSelectNextPage,
        Description = "The page to navigate to after selecting people in auto-select mode.",
        IsRequired = false,
        Order = 5 )]

    [CodeEditorField( "Pre-Selected Options Format",
        Key = AttributeKey.OptionFormat,
        Description = "The format to use when displaying auto-checkin options. Merge fields include GroupType, Group, Location, Schedule, LocationCount and DisplayLocationCount (true/false).",
        EditorMode = CodeEditorMode.Lava,
        EditorTheme = CodeEditorTheme.Rock,
        EditorHeight = 100,
        IsRequired = false,
        DefaultValue = PreSelectedOptionsFormatDefaultValue,
        Order = 6 )]

    [TextField( "Caption",
        Key = AttributeKey.Caption,
        IsRequired = false,
        DefaultValue = "Select People",
        Category = "Text",
        Order = 7 )]

    [TextField( "Option Title",
        Key = AttributeKey.OptionTitle,
        Description = "Title to display on option screen. Use {0} for person's full name.",
        IsRequired = false,
        DefaultValue = "{0}",
        Category = "Text",
        Order = 8 )]

    [TextField( "Option Sub Title",
        Key = AttributeKey.OptionSubTitle,
        Description = "Subtitle to display on option screen. Use {0} for person's nickname.",
        IsRequired = false,
        DefaultValue = "Please select the options that {0} would like to attend.",
        Category = "Text",
        Order = 9 )]

    [TextField( "No Option Message",
        Key = AttributeKey.NoOptionMessage,
        Description = @"Message to display on a person's row if they don't have any options available. This is rare, as people without options are normally removed from the list altogether, but there are some check-in configurations that can lead to ""unscheduleable"" people remaining in the list.",
        IsRequired = false,
        DefaultValue = "",
        Category = "Text",
        Order = 10 )]

    [TextField( "Next Button Text",
        Key = AttributeKey.NextButtonText,
        Description = "",
        IsRequired = false,
        DefaultValue = "Next",
        Category = "Text",
        Order = 11 )]

    #endregion Attribute Keys

    [Rock.SystemGuid.BlockTypeGuid( "92DCF018-F551-4890-8BA1-511D97BF6B8A" )]
    public partial class MultiPersonSelect : CheckInBlock
    {
        /* 2021-05/07 ETD
         * Use new here because the parent CheckInBlock also has inherited class AttributeKey.
         */
        private new static class AttributeKey
        {
            public const string AutoSelectNextPage = "AutoSelectNextPage";
            public const string OptionFormat = "OptionFormat";
            public const string Caption = "Caption";
            public const string OptionTitle = "OptionTitle";
            public const string OptionSubTitle = "OptionSubTitle";
            public const string NoOptionMessage = "NoOptionMessage";
            public const string NextButtonText = "NextButtonText";
        }

        #region Attribute Default Values
        private const string PreSelectedOptionsFormatDefaultValue = @"
<span class='auto-select-schedule'>{{ Schedule.Name }}:</span>
<span class='auto-select-group'>{{ Group.Name }}</span>
<span class='auto-select-location'>{{ Location.Name }}</span
{% if DisplayLocationCount == true %}
<span class='ml-3'>Count: {{ LocationCount }}</span>
{% endif %} 
";


        #endregion Attribute Default Values

        bool _hidePhotos = false;
        bool _autoCheckin = false;

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );

            rSelection.ItemDataBound += rSelection_ItemDataBound;
            rSelection.ItemCommand += RSelection_ItemCommand;

            string script = string.Format( @"
        function GetPersonSelection() {{
            var ids = '';
            $('div.checkin-person-list').find('i.fa-check-square').each( function() {{
                ids += $(this).closest('a').attr('data-person-id') + ',';
            }});
            if (ids == '') {{
                bootbox.alert('Please select at least one person');
                return false;
            }}
            else
            {{
                $('#{0}').button('loading')
                $('#{1}').val(ids);
                return true;
            }}
        }}

        $('a.js-person-select').on('click', function() {{
            $(this).toggleClass('active');
            $(this).find('i').toggleClass('fa-check-square').toggleClass('fa-square-o');
            var ids = '';
            $('div.checkin-person-list').find('i.fa-check-square').each( function() {{
                ids += $(this).closest('a').attr('data-person-id') + ',';
            }});
            $('#{1}').val(ids);
        }});

        function GetOptionSelection() {{
            var keys = '';
            $('div.checkin-option-list').find('i.fa-check-square').each( function() {{
                keys += $(this).closest('a').attr('data-key') + ',';
            }});
            if (keys == '') {{
                bootbox.alert('Please select at least one option');
                return false;
            }}
            else
            {{
                $('#{2}').button('loading')
                $('#{3}').val(keys);
                return true;
            }}
        }}

        $('a.js-option-select').on('click', function() {{
            $(this).removeClass('btn-dimmed');
            $(this).find('i').toggleClass('fa-check-square').toggleClass('fa-square-o');
            var scheduleId = $(this).attr('data-schedule-id');
            var selected = $(this).find('i.fa-check-square').length != 0;
            $(this).siblings().each( function() {{
                if ( $(this).attr('data-schedule-id') == scheduleId ) {{
                    if ( selected ) {{
                        $(this).find('i').removeClass('fa-check-square').addClass('fa-square-o');
                        $(this).addClass('btn-dimmed');
                    }} else {{
                        $(this).removeClass('btn-dimmed');
                    }}
                }}
            }});
        }});
", lbSelect.ClientID, hfPeople.ClientID, lbOptionSelect.ClientID, hfOptions.ClientID );
            ScriptManager.RegisterStartupScript( pnlContent, pnlContent.GetType(), "SelectPerson", script, true );
        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Load" /> event.
        /// </summary>
        /// <param name="e">The <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnLoad( EventArgs e )
        {
            base.OnLoad( e );

            RockPage.AddScriptLink( "~/Scripts/CheckinClient/checkin-core.js" );

            if ( CurrentWorkflow == null || CurrentCheckInState == null )
            {
                NavigateToHomePage();
            }
            else
            {
                _autoCheckin = CurrentCheckInState.CheckInType.AutoSelectOptions.HasValue && CurrentCheckInState.CheckInType.AutoSelectOptions.Value == 1;
                _hidePhotos = CurrentCheckInState.CheckInType.HidePhotos;

                if ( !Page.IsPostBack )
                {
                    ClearSelection();

                    var family = CurrentCheckInState.CheckIn.CurrentFamily;
                    if ( family == null )
                    {
                        GoBack();
                        return;
                    }

                    lbEditFamily.Visible = CurrentCheckInState.Kiosk.RegistrationModeEnabled;

                    lTitle.Text = GetTitleText();
                    lCaption.Text = GetAttributeValue( AttributeKey.Caption );
                    lbSelect.Text = GetAttributeValue( AttributeKey.NextButtonText );

                    if ( _autoCheckin )
                    {
                        // Because auto-checkin bypasses any other workflow processing, the check for previous check-ins needs to be done manually
                        bool preventDuplicate = !IsOverride && CurrentCheckInState.CheckInType.PreventDuplicateCheckin;
                        using ( var rockContext = new Rock.Data.RockContext() )
                        {
                            Rock.Workflow.Action.CheckIn.SetAvailableSchedules.ProcessForFamily( rockContext, family );
                            Rock.Workflow.Action.CheckIn.FilterByPreviousCheckin.ProcessForFamily( rockContext, family, preventDuplicate );
                        }
                    }

                    foreach ( var person in family.People )
                    {
                        // Check to see if person has option pre-selected and if not, select first item.
                        if ( _autoCheckin && !person.GroupTypes.Any( t => t.PreSelected ) )
                        {
                            SelectFirstOption( person );
                        }

                        // JS uses this to keep track of the selected items, so make sure to start it out with the preselected persons.
                        if ( person.PreSelected )
                        {
                            hfPeople.Value += $"{person.Person.Id},";
                        }
                    }

                    hfPeople.Value = hfPeople.Value.TrimEnd( new char[] { ',' } );

                    BindData();
                }
                else
                {
                    var selectedPersonIds = hfPeople.Value.SplitDelimitedValues().AsIntegerList();

                    var family = CurrentCheckInState.CheckIn.CurrentFamily;
                    if ( family != null )
                    {
                        foreach ( var person in family.People )
                        {
                            person.PreSelected = selectedPersonIds.Contains( person.Person.Id );
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Handles the ItemDataBound event of the rSelection control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RepeaterItemEventArgs"/> instance containing the event data.</param>
        private void rSelection_ItemDataBound( object sender, RepeaterItemEventArgs e )
        {
            if ( e.Item.ItemType == ListItemType.Item || e.Item.ItemType == ListItemType.AlternatingItem )
            {
                var pnlPhoto = e.Item.FindControl( "pnlPhoto" ) as Panel;
                pnlPhoto.Visible = !_hidePhotos;

                var lPersonButton = e.Item.FindControl( "lPersonButton" ) as Literal;
                var lPersonSelectLava = e.Item.FindControl( "lPersonSelectLava" ) as Literal;
                var person = e.Item.DataItem as CheckInPerson;

                if ( lPersonButton != null && person != null )
                {
                    var options = new List<string>();

                    if ( _autoCheckin )
                    {
                        var selectedOptions = person.GetOptions( true, true );

                        foreach ( var option in selectedOptions )
                        {
                            var optionText = GetOptionText( option );
                            options.Add( optionText );
                        }

                        var pnlPersonButton = e.Item.FindControl( "pnlPersonButton" ) as Panel;
                        var pnlChangeButton = e.Item.FindControl( "pnlChangeButton" ) as Panel;
                        if ( pnlPersonButton != null && pnlChangeButton != null )
                        {
                            pnlPersonButton.CssClass = "checkin-person-btn checkin-person-has-change col-xs-12 col-sm-9 col-md-10";
                            pnlChangeButton.Visible = selectedOptions.Count > 1 || AnyUnselectedOptions( person );
                        }
                    }

                    var noOptionMessage = GetAttributeValue( AttributeKey.NoOptionMessage );

                    if ( options.Any() )
                    {
                        lPersonButton.Text = string.Format( @"
<div class='row'>
    <div class='col-md-5 family-personselect'>{0}</div>
    <div class='col-md-7 auto-select family-auto-select'>
        <div class='auto-select-caption'>Current Selection</div>
        <div class='auto-select-details'>{1}</div>
    </div>
</div>

", person.Person.FullName, options.AsDelimited( "<br/>" ) );
                    }
                    else if ( !person.AnyPossibleSchedules && noOptionMessage.IsNotNullOrWhiteSpace() )
                    {
                        lPersonButton.Text = string.Format( @"
<div class='row'>
    <div class='col-md-5 family-personselect'>{0}</div>
    <div class='col-md-7 family-no-option'>
        <div class='no-option-caption'>{1}</div>
    </div>
</div>"
, person.Person.FullName, noOptionMessage );
                    }
                    else
                    {
                        lPersonButton.Text = string.Format( @"
<div class='family-personselect'>{0}</div>
", person.Person.FullName );
                    }
                }

                if ( lPersonSelectLava != null && person != null )
                {
                    var personSelectLavaTemplate = CurrentCheckInState.CheckInType.PersonSelectAdditionalInfoLavaTemplate;
                    if ( personSelectLavaTemplate.IsNotNullOrWhiteSpace() )
                    {
                        var mergeFields = Rock.Lava.LavaHelper.GetCommonMergeFields( this.RockPage, null, new Rock.Lava.CommonMergeFieldsOptions() );
                        mergeFields.Add( "Person", person );
                        lPersonSelectLava.Text = personSelectLavaTemplate.ResolveMergeFields( mergeFields );
                    }
                }
            }
        }

        /// <summary>
        /// Get the option text.
        /// </summary>
        private string GetOptionText( CheckInPersonSummary option )
        {
            var format = GetAttributeValue( AttributeKey.OptionFormat );
            var mergeFields = new Dictionary<string, object>
            {
                { "GroupType", option.GroupType },
                { "Group", option.Group },
                { "Location", option.Location },
                { "Schedule", option.Schedule },
                { "DisplayLocationCount", CurrentCheckInState.CheckInType.DisplayLocationCount },
                { "LocationCount", KioskLocationAttendance.Get( option.Location.Location.Id ).CurrentCount }
            };

            var optionText = format.ResolveMergeFields( mergeFields );
            return optionText;
        }

        /// <summary>
        /// Handles the ItemCommand event of the RSelection control.
        /// </summary>
        /// <param name="source">The source of the event.</param>
        /// <param name="e">The <see cref="RepeaterCommandEventArgs"/> instance containing the event data.</param>
        /// <exception cref="System.NotImplementedException"></exception>
        private void RSelection_ItemCommand( object source, RepeaterCommandEventArgs e )
        {
            if ( e.CommandName == "Change" )
            {
                ShowOptions( e.CommandArgument.ToString().AsInteger() );
            }
        }

        /// <summary>
        /// Handles the Click event of the lbSelect control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void lbSelect_Click( object sender, EventArgs e )
        {
            if ( KioskCurrentlyActive )
            {
                var family = CurrentCheckInState.CheckIn.CurrentFamily;
                if ( family != null )
                {
                    foreach ( var person in family.People )
                    {
                        person.Selected = person.PreSelected;

                        if ( _autoCheckin && person.Selected )
                        {
                            foreach ( var groupType in person.GroupTypes )
                            {
                                groupType.Selected = groupType.PreSelected;
                                foreach ( var group in groupType.Groups )
                                {
                                    group.Selected = group.PreSelected;
                                    foreach ( var location in group.Locations )
                                    {
                                        location.Selected = location.PreSelected;
                                        foreach ( var schedule in location.Schedules )
                                        {
                                            schedule.Selected = schedule.PreSelected;
                                            if ( schedule.Selected )
                                            {
                                                int scheduleId = schedule.Schedule.Id;
                                                person.PossibleSchedules.Where( s => s.Schedule.Id == scheduleId ).ToList().ForEach( s => { s.Selected = true; } );
                                                groupType.SelectedForSchedule.Add( scheduleId, true );
                                                group.SelectedForSchedule.Add( scheduleId, true );
                                                location.SelectedForSchedule.Add( scheduleId, true );
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }

                    if ( _autoCheckin )
                    {
                        SaveState();
                        NavigateToLinkedPage( AttributeKey.AutoSelectNextPage );
                    }
                    else
                    {
                        ProcessSelection( maWarning );
                    }
                }
            }
        }

        protected void lbBack_Click( object sender, EventArgs e )
        {
            GoBack();
        }

        protected void lbCancel_Click( object sender, EventArgs e )
        {
            CancelCheckin();
        }

        protected void lbOptionCancel_Click( object sender, EventArgs e )
        {
            pnlOptions.Visible = false;
            pnlSelection.Visible = true;
        }

        protected void lbOptionSelect_Click( object sender, EventArgs e )
        {
            var family = CurrentCheckInState.CheckIn.CurrentFamily;
            if ( family != null )
            {
                var person = family.People.FirstOrDefault( p => p.Person.Id == hfPersonId.ValueAsInt() );
                if ( person != null )
                {
                    var selectedKeys = hfOptions.Value.Split( new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries ).ToList();

                    person.PreSelected = false;
                    foreach( var groupType in person.GroupTypes )
                    {
                        groupType.PreSelected = false;
                        foreach( var group in groupType.Groups )
                        {
                            group.PreSelected = false;
                            foreach( var location in group.Locations )
                            {
                                location.PreSelected = false;
                                foreach( var schedule in location.Schedules )
                                {
                                    schedule.PreSelected = false;

                                    string currentKey = string.Format( "{0}|{1}|{2}|{3}", groupType.GroupType.Id, group.Group.Id, location.Location.Id, schedule.Schedule.Id );
                                    if ( selectedKeys.Contains( currentKey ) )
                                    {
                                        schedule.PreSelected = true;
                                        location.PreSelected = true;
                                        group.PreSelected = true;
                                        groupType.PreSelected = true;
                                        person.PreSelected = true;
                                    }
                                }
                            }
                        }
                    }
                }
            }

            BindData();

            pnlOptions.Visible = false;
            pnlSelection.Visible = true;

        }

        /// <summary>
        /// Clear any previously selected people.
        /// </summary>
        private void ClearSelection()
        {
            foreach ( var family in CurrentCheckInState.CheckIn.Families )
            {
                foreach ( var person in family.People )
                {
                    person.ClearFilteredExclusions();
                    person.Selected = false;
                    person.Processed = false;
                }
            }
        }

        private void BindData()
        {
            var family = CurrentCheckInState.CheckIn.CurrentFamily;
            if ( family != null )
            {
                rSelection.DataSource = family.People
                .OrderByDescending( p => p.FamilyMember )
                .ThenBy( p => p.Person.BirthYear )
                .ThenBy( p => p.Person.BirthMonth )
                .ThenBy( p => p.Person.BirthDay )
                .ToList();

                rSelection.DataBind();
            }
        }

        protected string GetSelectedClass( bool selected )
        {
            return selected ? "active" : "";
        }

        protected string GetDisabledClass( bool anyPossibleSchedules )
        {
            return !anyPossibleSchedules ? "disabled" : "";
        }

        protected string GetCheckboxClass( bool selected )
        {
            return selected ? "fa fa-check-square fa-3x" : "fa fa-square-o fa-3x";
        }

        protected string GetPersonImageTag( object dataitem )
        {
            var person = dataitem as Person;
            if ( person != null )
            {
                return Person.GetPersonPhotoUrl( person, 200, 200 );
            }
            return string.Empty;
        }

        protected string GetButtonText( object dataItem )
        {
            var person = dataItem as CheckInPerson;
            if ( person != null )
            {
                var options = new List<string>();
                if ( _autoCheckin && person.PreSelected )
                {
                    foreach ( var option in person.GetOptions( true, true ) )
                    {
                        var optionText = GetOptionText( option );
                        options.Add( optionText );
                    }
                }

                if (options.Any() )
                {
                    return string.Format( @"
<div class='row'>
    <div class='col-md-5 family-personselect'>{0}</div>
    <div class='col-md-7 auto-select family-auto-select'><div class='auto-select-caption'>Current Selection</div><div class='auto-select-details'>{1}</div></div>
</div>
", person.Person.FullName, options.AsDelimited( "<br/>" ) );
                }
                else
                {
                    return string.Format( @"
<div class='family-personselect'>{0}</div>
", person.Person.FullName );
                }
            }

            return string.Empty;
        }

        private string GetTitleText()
        {
            var mergeFields = new Dictionary<string, object>
            {
                { LavaMergeFieldName.Family, CurrentCheckInState.CheckIn.CurrentFamily.Group }
            };

            var multiPersonSelectHeaderLavaTemplate = CurrentCheckInState.CheckInType.MultiPersonSelectHeaderLavaTemplate ?? string.Empty;
            return multiPersonSelectHeaderLavaTemplate.ResolveMergeFields( mergeFields );
        }

        private void ShowOptions( int personId )
        {
            var family = CurrentCheckInState.CheckIn.CurrentFamily;
            if ( family == null )
            {
                GoBack();
            }

            var person = family.People.FirstOrDefault( p => p.Person.Id == personId );
            if ( person != null )
            {
                hfPersonId.Value = person.Person.Id.ToString();
                lOptionTitle.Text = string.Format( GetAttributeValue( AttributeKey.OptionTitle ), person.Person.FullName );
                lOptionSubTitle.Text = string.Format( GetAttributeValue( AttributeKey.OptionSubTitle ), person.Person.NickName );

                BindOptions();

                pnlSelection.Visible = false;
                pnlOptions.Visible = true;
            }
        }

        private void BindOptions()
        {
            var family = CurrentCheckInState.CheckIn.CurrentFamily;
            if ( family != null )
            {
                var person = family.People.FirstOrDefault( p => p.Person.Id == hfPersonId.ValueAsInt() );
                if ( person != null )
                {
                    var options = person.GetOptions( false, false );
                    var selectedScheduleIds = options.Where( o => o.Selected ).Select( o => o.Schedule.Schedule.Id ).ToList();
                    options.Where( o => !o.Selected && selectedScheduleIds.Contains( o.Schedule.Schedule.Id ) ).ToList().ForEach( o => { o.Disabled = true; } );

                    rOptions.DataSource = options;
                    rOptions.DataBind();
                }
            }
        }

        protected string GetOptionText( object dataItem )
        {
            var option = dataItem as CheckInPersonSummary;
            if ( option != null )
            {
                return GetOptionText( option );
            }

            return string.Empty;
        }

        /// <summary>
        /// Are there any unselected options.
        /// </summary>
        /// <returns></returns>
        public bool AnyUnselectedOptions( CheckInPerson person )
        {
            foreach ( var groupType in person.GroupTypes )
            {
                if ( !groupType.PreSelected && groupType.AvailableForSchedule?.Any() == true )
                {
                    return true;
                }
                foreach ( var group in groupType.Groups )
                {
                    if ( !group.PreSelected && group.AvailableForSchedule?.Any() == true )
                    {
                        return true;
                    }
                    foreach ( var location in group.Locations )
                    {
                        if ( !location.PreSelected && location.AvailableForSchedule?.Any() == true )
                        {
                            return true;
                        }
                        foreach ( var schedule in location.Schedules )
                        {
                            if ( !schedule.PreSelected )
                            {
                                return true;
                            }
                        }
                    }
                }
            }

            return false;
        }

        /// <summary>
        /// Selects the first option based on the order property of Schedule from CheckinPerson.PossibleSchedules.
        /// If no schedules exists in PossibleSchedules than one is chosen based on the Order property of GroupType, Group, Location, and Schedule.
        /// </summary>
        /// <param name="person">The person.</param>
        private void SelectFirstOption( CheckInPerson person )
        {
            var firstSchedule = person.PossibleSchedules.OrderBy( s => s.Schedule.Order ).FirstOrDefault();
            if ( firstSchedule != null )
            {
                foreach ( var groupType in person.GroupTypes.Where( t => t.AvailableForSchedule.Contains( firstSchedule.Schedule.Id ) ).OrderBy( t => t.GroupType.Order ) )
                {
                    foreach ( var group in groupType.Groups.Where( g => g.AvailableForSchedule.Contains( firstSchedule.Schedule.Id ) ).OrderBy( g => g.Group.Order ) )
                    {
                        foreach ( var location in group.Locations.Where( l => l.AvailableForSchedule.Contains( firstSchedule.Schedule.Id ) ).OrderBy( l => l.Order ) )
                        {
                            foreach ( var schedule in location.Schedules.Where( s => s.Schedule.Id == firstSchedule.Schedule.Id ) )
                            {
                                if ( location.AvailableForSchedule.Contains( schedule.Schedule.Id ) )
                                {
                                    schedule.PreSelected = true;
                                    location.PreSelected = true;
                                    group.PreSelected = true;
                                    groupType.PreSelected = true;

                                    return;
                                }
                            }
                        }
                    }
                }
            }

            // Couldn't find a match for first schedule, just select first available option based on the order property
            foreach ( var groupType in person.GroupTypes.OrderBy( t => t.GroupType.Order ) )
            {
                foreach ( var group in groupType.Groups.OrderBy( g => g.Group.Order ) )
                {
                    foreach ( var location in group.Locations.OrderBy( l => l.Order ) )
                    {
                        foreach ( var schedule in location.Schedules.OrderBy( s => s.Schedule.Order ) )
                        {
                            int scheduleId = schedule.Schedule.Id;

                            if (location.AvailableForSchedule.Contains( scheduleId ) &&
                                group.AvailableForSchedule.Contains( scheduleId ) &&
                                groupType.AvailableForSchedule.Contains( scheduleId ) )
                            {
                                schedule.PreSelected = true;
                                location.PreSelected = true;
                                group.PreSelected = true;
                                groupType.PreSelected = true;

                                return;
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Handles the Click event of the lbEditFamily control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void lbEditFamily_Click( object sender, EventArgs e )
        {
            if ( CurrentCheckInState == null )
            {
                return;
            }

            var editFamilyBlock = this.RockPage.ControlsOfTypeRecursive<CheckInEditFamilyBlock>().FirstOrDefault();
            if ( editFamilyBlock != null && CurrentCheckInState.CheckIn.CurrentFamily != null )
            {
                editFamilyBlock.ShowEditFamily( CurrentCheckInState.CheckIn.CurrentFamily );
            }
        }


    }
}