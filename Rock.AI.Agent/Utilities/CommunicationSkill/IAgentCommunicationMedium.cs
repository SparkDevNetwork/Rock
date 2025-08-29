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
        List<string> ValidateRecipients( List<Rock.Model.Person> recipient );

        Task<DraftResult> DraftAsync(
            Kernel kernel,
            DraftRequest request );

        Rock.Model.Communication BuildCommunication(
            DraftRequest request,
            List<Rock.Model.Person> recipients,
            DraftResult content );

        void UpdateCommunication(
            DraftRequest request,
            List<Rock.Model.Person> recipients,
            Rock.Model.Communication communication,
            DraftResult content );
    }
}
