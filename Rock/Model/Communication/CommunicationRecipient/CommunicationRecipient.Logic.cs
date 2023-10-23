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


using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using Rock.Data;
using Rock.Web.UI.Controls;

namespace Rock.Model
{
    public partial class CommunicationRecipient
    {
        #region Properties

        /// <summary>
        /// Gets a list of activities.
        /// </summary>
        /// <value>
        /// The activity list.
        /// </value>
        [NotMapped]
        public virtual string ActivityList
        {
            get
            {
                using ( var rockContext = new RockContext() )
                {
                    var interactions = this.GetInteractions( rockContext )
                       .OrderBy( a => a.InteractionDateTime )
                       .ToList();
                    StringBuilder sb = new StringBuilder();
                    foreach ( var interaction in interactions )
                    {
                        sb.AppendFormat( "{0} ({1} {2}): {3}<br/>",
                            interaction.Operation,
                            interaction.InteractionDateTime.ToShortDateString(),
                            interaction.InteractionDateTime.ToShortTimeString(),
                            GetInteractionDetails( interaction ) );
                    }

                    return sb.ToString();
                }
            }
        }

        /// <summary>
        /// Gets a list of activities.
        /// </summary>
        /// <value>
        /// The activity list.
        /// </value>
        [NotMapped]
        public virtual string ActivityListHtml
        {
            get
            {
                using ( var rockContext = new RockContext() )
                {
                    var interactions = this.GetInteractions( rockContext )
                        .OrderBy( a => a.InteractionDateTime )
                        .ToList();

                    StringBuilder sb = new StringBuilder();
                    sb.Append( "<ul>" );
                    foreach ( var interaction in interactions )
                    {
                        sb.AppendFormat( "<li>{0} <small>({1} {2})</small>: {3}</li>",
                            interaction.Operation,
                            interaction.InteractionDateTime.ToShortDateString(),
                            interaction.InteractionDateTime.ToShortTimeString(),
                            GetInteractionDetails( interaction ) );
                    }

                    sb.Append( "</ul>" );

                    return sb.ToString();
                }
            }
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Gets the interactions (Opened and Click activity)
        /// </summary>
        /// <param name="rockContext">The rock context.</param>
        /// <returns></returns>
        public virtual IQueryable<Interaction> GetInteractions( RockContext rockContext )
        {
            var interactionService = new InteractionService( rockContext );
            var interactionChannelGuid = Rock.SystemGuid.InteractionChannel.COMMUNICATION.AsGuid();
            var result = interactionService.Queryable()
                .Where( a => a.InteractionComponent.InteractionChannel.Guid == interactionChannelGuid && a.InteractionComponentId == this.CommunicationId );
            return result;
        }

        /// <summary>
        /// Helper method to get recipient merge values for sending communication.
        /// </summary>
        /// <param name="globalConfigValues">The global configuration values.</param>
        /// <returns></returns>
        public Dictionary<string, object> CommunicationMergeValues( Dictionary<string, object> globalConfigValues )
        {
            Dictionary<string, object> mergeValues = new Dictionary<string, object>();

            globalConfigValues.ToList().ForEach( v => mergeValues.Add( v.Key, v.Value ) );

            if ( this.Communication != null )
            {
                mergeValues.Add( "Communication", this.Communication );
            }

            if ( this.PersonAlias != null && this.PersonAlias.Person != null )
            {
                mergeValues.Add( "Person", this.PersonAlias.Person );
            }

            // Add any additional merge fields created through a report
            foreach ( var mergeField in this.AdditionalMergeValues )
            {
                if ( !mergeValues.ContainsKey( mergeField.Key ) )
                {
                    var entityTypeInfo = MergeFieldPicker.GetEntityTypeInfoFromMergeFieldId( mergeField.Key );
                    if ( entityTypeInfo?.EntityType != null )
                    {
                        // Merge Field is reference to an Entity record. So, get the Entity from the database and use that as a merge object
                        var entityTypeType = entityTypeInfo.EntityType.GetEntityType();
                        var mergeEntity = Reflection.GetIEntityForEntityType( entityTypeType, mergeField.Value.ToString() );

                        // Add Entity as Merge field. For example ("GroupMember", groupMember)
                        mergeValues.Add( entityTypeType.Name, mergeEntity );
                    }
                    else
                    {
                        // regular mergefield value
                        mergeValues.Add( mergeField.Key, mergeField.Value );
                    }
                }
            }

            return mergeValues;
        }

        /// <summary>
        /// Returns a <see cref="System.String" /> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String" /> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            if ( this.PersonAlias != null && this.PersonAlias.Person != null )
            {
                return this.PersonAlias.Person.ToStringSafe();
            }
            else
            {
                return base.ToString();
            }
        }

        #endregion

        #region Static Methods

        /// <summary>
        /// Gets the interaction details.
        /// </summary>
        /// <returns></returns>
        public static string GetInteractionDetails( Interaction interaction )
        {
            string interactionDetails = string.Empty;
            string ipAddress = interaction?.InteractionSession?.IpAddress ?? "'unknown'";

            if ( interaction.Operation == "Opened" )
            {
                interactionDetails = $"Opened from {ipAddress}";
            }
            else if ( interaction.Operation == "Click" )
            {
                interactionDetails = $"Clicked the address {interaction?.InteractionData} from {ipAddress}";
            }
            else
            {
                interactionDetails = $"{interaction?.Operation}";
            }

            string deviceTypeDetails = $"{interaction?.InteractionSession?.DeviceType?.OperatingSystem} {interaction?.InteractionSession?.DeviceType?.DeviceTypeData} {interaction?.InteractionSession?.DeviceType?.Application} {interaction?.InteractionSession?.DeviceType?.ClientType}";
            if ( deviceTypeDetails.IsNotNullOrWhiteSpace() )
            {
                interactionDetails += $" using {deviceTypeDetails}";
            }

            return interactionDetails;
        }

        #endregion
    }
}
