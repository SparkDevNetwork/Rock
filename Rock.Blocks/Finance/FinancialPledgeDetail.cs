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

using Rock.Attribute;
using Rock.Constants;
using Rock.Data;
using Rock.Model;
using Rock.ViewModels.Blocks;
using Rock.ViewModels.Blocks.Finance.FinancialPledgeDetail;
using Rock.Web.Cache;
using Rock.Web.UI;
using Rock.Web.UI.Controls;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace Rock.Blocks.Finance
{
    /// <summary>
    /// Displays the details of a particular financial pledge.
    /// </summary>
    /// <seealso cref="Rock.Blocks.RockObsidianDetailBlockType" />

    [DisplayName( "Pledge Entry" )]
    [Category( "Finance" )]
    [Description( "Allows a website visitor to create pledge for the configured accounts, start and end date. This block also creates a new person record if a matching person could not be found." )]
    [IconCssClass( "fa fa-question" )]

    #region Block Attributes

    [BooleanField( "Enable Smart Names",
        Key = AttributeKey.EnableSmartNames,
        Description = "Check the first name for 'and' and '&' and split it to just use the first name provided.",
        DefaultBooleanValue = true,
        Order = 1 )]

    [AccountField( "Account",
        Key = AttributeKey.Account,
        Description = "The account that new pledges will be allocated toward.",
        IsRequired = true,
        DefaultValue = Rock.SystemGuid.FinancialAccount.GENERAL_FUND,
        Category = "",
        Order = 2 )]

    [DefinedValueField( "New Connection Status",
        Key = AttributeKey.NewConnectionStatus,
        DefinedTypeGuid = Rock.SystemGuid.DefinedType.PERSON_CONNECTION_STATUS,
        Description = "Person connection status to assign to a new user.",
        IsRequired = true,
        AllowMultiple = false,
        DefaultValue = Rock.SystemGuid.DefinedValue.PERSON_CONNECTION_STATUS_PARTICIPANT,
        Order = 3 )]

    [DateRangeField( "Pledge Date Range",
        Key = AttributeKey.PledgeDateRange,
        Description = "Date range of the pledge.",
        IsRequired = false,
        Order = 4 )]

    [BooleanField( "Show Pledge Frequency",
        Key = AttributeKey.ShowPledgeFrequency,
        Description = "Show the pledge frequency option to the user.",
        DefaultValue = "false",
        Order = 5 )]

    [BooleanField( "Require Pledge Frequency",
        Key = AttributeKey.RequirePledgeFrequency,
        Description = "Require that a user select a specific pledge frequency (when pledge frequency is shown).",
        DefaultValue = "false",
        Order = 6 )]

    [TextField( "Save Button Text",
        Key = AttributeKey.SaveButtonText,
        Description = "The Text to shown on the Save button",
        IsRequired = true,
        DefaultValue = "Save",
        Order = 7 )]

    [TextField( "Note Message",
        Key = AttributeKey.NoteMessage,
        Description = "Message to show at the bottom of the create pledge block.",
        IsRequired = false,
        DefaultValue = "Note: This commitment is a statement of intent and may be changed as your circumstances change.",
        Order = 8 )]

    [CodeEditorField( "Receipt Text",
        Key = AttributeKey.ReceiptText,
        Description = "The text (or HTML) to display as the pledge receipt. <span class='tip tip-lava'></span> <span class='tip tip-html'>",
        EditorMode = CodeEditorMode.Lava,
        EditorTheme = CodeEditorTheme.Rock,
        EditorHeight = 200,
        Order = 9,
        DefaultValue =
        @"
<h1>Thank You!</h1>
<p>
{{Person.NickName}}, thank you for your commitment of ${{FinancialPledge.TotalAmount}} to {{Account.Name}}.  To make your commitment even easier, you might consider making a scheduled giving profile.
</p>
<p>
    <a href='~/page/186?PledgeId={{ FinancialPledge.Id }}' class='btn btn-default' >Setup a Giving Profile</a>
</p>
" )]

    [SystemCommunicationField( "Confirmation Email Template",
        Key = AttributeKey.ConfirmationEmailTemplate,
        Description = "Email template to use after submitting a new pledge. Leave blank to not send an email.",
        IsRequired = false,
        DefaultSystemCommunicationGuid = "",
        Order = 10 )]

    [GroupTypeField( "Select Group Type",
        Key = AttributeKey.SelectGroupType,
        Description = "Optional Group Type that if selected will display a selection of groups that current user belongs to that can then be associated with the pledge.",
        IsRequired = false,
        DefaultValue = "",
        Category = "",
        Order = 11 )]

    [TextField( "Pledge Term",
        Key = AttributeKey.PledgeTerm,
        Description = "The Text to display as the pledge term on the pledge amount input label.",
        IsRequired = false,
        DefaultValue = "Pledge",
        Order = 12 )]

    #endregion

    [Rock.SystemGuid.EntityTypeGuid( "fad28407-5128-4ddb-9c1c-a0c2233f3e73" )]
    [Rock.SystemGuid.BlockTypeGuid( "0455ecbd-d54d-4485-bf4d-f469048ae10f" )]
    public class FinancialPledgeDetail : RockObsidianDetailBlockType
    {
        #region Keys

        private static class PageParameterKey
        {
            public const string FinancialPledgeId = "FinancialPledgeId";
        }

        private static class NavigationUrlKey
        {
            public const string ParentPage = "ParentPage";
        }

        private static class AttributeKey
        {
            public const string EnableSmartNames = "EnableSmartNames";
            public const string Account = "Account";
            public const string NewConnectionStatus = "NewConnectionStatus";
            public const string PledgeDateRange = "PledgeDateRange";
            public const string ShowPledgeFrequency = "ShowPledgeFrequency";
            public const string RequirePledgeFrequency = "RequirePledgeFrequency";
            public const string SaveButtonText = "SaveButtonText";
            public const string NoteMessage = "NoteMessage";
            public const string ReceiptText = "ReceiptText";
            public const string ConfirmationEmailTemplate = "ConfirmationEmailTemplate";
            public const string SelectGroupType = "SelectGroupType";
            public const string PledgeTerm = "PledgeTerm";
        }

        private const string btnConfirm = "Confirm";

        #endregion Keys

        public override string BlockFileUrl => $"{base.BlockFileUrl}.obs";

        #region Methods

        /// <inheritdoc/>
        public override object GetObsidianBlockInitialization()
        {
            using ( var rockContext = new RockContext() )
            {
                var box = new DetailBlockBox<FinancialPledgeBag, FinancialPledgeDetailOptionsBag>();

                SetBoxInitialEntityState( box, rockContext );

                box.NavigationUrls = GetBoxNavigationUrls();
                box.Options = GetBoxOptions( box.IsEditable, rockContext );
                box.QualifiedAttributeProperties = GetAttributeQualifiedColumns<FinancialPledge>();

                return box;
            }
        }

        /// <summary>
        /// Gets the box options required for the component to render the view
        /// or edit the entity.
        /// </summary>
        /// <param name="isEditable"><c>true</c> if the entity is editable; otherwise <c>false</c>.</param>
        /// <param name="rockContext">The rock context.</param>
        /// <returns>The options that provide additional details to the block.</returns>
        private FinancialPledgeDetailOptionsBag GetBoxOptions( bool isEditable, RockContext rockContext )
        {
            var dateRange = GetAttributeValue( AttributeKey.PledgeDateRange );
            var dateRangePicker = new DateRangePicker() { DelimitedValues = dateRange };

            var options = new FinancialPledgeDetailOptionsBag()
            {
                SelectGroupTypeGuid = GetAttributeValue( AttributeKey.SelectGroupType ).AsGuidOrNull(),
                NoteMessage = GetAttributeValue( AttributeKey.NoteMessage ),
                SaveButtonText = GetAttributeValue( AttributeKey.SaveButtonText ),
                ShowPledgeFrequency = GetAttributeValue( AttributeKey.ShowPledgeFrequency ).AsBoolean(),
                RequirePledgeFrequency = GetAttributeValue( AttributeKey.RequirePledgeFrequency ).AsBoolean(),
                ShowDateRange = dateRangePicker.LowerValue == null || dateRangePicker.UpperValue == null
            };

            return options;
        }

        /// <summary>
        /// Validates the FinancialPledge for any final information that might not be
        /// valid after storing all the data from the client.
        /// </summary>
        /// <param name="financialPledge">The FinancialPledge to be validated.</param>
        /// <param name="rockContext">The rock context.</param>
        /// <param name="errorMessage">On <c>false</c> return, contains the error message.</param>
        /// <returns><c>true</c> if the FinancialPledge is valid, <c>false</c> otherwise.</returns>
        private bool ValidateFinancialPledge( FinancialPledge financialPledge, RockContext rockContext, out string errorMessage )
        {
            errorMessage = null;

            if ( !financialPledge.IsValid )
            {
                errorMessage = string.Format( "Please correct the following:{0}", financialPledge.ValidationResults.AsDelimited( " ," ) );
                return false;
            }

            return true;
        }

        /// <summary>
        /// Sets the initial entity state of the box. Populates the Entity or
        /// ErrorMessage properties depending on the entity and permissions.
        /// </summary>
        /// <param name="box">The box to be populated.</param>
        /// <param name="rockContext">The rock context.</param>
        private void SetBoxInitialEntityState( DetailBlockBox<FinancialPledgeBag, FinancialPledgeDetailOptionsBag> box, RockContext rockContext )
        {
            var entity = GetInitialEntity( rockContext );

            if ( entity == null )
            {
                box.ErrorMessage = $"The {FinancialPledge.FriendlyTypeName} was not found.";
                return;
            }

            var isViewable = entity.IsAuthorized( Rock.Security.Authorization.VIEW, RequestContext.CurrentPerson );
            box.IsEditable = entity.IsAuthorized( Rock.Security.Authorization.EDIT, RequestContext.CurrentPerson );

            entity.LoadAttributes( rockContext );

            if ( entity.Id != 0 )
            {
                // Existing entity was found, prepare for view mode by default.
                if ( isViewable )
                {
                    box.Entity = GetEntityBagForView( entity );
                    box.SecurityGrantToken = GetSecurityGrantToken( entity );
                }
                else
                {
                    box.ErrorMessage = EditModeMessage.NotAuthorizedToView( FinancialPledge.FriendlyTypeName );
                }
            }
            else
            {
                // New entity is being created, prepare for edit mode by default.
                if ( box.IsEditable )
                {
                    box.Entity = GetEntityBagForEdit( entity );
                    box.SecurityGrantToken = GetSecurityGrantToken( entity );
                }
                else
                {
                    box.ErrorMessage = EditModeMessage.NotAuthorizedToEdit( FinancialPledge.FriendlyTypeName );
                }
            }
        }

        /// <summary>
        /// Gets the entity bag that is common between both view and edit modes.
        /// </summary>
        /// <param name="entity">The entity to be represented as a bag.</param>
        /// <returns>A <see cref="FinancialPledgeBag"/> that represents the entity.</returns>Sni
        private FinancialPledgeBag GetCommonEntityBag( FinancialPledge entity )
        {
            if ( entity == null )
            {
                return null;
            }

            var entityBag = new FinancialPledgeBag
            {
                IdKey = entity.IdKey,
                Group = entity.Group.ToListItemBag(),
                CurrentPerson = RequestContext.CurrentPerson.ToListItemBag(),
                PersonAlias = entity.PersonAlias.ToListItemBag(),
                PledgeFrequencyValue = entity.PledgeFrequencyValue.ToListItemBag(),
                TotalAmount = entity.TotalAmount
            };

            if ( entity.StartDate != DateTime.MinValue )
            {
                entityBag.StartDate = entity.StartDate;
            }

            if ( entity.EndDate != DateTime.MinValue )
            {
                entityBag.EndDate = entity.EndDate;
            }

            return entityBag;
        }

        /// <summary>
        /// Gets the bag for viewing the specified entity.
        /// </summary>
        /// <param name="entity">The entity to be represented for view purposes.</param>
        /// <returns>A <see cref="FinancialPledgeBag"/> that represents the entity.</returns>
        private FinancialPledgeBag GetEntityBagForView( FinancialPledge entity )
        {
            if ( entity == null )
            {
                return null;
            }

            var bag = GetCommonEntityBag( entity );

            bag.LoadAttributesAndValuesForPublicView( entity, RequestContext.CurrentPerson );

            return bag;
        }

        /// <summary>
        /// Gets the bag for editing the specified entity.
        /// </summary>
        /// <param name="entity">The entity to be represented for edit purposes.</param>
        /// <returns>A <see cref="FinancialPledgeBag"/> that represents the entity.</returns>
        private FinancialPledgeBag GetEntityBagForEdit( FinancialPledge entity )
        {
            if ( entity == null )
            {
                return null;
            }

            var bag = GetCommonEntityBag( entity );

            bag.LoadAttributesAndValuesForPublicEdit( entity, RequestContext.CurrentPerson );

            return bag;
        }

        /// <summary>
        /// Updates the entity from the data in the save box.
        /// </summary>
        /// <param name="entity">The entity to be updated.</param>
        /// <param name="box">The box containing the information to be updated.</param>
        /// <param name="rockContext">The rock context.</param>
        /// <returns><c>true</c> if the box was valid and the entity was updated, <c>false</c> otherwise.</returns>
        private bool UpdateEntityFromBox( FinancialPledge entity, DetailBlockBox<FinancialPledgeBag, FinancialPledgeDetailOptionsBag> box, RockContext rockContext )
        {
            if ( box.ValidProperties == null )
            {
                return false;
            }

            var isNew = entity.Id == 0;

            box.IfValidProperty( nameof( box.Entity.PersonAlias ),
                () => entity.PersonAliasId = box.Entity.PersonAlias.GetEntityId<PersonAlias>( rockContext ) );

            box.IfValidProperty( nameof( box.Entity.EndDate ),
                () => entity.EndDate = box.Entity.EndDate ?? DateTime.MinValue );

            box.IfValidProperty( nameof( box.Entity.Group ),
                () => entity.GroupId = box.Entity.Group.GetEntityId<Group>( rockContext ) );

            box.IfValidProperty( nameof( box.Entity.PledgeFrequencyValue ),
                () => entity.PledgeFrequencyValueId = box.Entity.PledgeFrequencyValue.GetEntityId<DefinedValue>( rockContext ) );

            box.IfValidProperty( nameof( box.Entity.StartDate ),
                () => entity.StartDate = box.Entity.StartDate ?? DateTime.MinValue );

            box.IfValidProperty( nameof( box.Entity.TotalAmount ),
                () => entity.TotalAmount = box.Entity.TotalAmount );

            if ( !isNew )
            {
                box.IfValidProperty( nameof( box.Entity.Account ),
                () => entity.GroupId = box.Entity.Account.GetEntityId<FinancialAccount>( rockContext ) );
            }

            box.IfValidProperty( nameof( box.Entity.AttributeValues ),
                () =>
                {
                    entity.LoadAttributes( rockContext );

                    entity.SetPublicAttributeValues( box.Entity.AttributeValues, RequestContext.CurrentPerson );
                } );

            return true;
        }

        /// <summary>
        /// Gets the initial entity from page parameters or creates a new entity
        /// if page parameters requested creation.
        /// </summary>
        /// <param name="rockContext">The rock context.</param>
        /// <returns>The <see cref="FinancialPledge"/> to be viewed or edited on the page.</returns>
        private FinancialPledge GetInitialEntity( RockContext rockContext )
        {
            return GetInitialEntity<FinancialPledge, FinancialPledgeService>( rockContext, PageParameterKey.FinancialPledgeId );
        }

        /// <summary>
        /// Gets the box navigation URLs required for the page to operate.
        /// </summary>
        /// <returns>A dictionary of key names and URL values.</returns>
        private Dictionary<string, string> GetBoxNavigationUrls()
        {
            return new Dictionary<string, string>
            {
                [NavigationUrlKey.ParentPage] = this.GetParentPageUrl()
            };
        }

        /// <inheritdoc/>
        protected override string RenewSecurityGrantToken()
        {
            using ( var rockContext = new RockContext() )
            {
                var entity = GetInitialEntity( rockContext );

                if ( entity != null )
                {
                    entity.LoadAttributes( rockContext );
                }

                return GetSecurityGrantToken( entity );
            }
        }

        /// <summary>
        /// Gets the security grant token that will be used by UI controls on
        /// this block to ensure they have the proper permissions.
        /// </summary>
        /// <returns>A string that represents the security grant token.</string>
        private string GetSecurityGrantToken( FinancialPledge entity )
        {
            var securityGrant = new Rock.Security.SecurityGrant();

            securityGrant.AddRulesForAttributes( entity, RequestContext.CurrentPerson );

            return securityGrant.ToToken();
        }

        /// <summary>
        /// Attempts to load an entity to be used for an edit action.
        /// </summary>
        /// <param name="idKey">The identifier key of the entity to load.</param>
        /// <param name="rockContext">The database context to load the entity from.</param>
        /// <param name="entity">Contains the entity that was loaded when <c>true</c> is returned.</param>
        /// <param name="error">Contains the action error result when <c>false</c> is returned.</param>
        /// <returns><c>true</c> if the entity was loaded and passed security checks.</returns>
        private bool TryGetEntityForEditAction( string idKey, RockContext rockContext, out FinancialPledge entity, out BlockActionResult error )
        {
            var entityService = new FinancialPledgeService( rockContext );
            error = null;

            // Determine if we are editing an existing entity or creating a new one.
            if ( idKey.IsNotNullOrWhiteSpace() )
            {
                // If editing an existing entity then load it and make sure it
                // was found and can still be edited.
                entity = entityService.Get( idKey, !PageCache.Layout.Site.DisablePredictableIds );
            }
            else
            {
                // Create a new entity.
                entity = new FinancialPledge();
                entityService.Add( entity );
            }

            if ( entity == null )
            {
                error = ActionBadRequest( $"{FinancialPledge.FriendlyTypeName} not found." );
                return false;
            }

            if ( !entity.IsAuthorized( Rock.Security.Authorization.EDIT, RequestContext.CurrentPerson ) )
            {
                error = ActionBadRequest( $"Not authorized to edit ${FinancialPledge.FriendlyTypeName}." );
                return false;
            }

            return true;
        }

        /// <summary>
        /// Finds the person if they're logged in, or by email and name. If not exactly one found, creates a new person (and family)
        /// </summary>
        /// <param name="rockContext">The rock context.</param>
        /// <param name="entityBag">The entity bag.</param>
        /// <returns></returns>
        private Person FindPerson( RockContext rockContext, FinancialPledgeBag entityBag )
        {
            Person person;
            var personService = new PersonService( rockContext );

            if ( !string.IsNullOrWhiteSpace( entityBag.IdKey ) )
            {
                person = personService.GetQueryableByKey( entityBag.IdKey ).FirstOrDefault();
            }
            else
            {
                if ( RequestContext.CurrentPerson != null )
                {
                    person = RequestContext.CurrentPerson;
                }
                else
                {
                    var firstName = entityBag.FirstName;
                    if ( GetAttributeValue( AttributeKey.EnableSmartNames ).AsBooleanOrNull() ?? true )
                    {
                        // If they tried to specify first name as multiple first names, like "Steve and Judy" or "Bob & Sally", just take the first first name
                        var parts = firstName.Split( new string[] { " and ", " & " }, StringSplitOptions.RemoveEmptyEntries );
                        if ( parts.Length > 0 )
                        {
                            firstName = parts[0];
                        }
                    }

                    // Same logic as TransactionEntry.ascx.cs
                    var personQuery = new PersonService.PersonMatchQuery( firstName, entityBag.LastName, entityBag.Email, string.Empty );
                    person = personService.FindPerson( personQuery, true );
                }

                if ( person == null )
                {
                    var definedValue = DefinedValueCache.Get( GetAttributeValue( "NewConnectionStatus" ).AsGuidOrNull() ?? Rock.SystemGuid.DefinedValue.PERSON_CONNECTION_STATUS_PARTICIPANT.AsGuid() );
                    person = new Person
                    {
                        FirstName = entityBag.FirstName,
                        LastName = entityBag.LastName,
                        Email = entityBag.Email,
                        EmailPreference = Rock.Model.EmailPreference.EmailAllowed,
                        ConnectionStatusValueId = definedValue.Id,
                    };

                    person.IsSystem = false;
                    person.RecordTypeValueId = DefinedValueCache.Get( Rock.SystemGuid.DefinedValue.PERSON_RECORD_TYPE_PERSON.AsGuid() ).Id;
                    person.RecordStatusValueId = DefinedValueCache.Get( Rock.SystemGuid.DefinedValue.PERSON_RECORD_STATUS_PENDING.AsGuid() ).Id;

                    PersonService.SaveNewPerson( person, rockContext, null, false );
                }
            }

            return person;
        }

        /// <summary>
        /// Gets the financial account.
        /// </summary>
        /// <param name="rockContext">The rock context.</param>
        /// <returns></returns>
        public FinancialAccount GetFinancialAccount( RockContext rockContext )
        {
            var financialAccountService = new FinancialAccountService( rockContext );
            var financialAccount = financialAccountService.Get( GetAttributeValue( AttributeKey.Account ).AsGuid() );
            return financialAccount;
        }

        #endregion

        #region Block Actions

        /// <summary>
        /// Gets the box that will contain all the information needed to begin
        /// the edit operation.
        /// </summary>
        /// <param name="key">The identifier of the entity to be edited.</param>
        /// <returns>A box that contains the entity and any other information required.</returns>
        [BlockAction]
        public BlockActionResult Edit( string key )
        {
            using ( var rockContext = new RockContext() )
            {
                if ( !TryGetEntityForEditAction( key, rockContext, out var entity, out var actionError ) )
                {
                    return actionError;
                }

                entity.LoadAttributes( rockContext );

                var box = new DetailBlockBox<FinancialPledgeBag, FinancialPledgeDetailOptionsBag>
                {
                    Entity = GetEntityBagForEdit( entity )
                };

                return ActionOk( box );
            }
        }

        /// <summary>
        /// Saves the entity contained in the box.
        /// </summary>
        /// <param name="box">The box that contains all the information required to save.</param>
        /// <returns>A new entity bag to be used when returning to view mode, or the URL to redirect to after creating a new entity.</returns>
        [BlockAction]
        public BlockActionResult Save( DetailBlockBox<FinancialPledgeBag, FinancialPledgeDetailOptionsBag> box )
        {
            using ( var rockContext = new RockContext() )
            {
                var entityService = new FinancialPledgeService( rockContext );
                var definedValueService = new DefinedValueService( rockContext );

                if ( !TryGetEntityForEditAction( box.Entity.IdKey, rockContext, out var entity, out var actionError ) )
                {
                    return actionError;
                }

                // Update the entity instance from the information in the bag.
                if ( !UpdateEntityFromBox( entity, box, rockContext ) )
                {
                    return ActionBadRequest( "Invalid data." );
                }

                var isNew = entity.Id == 0;
                Person person = null;
                FinancialAccount financialAccount = null;

                if ( isNew )
                {
                    person = FindPerson( rockContext, box.Entity );
                    entity.PersonAliasId = person.PrimaryAliasId;

                    financialAccount = GetFinancialAccount( rockContext );
                    entity.AccountId = financialAccount.Id;
                    entity.Account = financialAccount;
                }

                // Ensure everything is valid before saving.
                if ( !ValidateFinancialPledge( entity, rockContext, out var validationMessage ) )
                {
                    return ActionBadRequest( validationMessage );
                }

                if ( box.Entity.ButtonAction != btnConfirm && isNew )
                {
                    var duplicatePledges = entityService.Queryable()
                        .Where( a => a.PersonAlias.PersonId == person.Id )
                        .Where( a => a.AccountId == entity.AccountId )
                        .Where( a => a.StartDate == entity.StartDate )
                        .Where( a => a.EndDate == entity.EndDate ).ToList();

                    if ( duplicatePledges.Any() )
                    {
                        box.Entity.DuplicatePledgeWarningText = "The following pledges have already been entered for you:";
                        box.Entity.DuplicatePledgeWarningText += "<ul>";
                        foreach ( var pledge in duplicatePledges.OrderBy( a => a.StartDate ).ThenBy( a => a.Account.Name ) )
                        {
                            box.Entity.DuplicatePledgeWarningText += string.Format( "<li>{0} {1} {2}</li>", pledge.Account, pledge.PledgeFrequencyValue, pledge.TotalAmount );
                        }

                        box.Entity.DuplicatePledgeWarningText += "</ul>";
                        box.Entity.ButtonAction = btnConfirm;
                        box.Entity.IsDuplicatePledge = true;
                        return ActionOk( box.Entity );
                    }
                }

                rockContext.WrapTransaction( () =>
                {
                    rockContext.SaveChanges();
                    entity.SaveAttributeValues( rockContext );
                } );

                var entityBag = GetEntityBagForView( entity );

                if ( isNew )
                {
                    // Ensure navigation properties will work now.
                    entity = entityService.Get( entity.Id );
                    entity.LoadAttributes( rockContext );

                    // populate PledgeFrequencyValue so that Liquid can access it
                    entity.PledgeFrequencyValue = definedValueService.Get( entity.PledgeFrequencyValueId ?? 0 );

                    var mergeFields = RequestContext.GetCommonMergeFields( RequestContext.CurrentPerson );
                    mergeFields.Add( "Person", person );
                    mergeFields.Add( "FinancialPledge", entity );
                    mergeFields.Add( "PledgeFrequency", DefinedValueCache.Get( entity.PledgeFrequencyValueId ?? 0 ) );
                    mergeFields.Add( "Account", financialAccount );
                    entityBag.ReceiptText = GetAttributeValue( AttributeKey.ReceiptText ).ResolveMergeFields( mergeFields );

                    // Resolve any dynamic url references
                    string appRoot = RequestContext.ResolveRockUrl( "~/" );
                    string themeRoot = RequestContext.ResolveRockUrl( "~~/" );
                    entityBag.ReceiptText = entityBag.ReceiptText.Replace( "~~/", themeRoot ).Replace( "~/", appRoot );

                    return ActionOk( entityBag );
                }

                return ActionOk(entityBag);
            }
        }

        /// <summary>
        /// Deletes the specified entity.
        /// </summary>
        /// <param name="key">The identifier of the entity to be deleted.</param>
        /// <returns>A string that contains the URL to be redirected to on success.</returns>
        [BlockAction]
        public BlockActionResult Delete( string key )
        {
            using ( var rockContext = new RockContext() )
            {
                var entityService = new FinancialPledgeService( rockContext );

                if ( !TryGetEntityForEditAction( key, rockContext, out var entity, out var actionError ) )
                {
                    return actionError;
                }

                if ( !entityService.CanDelete( entity, out var errorMessage ) )
                {
                    return ActionBadRequest( errorMessage );
                }

                entityService.Delete( entity );
                rockContext.SaveChanges();

                return ActionOk( this.GetParentPageUrl() );
            }
        }

        /// <summary>
        /// Refreshes the list of attributes that can be displayed for editing
        /// purposes based on any modified values on the entity.
        /// </summary>
        /// <param name="box">The box that contains all the information about the entity being edited.</param>
        /// <returns>A box that contains the entity and attribute information.</returns>
        [BlockAction]
        public BlockActionResult RefreshAttributes( DetailBlockBox<FinancialPledgeBag, FinancialPledgeDetailOptionsBag> box )
        {
            using ( var rockContext = new RockContext() )
            {
                if ( !TryGetEntityForEditAction( box.Entity.IdKey, rockContext, out var entity, out var actionError ) )
                {
                    return actionError;
                }

                // Update the entity instance from the information in the bag.
                if ( !UpdateEntityFromBox( entity, box, rockContext ) )
                {
                    return ActionBadRequest( "Invalid data." );
                }

                // Reload attributes based on the new property values.
                entity.LoadAttributes( rockContext );

                var refreshedBox = new DetailBlockBox<FinancialPledgeBag, FinancialPledgeDetailOptionsBag>
                {
                    Entity = GetEntityBagForEdit( entity )
                };

                var oldAttributeGuids = box.Entity.Attributes.Values.Select( a => a.AttributeGuid ).ToList();
                var newAttributeGuids = refreshedBox.Entity.Attributes.Values.Select( a => a.AttributeGuid );

                // If the attributes haven't changed then return a 204 status code.
                if ( oldAttributeGuids.SequenceEqual( newAttributeGuids ) )
                {
                    return ActionStatusCode( System.Net.HttpStatusCode.NoContent );
                }

                // Replace any values for attributes that haven't changed with
                // the value sent by the client. This ensures any unsaved attribute
                // value changes are not lost.
                foreach ( var kvp in refreshedBox.Entity.Attributes )
                {
                    if ( oldAttributeGuids.Contains( kvp.Value.AttributeGuid ) )
                    {
                        refreshedBox.Entity.AttributeValues[kvp.Key] = box.Entity.AttributeValues[kvp.Key];
                    }
                }

                return ActionOk( refreshedBox );
            }
        }

        #endregion
    }
}
