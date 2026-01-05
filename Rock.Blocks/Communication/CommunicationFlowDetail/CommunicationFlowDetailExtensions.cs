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
using System.ComponentModel.DataAnnotations;
using System.Data.Entity;
using System.Linq;

using Rock.Communication;
using Rock.Model;
using Rock.ViewModels.Blocks.Communication.CommunicationFlowDetail;
using Rock.ViewModels.Utility;

namespace Rock.Blocks.Communication
{
    /// <summary>
    /// Communication Flow Detail extension methods.
    /// </summary>
    internal static class CommunicationFlowDetailExtensions
    {
        /// <summary>
        /// Includes the entities needed to convert Communication Flow entities to Communication Flow Bag objects.
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        public static IQueryable<CommunicationFlow> IncludeCommunicationFlowBagEntities( this IQueryable<CommunicationFlow> query )
        {
            return query
                // CommunicationFlowIncludes
                .Include( e => e.Category )
                .Include( e => e.Schedule )
                .Include( e => e.TargetAudienceDataView )
                .Include( e => e.CommunicationFlowCommunications )

                // CommunicationFlowCommunication includes
                .Include( e => e.CommunicationFlowCommunications.Select( c => c.CommunicationTemplate ) )

                // CommunicationTemplate includes
                .Include( e => e.CommunicationFlowCommunications.Select( c => c.CommunicationTemplate.ImageFile ) )
                .Include( e => e.CommunicationFlowCommunications.Select( c => c.CommunicationTemplate.Attachments ) );
        }

        /// <summary>
        /// Includes the entities needed to convert Communication Flow Detail Communication Template Bag objects to Communication Template Bag objects.
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        public static IQueryable<CommunicationTemplate> IncludeCommunicationTemplateBagEntities( this IQueryable<CommunicationTemplate> query )
        {
            return query
                .Include( e => e.ImageFile )
                .Include( e => e.Attachments )
                .Include( a => a.Attachments.Select( b => b.BinaryFile ) );
        }

        /// <summary>
        /// Includes the entities needed to convert Communication Flow entities to Communication Flow Bag objects.
        /// </summary>
        /// <param name="communicationTemplateService"></param>
        /// <param name="communicationTemplateGuid"></param>
        /// <returns></returns>
        public static CommunicationTemplate GetWithCommunicationTemplateBagIncludes( this CommunicationTemplateService communicationTemplateService, Guid communicationTemplateGuid )
        {
            return communicationTemplateService
                .Queryable()
                .IncludeCommunicationTemplateBagEntities()
                .FirstOrDefault( c => c.Guid == communicationTemplateGuid );
        }

        /// <summary>
        /// Includes the entities needed to convert Communication Flow entities to Communication Flow Bag objects.
        /// </summary>
        /// <param name="communicationTemplateService"></param>
        /// <param name="communicationTemplateId"></param>
        /// <returns></returns>
        public static CommunicationTemplate GetWithCommunicationTemplateBagIncludes( this CommunicationTemplateService communicationTemplateService, int communicationTemplateId )
        {
            return communicationTemplateService
                .Queryable()
                .IncludeCommunicationTemplateBagEntities()
                .FirstOrDefault( c => c.Id == communicationTemplateId );
        }

        /// <summary>
        /// Returns a query filtering Communication Template entities
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        public static IQueryable<CommunicationTemplate> GetEmailTemplates( this IQueryable<CommunicationTemplate> query )
        {
            return query
                .Where( t =>
                    ( t.Message != null && t.Message.Trim() != string.Empty )
                    || ( t.FromEmail != null && t.FromEmail.Trim() != string.Empty ) );
        }
    }
}
