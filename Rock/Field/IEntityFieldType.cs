using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.UI;
using Rock.Web.UI.Controls;

namespace Rock.Field
{
    public interface IEntityFieldType : IFieldType
    {
        /// <summary>
        /// Gets the edit value as the IEntity.Id
        /// </summary>
        /// <param name="control">The control.</param>
        /// <param name="configurationValues">The configuration values.</param>
        /// <returns></returns>
        int? GetEditValueAsEntityId( Control control, Dictionary<string, ConfigurationValue> configurationValues );

        /// <summary>
        /// Sets the edit value from IEntity.Id value
        /// </summary>
        /// <param name="control">The control.</param>
        /// <param name="configurationValues">The configuration values.</param>
        /// <param name="id">The identifier.</param>
        void SetEditValueFromEntityId( Control control, Dictionary<string, ConfigurationValue> configurationValues, int? id );
    }
}
