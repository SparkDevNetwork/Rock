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
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using Rock.Data;
using Rock.Rest.Filters;
using Rock.Rest.Utility;

namespace Rock.Rest.Controllers
{
    /// <summary>
    /// Communications REST API
    /// </summary>
    public partial class CommunicationsController
    {
        /// <summary>
        /// Sends a communication.
        /// </summary>
        /// <param name="id">The identifier.</param>
        [Authenticate, Secured]
        [HttpPost]
        [System.Web.Http.Route( "api/Communications/Send/{id}" )]
        [Rock.SystemGuid.RestActionGuid( "272C25FC-C608-4673-99D5-7FB1377D8A61" )]
        public virtual Task Send( int id )
        {
            var communication = GetById( id );
            return Model.Communication.SendAsync( communication );
        }

        /// <summary>
        /// Imports a communication record <see cref="Model.Communication"/> (with a sent status) for a list of given recipient <see cref="Model.CommunicationRecipient"/> data.
        /// This is useful for recording emails that have already been sent (via some external system) into Rock as communications.
        /// These communications won't receive any interactions for opens or clicks, but will record the fact that the communication was sent so they can be seen in Rock by staff.
        /// The payload data needs to use correct Rock Ids when provided (for things like MediumEntityTypeId, SenderPersonAliasId, etc.) but other items such as ForeignId/Guid are optional.
        /// </summary>
        /// <param name="importCommunications">The communication payload.</param>
        [Authenticate, Secured]
        [HttpPost]
        [System.Web.Http.Route( "api/Communications/ImportSent" )]
        [Rock.SystemGuid.RestActionGuid( "D99D0FEE-3479-4DC5-B92E-5D743C07EFC1" )]
        public virtual async Task<HttpResponseMessage> ImportSent( [FromBody] IEnumerable<ImportSentCommunicationApiModel> importCommunications )
        {
            if ( importCommunications == null || importCommunications.Count() == 0 )
            {
                return ControllerContext.Request.CreateResponse( System.Net.HttpStatusCode.OK, new
                {
                    Message = "An empty communication payload was specified."
                } );
            }

            var addedInfo = new List<ImportSentCommunicationResultApiModel>();

            using ( var rockContext = new Rock.Data.RockContext() )
            {
                var committed = false;
                rockContext.WrapTransaction( () =>
               {
                   var communicationsWithStatusSet = new List<Model.Communication>();

                   foreach ( var importCommunication in importCommunications )
                   {
                       if ( importCommunication == null )
                       {
                           continue;
                       }

                       // Map ApiModel properties to Entity model properties for Communication
                       var communicationToAdd = importCommunication.CreateMapped<Model.Communication>();

                       var sentDateTime = ( importCommunication?.SendDateTime ).GetValueOrDefault( RockDateTime.Now );

                       // Set properties from the POST payload
                       communicationToAdd.CreatedDateTime = sentDateTime;
                       communicationToAdd.ModifiedDateTime = sentDateTime;
                       communicationToAdd.SendDateTime = sentDateTime;
                       communicationToAdd.Status = Model.CommunicationStatus.Approved;

                       new Model.CommunicationService( rockContext ).Add( communicationToAdd );

                       var recipientsCount = ( importCommunication.Recipients?.Count ).GetValueOrDefault( 0 );

                       // Add the communication Guid that is associated with the payload
                       addedInfo.Add( new ImportSentCommunicationResultApiModel
                       {
                           CommunicationCount = importCommunications.Count(),
                           Details = new ImportSentCommunicationResultDetailsApiModel
                           {
                               CommunicationGuid = communicationToAdd.Guid,
                               CommunicationRecipientGuids = new List<System.Guid>( recipientsCount ),
                               RecipientCount = recipientsCount
                           }

                       } );

                       // Add the communication recipient
                       foreach ( var importCommunicationRecipient in importCommunication.Recipients )
                       {
                           if ( importCommunicationRecipient == null )
                           {
                               continue;
                           }

                           // Map ApiModel properties to Entity model properties for CommunicationRecipient
                           var importCommunicationRecipientToAdd = importCommunicationRecipient.CreateMapped<Model.CommunicationRecipient>();

                           importCommunicationRecipientToAdd.Status = Model.CommunicationRecipientStatus.Delivered;
                           importCommunicationRecipientToAdd.Communication = communicationToAdd;

                           new Model.CommunicationRecipientService( rockContext ).Add( importCommunicationRecipientToAdd );

                           // Add the recipient Guid(s) that are associated with the parent communication
                           var infoItem = addedInfo.FirstOrDefault( v => v.Details.CommunicationGuid == communicationToAdd.Guid );
                           if ( infoItem != null )
                           {
                               infoItem.Details.CommunicationRecipientGuids.Add( importCommunicationRecipientToAdd.Guid );
                           }
                       }
                   }
                   committed = true;
               } );

                if ( committed )
                {
                    //Write the transaction to the database
                    await rockContext.SaveChangesAsync();
                }

                return ControllerContext.Request.CreateResponse( System.Net.HttpStatusCode.OK, new
                {
                    Message = "Communications Imported: Response",
                    CommunicationsAdded = addedInfo
                } );
            }
        }
    }

    #region Api Models

    /// <summary>
    /// Class ImportSentCommunicationApiModel.
    /// Implements the <see cref="Rock.Rest.Utility.IMappedApiModel" />
    /// </summary>
    /// <seealso cref="Rock.Rest.Utility.IMappedApiModel" />
    public class ImportSentCommunicationApiModel : IMappedApiModel
    {
        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        /// <value>The name.</value>
        public string Name { get; set; }
        /// <summary>
        /// Gets or sets the type of the communication.
        /// </summary>
        /// <value>The type of the communication.</value>
        public Model.CommunicationType CommunicationType { get; set; }
        /// <summary>
        /// Gets or sets a value indicating whether this instance is bulk communication.
        /// </summary>
        /// <value><c>true</c> if this instance is bulk communication; otherwise, <c>false</c>.</value>
        public bool IsBulkCommunication { get; set; }
        /// <summary>
        /// Gets or sets the sender person alias identifier.
        /// </summary>
        /// <value>The sender person alias identifier.</value>
        public int? SenderPersonAliasId { get; set; }
        /// <summary>
        /// Gets or sets the reviewer person alias identifier.
        /// </summary>
        /// <value>The reviewer person alias identifier.</value>
        public int? ReviewerPersonAliasId { get; set; }
        /// <summary>
        /// Gets or sets the list group identifier.
        /// </summary>
        /// <value>The list group identifier.</value>
        public int? ListGroupId { get; set; }
        /// <summary>
        /// Gets or sets the communication template identifier.
        /// </summary>
        /// <value>The communication template identifier.</value>
        public int? CommunicationTemplateId { get; set; }
        /// <summary>
        /// Gets or sets the subject.
        /// </summary>
        /// <value>The subject.</value>
        public string Subject { get; set; }
        /// <summary>
        /// Gets or sets the send date time.
        /// </summary>
        /// <value>The send date time.</value>
        public System.DateTime? SendDateTime { get; set; }
        /// <summary>
        /// Gets or sets from name.
        /// </summary>
        /// <value>From name.</value>
        public string FromName { get; set; }
        /// <summary>
        /// Gets or sets from email.
        /// </summary>
        /// <value>From email.</value>
        public string FromEmail { get; set; }
        /// <summary>
        /// Gets or sets the reply to email.
        /// </summary>
        /// <value>The reply to email.</value>
        public string ReplyToEmail { get; set; }
        /// <summary>
        /// Gets or sets the cc emails.
        /// </summary>
        /// <value>The cc emails.</value>
        public string CCEmails { get; set; }
        /// <summary>
        /// Gets or sets the BCC emails.
        /// </summary>
        /// <value>The BCC emails.</value>
        public string BCCEmails { get; set; }
        /// <summary>
        /// Gets or sets the URL referrer.
        /// </summary>
        /// <value>The URL referrer.</value>
        public string UrlReferrer { get; set; }
        /// <summary>
        /// Gets or sets the foreign identifier.
        /// </summary>
        /// <value>The foreign identifier.</value>
        public int? ForeignId { get; set; }
        /// <summary>
        /// Gets or sets the foreign unique identifier.
        /// </summary>
        /// <value>The foreign unique identifier.</value>
        public System.Guid? ForeignGuid { get; set; }
        /// <summary>
        /// Gets or sets the foreign key.
        /// </summary>
        /// <value>The foreign key.</value>
        public string ForeignKey { get; set; }
        /// <summary>
        /// Gets or sets the recipients.
        /// </summary>
        /// <value>The recipients.</value>
        public List<ImportSentCommunicationRecipientApiModel> Recipients { get; set; }
    }

    /// <summary>
    /// Class ImportSentCommunicationRecipientApiModel.
    /// Implements the <see cref="Rock.Rest.Utility.IMappedApiModel" />
    /// </summary>
    /// <seealso cref="Rock.Rest.Utility.IMappedApiModel" />
    public class ImportSentCommunicationRecipientApiModel : IMappedApiModel
    {
        /// <summary>
        /// Gets or sets the unique message identifier.
        /// </summary>
        /// <value>The unique message identifier.</value>
        public string UniqueMessageId { get; set; }
        /// <summary>
        /// Gets or sets the person alias identifier.
        /// </summary>
        /// <value>The person alias identifier.</value>
        public int? PersonAliasId { get; set; }
        /// <summary>
        /// Gets or sets the medium entity type identifier.
        /// </summary>
        /// <value>The medium entity type identifier.</value>
        public int? MediumEntityTypeId { get; set; }
        /// <summary>
        /// Gets or sets the sent message.
        /// </summary>
        /// <value>The sent message.</value>
        public string SentMessage { get; set; }
        /// <summary>
        /// Gets or sets the personal device identifier.
        /// </summary>
        /// <value>The personal device identifier.</value>
        public int? PersonalDeviceId { get; set; }
        /// <summary>
        /// Gets or sets the foreign identifier.
        /// </summary>
        /// <value>The foreign identifier.</value>
        public int? ForeignId { get; set; }
        /// <summary>
        /// Gets or sets the foreign unique identifier.
        /// </summary>
        /// <value>The foreign unique identifier.</value>
        public System.Guid? ForeignGuid { get; set; }
        /// <summary>
        /// Gets or sets the foreign key.
        /// </summary>
        /// <value>The foreign key.</value>
        public string ForeignKey { get; set; }
    }

    /// <summary>
    /// Class ImportSentCommunicationResultApiModel.
    /// </summary>
    public class ImportSentCommunicationResultApiModel
    {
        /// <summary>
        /// Gets or sets the communication count.
        /// </summary>
        /// <value>The communication count.</value>
        public int CommunicationCount { get; set; }
        /// <summary>
        /// Gets or sets the details.
        /// </summary>
        /// <value>The details.</value>
        public ImportSentCommunicationResultDetailsApiModel Details { get; set; }
    }

    /// <summary>
    /// Class ImportSentCommunicationResultDetailsApiModel.
    /// </summary>
    public class ImportSentCommunicationResultDetailsApiModel
    {
        /// <summary>
        /// Gets or sets the communication unique identifier.
        /// </summary>
        /// <value>The communication unique identifier.</value>
        public System.Guid CommunicationGuid { get; set; }
        /// <summary>
        /// Gets or sets the communication recipient guids.
        /// </summary>
        /// <value>The communication recipient guids.</value>
        public List<System.Guid> CommunicationRecipientGuids { get; set; }
        /// <summary>
        /// Gets or sets the recipient count.
        /// </summary>
        /// <value>The recipient count.</value>
        public int RecipientCount { get; set; }
    }
    #endregion Api Models
}