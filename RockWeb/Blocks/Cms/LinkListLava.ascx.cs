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
using System.ComponentModel;
using System.Data.Entity;
using System.Linq;
using System.Web.UI;
using System.Web.UI.WebControls;

using Rock;
using Rock.Attribute;
using Rock.Data;
using Rock.Model;
using Rock.Security;
using Rock.Web.Cache;
using Rock.Web.UI.Controls;

namespace RockWeb.Blocks.Cms
{
    /// <summary>
    /// Displays a list of links.
    /// </summary>
    [DisplayName( "Link List Lava" )]
    [Category( "CMS" )]
    [Description( "Displays a list of links." )]

    #region Block Attributes

    [DefinedTypeField(
        "Defined Type",
        Description = "The defined type to use when saving link information.",
        IsRequired = true,
        DefaultValue = Rock.SystemGuid.DefinedType.LINKLIST_DEFAULT_LIST,
        Order = 0,
        Key = AttributeKey.DefinedType )]

    [CodeEditorField(
        "Lava Template",
        Description = "Lava template to use to display content",
        EditorMode = CodeEditorMode.Lava,
        EditorTheme = CodeEditorTheme.Rock,
        EditorHeight = 400,
        IsRequired = true,
        DefaultValue =  DefaultLavaTemplate,
        Order = 1,
        Key = AttributeKey.LavaTemplate )]

    [CodeEditorField(
        "Edit Header",
        Description = "The HTML to display above list when editing values.",
        EditorMode = CodeEditorMode.Html,
        EditorTheme = CodeEditorTheme.Rock,
        EditorHeight = 100,
        IsRequired = true,
        DefaultValue = DefaultEditHeader,
        Order =  3,
        Key = AttributeKey.EditHeader)]

    [CodeEditorField(
        "Edit Footer",
        Description = "The HTML to display above list when editing values.",
        EditorMode = CodeEditorMode.Html,
        EditorTheme = CodeEditorTheme.Rock,
        EditorHeight = 100,
        IsRequired = true,
        DefaultValue = DefaultEditFooter,
        Key = AttributeKey.EditFooter,
        Order = 4 )]

    #endregion Block Attributes
    [Rock.SystemGuid.BlockTypeGuid( "BBA9210E-80E1-486A-822D-F8842FE09F99" )]
    public partial class LinkListLava : Rock.Web.UI.RockBlock
    {
        #region Attribute Keys

        private static class AttributeKey
        {
            public const string DefinedType = "DefinedType";
            public const string LavaTemplate = "LavaTemplate";
            public const string EditHeader = "EditHeader";
            public const string EditFooter = "EditFooter";
        }

        #endregion Attribute Keys

        #region constants

        private const string DefaultLavaTemplate = @"
<div class=""panel panel-block""> 
    <div class=""panel-heading"">
        <h4 class=""panel-title"">Links</h4>
        {% if AllowedActions.Edit == true %}
            <span class=""pull-right""><a href=""#"" onclick=""{{ '' | Postback:'EditList' }}""><i class='fa fa-gear'></i></a></span>
        {% endif %}
    </div>
    <div class=""block-content"">
        <ul class='list-group list-group-panel'>
        {% for definedValue in DefinedValues %}
            {% assign IsLink = definedValue | Attribute:'IsLink','RawValue' %}
            {% if IsLink == 'True' %}
                <li class='list-group-item'><a href='{{ definedValue.Description }}'>{{ definedValue.Value }}</a></li>
            {% else %}
                <li class='list-group-item'><h4 class='list-group-item-heading'>{{ definedValue.Value }}</h4></li>
            {% endif %}
        {% endfor %}
        </ul>
    </div>
</div>
";

        private const string DefaultEditHeader = @"
<div class='panel panel-block'>
    <div class='panel-heading'>
        <h4 class='panel-title'>Links</div>
    <div>
    <div class='panel-body'>
";

        private const string DefaultEditFooter = @"
    </div>
</div>
";
        #endregion Constants

        #region Fields

        bool _canEdit = false;
        DefinedTypeCache _definedType = null;

        #endregion

        #region Base Control Methods

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );

            // this event gets fired after block settings are updated. it's nice to repaint the screen if these settings would alter it
            this.BlockUpdated += Block_BlockUpdated;
            this.AddConfigurationUpdateTrigger( upnlContent );

            _canEdit = UserCanEdit;
            gLinks.DataKeyNames = new string[] { "Id" };
            gLinks.Actions.AddClick += gLinks_Add;
            gLinks.GridRebind += gLinks_GridRebind;
            gLinks.GridReorder += gLinks_GridReorder;
            gLinks.RowDataBound += gLinks_RowDataBound;
            gLinks.Actions.ShowAdd = _canEdit;
            gLinks.IsDeleteEnabled = _canEdit;
            gLinks.Actions.AddButton.CssClass = "btn-add btn btn-default btn-xs";

            foreach ( var securityField in gLinks.Columns.OfType<SecurityField>() )
            {
                securityField.EntityTypeId = EntityTypeCache.Get( typeof( DefinedValue ) ).Id;
            }

            _definedType = DefinedTypeCache.Get( GetAttributeValue( AttributeKey.DefinedType ).AsGuid() );
        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Load" /> event.
        /// </summary>
        /// <param name="e">The <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnLoad( EventArgs e )
        {
            RouteAction();

            if ( !Page.IsPostBack )
            {
                ShowList();
            }
            else
            {
                ShowDialog();
            }

            base.OnLoad( e );
        }

        #endregion

        #region Events

        /// <summary>
        /// Handles the BlockUpdated event of the control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void Block_BlockUpdated( object sender, EventArgs e )
        {
            ShowList();
        }

        /// <summary>
        /// Handles the RowSelected event of the gLinks control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RowEventArgs"/> instance containing the event data.</param>
        protected void gLinks_RowSelected( object sender, RowEventArgs e )
        {
            dlgLink.Title = "Edit Link";

            using ( var rockContext = new RockContext() )
            {
                var dv = new DefinedValueService( rockContext ).Get( e.RowKeyId );
                if ( dv != null )
                {
                    dv.LoadAttributes();
                    bool isLink = dv.GetAttributeValue( "IsLink" ).AsBoolean( true );

                    hfDefinedValueId.Value = dv.Id.ToString(); ;
                    tbTitle.Text = dv.Value;
                    rblLinkType.SelectedValue = isLink ? "Link" : "Heading";
                    tbLink.Text = dv.Description;
                    tbLink.Visible = isLink;

                    ShowDialog( "Link", true );
                }
            }
        }

        /// <summary>
        /// Handles the Add event of the gLinks control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void gLinks_Add( object sender, EventArgs e )
        {
            dlgLink.Title = "New Link";

            tbTitle.Text = string.Empty;
            rblLinkType.SelectedValue = "Link";
            tbLink.Text = string.Empty;
            tbLink.Visible = true;

            hfDefinedValueId.Value = string.Empty;
            ShowDialog( "Link", true );
        }

        /// <summary>
        /// Handles the Delete event of the gLinks control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RowEventArgs"/> instance containing the event data.</param>
        protected void gLinks_Delete( object sender, RowEventArgs e )
        {
            using ( var rockContext = new RockContext() )
            {
                var service = new DefinedValueService( rockContext );
                var definedValue = service.Get( e.RowKeyId );

                if ( definedValue != null )
                {
                    string errorMessage;
                    if ( !service.CanDelete( definedValue, out errorMessage ) )
                    {
                        mdGridWarningValues.Show( errorMessage, ModalAlertType.Information );
                        return;
                    }

                    service.Delete( definedValue );
                    rockContext.SaveChanges();
                }
            }

            BindGrid();
        }

        /// <summary>
        /// Handles the GridReorder event of the gLinks control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="GridReorderEventArgs"/> instance containing the event data.</param>
        void gLinks_GridReorder( object sender, GridReorderEventArgs e )
        {
            using ( var rockContext = new RockContext() )
            {
                var service = new DefinedValueService( rockContext );
                var definedValues = service.Queryable().Where( a => a.DefinedTypeId == _definedType.Id ).OrderBy( a => a.Order ).ThenBy( a => a.Value );
                var changedIds = service.Reorder( definedValues.ToList(), e.OldIndex, e.NewIndex );
                rockContext.SaveChanges();
            }

            BindGrid();
        }

        /// <summary>
        /// Handles the RowDataBound event of the gLinks control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="GridViewRowEventArgs"/> instance containing the event data.</param>
        void gLinks_RowDataBound( object sender, GridViewRowEventArgs e )
        {
            foreach ( TableCell cell in e.Row.Cells )
            {
                cell.Style.Add( "padding", "5px" );
            }

            if ( e.Row.RowType == DataControlRowType.DataRow )
            {
                var dv = e.Row.DataItem as DefinedValue;
                var lValue = e.Row.FindControl( "lValue" ) as Literal;
                if ( dv != null && lValue != null )
                {
                    if ( dv.GetAttributeValue( "IsLink" ).AsBoolean() )
                    {
                        lValue.Text = dv.Value;
                    }
                    else
                    {
                        lValue.Text = string.Format( "<strong>{0}</strong>", dv.Value );
                    }
                }
            }
        }

        /// <summary>
        /// Handles the GridRebind event of the gLinks control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        void gLinks_GridRebind( object sender, EventArgs e )
        {
            BindGrid();
        }

        /// <summary>
        /// Handles the SelectedIndexChanged event of the rblLinkType control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void rblLinkType_SelectedIndexChanged( object sender, EventArgs e )
        {
            tbLink.Visible = rblLinkType.SelectedValue == "Link";
        }

        /// <summary>
        /// Handles the SaveClick event of the dlgLink control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void dlgLink_SaveClick( object sender, EventArgs e )
        {
            DefinedValue definedValue = null;
            using ( var rockContext = new RockContext() )
            {
                var service = new DefinedValueService( rockContext );
                int? definedValueId = hfDefinedValueId.Value.AsIntegerOrNull();
                if ( definedValueId.HasValue )
                {
                    definedValue = service.Get( definedValueId.Value );
                }

                if ( definedValue == null )
                {
                    definedValue = new DefinedValue { Id = 0 };
                    definedValue.DefinedTypeId = _definedType.Id;
                    definedValue.IsSystem = false;

                    var orders = service.Queryable()
                        .Where( d => d.DefinedTypeId == _definedType.Id )
                        .Select( d => d.Order )
                        .ToList();

                    definedValue.Order = orders.Any() ? orders.Max() + 1 : 0;
                }

                definedValue.Value = tbTitle.Text;
                definedValue.Description = tbLink.Text;
                definedValue.LoadAttributes();
                definedValue.SetAttributeValue( "IsLink", ( rblLinkType.SelectedValue == "Link" ).ToString() );

                rockContext.WrapTransaction( () =>
                {
                    if ( definedValue.Id.Equals( 0 ) )
                    {
                        service.Add( definedValue );
                    }

                    rockContext.SaveChanges();

                    definedValue.SaveAttributeValues( rockContext );

                } );
            }

            HideDialog();

            BindGrid();
        }

        /// <summary>
        /// Handles the Click event of the btnDone control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnDone_Click( object sender, EventArgs e )
        {
            ShowList();

            pnlEdit.Visible = false;
            pnlView.Visible = true;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Route the request to the correct panel
        /// </summary>
        private void RouteAction()
        {
            if ( Request.Form["__EVENTARGUMENT"] != null )
            {
                string[] eventArgs = Request.Form["__EVENTARGUMENT"].Split( '^' );

                if ( eventArgs.Length == 2 )
                {
                    string action = eventArgs[0];
                    string parameters = eventArgs[1];

                    int argument = 0;
                    int.TryParse( parameters, out argument );

                    switch ( action )
                    {
                        case "EditList":
                            DisplayEditList();
                            break;
                    }
                }
            }
            else
            {
                pnlView.Visible = true;
                pnlEdit.Visible = false;
            }
        }

        /// <summary>
        /// Shows the list.
        /// </summary>
        protected void ShowList()
        {
            var mergeFields = new Dictionary<string, object>();
            mergeFields.Add( "CurrentPerson", CurrentPerson );
            
            if ( _definedType != null  )
            {
                var definedValues = new List<DefinedValueCache>();
                foreach ( var definedValue in _definedType.DefinedValues )
                {
                    if ( _canEdit || definedValue.IsAuthorized( Authorization.VIEW, CurrentPerson ) )
                    {
                        definedValues.Add( definedValue );
                    }
                }
                mergeFields.Add( "DefinedValues", definedValues );
            }

            // add collection of allowed security actions
            Dictionary<string, object> securityActions = new Dictionary<string, object>();
            securityActions.Add( "Edit", UserCanEdit );
            securityActions.Add( "Administrate", UserCanAdministrate );
            mergeFields.Add( "AllowedActions", securityActions ); 
            
            string template = GetAttributeValue( AttributeKey.LavaTemplate );

            lContent.Text = template.ResolveMergeFields( mergeFields ).ResolveClientIds( upnlContent.ClientID );
        }

        /// <summary>
        /// Displays the edit list.
        /// </summary>
        private void DisplayEditList()
        {
            lEditHeader.Text = GetAttributeValue( AttributeKey.EditHeader );
            lEditFooter.Text = GetAttributeValue( AttributeKey.EditFooter );

            if ( _definedType != null )
            {
                using ( var rockContext = new RockContext() )
                {
                    var entityType = EntityTypeCache.Get( "Rock.Model.DefinedValue");
                    var definedType = new DefinedTypeService( rockContext ).Get( _definedType.Id );
                    if ( definedType != null && entityType != null )
                    {
                        var attributeService = new AttributeService( rockContext );
                        var attributes = new AttributeService( rockContext )
                            .GetByEntityTypeQualifier( entityType.Id, "DefinedTypeId", definedType.Id.ToString(), false )
                            .ToList();

                        // Verify (and create if necessary) the "Is Link" attribute
                        if ( !attributes.Any( a => a.Key == "IsLink" ) )
                        {
                            var fieldType = FieldTypeCache.Get( Rock.SystemGuid.FieldType.BOOLEAN );
                            if ( entityType != null && fieldType != null )
                            {
                                var attribute = new Rock.Model.Attribute();
                                attributeService.Add( attribute );
                                attribute.EntityTypeId = entityType.Id;
                                attribute.EntityTypeQualifierColumn = "DefinedTypeId";
                                attribute.EntityTypeQualifierValue = definedType.Id.ToString();
                                attribute.FieldTypeId = fieldType.Id;
                                attribute.Name = "Is Link";
                                attribute.Key = "IsLink";
                                attribute.Description = "Flag indicating if value is a link (vs Header)";
                                attribute.IsGridColumn = true;
                                attribute.DefaultValue = true.ToString();

                                var qualifier1 = new AttributeQualifier();
                                qualifier1.Key = "truetext";
                                qualifier1.Value = "Yes";
                                attribute.AttributeQualifiers.Add( qualifier1 );

                                var qualifier2 = new AttributeQualifier();
                                qualifier2.Key = "falsetext";
                                qualifier2.Value = "No";
                                attribute.AttributeQualifiers.Add( qualifier2 );

                                rockContext.SaveChanges();
                            }
                        }

                    }
                }

                BindGrid();

                pnlView.Visible = false;
                pnlEdit.Visible = true;
            }
        }

        /// <summary>
        /// Binds the grid.
        /// </summary>
        private void BindGrid()
        {
            foreach( var column in gLinks.ColumnsOfType<RockTemplateField>() )
            {
                column.HeaderStyle.CssClass = string.Empty;
                column.ItemStyle.CssClass = string.Empty;
            }

            if ( _definedType != null )
            {
                using ( var rockContext = new RockContext() )
                {
                    var definedValues = new DefinedValueService( rockContext )
                        .Queryable().AsNoTracking()
                        .Where( v => v.DefinedTypeId == _definedType.Id )
                        .OrderBy( v => v.Order )
                        .ToList();

                    foreach( var definedValue in definedValues )
                    {
                        definedValue.LoadAttributes();
                    }

                    gLinks.DataSource = definedValues;
                    gLinks.DataBind();
                }
            }
        }

        /// <summary>
        /// Shows the dialog.
        /// </summary>
        /// <param name="dialog">The dialog.</param>
        /// <param name="setValues">if set to <c>true</c> [set values].</param>
        private void ShowDialog( string dialog, bool setValues = false )
        {
            hfActiveDialog.Value = dialog.ToUpper().Trim();
            ShowDialog( setValues );
        }

        /// <summary>
        /// Shows the dialog.
        /// </summary>
        /// <param name="setValues">if set to <c>true</c> [set values].</param>
        private void ShowDialog( bool setValues = false )
        {
            switch ( hfActiveDialog.Value )
            {
                case "LINK":
                    dlgLink.Show();
                    break;
            }
        }

        /// <summary>
        /// Hides the dialog.
        /// </summary>
        private void HideDialog()
        {
            switch ( hfActiveDialog.Value )
            {
                case "LINK":
                    dlgLink.Hide();
                    break;
            }

            hfActiveDialog.Value = string.Empty;
        }

        #endregion

}
}