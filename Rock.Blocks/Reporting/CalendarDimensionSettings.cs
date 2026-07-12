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

using Rock.Attribute;
using Rock.Model;
using Rock.SystemKey;
using Rock.ViewModels.Blocks.Reporting.CalendarDimensionSettings;

namespace Rock.Blocks.Reporting
{
    [DisplayName( "Calendar Dimension Settings" )]
    [Category( "Reporting" )]
    [Description( "Helps configure and generate the AnalyticsSourceDate table for BI Analytics" )]
    [IconCssClass( "ti ti-calendar" )]
    [SupportedSiteTypes( Model.SiteType.Web )]

    [SystemGuid.EntityTypeGuid( "df6324dc-1e42-4556-b337-d4e416868a01" )]
    //WAS [Rock.SystemGuid.BlockTypeGuid( "a4c6b9df-5034-40f6-958a-342921142ffb" )]
    [Rock.SystemGuid.BlockTypeGuid( "7711EAE9-5CF0-46E4-A4E6-26C05A71FE43" )]
    internal class CalendarDimensionSettings : RockBlockType
    {
        #region Methods

        /// <inheritdoc/>
        public override object GetObsidianBlockInitialization()
        {
            var bag = new CalendarDimensionSettingsBag
            {
                StartDate = Rock.Web.SystemSettings.GetValue( SystemSetting.ANALYTICS_CALENDAR_DIMENSION_START_DATE ).AsDateTime()
                    ?? new DateTime( RockDateTime.Today.AddYears( -150 ).Year, 1, 1 ),
                EndDate = Rock.Web.SystemSettings.GetValue( SystemSetting.ANALYTICS_CALENDAR_DIMENSION_END_DATE ).AsDateTime()
                    ?? new DateTime( RockDateTime.Today.AddYears( 101 ).Year, 1, 1 ).AddDays( -1 ),
                FiscalStartMonth = Rock.Web.SystemSettings.GetValue( SystemSetting.ANALYTICS_CALENDAR_DIMENSION_FISCAL_START_MONTH ).AsIntegerOrNull() ?? 1,
                IsGivingMonthUseSundayDate = Rock.Web.SystemSettings.GetValue( SystemSetting.ANALYTICS_CALENDAR_DIMENSION_GIVING_MONTH_USE_SUNDAY_DATE ).AsBoolean()
            };

            return bag;
        }

        #endregion

        #region Block Actions

        /// <summary>
        /// Validates, saves, and regenerates the AnalyticsSourceDate table using the provided settings.
        /// </summary>
        /// <param name="bag">The settings to apply.</param>
        /// <returns>The saved settings bag, or a bad request result if validation fails.</returns>
        [BlockAction]
        public BlockActionResult GenerateDimension( CalendarDimensionSettingsBag bag )
        {
            if ( bag.StartDate == null || bag.EndDate == null )
            {
                return ActionBadRequest( "Start Date and End Date are required." );
            }

            var maximumStartDate = RockDateTime.Now.AddYears( -120 ).Date;

            if ( bag.StartDate.Value > maximumStartDate )
            {
                return ActionBadRequest( $"Start Date must be at least 120 years before today ({maximumStartDate:d} or earlier)." );
            }

            if ( bag.EndDate.Value <= bag.StartDate.Value )
            {
                return ActionBadRequest( "End Date must be after Start Date." );
            }

            if ( bag.FiscalStartMonth < 1 || bag.FiscalStartMonth > 12 )
            {
                return ActionBadRequest( "Fiscal Start Month must be a valid Month." );
            }

            Rock.Web.SystemSettings.SetValue( SystemSetting.ANALYTICS_CALENDAR_DIMENSION_START_DATE, bag.StartDate.Value.ToString( "o" ) );
            Rock.Web.SystemSettings.SetValue( SystemSetting.ANALYTICS_CALENDAR_DIMENSION_END_DATE, bag.EndDate.Value.ToString( "o" ) );
            Rock.Web.SystemSettings.SetValue( SystemSetting.ANALYTICS_CALENDAR_DIMENSION_FISCAL_START_MONTH, bag.FiscalStartMonth.ToString() );
            Rock.Web.SystemSettings.SetValue( SystemSetting.ANALYTICS_CALENDAR_DIMENSION_GIVING_MONTH_USE_SUNDAY_DATE, bag.IsGivingMonthUseSundayDate.ToString() );

            AnalyticsSourceDate.GenerateAnalyticsSourceDateData( bag.FiscalStartMonth, bag.IsGivingMonthUseSundayDate, bag.StartDate.Value, bag.EndDate.Value );

            return ActionOk( bag );
        }

        #endregion
    }
}
