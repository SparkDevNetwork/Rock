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
using System.ComponentModel;
using System.Linq;

using Rock.AI.Agent.Annotations;
using Rock.AI.Agent.Classes.Common;
using Rock.AI.Agent.Classes.Entity;
using Rock.AI.Agent.Classes.Skills.FinanceSkill;
using Rock.Model;
using Rock.SystemGuid;
using Rock.Web.Cache;

namespace Rock.AI.Agent.Skills
{
    internal sealed partial class FinanceSkill
    {
        #region Tool(s)

        [Description( "Retrieves the details of a single benevolence request." )]
        [AgentPurpose( "Retrieves the details of a single benevolence request." )]
        [AgentToolGuid( "a318f309-f04f-49de-98cb-68de396cc35f" )]
        public IAgentToolResult GetBenevolenceRequest( string benevolenceRequestIdKey )
        {
            var helper = new AgentToolHelper( AgentRequestContext, _logger );
            var benevolenceTypeIds = GetConfiguredBenevolenceTypes().Select( bt => bt.Id ).ToList();

            var benevolenceRequest = helper.GetRequiredEntity<BenevolenceRequest>( benevolenceRequestIdKey, checkSecurity: true );

            if ( benevolenceRequest != null && !benevolenceTypeIds.Contains( benevolenceRequest.BenevolenceTypeId ) )
            {
                helper.AddError( "That benevolence request is not available." );
            }

            if ( helper.HasErrors )
            {
                return helper.ErrorResult;
            }

            var result =  new BenevolenceRequestResult
            {
                Id = benevolenceRequest.Id,
                Person = PersonResult.NameOnly( benevolenceRequest.RequestedByPersonAlias ),
                FirstName = benevolenceRequest.FirstName,
                LastName = benevolenceRequest.LastName,
                RequestDateTime = benevolenceRequest.RequestDateTime,
                AssignedToPerson = PersonResult.NameOnly( benevolenceRequest.CaseWorkerPersonAlias ),
                RequestText = benevolenceRequest.RequestText.IfEmpty( null ),
                ResultSummary = benevolenceRequest.ResultSummary.IfEmpty( null ),
                RequestStatus = benevolenceRequest.RequestStatusValueId.HasValue
                    ? new KeyNameResult
                    {
                        Id = benevolenceRequest.RequestStatusValueId,
                        Name = benevolenceRequest.RequestStatusValue.Value,
                    }
                    : null,
                NextSteps = benevolenceRequest.ProvidedNextSteps.IfEmpty( null ),
                Documents = benevolenceRequest.Documents
                    .OrderBy( bd => bd.Order )
                    .Select( bd => new BenevolenceDocumentResult
                    {
                        FileName = bd.BinaryFile.FileName,
                        DownloadUrl = AgentRequestContext.ResolveRockUrl( $"~/GetFile.ashx?Guid={bd.BinaryFile.Guid}" ),
                    } )
                    .ToList(),
                Results = benevolenceRequest.BenevolenceResults
                    .Select( br => new BenevolenceResultResult
                    {
                        Amount = br.Amount,
                        Details = br.ResultSummary.IfEmpty( null ),
                        ResultType = new KeyNameResult
                        {
                            Id = br.ResultTypeValueId,
                            Name = DefinedValueCache.Get( br.ResultTypeValueId, AgentRequestContext.RockContext ).Value,
                        }
                    } )
                    .ToList(),
            };

            return Success( result )
                .WithInstructions( "If markdown is supported then render document links using inline link syntax, where the filename is the link text and the URL is the target. Otherwise hide the URL unless specifically asked for it." );
        }

        #endregion
    }
}
