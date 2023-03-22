using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Rock.AI.Classes.Completions;
using Rock.AI.Classes.Moderations;
using Rock.Extension;

namespace Rock.AI.Provider
{
    public abstract class AIProviderComponent : Component
    {

        public abstract Task<CompletionsResponse> GetCompletions( CompletionsRequest request );

        public abstract Task<ModerationsResponse> GetModerations( ModerationsRequest request );
    }
}
