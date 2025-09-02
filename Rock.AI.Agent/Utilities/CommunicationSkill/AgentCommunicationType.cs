using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rock.AI.Agent.Utilities.CommunicationSkill
{
    /// <summary>
    /// A list of the supported communication types for the agent (used for drafting + sending).
    /// </summary>
    internal enum AgentCommunicationType
    {
        Email,
        Sms,
        Push
    }
}
