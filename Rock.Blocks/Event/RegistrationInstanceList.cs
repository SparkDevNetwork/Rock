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
using Rock.Security;
using Rock.Utility;
using Rock.ViewModels.Blocks;
using Rock.ViewModels.Blocks.Event.RegistrationInstanceList;
using Rock.Web.Cache;

namespace Rock.Blocks.Event
{
    /// <summary>
    /// Displays a list of registration instances.
    /// </summary>
    [DisplayName( "Registration Instance List" )]
    [Category( "Event" )]
    [Description( "Displays a list of registration instances." )]
    [IconCssClass( "fa fa-list" )]
    // [SupportedSiteTypes( Model.SiteType.Web )]

    [LinkedPage( "Detail Page",
        Description = "The page that will show the registration instance details.",
        Key = AttributeKey.DetailPage )]
        
    [Rock.SystemGuid.EntityTypeGuid( "5cc98267-2b3c-45ef-9055-31db629d579b" )]
    [Rock.SystemGuid.BlockTypeGuid( "051f65ad-9301-4d41-bd5e-d4e93f4dc438" )]
    [CustomizedGrid]
    public class RegistrationInstanceList : RockEntityListBlockType<RegistrationInstance>
    {
        #region Keys

        private static class AttributeKey
        {
            public const string DetailPage = "DetailPage";
        }

        private static class NavigationUrlKey
        {
            public const string DetailPage = "DetailPage";
        }

        #endregion Keys

        #region Methods

        /// <inheritdoc/>
        public override object GetObsidianBlockInitialization()
        {
            var box = new ListBlockBox<RegistrationInstanceListOptionsBag>();
            var builder = GetGridBuilder();

            box.IsAddEnabled = GetIsAddEnabled();
            box.IsDeleteEnabled = true;
            box.ExpectedRowCount = null;
            box.NavigationUrls = GetBoxNavigationUrls();
            box.Options = GetBoxOptions();
            box.GridDefinition = builder.BuildDefinition();

            return box;
        }

        /// <summary>
        /// Gets the box options required for the component to render the list.
        /// </summary>
        /// <returns>The options that provide additional details to the block.</returns>
        private RegistrationInstanceListOptionsBag GetBoxOptions()
        {
            var options = new RegistrationInstanceListOptionsBag();

            var templateId = PageParameter( "RegistrationTemplateId" ).AsInteger();
            if ( templateId != 0 )
            {
                var template = new RegistrationTemplateService( RockContext ).Get( templateId );
                if ( template != null )
                {
                    options.RegistrationInstanceName = template.Name;
                    options.ShowWaitList = template.WaitListEnabled;
                    options.ShowDetailsColumn = false;
                }
            }
            else
            {
                options.RegistrationInstanceName = "Active Registration";
                options.ShowDetailsColumn = true;
            }

            return options;
        }

        /// <summary>
        /// Determines if the add button should be enabled in the grid.
        /// <summary>
        /// <returns>A boolean value that indicates if the add button should be enabled.</returns>
        private bool GetIsAddEnabled()
        {
            var templateId = PageParameter( "RegistrationTemplateId" ).AsInteger();
            if ( templateId == 0 )
            {
                return false;
            }

            var entity = new RegistrationInstance();
            return entity.IsAuthorized( Authorization.EDIT, RequestContext.CurrentPerson );
        }

        /// <summary>
        /// Gets the box navigation URLs required for the page to operate.
        /// </summary>
        /// <returns>A dictionary of key names and URL values.</returns>
        private Dictionary<string, string> GetBoxNavigationUrls()
        {
            var templateId = PageParameter( "RegistrationTemplateId" ).AsInteger();
            return new Dictionary<string, string>
            {
                [NavigationUrlKey.DetailPage] = this.GetLinkedPageUrl( AttributeKey.DetailPage, new Dictionary<string, string>
                {
                    ["RegistrationInstanceId"] = "((Key))",
                    ["RegistrationTemplateId"] = templateId.ToString()
                })
            };
        }

        /// <inheritdoc/>
        protected override IQueryable<RegistrationInstance> GetListQueryable( RockContext rockContext )
        {
            var qry = base.GetListQueryable( rockContext );
            qry = qry.Include( i => i.Registrations.Select( r => r.Registrants ) );

            var templateId = PageParameter( "RegistrationTemplateId" ).AsInteger();
            if ( templateId != 0 )
            {
                qry = qry.Where( i => i.RegistrationTemplateId == templateId );
            }
            else
            {
                qry = qry.Where( i => i.IsActive );
            }

            return qry;
        }

        /// <inheritdoc/>
        protected override IQueryable<RegistrationInstance> GetOrderedListQueryable( IQueryable<RegistrationInstance> queryable, RockContext rockContext )
        {
            return queryable.OrderByDescending( a => a.StartDateTime );
        }

        /// <inheritdoc/>
        protected override GridBuilder<RegistrationInstance> GetGridBuilder()
        {
            var templateId = PageParameter( "RegistrationTemplateId" ).AsInteger();
            bool showWaitList = false;

            if ( templateId != 0 )
            {
                var template = new RegistrationTemplateService( RockContext ).Get( templateId );
                showWaitList = template != null && template.WaitListEnabled;
            }

            var builder = new GridBuilder<RegistrationInstance>()
                .WithBlock( this )
                .AddTextField( "idKey", a => a.IdKey )
                .AddTextField( "name", a => a.Name )
                .AddDateTimeField( "startDate", a => a.StartDateTime )
                .AddDateTimeField( "endDate", a => a.EndDateTime )
                .AddField( "details", a => a.Details )
                .AddField( "isActive", a => a.IsActive )
                .AddField( "registrants", a => a.Registrations.Where( r => !r.IsTemporary ).SelectMany( r => r.Registrants ).Count( r => !r.OnWaitList ) )
                .AddField( "waitList", a => a.Registrations.Where( r => !r.IsTemporary ).SelectMany( r => r.Registrants ).Count( r => r.OnWaitList ) )
                .AddField( "hasPaymentPlans", a => a.Registrations.Any( r => r.PaymentPlanFinancialScheduledTransaction != null && r.PaymentPlanFinancialScheduledTransaction.IsActive ) )
                .AddField( "isSecurityDisabled", a => !a.IsAuthorized( Authorization.ADMINISTRATE, RequestContext.CurrentPerson ) )
                .AddAttributeFields( GetGridAttributes() );

            return builder;
        }

        #endregion

        #region Block Actions

        /// <summary>
        /// Deletes the specified entity.
        /// </summary>
        /// <param name="key">The identifier of the entity to be deleted.</param>
        /// <returns>An empty result that indicates if the operation succeeded.</returns>
        [BlockAction]
        public BlockActionResult Delete( string key )
        {
            var entityService = new RegistrationInstanceService( RockContext );
            var registrationInstance = entityService.Get( key, !PageCache.Layout.Site.DisablePredictableIds );

            if ( registrationInstance == null )
            {
                return ActionBadRequest( $"{RegistrationInstance.FriendlyTypeName} not found." );
            }

            if ( !registrationInstance.IsAuthorized( Authorization.EDIT, RequestContext.CurrentPerson ) )
            {
                return ActionBadRequest( $"Not authorized to delete {RegistrationInstance.FriendlyTypeName}." );
            }

            if ( !entityService.CanDelete( registrationInstance, out var errorMessage ) )
            {
                return ActionBadRequest( errorMessage );
            }
            
            var registrationService = new RegistrationService( RockContext );
            var financialScheduledTransactionService = new FinancialScheduledTransactionService( RockContext );
            var errors = new List<string>();
            var warnings = new List<string>();

            foreach ( var registration in registrationInstance.Registrations.ToList() )
            {
                var success = registrationService.TryCancelPaymentPlan( registration, financialScheduledTransactionService, out var error, out var warning );
                string registrationInfo = $"Registration Id {registration.Id} ({registration.FirstName} {registration.LastName})";
                if ( !success )
                {
                    errors.Add( $"{registrationInfo}: {error ?? "Unknown error"}" );
                }
                if ( !string.IsNullOrWhiteSpace( warning ) )
                {
                    warnings.Add( $"{registrationInfo}: {warning}" );
                }
            }

            if ( errors.Any() )
            {
                return ActionBadRequest( "The following registrations could not have their payment plans canceled:\n" + string.Join( "\n", errors ) );
            }
            if ( warnings.Any() )
            {
                return ActionBadRequest( "Warnings occurred for the following registrations:\n" + string.Join( "\n", warnings ) );
            }

            RockContext.SaveChanges();

            RockContext.WrapTransaction( () =>
            {
                registrationService.DeleteRange( registrationInstance.Registrations );
                entityService.Delete( registrationInstance );
                RockContext.SaveChanges();
            } );

            return ActionOk();
        }

        #endregion
    }
}
