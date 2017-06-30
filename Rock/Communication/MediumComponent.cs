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

using Rock.Attribute;
using Rock.Data;
using Rock.Extension;
using Rock.Model;
using Rock.Web.Cache;
using Rock.Web.UI.Controls.Communication;

namespace Rock.Communication
{
    /// <summary>
    /// Base class for components communication mediums (i.e. email, sms, twitter, etc) 
    /// </summary>
    [ComponentField( "Rock.Communication.TransportContainer, Rock", "Transport Container", "", false, "", "", 1 )]
    public abstract class MediumComponent : Component
    {
        /// <summary>
        /// Gets the transport.
        /// </summary>
        /// <value>
        /// The transport.
        /// </value>
        public TransportComponent Transport
        {
            get
            {
                Guid entityTypeGuid = Guid.Empty;
                if ( Guid.TryParse( GetAttributeValue( "TransportContainer" ), out entityTypeGuid ) )
                {
                    foreach ( var serviceEntry in TransportContainer.Instance.Components )
                    {
                        var component = serviceEntry.Value.Value;
                        var entityType = EntityTypeCache.Read( component.GetType() );
                        if ( entityType != null && entityType.Guid.Equals( entityTypeGuid ) )
                        {
                            return component;
                        }
                    }
                }
                return null;
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MediumComponent" /> class.
        /// </summary>
        public MediumComponent() : base( false )
        {
            this.LoadAttributes();
        }

        /// <summary>
        /// Gets the HTML preview.
        /// </summary>
        /// <param name="communication">The communication.</param>
        /// <param name="person">The person.</param>
        /// <returns></returns>
        [Obsolete("The GetCommunication now creates the HTML Preview directly")]
        public abstract string GetHtmlPreview( Model.Communication communication, Person person );

        /// <summary>
        /// Gets the read-only message details.
        /// </summary>
        /// <param name="communication">The communication.</param>
        /// <returns></returns>
        [Obsolete( "The CommunicationDetail block now creates the details" )]
        public abstract string GetMessageDetails( Model.Communication communication );

        /// <summary>
        /// Gets the control.
        /// </summary>
        /// <param name="useSimpleMode">if set to <c>true</c> [use simple mode].</param>
        /// <returns></returns>
        public abstract MediumControl GetControl( bool useSimpleMode );

        /// <summary>
        /// Gets a value indicating whether [supports bulk communication].
        /// </summary>
        /// <value>
        /// <c>true</c> if [supports bulk communication]; otherwise, <c>false</c>.
        /// </value>
        [Obsolete( "All meduims now support bulk communications")]
        public abstract bool SupportsBulkCommunication
        {
            get;
        }

        /// <summary>
        /// Sends the specified communication.
        /// </summary>
        /// <param name="communication">The communication.</param>
        public virtual void Send( Rock.Model.Communication communication )
        {
            var rockContext = new RockContext();
            var communicationService = new CommunicationService( rockContext );

            communication = communicationService.Queryable()
                .FirstOrDefault( t => t.Id == communication.Id );

            if ( communication != null &&
                communication.Status == Model.CommunicationStatus.Approved &&
                communication.HasPendingRecipients( rockContext ) &&
                ( !communication.FutureSendDateTime.HasValue || communication.FutureSendDateTime.Value.CompareTo( RockDateTime.Now ) <= 0 ) )
            {
                // Update any recipients that should not get sent the communication
                var recipients = new CommunicationRecipientService( rockContext )
                    .Queryable( "PersonAlias.Person" )
                    .Where( r =>
                        r.CommunicationId == communication.Id &&
                        ( !r.MediumEntityTypeId.HasValue || r.MediumEntityTypeId.Value == this.EntityType.Id ) &&
                        r.Status == CommunicationRecipientStatus.Pending )
                    .ToList();

                foreach ( var recipient in recipients )
                {
                    var person = recipient.PersonAlias.Person;

                    if ( person.IsDeceased )
                    {
                        recipient.Status = CommunicationRecipientStatus.Failed;
                        recipient.StatusNote = "Person is deceased";
                    }
                    else if ( person.EmailPreference == Model.EmailPreference.DoNotEmail )
                    {
                        recipient.Status = CommunicationRecipientStatus.Failed;
                        recipient.StatusNote = "Communication Preference of 'Do Not Send Communication'";
                    }
                    else if ( person.EmailPreference == Model.EmailPreference.NoMassEmails && communication.IsBulkCommunication )
                    {
                        recipient.Status = CommunicationRecipientStatus.Failed;
                        recipient.StatusNote = "Communication Preference of 'No Bulk Communication'";
                    }
                    else
                    {
                        ValidateRecipientForMedium( person, recipient );
                    }
                }

                // Add Each Medium attribute values as a medium data value
                foreach ( var attr in this.Attributes.Select( a => a.Value ) )
                {
                    string value = this.GetAttributeValue( attr.Key );
                    if ( value.IsNotNullOrWhitespace() )
                    {
                        communication.SetMediumDataValue( attr.Key, GetAttributeValue( attr.Key ) );
                    }
                }

                rockContext.SaveChanges();
            }

            var transport = Transport;
            if ( transport != null && transport.IsActive )
            {
                transport.Send( communication );
            }

        }


    }

}