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
using System.Linq;

using Rock.Attribute;
using Rock.Common.Mobile.Blocks.Engagement.ContactProfile;
using Rock.Enums.Engagement;
using Rock.Mobile;
using Rock.Model;
using Rock.Utility;

using Gender = Rock.Model.Gender;
using RelationshipFocus = Rock.Enums.Engagement.RelationshipFocus;
using RelationshipStrength = Rock.Enums.Engagement.RelationshipStrength;
using TouchpointType = Rock.Enums.Engagement.TouchpointType;

namespace Rock.Blocks.Types.Mobile.Engagement
{
    /// <summary>
    /// Allow you to view the contact detail.
    /// </summary>
    [DisplayName( "Contact Profile" )]
    [Category( "Engagement" )]
    [IconCssClass( "ti ti-address-book" )]
    [Description( "Allow you to view the contact detail." )]
    [SupportedSiteTypes( SiteType.Mobile )]

    [SystemGuid.EntityTypeGuid( SystemGuid.EntityType.MOBILE_OUTREACH_CONTACT_PROFILE_BLOCK_TYPE )]
    [SystemGuid.BlockTypeGuid( SystemGuid.BlockType.MOBILE_OUTREACH_CONTACT_PROFILE )]
    public class ContactProfile : RockBlockType
    {
        #region Methods

        private int GetTouchpointCount( int contactId, TouchpointType type, int? completedDaysAgo = null )
        {
            ContactTouchpointService contactTouchpointService = new ContactTouchpointService( RockContext );

            var qry = contactTouchpointService.Queryable()
                .Where( tp => tp.ContactId == contactId )
                .Where( tp => tp.Type == type )
                .Where( tp => tp.CompletedDateTime.HasValue );

            if ( completedDaysAgo.HasValue )
            {
                var dateTime = RockDateTime.Now.AddDays( -completedDaysAgo.Value );
                qry = qry.Where( tp => tp.CompletedDateTime >= dateTime );
            }

            var totalCount = qry.Count();

            return totalCount;
        }

        #endregion

        #region Block Action

        /// <summary>
        /// Gets the contact profile.
        /// </summary>
        /// <param name="contactIdKey"></param>
        /// <returns></returns>
        [BlockAction]
        public BlockActionResult GetContactProfile( string contactIdKey )
        {
            ContactService contactService = new ContactService( RockContext );
            var contact = contactService.Get( contactIdKey );

            if ( contact == null )
            {
                return ActionBadRequest( "Contact not found." );
            }

            var photoUrl = contact.PhotoId.HasValue
                ? MobileHelper.BuildPublicApplicationRootUrl( FileUrlHelper.GetImageUrl( contact.PhotoId.Value, new GetImageUrlOptions { Width = 256, Height = 256 } ) )
                : string.Empty;

            var contactProfile = new ContactProfileBag
            {
                ContactId = contact.Id,
                FirstName = contact.FirstName,
                LastName = contact.LastName,
                PhotoUrl = photoUrl,
                LastUpdated = contact.ModifiedDateTime ?? contact.CreatedDateTime ?? DateTime.MinValue,
                Gender = ( int ) contact.Gender,
                MobilePhone = contact.MobilePhone,
                Email = contact.Email,
                RelationshipFocus = ( int ) contact.RelationshipFocus,
                RelationshipStrength = ( int ) contact.RelationshipStrength,
                BirthDay = contact.BirthDay,
                BirthMonth = contact.BirthMonth,
                BirthYear = contact.BirthYear,
                AnniversaryDay = contact.WeddingDay,
                AnniversaryMonth = contact.WeddingMonth,
                AnniversaryYear = contact.WeddingYear,
                SalvationDay = contact.SalvationDay,
                SalvationMonth = contact.SalvationMonth,
                SalvationYear = contact.SalvationYear,
                BaptismDay = contact.BaptismDay,
                BaptismMonth = contact.BaptismMonth,
                BaptismYear = contact.BaptismYear,
                LinkedInProfileUrl = contact.LinkedInProfileUrl,
                FacebookProfileUrl = contact.FacebookProfileUrl,
                TikTokProfileUrl = contact.TikTokProfileUrl,
                InstagramProfileUrl = contact.InstagramProfileUrl,
                XProfileUrl = contact.XProfileUrl,
                PrayerCadence = contact.PrayerCadence.ToMobile(),
                ConnectionCadence = contact.ConnectionCadence.ToMobile(),
                PrayerNote = contact.PrayerNote,
                TotalCompletedPrayersCount = GetTouchpointCount( contact.Id, TouchpointType.Prayer ),
                CompletedPrayersLast30DaysCount = GetTouchpointCount( contact.Id, TouchpointType.Prayer, 30 ),
                ConnectionNote = contact.ConnectionNote,
                TotalCompletedConnectionsCount = GetTouchpointCount( contact.Id, TouchpointType.Connection ),
                CompletedConnectionsLast30DaysCount = GetTouchpointCount( contact.Id, TouchpointType.Connection, 30 ),
            };

            return ActionOk( contactProfile );
        }

        /// <summary>
        /// Changes the contact image.
        /// </summary>
        /// <param name="contactIdKey"></param>
        /// <param name="photoGuid"></param>
        /// <returns></returns>
        [BlockAction]
        public BlockActionResult ChangeContactImage( string contactIdKey, Guid photoGuid )
        {
            ContactService contactService = new ContactService( RockContext );
            var contact = contactService.Get( contactIdKey );
            if ( contact == null )
            {
                return ActionBadRequest( "Contact not found." );
            }

            var binaryFileService = new BinaryFileService( RockContext );

            // If the contact already has a photo, delete it (we don't want to keep old photos lying around).
            if ( contact.PhotoId.HasValue )
            {
                var oldPhoto = binaryFileService.Get( contact.PhotoId.Value );
                if ( oldPhoto != null )
                {
                    binaryFileService.Delete( oldPhoto );
                }
            }

            // Get the new photo
            var newPhoto = binaryFileService.Get( photoGuid );
            if ( newPhoto == null )
            {
                return ActionBadRequest( "There was a problem changing profile picture." );
            }

            contact.PhotoId = newPhoto.Id;
            RockContext.SaveChanges();

            return ActionOk();
        }

        /// <summary>
        /// Updates the contact.
        /// </summary>
        /// <param name="contactIdKey"></param>
        /// <param name="contactProfileBag"></param>
        /// <returns></returns>
        [BlockAction]
        public BlockActionResult UpdateContact( string contactIdKey, ContactProfileBag contactProfileBag )
        {
            ContactService contactService = new ContactService( RockContext );
            var contact = contactService.Get( contactIdKey );
            if ( contact == null )
            {
                return ActionBadRequest( "Contact not found." );
            }

            var newRelationshipFocus = ( RelationshipFocus ) contactProfileBag.RelationshipFocus;
            var newRelationshipStrength = ( RelationshipStrength ) contactProfileBag.RelationshipStrength;

            RockContext.WrapTransaction( () =>
            {
                // If the relationship strength has changed, create a new ContactRelationshipStrengthChanges record.
                if ( contact.RelationshipStrength != newRelationshipStrength )
                {
                    ContactRelationshipChangeService contactRelationshipChangesService = new ContactRelationshipChangeService( RockContext );
                    var contactRelationshipChange = new ContactRelationshipChange
                    {
                        ContactId = contact.Id,
                        PreviousRelationshipStrength = contact.RelationshipStrength,
                        NewRelationshipStrength = newRelationshipStrength,
                    };

                    contactRelationshipChangesService.Add( contactRelationshipChange );
                }

                contact.FirstName = contactProfileBag.FirstName;
                contact.LastName = contactProfileBag.LastName;
                contact.Email = contactProfileBag.Email;
                contact.MobilePhone = contactProfileBag.MobilePhone;
                contact.Gender = ( Gender ) contactProfileBag.Gender;
                contact.BirthDay = contactProfileBag.BirthDay;
                contact.BirthMonth = contactProfileBag.BirthMonth;
                contact.BirthYear = contactProfileBag.BirthYear;
                contact.WeddingDay = contactProfileBag.AnniversaryDay;
                contact.WeddingMonth = contactProfileBag.AnniversaryMonth;
                contact.WeddingYear = contactProfileBag.AnniversaryYear;
                contact.SalvationDay = contactProfileBag.SalvationDay;
                contact.SalvationMonth = contactProfileBag.SalvationMonth;
                contact.SalvationYear = contactProfileBag.SalvationYear;
                contact.BaptismDay = contactProfileBag.BaptismDay;
                contact.BaptismMonth = contactProfileBag.BaptismMonth;
                contact.BaptismYear = contactProfileBag.BaptismYear;
                contact.RelationshipFocus = newRelationshipFocus;
                contact.RelationshipStrength = newRelationshipStrength;
                contact.LinkedInProfileUrl = contactProfileBag.LinkedInProfileUrl;
                contact.FacebookProfileUrl = contactProfileBag.FacebookProfileUrl;
                contact.TikTokProfileUrl = contactProfileBag.TikTokProfileUrl;
                contact.InstagramProfileUrl = contactProfileBag.InstagramProfileUrl;
                contact.XProfileUrl = contactProfileBag.XProfileUrl;

                contact.HasBeenBaptized = contactProfileBag.BaptismDay.HasValue && contactProfileBag.BaptismMonth.HasValue;
                contact.HasAcceptedJesus = contactProfileBag.SalvationDay.HasValue && contactProfileBag.SalvationMonth.HasValue;

                RockContext.SaveChanges();
            } );

            return ActionOk();
        }

        /// <summary>
        /// Updates the contact note and cadence.
        /// </summary>
        /// <param name="bag"></param>
        /// <returns></returns>
        [BlockAction]
        public BlockActionResult UpdateContactNoteAndCadence( UpdateContactNoteAndCadence bag )
        {
            ContactService contactService = new ContactService( RockContext );
            var contact = contactService.Get( bag.ContactIdKey );
            if ( contact == null )
            {
                return ActionBadRequest( "Contact not found." );
            }

            // Only update the fields that were included in the request
            contact.PrayerNote = bag.PrayerNote != null ? bag.PrayerNote : contact.PrayerNote;
            contact.ConnectionNote = bag.ConnectionNote != null ? bag.ConnectionNote : contact.ConnectionNote;
            contact.PrayerCadence = bag.PrayerCadence != null ? bag.PrayerCadence.Value.ToNative() : contact.PrayerCadence;
            contact.ConnectionCadence = bag.ConnectionCadence != null ? bag.ConnectionCadence.Value.ToNative() : contact.ConnectionCadence;

            RockContext.SaveChanges();

            return ActionOk();
        }

        /// <summary>
        /// Gets the contact touchpoint history.
        /// </summary>
        /// <param name="idKey"></param>
        /// <param name="touchpointTypeFilter"></param>
        /// <param name="take"></param>
        /// <returns></returns>
        [BlockAction]
        public BlockActionResult GetContactTouchpointHistory( string idKey, int? take, int? touchpointTypeFilter )
        {
            if ( idKey.IsNullOrWhiteSpace() )
            {
                return ActionBadRequest( "Contact not found." );
            }

            ContactService contactService = new ContactService( RockContext );
            var contact = contactService.Get( idKey );
            if ( contact == null )
            {
                return ActionBadRequest( "Contact not found." );
            }

            ContactTouchpointService contactTouchpointService = new ContactTouchpointService( RockContext );
            var qry = contactTouchpointService.Queryable()
                .Where( tp => tp.ContactId == contact.Id )
                .Where( tp => tp.CompletedDateTime.HasValue );

            if ( touchpointTypeFilter.HasValue )
            {
                qry = qry
                    .Where( tp => tp.Type == ( TouchpointType ) touchpointTypeFilter.Value );
            }

            if ( take.HasValue )
            {
                qry = qry.Take( take.Value );
            }

            var touchpointHistoryBag = qry
                .OrderByDescending( bag => bag.CompletedDateTime )
                .AsEnumerable()
                .Select( tp => new TouchpointHistoryBag
                {
                    TouchpointType = tp.Type.ToMobile(),
                    CommunicationMedium = tp.CommunicationMedium?.ToMobile(),
                    ContactFirstName = contact.FirstName,
                    ScheduleDateTime = tp.ScheduledDateTime,
                    CompletedDateTime = tp.CompletedDateTime.Value,
                    Note = tp.Note
                } ).ToList();

            return ActionOk( touchpointHistoryBag );
        }

        /// <summary>
        /// Adds the reminder touchpoint.
        /// </summary>
        /// <param name="contactIdKey"></param>
        /// <param name="reminderDate"></param>
        /// <param name="reminderNote"></param>
        /// <returns></returns>
        [BlockAction]
        public BlockActionResult AddReminderTouchpoint( string contactIdKey, DateTimeOffset reminderDate, string reminderNote )
        {
            ContactService contactService = new ContactService( RockContext );
            var contact = contactService.Get( contactIdKey );
            if ( contact == null )
            {
                return ActionBadRequest( "Contact not found." );
            }

            ContactTouchpointService contactTouchpointService = new ContactTouchpointService( RockContext );
            var reminderTouchpoint = new ContactTouchpoint
            {
                ContactId = contact.Id,
                Type = TouchpointType.Reminder,
                ScheduledDateTime = reminderDate.DateTime,
                SystemNote = reminderNote,
            };
            contactTouchpointService.Add( reminderTouchpoint );
            RockContext.SaveChanges();

            return ActionOk();
        }

        /// <summary>
        /// Stops the contact touchpoint.
        /// </summary>
        /// <param name="contactId"></param>
        /// <returns></returns>
        [BlockAction]
        public BlockActionResult StopContactTouchpoint( int contactId )
        {
            ContactService contactService = new ContactService( RockContext );
            var contact = contactService.Get( contactId );
            if ( contact == null )
            {
                return ActionBadRequest( "Contact not found." );
            }

            contact.PrayerCadence = OutreachCadence.Paused;
            contact.ConnectionCadence = OutreachCadence.Paused;

            RockContext.SaveChanges();

            return ActionOk();
        }

        #endregion
    }
}