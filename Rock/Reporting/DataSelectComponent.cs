using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Web.UI;
using Rock.Data;
using Rock.Extension;

namespace Rock.Reporting
{
    /// <summary>
    /// 
    /// </summary>
    public abstract class DataSelectComponent : Component
    {

        #region Properties

        /// <summary>
        /// Gets the name of the entity type. Filter should be an empty string
        /// if it applies to all entities
        /// </summary>
        /// <value>
        /// The name of the entity type.
        /// </value>
        public abstract string AppliesToEntityType { get; }

        /// <summary>
        /// Gets the section.
        /// </summary>
        /// <value>
        /// The section.
        /// </value>
        public virtual string Section
        {
            get { return "Other"; }
        }
        
        /// <summary>
        /// The PropertyName of the property in the anonymous class returned by the SelectExpression
        /// </summary>
        /// <value>
        /// The name of the column property.
        /// </value>
        public abstract string ColumnPropertyName { get; }

        /// <summary>
        /// Gets the type of the column field.
        /// </summary>
        /// <value>
        /// The type of the column field.
        /// </value>
        public abstract Type ColumnFieldType { get; }

        /// <summary>
        /// Gets the default column header text.
        /// </summary>
        /// <value>
        /// The default column header text.
        /// </value>
        public abstract string ColumnHeaderText { get; }

        /// <summary>
        /// Gets the attribute value defaults.
        /// </summary>
        /// <value>
        /// The attribute defaults.
        /// </value>
        public override Dictionary<string, string> AttributeValueDefaults
        {
            get
            {
                var defaults = new Dictionary<string, string>();
                defaults.Add( "Active", "True" );
                return defaults;
            }
        }

        #endregion

        #region Methods

        /// <summary>
        /// Gets the title.
        /// </summary>
        /// <value>
        /// The title.
        /// </value>
        public abstract string GetTitle( Type entityType );

        /// <summary>
        /// Formats the selection on the client-side.  When the widget is collapsed by the user, the Filterfield control
        /// will set the description of the filter to whatever is returned by this property.  If including script, the
        /// controls parent container can be referenced through a '$content' variable that is set by the control before
        /// referencing this property.
        /// </summary>
        /// <returns></returns>
        /// <value>
        /// The client format script.
        ///   </value>
        public virtual string GetClientFormatSelection()
        {
            return this.GetTitle( null );
        }

        /// <summary>
        /// Formats the selection.
        /// </summary>
        /// <param name="selection">The selection.</param>
        /// <returns></returns>
        public virtual string FormatSelection( string selection )
        {
            return this.GetTitle( null );
        }

        /// <summary>
        /// Creates the child controls.
        /// </summary>
        /// <returns></returns>
        public virtual Control[] CreateChildControls( Control parentControl )
        {
            return new Control[0];
        }

        /// <summary>
        /// Renders the controls.
        /// </summary>
        /// <param name="parentControl">The parent control.</param>
        /// <param name="writer">The writer.</param>
        /// <param name="controls">The controls.</param>
        public virtual void RenderControls( Control parentControl, HtmlTextWriter writer, Control[] controls )
        {
            foreach ( var control in controls )
            {
                control.RenderControl( writer );
                writer.WriteLine();
            }
        }

        /// <summary>
        /// Gets the selection.
        /// This is typically a string that contains the values selected with the Controls
        /// </summary>
        /// <param name="controls">The controls.</param>
        /// <returns></returns>
        public virtual string GetSelection( Control[] controls )
        {
            return string.Empty;
        }

        /// <summary>
        /// Sets the selection.
        /// </summary>
        /// <param name="controls">The controls.</param>
        /// <param name="selection">The selection.</param>
        public virtual void SetSelection( Control[] controls, string selection )
        {
            string[] selectionValues = selection.Split( '|' );
        }

        /// <summary>
        /// Gets the expression.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="entityIdProperty">The entity identifier property.</param>
        /// <param name="selection">The selection.</param>
        /// <returns></returns>
        public abstract Expression GetExpression( RockContext context, Expression entityIdProperty, string selection );

        #endregion

    }

    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public abstract class DataSelectComponent<T> : DataSelectComponent
    {

    }
}