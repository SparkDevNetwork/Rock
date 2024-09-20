using System;

namespace Rock.ViewModels.Blocks.Core.LogViewer
{
    public class RockLogEventsBag
    {
        /// <summary>
        /// Gets or sets the DateTime
        /// </summary>
        public DateTimeOffset DateTime { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string Level { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string Domain { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string Message { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string Exception { get; set; }
    }
}
