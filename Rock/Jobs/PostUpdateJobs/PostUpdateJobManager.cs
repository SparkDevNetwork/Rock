using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rock.Jobs
{
    /// <summary>
    /// A helper class to manage the state when it's safe to run certain PostUpdateJobs.
    /// </summary>
    public static class PostUpdateJobManager
    {
        /// <summary>
        ///  This property will be set to true once the Rock startup BlockType compilation thread is finished.
        /// </summary>
        public static bool IsBlockTypeCompilationFinished { get; set; } = false;
    }
}
