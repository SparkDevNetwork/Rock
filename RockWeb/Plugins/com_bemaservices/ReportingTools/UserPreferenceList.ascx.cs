// <copyright>
// Copyright by BEMA Information Technologies
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
using System.ComponentModel;
using System.Collections.Generic;
using System.Linq;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Net;

using Rock;
using Rock.Attribute;
using Rock.Constants;
using Rock.Data;
using Rock.Model;
using Rock.Web.Cache;
using Rock.Web.UI;
using Rock.Web.UI.Controls;
using Rock.Security;
using System.Data.Entity;

namespace RockWeb.Plugins.com_bemaservices.ReportingTools
{
    [EntityTypeField( "Entity", "Entity Name", false, "Applies To", 0 )]
    [TextField( "Entity Qualifier Column", "The entity column to evaluate when determining if this attribute applies to the entity", false, "", "Applies To", 1 )]
    [TextField( "Entity Qualifier Value", "The entity column value to evaluate.  Attributes will only apply to entities with this value", false, "", "Applies To", 2 )]
    [BooleanField( "Allow Setting of Values", "Should UI be available for setting values of the specified Entity ID?", false, order: 3 )]
    [IntegerField( "Entity Id", "The entity id that values apply to", false, 0, order: 4 )]
    [BooleanField( "Enable Show In Grid", "Should the 'Show In Grid' option be displayed when editing attributes?", false, order: 5 )]
    [TextField( "Category Filter", "A comma separated list of category GUIDs to limit the display of attributes to.", false, "", order: 6 )]

    public partial class UserPreferenceList : RockBlock, ICustomGridColumns
    {
        #region Fields

        private int? _entityTypeId = null;

        #endregion

        #region Base Control Methods

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );

            // if limiting by category list hide the filters
            if ( !string.IsNullOrWhiteSpace( GetAttributeValue( "CategoryFilter" ) ) )
            {
                rFilter.Visible = false;
            }

            Guid? entityTypeGuid = "EC04E5FF-3608-465E-AC36-49625825D4E1".AsGuidOrNull();
            if ( entityTypeGuid.HasValue )
            {
                if ( default( Guid ) == entityTypeGuid )
                {
                    _entityTypeId = default( int );
                }
                else
                {
                    _entityTypeId = EntityTypeCache.Get( entityTypeGuid.Value ).Id;
                }
            }

            rGrid.DataKeyNames = new string[] { "Id" };
            rGrid.Actions.ShowAdd = true;
            rGrid.GridRebind += rGrid_GridRebind;
            rGrid.RowDataBound += rGrid_RowDataBound;
            rFilter.ApplyFilterClick += rFilter_ApplyFilterClick;

            mdAttributeValue.SaveClick += mdAttributeValue_SaveClick;

            BindFilter();
        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Load" /> event.
        /// </summary>
        /// <param name="e">The <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnLoad( EventArgs e )
        {
            if ( !Page.IsPostBack )
            {
                BindGrid();
            }
            else
            {
                int? attributeValueId = hfAttributeValueId.Value.AsIntegerOrNull();
                if ( attributeValueId.HasValue )
                {
                    ShowEditValue( attributeValueId.Value, false );
                }

                if ( hfActiveDialog.Value.ToUpper() == "ATTRIBUTEVALUE" )
                {
                    //
                }

                ShowDialog();
            }

            base.OnLoad( e );
        }

        #endregion

        #region Events

        /// <summary>
        /// Handles the ApplyFilterClick event of the rFilter control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void rFilter_ApplyFilterClick( object sender, EventArgs e )
        {
            int personId = ppPerson.PersonId ?? 0;
            rFilter.SaveUserPreference( "Person", personId.ToString() );
            rFilter.SaveUserPreference( "Attribute Key", rtbAttributeKey.Text );

            BindGrid();
        }

        /// <summary>
        /// Handles the ClearFilterClick event of the rFilter control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void rFilter_ClearFilterClick( object sender, EventArgs e )
        {
            rFilter.DeleteUserPreferences();
            BindFilter();
            BindGrid();
        }

        /// <summary>
        /// Rs the filter_ display filter value.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The e.</param>
        protected void rFilter_DisplayFilterValue( object sender, GridFilter.DisplayFilterValueArgs e )
        {
            switch ( e.Key )
            {
                case "Attribute Key":
                    {
                        break;
                    }

                case "Person":
                    {
                        string personName = string.Empty;

                        int? personId = e.Value.AsIntegerOrNull();
                        if ( personId.HasValue )
                        {
                            var personService = new PersonService( new RockContext() );
                            var person = personService.Get( personId.Value );
                            if ( person != null )
                            {
                                personName = person.FullName;
                            }
                        }

                        e.Value = personName;

                        break;
                    }

                default:
                    e.Value = string.Empty;
                    break;
            }
        }

        /// <summary>
        /// Handles the EditValue event of the rGrid control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RowEventArgs" /> instance containing the event data.</param>
        protected void rGrid_RowSelected( object sender, RowEventArgs e )
        {
            ShowEditValue( e.RowKeyId, true );
        }

        /// <summary>
        /// Handles the Delete event of the rGrid control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RowEventArgs" /> instance containing the event data.</param>
        protected void rGrid_Delete( object sender, RowEventArgs e )
        {
            var rockContext = new RockContext();
            var attributeValueService = new AttributeValueService( rockContext );
            var personService = new PersonService( rockContext );

            var attributeValue = attributeValueService.Get( e.RowKeyId );
            if ( attributeValue != null && attributeValue.EntityId.HasValue )
            {
                var person = personService.Get( attributeValue.EntityId.Value );

                string sessionKey = string.Format( "{0}_{1}",
                Person.USER_VALUE_ENTITY, person != null ? person.Id : 0 );
                var userPreferences = Session[sessionKey] as Dictionary<string, string>;
                if ( userPreferences == null )
                {
                    if ( person != null )
                    {
                        userPreferences = PersonService.GetUserPreferences( person );
                    }
                    else
                    {
                        userPreferences = new Dictionary<string, string>();
                    }
                    Session[sessionKey] = userPreferences;
                }

                var sessionValues = userPreferences;
                if ( sessionValues.ContainsKey( attributeValue.Attribute.Key ) )
                {
                    sessionValues.Remove( attributeValue.Attribute.Key );
                }

                PersonService.DeleteUserPreference( person, attributeValue.Attribute.Key );

            }

            BindGrid();
        }

        /// <summary>
        /// Handles the GridRebind event of the rGrid control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void rGrid_GridRebind( object sender, EventArgs e )
        {
            BindGrid();
        }

        /// <summary>
        /// Handles the RowDataBound event of the rGrid control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="GridViewRowEventArgs" /> instance containing the event data.</param>
        protected void rGrid_RowDataBound( object sender, GridViewRowEventArgs e )
        {
            if ( e.Row.RowType == DataControlRowType.DataRow )
            {
                int attributeId = ( int ) rGrid.DataKeys[e.Row.RowIndex].Value;

            }
        }

        /// <summary>
        /// Handles the SaveClick event of the mdAttributeValue control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void mdAttributeValue_SaveClick( object sender, EventArgs e )
        {
            int attributeValueId = 0;
            if ( hfAttributeValueId.Value != string.Empty && !int.TryParse( hfAttributeValueId.Value, out attributeValueId ) )
            {
                attributeValueId = 0;
            }

            if ( attributeValueId != 0 && phEditControls.Controls.Count > 0 )
            {

                var rockContext = new RockContext();
                var attributeValueService = new AttributeValueService( rockContext );
                var attributeValue = attributeValueService.Get( attributeValueId );
                if ( attributeValue != null )
                {
                    var attribute = Rock.Web.Cache.AttributeCache.Get( attributeValue.AttributeId );
                    var fieldType = FieldTypeCache.Get( attributeValue.Attribute.FieldType.Id );
                    attributeValue.Value = fieldType.Field.GetEditValue( attribute.GetControl( phEditControls.Controls[0] ), attribute.QualifierValues );
                    rockContext.SaveChanges();
                }
            }

            hfAttributeValueId.Value = string.Empty;

            HideDialog();
            BindGrid();
        }

        #endregion

        #region Methods

        /// <summary>
        /// Binds the filter.
        /// </summary>
        private void BindFilter()
        {
            rtbAttributeKey.Text = rFilter.GetUserPreference( "Attribute Key" );
            int? personId = rFilter.GetUserPreference( "Person" ).AsIntegerOrNull();

            if ( personId.HasValue && personId.Value != 0 )
            {
                var personService = new PersonService( new RockContext() );
                var person = personService.Get( personId.Value );
                if ( person != null )
                {
                    ppPerson.SetValue( person );
                }
            }
        }

        /// <summary>
        /// Binds the grid.
        /// </summary>
        private void BindGrid()
        {
            var rockContext = new RockContext();
            AttributeValueService attributeValueService = new AttributeValueService( rockContext );
            PersonService personService = new PersonService( rockContext );

            var attributeValues = attributeValueService.Queryable().AsNoTracking().Where( av => av.Attribute.EntityTypeId == _entityTypeId.Value );

            if ( rtbAttributeKey.Text.IsNotNullOrWhiteSpace() )
            {
                attributeValues = attributeValues.Where( av => av.Attribute.Key.Contains( rtbAttributeKey.Text ) );
            }

            var people = personService.Queryable().AsNoTracking();
            if ( ppPerson.PersonId != null )
            {
                people = people.Where( p => p.Id == ppPerson.PersonId );
            }

            var result = attributeValues.Join(
                people,
                k1 => k1.EntityId,
                k2 => k2.Id,
                ( attributeValue, person ) => new
                {
                    Id = attributeValue.Id,
                    AttributeValue = attributeValue,
                    Person = person
                } );
            result = result.OrderBy( av => av.AttributeValue.Attribute.Name ).ThenBy( av => av.Person.LastName ).ThenBy( av => av.Person.NickName );
            rGrid.DataSource = result.ToList();
            rGrid.DataBind();
        }

        /// <summary>
        /// Shows the edit value.
        /// </summary>
        /// <param name="attributeId">The attribute id.</param>
        /// <param name="setValues">if set to <c>true</c> [set values].</param>
        protected void ShowEditValue( int attributeValueId, bool setValues )
        {

            phEditControls.Controls.Clear();
            var attributeValue = new AttributeValueService( new RockContext() ).Get( attributeValueId );
            if ( attributeValue != null )
            {
                var attribute = Rock.Web.Cache.AttributeCache.Get( attributeValue.AttributeId );
                if ( attribute != null )
                {
                    mdAttributeValue.Title = attribute.Name + " Value";

                    string value = attributeValue != null && !string.IsNullOrWhiteSpace( attributeValue.Value ) ? attributeValue.Value : attribute.DefaultValue;
                    attribute.AddControl( phEditControls.Controls, value, string.Empty, setValues, true );

                    SetValidationGroup( phEditControls.Controls, mdAttributeValue.ValidationGroup );

                    if ( setValues )
                    {
                        hfAttributeValueId.Value = attributeValue.Id.ToString();
                        ShowDialog( "AttributeValue", true );
                    }
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
                case "ATTRIBUTEVALUE":
                    mdAttributeValue.Show();
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
                case "ATTRIBUTEVALUE":
                    mdAttributeValue.Hide();
                    break;
            }

            hfActiveDialog.Value = string.Empty;
        }

        /// <summary>
        /// Check if Entity Type Is Valid
        /// </summary>
        private bool IsEntityTypeValid()
        {
            if ( _entityTypeId.HasValue )
            {
                pnlGrid.Visible = true;
                nbMessage.Visible = false;
                return true;
            }
            else
            {
                pnlGrid.Visible = false;
                nbMessage.Text = "Please select an entity to display attributes for.";
                nbMessage.Visible = true;
                return false;
            }
        }

        #endregion
    }
}
