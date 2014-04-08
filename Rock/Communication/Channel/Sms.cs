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
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.Linq;

using Rock.Data;
using Rock.Model;

using Rock.Web.UI.Controls.Communication;

namespace Rock.Communication.Channel
{
    /// <summary>
    /// An SMS communication
    /// </summary>
    [Description( "An SMS communication" )]
    [Export( typeof( ChannelComponent ) )]
    [ExportMetadata( "ComponentName", "SMS" )]
    public class Sms : ChannelComponent
    {
        /// <summary>
        /// Gets the control path.
        /// </summary>
        /// <value>
        /// The control path.
        /// </value>
        public override ChannelControl Control
        {
            get { return new Rock.Web.UI.Controls.Communication.Sms(); }
        }

        /// <summary>
        /// Gets the HTML preview.
        /// </summary>
        /// <param name="communication">The communication.</param>
        /// <param name="person">The person.</param>
        /// <returns></returns>
        public override string GetHtmlPreview( Model.Communication communication, Person person )
        {
            var rockContext = new RockContext();

            // Requery the Communication object
            communication = new CommunicationService( rockContext ).Get( communication.Id );

            var globalAttributes = Rock.Web.Cache.GlobalAttributesCache.Read();
            var mergeValues = Rock.Web.Cache.GlobalAttributesCache.GetMergeFields( null );

            if ( person != null )
            {
                mergeValues.Add( "Person", person );

                var recipient = communication.Recipients.Where( r => r.PersonId == person.Id ).FirstOrDefault();
                if ( recipient != null )
                {
                    // Add any additional merge fields created through a report
                    foreach ( var mergeField in recipient.AdditionalMergeValues )
                    {
                        if ( !mergeValues.ContainsKey( mergeField.Key ) )
                        {
                            mergeValues.Add( mergeField.Key, mergeField.Value );
                        }
                    }
                }
            }

            string message = communication.GetChannelDataValue( "Message" );
            return message.ResolveMergeFields( mergeValues );
        }

    }
}
