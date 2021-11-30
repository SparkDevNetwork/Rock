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
using System.Linq;
using Rock.Data;

namespace Rock.Model
{
    public static partial class CommunicationExtensionMethods
    {
        /// <summary>
        /// Communications that were not recently approved within since the given cutoffTime.
        /// </summary>
        /// <param name="communications">The communications.</param>
        /// <param name="cutoffTime">The cutoff time.</param>
        /// <returns></returns>
        public static IQueryable<Communication> NotRecentlyApproved( this IQueryable<Communication> communications, DateTime cutoffTime )
        {
            // Make sure communication wasn't just recently approved
            return communications.Where( c => !c.ReviewedDateTime.HasValue || c.ReviewedDateTime.Value < cutoffTime );
        }

        /// <summary>
        /// Communications that were, if scheduled, are within the given window.
        /// </summary>
        /// <param name="communications">The communications.</param>
        /// <param name="startWindow">The start window.</param>
        /// <param name="endWindow">The end window.</param>
        /// <returns></returns>
        public static IQueryable<Communication> IfScheduledAreInWindow( this IQueryable<Communication> communications, DateTime startWindow, DateTime endWindow )
        {
            return communications.Where( c =>
                (
                    !c.FutureSendDateTime.HasValue ||
                    ( c.FutureSendDateTime.HasValue && c.FutureSendDateTime.Value >= startWindow && c.FutureSendDateTime.Value <= endWindow )
                )
            );
        }

        /// <summary>
        /// Returns a queryable of <see cref="Rock.Model.Communication"/> where the <paramref name="person"/> is authorized
        /// to <see cref="Rock.Security.Authorization.VIEW">view</see> the communication.
        /// This is based on the VIEW auth of the <see cref="Rock.Model.CommunicationTemplate"/> and/or <see cref="Rock.Model.SystemCommunication"/> that
        /// is associated with each communication.
        /// </summary>
        /// <param name="qryCommunications">The qry communications.</param>
        /// <param name="rockContext">The rock context.</param>
        /// <param name="person">The person.</param>
        /// <returns>IQueryable&lt;Communication&gt;.</returns>
        public static IQueryable<Communication> WherePersonAuthorizedToView( this IQueryable<Communication> qryCommunications, RockContext rockContext, Person person )
        {
            // We want to limit to only communications that they are authorized to view, but if there are a large number of communications, that could be very slow.
            // So, since communication security is based on CommunicationTemplate or SystemCommunication, take a shortcut and just limit based on
            // authorized CommunicationTemplates and authorized SystemCommunications
            var authorizedCommunicationTemplateIds = new CommunicationTemplateService( rockContext ).Queryable()
                .Where( a => qryCommunications.Any( x => x.CommunicationTemplateId.HasValue && x.CommunicationTemplateId == a.Id ) )
                .ToList().Where( a => a.IsAuthorized( Rock.Security.Authorization.VIEW, person ) ).Select( a => a.Id ).ToList();

            var authorizedSystemCommunicationIds = new SystemCommunicationService( rockContext ).Queryable()
                .Where( a => qryCommunications.Any( x => x.SystemCommunicationId.HasValue && a.Id == x.SystemCommunicationId ) )
                .ToList().Where( a => a.IsAuthorized( Rock.Security.Authorization.VIEW, person ) ).Select( a => a.Id ).ToList();

            return qryCommunications.Where( a =>
                    ( a.CommunicationTemplateId == null || authorizedCommunicationTemplateIds.Contains( a.CommunicationTemplateId.Value ) )
                    &&
                    ( a.SystemCommunicationId == null || authorizedSystemCommunicationIds.Contains( a.SystemCommunicationId.Value ) )
                    );
        }
    }
}
