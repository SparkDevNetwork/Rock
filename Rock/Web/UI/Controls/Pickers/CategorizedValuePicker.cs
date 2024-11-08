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
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using Rock.Model;

namespace Rock.Web.UI.Controls
{
    /// <summary>
    /// A picker control that allows a top-down, stepped selection of nodes from a tree structure.
    /// Values are selected for each level of the tree one at a time from the root, and each subsequent
    /// selection shows only the children of the previously selected node or nodes that do not have a parent.
    /// </summary>
    public class CategorizedValuePicker : CompositeControl, IRockControl, IRockChangeHandlerControl
    {
        private Repeater _categorySelectorRepeater = null;

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="ValueList"/> class.
        /// </summary>
        public CategorizedValuePicker() : base()
        {
            this.HelpBlock = new HelpBlock();
            this.WarningBlock = new WarningBlock();
            this.RequiredFieldValidator = new HiddenFieldValidator();
        }

        #endregion

        #region IRockControl implementation

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
        /// Gets or sets the warning text.
        /// </summary>
        /// <value>
        /// The warning text.
        /// </value>
        [
        Bindable( true ),
        Category( "Appearance" ),
        DefaultValue( "" ),
        Description( "The warning block." )
        ]
        public string Warning
        {
            get
            {
                return WarningBlock != null ? WarningBlock.Text : string.Empty;
            }
            set
            {
                if ( WarningBlock != null )
                {
                    WarningBlock.Text = value;
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
            get { return ViewState["Required"] as bool? ?? false; }
            set { ViewState["Required"] = value; }
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
        /// Gets or sets the warning block.
        /// </summary>
        /// <value>
        /// The warning block.
        /// </value>
        public WarningBlock WarningBlock { get; set; }

        /// <summary>
        /// Gets or sets the required field validator.
        /// </summary>
        /// <value>
        /// The required field validator.
        /// </value>
        public RequiredFieldValidator RequiredFieldValidator { get; set; }

        /// <summary>
        /// Gets or sets an optional validation group to use.
        /// </summary>
        /// <value>
        /// The validation group.
        /// </value>
        public string ValidationGroup
        {
            get
            {
                return this.RequiredFieldValidator.ValidationGroup;
            }
            set
            {
                this.RequiredFieldValidator.ValidationGroup = value;
            }
        }

        #endregion

        #region IRockChangeHandlerControl

        /// <inheritdoc/>
        public event EventHandler ValueChanged;

        #endregion

        #region Controls

        /// <summary>
        /// Stores the current value of the control.
        /// </summary>
        private HiddenField _hfValue;

        /// <summary>
        /// Stores the current selection of the control.
        /// </summary>
        private HiddenField _hfSelectedKey;

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets the value prompt.
        /// </summary>
        /// <value>
        /// The value prompt.
        /// </value>
        public string ValuePrompt
        {
            get { return ViewState["ValuePrompt"] as string ?? "Value"; }
            set { ViewState["ValuePrompt"] = value; }
        }

        /// <summary>
        /// Gets or sets the hierarchy of values for the control.
        /// </summary>
        /// <value>
        /// The custom values.
        /// </value>
        public TreeNode<CategorizedValuePickerItem> ValueTree
        {
            get { return ViewState["ValueTree"] as TreeNode<CategorizedValuePickerItem>; }
            set { ViewState["ValueTree"] = value; }
        }

        /// <summary>
        /// Gets or sets the selected node key.
        /// </summary>
        public string SelectedNodeKey
        {
            get { return ViewState["SelectedNode"] as string ?? ""; }
            set { ViewState["SelectedNode"] = value; }
        }

        /// <summary>
        /// Gets or sets the selected value.
        /// </summary>
        /// <value>
        /// The selected value.
        /// </value>
        public string SelectedValue
        {
            get
            {
                EnsureChildControls();
                return _hfValue.Value;
            }
            set
            {
                EnsureChildControls();

                // Find the first node in the selection tree with a matching value, and set it as the selected node.
                var selectedNodeKey = ValueTree?.Find( x => x.Value.Value == value ).Select( x => x.Value.Key ).FirstOrDefault();

                SetSelection( selectedNodeKey, out bool isChanged );
                CreateSelectionControls( selectedNodeKey );

                if ( isChanged )
                {
                    ValueChanged?.Invoke( this, new EventArgs() );
                }
            }
        }

        /// <summary>
        /// Gets or sets the selected item by key.
        /// </summary>
        /// <value>
        /// The item key.
        /// </value>
        public string SelectedKey
        {
            get
            {
                EnsureChildControls();
                return _hfSelectedKey.Value;
            }
            set
            {
                EnsureChildControls();

                SetSelection( value, out bool isChanged );
                CreateSelectionControls( value );

                if ( isChanged )
                {
                    ValueChanged?.Invoke( this, new EventArgs() );
                }
            }
        }
        /// <summary>
        /// Set the currently selected item for the control.
        /// </summary>
        /// <param name="itemKey"></param>
        /// <param name="isValueChanged"></param>
        private void SetSelection( string itemKey, out bool isValueChanged )
        {
            var selectedNode = ValueTree?.Find( x => x.Value.Key == itemKey ).FirstOrDefault();
            var isValue = selectedNode?.Value.IsValue() ?? false;
            var isChanged = _hfSelectedKey.Value != itemKey;

            if ( isChanged )
            {
                // Set the selected item and value properties.
                // The Value property is only set if the selected item is a Value type.
                _hfSelectedKey.Value = itemKey;
                _hfValue.Value = isValue ? selectedNode?.Value?.Value : string.Empty;

                isValueChanged = isValue;
            }
            else
            {
                isValueChanged = false;
            }
        }

        #endregion

        /// <summary>
        /// Called by the ASP.NET page framework to notify server controls that use composition-based implementation to create any child controls they contain in preparation for posting back or rendering.
        /// </summary>
        protected override void CreateChildControls()
        {
            base.CreateChildControls();
            Controls.Clear();
            RockControlHelper.CreateChildControls( this, Controls );

            _hfValue = new HiddenField();
            _hfValue.ID = this.ID + "_hfValue";
            Controls.Add( _hfValue );
            _hfSelectedKey = new HiddenField();
            _hfSelectedKey.ID = this.ID + "_hfSelection";
            Controls.Add( _hfSelectedKey );

            // Link the RequiredFieldValidator to the hidden field that stores the selected value.
            this.RequiredFieldValidator.ControlToValidate = _hfValue.ID;

            var template = new CategorizedValueSelectorTemplate();

            _categorySelectorRepeater = new Repeater { ID = "valueSelectorRepeater" };
            _categorySelectorRepeater.ItemTemplate = template;
            _categorySelectorRepeater.ItemDataBound += categorySelectorRepeater_ItemDataBound;

            Controls.Add( _categorySelectorRepeater );
        }

        /// <inheritdoc/>
        protected override void OnLoad( EventArgs e )
        {
            base.OnLoad( e );

            if ( !this.Page.IsPostBack )
            {
                CreateSelectionControls( this.SelectedKey );
            }
            else
            {
                EnsureChildControls();
                var postBackControlId = this.Page.Request.Params["__EVENTTARGET"];
                var postBackControl = FindControlRecursive( this, postBackControlId );

                // If Postback is from a control on this Picker, recreate dynamic controls to preserve state of controls,
                // This is a safe guard for when the Picker is used in conjunction with another control like the AttributeMatrixEditor.
                if ( postBackControl != null )
                {
                    // When used in another control like the AttributeMatrixEditor, the Picker could be re-created several times in between PostBacks,
                    // in that case the NoteKey saved in this control's view state is used to recreate the dynamic controls since it will have the
                    // latest, non-persisted value.
                    EnsureChildControls();
                    CreateSelectionControls( this.SelectedNodeKey );
                }
            }
        }

        private Control FindControlRecursive( Control root, string id )
        {
            if ( root.UniqueID == id )
            {
                return root;
            }

            foreach ( Control child in root.Controls )
            {
                Control found = FindControlRecursive( child, id );
                if ( found != null )
                {
                    return found;
                }
            }

            return null;
        }

        /// <summary>
        /// Outputs server control content to a provided <see cref="T:System.Web.UI.HtmlTextWriter" /> object and stores tracing information about the control if tracing is enabled.
        /// </summary>
        /// <param name="writer">The <see cref="T:System.Web.UI.HtmlTextWriter" /> object that receives the control content.</param>
        public override void RenderControl( HtmlTextWriter writer )
        {
            if ( this.Visible )
            {
                // Set the required field message here because the control label may have been modified during initialization.
                this.RequiredFieldValidator.ErrorMessage = string.Format( "{0} must have a value.", this.Label );

                RockControlHelper.RenderControl( this, writer );
            }
        }

        /// <summary>
        /// Handles the ItemDataBound event of the valueSelectorRepeater control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RepeaterItemEventArgs"/> instance containing the event data.</param>
        private void categorySelectorRepeater_ItemDataBound( object sender, RepeaterItemEventArgs e )
        {
            var repeaterItem = e.Item;
            var controlInfo = e.Item.DataItem as SelectionControlInfo;
            var hfNodeKey = repeaterItem.FindControl( "hfNodeKey" ) as HiddenField;
            var lLabel = repeaterItem.FindControl( "lLabel" ) as Label;
            var ddlSelector = repeaterItem.FindControl( "ddlSelector" ) as RockDropDownList;

            hfNodeKey.Value = controlInfo.NodeKey;

            lLabel.Text = controlInfo.Label;
            if ( this.Required )
            {
                lLabel.AddCssClass( "required-indicator" );
            }

            ddlSelector.Items.Clear();
            if ( controlInfo.Items != null )
            {
                foreach ( var item in controlInfo.Items )
                {
                    ddlSelector.Items.Add( new ListItem( item.Text, item.Key ) );
                }
            }
            ddlSelector.Required = this.Required;
            ddlSelector.SelectedValue = controlInfo.SelectedItemKey ?? CategorizedValuePickerItem.EmptyValue;
        }

        private void CreateSelectionControls( string selectedNodeKey )
        {
            var selectionControls = new List<SelectionControlInfo>();

            if ( ValueTree != null )
            {
                var selectedNode = ValueTree.Find( x => x.Value.Key == selectedNodeKey )
                    .OrderBy( x => x.GetAncestors().Count )
                    .FirstOrDefault();

                // Get the currently selected node.
                // If there is more than one match for the selected value, get the node at the highest level.
                var selectorNode = GetFirstDecisionNode( selectedNodeKey );

                // Create a selection control for each decision node, working back from the selected node to the root node.
                var selectorNodeKey = selectedNodeKey;
                while ( selectorNode != null )
                {
                    // Add a selector for this node if it represents a category.
                    if ( selectorNode.Value.IsCategory() )
                    {
                        var info = GetSelectorInfo( selectorNode, selectorNodeKey );

                        // If the node only contains a single category, skip the selector and drill-down to the next level.
                        var selectableNodes = info.Items.Where( x => x.IsCategory() || x.IsValue() ).ToList();
                        var containsSingleCategory = selectableNodes.Count == 1
                            && selectableNodes[0].IsCategory();

                        if ( !containsSingleCategory )
                        {
                            selectionControls.Add( info );
                        }
                    }

                    // Set the value of the parent selector control to select the current node.
                    selectorNodeKey = selectorNode.Value.Key;
                    // Move to the parent node
                    selectorNode = selectorNode.Parent;
                }

                // Reorder the controls with the top-level selection first.
                selectionControls.Reverse();

                // Add a value selector for the next unselected level in the hierarchy.
                if ( selectedNode != null
                     && selectedNode.Value.IsCategory() )
                {
                    var finalSelectionNode = selectedNode.Children.FirstOrDefault( x => x.Value.Key == selectedNodeKey );
                    if ( finalSelectionNode != null )
                    {
                        var info = GetSelectorInfo( finalSelectionNode, null );
                        selectionControls.Add( info );
                    }
                }
            }

            // If no ValueTree is defined, return a single empty selector.
            if ( !selectionControls.Any() )
            {
                selectionControls.Add( new SelectionControlInfo { NodeKey = CategorizedValuePickerItem.EmptyValue, Label = string.Empty, Items = null } );
            }

            // Render the selection controls top-down.
            _categorySelectorRepeater.DataSource = selectionControls;
            _categorySelectorRepeater.DataBind();

            // Set the selected value.
            SetSelection( selectedNodeKey, out bool _ );
        }

        private TreeNode<CategorizedValuePickerItem> GetFirstDecisionNode( string startNodeKey )
        {
            var selectedNode = ValueTree.Find( x => x.Value.Key == startNodeKey )
                .OrderBy( x => x.GetAncestors().Count )
                .FirstOrDefault();

            // If no selected node, find the first child node that requires a decision.
            var candidateNode = selectedNode ?? this.ValueTree;
            TreeNode<CategorizedValuePickerItem> decisionNode = null;

            while ( candidateNode != null && decisionNode == null )
            {
                // If the candidate node is a Value, select it.
                if ( candidateNode.Value.IsValue() )
                {
                    decisionNode = candidateNode;
                    continue;
                }

                // Get the selectable child nodes of the candidate node.
                var selectableNodes = candidateNode.Children
                    .Where( x => x.Value.IsValue() || x.Value.IsCategory() )
                    .ToList();
                if ( selectableNodes.Count > 1 )
                {
                    // If a default selection is available, select it and continue.
                    var defaultNode = selectableNodes.FirstOrDefault( x => x.Value.IsDefaultSelection );
                    if ( defaultNode != null )
                    {
                        candidateNode = defaultNode;
                        continue;
                    }
                }

                if ( selectableNodes.Count > 1 )
                {
                    // If this node has multiple child nodes, make it the final selector.
                    decisionNode = candidateNode;
                }
                else if ( selectableNodes.Count == 1 )
                {
                    var childValue = candidateNode.Children[0].Value;
                    if ( childValue.IsValue() )
                    {
                        // If this node has a single child value, make it the final selector.
                        // This is a decision node because an empty selection is also possible.
                        decisionNode = candidateNode;
                    }
                    else
                    {
                        // If this node has a single child category, drill down further.
                        candidateNode = candidateNode[0];
                    }
                }
                else
                {
                    // If this node has no child nodes, no selection is possible.
                    candidateNode = null;
                }
            }

            decisionNode = decisionNode ?? this.ValueTree;

            return decisionNode;
        }

        private SelectionControlInfo GetSelectorInfo( TreeNode<CategorizedValuePickerItem> selectorNode, string selectedNodeKey )
        {
            var availableNodes = GetSelectableNodesFromParentNode( selectorNode );

            var emptyItem = new CategorizedValuePickerItem { Key = CategorizedValuePickerItem.EmptyValue };
            availableNodes.Insert( 0, emptyItem );

            var info = new SelectionControlInfo
            {
                NodeKey = selectorNode.Value.Key,
                Label = selectorNode.Value.CategoryLabel,
                Items = availableNodes
            };

            if ( availableNodes.Any( x => x.Key == selectedNodeKey ) )
            {
                info.SelectedItemKey = selectedNodeKey;
            }
            else
            {
                var defaultNode = availableNodes.FirstOrDefault( x => x.IsDefaultSelection );

                info.SelectedItemKey = defaultNode?.Key;
            }

            return info;
        }

        /// <summary>
        /// Renders the base control.
        /// </summary>
        /// <param name="writer">The writer.</param>
        public virtual void RenderBaseControl( HtmlTextWriter writer )
        {
            writer.AddAttribute( HtmlTextWriterAttribute.Class, "value-list" );
            writer.AddAttribute( HtmlTextWriterAttribute.Id, this.ClientID );
            writer.RenderBeginTag( HtmlTextWriterTag.Div );
            writer.WriteLine();

            _hfValue.RenderControl( writer );
            _hfSelectedKey.RenderControl( writer );

            writer.WriteLine();

            // Render the selection controls top-down.
            _categorySelectorRepeater.RenderControl( writer );

            writer.RenderEndTag();
            writer.WriteLine();
        }

        /// <summary>
        /// Get the list of nodes available for selection for the specified parent node.
        /// </summary>
        /// <param name="parentNode"></param>
        /// <returns></returns>
        private List<CategorizedValuePickerItem> GetSelectableNodesFromParentNode( TreeNode<CategorizedValuePickerItem> parentNode )
        {
            var selectableNodes = new List<CategorizedValuePickerItem>();

            if ( parentNode == null )
            {
                return selectableNodes;
            }

            // If the parent node is a Value, there are no selections available.
            if ( parentNode.Value.IsValue() )
            {
                return selectableNodes;
            }

            selectableNodes = parentNode.Children.Select( x => x.Value ).ToList();
            return selectableNodes;
        }

        #region Picker Template

        private class CategorizedValueSelectorTemplate : ITemplate
        {
            /// <summary>
            /// When implemented by a class, defines the <see cref="T:System.Web.UI.Control" /> object that child controls and templates belong to. These child controls are in turn defined within an inline template.
            /// </summary>
            /// <param name="container">The <see cref="T:System.Web.UI.Control" /> object to contain the instances of controls from the inline template.</param>
            public void InstantiateIn( Control container )
            {
                var hf = new HiddenField { ID = "hfNodeKey" };
                var label = new Label { ID = "lLabel", CssClass = "control-label control-label-sm" };
                var ddlSelector = new RockDropDownList
                {
                    DataTextField = "Value",
                    DataValueField = "Key",
                    ID = "ddlSelector",
                    AutoPostBack = true,
                };

                ddlSelector.AddCssClass( "form-control" );
                ddlSelector.SelectedIndexChanged += ddlSelector_SelectedIndexChanged;

                HtmlGenericControl divFormGroup = new HtmlGenericControl( "div" );
                divFormGroup.AddCssClass( "form-group" );
                container.Controls.Add( divFormGroup );

                divFormGroup.Controls.Add( hf );
                divFormGroup.Controls.Add( label );
                divFormGroup.Controls.Add( ddlSelector );
            }

            private void ddlSelector_SelectedIndexChanged( object sender, EventArgs e )
            {
                var ddlSelector = sender as DropDownList;
                var container = ddlSelector?.FirstParentControlOfType<RepeaterItem>();
                var picker = container?.FirstParentControlOfType<CategorizedValuePicker>();
                var hfNodeKey = container?.FindControl( "hfNodeKey" ) as HiddenField;

                if ( picker == null || hfNodeKey == null )
                {
                    return;
                }

                // Get the currently selected node key, which is stored in the Value property of the list item.
                var newKey = ddlSelector.SelectedValue;
                if ( newKey == CategorizedValuePickerItem.EmptyValue )
                {
                    var selectorNode = picker.ValueTree.Find( x => x.Value.Key == hfNodeKey.Value ).FirstOrDefault();
                    if ( selectorNode != null )
                    {
                        newKey = hfNodeKey.Value;
                    }
                }

                picker.SelectedNodeKey = newKey;
                picker.CreateSelectionControls( newKey );
            }
        }

        #endregion

        #region Support classes

        /// <summary>
        /// Represents a selection control in the user interface.
        /// </summary>
        private class SelectionControlInfo
        {
            public string NodeKey;
            public string Label;
            public List<CategorizedValuePickerItem> Items;
            public string SelectedItemKey;

            public override string ToString()
            {
                return $"[{NodeKey}] {Label} ({Items.Count},Selected={SelectedItemKey})";
            }
        }

        #endregion
    }

    #region Support classes

    /// <summary>
    /// An entry in a categorized value picker.
    /// </summary>
    public class CategorizedValuePickerItem
    {
        /// <summary>
        /// Represents the value of an empty selection.
        /// </summary>
        public const string EmptyValue = "";

        /// <summary>
        /// The unique identifier for this item.
        /// </summary>
        public string Key { get; set; }

        /// <summary>
        /// The value associated with this item.
        /// </summary>
        public string Value { get; set; }

        /// <summary>
        /// The text to display for this item.
        /// </summary>
        public string Text { get; set; } = null;

        /// <summary>
        /// The label for the category this item represents, or empty if the item represents a value.
        /// </summary>
        public string CategoryLabel { get; set; } = null;

        /// <summary>
        /// Is this item selected by default?
        /// </summary>
        public bool IsDefaultSelection { get; set; }

        /// <summary>
        /// Does this item represent a category?
        /// </summary>
        /// <returns></returns>
        public bool IsCategory()
        {
            return !string.IsNullOrEmpty( CategoryLabel );
        }

        /// <summary>
        /// Does this item represent a value?
        /// </summary>
        /// <returns></returns>
        public bool IsValue()
        {
            return string.IsNullOrEmpty( CategoryLabel ) && !IsEmpty();
        }

        /// <summary>
        /// Does this item represent an empty selection?
        /// </summary>
        /// <returns></returns>
        public bool IsEmpty()
        {
            return Key == CategorizedValuePickerItem.EmptyValue;
        }

        /// <inheritdoc/>
        public override string ToString()
        {
            return $"[{Key}] {Text} [{CategoryLabel}]";
        }
    }

    #endregion

    #region Tree Data Structure

    /// <summary>
    /// A tree data structure where each node maintains child and parent pointers, to allow for simple traversal of the tree.
    /// </summary>
    internal class TreeNode
    {
        /// <summary>
        /// Builds a tree data structure from a list of objects related by identifiable key and parent key properties.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="objects"></param>
        /// <param name="keySelector"></param>
        /// <param name="parentKeySelector"></param>
        /// <param name="rootNodeParentKey"></param>
        /// <returns></returns>
        public static List<TreeNode<T>> BuildTree<T>( IEnumerable<T> objects, Func<T, string> keySelector, Func<T, string> parentKeySelector, string rootNodeParentKey = null )
        {
            var rootNodes = new List<TreeNode<T>>();

            // Create a set of new tree nodes for the data objects, then map the nodes by key value.
            var allNodes = objects.Select( x => new TreeNode<T>( x ) ).ToList();
            var nodeKeyMap = allNodes.ToDictionary( x => keySelector( x.Value ) );

            // Add the node relationships.
            foreach ( var currentNode in allNodes )
            {
                var parentKey = parentKeySelector( currentNode.Value );
                if ( parentKey == rootNodeParentKey )
                {
                    rootNodes.Add( currentNode );
                }
                else if ( !nodeKeyMap.ContainsKey( parentKey ) )
                {
                    // If the parent key reference is invalid, add this as a root node.
                    rootNodes.Add( currentNode );
                }
                else
                {
                    nodeKeyMap[parentKey].AddChild( currentNode );
                }
            }

            return rootNodes;
        }

        /// <summary>
        /// Converts a tree data structure containing values of a specified Type using the specified conversion function.
        /// </summary>
        /// <typeparam name="TIn">The Type of the value contained by the source node.</typeparam>
        /// <typeparam name="TOut">The Type of the value contained by the destination node.</typeparam>
        /// <param name="node">The source node.</param>
        /// <param name="valueSelector">The function used to convert the source value to the destination value.</param>
        /// <returns></returns>
        public static TreeNode<TOut> Convert<TIn, TOut>( TreeNode<TIn> node, Func<TIn, TOut> valueSelector )
        {
            // Convert the supplied node and all children.
            var outNode = new TreeNode<TOut>( valueSelector( node.Value ) );

            foreach ( var childNode in node.Children )
            {
                outNode.AddChild( Convert( childNode, valueSelector ) );
            }

            return outNode;
        }
    }

    /// <summary>
    /// A simple tree data structure implementation.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class TreeNode<T>
    {
        private T _value;
        private List<TreeNode<T>> _children = new List<TreeNode<T>>();

        #region Constructors

        /// <summary>
        /// Create a new instance.
        /// </summary>
        /// <param name="value"></param>
        public TreeNode( T value )
        {
            _value = value;
        }

        #endregion

        /// <summary>
        /// Gets a child node by index.
        /// </summary>
        /// <param name="i"></param>
        /// <returns></returns>
        public TreeNode<T> this[int i]
        {
            get { return _children[i]; }
        }

        /// <summary>
        /// Gets the parent of this node.
        /// </summary>
        public TreeNode<T> Parent { get; private set; }

        /// <summary>
        /// The value object stored in this tree node.
        /// </summary>
        public T Value
        {
            get { return _value; }
            set { _value = value; }
        }

        /// <summary>
        /// Gets the collection of child nodes for this node.
        /// </summary>
        public ReadOnlyCollection<TreeNode<T>> Children
        {
            get { return _children.AsReadOnly(); }
        }

        /// <summary>
        /// Add a child node to this node.
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public TreeNode<T> AddChild( T value )
        {
            // Create a new node and add it to the collection of child nodes.
            var node = new TreeNode<T>( value ) { Parent = this };
            _children.Add( node );
            return node;
        }

        /// <summary>
        /// Add a child node to this node.
        /// </summary>
        /// <param name="node"></param>
        /// <returns></returns>
        public TreeNode<T> AddChild( TreeNode<T> node )
        {
            // Re-parent the node and add it to the collection of child nodes.
            node.Parent = this;
            _children.Add( node );
            return node;
        }

        /// <summary>
        /// Add a collection of child nodes to this node.
        /// </summary>
        /// <param name="nodes"></param>
        public void AddChildren( IEnumerable<TreeNode<T>> nodes )
        {
            foreach ( var node in nodes )
            {
                AddChild( node );
            }
        }

        /// <summary>
        /// Add a collection of child nodes to this node.
        /// </summary>
        /// <param name="items"></param>
        /// <returns></returns>
        public List<TreeNode<T>> AddChildren( IEnumerable<T> items )
        {
            var nodes = items.Select( x => new TreeNode<T>( x ) ).ToList();
            AddChildren( nodes );
            return nodes;
        }

        /// <summary>
        /// Inserts a child node of this node.
        /// </summary>
        /// <param name="index"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public TreeNode<T> InsertChild( int index, T value )
        {
            // Create a new node and add it to the collection of child nodes.
            var node = new TreeNode<T>( value ) { Parent = this };
            _children.Insert( index, node );
            return node;
        }

        /// <summary>
        /// Remove a child node from this node.
        /// </summary>
        /// <param name="node"></param>
        /// <returns></returns>
        public bool RemoveChild( TreeNode<T> node )
        {
            return _children.Remove( node );
        }

        /// <summary>
        /// Gets all descendant nodes that match the specified predicate, including this node.
        /// </summary>
        /// <param name="predicate"></param>
        /// <returns></returns>
        public List<TreeNode<T>> Find( Func<TreeNode<T>, bool> predicate )
        {
            var nodes = Flatten().Where( predicate ).ToList();
            return nodes;
        }

        /// <summary>
        /// Get a collection of descendant nodes, including this node.
        /// </summary>
        /// <returns></returns>
        public List<TreeNode<T>> Flatten()
        {
            var nodes = new List<TreeNode<T>>();

            nodes.Add( this );
            nodes.AddRange( _children.SelectMany( x => x.Flatten() ) );

            return nodes;
        }

        /// <summary>
        /// Gets the ancestor nodes of the current tree node.
        /// </summary>
        /// <returns>The ancestor nodes preceding the current node in the tree.</returns>
        public List<TreeNode<T>> GetAncestors()
        {
            var ancestors = new List<TreeNode<T>>();
            var parent = this.Parent;

            while ( parent != null )
            {
                ancestors.Add( parent );
                parent = parent.Parent;
            }
            ancestors.Reverse();

            return ancestors;
        }

        /// <inheritdoc/>
        public override string ToString()
        {
            return _value == null ? base.ToString() : _value.ToString();
        }
    }

    #endregion
}