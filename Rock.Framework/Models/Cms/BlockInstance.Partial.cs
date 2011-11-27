using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Runtime.Caching;
using System.Text;

namespace Rock.Models.Cms
{
    public partial class BlockInstance
    {
        /// <summary>
        /// Gets the supported actions.
        /// </summary>
        public override List<string> SupportedActions
        {
            get { return new List<string>() { "View", "Edit", "Configure" }; }
        }

        /// <summary>
        /// Returns a <see cref="string"/> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="string"/> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            return this.Name;
        }
    }
}
