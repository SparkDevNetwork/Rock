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
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.Entity;
using System.Linq;

using Rock.Attribute;
using Rock.Common.Mobile.Blocks.Engagement.OutreachDashboard;
using Rock.Common.Mobile.Blocks.Engagement.ContactProfile;
using Rock.Common.Mobile.Blocks.Engagement.TouchpointDetail;
using Rock.Enums.Engagement;
using Rock.Mobile;
using Rock.Model;
using Rock.Utility;

namespace Rock.Blocks.Types.Mobile.Engagement
{
    /// <summary>
    /// Touchpoint Detail block allows you to connect, prayed and celebrate special events for your contacts.
    /// </summary>
    [DisplayName( "Touchpoint Detail" )]
    [Category( "Engagement" )]
    [IconCssClass( "ti ti-affiliate" )]
    [Description( "Touchpoint Detail block allows you to connect, prayed and celebrate special events for your contacts." )]
    [SupportedSiteTypes( SiteType.Mobile )]

    [TextField( "Baptism Info",
        Description = "The URL to open within a Pulse touchpoint during the baptism questionnaire.",
        IsRequired = false,
        DefaultValue = "",
        Key = AttributeKeys.BaptismInfo,
        Order = 0 )]

    [SystemGuid.EntityTypeGuid( SystemGuid.EntityType.MOBILE_OUTREACH_TOUCHPOINT_DETAIL_BLOCK_TYPE )]
    [SystemGuid.BlockTypeGuid( SystemGuid.BlockType.MOBILE_OUTREACH_TOUCHPOINT_DETAIL )]
    public class TouchpointDetail : RockBlockType
    {
        #region Constants

        private static class AttributeKeys
        {
            public const string BaptismInfo = "BaptismInfo";
        }

        #endregion

        #region Methods

        /// <summary>
        /// Gets the pending touchpoints for the given contact ids.
        /// </summary>
        /// <param name="contactIds"></param>
        /// <returns></returns>
        private List<ContactTouchpoint> GetPendingTouchpoints( List<int> contactIds )
        {
            ContactTouchpointService contactTouchpointService = new ContactTouchpointService( RockContext );

            var pendingTouchpoints = contactTouchpointService
                .Queryable()
                .AsNoTracking()
                .Where( tp => contactIds.Contains( tp.ContactId ) )         // Grab the person contact touchpoints
                .Where( tp => tp.CompletedDateTime == null )                // Only get the uncompleted touchpoints
                .Where( tp => tp.ScheduledDateTime < RockDateTime.Now )     // Only get the touchpoints that are scheduled for now or earlier
                .OrderBy( tp => tp.ScheduledDateTime )                      // Order by scheduled date time
                .ToList();

            return pendingTouchpoints;
        }

        /// <summary>
        /// Gets the gender string.
        /// </summary>
        /// <param name="gender"></param>
        /// <param name="maleText"></param>
        /// <param name="femaleText"></param>
        /// <param name="unknownText"></param>
        /// <returns></returns>
        public string GetGenderString( Gender gender, string maleText, string femaleText, string unknownText )
        {
            switch ( gender )
            {
                case Gender.Male:
                    return maleText;
                case Gender.Female:
                    return femaleText;
                default:
                    return unknownText;
            }
        }

        /// <summary>
        /// Gets the touchpoint view bag.
        /// </summary>
        /// <param name="touchpointType"></param>
        /// <param name="contact"></param>
        /// <returns></returns>
        private TouchpointViewBag GetTouchpointView( TouchpointType touchpointType, Contact contact )
        {
            var genderedPronoun = GetGenderString( contact.Gender, "his", "her", contact.FirstName );
            switch ( touchpointType )
            {
                case TouchpointType.Prayer:
                    return new TouchpointViewBag
                    {
                        IconSource = "resource://Rock.Mobile.Resources.outreach-prayer-hand.png",
                        Title = "Prayer",
                        InformationText = $"Lift up {contact.FirstName} in prayer."
                    };
                case TouchpointType.Connection:
                    return new TouchpointViewBag
                    {
                        IconSource = "resource://Rock.Mobile.Resources.outreach-conversation-bubble.png",
                        Title = "Connection",
                        InformationText = $"Check in to see how {contact.FirstName} doing."
                    };
                case TouchpointType.Reminder:
                    return new TouchpointViewBag
                    {
                        IconSource = "resource://Rock.Mobile.Resources.outreach-sticky-note.png",
                        Title = "Reminder",
                        InformationText = "Here’s what you wrote:"
                    };
                case TouchpointType.Pulse:
                    return new TouchpointViewBag
                    {
                        IconSource = "resource://Rock.Mobile.Resources.outreach-heart.png",
                        Title = "Pulse",
                        InformationText = $"Has your connection with {contact.FirstName} grown,or has he taken a step toward Christ?"
                    };
                case TouchpointType.Birthday:
                    return new TouchpointViewBag
                    {
                        IconSource = "resource://Rock.Mobile.Resources.outreach-birthday.png",
                        Title = "Birthday",
                        InformationText = $"Celebrate {genderedPronoun} life and your relationship",
                    };
                case TouchpointType.WeddingAnniversary:
                    return new TouchpointViewBag
                    {
                        IconSource = "resource://Rock.Mobile.Resources.outreach-wedding-anniversary.png",
                        Title = "Wedding Anniversary",
                        InformationText = $"Celebrate {genderedPronoun} commitment",
                    };
                case TouchpointType.BaptismAnniversary:
                    return new TouchpointViewBag
                    {
                        IconSource = "resource://Rock.Mobile.Resources.outreach-baptism-anniversary.png",
                        Title = "Baptism Anniversary",
                        InformationText = $"Celebrate {genderedPronoun} decision",
                    };
                case TouchpointType.SalvationAnniversary:
                    return new TouchpointViewBag
                    {
                        IconSource = "resource://Rock.Mobile.Resources.outreach-salvation-anniversary.png",
                        Title = "Salvation Anniversary",
                        InformationText = $"Celebrate {genderedPronoun} decision",
                    };
                default:
                    return new TouchpointViewBag
                    {
                        IconSource = "resource://Rock.Mobile.Resources.outreach-prayer-hand.png",
                        Title = "Touchpoint",
                        InformationText = $"Connect with {contact.FirstName}.",
                    };
            }
        }

        /// <summary>
        /// Resolves the URL.
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        private string ResolveURL( string url )
        {
            if ( url.IsNullOrWhiteSpace() )
            {
                return string.Empty;
            }

            if ( url.StartsWith( "http://" ) || url.StartsWith( "https://" ) )
            {
                return url;
            }
            else
            {
                return "https://" + url;
            }
        }

        #endregion

        #region Block Action

        /// <summary>
        /// Gets the contact touch point by id key.
        /// </summary>
        /// <param name="idKey"></param>
        /// <returns></returns>
        [BlockAction]
        public BlockActionResult GetTouchpointByIdKey( string idKey )
        {
            ContactTouchpointService contactTouchpointService = new ContactTouchpointService( RockContext );

            var touchpoint = contactTouchpointService
                .Queryable()
                .Where( tp => tp.CompletedDateTime == null )
                .AsEnumerable()
                .FirstOrDefault( tp => tp.IdKey == idKey );

            ContactService contactService = new ContactService( RockContext );
            var contact = contactService.Get( touchpoint.ContactId );

            var touchpointBag = new ContactTouchpointBag
            {
                ContactId = touchpoint.ContactId,
                Type = touchpoint.Type.ToMobile(),
                TouchpointIdKey = touchpoint.IdKey,
                ScheduledDate = touchpoint.ScheduledDateTime,
                PhotoUrl = contact.PhotoId != null ? MobileHelper.BuildPublicApplicationRootUrl( FileUrlHelper.GetImageUrl( contact.PhotoId.Value, new GetImageUrlOptions { Width = 256, Height = 256 } ) ) : string.Empty,
                LastUpdated = contact.ModifiedDateTime ?? contact.CreatedDateTime ?? DateTime.MinValue,
                FirstName = contact.FirstName,
                LastName = contact.LastName,
                Gender = ( int ) contact.Gender.ToMobile(),
                ConnectionNote = contact.ConnectionNote,
                PrayerNote = contact.PrayerNote,
                MobilePhone = contact.MobilePhone,
                SystemNote = touchpoint.SystemNote,
                Note = touchpoint.Note,
                Email = contact.Email,
                PrayerCadence = contact.PrayerCadence.ToMobile(),
                ConnectionCadence = contact.ConnectionCadence.ToMobile(),
                RelationshipFocus = ( int ) contact.RelationshipFocus,
                RelationshipStrength = ( int ) contact.RelationshipStrength,
                BirthDay = contact.BirthDay,
                BirthMonth = contact.BirthMonth,
                BirthYear = contact.BirthYear,
                AnniversaryDay = contact.WeddingDay,
                AnniversaryMonth = contact.WeddingMonth,
                AnniversaryYear = contact.WeddingYear,
                HasAcceptedJesus = contact.HasAcceptedJesus,
                SalvationDay = contact.SalvationDay,
                SalvationMonth = contact.SalvationMonth,
                SalvationYear = contact.SalvationYear,
                Baptized = contact.HasBeenBaptized,
                BaptismDay = contact.BaptismDay,
                BaptismMonth = contact.BaptismMonth,
                BaptismYear = contact.BaptismYear,
                TouchpointViewBag = GetTouchpointView( touchpoint.Type, contact )
            };

            return ActionOk( touchpointBag );
        }

        /// <summary>
        /// Gets the pending touchpoint identifier keys.
        /// </summary>
        /// <returns></returns>
        [BlockAction]
        public BlockActionResult GetPendingTouchpointIdKeys()
        {
            var person = RequestContext.CurrentPerson;
            if ( person == null )
            {
                return ActionOk( new List<string>() );
            }

            ContactService contactService = new ContactService( RockContext );
            var personContact = contactService.Queryable()
                .AsNoTracking()
                .Where( c => c.OwnerPersonAliasId == person.PrimaryAliasId )
                .Select( c => c.Id )
                .ToList();

            var pendingTouchpoint = GetPendingTouchpoints( personContact );
            var pendingTouchpointIdKey = pendingTouchpoint.Select( tp => tp.IdKey ).ToList();

            return ActionOk( pendingTouchpointIdKey );
        }

        /// <summary>
        /// Completes the touchpoint.
        /// </summary>
        /// <param name="bag"></param>
        /// <returns></returns>
        [BlockAction]
        public BlockActionResult CompleteTouchpoint( CompleteTouchpointBag bag )
        {
            if ( bag == null )
            {
                return ActionBadRequest( "Bag is required" );
            }

            ContactTouchpointService contactTouchpointService = new ContactTouchpointService( RockContext );
            var touchpoint = contactTouchpointService.Get( bag.IdKey );

            if ( touchpoint == null )
            {
                return ActionNotFound( "Touchpoint not found." );
            }

            touchpoint.CommunicationMedium = bag.CommunicationMedium?.ToNative();
            touchpoint.Note = bag.Note;
            touchpoint.CompletedDateTime = bag.CompletedDate.HasValue ? bag.CompletedDate.Value.DateTime : RockDateTime.Now;
            RockContext.SaveChanges();

            return ActionOk();
        }

        /// <summary>
        /// Updates the contact connection detail.
        /// </summary>
        /// <param name="updateContactConnectionBag"></param>
        /// <returns></returns>
        [BlockAction]
        public BlockActionResult UpdateContactConnectionDetail( UpdateContactConnectionBag updateContactConnectionBag )
        {
            ContactService contactService = new ContactService( RockContext );
            var contact = contactService.Get( updateContactConnectionBag.ContactId );
            if ( contact == null )
            {
                return ActionNotFound( "Contact not found." );
            }

            contact.ConnectionNote = updateContactConnectionBag.ConnectionNote;
            if ( updateContactConnectionBag.ConnectionCadence.HasValue )
            {
                contact.ConnectionCadence = updateContactConnectionBag.ConnectionCadence.Value.ToNative();
            }

            RockContext.SaveChanges();

            return ActionOk();
        }

        /// <summary>
        /// Updates touchpoint scheduled date.
        /// </summary>
        /// <param name="idKey"></param>
        /// <param name="scheduleDate"></param>
        /// <returns></returns>
        [BlockAction]
        public BlockActionResult UpdateScheduledDate( string idKey, DateTime scheduleDate )
        {
            ContactTouchpointService contactTouchpointService = new ContactTouchpointService( RockContext );
            var touchpoint = contactTouchpointService.Get( idKey );
            if ( touchpoint == null )
            {
                return ActionNotFound( "Touchpoint not found." );
            }

            touchpoint.ScheduledDateTime = scheduleDate;
            RockContext.SaveChanges();

            return ActionOk();
        }

        /// <summary>
        /// Updates the contact prayer detail.
        /// </summary>
        /// <param name="updateContactPrayerBag"></param>
        /// <returns></returns>
        [BlockAction]
        public BlockActionResult UpdateContactPrayerDetail( UpdateContactPrayerBag updateContactPrayerBag )
        {
            ContactService contactService = new ContactService( RockContext );
            var contact = contactService.Get( updateContactPrayerBag.ContactId );
            if ( contact == null )
            {
                return ActionNotFound( "Contact not found." );
            }

            contact.PrayerNote = updateContactPrayerBag.PrayerNote;
            if ( updateContactPrayerBag.PrayerCadence.HasValue )
            {
                contact.PrayerCadence = updateContactPrayerBag.PrayerCadence.Value.ToNative();
            }

            RockContext.SaveChanges();

            return ActionOk();
        }

        /// <summary>
        /// Gets the contact touchpoint history.
        /// </summary>
        /// <param name="contactId"></param>
        /// <returns></returns>
        [BlockAction]
        public BlockActionResult GetContactTouchpointHistory( int contactId )
        {
            ContactService contactService = new ContactService( RockContext );
            var contact = contactService.Get( contactId );
            if ( contact == null )
            {
                return ActionBadRequest( "Contact not found." );
            }

            ContactTouchpointService contactTouchpointService = new ContactTouchpointService( RockContext );
            var qry = contactTouchpointService.Queryable()
                .Where( tp => tp.ContactId == contact.Id )
                .Where( tp => tp.CompletedDateTime.HasValue );

            var touchpointHistoryBag = qry
                .OrderByDescending( bag => bag.CompletedDateTime )
                .AsEnumerable()
                .Select( tp => new TouchpointHistoryBag
                {
                    CommunicationMedium = tp.CommunicationMedium?.ToMobile(),
                    TouchpointType = tp.Type.ToMobile(),
                    ContactFirstName = contact.FirstName,
                    ScheduleDateTime = tp.ScheduledDateTime,
                    CompletedDateTime = tp.CompletedDateTime.Value,
                    Note = tp.Note
                } ).ToList();

            return ActionOk( touchpointHistoryBag );
        }

        /// <summary>
        /// Pulses the touchpoint contact update.
        /// </summary>
        /// <param name="bag"></param>
        /// <returns></returns>
        [BlockAction]
        public BlockActionResult PulseTouchpointContactUpdate( PulseContactUpdateBag bag )
        {
            ContactTouchpointService contactTouchpointService = new ContactTouchpointService( RockContext );
            ContactService contactService = new ContactService( RockContext );
            var contactRelationshipChangesService = new ContactRelationshipChangeService( RockContext );

            var touchpoint = contactTouchpointService.Get( bag.IdKey );
            var contact = contactService.Get( touchpoint.ContactId );


            // If the relationship strength change.
            if ( contact.RelationshipStrength != bag.RelationshipStrength.ToNative()
                || contact.HasAcceptedJesus != bag.hasAcceptedJesus
                || contact.HasBeenBaptized != bag.Baptized )
            {
                var newRelationshipChange = new ContactRelationshipChange();
                newRelationshipChange.ContactId = contact.Id;
                newRelationshipChange.PreviousRelationshipStrength = contact.RelationshipStrength;
                newRelationshipChange.NewRelationshipStrength = bag.RelationshipStrength.ToNative();

                newRelationshipChange.HasAcceptedJesus = bag.hasAcceptedJesus;
                newRelationshipChange.WasAcceptanceInfluencedByApp = bag.AppInfluenceSalvation ?? false;

                newRelationshipChange.HasBeenBaptized = bag.Baptized;
                newRelationshipChange.WasBaptismInfluencedByApp = bag.AppInfluenceBaptism ?? false;
                contactRelationshipChangesService.Add( newRelationshipChange );
            }

            contact.RelationshipStrength = bag.RelationshipStrength.ToNative();
            contact.RelationshipFocus = bag.RelationshipFocus.ToNative();
            contact.HasAcceptedJesus = bag.hasAcceptedJesus;
            contact.SalvationDay = bag.SalvationDay;
            contact.SalvationMonth = bag.SalvationMonth;
            contact.SalvationYear = bag.SalvationYear;
            contact.HasBeenBaptized = bag.Baptized;
            contact.BaptismDay = bag.BaptismDay;
            contact.BaptismMonth = bag.BaptismMonth;
            contact.BaptismYear = bag.BaptismYear;

            touchpoint.CompletedDateTime = RockDateTime.Now;

            RockContext.SaveChanges();

            return ActionOk();
        }

        /// <summary>
        /// Adds the reminder touchpoint.
        /// </summary>
        /// <param name="contactId"></param>
        /// <param name="reminderDate"></param>
        /// <param name="reminderNote"></param>
        /// <returns></returns>
        [BlockAction]
        public BlockActionResult AddReminderTouchpoint( int contactId, DateTimeOffset reminderDate, string reminderNote )
        {
            ContactService contactService = new ContactService( RockContext );
            var contact = contactService.Get( contactId );
            if ( contact == null )
            {
                return ActionBadRequest( "Contact not found." );
            }

            ContactTouchpointService contactTouchpointService = new ContactTouchpointService( RockContext );
            var newTouchpoint = new ContactTouchpoint
            {
                ContactId = contact.Id,
                Type = TouchpointType.Reminder,
                ScheduledDateTime = reminderDate.DateTime,
                SystemNote = reminderNote,
            };
            contactTouchpointService.Add( newTouchpoint );
            RockContext.SaveChanges();

            return ActionOk();
        }

        #endregion

        #region Mobile Configuration

        /// <inheritdoc/>
        public override object GetMobileConfigurationValues()
        {
            return new Rock.Common.Mobile.Blocks.Engagement.TouchpointDetail.Configuration
            {
                BaptismInfoUrl = ResolveURL( GetAttributeValue( AttributeKeys.BaptismInfo ) )
            };
        }

        #endregion
    }
}
