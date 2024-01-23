using System.Collections.Generic;
using System.ComponentModel;
using System.Data.Entity;
using System.Linq;

using Rock.Attribute;
using Rock.Data;
using Rock.Model;
using Rock.ViewModels.Blocks.Event;
using Rock.Web.UI.Controls;

namespace Rock.Blocks.Event
{
    /// <summary>
    /// 
    /// </summary>
    /// <seealso cref="Rock.Blocks.RockBlockType" />
    [DisplayName( "Registration List Lava" )]
    [Category( "Event" )]
    [Description( "List recent registrations using a Lava template." )]
    [SupportedSiteTypes( Model.SiteType.Web )]

    #region Block Attributes

    [CodeEditorField( "Lava Template",
        Description = "Lava template to use to display content",
        EditorMode = CodeEditorMode.Lava,
        EditorTheme = CodeEditorTheme.Rock,
        EditorHeight = 400,
        IsRequired = true,
        DefaultValue = @"{% include '~~/Assets/Lava/RegistrationListSidebar.lava' %}",
        Category = "",
        Order = 2,
        Key = AttributeKey.LavaTemplate )]

    [CustomCheckboxListField( "Registrations to Display",
        Description = "The items you select here will control which registrations will be shown. When Balance Due is selected, only items with a balance will be shown",
        ListSource = "BalanceDue^Balance Due, RecentRegistrations^Recent Registrations, FutureEvents^Future Events",
        Order = 3,
        Key = AttributeKey.RegistrationsToDisplay )]

    [SlidingDateRangeField( "Recent Registrations Date Range",
        Description = "If Recent Registrations is selected above, this sliding date range controls which registrations should be displayed.",
        DefaultValue = "Last|3|Month||",
        IsRequired = false,
        EnabledSlidingDateRangeTypes = "Last",
        Order = 6,
        Key = AttributeKey.RecentRegistrations )]

    [SlidingDateRangeField( "Future Events Date Range",
        Description = "If Future Events is selected above, this sliding date range controls which registrations for future events should be displayed.",
        EnabledSlidingDateRangeTypes = "Next",
        Order = 7,
        IsRequired = false,
        Key = AttributeKey.DateRange )]

    #endregion
    [Rock.SystemGuid.BlockTypeGuid( "C0CFDAB7-BB29-499E-BD0A-468B0856C037" )]
    [Rock.SystemGuid.EntityTypeGuid( "52C84E33-FE5F-4023-8365-A5FE1F71C93B" )]
    public class RegistrationListLava : RockBlockType
    {
        #region Keys

        private static class AttributeKey
        {
            public const string LavaTemplate = "LavaTemplate";
            public const string RegistrationsToDisplay = "RegistrationsToDisplay";
            public const string DateRange = "DateRange";
            public const string RecentRegistrations = "RecentRegistrations";
        }

        private static class NavigationUrlKey
        {
            public const string ParentPage = "ParentPage";
        }

        private static class RegistrationsToDisplayKey
        {
            public const string BalanceDue = "BalanceDue";
            public const string FutureEvents = "FutureEvents";
            public const string RecentRegistrations = "RecentRegistrations";
        }

        #endregion


        #region Properties

        private string LavaTemplate => GetAttributeValue( AttributeKey.LavaTemplate );

        #endregion

        #region Public Methods

        /// <inheritdoc/>
        public override object GetObsidianBlockInitialization()
        {
            var registrationListLava = new RegistrationListLavaBox();
            using ( RockContext rockContext = new RockContext() )
            {
                var registrationService = new RegistrationService( rockContext );
                List<Registration> registrationList = Registrations( registrationService );

                var mergeFields = this.RequestContext.GetCommonMergeFields();
                mergeFields.Add( "Registrations", registrationList );
                registrationListLava.Html = $@"<div> {LavaTemplate.ResolveMergeFields( mergeFields )}</div>";
            }
            return registrationListLava;
        }

        private List<Registration> Registrations( RegistrationService registrationService )
        {
            var registrationsToDisplay = GetAttributeValue( AttributeKey.RegistrationsToDisplay ).SplitDelimitedValues();

            // Limit to the current person
            int currentPersonId = this.GetCurrentPerson()?.Id ?? 0;

            // Return an empty list if the person is not logged in.
            if(currentPersonId == 0)
            {
                return new List<Registration>();
            }

            var registrationList = registrationService.Queryable()
                .Include( r => r.RegistrationInstance.Linkages )
                .Where( a =>
                    a.RegistrationInstance.IsActive == true &&
                    !a.IsTemporary &&
                    a.PersonAlias.PersonId == currentPersonId )
                .ToList();

            // filter out the registrations with no balance if required.
            if ( registrationsToDisplay.Contains( RegistrationsToDisplayKey.BalanceDue ) )
            {
                registrationList = registrationList.Where( r => r.BalanceDue != 0 )
                    .ToList();
            }

            // if no date range is specified, return all the registrations.
            if ( !registrationsToDisplay.Contains( RegistrationsToDisplayKey.FutureEvents ) && !registrationsToDisplay.Contains( RegistrationsToDisplayKey.RecentRegistrations ) )
            {
                return registrationList;
            }

            HashSet<Registration> futureRegistrationsToDisplay = new HashSet<Registration>();
            HashSet<Registration> pastRegistrationsToDisplay = new HashSet<Registration>();

            // display future events if needed
            if ( registrationsToDisplay.Contains( RegistrationsToDisplayKey.FutureEvents ) )
            {
                var futureEventsDataRange = RockDateTimeHelper.CalculateDateRangeFromDelimitedValues( GetAttributeValue( AttributeKey.DateRange ), RockDateTime.Now );

                // if the value for futureEventsDataRange happens to be null, show all the registrations having an event
                if ( futureEventsDataRange.Start == null && futureEventsDataRange.End == null )
                {
                    futureRegistrationsToDisplay = registrationList.Where( r =>
                    {
                        var nextOccurrence = r.RegistrationInstance.Linkages
                            .Where( x => x.EventItemOccurrenceId.HasValue && x.EventItemOccurrence.NextStartDateTime.HasValue )
                            .OrderBy( b => b.EventItemOccurrence.NextStartDateTime )
                            .FirstOrDefault();
                        return nextOccurrence != null;
                    } )
                        .ToHashSet();
                }
                else
                {
                    futureRegistrationsToDisplay = registrationList.Where( r =>
                        {
                            var nextOccurrence = r.RegistrationInstance.Linkages
                                .Where( x => x.EventItemOccurrenceId.HasValue && x.EventItemOccurrence.NextStartDateTime.HasValue )
                                .OrderBy( b => b.EventItemOccurrence.NextStartDateTime )
                                .FirstOrDefault();
                            return nextOccurrence != null && futureEventsDataRange.Contains( nextOccurrence.EventItemOccurrence.NextStartDateTime.Value );
                        } )
                        .ToHashSet();
                }
            }

            // display past events if needed
            if ( registrationsToDisplay.Contains( RegistrationsToDisplayKey.RecentRegistrations ) )
            {
                var pastEventsDataRange = RockDateTimeHelper.CalculateDateRangeFromDelimitedValues( GetAttributeValue( AttributeKey.RecentRegistrations ), RockDateTime.Now );
                // in case the pastEventsDataRange happens to not have a value, show all the registrations else filter out the ones within the range
                if ( pastEventsDataRange.Start == null && pastEventsDataRange.End == null )
                {
                    pastRegistrationsToDisplay = registrationList.ToHashSet();
                }
                else
                {
                    pastRegistrationsToDisplay = registrationList.Where( r => pastEventsDataRange.Contains( r.CreatedDateTime.Value ) )
                        .ToHashSet();
                }
            }

            return futureRegistrationsToDisplay.Union( pastRegistrationsToDisplay ).ToList();

        }

        #endregion
    }
}
