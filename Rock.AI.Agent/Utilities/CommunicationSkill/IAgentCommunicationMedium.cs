using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.SemanticKernel;

namespace Rock.AI.Agent.Utilities.CommunicationSkill
{
    internal interface IAgentCommunicationMedium
    {
        List<string> ValidateRecipient( Rock.Model.Person recipient );

        Task<DraftResult> DraftAsync(
            Kernel kernel,
            DraftRequest request,
            Rock.Model.Person recipient );

        Rock.Model.Communication BuildCommunication(
            DraftRequest request,
            Rock.Model.Person recipient,
            DraftResult content );

        void UpdateCommunication(
            DraftRequest request,
            Rock.Model.Person recipient,
            Rock.Model.Communication communication,
            DraftResult content );
    }
}
