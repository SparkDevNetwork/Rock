using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Rock.AI.Agent.Classes.Common;

namespace Rock.AI.Agent.Classes.Entity
{
    /// <summary>
    /// Represents a note type.
    /// </summary>
    public class NoteTypeResult : EntityResultBase
    {
        /// <summary>
        /// The name of the note type.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// The entity type that this note type is associated with.
        /// </summary>
        public KeyNameResult EntityType { get; set; }
    }
}
