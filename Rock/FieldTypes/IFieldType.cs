//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//

using System.Collections.Generic;
using System.Web.UI;

namespace Rock.FieldTypes
{
    /// <summary>
    /// Interface that a custom field type must implement
    /// </summary>
    public interface IFieldType
    {
        /// <summary>
        /// Gets the qualifiers.
        /// </summary>
        List<FieldQualifier> Qualifiers { get; }

        /// <summary>
        /// Gets or sets the qualifier values.
        /// </summary>
        /// <value>
        /// The qualifier values. The Dictionary's key contains the qualifier key, the KeyValuePair's
        /// key contains the qualifier name, and the KeyValuePair's value contains the qualifier 
        /// value
        /// </value>
        Dictionary<string, KeyValuePair<string, string>> QualifierValues { get; set;  }

        /// <summary>
        /// Formats the value based on the type and qualifiers
        /// </summary>
        /// <param name="parentControl">The parent control.</param>
        /// <param name="value">The value.</param>
        /// <param name="condensed">if set to <c>true</c> [condensed].</param>
        /// <returns></returns>
        string FormatValue( Control parentControl, string value, bool condensed );

        /// <summary>
        /// Creates an HTML control.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <param name="setValue">if set to <c>true</c> [set value].</param>
        /// <returns></returns>
        Control CreateControl( string value, bool setValue );

        /// <summary>
        /// Reads the value of the control.
        /// </summary>
        /// <param name="control">The control.</param>
        /// <returns></returns>
        string ReadValue( Control control );

        /// <summary>
        /// Creates a client-side function that can be called to display appropriate html and event handler to update the target element.
        /// </summary>
        /// <param name="page">The page.</param>
        /// <param name="id">The id.</param>
        /// <param name="parentElement">The parent element.</param>
        /// <param name="targetElement">The target element.</param>
        /// <returns></returns>
        string ClientUpdateScript( Page page, string id, string value, string parentElement, string targetElement );

        /// <summary>
        /// Registers a client change script that will update a target element with a controls value whenever it is changed.
        /// </summary>
        /// <param name="control">The control.</param>
        /// <param name="targetElement">The target element.</param>
        void RegisterClientChangeScript( Control control, string targetElement );
    }
}
