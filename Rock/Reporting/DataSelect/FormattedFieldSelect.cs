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
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.Core.Metadata.Edm;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Linq.Expressions;
using System.Runtime.Serialization;
using System.Web.UI.WebControls;

using Rock;
using Rock.Data;
using Rock.Reporting;
using Rock.Web.UI.Controls;
using Newtonsoft.Json;

namespace Rock.Reporting.DataSelect
{
    /// <summary>
    /// Generic implementation of a Formatted Field DataSelect component that
    /// uses the TargetEntityType to give the user a drop down list of all
    /// available properties to report on. The value is parsed through a
    /// Lava filter to allow the user to format it how they wish.
    /// </summary>
    /// <typeparam name="TargetEntityType"></typeparam>
    public abstract class FormattedFieldSelect<TargetEntityType> : DataSelectComponent where TargetEntityType : IEntity
    {
        #region Properties

        /// <summary>
        /// Gets the name of the entity type. Filter should be an empty string
        /// if it applies to all entities
        /// </summary>
        /// <value>
        /// The name of the entity type.
        /// </value>
        public override string AppliesToEntityType
        {
            get
            {
                return typeof( TargetEntityType ).FullName;
            }
        }

        /// <summary>
        /// Gets the section that this will appear in in the Field Selector
        /// </summary>
        /// <value>
        /// The section.
        /// </value>
        public override string Section
        {
            get
            {
                return base.Section;
            }
        }

        /// <summary>
        /// The PropertyName of the property in the anonymous class returned by the SelectExpression
        /// </summary>
        /// <value>
        /// The name of the column property.
        /// </value>
        public override string ColumnPropertyName
        {
            get
            {
                return "FormattedField";
            }
        }

        /// <summary>
        /// Gets the type of the column field.
        /// </summary>
        /// <value>
        /// The type of the column field.
        /// </value>
        public override Type ColumnFieldType
        {
            get
            {
                return typeof( object );
            }
        }

        /// <summary>
        /// Gets the default column header text.
        /// </summary>
        /// <value>
        /// The default column header text.
        /// </value>
        public override string ColumnHeaderText
        {
            get
            {
                return "Formatted Field";
            }
        }

        #endregion

        #region Methods

        /// <summary>
        /// Gets the title.
        /// </summary>
        /// <param name="entityType"></param>
        /// <returns></returns>
        /// <value>
        /// The title.
        /// </value>
        public override string GetTitle( Type entityType )
        {
            return "Formatted Field";
        }

        /// <summary>
        /// Comma-delimited list of the Entity properties that should be used for Sorting. Normally, you should leave this as null which will make it sort on the returned field 
        /// To disable sorting for this field, return string.Empty;
        /// </summary>
        /// <value>
        /// The sort expression.
        /// </value>
        public override string SortProperties( string selection )
        {
            SelectionData data = DeserializeSelectionData( selection );

            /* We don't know yet, assume yes. */
            if ( data.Property == null )
            {
                return null;
            }

            /* IEnumerable types are usually to-many relationships and
             * thus cannot be sorted. */
            if ( typeof( IEnumerable ).IsAssignableFrom( typeof( TargetEntityType ).GetProperty( data.Property ).PropertyType ) )
            {
                return string.Empty;
            }

            if ( !string.IsNullOrWhiteSpace( data.SortProperty ) )
            {
                return string.Format( "{0}.{1}", data.Property, data.SortProperty );
            }

            return data.Property;
        }

        /// <summary>
        /// Gets the grid field.
        /// </summary>
        /// <param name="entityType">Type of the entity.</param>
        /// <param name="selection">The selection.</param>
        /// <returns></returns>
        public override System.Web.UI.WebControls.DataControlField GetGridField( Type entityType, string selection )
        {
            SelectionData data = DeserializeSelectionData( selection );

            var result = new LavaBoundField();
            result.LavaTemplate = data.Template;
            result.LavaKey = data.Property;

            /* Legacy options add about 0.3s per column. Disable them. */
            var options = new Rock.Lava.CommonMergeFieldsOptions();
            result.LavaMergeFields = Rock.Lava.LavaHelper.GetCommonMergeFields( null, null, options );

            return result;
        }

        /// <summary>
        /// Gets the expression.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="entityIdProperty">The entity identifier property.</param>
        /// <param name="selection">The selection.</param>
        /// <returns></returns>
        public override Expression GetExpression( RockContext context, MemberExpression entityIdProperty, string selection )
        {
            SelectionData data = DeserializeSelectionData( selection );

            var query = LinqSelectForProperty( context, typeof( TargetEntityType ), data.Property, "o" );

            return SelectExpressionExtractor.Extract( query, entityIdProperty, "o" );
        }

        /// <summary>
        /// Creates the child controls.
        /// </summary>
        /// <param name="parentControl"></param>
        /// <returns></returns>
        public override System.Web.UI.Control[] CreateChildControls( System.Web.UI.Control parentControl )
        {
            var ddlProperty = new RockDropDownList();
            ddlProperty.HelpBlock.Text = "Select the property you wish to include in the report";
            ddlProperty.ID = parentControl.ID + "_0";
            ddlProperty.Label = "Property";
            ddlProperty.SelectedIndexChanged += ddlProperty_SelectedIndexChanged;
            ddlProperty.AutoPostBack = true;
            parentControl.Controls.Add( ddlProperty );

            /* Get all the DataMember properties that are not hidden in reporting. */
            var properties = typeof( TargetEntityType )
                .GetProperties()
                .Where( p => System.Attribute.IsDefined( p, typeof( DataMemberAttribute ) ) )
                .Where( p => !System.Attribute.IsDefined( p, typeof( NotMappedAttribute ) ) )
                .Where( p => !System.Attribute.IsDefined( p, typeof( HideFromReportingAttribute ) ) )
                .Select( p => p.Name )
                .ToList();

            /* Some of the navigation properties are not marked as DataMember so add missing ones in. */
            foreach ( var p in GetNavigationPropertyNames( typeof( TargetEntityType ) ) )
            {
                if ( !properties.Contains( p ) )
                {
                    properties.Add( p );
                }
            }

            /* Add each of the available properties to the drop down list. */
            ddlProperty.Items.Add( new ListItem() );
            foreach ( var prop in properties.OrderBy( n => n ) )
            {
                ddlProperty.Items.Add( prop );
            }

            /* Direct access the postback variables to set the currently selected property value. */
            string selectedPropertyValue = parentControl.Page.Request.Params[ddlProperty.UniqueID];
            ddlProperty.SelectedValue = selectedPropertyValue;

            /* Create the sort property drop down. */
            var ddlSortProperty = new RockDropDownList();
            ddlSortProperty.HelpBlock.Text = "The selected property is a linked entity and cannot be sorted on directly. Select the sub-property you wish to sort by.";
            ddlSortProperty.ID = parentControl.ID + "_1";
            ddlSortProperty.Label = "Sort-by Property";
            parentControl.Controls.Add( ddlSortProperty );
            PopulateSortProperties( ddlProperty.SelectedValue, ddlSortProperty );

            /* Create the code editor they use to provide Lava formatting syntax. */
            CodeEditor codeEditor = new CodeEditor();
            codeEditor.HelpBlock.Text = "Use Lava syntax to get format the value of the selected property. The lava variable uses the same name as the property name. Common merge fields such as CurrentPerson are also available.";
            codeEditor.EditorMode = CodeEditorMode.Lava;
            codeEditor.ID = parentControl.ID + "_2";
            codeEditor.Label = "Template";
            parentControl.Controls.Add( codeEditor );

            return new System.Web.UI.Control[] { ddlProperty, ddlSortProperty, codeEditor };
        }

        /// <summary>
        /// Populate the list of available sort sub-properties when the user changes the
        /// selected property index.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ddlProperty_SelectedIndexChanged( object sender, EventArgs e )
        {
            var ddlProperty = ( RockDropDownList ) sender;
            var ddlSortProperty = ( RockDropDownList ) ddlProperty.Parent.Controls[1];

            PopulateSortProperties( ddlProperty.SelectedValue, ddlSortProperty );
        }

        /// <summary>
        /// Populate the list of properties under the selected property that can
        /// be used for sorting.
        /// </summary>
        /// <param name="propertyName">The name of the selected property.</param>
        /// <param name="ddlSortProperty">RockDropDownList to populate.</param>
        protected void PopulateSortProperties( string propertyName, RockDropDownList ddlSortProperty )
        {
            var oldSelection = ddlSortProperty.SelectedValue;

            ddlSortProperty.Items.Clear();
            ddlSortProperty.Items.Add( new ListItem() );

            if ( !string.IsNullOrWhiteSpace( propertyName ) )
            {
                var targetProperty = typeof( TargetEntityType ).GetProperty( propertyName );
                if ( targetProperty != null )
                {
                    var properties = targetProperty
                        .PropertyType
                        .GetProperties()
                        .Where( p => System.Attribute.IsDefined( p, typeof( DataMemberAttribute ) ) )
                        .Where( p => !System.Attribute.IsDefined( p, typeof( NotMappedAttribute ) ) )
                        .Where( p => !System.Attribute.IsDefined( p, typeof( HideFromReportingAttribute ) ) )
                        .Where( p => !typeof( IEnumerable ).IsAssignableFrom( p.PropertyType ) )
                        .Where( p => !typeof( IEntity ).IsAssignableFrom( p.PropertyType ) )
                        .Select( p => p.Name );

                    foreach ( var prop in properties )
                    {
                        ddlSortProperty.Items.Add( prop );
                    }
                }
            }

            if ( ddlSortProperty.Items.FindByValue( oldSelection ) != null )
            {
                ddlSortProperty.SelectedValue = oldSelection;
            }

            ddlSortProperty.Visible = ddlSortProperty.Items.Count > 1;
        }

        /// <summary>
        /// Renders the controls.
        /// </summary>
        /// <param name="parentControl">The parent control.</param>
        /// <param name="writer">The writer.</param>
        /// <param name="controls">The controls.</param>
        public override void RenderControls( System.Web.UI.Control parentControl, System.Web.UI.HtmlTextWriter writer, System.Web.UI.Control[] controls )
        {
            base.RenderControls( parentControl, writer, controls );
        }

        /// <summary>
        /// Gets the selection.
        /// </summary>
        /// <param name="controls">The controls.</param>
        /// <returns></returns>
        public override string GetSelection( System.Web.UI.Control[] controls )
        {
            if ( controls.Count() == 3 )
            {
                RockDropDownList ddlProperty = controls[0] as RockDropDownList;
                RockDropDownList ddlSortProperty = controls[1] as RockDropDownList;
                CodeEditor codeEditor = controls[2] as CodeEditor;

                if ( codeEditor != null && ddlProperty != null )
                {
                    SelectionData data = new SelectionData();

                    data.Template = codeEditor.Text;
                    data.Property = ddlProperty.SelectedValue;
                    data.SortProperty = ddlSortProperty.SelectedValue;

                    return JsonConvert.SerializeObject( data );
                }
            }

            return string.Empty;
        }

        /// <summary>
        /// Sets the selection.
        /// </summary>
        /// <param name="controls">The controls.</param>
        /// <param name="selection">The selection.</param>
        public override void SetSelection( System.Web.UI.Control[] controls, string selection )
        {
            if ( controls.Count() == 3 )
            {
                RockDropDownList ddlProperty = controls[0] as RockDropDownList;
                RockDropDownList ddlSortProperty = controls[1] as RockDropDownList;
                CodeEditor codeEditor = controls[2] as CodeEditor;

                if ( codeEditor != null && ddlProperty != null && ddlSortProperty != null )
                {
                    SelectionData data = DeserializeSelectionData( selection );

                    codeEditor.Text = data.Template ?? string.Empty;
                    ddlProperty.SelectedValue = data.Property ?? string.Empty;

                    PopulateSortProperties( ddlProperty.SelectedValue, ddlSortProperty );
                    ddlSortProperty.SelectedValue = data.SortProperty;
                }
            }
        }

        /// <summary>
        /// Get the names of all the Navigation Properties for a given Entity Type. These are
        /// the properties that can be used to navigate to another entity, such as Group.Schedule
        /// </summary>
        /// <param name="entityType">The IEntity class type to extract navigation properties from.</param>
        /// <returns>A list of strings that represent the property names.</returns>
        static public List<string> GetNavigationPropertyNames( Type entityType )
        {
            var dbContext = Rock.Reflection.GetDbContextForEntityType( entityType );
            var workspace = ( ( IObjectContextAdapter ) new RockContext() ).ObjectContext.MetadataWorkspace;
            var itemCollection = ( ObjectItemCollection ) ( workspace.GetItemCollection( DataSpace.OSpace ) );
            var metaEntityType = itemCollection
                .OfType<EntityType>()
                .Single( e => itemCollection.GetClrType( e ) == entityType );

            return metaEntityType.NavigationProperties.Select( p => p.Name ).ToList();
        }

        /// <summary>
        /// Build an IQueryable that selects the property from the entity type. This is
        /// done dynamically so we do not need to know the property names at compile time.
        /// </summary>
        /// <param name="context">The database context to operate in.</param>
        /// <param name="entityType">The IEntity type that we will be selecting a property from.</param>
        /// <param name="propertyName">The name of the property we are going to Select.</param>
        /// <param name="expressionName">The name to use in the expression for the object.</param>
        /// <returns>An IQueryable that represents an entityType.Select(e => e.propertyName ) statement.</returns>
        static public IQueryable LinqSelectForProperty( RockContext context, Type entityType, string propertyName, string expressionName )
        {
            /* Get the service and an IQueryable object from it. */
            var service = Rock.Reflection.GetServiceForEntityType( entityType, context );
            var getQueryable = service.GetType().GetMethod( "Queryable", new Type[] { } );
            var queryable = getQueryable.Invoke( service, null ) as IQueryable;

            /* Build the property that can be used in LINQ. */
            var prop = entityType.GetProperty( propertyName );
            var param = Expression.Parameter( entityType, expressionName );
            var pred = Expression.Lambda( Expression.Property( param, prop ), param );

            /* Build the Linq.Select() expression. */
            var expr = Expression.Call( typeof( Queryable ),
                "Select",
                new Type[] { entityType, prop.PropertyType },
                Expression.Constant(queryable),
                pred );

            /* Wrap it in an IQueryable as that is what the helper method needs later */
            return queryable.Provider.CreateQuery( expr );
        }

        /// <summary>
        /// Deserialize the selection data or generate a new object if the data was not
        /// valid.
        /// </summary>
        /// <param name="selection">The serialized selection data.</param>
        /// <returns>A SelectionData instance that contains the data from the last selection.</returns>
        protected SelectionData DeserializeSelectionData( string selection )
        {
            try
            {
                SelectionData data = JsonConvert.DeserializeObject<SelectionData>( selection );
                return data ?? new SelectionData();
            }
            catch
            {
                return new SelectionData();
            }
        }

        #endregion

        /// <summary>
        /// Helper class for encoding data as JSON. We do this because we need to store
        /// multiple values, and one of the values could have any kind of text in it so the
        /// usual method of string as "val1|val2" is not safe.
        /// </summary>
        protected class SelectionData
        {
            /// <summary>
            /// Gets or sets the template.
            /// </summary>
            /// <value>
            /// The template.
            /// </value>
            public string Template { get; set; }

            /// <summary>
            /// Gets or sets the property.
            /// </summary>
            /// <value>
            /// The property.
            /// </value>
            public string Property { get; set; }

            /// <summary>
            /// Gets or sets the sort property.
            /// </summary>
            /// <value>
            /// The sort property.
            /// </value>
            public string SortProperty { get; set; }
        }
    }
}