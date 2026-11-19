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

using System;
using System.ComponentModel;

using Rock.AI.Agent.Annotations;
using Rock.AI.Agent.Classes.Common;
using Rock.AI.Agent.Classes.Entity;
using Rock.Configuration;
using Rock.Data;
using Rock.Enums.AI.Agent;
using Rock.Model;
using Rock.SystemGuid;
using Rock.Web.Cache;

namespace Rock.AI.Agent.Skills;

internal sealed partial class PrayerSkill
{
    #region Tool(s)

    [Description( "Adds a new or updates an existing prayer request." )]
    [AgentToolGuid( "3AE458AB-C06C-47BC-AD2D-86EB19E556F1" )]
    [AgentUsage( "If a personIdKey is provided, first and last name will be determined from their Person record." )]
    [AgentUsage( "When adding a request linked to a person without a campusIdKey, the person's campus is used." )]
    [AgentUsage( "Approved requests expire after the number of days configured on this skill unless an expirationDate is provided. Requests that are pending approval get their expiration date when a moderator approves them." )]
    [AgentToolPrerequisite( "Call the LookupPrayerCategories function to determine available categories. Select one that matches the prayer request sentiment." )]
    [AgentToolPrerequisite( "Call the SearchPerson function to first determine if there is an idKey you can use instead of first/last name." )]
    [AgentToolPrerequisite( "Call LookupCampuses to determine the campusIdKey when the user names a campus." )]
    public AgentToolResult AddOrUpdatePrayerRequest(
        string prayerRequestIdKey = null,
        string requestText = null,
        string categoryIdKey = null,
        string personIdKey = null,
        string firstName = null,
        string lastName = null,
        bool? isPublic = null,
        bool? isUrgent = null,

        [Description( "The campus the prayer request is for. When updating, leave empty to keep the current campus." )]
        string campusIdKey = null,

        [Description( "The last date the prayer request should be shown for prayer. Must be today or later. Leave empty to use the default number of days when adding, or to keep the current date when updating. A request with no date gets the default. Ignored for requests that are pending approval." )]
        DateTime? expirationDate = null )
    {
        using var rockContext = RockApp.Current.CreateRockContext();
        var helper = new AgentToolHelper( rockContext, AgentRequestContext, _logger );

        PrayerRequest prayerRequest;
        var isUpdate = false;

        if ( prayerRequestIdKey.IsNotNullOrWhiteSpace() )
        {
            prayerRequest = helper.GetRequiredEntity<PrayerRequest>( prayerRequestIdKey );
            isUpdate = prayerRequest != null;

            /*
                10/26/2026 - MSE

                Loading the request only checks VIEW, which every person has on
                prayer requests by default. Changing an existing request is only
                allowed for the person the request is for, the person that
                created it, or a person with EDIT on it, so a person who can use
                this skill cannot rewrite requests they could only see.
                Anonymous callers are never allowed.

                Reason: Prevent editing prayer requests the person does not own or cannot edit.
            */
            if ( prayerRequest != null && !IsAuthorizedToEditPrayerRequest( prayerRequest, AgentRequestContext.CurrentPerson ) )
            {
                return Error( "You are not authorized to edit this prayer request." );
            }

            /*
                10/26/2026 - MSE

                New requests from agents that are not internal wait for a
                moderator. An edit from one of those agents must go back to
                moderation too, otherwise the requester could approve a request
                with harmless text and then change it to anything while it stays
                approved. Every error after this point returns before saving, so
                this change is never saved on its own.

                Reason: Public agent edits must be approved again, the same as public agent creates.
            */
            if ( prayerRequest != null && AgentRequestContext.AudienceType != AudienceType.Internal )
            {
                prayerRequest.IsApproved = false;
                prayerRequest.ApprovedByPersonAliasId = null;
                prayerRequest.ApprovedOnDateTime = null;
            }
        }
        else
        {
            prayerRequest = rockContext.Set<PrayerRequest>().Create();
            new PrayerRequestService( rockContext ).Add( prayerRequest );

            prayerRequest.IsActive = true;
            prayerRequest.EnteredDateTime = RockDateTime.Now;

            // If this is an internal request, we will auto-approve it. If it's external, it will
            // need to be approved by a moderator.
            prayerRequest.IsApproved = AgentRequestContext.AudienceType == AudienceType.Internal;

            // Record who approved it and when, the same as the Prayer Request
            // Entry block does when it auto-approves a request.
            if ( prayerRequest.IsApproved == true )
            {
                prayerRequest.ApprovedByPersonAliasId = AgentRequestContext.CurrentPerson?.PrimaryAliasId;
                prayerRequest.ApprovedOnDateTime = RockDateTime.Now;
            }
        }

        if ( helper.HasErrors )
        {
            return helper.ErrorResult;
        }

        /*
            10/25/2026 - MSE

            The requester's name and email are copied onto a new request from
            the person named by personIdKey, so that person must be one the
            current person can view. On agents that are not internal, a request
            can only be linked to the current person; requests for anyone else
            must use first and last name. Anonymous callers cannot link a
            person at all. Re-sending the existing requester of a request being
            updated does not change the link, so it skips these checks.

            Reason: Prevent filing prayer requests under another person's record.
        */
        var existingRequester = prayerRequest.Id != 0 ? prayerRequest.RequestedByPersonAlias?.Person : null;
        var isExistingRequester = existingRequester != null
            && ( personIdKey == existingRequester.IdKey || personIdKey.AsGuidOrNull() == existingRequester.Guid );

        if ( personIdKey.IsNotNullOrWhiteSpace() && !isExistingRequester )
        {
            const string ownPersonOnlyMessage = "Prayer requests can only be linked to your own person record. Use firstName and lastName for anyone else.";

            if ( AgentRequestContext.CurrentPerson == null )
            {
                return Error( ownPersonOnlyMessage );
            }

            helper.UpdateNavigationProperty( prayerRequest, pr => pr.RequestedByPersonAlias, personIdKey, checkSecurity: true );

            if ( helper.HasErrors )
            {
                return helper.ErrorResult;
            }

            if ( AgentRequestContext.AudienceType != AudienceType.Internal
                && prayerRequest.RequestedByPersonAlias?.PersonId != AgentRequestContext.CurrentPerson.Id )
            {
                return Error( ownPersonOnlyMessage );
            }
        }

        helper.UpdateNavigationProperty( prayerRequest, pr => pr.Category, categoryIdKey );
        helper.UpdateNavigationProperty( prayerRequest, pr => pr.Campus, campusIdKey );
        helper.UpdateProperty( prayerRequest, pr => pr.IsPublic, isPublic );
        helper.UpdateProperty( prayerRequest, pr => pr.IsUrgent, isUrgent );
        helper.UpdateProperty( prayerRequest, pr => pr.Text, requestText );

        string email = null;

        if ( prayerRequest.Id == 0 && prayerRequest.RequestedByPersonAliasId.HasValue )
        {
            if ( firstName.IsNullOrWhiteSpace() )
            {
                firstName = prayerRequest.RequestedByPersonAlias.Person.FirstName;
            }

            if ( lastName.IsNullOrWhiteSpace() )
            {
                lastName = prayerRequest.RequestedByPersonAlias.Person.LastName;
            }

            email = prayerRequest.RequestedByPersonAlias.Person.Email;
        }

        helper.UpdateProperty( prayerRequest, pr => pr.FirstName, firstName );
        helper.UpdateProperty( prayerRequest, pr => pr.LastName, lastName );
        helper.UpdateProperty( prayerRequest, pr => pr.Email, email );

        if ( prayerRequest.Id == 0 && campusIdKey.IsNullOrWhiteSpace() )
        {
            SetCampusFromRequester( rockContext, prayerRequest );
        }

        UpdateExpirationDate( helper, prayerRequest, expirationDate );

        // Perform error checking to make sure this prayer request is valid.
        if ( !prayerRequest.CategoryId.HasValue )
        {
            helper.AddError( "Prayer requests must have a category." );
        }

        var prayerRequestEntityTypeId = EntityTypeCache.Get<PrayerRequest>( true, rockContext ).Id;
        if ( prayerRequest.Category.EntityTypeId != prayerRequestEntityTypeId )
        {
            helper.AddError( "Invalid prayer category. Category is not for prayer requests." );
        }

        if ( prayerRequest.FirstName.IsNullOrWhiteSpace() )
        {
            helper.AddError( "You must provide either a firstName or personIdKey when creating a prayer request." );
        }

        if ( prayerRequest.LastName.IsNullOrWhiteSpace() )
        {
            helper.AddError( "You must provide either a lastName or personIdKey when creating a prayer request." );
        }

        helper.SaveChangesIfNoErrors();

        if ( helper.HasErrors )
        {
            return helper.ErrorResult;
        }

        // The campus may have been set by Id only, so read the name from cache.
        var campus = prayerRequest.CampusId.HasValue ? CampusCache.Get( prayerRequest.CampusId.Value, rockContext ) : null;

        var result = new PrayerRequestResult
        {
            Id = prayerRequest.Id,
            Text = prayerRequest.Text,
            Category = new KeyNameResult( prayerRequest.Category.Id, prayerRequest.Category.Name ),
            Campus = campus != null ? new KeyNameResult( campus.Id, campus.Name ) : null,
            IsUrgent = prayerRequest.IsUrgent,
            IsApproved = prayerRequest.IsApproved,
            IsPublic = prayerRequest.IsPublic,
            ExpirationDate = prayerRequest.ExpirationDate,
        };

        var toolResult = Success( result )
            .WithHistoryContent( new KeyNameResult
            {
                Id = prayerRequest.Id,
            } );

        var action = isUpdate ? "updated" : "added";

        if ( prayerRequest.IsApproved == true )
        {
            toolResult.WithInstructions( $"The prayer request has been {action} and approved." );
        }
        else
        {
            toolResult.WithInstructions( $"The prayer request has been {action} and is pending approval." );
        }

        return toolResult;
    }

    #endregion

    #region Private Methods

    /// <summary>
    /// Sets the campus of a new prayer request to the campus of the person
    /// it is for. This matches the Prayer Request Entry block, which defaults
    /// the campus to the current person's campus. Nothing changes when the
    /// request is not linked to a person.
    /// </summary>
    /// <param name="rockContext">The context the prayer request was loaded from.</param>
    /// <param name="prayerRequest">The new prayer request.</param>
    private static void SetCampusFromRequester( RockContext rockContext, PrayerRequest prayerRequest )
    {
        var requester = prayerRequest.RequestedByPersonAlias?.Person;

        if ( requester == null )
        {
            return;
        }

        // This is the same lookup as Person.GetCampus(), but it uses our
        // context. GetCampus() falls back to a new RockContext to find the
        // family when the primary campus is not set.
        prayerRequest.CampusId = requester.PrimaryCampusId ?? requester.GetFamily( rockContext )?.CampusId;
    }

    /// <summary>
    /// Sets the expiration date of an approved prayer request. A new request,
    /// or an existing one with no expiration date, uses
    /// <paramref name="expirationDate"/> or the configured number of days.
    /// An existing request that already has a date only changes when a date
    /// is provided. Requests that are pending approval are not changed,
    /// because the moderator's approval sets the expiration date.
    /// </summary>
    /// <param name="helper">The helper that collects errors for the tool.</param>
    /// <param name="prayerRequest">The prayer request being added or updated.</param>
    /// <param name="expirationDate">The expiration date the agent asked for, if any.</param>
    private void UpdateExpirationDate( AgentToolHelper helper, PrayerRequest prayerRequest, DateTime? expirationDate )
    {
        /*
            11/19/26 - CLAUDE

            Prayer Session (web and mobile) only shows requests whose
            expiration date is today or later. A NULL expiration date fails
            that filter, so an approved request without one is never shown
            for prayer. Approved requests must always get an expiration date,
            the same way the Prayer Request Entry block does when it
            auto-approves a request. An existing approved request with no
            date also gets the default when it is edited, which repairs
            requests the tool added before it set this date.

            Reason: Approved requests with no expiration date are hidden from Prayer Session.
        */
        if ( prayerRequest.IsApproved != true )
        {
            return;
        }

        if ( expirationDate.HasValue )
        {
            if ( expirationDate.Value.Date < RockDateTime.Today )
            {
                helper.AddError( $"The {nameof( expirationDate )} must be today or later." );
                return;
            }

            prayerRequest.ExpirationDate = expirationDate.Value.Date;
        }
        else if ( prayerRequest.Id == 0 || !prayerRequest.ExpirationDate.HasValue )
        {
            prayerRequest.ExpirationDate = RockDateTime.Today.AddDays( GetExpireDays() );
        }
    }

    /// <summary>
    /// Gets the number of days until a new approved prayer request expires.
    /// A blank, zero or negative setting uses the default of 14 days. There
    /// is no "never expires" option, because a request with no expiration
    /// date is hidden from Prayer Session.
    /// </summary>
    /// <returns>The number of days until a new request expires.</returns>
    private int GetExpireDays()
    {
        var expireDays = ConfigurationValues.GetReadOnlyValueOrDefault( ConfigurationKey.ExpireDays, string.Empty ).AsIntegerOrNull();

        return expireDays.HasValue && expireDays.Value > 0 ? expireDays.Value : DefaultExpireDays;
    }

    #endregion
}
