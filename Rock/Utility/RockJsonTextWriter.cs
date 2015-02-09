using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rock.Utility
{
    /// <summary>
    /// A JsonTextWriter that is aware of SerializeInSimpleMode
    /// AttributeCacheJsonConverter and AttributeValueJsonConverter use this to figure out how they should serialize
    /// </summary>
    public class RockJsonTextWriter : Newtonsoft.Json.JsonTextWriter
    {
        /// <summary>
        /// Gets or sets a value indicating whether [serialize in simple mode].
        /// </summary>
        /// <value>
        /// <c>true</c> if [serialize in simple mode]; otherwise, <c>false</c>.
        /// </value>
        public bool SerializeInSimpleMode { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="RockJsonTextWriter"/> class.
        /// </summary>
        /// <param name="textWriter">The text writer.</param>
        /// <param name="serializeInSimpleMode">if set to <c>true</c> [serialize in simple mode].</param>
        public RockJsonTextWriter( System.IO.TextWriter textWriter, bool serializeInSimpleMode )
            : base( textWriter )
        {
            SerializeInSimpleMode = serializeInSimpleMode;
        }
    }
}
