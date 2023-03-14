using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Rock.AI.Classes.Completions;
using Rock.Extension;

namespace Rock.AI.Provider
{
    internal abstract class AIProviderComponent : Component
    {

        public abstract CompletionResponse GetCompletion( CompletionRequest request );

    }
}
