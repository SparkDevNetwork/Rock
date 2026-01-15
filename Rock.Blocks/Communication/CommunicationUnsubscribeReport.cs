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
using System.Data.Entity;
using System.Linq;

using Rock.Enums.Controls;
using Rock.Model;
using Rock.Obsidian.UI;
using Rock.Utility;
using Rock.ViewModels.Blocks;
using Rock.ViewModels.Blocks.Communication.CommunicationUnsubscribeReport;
using Rock.ViewModels.Controls;
using Rock.Web.Cache;

namespace Rock.Blocks.Communication
{
    /// <summary>
    /// Used for displaying details of recipients who have unsubscribed as a result of receiving communications.
    /// </summary>
    /// <seealso cref="Rock.Blocks.RockBlockType" />

    [DisplayName( "Communication Unsubscribe Report" )]
    [Category( "Communication" )]
    [Description( "Used for displaying details of recipients who have unsubscribed as a result of receiving communications." )]
    [SupportedSiteTypes( Model.SiteType.Web )]

    [Rock.SystemGuid.EntityTypeGuid( "FA66E8EA-EC5B-4E1B-BC08-20608AB3CD22" )]
    [Rock.SystemGuid.BlockTypeGuid( "33AC3AE0-928E-42C4-B6AC-BA4AB1DA4520" )]
    public class CommunicationUnsubscribeReport : RockBlockType
    {
        #region Keys & Constants

        private static class PersonPreferenceKey
        {
            public const string FilterSendDateRange = "filter-send-date-range";
            public const string FilterUnsubscribeDateRange = "filter-unsubscribe-date-range";
            public const string FilterUnsubscribeLevel = "filter-unsubscribe-level";
        }

        #endregion Keys & Constants

        #region Properties

        /// <summary>
        /// Gets the block person preferences.
        /// </summary>
        private PersonPreferenceCollection BlockPersonPreferences => this.GetBlockPersonPreferences();

        /// <summary>
        /// Gets the send date range by which to filter the results.
        /// </summary>
        private SlidingDateRangeBag FilterSendDateRange => BlockPersonPreferences
            .GetValue( PersonPreferenceKey.FilterSendDateRange )
            .ToSlidingDateRangeBagOrNull();

        /// <summary>
        /// Gets the unsubscribe date range by which to filter the results.
        /// </summary>
        private SlidingDateRangeBag FilterUnsubscribeDateRange => BlockPersonPreferences
            .GetValue( PersonPreferenceKey.FilterUnsubscribeDateRange )
            .ToSlidingDateRangeBagOrNull();

        /// <summary>
        /// Gets the unsubscribe level by which to filter the results.
        /// </summary>
        private UnsubscribeLevel? FilterUnsubscribeLevel
        {
            get
            {
                var unsubscribeLevelValue = BlockPersonPreferences
                    .GetValue( PersonPreferenceKey.FilterUnsubscribeLevel );

                if ( unsubscribeLevelValue.IsNullOrWhiteSpace() )
                {
                    return null;
                }

                if ( Enum.TryParse<UnsubscribeLevel>( unsubscribeLevelValue, out var unsubscribeLevel ) )
                {
                    return unsubscribeLevel;
                }

                return null;
            }
        }

        #endregion Properties

        #region RockBlockType Implementation

        /// <inheritdoc/>
        public override object GetObsidianBlockInitialization()
        {
            var box = new ListBlockBox<CommunicationUnsubscribeReportOptionsBag>();
            var builder = GetGridBuilder();

            box.ExpectedRowCount = 100;
            box.Options = GetBoxOptions();
            box.GridDefinition = builder.BuildDefinition();

            return box;
        }

        #endregion RockBlockType Implementation

        #region Block Actions

        /// <summary>
        /// Gets the grid data.
        /// </summary>
        /// <returns>A bag containing the grid data.</returns>
        [BlockAction]
        public BlockActionResult GetGridData()
        {
            // Default to the last 6 months if a null/invalid range was selected.
            var defaultSlidingDateRange = new SlidingDateRangeBag
            {
                RangeType = SlidingDateRangeType.Last,
                TimeUnit = TimeUnitType.Month,
                TimeValue = 6
            };

            var sendDateRange = FilterSendDateRange.Validate( defaultSlidingDateRange ).ActualDateRange;
            var sendDateTimeStart = sendDateRange.Start;
            var sendDateTimeEnd = sendDateRange.End;

            var unsubscribeDateRange = FilterUnsubscribeDateRange.Validate( defaultSlidingDateRange ).ActualDateRange;
            var unsubscribeDateTimeStart = unsubscribeDateRange.Start;
            var unsubscribeDateTimeEnd = unsubscribeDateRange.End;

            var qry = new CommunicationRecipientService( RockContext )
                .Queryable()
                .AsNoTracking()
                .Where( cr =>
                    cr.PersonAliasId.HasValue
                    && cr.SendDateTime.HasValue
                    && cr.SendDateTime >= sendDateTimeStart
                    && cr.SendDateTime < sendDateTimeEnd
                    && cr.UnsubscribeDateTime.HasValue
                    && cr.UnsubscribeDateTime >= unsubscribeDateTimeStart
                    && cr.UnsubscribeDateTime < unsubscribeDateTimeEnd
                    && cr.UnsubscribeLevel.HasValue
                );

            if ( FilterUnsubscribeLevel.HasValue )
            {
                qry = qry.Where( cr => cr.UnsubscribeLevel == FilterUnsubscribeLevel.Value );
            }

            var communicationUnsubscribeRows = qry
                .Select( cr => new CommunicationUnsubscribeRow
                {
                    CommunicationRecipientId = cr.Id,
                    RecipientPersonId = cr.PersonAlias.Person.Id,
                    RecipientPersonNickName = cr.PersonAlias.Person.NickName,
                    RecipientPersonLastName = cr.PersonAlias.Person.LastName,
                    RecipientPersonSuffixValueId = cr.PersonAlias.Person.SuffixValueId,
                    RecipientPersonConnectionStatusValueId = cr.PersonAlias.Person.ConnectionStatusValueId,
                    RecipientPersonPhotoId = cr.PersonAlias.Person.PhotoId,
                    RecipientPersonBirthDay = cr.PersonAlias.Person.BirthDay,
                    RecipientPersonBirthMonth = cr.PersonAlias.Person.BirthMonth,
                    RecipientPersonBirthYear = cr.PersonAlias.Person.BirthYear,
                    RecipientPersonGender = cr.PersonAlias.Person.Gender,
                    RecipientPersonRecordTypeValueId = cr.PersonAlias.Person.RecordTypeValueId,
                    RecipientPersonAgeClassification = cr.PersonAlias.Person.AgeClassification,
                    SendDateTime = ( DateTime ) cr.SendDateTime,
                    UnsubscribeDateTime = ( DateTime ) cr.UnsubscribeDateTime,
                    UnsubscribeLevel = ( UnsubscribeLevel ) cr.UnsubscribeLevel,
                    CommunicationName = cr.Communication.Name,
                    CommunicationSubject = cr.Communication.Subject,
                    CommunicationPushTitle = cr.Communication.PushTitle,
                    TopicValueId = cr.Communication.CommunicationTopicValueId,
                    SenderPersonId = cr.Communication.SenderPersonAliasId.HasValue ? cr.Communication.SenderPersonAlias.Person.Id : ( int? ) null,
                    SenderPersonNickName = cr.Communication.SenderPersonAliasId.HasValue ? cr.Communication.SenderPersonAlias.Person.NickName : null,
                    SenderPersonLastName = cr.Communication.SenderPersonAliasId.HasValue ? cr.Communication.SenderPersonAlias.Person.LastName : null,
                    SenderPersonSuffixValueId = cr.Communication.SenderPersonAliasId.HasValue ? cr.Communication.SenderPersonAlias.Person.SuffixValueId : null,
                    SenderPersonRecordTypeValueId = cr.Communication.SenderPersonAliasId.HasValue ? cr.Communication.SenderPersonAlias.Person.RecordTypeValueId : null
                } )
                .ToList();

            var builder = GetGridBuilder();

            var gridDataBag = builder.Build(
                communicationUnsubscribeRows
                    .OrderByDescending( r => r.SendDateTime )
                    .ThenBy( r => r.RecipientPersonLastName )
                    .ThenBy( r => r.RecipientPersonNickName )
            );

            return ActionOk( gridDataBag );
        }

        #endregion Block Actions

        #region Private Methods

        /// <summary>
        /// Gets the box options required for the component to render the list.
        /// </summary>
        /// <returns>The options that provide additional details to the block.</returns>
        private CommunicationUnsubscribeReportOptionsBag GetBoxOptions()
        {
            var options = new CommunicationUnsubscribeReportOptionsBag
            {
                UnsubscribeLevelItems = typeof( UnsubscribeLevel ).ToEnumListItemBag()
            };

            return options;
        }

        /// <summary>
        /// Gets the grid builder for the communication unsubscribe report.
        /// </summary>
        /// <returns>The grid builder for the communication unsubscribe report.</returns>
        private GridBuilder<CommunicationUnsubscribeRow> GetGridBuilder()
        {
            var gridBuilder = new GridBuilder<CommunicationUnsubscribeRow>()
                .WithBlock( this )
                .AddField( "idKey", a => a.CommunicationRecipientId.AsIdKey() )
                .AddPersonField( "recipientPerson", a =>
                {
                    return new Person
                    {
                        Id = a.RecipientPersonId,
                        NickName = a.RecipientPersonNickName,
                        LastName = a.RecipientPersonLastName,
                        ConnectionStatusValueId = a.RecipientPersonConnectionStatusValueId,
                        PhotoId = a.RecipientPersonPhotoId,
                        BirthDay = a.RecipientPersonBirthDay,
                        BirthMonth = a.RecipientPersonBirthMonth,
                        BirthYear = a.RecipientPersonBirthYear,
                        Gender = a.RecipientPersonGender,
                        RecordTypeValueId = a.RecipientPersonRecordTypeValueId,
                        AgeClassification = a.RecipientPersonAgeClassification
                    };
                } )
                .AddDateTimeField( "sendDateTime", a => a.SendDateTime )
                .AddDateTimeField( "unsubscribeDateTime", a => a.UnsubscribeDateTime )
                .AddField( "unsubscribeLevel", a => a.UnsubscribeLevel )
                .AddTextField( "communicationName", a => a.CommunicationDisplayName )
                .AddTextField( "topic", a =>
                {
                    if ( !a.TopicValueId.HasValue )
                    {
                        return null;
                    }

                    return DefinedValueCache.Get( a.TopicValueId.Value )?.Value;
                } )
                .AddPersonField( "sentByPerson", a =>
                {
                    if ( !a.SenderPersonId.HasValue )
                    {
                        return null;
                    }

                    // We're not going to display the avatar or connection status, so we need very little data here.
                    return new Person
                    {
                        Id = a.SenderPersonId.Value,
                        NickName = a.SenderPersonNickName,
                        LastName = a.SenderPersonLastName,
                        SuffixValueId = a.SenderPersonSuffixValueId,
                        RecordSourceValueId = a.SenderPersonRecordTypeValueId
                    };
                } );

            return gridBuilder;
        }

        #endregion Private Methods

        #region Supporting Classes

        /// <summary>
        /// A POCO to represent a communication unsubscribe row in the report.
        /// </summary>
        private class CommunicationUnsubscribeRow
        {
            /// <summary>
            /// Gets or sets the <see cref="CommunicationRecipient"/> identifier.
            /// </summary>
            public int CommunicationRecipientId { get; set; }

            /// <summary>
            /// Gets or sets the identifier of the person who received the <see cref="Rock.Model.Communication"/>.
            /// </summary>
            public int RecipientPersonId { get; set; }

            /// <summary>
            /// Gets or sets the nickname of the person who received the <see cref="Rock.Model.Communication"/>.
            /// </summary>
            public string RecipientPersonNickName { get; set; }

            /// <summary>
            /// Gets or sets the last name of the person who received the <see cref="Rock.Model.Communication"/>.
            /// </summary>
            public string RecipientPersonLastName { get; set; }

            /// <summary>
            /// Get or sets the suffix value identifier of the person who received the <see cref="Rock.Model.Communication"/>.
            /// </summary>
            public int? RecipientPersonSuffixValueId { get; set; }

            /// Gets or sets the connection status value identifier of the person who received the <see cref="Rock.Model.Communication"/>.
            public int? RecipientPersonConnectionStatusValueId { get; set; }

            /// Gets or sets the photo identifier of the person who received the <see cref="Rock.Model.Communication"/>.
            public int? RecipientPersonPhotoId { get; set; }

            /// Gets or sets the birth day of the person who received the <see cref="Rock.Model.Communication"/>.
            public int? RecipientPersonBirthDay { get; set; }

            /// Gets or sets the birth month of the person who received the <see cref="Rock.Model.Communication"/>.
            public int? RecipientPersonBirthMonth { get; set; }

            /// Gets or sets the birth year of the person who received the <see cref="Rock.Model.Communication"/>.
            public int? RecipientPersonBirthYear { get; set; }

            /// Gets or sets the gender of the person who received the <see cref="Rock.Model.Communication"/>.
            public Gender RecipientPersonGender { get; set; }

            /// Gets or sets the record type value identifier of the person who received the <see cref="Rock.Model.Communication"/>.
            public int? RecipientPersonRecordTypeValueId { get; set; }

            /// Gets or sets the age classification of the person who received the <see cref="Rock.Model.Communication"/>.
            public AgeClassification RecipientPersonAgeClassification { get; set; }

            /// <inheritdoc cref="CommunicationRecipient.SendDateTime"/>.
            public DateTime SendDateTime { get; set; }

            /// <inheritdoc cref="CommunicationRecipient.UnsubscribeDateTime"/>.
            public DateTime UnsubscribeDateTime { get; set; }

            /// <inheritdoc cref="CommunicationRecipient.UnsubscribeLevel"/>.
            public UnsubscribeLevel UnsubscribeLevel { get; set; }

            /// <summary>
            /// Gets or sets the name of the communication.
            /// </summary>
            public string CommunicationName { get; set; }

            /// <summary>
            /// Gets or sets the subject of the communication.
            /// </summary>
            public string CommunicationSubject { get; set; }

            /// <summary>
            /// Gets or sets the push title of the communication.
            /// </summary>
            public string CommunicationPushTitle { get; set; }

            /// <summary>
            /// Gets the display name of the communication.
            /// </summary>
            /// <remarks>
            /// The display name is determined by first checking the communication name, then the subject, and finally the push title.
            /// </remarks>
            public string CommunicationDisplayName
            {
                get
                {
                    if ( CommunicationName.IsNotNullOrWhiteSpace() )
                    {
                        return CommunicationName;
                    }

                    if ( CommunicationSubject.IsNotNullOrWhiteSpace() )
                    {
                        return CommunicationSubject;
                    }

                    return CommunicationPushTitle;
                }
            }

            /// <inheritdoc cref="Rock.Model.Communication.CommunicationTopicValueId"/>
            public int? TopicValueId { get; set; }

            /// <summary>
            /// Gets or sets the identifier of the person who sent the <see cref="Rock.Model.Communication"/>.
            /// </summary>
            public int? SenderPersonId { get; set; }

            /// <summary>
            /// Gets or sets the nickname of the person who sent the <see cref="Rock.Model.Communication"/>.
            /// </summary>
            public string SenderPersonNickName { get; set; }

            /// <summary>
            /// Gets or sets the last name of the person who sent the <see cref="Rock.Model.Communication"/>.
            /// </summary>
            public string SenderPersonLastName { get; set; }

            /// <summary>
            /// Get or sets the suffix value identifier of the person who sent the <see cref="Rock.Model.Communication"/>.
            /// </summary>
            public int? SenderPersonSuffixValueId { get; set; }

            /// <summary>
            /// Gets or sets the record type value identifier of the person who sent the <see cref="Rock.Model.Communication"/>.
            /// </summary>
            public int? SenderPersonRecordTypeValueId { get; set; }
        }

        #endregion Supporting Classes
    }
}
