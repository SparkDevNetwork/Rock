using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rock.Workflow.FormBuilder
{
    internal class PersonEntryAdditionalSettings
    {
        /// <summary>
        /// Stores whether the Person Entry section of the Form Builder should allow InactiveCampus
        /// </summary>
        public bool? IncludeInactiveCampus { get; set; } = true;
    }
}
