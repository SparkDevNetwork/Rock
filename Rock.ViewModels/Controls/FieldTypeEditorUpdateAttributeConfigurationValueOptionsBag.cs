using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rock.ViewModels.Controls
{
    /// <summary>
    /// Contains information required to update a field type's configuration.
    /// </summary>
    public class FieldTypeEditorUpdateAttributeConfigurationValueOptionsBag
    {
        /// <summary>
        /// Gets or sets the field type unique identifier.
        /// </summary>
        /// <value>The field type unique identifier.</value>
        public Guid FieldTypeGuid { get; set; }

        /// <summary>
        /// Gets or sets the attribute unique identifier.
        /// </summary>
        /// <value>The attribute unique identifier.</value>
        public Guid AttributeGuid { get; set; }

        /// <summary>
        /// Gets or sets the configuration value Key.
        /// </summary>
        /// <remarks>
        /// See: Rock.Field.IFieldType.GetPublicConfigurationValues()
        /// </remarks>
        /// <value>The configuration value key.</value>
        public string ConfigurationKey { get; set; }

        /// <summary>
        /// Gets or sets the configuration value currently set.
        /// </summary>
        /// <value>The configuration value currently set.</value>
        public string ConfigurationValue { get; set; }
    }

}
