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

using Rock.Model;
using Rock.Net.Geolocation;
using Rock.Transactions;

namespace Rock.Core
{
    /// <summary>
    /// Describes an interaction.
    /// </summary>
    internal class InteractionInfo
    {
        #region InteractionSessionLocation Properties

        /// <inheritdoc cref="IpGeolocation.IpAddress"/>
        public string GeolocationIpAddress { get; set; }

        /// <inheritdoc cref="InteractionSessionLocation.LookupDateTime"/>
        public DateTime? GeolocationLookupDateTime { get; set; }

        /// <inheritdoc cref="IpGeolocation.City"/>
        public string City { get; set; }

        /// <inheritdoc cref="IpGeolocation.RegionName"/>
        public string RegionName { get; set; }

        /// <inheritdoc cref="InteractionSessionLocation.RegionCode"/>
        public string RegionCode { get; set; }

        /// <inheritdoc cref="InteractionSessionLocation.RegionValueId"/>
        public int? RegionValueId { get; set; }

        /// <inheritdoc cref="InteractionSessionLocation.CountryCode"/>
        public string CountryCode { get; set; }

        /// <inheritdoc cref="InteractionSessionLocation.CountryValueId"/>
        public int? CountryValueId { get; set; }

        /// <inheritdoc cref="InteractionSessionLocation.PostalCode"/>
        public string PostalCode { get; set; }

        /// <inheritdoc cref="IpGeolocation.Latitude"/>
        public double? Latitude { get; set; }

        /// <inheritdoc cref="IpGeolocation.Longitude"/>
        public double? Longitude { get; set; }

        #endregion InteractionSessionLocation Properties

        #region InteractionSession Properties

        /// <inheritdoc cref="InteractionSession.IpAddress"/>
        public string IpAddress { get; set; }

        /// <summary>
        /// The <c>RockSessionId</c> (Guid) (set in Global.Session_Start)
        /// </summary>
        public Guid? BrowserSessionId { get; set; }

        #endregion InteractionSession Properties

        #region Interaction Properties

        /// <inheritdoc cref="IEntity.Guid"/>
        /// <remarks>
        /// If this is not specified then a new Guid will be created.
        /// </remarks>
        public Guid? InteractionGuid { get; set; }

        /// <inheritdoc cref="Interaction.InteractionDateTime"/>
        public DateTime InteractionDateTime { get; set; }

        /// <inheritdoc cref="Interaction.Operation"/>
        public string Operation { get; set; }

        /// <inheritdoc cref="Interaction.InteractionComponentId"/>
        public int InteractionComponentId { get; set; }

        /// <inheritdoc cref="Interaction.EntityId"/>
        public int? EntityId { get; set; }

        /// <inheritdoc cref="Interaction.RelatedEntityTypeId"/>
        public int? RelatedEntityTypeId { get; set; }

        /// <inheritdoc cref="Interaction.RelatedEntityId"/>
        public int? RelatedEntityId { get; set; }

        /// <inheritdoc cref="Interaction.PersonAliasId"/>
        public int? PersonAliasId { get; set; }

        /// <inheritdoc cref="Interaction.InteractionSummary"/>
        public string InteractionSummary { get; set; }

        /// <inheritdoc cref="Interaction.InteractionData"/>
        public string InteractionData { get; set; }

        /// <inheritdoc cref="Interaction.InteractionEndDateTime"/>
        public DateTime? InteractionEndDateTime { get; set; }

        /// <inheritdoc cref="Interaction.ChannelCustom1"/>
        public string ChannelCustom1 { get; set; }

        /// <inheritdoc cref="Interaction.ChannelCustom2"/>
        public string ChannelCustom2 { get; set; }

        /// <inheritdoc cref="Interaction.ChannelCustomIndexed1"/>
        public string ChannelCustomIndexed1 { get; set; }

        /// <inheritdoc cref="Interaction.InteractionLength"/>
        public double? InteractionLength { get; set; }

        /// <inheritdoc cref="Interaction.InteractionTimeToServe"/>
        public double? InteractionTimeToServe { get; set; }

        /// <inheritdoc cref="Interaction.Source"/>
        public string Source { get; set; }

        /// <inheritdoc cref="Interaction.Medium"/>
        public string Medium { get; set; }

        /// <inheritdoc cref="Interaction.Campaign"/>
        public string Campaign { get; set; }

        /// <inheritdoc cref="Interaction.Content"/>
        public string Content { get; set; }

        /// <inheritdoc cref="Interaction.Content"/>
        public string Term { get; set; }

        #endregion Interaction Properties

        #region Helper Properties

        /// <summary>
        /// Gets or sets the raw user agent string of the client browser.
        /// </summary>
        public string UserAgent { get; set; }

        #endregion Helper Properties

        #region Constructors

        /// <summary>
        /// Creates a new interaction info instance.
        /// </summary>
        public InteractionInfo()
        {
        }

        /// <summary>
        /// Creates a new interaction info instance and sets its property values using
        /// the provided interaction transaction info object.
        /// </summary>
        /// <param name="info">The interaction transaction info that will be used to
        /// set this interaction info's property values.</param>
        public InteractionInfo( InteractionTransactionInfo info )
        {
            if ( info == null )
            {
                return;
            }

            #region Set InteractionSessionLocation Properties

            this.GeolocationIpAddress = info.GeolocationIpAddress;
            this.GeolocationLookupDateTime = info.GeolocationLookupDateTime;
            this.City = info.City;
            this.RegionName = info.RegionName;
            this.RegionCode = info.RegionCode;
            this.RegionValueId = info.RegionValueId;
            this.CountryCode = info.CountryCode;
            this.CountryValueId = info.CountryValueId;
            this.PostalCode = info.PostalCode;
            this.Latitude = info.Latitude;
            this.Longitude = info.Longitude;

            #endregion Set InteractionSessionLocation Properties

            #region Set InteractionSession Properties

            this.IpAddress = info.IPAddress;
            this.BrowserSessionId = info.BrowserSessionId;

            #endregion Set InteractionSession Properties

            #region Set Interaction Properties

            this.InteractionGuid = info.InteractionGuid;
            this.InteractionDateTime = info.InteractionDateTime;
            this.Operation = info.InteractionOperation;
            this.InteractionComponentId = info.InteractionComponentId ?? 0;
            this.EntityId = info.ComponentEntityId;
            this.RelatedEntityTypeId = info.InteractionRelatedEntityTypeId;
            this.RelatedEntityId = info.InteractionRelatedEntityId;
            this.PersonAliasId = info.PersonAliasId;
            this.InteractionSummary = info.InteractionSummary;
            this.InteractionData = info.InteractionData;
            this.InteractionEndDateTime = info.InteractionEndDateTime;
            this.ChannelCustom1 = info.InteractionChannelCustom1;
            this.ChannelCustom2 = info.InteractionChannelCustom2;
            this.ChannelCustomIndexed1 = info.InteractionChannelCustomIndexed1;
            this.InteractionLength = info.InteractionLength;
            this.InteractionTimeToServe = info.InteractionTimeToServe;
            this.Source = info.InteractionSource;
            this.Medium = info.InteractionMedium;
            this.Campaign = info.InteractionCampaign;
            this.Content = info.InteractionContent;
            this.Term = info.InteractionTerm;

            #endregion Set Interaction Properties

            #region Set Helper Properties

            this.UserAgent = info.UserAgent;

            #endregion Set Helper Properties
        }

        #endregion Constructors
    }
}
