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
using System.Web;
using Rock.Data;
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
        /// <param name="appRoot">The application root.</param>
        /// <param name="themeRoot">The theme root.</param>
        public static void Send( Guid emailTemplateGuid, List<RecipientData> recipients, string appRoot = "", string themeRoot = "" )
        {
            try
            {
                if ( recipients != null && recipients.Any() )
                {
                    var channelEntity = EntityTypeCache.Read( Rock.SystemGuid.EntityType.COMMUNICATION_CHANNEL_EMAIL.AsGuid() );
                    if ( channelEntity != null )
                    {
                        var channel = ChannelContainer.GetComponent( channelEntity.Name );
                        if ( channel != null && channel.IsActive )
                        {
                            var transport = channel.Transport;
                            if ( transport != null && transport.IsActive )
                            {
                                var template = new SystemEmailService( new RockContext() ).Get( emailTemplateGuid );
                                if ( template != null )
                                {
                                    try
                                    {
                                        transport.Send( template, recipients, appRoot, themeRoot );
                                    }
                                    catch(Exception ex1)
                                    {
                                        throw new Exception( string.Format( "Error sending System Email ({0}).", template.Title ), ex1 );
                                    }
                                }
                                else
                                {
                                    throw new Exception( string.Format( "Error sending System Email: An invalid System Email Identifier was provided ({0}).", emailTemplateGuid.ToString() ) );
                                }
                            }
                            else
                            {
                                throw new Exception(string.Format("Error sending System Email: The '{0}' channel does not have a valid transport, or the transport is not active.", channelEntity.FriendlyName));
                            }
                        }
                        else
                        {
                            throw new Exception(string.Format("Error sending System Email: The '{0}' channel does not exist, or is not active (type: {1}).", channelEntity.FriendlyName, channelEntity.Name));
                        }
                    }
                    else
                    {
                        throw new Exception("Error sending System Email: Could not read Email Channel Entity Type");
                    }
                }
            }
            catch ( Exception ex )
            {
                ExceptionLogService.LogException( ex, HttpContext.Current );
            }
        }
    }
}
