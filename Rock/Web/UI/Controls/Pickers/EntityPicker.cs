// <copyright>
// Copyright 2013 by the Spark Development Network
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.apache.org/licenses/LICENSE-2.0
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
using System.ComponentModel;
using System.Linq;
using System.Web.UI;
using System.Web.UI.WebControls;
using Rock.Data;
using Rock.Field;
using Rock.Model;
using Rock.Web.Cache;

namespace Rock.Web.UI.Controls
{
    /// <summary>
    /// 
    /// </summary>
    public class EntityPicker : CompositeControl, IRockControl
    {
        #region IRockControl implementation (Custom implementation)

        /// <summary>
        /// Gets or sets the label text.
        /// </summary>
        /// <value>
        /// The label text.
        /// </value>
        [
        Bindable( true ),
        Category( "Appearance" ),
        DefaultValue( "" ),
        Description( "The text for the label." )
        ]
        public string Label
        {
            get { return ViewState["Label"] as string ?? string.Empty; }
            set { ViewState["Label"] = value; }
        }

        /// <summary>
        /// Gets or sets the form group class.
        /// </summary>
        /// <value>
        /// The form group class.
        /// </value>
        [
        Bindable( true ),
        Category( "Appearance" ),
        Description( "The CSS class to add to the form-group div." )
        ]
        public string FormGroupCssClass
        {
            get { return ViewState["FormGroupCssClass"] as string ?? string.Empty; }
            set { ViewState["FormGroupCssClass"] = value; }
        }

        /// <summary>
        /// Gets or sets the help text.
        /// </summary>
        /// <value>
        /// The help text.
        /// </value>
        [
        Bindable( true ),
        Category( "Appearance" ),
        DefaultValue( "" ),
        Description( "The help block." )
        ]
        public string Help
        {
            get
            {
                return HelpBlock != null ? HelpBlock.Text : string.Empty;
            }

            set
            {
                if ( HelpBlock != null )
                {
                    HelpBlock.Text = value;
                }
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether this <see cref="RockTextBox"/> is required.
        /// </summary>
        /// <value>
        ///   <c>true</c> if required; otherwise, <c>false</c>.
        /// </value>
        [
        Bindable( true ),
        Category( "Behavior" ),
        DefaultValue( "false" ),
        Description( "Is the value required?" )
        ]
        public bool Required
        {
            get
            {
                EnsureChildControls();
                return _etpEntityType.Required;
            }

            set
            {
                EnsureChildControls();
                _etpEntityType.Required = value;
            }
        }

        /// <summary>
        /// Gets or sets the required error message.  If blank, the LabelName name will be used
        /// </summary>
        /// <value>
        /// The required error message.
        /// </value>
        public string RequiredErrorMessage
        {
            get
            {
                return RequiredFieldValidator != null ? RequiredFieldValidator.ErrorMessage : string.Empty;
            }

            set
            {
                if ( RequiredFieldValidator != null )
                {
                    RequiredFieldValidator.ErrorMessage = value;
                }
            }
        }

        /// <summary>
        /// Gets or sets an optional validation group to use.
        /// </summary>
        /// <value>
        /// The validation group.
        /// </value>
        public string ValidationGroup
        {
            get { return ViewState["ValidationGroup"] as string; }
            set { ViewState["ValidationGroup"] = value; }
        }

        /// <summary>
        /// Gets a value indicating whether this instance is valid.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is valid; otherwise, <c>false</c>.
        /// </value>
        public virtual bool IsValid
        {
            get
            {
                return !Required || RequiredFieldValidator == null || RequiredFieldValidator.IsValid;
            }
        }

        /// <summary>
        /// Gets or sets the help block.
        /// </summary>
        /// <value>
        /// The help block.
        /// </value>
        public HelpBlock HelpBlock { get; set; }

        /// <summary>
        /// Gets or sets the required field validator.
        /// </summary>
        /// <value>
        /// The required field validator.
        /// </value>
        public RequiredFieldValidator RequiredFieldValidator { get; set; }

        #endregion

        #region Controls

        private EntityTypePicker _etpEntityType;
        private PlaceHolder _phEntityTypeEntityIdValue;
        private Control _entityTypeEditControl;

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets the entity type identifier.
        /// </summary>
        /// <value>
        /// The entity type identifier.
        /// </value>
        public int? EntityTypeId
        {
            get
            {
                EnsureChildControls();
                return _etpEntityType.SelectedEntityTypeId;
            }

            set
            {
                EnsureChildControls();
                _etpEntityType.SelectedValue = value.ToString();
                _etpEntityType_SelectedIndexChanged( null, null );
            }
        }

        /// <summary>
        /// Gets or sets the name of the entity type.
        /// </summary>
        /// <value>
        /// The name of the entity type.
        /// </value>
        public string EntityTypeName
        {
            get
            {
                var entityType = EntityTypeCache.Read( this.EntityId ?? 0 );
                if ( entityType != null )
                {
                    return entityType.Name;
                }
                else
                {
                    return null;
                }
            }

            set
            {
                var entityType = EntityTypeCache.Read( value );
                if ( entityType != null )
                {
                    this.EntityTypeId = entityType.Id;
                }
                else
                {
                    this.EntityTypeId = null;
                }
            }
        }

        /// <summary>
        /// Gets or sets the entity identifier.
        /// </summary>
        /// <value>
        /// The entity identifier.
        /// </value>
        public int? EntityId
        {
            get
            {
                EnsureChildControls();

                if ( _entityTypeEditControl == null )
                {
                    return null;
                }

                int? result = null;

                var entityType = EntityTypeCache.Read( this.EntityTypeId ?? 0 );
                if ( entityType != null && entityType.SingleValueFieldType != null && entityType.SingleValueFieldType.Field is IEntityFieldType )
                {
                    result = ( entityType.SingleValueFieldType.Field as IEntityFieldType ).GetEditValueAsEntityId( _entityTypeEditControl, new Dictionary<string, ConfigurationValue>() );
                }

                return result;
            }

            set
            {
                EnsureChildControls();

                if ( _entityTypeEditControl == null )
                {
                    return;
                }

                var entityType = EntityTypeCache.Read( this.EntityTypeId ?? 0 );
                if ( entityType != null && entityType.SingleValueFieldType != null && entityType.SingleValueFieldType.Field is IEntityFieldType )
                {
                    ( entityType.SingleValueFieldType.Field as IEntityFieldType ).SetEditValueFromEntityId( _entityTypeEditControl, new Dictionary<string, ConfigurationValue>(), value );
                }
            }
        }

        /// <summary>
        /// Gets or sets the entity control help text format
        /// Include a {0} in places where you want the EntityType name (Campus, Group, etc) to be included
        /// and/or a {1} in places where you the the pluralized EntityType name (Campuses, Groups, etc) to be included
        /// </summary>
        /// <value>
        /// The entity control help text.
        /// </value>
        public string EntityControlHelpTextFormat
        {
            get
            {
                return ViewState["EntityControlHelpTextFormat"] as string;
            }

            set
            {
                ViewState["EntityControlHelpTextFormat"] = value;
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether [entity type picker visible].
        /// </summary>
        /// <value>
        /// <c>true</c> if [entity type picker visible]; otherwise, <c>false</c>.
        /// </value>
        public bool EntityTypePickerVisible
        {
            get
            {
                return ViewState["EntityTypePickerVisible"] as bool? ?? true;
            }

            set
            {
                ViewState["EntityTypePickerVisible"] = value;
            }
        }

        #endregion

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Load" /> event.
        /// </summary>
        /// <param name="e">The <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnLoad( EventArgs e )
        {
            base.OnLoad( e );
            EnsureChildControls();
            UpdateEntityTypeControls();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="EntityPicker"/> class.
        /// </summary>
        public EntityPicker()
            : base()
        {
            HelpBlock = new HelpBlock();
        }

        /// <summary>
        /// Called by the ASP.NET page framework to notify server controls that use composition-based implementation to create any child controls they contain in preparation for posting back or rendering.
        /// </summary>
        protected override void CreateChildControls()
        {
            base.CreateChildControls();
            Controls.Clear();
            RockControlHelper.CreateChildControls( this, Controls );

            _etpEntityType = new EntityTypePicker();
            _etpEntityType.ID = this.ID + "_etpEntityType";
            _etpEntityType.Required = false;
            _etpEntityType.IncludeGlobalOption = false;
            _etpEntityType.EntityTypes = new EntityTypeService( new RockContext() ).Queryable().Where( a => a.IsEntity == true && a.SingleValueFieldTypeId.HasValue ).ToList();
            _etpEntityType.AutoPostBack = true;
            _etpEntityType.SelectedIndexChanged += _etpEntityType_SelectedIndexChanged;
            Controls.Add( _etpEntityType );

            _phEntityTypeEntityIdValue = new PlaceHolder();
            _phEntityTypeEntityIdValue.ID = this.ID + "_phEntityTypeEntityIdValue";
            _phEntityTypeEntityIdValue.EnableViewState = false;
            Controls.Add( _phEntityTypeEntityIdValue );

            // figure out which picker to render based on the EntityType
            UpdateEntityTypeControls();
        }

        /// <summary>
        /// Updates the entity type controls.
        /// </summary>
        private void UpdateEntityTypeControls()
        {
            _phEntityTypeEntityIdValue.Controls.Clear();

            string fieldTypeName = "Entity";
            Control entityTypeEditControl = null;

            var entityType = EntityTypeCache.Read( this.EntityTypeId ?? 0 );
            if ( entityType != null && entityType.SingleValueFieldType != null )
            {
                fieldTypeName = entityType.SingleValueFieldType.Name;
                entityTypeEditControl = entityType.SingleValueFieldType.Field.EditControl( new Dictionary<string, Field.ConfigurationValue>(), string.Format( "{0}_{1}_Picker", this.ID, fieldTypeName ) );
            }

            // only set the _entityTypeEditControl is needs to be
            if ( _entityTypeEditControl == null || !_entityTypeEditControl.GetType().Equals( entityTypeEditControl.GetType() ) || _entityTypeEditControl.ID != entityTypeEditControl.ID )
            {
                _entityTypeEditControl = entityTypeEditControl;
            }

            if ( _entityTypeEditControl != null )
            {
                if ( _entityTypeEditControl is IRockControl )
                {
                    if ( entityType != null )
                    {
                        ( _entityTypeEditControl as IRockControl ).Label = entityType.FriendlyName;
                    }

                    ( _entityTypeEditControl as IRockControl ).Help = string.Format( EntityControlHelpTextFormat ?? string.Empty, fieldTypeName, fieldTypeName.Pluralize() );
                }

                _phEntityTypeEntityIdValue.Controls.Add( _entityTypeEditControl );
            }
        }

        /// <summary>
        /// Handles the SelectedIndexChanged event of the _etpEntityType control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void _etpEntityType_SelectedIndexChanged( object sender, EventArgs e )
        {
            // figure out which picker to render based on the EntityType
            UpdateEntityTypeControls();
        }

        /// <summary>
        /// Outputs server control content to a provided <see cref="T:System.Web.UI.HtmlTextWriter" /> object and stores tracing information about the control if tracing is enabled.
        /// </summary>
        /// <param name="writer">The <see cref="T:System.Web.UI.HtmlTextWriter" /> object that receives the control content.</param>
        public override void RenderControl( HtmlTextWriter writer )
        {
            if ( this.Visible )
            {
                RockControlHelper.RenderControl( this, writer );
            }
        }

        /// <summary>
        /// Renders the base control.
        /// </summary>
        /// <param name="writer">The writer.</param>
        public void RenderBaseControl( HtmlTextWriter writer )
        {
            _etpEntityType.Visible = EntityTypePickerVisible;
            _etpEntityType.RenderControl( writer );

            _phEntityTypeEntityIdValue.RenderControl( writer );
        }

        /// <summary>
        /// Handles the SelectedIndexChanged event of the rblSelectOrContext control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        public void rblSelectOrContext_SelectedIndexChanged( object sender, EventArgs e )
        {
            // intentionally blank, but we need the postback to fire
        }
    }
}