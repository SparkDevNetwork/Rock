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

        #endregion Keys

        #region Fields

        protected RegistrationInstance _registrationInstance;

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
            }

            return options;
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
    }
}
