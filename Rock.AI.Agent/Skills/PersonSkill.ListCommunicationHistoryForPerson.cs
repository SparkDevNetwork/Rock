using System;
using System.ComponentModel;
using System.Data.Entity;
using System.Linq;

using Rock.AI.Agent.Annotations;
using Rock.AI.Agent.Classes.Common;
using Rock.AI.Agent.Classes.Entity;
using Rock.AI.Agent.Classes.Skills.PersonSkill;
using Rock.Data;
using Rock.Model;
using Rock.SystemGuid;

namespace Rock.AI.Agent.Skills
{
    internal partial class PersonSkill
    {
        #region Tool(s)

        [Description( "Lists communication records for a specific person." )]
        [AgentPurpose( "Retrieves the communication records that have been sent to an individual." )]
        [AgentToolGuid( "dd7510bb-9176-4463-9b23-665000992a62" )]
        public IAgentToolResult ListCommunicationHistoryForPerson(
            string personIdKey,

            string senderPersonIdKey = null,
            DateTime? startDate = null,
            DateTime? endDate = null,

            int pageNumber = 1 )
        {
            var helper = new AgentToolHelper( AgentRequestContext, _logger );
            var qry = new CommunicationRecipientService( AgentRequestContext.RockContext )
                .Queryable();

            qry = helper.WhereRequiredIdKey( qry, cr => cr.PersonAlias.PersonId, personIdKey );
            qry = helper.WhereOptionalIdKey( qry, cr => cr.Communication.SenderPersonAlias.PersonId, senderPersonIdKey );
            qry = helper.WhereOptionalPropertyBetween( qry, cr => cr.Communication.CreatedDateTime, startDate, endDate );

            if ( helper.HasErrors )
            {
                return helper.ErrorResult;
            }

            var orderedQry = qry
                .AsExpandable()
                .Select( cr => new CommunicationHistoryResult
                {
                    Id = cr.CommunicationId,
                    Name = cr.Communication.Name,
                    MediumEntityTypeId = cr.MediumEntityTypeId,
                    CreatedDateTime = cr.Communication.CreatedDateTime,
                    Sender = PersonResult.NameOnly( cr.Communication.SenderPersonAlias ),
                    SentDateTime = cr.SendDateTime,
                    Status = cr.Status,
                    StatusMessage = cr.StatusNote,
                } )
                .OrderByDescending( a => a.CreatedDateTime )
                .ThenBy( a => a.Id );

            var page = helper.GetPaginatedItems( orderedQry, pageNumber );
            var historyPage = page.WithItems( page.Items
                .Select( a => new KeyNameResult( a.Id, a.Name ) ) );

            return helper.GetPaginatedResult( page, historyPage );
        }

        #endregion
    }
}
