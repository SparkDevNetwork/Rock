using System;
using System.Collections.Generic;
using System.ComponentModel;
using Rock.Attribute;
using Rock.Data;
using Rock.Model;
using Rock.Security;
using Rock.SystemGuid;
using Rock.ViewModels.Blocks.Engagement.StreakMapEditor;
using Rock.ViewModels.Utility;
using Rock.Web.Cache;

namespace Rock.Blocks.Engagement
{
    [DisplayName( "Streak Map Editor" )]
    [Category( "Engagement" )]
    [Description( "Allows editing a streak occurrence, engagement, or exclusion map." )]
    [IconCssClass( "fa fa-calendar-check" )]
    // [SupportedSiteTypes( Model.SiteType.Web )]

    #region Block Attributes

    [BooleanField(
        name: "Show Streak Enrollment Exclusion Map",
        description: "If this map editor is placed in the context of a streak enrollment, should it show the person exclusion map for that streak enrollment?",
        defaultValue: false,
        Key = AttributeKey.IsEngagementExclusion,
        Order = 0 )]

    #endregion

    [Rock.SystemGuid.EntityTypeGuid( "4935B24C-851A-4480-A907-EAEB90D594D2" )]
    [Rock.SystemGuid.BlockTypeGuid( "B5616E10-0551-41BB-BD14-3ABA33E0040B" )]
    public class StreakMapEditor : RockBlockType
    {
        #region Keys

        /// <summary>
        /// Keys to use for Block Attributes
        /// </summary>
        private static class AttributeKey
        {
            /// <summary>
            /// The is engagement exclusion map key
            /// </summary>
            public const string IsEngagementExclusion = "IsEngagementExclusion";
        }

        /// <summary>
        /// Keys for user preferences
        /// </summary>
        private static class PreferenceKey
        {
            /// <summary>
            /// The date range user preference key
            /// </summary>
            public const string DateRange = "date-range";
        }

        /// <summary>
        /// Keys to use for Page Parameters
        /// </summary>
        private static class PageParameterKey
        {
            /// <summary>
            /// The streak type id page parameter key
            /// </summary>
            public const string StreakTypeId = "StreakTypeId";

            /// <summary>
            /// The streak id page parameter key
            /// </summary>
            public const string StreakId = "StreakId";

            /// <summary>
            /// The streak type exclusion id page parameter key
            /// </summary>
            public const string StreakTypeExclusionId = "StreakTypeExclusionId";
        }

        #endregion Keys

        #region Constants

        private const int DaysPerWeek = 7;
        private const int DefaultCheckboxCount = 7;

        #endregion Constants

        #region Properties

        protected string DateRange => GetBlockPersonPreferences()
            .GetValue( PreferenceKey.DateRange );

        #endregion

        #region Methods

        /// <inheritdoc/>
        public override object GetObsidianBlockInitialization()
        {
            StreakMapEditorBag bag = GetCheckboxData( DateRange );

            bag.IsEngagementExclusion = GetAttributeValue( AttributeKey.IsEngagementExclusion ).AsBoolean();
            
            return bag;
        }

        /// <summary>
        /// Get the selected date range or the default range
        /// </summary>
        /// <returns></returns>
        private DateRange GetDateRange( StreakTypeCache streakType, string delimitedDateValues )
        {
            var isDaily = IsStreakTypeDaily( streakType );
            var range = RockDateTimeHelper.CalculateDateRangeFromDelimitedValues( delimitedDateValues, RockDateTime.Now );

            if ( !range.Start.HasValue && !range.End.HasValue )
            {
                if ( isDaily )
                {
                    range.Start = RockDateTime.Today.AddDays( 0 - ( DefaultCheckboxCount - 1 ) );
                }
                else
                {
                    range.Start = RockDateTime.Today.AddDays( 0 - ( DaysPerWeek * ( DefaultCheckboxCount - 1 ) ) );
                }

                range.End = StreakTypeService.AlignDate( RockDateTime.Now, streakType );
            }
            else if ( !range.Start.HasValue )
            {
                range.Start = range.End.Value.AddDays( ( 0 - DefaultCheckboxCount ) * ( isDaily ? 1 : DaysPerWeek ) );
            }
            else if ( !range.End.HasValue )
            {
                range.End = range.Start.Value.AddDays( ( DefaultCheckboxCount ) * ( isDaily ? 1 : DaysPerWeek ) );
            }

            return range;
        }

        /// <summary>
        /// Get the actual enrollment model for deleting or editing
        /// </summary>
        /// <returns></returns>
        private StreakTypeCache GetStreakType()
        {
            int streakTypeId;
            var streak = GetStreak();
            var exclusion = GetStreakTypeExclusion();

            if ( streak != null )
            {
                streakTypeId = streak.StreakTypeId;
            }
            else if ( exclusion != null )
            {
                streakTypeId = exclusion.StreakTypeId;
            }
            else
            {
                var streakTypeIdParam = PageParameter( PageParameterKey.StreakTypeId );
                streakTypeId = Rock.Utility.IdHasher.Instance.GetId( streakTypeIdParam ) ?? streakTypeIdParam.AsInteger();
            }

            var streakType = StreakTypeCache.Get( streakTypeId );
            return streakType;
        }

        /// <summary>
        /// Get the actual streak model for deleting or editing
        /// </summary>
        /// <returns></returns>
        private Streak GetStreak()
        {
            if ( _streak == null )
            {
                var streakIdParam = PageParameter( PageParameterKey.StreakId );
                var streakId = Rock.Utility.IdHasher.Instance.GetId( streakIdParam ) ?? streakIdParam.AsIntegerOrNull();

                if ( streakId.HasValue && streakId.Value > 0 )
                {
                    var streakService = new StreakService( RockContext );
                    _streak = streakService.Get( streakId.Value );
                }
            }

            return _streak;
        }
        private Streak _streak = null;

        /// <summary>
        /// Get the exclusion model
        /// </summary>
        /// <returns></returns>
        private StreakTypeExclusion GetStreakTypeExclusion()
        {
            if ( _streakTypeExclusion == null )
            {
                var exclusionIdParam = PageParameter( PageParameterKey.StreakTypeExclusionId );
                var id = Rock.Utility.IdHasher.Instance.GetId( exclusionIdParam ) ?? exclusionIdParam.AsIntegerOrNull();

                if ( id.HasValue && id.Value > 0 )
                {
                    var service = new StreakTypeExclusionService( RockContext );
                    _streakTypeExclusion = service.Get( id.Value );
                }
            }

            return _streakTypeExclusion;
        }
        private StreakTypeExclusion _streakTypeExclusion = null;

        /// <summary>
        /// Get the map being edited
        /// </summary>
        /// <returns></returns>
        private byte[] GetTargetMap( out string mapTitle )
        {
            // Title defaults to Occurrence Map just like Webforms.
            mapTitle = "Occurrence Map";

            if ( IsTargetingEngagementMap() )
            {
                var isEngagementExclusionMap = GetAttributeValue( AttributeKey.IsEngagementExclusion ).AsBoolean();
                if ( isEngagementExclusionMap )
                {
                    mapTitle = "Engagement Exclusion Map";
                    return GetStreak()?.ExclusionMap;
                }

                mapTitle = "Engagement Map";
                return GetStreak()?.EngagementMap;
            }

            if ( IsTargetingExclusionMap() )
            {
                mapTitle = "Exclusion Map";
                return GetStreakTypeExclusion()?.ExclusionMap;
            }

            if ( IsTargetingOccurrenceMap() )
            {
                return GetStreakType()?.OccurrenceMap;
            }

            return null;
        }

        /// <summary>
        /// Is the editor editing the enrollment Engagement map
        /// </summary>
        /// <returns></returns>
        private bool IsTargetingEngagementMap()
        {
            return GetStreak() != null;
        }

        /// <summary>
        /// Is the editor editing the exclusion map
        /// </summary>
        /// <returns></returns>
        private bool IsTargetingExclusionMap()
        {
            return !IsTargetingEngagementMap() && GetStreakTypeExclusion() != null;
        }

        /// <summary>
        /// Is the editor editing the occurrence map
        /// </summary>
        /// <returns></returns>
        private bool IsTargetingOccurrenceMap()
        {
            return !IsTargetingEngagementMap() && !IsTargetingExclusionMap() && GetStreakType() != null;
        }

        /// <summary>
        /// Returns true if the streak type is daily (vs weekly)
        /// </summary>
        /// <returns></returns>
        private bool IsStreakTypeDaily( StreakTypeCache streakType )
        {
            return streakType.OccurrenceFrequency == StreakOccurrenceFrequency.Daily;
        }

        /// <summary>
        /// Shows the controls needed
        /// </summary>
        private StreakMapEditorBag GetCheckboxData( string delimitedDateValues )
        {
            StreakMapEditorBag bag = new StreakMapEditorBag
            {
                DelimitedDateValues = delimitedDateValues,
                CheckboxItems = new List<ListItemBag>(),
                SelectedDates = new List<string>()
            };
            var streakType = GetStreakType();

            if ( streakType == null )
            {
                bag.ErrorMessage = "A streak type is required.";
                return bag;
            }

            if ( !CanEdit( streakType ) )
            {
                bag.IsPanelHidden = true;
                return bag;
            }

            var map = GetTargetMap( out string mapTitle );
            bag.MapTitle = mapTitle;

            bag.IsStreakTypeDaily = IsStreakTypeDaily( streakType );

            var dateRange = GetDateRange( streakType, bag.DelimitedDateValues );
            var startDate = StreakTypeService.AlignDate( dateRange.Start.Value, streakType );
            var endDate = StreakTypeService.AlignDate( dateRange.End.Value, streakType );

            // Change values based on streakType.OccurrenceFrequency (Days/Weeks/Months/Years).
            string dateTimeFormat;
            Func<DateTime, int, DateTime> dateTimeGenerator;

            switch ( streakType.OccurrenceFrequency )
            {
                case StreakOccurrenceFrequency.Daily:
                    bag.CheckboxLabel = "Days";
                    // Format as "ddd, MMM dd, yyyy" (ex. Wed, Jan 02, 2019).
                    dateTimeFormat = "ddd, MMM dd, yyyy";
                    dateTimeGenerator = ( d, i ) => d.AddDays( i );
                    break;
                case StreakOccurrenceFrequency.Weekly:
                    bag.CheckboxLabel = "Weeks";
                    // Format as "MMM dd, yyyy" (ex. Jan 02, 2019).
                    dateTimeFormat = "MMM dd, yyyy";
                    dateTimeGenerator = ( d, i ) => d.AddDays( i * DaysPerWeek );
                    break;
                case StreakOccurrenceFrequency.Monthly:
                    bag.CheckboxLabel = "Months";
                    // Format as "MMM yyyy" (ex. Jan 2019).
                    dateTimeFormat = "MMM yyyy";
                    dateTimeGenerator = ( d, i ) => d.AddMonths( i );
                    break;
                case StreakOccurrenceFrequency.Yearly:
                    bag.CheckboxLabel = "Years";
                    // Format as "yyyy" (ex. 2019).
                    dateTimeFormat = "yyyy";
                    dateTimeGenerator = ( d, i ) => d.AddYears( i );
                    break;
                default:
                    throw new NotImplementedException( $"StreakOccurrenceFrequency '{streakType.OccurrenceFrequency}' is not implemented" );
            }

            var minDate = StreakTypeService.AlignDate( streakType.StartDate, streakType );

            if ( IsTargetingEngagementMap() )
            {
                var enrollment = GetStreak();
                var enrollmentDate = StreakTypeService.AlignDate( enrollment.EnrollmentDate, streakType );

                if ( enrollmentDate > minDate )
                {
                    minDate = enrollmentDate;
                }
            }

            var maxDate = StreakTypeService.AlignDate( RockDateTime.Today, streakType );
            var checkboxCount = StreakTypeService.GetFrequencyUnitDifference( startDate, endDate, streakType, true );

            for ( var i = 0; i < checkboxCount; i++ )
            {
                var representedDate = dateTimeGenerator( startDate, i );
                bag.CheckboxItems.Add( new ListItemBag
                {
                    Disabled = representedDate < minDate || representedDate > maxDate,
                    Text = representedDate.ToString( dateTimeFormat ),
                    Value = representedDate.ToISO8601DateString()
                } );

                if ( StreakTypeService.IsBitSet( streakType, map, representedDate, out _ ) )
                {
                    bag.SelectedDates.Add( representedDate.ToISO8601DateString() );
                }
            }

            return bag;
        }

        /// <summary>
        /// Can the user edit the enrollment
        /// </summary>
        /// <returns></returns>
        private bool CanEdit(StreakTypeCache streakType)
        {
            return BlockCache.IsAuthorized( Authorization.ADMINISTRATE, RequestContext.CurrentPerson ) && streakType != null;
        }

        #endregion Methods

        #region Block Actions

        /// <summary>
        /// Refreshes the checkbox data with respect to the date range.
        /// </summary>
        /// <param name="delimitedDateValues">The date values to filter by</param>
        /// <returns></returns>
        [BlockAction]
        public BlockActionResult RefreshCheckboxData( string delimitedDateValues )
        {
            StreakMapEditorBag bag = GetCheckboxData( delimitedDateValues );

            if ( bag.ErrorMessage.IsNotNullOrWhiteSpace() )
            {
                return ActionBadRequest( bag.ErrorMessage );
            }

            return ActionOk( bag );
        }

        /// <summary>
        /// Saves the current record
        /// </summary>
        /// <param name="selectedDates">The selected checkbox dates</param>
        /// <param name="checkboxItems">All checkboxes that were within the date range</param>
        /// <returns></returns>
        [BlockAction]
        public BlockActionResult SaveRecord( List<string> selectedDates, List<ListItemBag> checkboxItems )
        {
            // Validate the streak type            
            var streakType = GetStreakType();

            if ( !CanEdit( streakType ) )
            {
                return ActionBadRequest( "You are not authorized." );
            }

            var map = GetTargetMap( out string mapTitle );
            var errorMessage = string.Empty;

            foreach ( ListItemBag item in checkboxItems )
            {
                var isSelected = selectedDates.Contains( item.Value );
                var representedDate = item.Value.AsDateTime();

                if ( representedDate.HasValue )
                {
                    map = StreakTypeService.SetBit( streakType, map, representedDate.Value, isSelected, out errorMessage );
                }
            }

            if ( errorMessage.IsNotNullOrWhiteSpace() )
            {
                return ActionBadRequest( errorMessage );
            }

            if ( IsTargetingEngagementMap() )
            {
                var isEngagementExclusionMap = GetAttributeValue( AttributeKey.IsEngagementExclusion ).AsBoolean();
                var enrollment = GetStreak();

                if ( isEngagementExclusionMap )
                {
                    RockContext.Entry( enrollment ).Property( s => s.ExclusionMap ).IsModified = true;
                    enrollment.ExclusionMap = map;
                }
                else
                {
                    RockContext.Entry( enrollment ).Property( s => s.EngagementMap ).IsModified = true;
                    enrollment.EngagementMap = map;
                }
            }
            else if ( IsTargetingExclusionMap() )
            {
                var exclusion = GetStreakTypeExclusion();
                RockContext.Entry( exclusion ).Property( s => s.ExclusionMap ).IsModified = true;
                exclusion.ExclusionMap = map;
            }
            else
            {
                var streakTypeEntity = new StreakTypeService( RockContext ).Get(streakType.Id);
                RockContext.Entry( streakTypeEntity ).Property( s => s.OccurrenceMap ).IsModified = true;
                streakTypeEntity.OccurrenceMap = map;
            }

            RockContext.SaveChanges();

            StreakMapEditorBag bag = new StreakMapEditorBag();

            if ( IsTargetingOccurrenceMap() )
            {
                var occurrenceTerm = IsStreakTypeDaily( streakType ) ? "today" : "this week";
                bag.SuccessMessage = string.Format( "Saved successfully. Please note that streak counts will not reflect these changes until the end of {0}.", occurrenceTerm );
            }
            else
            {
                bag.SuccessMessage = "Saved successfully.";
            }

            return ActionOk( bag );
        }

        #endregion Block Actions
    }
}
