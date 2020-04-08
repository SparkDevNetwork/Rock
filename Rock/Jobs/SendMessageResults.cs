using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rock.Jobs
{
    /// <summary>
    /// This is the list of results that a task can have.
    /// </summary>
    internal class SendMessageResult
    {
        public List<string> Warnings { get; set; } = new List<string>();
        public List<string> Errors { get; set; } = new List<string>();
        public int MessagesSent { get; set; }
    }
}
