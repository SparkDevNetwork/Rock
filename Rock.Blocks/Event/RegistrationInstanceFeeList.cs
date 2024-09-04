﻿// <copyright>
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

using Rock.Attribute;
using Rock.Data;
using Rock.Model;
using Rock.Obsidian.UI;
using Rock.ViewModels.Blocks;
using Rock.ViewModels.Blocks.Event.RegistrationInstanceFeeList;
using Rock.ViewModels.Utility;
using Rock.Web.UI;

namespace Rock.Blocks.Event
{
    /// <summary>
    /// Displays a list of registration registrant fees.
    /// </summary>
    [DisplayName( "Registration Instance - Fee List" )]
    [Category( "Event" )]
    [Description( "Displays the fees related to an event registration instance." )]
    [IconCssClass( "fa fa-list" )]
    // [SupportedSiteTypes( Model.SiteType.Web )]

    [Rock.SystemGuid.EntityTypeGuid( "10f4d211-fc60-40d5-b96b-6b9fcbdbefac" )]
    [Rock.SystemGuid.BlockTypeGuid( "dbcfb477-0553-4bae-bac9-2aec38e1da37" )]
    [CustomizedGrid]
    [ContextAware( typeof( RegistrationInstance ) )]
    public class RegistrationInstanceFeeList : RockEntityListBlockType<RegistrationRegistrantFee>
    {
        #region Keys

        private static class PageParameterKey
        {
            public const string RegistrationInstanceId = "RegistrationInstanceId";
        }

        private static class PreferenceKey
        {
            public const string FilterDateRange = "filter-date-range";

            public const string FilterFeeName = "filter-fee-name";

            public const string FilterFeeOptions = "filter-fee-options";
        }

        #endregion Keys

        #region Fields

        protected RegistrationInstance _registrationInstance;

        #endregion

        #region Properties

        protected string FilterDateRange => GetBlockPersonPreferences()
            .GetValue( PreferenceKey.FilterDateRange );

        protected Guid? FilterFeeName => GetBlockPersonPreferences()
            .GetValue( PreferenceKey.FilterFeeName )
            .AsGuidOrNull();

        protected List<Guid> FilterFeeOptions => GetBlockPersonPreferences()
            .GetValue( PreferenceKey.FilterFeeOptions )
            .FromJsonOrNull<List<Guid>>() ?? new List<Guid>();

        #endregion

        #region Methods

        /// <inheritdoc/>
        public override object GetObsidianBlockInitialization()
        {
            var box = new ListBlockBox<RegistrationInstanceFeeListOptionsBag>();
            var builder = GetGridBuilder();

            box.IsAddEnabled = false;
            box.IsDeleteEnabled = false;
            box.ExpectedRowCount = null;
            box.Options = GetBoxOptions();
            box.GridDefinition = builder.BuildDefinition();

            return box;
        }

        /// <summary>
        /// Gets the box options required for the component to render the list.
        /// </summary>
        /// <returns>The options that provide additional details to the block.</returns>
        private RegistrationInstanceFeeListOptionsBag GetBoxOptions()
        {
            var options = new RegistrationInstanceFeeListOptionsBag();
            var registrationInstance = GetRegistrationInstance();

            if ( registrationInstance != null )
            {
                options.ExportTitleName = registrationInstance.Name + " - Registration Fees";
                options.ExportFileName = registrationInstance.Name + "RegistrationFees";
                options.FeeNameItems = GetTemplateFees();
                options.FeeOptionsItems = GetTemplateFeeItems( FilterFeeName );
            }

            return options;
        }

        private List<ListItemBag> GetTemplateFees()
        {
            var instanceId = GetRegistrationInstance()?.Id;

            if ( !instanceId.HasValue )
            {
                return new List<ListItemBag>();
            }

            var registrationInstanceService = new RegistrationInstanceService( RockContext );
            var templateId = registrationInstanceService.Get( instanceId.Value ).RegistrationTemplateId;

            var templateFees = new RegistrationTemplateFeeService( RockContext ).Queryable()
                .Where( f => f.RegistrationTemplateId == templateId )
                .Select( f => new ListItemBag() { Text = f.Name, Value = f.Guid.ToString() } )
                .ToList();

            return templateFees;
        }

        private List<ListItemBag> GetTemplateFeeItems( Guid? templateFeeGuid )
        {
            var items = new List<ListItemBag>();
            var templateService = new RegistrationTemplateFeeItemService( RockContext );

            if ( templateFeeGuid.HasValue || FilterFeeOptions.Count > 0 )
            {
                var activeOptions = templateService.Queryable()
                    .Where( a => templateFeeGuid.HasValue ? a.RegistrationTemplateFee.Guid == templateFeeGuid.Value : FilterFeeOptions.Contains( a.Guid ) )
                    .Select( a => new ListItemBag() { Text = a.Name, Value = a.Guid.ToString() } );

                items.AddRange( activeOptions );
            }

            return items.ToList();
        }

        /// <inheritdoc/>
        protected override IQueryable<RegistrationRegistrantFee> GetListQueryable( RockContext rockContext )
        {
            var instanceId = GetRegistrationInstance()?.Id;
            IEnumerable<RegistrationRegistrantFee> registrationFees = Enumerable.Empty<RegistrationRegistrantFee>();

            if ( instanceId.HasValue )
            {
                registrationFees = new RegistrationRegistrantFeeService( rockContext ).Queryable()
                    .Include( a => a.RegistrationRegistrant.Registration )
                    .Include( a => a.RegistrationRegistrant.PersonAlias )
                    .Include( a => a.RegistrationTemplateFee )
                    .Include( a => a.RegistrationTemplateFeeItem )
                    .Where( a => a.RegistrationRegistrant.Registration.RegistrationInstanceId == instanceId );

                var dateRange = RockDateTimeHelper.CalculateDateRangeFromDelimitedValues( FilterDateRange, RockDateTime.Now );

                // Filter by Date Range
                if ( dateRange.Start.HasValue )
                {
                    registrationFees = registrationFees.Where( r => r.RegistrationRegistrant.Registration.CreatedDateTime >= dateRange.Start.Value );
                }

                if ( dateRange.End.HasValue )
                {
                    registrationFees = registrationFees.Where( r => r.RegistrationRegistrant.Registration.CreatedDateTime < dateRange.End.Value );
                }

                // Filter by Fee Name
                if ( FilterFeeName.HasValue )
                {
                    registrationFees = registrationFees.Where( r => r.RegistrationTemplateFee.Guid == FilterFeeName.Value );
                }

                // Filter by Fee Options
                if ( FilterFeeOptions.Count > 0 )
                {
                    registrationFees = registrationFees.Where( r => FilterFeeOptions.Contains( r.RegistrationTemplateFeeItem.Guid ) );
                }
            }

            return registrationFees.AsQueryable();
        }

        /// <inheritdoc/>
        protected override IQueryable<RegistrationRegistrantFee> GetOrderedListQueryable( IQueryable<RegistrationRegistrantFee> queryable, RockContext rockContext )
        {
            return queryable.OrderBy( r => r.CreatedDateTime );
        }

        /// <inheritdoc/>
        protected override GridBuilder<RegistrationRegistrantFee> GetGridBuilder()
        {
            return new GridBuilder<RegistrationRegistrantFee>()
                .WithBlock( this )
                .AddTextField( "idKey", a => a.IdKey )
                .AddTextField( "registrationId", a => a.RegistrationRegistrant.Registration.IdKey )
                .AddDateTimeField( "registrationDate", a => a.RegistrationRegistrant.Registration.CreatedDateTime ?? DateTime.MinValue )
                .AddTextField( "registeredBy", a => a.RegistrationRegistrant.Registration.FirstName + " " + a.RegistrationRegistrant.Registration.LastName )
                .AddTextField( "registrant", a => a.RegistrationRegistrant.PersonAlias.Person.FullName )
                .AddField( "registrantId", a => a.RegistrationRegistrant.IdKey )
                .AddTextField( "feeName", a => a.RegistrationTemplateFee.Name )
                .AddTextField( "feeItemName", a => a.Option )
                .AddField( "cost", a => a.Cost )
                .AddField( "quantity", a => a.Quantity )
                .AddField( "feeTotal", a => a.Quantity * a.Cost );
        }

        /// <summary>
        /// Gets the registration instance.
        /// </summary>
        /// <returns></returns>
        private RegistrationInstance GetRegistrationInstance()
        {
            _registrationInstance = RequestContext.GetContextEntity<RegistrationInstance>();

            if ( _registrationInstance == null )
            {
                var instanceId = PageParameter( PageParameterKey.RegistrationInstanceId ).AsIntegerOrNull();

                if ( instanceId.HasValue )
                {
                    _registrationInstance = new RegistrationInstanceService( RockContext ).Get( instanceId.Value );
                }
            }

            return _registrationInstance;
        }

        #endregion

        #region Block Actions

        /// <summary>
        /// ets the Template Fee Items for the selected template fees.
        /// </summary>
        /// <param name="templateFeeGuid">The template fee unique identifier.</param>
        /// <returns></returns>
        [BlockAction]
        public BlockActionResult FeeItems( Guid? templateFeeGuid )
        {
            var items = GetTemplateFeeItems( templateFeeGuid );
            return ActionOk( items );
        }

        #endregion 
    }
}
