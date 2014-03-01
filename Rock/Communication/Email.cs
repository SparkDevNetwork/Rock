// <copyright>
// Copyright 2013 by the Spark Development Network
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.apache.org/licenses/LICENSE-2.0
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
using System.Linq;

using Rock.Model;
using Rock.Web.Cache;

namespace Rock.Communication
{
    /// <summary>
    /// Sends an email template using the Email communication channel
    /// </summary>
    public static class Email
    {
        /// <summary>
        /// Sends the specified email template unique identifier.
        /// </summary>
        /// <param name="emailTemplateGuid">The email template unique identifier.</param>
        /// <param name="recipients">The recipients.</param>
        public static void Send( Guid emailTemplateGuid, Dictionary<string, Dictionary<string, object>> recipients )
        {
            if ( emailTemplateGuid != Guid.Empty && recipients != null && recipients.Any() )
            {
                var channelEntity = EntityTypeCache.Read( Rock.SystemGuid.EntityType.COMMUNICATION_CHANNEL_EMAIL.AsGuid() );
                if ( channelEntity != null )
                {
                    var channel = ChannelContainer.GetComponent( channelEntity.Name );
                    if ( channel != null )
                    {
                        var transport = channel.Transport;
                        if ( transport != null )
                        {
                            var template = new SystemEmailService().Get( emailTemplateGuid );
                            if ( template != null )
                            {
                                transport.Send( template, recipients );
                            }
                        }
                    }
                }
            }
        }
    }
}
