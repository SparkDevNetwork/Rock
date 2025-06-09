// <copyright>
// Copyright by the Spark Development Network
//
// Licensed under the Rock Community License (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.rockrms.com/license
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
// </copyright>
//
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
#if WEBFORMS
using System.Web.UI;
using System.Web.UI.WebControls;
#endif

using Rock.Attribute;
using Rock.Data;
using Rock.Extension;
using Rock.Net;
using Rock.Security;
using Rock.ViewModels.Controls;

#if REVIEW_NET5_0_OR_GREATER
using EFDbContext = Microsoft.EntityFrameworkCore.DbContext;
#else
using EFDbContext = System.Data.Entity.DbContext;
#endif

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
        /// Gets the section that this will appear in in the Field Selector
        /// </summary>
        /// <value>
        /// The section.
        /// </value>
        public virtual string Section => "Advanced";

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

        /// <summary>
        /// The PropertyName of the property in the anonymous class returned by the SelectExpression
        /// </summary>
        /// <value>
        /// The name of the column property.
        /// </value>
        public abstract string ColumnPropertyName { get; }

        /// <summary>
        /// Gets the type of the column field.
        /// Override this property to specify a Type other than the default of System.String
        /// </summary>
        /// <value>
        /// The type of the column field.
        /// </value>
        public virtual Type ColumnFieldType => typeof( string );

        /// <summary>
        /// Gets the default column header text.
        /// Override this property to specify a Header that is different from the ColumnPropertyName.
        /// </summary>
        /// <value>
        /// The default column header text.
        /// </value>
        public virtual string ColumnHeaderText => ColumnPropertyName;

        /// <summary>
        /// The URL that will be used to load the Obsidian component. This may
        /// be a path prefixed with "~/" instead of a full absolute URL. This should
        /// return <c>null</c> to indicate Obsidian is not supported and an empty
        /// string to indicate it is supported but no UI is required.
        /// </summary>
        public virtual string ObsidianFileUrl => null;

        #endregion

        #region Configuration

        /// <summary>
        /// Gets the definition of the Obsidian component that will be used to
        /// render the UI for editing the data select.
        /// </summary>
        /// <param name="entityType">The <see cref="Type"/> of the entity this applies to, such as <see cref="Model.Person"/>.</param>
        /// <param name="selection">The selection string from the database.</param>
        /// <param name="rockContext">The context to use for any database access that is required.</param>
        /// <param name="requestContext">The context describing the current request.</param>
        /// <returns>An instance of <see cref="DynamicComponentDefinitionBag"/> that describes how to render the UI.</returns>
        [RockInternal( "17.0" )]
        public virtual DynamicComponentDefinitionBag GetComponentDefinition( Type entityType, string selection, RockContext rockContext, RockRequestContext requestContext )
        {
            return null;
        }

        /// <summary>
        /// Executes a request that is sent from the UI component to the server
        /// component. This is used to handle any dynamic updates that are
        /// required by the UI in order to operate correctly.
        /// </summary>
        /// <param name="request">The request object from the UI component.</param>
        /// <param name="securityGrant">The security grant that is providing additional authorization to this request.</param>
        /// <param name="rockContext">The context to use when accessing the database.</param>
        /// <param name="requestContext">The context that describes the current network request being processed.</param>
        /// <returns>A dictionary of values that will be returned to the UI component.</returns>
        [RockInternal( "17.0" )]
        public virtual Dictionary<string, string> ExecuteComponentRequest( Dictionary<string, string> request, SecurityGrant securityGrant, RockContext rockContext, RockRequestContext requestContext )
        {
            return null;
        }

        /// <summary>
        /// Gets the component data that will be provided to the Obsidian component
        /// when it is initialized. This should include representations of the current
        /// values as well as any additional data required to initialize the UI.
        /// </summary>
        /// <param name="entityType">The <see cref="Type"/> of the entity this applies to, such as <see cref="Model.Person"/>.</param>
        /// <param name="selection">The selection string from the database.</param>
        /// <param name="rockContext">The context to use if access to the database is required.</param>
        /// <param name="requestContext">The context that describes the current request.</param>
        /// <returns>A dictionary of strings that will be provided to the Obsidian component.</returns>
        [RockInternal( "17.0" )]
        public virtual Dictionary<string, string> GetObsidianComponentData( Type entityType, string selection, RockContext rockContext, RockRequestContext requestContext )
        {
            return new Dictionary<string, string>();
        }

        /// <summary>
        /// Gets the selection string that will be saved to the database from
        /// the data returned by the Obsidian component.
        /// </summary>
        /// <param name="entityType">The <see cref="Type"/> of the entity this applies to, such as <see cref="Model.Person"/>.</param>
        /// <param name="data">The data the was returned by the Obsidian component.</param>
        /// <param name="rockContext">The context to use if access to the database is required.</param>
        /// <param name="requestContext">The context that describes the current request.</param>
        /// <returns>The string of text that represents the selection which will be written to the database.</returns>
        [RockInternal( "17.0" )]
        public virtual string GetSelectionFromObsidianComponentData( Type entityType, Dictionary<string, string> data, RockContext rockContext, RockRequestContext requestContext )
        {
            return string.Empty;
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Gets the title of the DataSelectComponent.
        /// Override this property to specify a Title that is different from the ColumnPropertyName.
        /// </summary>
        /// <value>
        /// The title.
        /// </value>
        public virtual string GetTitle( Type entityType )
        {
            return this.ColumnPropertyName;
        }

        /// <summary>
        /// Comma-delimited list of the Entity properties that should be used for Sorting. Normally, you should leave this as null which will make it sort on the returned field 
        /// To disable sorting for this field, return string.Empty;
        /// </summary>
        /// <value>
        /// The sort expression.
        /// </value>
        public virtual string SortProperties( string selection )
        {
            return null;
        }

        /// <summary>
        /// Override this and set to true to have this field sort in the opposite direction
        /// Normally this should be left as false unless there is a special case where it makes sense have it sort reversed
        /// </summary>
        /// <param name="selection">The selection.</param>
        /// <returns></returns>
        /// <value></value>
        public virtual bool SortReversed( string selection )
        {
            return false;
        }

        /// <summary>
        /// Gets the attribute value from selection.
        /// </summary>
        /// <param name="attributeKey">The attribute key.</param>
        /// <param name="selection">The selection.</param>
        /// <returns></returns>
        public string GetAttributeValueFromSelection( string attributeKey, string selection )
        {
            Dictionary<string, string> values;
            try
            {
                values = Newtonsoft.Json.JsonConvert.DeserializeObject( selection, typeof( Dictionary<string, string> ) ) as Dictionary<string, string>;
            }
            catch
            {
                return string.Empty;
            }

            if ( values != null && values.ContainsKey( attributeKey ) )
            {
                return values[attributeKey];
            }

            return string.Empty;
        }

#if WEBFORMS
        /// <summary>
        /// Gets the grid field.
        /// </summary>
        /// <param name="entityType">Type of the entity.</param>
        /// <param name="selection">The selection.</param>
        /// <returns></returns>
        public virtual System.Web.UI.WebControls.DataControlField GetGridField( Type entityType, string selection )
        {
            BoundField result = Rock.Web.UI.Controls.Grid.GetGridField( this.ColumnFieldType );
            return result;
        }

        /// <summary>
        /// Creates the child controls.
        /// </summary>
        /// <returns></returns>
        public virtual Control[] CreateChildControls( Control parentControl )
        {
            string validationGroup = null;
            PlaceHolder phAttributes = new PlaceHolder();
            phAttributes.ID = parentControl.ID + "_phAttributes";
            parentControl.Controls.Add( phAttributes );
            Rock.Attribute.Helper.AddEditControls( this, phAttributes, true, validationGroup, new List<string>() { "Active", "Order" } );

            return new Control[1] { phAttributes };
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
            return GetAttributesSelectionValues( controls ).ToJson();
        }

        /// <summary>
        /// Gets the attributes selection values.
        /// </summary>
        /// <returns></returns>
        private Dictionary<string, string> GetAttributesSelectionValues( Control[] controls )
        {
            PlaceHolder phAttributes = controls.FirstOrDefault( a => a.ID.EndsWith( "_phAttributes" ) ) as PlaceHolder;
            Dictionary<string, string> values = new Dictionary<string, string>();
            if ( this.Attributes != null )
            {
                foreach ( var attribute in this.Attributes )
                {
                    Control control = phAttributes.FindControl( string.Format( "attribute_field_{0}", attribute.Value.Id ) );
                    if ( control != null )
                    {
                        string value = attribute.Value.FieldType.Field.GetEditValue( control, attribute.Value.QualifierValues );
                        values.Add( attribute.Key, value );
                    }
                }
            }

            return values;
        }

        /// <summary>
        /// Sets the attributes selection values.
        /// </summary>
        /// <param name="controls">The controls.</param>
        /// <param name="selection">The selection.</param>
        private void SetAttributesSelectionValues( Control[] controls, string selection )
        {
            Dictionary<string, string> values = null;
            try
            {
                values = Newtonsoft.Json.JsonConvert.DeserializeObject( selection, typeof( Dictionary<string, string> ) ) as Dictionary<string, string>;
            }
            catch
            {
                // intentionally ignore if selection is corrupt
            }

            values = values ?? new Dictionary<string, string>();

            if ( this.Attributes != null )
            {
                var allControls = new List<Control>();
                foreach ( var control in controls )
                {
                    allControls.Add( control );
                    allControls.AddRange( control.ControlsOfTypeRecursive<Control>() );
                }

                foreach ( var attributeKey in values.Keys )
                {
                    var attribute = this.Attributes[attributeKey];
                    if ( attribute != null )
                    {
                        Control control = allControls.FirstOrDefault( a => a.ID == string.Format( "attribute_field_{0}", attribute.Id ) );
                        if ( control != null )
                        {
                            attribute.FieldType.Field.SetEditValue( control, attribute.QualifierValues, values[attribute.Key] );
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Sets the selection.
        /// </summary>
        /// <param name="controls">The controls.</param>
        /// <param name="selection">The selection.</param>
        public virtual void SetSelection( Control[] controls, string selection )
        {
            SetAttributesSelectionValues( controls, selection );
        }
#endif

        /// <summary>
        /// Gets the expression.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="entityIdProperty">The entity identifier property.</param>
        /// <param name="selection">The selection.</param>
        /// <returns></returns>
        public abstract Expression GetExpression( RockContext context, MemberExpression entityIdProperty, string selection );

        /// <summary>
        /// Gets the expression.
        /// override this for non-RockContext Expressions
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="entityIdProperty">The entity identifier property.</param>
        /// <param name="selection">The selection.</param>
        /// <returns></returns>
        public virtual Expression GetExpression( EFDbContext context, MemberExpression entityIdProperty, string selection )
        {
            return GetExpression( context as RockContext, entityIdProperty, selection );
        }

        /// <summary>
        /// Returns a <see cref="System.String" /> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String" /> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            string title = null;
            try
            {
                title = this.GetTitle( null );
            }
            catch
            {
                //
            }
            
            if (!string.IsNullOrWhiteSpace(title))
            {
                return title;
            }
            else
            { 
                return base.ToString();
            }
        }

        #endregion
    }
}