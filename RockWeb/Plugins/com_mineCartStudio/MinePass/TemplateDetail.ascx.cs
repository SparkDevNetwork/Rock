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
using System.Linq;
using System.Web.UI;
using System.Web.UI.WebControls;
using Rock;
using Rock.Data;
using Rock.Model;
using Rock.Web.UI.Controls;
using com.minecartstudio.MinePass.Client.Model;
using Rock.Constants;
using com.minecartstudio.MinePass.Common.Enums;
using Newtonsoft.Json;
using com.minecartstudio.MinePass.Common;
using com.minecartstudio.MinePass.Common.Fields;
using com.minecartstudio.MinePassCommon.Relevance;
using Rock.Web.Cache;

namespace RockWeb.Plugins.com_mineCartStudio.MinePass
{
    /// <summary>
    /// Block for editing Mine Pass Templates
    /// </summary>
    [DisplayName( "Mine Pass Template Detail" )]
    [Category( "Mine Cart Studio > Mine Pass" )]
    [Description( "Block for editing Mine Pass Templates." )]
    public partial class TemplateDetail : Rock.Web.UI.RockBlock
    {
        #region Fields

        // used for private variables
        private List<int> _orphanedBinaryFileIds = new List<int>();
        private List<int> _newImageBinaryFileIds = new List<int>();

        private enum FieldType { Header, Primary, Secondary, Back, Auxiliary }

        // ViewState Keys
        private const string VS_CURRENT_TEMPLATE_ID = "CurrentTemplateId";

        private const string VS_HEADER_FIELDS = "HeaderFields";
        private const string VS_PRIMARY_FIELDS = "PrimaryFields";
        private const string VS_SECONDARY_FIELDS = "SecondaryFields";
        private const string VS_BACK_FIELDS = "BackFields";
        private const string VS_AUXILIARY_FIELDS = "AuxiliaryFields";

        private const string VS_RELEVANCE_LOCATIONS = "RelevanceLocations";
        private const string VS_RELEVANCE_BEACONS = "RelevanceBeacons";

        private JsonSerializerSettings _viewStateJsonSettings = new JsonSerializerSettings
        {
            ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
            ContractResolver = new Rock.Utility.IgnoreUrlEncodedKeyContractResolver()
        };
        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets the current template identifier.
        /// </summary>
        /// <value>
        /// The current template identifier.
        /// </value>
        public int CurrentTemplateId
        {
            get
            {
                return ViewState[VS_CURRENT_TEMPLATE_ID] as int? ?? 0;
            }
            set
            {
                ViewState[VS_CURRENT_TEMPLATE_ID] = value;
            }
        }

        #region Field Properties
        public List<MinePassAppleField> HeaderFieldState
        {
            get
            {
                string jsonFields = ViewState[VS_HEADER_FIELDS] as string;
                if ( jsonFields.IsNotNullOrWhiteSpace() && jsonFields != "null" )
                {
                    return JsonConvert.DeserializeObject<List<MinePassAppleField>>( jsonFields );
                }
                else
                {
                    return new List<MinePassAppleField>();
                }
            }

            set
            {
                ViewState[VS_HEADER_FIELDS] = JsonConvert.SerializeObject( value, Formatting.None, _viewStateJsonSettings );
            }
        }

        public List<MinePassAppleField> PrimaryFieldState
        {
            get
            {
                string jsonFields = ViewState[VS_PRIMARY_FIELDS] as string;
                if ( jsonFields.IsNotNullOrWhiteSpace() && jsonFields != "null" )
                {
                    return JsonConvert.DeserializeObject<List<MinePassAppleField>>( jsonFields );
                }
                else
                {
                    return new List<MinePassAppleField>();
                }
            }

            set
            {
                ViewState[VS_PRIMARY_FIELDS] = JsonConvert.SerializeObject( value, Formatting.None, _viewStateJsonSettings );
            }
        }

        public List<MinePassAppleField> SecondaryFieldState
        {
            get
            {
                string jsonFields = ViewState[VS_SECONDARY_FIELDS] as string;
                if ( jsonFields.IsNotNullOrWhiteSpace() && jsonFields != "null" )
                {
                    return JsonConvert.DeserializeObject<List<MinePassAppleField>>( jsonFields );
                }
                else
                {
                    return new List<MinePassAppleField>();
                }
            }

            set
            {
                ViewState[VS_SECONDARY_FIELDS] = JsonConvert.SerializeObject( value, Formatting.None, _viewStateJsonSettings );
            }
        }

        public List<MinePassAppleField> BackFieldState
        {
            get
            {
                string jsonFields = ViewState[VS_BACK_FIELDS] as string;
                if ( jsonFields.IsNotNullOrWhiteSpace() && jsonFields != "null" )
                {
                    return JsonConvert.DeserializeObject<List<MinePassAppleField>>( jsonFields );
                }
                else
                {
                    return new List<MinePassAppleField>();
                }
            }

            set
            {
                ViewState[VS_BACK_FIELDS] = JsonConvert.SerializeObject( value, Formatting.None, _viewStateJsonSettings );
            }
        }

        public List<MinePassAppleField> AuxiliaryFieldState
        {
            get
            {
                string jsonFields = ViewState[VS_AUXILIARY_FIELDS] as string;
                if ( jsonFields.IsNotNullOrWhiteSpace() && jsonFields != "null" )
                {
                    return JsonConvert.DeserializeObject<List<MinePassAppleField>>( jsonFields );
                }
                else
                {
                    return new List<MinePassAppleField>();
                }
            }

            set
            {
                ViewState[VS_AUXILIARY_FIELDS] = JsonConvert.SerializeObject( value, Formatting.None, _viewStateJsonSettings );
            }
        }
        #endregion

        #region Relevance Properties
        public List<MinePassAppleRelevantLocation> RelevanceLocationsState
        {
            get
            {
                string jsonFields = ViewState[VS_RELEVANCE_LOCATIONS] as string;
                if ( jsonFields.IsNotNullOrWhiteSpace() && jsonFields != "null" )
                {
                    return JsonConvert.DeserializeObject<List<MinePassAppleRelevantLocation>>( jsonFields );
                }
                else
                {
                    return new List<MinePassAppleRelevantLocation>();
                }
            }

            set
            {
                ViewState[VS_RELEVANCE_LOCATIONS] = JsonConvert.SerializeObject( value, Formatting.None, _viewStateJsonSettings );
            }
        }

        public List<MinePassAppleRelevantBeacon> RelevanceBeaconsState
        {
            get
            {
                string jsonFields = ViewState[VS_RELEVANCE_BEACONS] as string;
                if ( jsonFields.IsNotNullOrWhiteSpace() && jsonFields != "null" )
                {
                    return JsonConvert.DeserializeObject<List<MinePassAppleRelevantBeacon>>( jsonFields );
                }
                else
                {
                    return new List<MinePassAppleRelevantBeacon>();
                }
            }

            set
            {
                ViewState[VS_RELEVANCE_BEACONS] = JsonConvert.SerializeObject( value, Formatting.None, _viewStateJsonSettings );
            }
        }
        #endregion

        #endregion

        #region Base Control Methods

        //  overrides of the base RockBlock methods (i.e. OnInit, OnLoad)

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );

            CurrentTemplateId = ViewState["CurrentGroupTypeId"] as int? ?? 0;

            // this event gets fired after block settings are updated. it's nice to repaint the screen if these settings would alter it
            this.BlockUpdated += Block_BlockUpdated;
            this.AddConfigurationUpdateTrigger( upnlContent );

            // Field Grids
            gHeaderFields.GridRebind += gHeaderFields_GridRebind;
            gHeaderFields.DataKeyNames = new string[] { "Label" };
            gHeaderFields.Actions.ShowAdd = true;
            gHeaderFields.Actions.AddClick += gHeaderFields_AddClick;
            gHeaderFields.GridReorder += gHeaderFields_GridReorder;

            gPrimaryFields.GridRebind += gPrimaryFields_GridRebind;
            gPrimaryFields.DataKeyNames = new string[] { "Label" };
            gPrimaryFields.Actions.ShowAdd = true;
            gPrimaryFields.Actions.AddClick += gPrimaryFields_AddClick;
            gPrimaryFields.GridReorder += gPrimaryFields_GridReorder;

            gSecondaryFields.GridRebind += gSecondaryFields_GridRebind;
            gSecondaryFields.DataKeyNames = new string[] { "Label" };
            gSecondaryFields.Actions.ShowAdd = true;
            gSecondaryFields.Actions.AddClick += gSecondaryFields_AddClick;
            gSecondaryFields.GridReorder += gSecondaryFields_GridReorder;

            gBackFields.GridRebind += gBackFields_GridRebind;
            gBackFields.DataKeyNames = new string[] { "Label" };
            gBackFields.Actions.ShowAdd = true;
            gBackFields.Actions.AddClick += gBackFields_AddClick;
            gBackFields.GridReorder += gBackFields_GridReorder;

            gAuxiliaryFields.GridRebind += gAuxiliaryFields_GridRebind;
            gAuxiliaryFields.DataKeyNames = new string[] { "Label" };
            gAuxiliaryFields.Actions.ShowAdd = true;
            gAuxiliaryFields.Actions.AddClick += gAuxiliaryFields_AddClick;
            gAuxiliaryFields.GridReorder += gAuxiliaryFields_GridReorder;

            // Relevance Grids
            gLocations.GridRebind += gLocations_GridRebind;
            gLocations.DataKeyNames = new string[] { "Key" };
            gLocations.Actions.ShowAdd = true;
            gLocations.Actions.AddClick += gLocations_AddClick;
            gLocations.ShowConfirmDeleteDialog = true;

            gBeacons.GridRebind += gBeacons_GridRebind;
            gBeacons.DataKeyNames = new string[] { "Key" };
            gBeacons.Actions.ShowAdd = true;
            gBeacons.Actions.AddClick += gBeacons_AddClick;
            gBeacons.ShowConfirmDeleteDialog = true;

            // Image Pickers
            var defaultBindaryFileTypeGuid = new Guid( Rock.SystemGuid.BinaryFiletype.DEFAULT );
            imgupIcon.BinaryFileTypeGuid = defaultBindaryFileTypeGuid;
            imgupIconRetina.BinaryFileTypeGuid = defaultBindaryFileTypeGuid;

            imgupLogo.BinaryFileTypeGuid = defaultBindaryFileTypeGuid;
            imgupLogoRetina.BinaryFileTypeGuid = defaultBindaryFileTypeGuid;

            imgupStrip.BinaryFileTypeGuid = defaultBindaryFileTypeGuid;
            imgupStripRetina.BinaryFileTypeGuid = defaultBindaryFileTypeGuid;

            imgupFooter.BinaryFileTypeGuid = defaultBindaryFileTypeGuid;
            imgupFooterRetina.BinaryFileTypeGuid = defaultBindaryFileTypeGuid;

            imgupBackground.BinaryFileTypeGuid = defaultBindaryFileTypeGuid;
            imgupBackgroundRetina.BinaryFileTypeGuid = defaultBindaryFileTypeGuid;
        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Load" /> event.
        /// </summary>
        /// <param name="e">The <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnLoad( EventArgs e )
        {
            base.OnLoad( e );

            if ( !Page.IsPostBack )
            {
                CurrentTemplateId = PageParameter( "TemplateId" ).AsInteger();
                ShowDetail();
            }
        }
        
        #endregion

        #region Grid Events

        #region Field Grids
        private void ReorderField( int oldIndex, int newIndex, FieldType fieldType )
        {
            // Get field list
            var fieldList = GetFieldsOfType( fieldType );

            // Reorder fields
            var sourceField = fieldList[oldIndex];

            fieldList.RemoveAt(oldIndex);
            fieldList.Insert(newIndex, sourceField);

            // Reset field list back to viewstate
            SetFieldsOfType( fieldType, fieldList );

            BindFields();
        }

        private void DeleteField( int index, FieldType fieldType )
        {
            // Get field list
            var fieldList = GetFieldsOfType( fieldType );

            // Remove item
            fieldList.RemoveAt( index );

            // Reset field list back to viewstate
            SetFieldsOfType( fieldType, fieldList );

            BindFields();
        }

        private void ShowFieldEditModal( int index, FieldType fieldType )
        {
            var fieldList = GetFieldsOfType( fieldType );
            var field = fieldList[index];

            hfFieldKey.Value = field.Key;
            hfFieldType.Value = fieldType.ToString();

            rdlFieldType.BindToEnum<MinePassAppleFieldType>();
            rdlFieldType.SelectedValue = ( ( int ) field.PassFieldType ).ToString();

            rdlFieldAlignment.BindToEnum<MinePassAppleFieldAlignment>();
            rdlFieldAlignment.SelectedValue = ( ( int ) field.Alignment ).ToString();

            tbLabel.Text = field.Label;
            ceFieldValue.Text = field.Value;

            mdFieldDetail.Show();
        }

        // Header
        private void gHeaderFields_GridReorder( object sender, GridReorderEventArgs e )
        {
            ReorderField( e.OldIndex, e.NewIndex, FieldType.Header );
        }

        private void gHeaderFields_AddClick( object sender, EventArgs e )
        {
            ShowFieldAddModal( FieldType.Header );
        }

        private void gHeaderFields_GridRebind( object sender, GridRebindEventArgs e )
        {
            BindFields();
        }

        protected void gHeaderFields_Delete( object sender, RowEventArgs e )
        {
            DeleteField( e.RowIndex, FieldType.Header );
        }

        protected void gHeaderFields_Edit( object sender, RowEventArgs e )
        {
            ShowFieldEditModal( e.RowIndex, FieldType.Header );
        }

        // Primary
        private void gPrimaryFields_GridReorder( object sender, GridReorderEventArgs e )
        {
            ReorderField( e.OldIndex, e.NewIndex, FieldType.Primary );
        }

        private void gPrimaryFields_AddClick( object sender, EventArgs e )
        {
            ShowFieldAddModal( FieldType.Primary );
        }

        private void gPrimaryFields_GridRebind( object sender, GridRebindEventArgs e )
        {
            BindFields();
        }

        protected void gPrimaryFields_Delete( object sender, RowEventArgs e )
        {
            DeleteField( e.RowIndex, FieldType.Primary );
        }

        protected void gPrimaryFields_Edit( object sender, RowEventArgs e )
        {
            ShowFieldEditModal( e.RowIndex, FieldType.Primary );
        }

        // Secondary
        private void gSecondaryFields_GridReorder( object sender, GridReorderEventArgs e )
        {
            ReorderField( e.OldIndex, e.NewIndex, FieldType.Secondary );
        }

        private void gSecondaryFields_AddClick( object sender, EventArgs e )
        {
            ShowFieldAddModal( FieldType.Secondary );
        }

        private void gSecondaryFields_GridRebind( object sender, GridRebindEventArgs e )
        {
            BindFields();
        }
        protected void gSecondaryFields_Delete( object sender, RowEventArgs e )
        {
            DeleteField( e.RowIndex, FieldType.Secondary );
        }

        protected void gSecondaryFields_Edit( object sender, RowEventArgs e )
        {
            ShowFieldEditModal( e.RowIndex, FieldType.Secondary );
        }

        // Back
        private void gBackFields_GridReorder( object sender, GridReorderEventArgs e )
        {
            ReorderField( e.OldIndex, e.NewIndex, FieldType.Back );
        }

        private void gBackFields_AddClick( object sender, EventArgs e )
        {
            ShowFieldAddModal( FieldType.Back );
        }

        private void gBackFields_GridRebind( object sender, GridRebindEventArgs e )
        {
            BindFields();
        }
        protected void gBackFields_Delete( object sender, RowEventArgs e )
        {
            DeleteField( e.RowIndex, FieldType.Back );
        }

        protected void gBackFields_Edit( object sender, RowEventArgs e )
        {
            ShowFieldEditModal( e.RowIndex, FieldType.Back );
        }

        // Auxiliary
        private void gAuxiliaryFields_GridReorder( object sender, GridReorderEventArgs e )
        {
            ReorderField( e.OldIndex, e.NewIndex, FieldType.Auxiliary );
        }

        private void gAuxiliaryFields_AddClick( object sender, EventArgs e )
        {
            ShowFieldAddModal( FieldType.Auxiliary );
        }

        private void gAuxiliaryFields_GridRebind( object sender, GridRebindEventArgs e )
        {
            BindFields();
        }
        protected void gAuxiliaryFields_Delete( object sender, RowEventArgs e )
        {
            DeleteField( e.RowIndex, FieldType.Auxiliary );
        }

        protected void gAuxiliaryFields_Edit( object sender, RowEventArgs e )
        {
            ShowFieldEditModal( e.RowIndex, FieldType.Auxiliary );
        }

        private void ShowFieldAddModal( FieldType fieldType )
        {
            hfFieldKey.Value = string.Empty;
            hfFieldType.Value = fieldType.ToString();

            rdlFieldType.BindToEnum<MinePassAppleFieldType>();
            rdlFieldType.SelectedValue = ( ( int ) MinePassAppleFieldType.Standard ).ToString();
 
            rdlFieldAlignment.BindToEnum<MinePassAppleFieldAlignment>();
            rdlFieldAlignment.SelectedValue = ( ( int ) MinePassAppleFieldAlignment.Natural ).ToString();

            tbLabel.Text = string.Empty;
            ceFieldValue.Text = string.Empty;
            
            mdFieldDetail.Show();
        }

        #endregion

        #region Relevance Grids

        // Locations
        protected void gLocations_Delete( object sender, RowEventArgs e )
        {
            var locations = RelevanceLocationsState;
            locations.RemoveAt( e.RowIndex );
            RelevanceLocationsState = locations;

            BindLocations();
        }

        protected void gLocations_Edit( object sender, RowEventArgs e )
        {
            var location = RelevanceLocationsState[e.RowIndex];

            hfLocationKey.Value = location.Key;

            tbLatitude.Text = location.Latitude;
            tbLongitude.Text = location.Longitude;
            ceLocationMessage.Text = location.Message;

            mdLocationDetail.Show();
        }

        private void gLocations_AddClick( object sender, EventArgs e )
        {
            hfLocationKey.Value = string.Empty;

            tbLatitude.Text = string.Empty;
            tbLongitude.Text = string.Empty;
            ceLocationMessage.Text = string.Empty;

            mdLocationDetail.Show();
        }

        private void gLocations_GridRebind( object sender, GridRebindEventArgs e )
        {
            BindLocations();
        }

        private void BindLocations()
        {
            gLocations.DataSource = this.RelevanceLocationsState;
            gLocations.DataBind();
        }

        // Beacons
        /// <summary>
        /// Handles the Delete event of the gBeacons control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RowEventArgs"/> instance containing the event data.</param>
        protected void gBeacons_Delete( object sender, RowEventArgs e )
        {
            var beacons = RelevanceBeaconsState;
            beacons.RemoveAt( e.RowIndex );
            RelevanceBeaconsState = beacons;

            BindBeacons();
        }

        /// <summary>
        /// Handles the Edit event of the gBeacons control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RowEventArgs"/> instance containing the event data.</param>
        protected void gBeacons_Edit( object sender, RowEventArgs e )
        {
            var beacon = RelevanceBeaconsState[e.RowIndex];

            hfBeaconKey.Value = beacon.Key;

            txtProximityUuid.Text = beacon.ProximityUuid;
            nbMajor.Text = beacon.Major.ToString();
            nbMinor.Text = beacon.Minor.ToString();
            ceBeaconMessage.Text = beacon.Message;

            mdBeaconDetail.Show();
        }

        /// <summary>
        /// Handles the AddClick event of the gBeacons control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void gBeacons_AddClick( object sender, EventArgs e )
        {
            hfBeaconKey.Value = string.Empty;

            txtProximityUuid.Text = string.Empty;
            nbMajor.Text = string.Empty;
            nbMinor.Text = string.Empty;
            ceBeaconMessage.Text = string.Empty;

            mdBeaconDetail.Show();
        }

        /// <summary>
        /// Handles the GridRebind event of the gBeacons control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="GridRebindEventArgs"/> instance containing the event data.</param>
        private void gBeacons_GridRebind( object sender, GridRebindEventArgs e )
        {
            BindBeacons();
        }

        /// <summary>
        /// Binds the beacons.
        /// </summary>
        private void BindBeacons()
        {
            gBeacons.DataSource = this.RelevanceBeaconsState;
            gBeacons.DataBind();
        }
        #endregion

        #endregion

        #region Events

        #region Dialogs

        /// <summary>
        /// Handles the SaveClick event of the mdLocationDetail control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void mdLocationDetail_SaveClick( object sender, EventArgs e )
        {
            string locationKey = hfLocationKey.Value;
            
            var location = new MinePassAppleRelevantLocation();
            if ( locationKey.IsNotNullOrWhiteSpace() )
            {
                // Edit
                location.Key = locationKey;
            }
            else
            {
                // Add
                location.Key = Guid.NewGuid().ToString();                ;
            }

            location.Longitude = tbLongitude.Text;
            location.Latitude = tbLatitude.Text;
            location.Message = ceLocationMessage.Text;

            var locations = this.RelevanceLocationsState;
            if ( location.Key.IsNotNullOrWhiteSpace() )
            {
                var existingField = locations.Where( f => f.Key == location.Key ).FirstOrDefault();

                if ( existingField != null )
                {
                    var indexOfExistingField = locations.IndexOf( existingField );
                    locations[indexOfExistingField] = location;
                }
                else
                {
                    locations.Add( location );
                }

            }
            else
            {
                locations.Add( location );
            }

            this.RelevanceLocationsState = locations;

            BindLocations();
            mdLocationDetail.Hide();
        }

        /// <summary>
        /// Handles the SaveClick event of the mdBeaconDetail control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void mdBeaconDetail_SaveClick( object sender, EventArgs e )
        {
            string beaconKey = hfBeaconKey.Value;

            var beacon = new MinePassAppleRelevantBeacon();
            if ( beaconKey.IsNotNullOrWhiteSpace() )
            {
                // Edit
                beacon.Key = beaconKey;
            }
            else
            {
                // Add
                beacon.Key = Guid.NewGuid().ToString();
                ;
            }

            beacon.ProximityUuid = txtProximityUuid.Text;
            beacon.Major = nbMajor.Text.AsInteger();
            beacon.Minor = nbMinor.Text.AsInteger();
            beacon.Message = ceBeaconMessage.Text;

            var beacons = this.RelevanceBeaconsState;
            if ( beacon.Key.IsNotNullOrWhiteSpace() )
            {
                var existingField = beacons.Where( f => f.Key == beacon.Key ).FirstOrDefault();

                if ( existingField != null )
                {
                    var indexOfExistingField = beacons.IndexOf( existingField );
                    beacons[indexOfExistingField] = beacon;
                }
                else
                {
                    beacons.Add( beacon );
                }

            }
            else
            {
                beacons.Add( beacon );
            }

            this.RelevanceBeaconsState = beacons;

            BindBeacons();
            mdBeaconDetail.Hide();
        }

        /// <summary>
        /// Handles the SaveClick event of the mdAccount control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void mdFieldDetail_SaveClick( object sender, EventArgs e )
        {
            string fieldKey = hfFieldKey.Value;
            var fieldType = (FieldType)Enum.Parse( typeof( FieldType ), hfFieldType.Value );

            var field = new MinePassAppleField();
            if ( fieldKey.IsNotNullOrWhiteSpace() )
            {
                // Edit
                field.Key = fieldKey;
            }
            else
            {
                // Add
                field.Key = MakeFieldKey( tbLabel.Text );
            }

            field.Label = tbLabel.Text;
            field.Value = ceFieldValue.Text;
            field.Alignment = rdlFieldAlignment.SelectedValueAsEnum<MinePassAppleFieldAlignment>( MinePassAppleFieldAlignment.Natural );

            SaveField( field, fieldType );

            BindFields();
            mdFieldDetail.Hide();
        }

        /// <summary>
        /// Makes the field key that is guaranteed to be unique.
        /// </summary>
        /// <param name="label">The label.</param>
        /// <param name="increment">The increment.</param>
        /// <returns></returns>
        private string MakeFieldKey( string label, int increment = 0 )
        {
            var keyBase = label.Replace( ' ', '-' );

            var key = string.Format( "{0}-{1}", keyBase, increment );

            var allKeys = HeaderFieldState.Select( f => f.Key ).ToList();
            allKeys.AddRange( PrimaryFieldState.Select( f => f.Key ).ToList() );
            allKeys.AddRange( SecondaryFieldState.Select( f => f.Key ).ToList() );
            allKeys.AddRange( BackFieldState.Select( f => f.Key ).ToList() );
            allKeys.AddRange( AuxiliaryFieldState.Select( f => f.Key ).ToList() );

            if ( allKeys.Contains( key ) )
            {
                key = MakeFieldKey( label, increment++ );
            }

            return key;
        }

        /// <summary>
        /// Gets the type of the fields of.
        /// </summary>
        /// <param name="fieldType">Type of the field.</param>
        /// <returns></returns>
        private List<MinePassAppleField> GetFieldsOfType( FieldType fieldType )
        {
            switch ( fieldType )
            {
                case FieldType.Header:
                    {
                        return HeaderFieldState;
                    }
                case FieldType.Primary:
                    {
                        return PrimaryFieldState;
                    }
                case FieldType.Secondary:
                    {
                        return SecondaryFieldState;
                    }
                case FieldType.Back:
                    {
                        return BackFieldState;
                    }
                case FieldType.Auxiliary:
                    {
                        return AuxiliaryFieldState;
                    }
                default:
                    {
                        return new List<MinePassAppleField>();
                    }
            }
        }

        /// <summary>
        /// Sets the type of the fields of.
        /// </summary>
        /// <param name="fieldType">Type of the field.</param>
        /// <param name="fieldList">The field list.</param>
        private void SetFieldsOfType( FieldType fieldType, List<MinePassAppleField> fieldList )
        {
            switch ( fieldType )
            {
                case FieldType.Header:
                    {
                        HeaderFieldState = fieldList;
                        break;
                    }
                case FieldType.Primary:
                    {
                        PrimaryFieldState = fieldList;
                        break;
                    }
                case FieldType.Secondary:
                    {
                        SecondaryFieldState = fieldList;
                        break;
                    }
                case FieldType.Back:
                    {
                        BackFieldState = fieldList;
                        break;
                    }
                case FieldType.Auxiliary:
                    {
                        AuxiliaryFieldState = fieldList;
                        break;
                    }
            }
        }

        /// <summary>
        /// Saves the field.
        /// </summary>
        /// <param name="field">The field.</param>
        /// <param name="fieldType">Type of the field.</param>
        /// <returns></returns>
        private MinePassAppleField SaveField( MinePassAppleField field, FieldType fieldType )
        {
            var fieldList = GetFieldsOfType( fieldType );           ;

            if ( field.Key.IsNotNullOrWhiteSpace() )
            {
                var existingField = fieldList.Where( f => f.Key == field.Key ).FirstOrDefault();

                if (existingField != null )
                {
                    var indexOfExistingField = fieldList.IndexOf( existingField );
                    fieldList[indexOfExistingField] = field;
                }
                else
                {
                    fieldList.Add( field );
                }
                
            }
            else
            {
                fieldList.Add( field );
            }

            // Reset field list back to viewstate
            SetFieldsOfType( fieldType, fieldList );

            return field;
        }
        #endregion

        // handlers called by the controls on your block

        /// <summary>
        /// Handles the BlockUpdated event of the control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void Block_BlockUpdated( object sender, EventArgs e )
        {

        }

        /// <summary>
        /// Handles the Click event of the btnEdit control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void btnEdit_Click( object sender, EventArgs e )
        {
            // Reset to General Tab
            ResetNavTabs();
            pnlTabGeneral.Visible = true;
            tabGeneral.AddCssClass( "active" );

            var template = new MinePassTemplateService( new RockContext() ).Get( CurrentTemplateId );
            SetEditMode( true );
            ShowEditDetails( template );
        }

        /// <summary>
        /// Handles the Click event of the btnSaveType control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void btnSave_Click( object sender, EventArgs e )
        {
            var rockContext = new RockContext();

            MinePassTemplate template = null;
            var templateService = new MinePassTemplateService( rockContext );

            if ( CurrentTemplateId > 0 )
            {
                template = templateService.Get( CurrentTemplateId );
            }

            if ( template == null )
            {
                template = new MinePassTemplate();
                templateService.Add( template );
            }

            template.Name = tbName.Text;
            template.IsActive = cbIsActive.Checked;

            var applePass = new MinePassApple();
            applePass.PassStyle = rblPassType.SelectedValueAsEnum<MinePassAppleStyle>( MinePassAppleStyle.Generic );
            applePass.TransitType = rblTransitType.SelectedValueAsEnum<MinePassAppleTransitType>( MinePassAppleTransitType.Air );

            applePass.OrganizationName = txtOrganizationName.Text;
            applePass.Description = txtPassDescription.Text;
            applePass.LogoText = txtLogoText.Text;

            applePass.BackgroundColor = cpBackgroundColor.Text;
            applePass.LabelColor = cpLabelColor.Text;
            applePass.ForegroundColor = cpForegroundColor.Text;

            applePass.ShowBarcode = cbShowBarcode.Checked;
            applePass.BarcodeValue = ceBarcodeValue.Text;
            applePass.BarcodeAlternateText = ceBarcodeAlternateText.Text;
            applePass.BarcodeEncoding = txtBarcodeEncoding.Text;
            applePass.BarCodeType = rblBarcodeType.SelectedValueAsEnum<MinePassAppleBarcodeType>( MinePassAppleBarcodeType.Qr );

            // Images
            applePass.ThumbnailImageUrl = ceThumbnailImageUrl.Text;
            applePass.ThumbnailImageRetinaUrl = ceThumbnailRetinaImageUrl.Text;

            template.IconImageBinaryFileId = SaveImage( imgupIcon, template.IconImageBinaryFileId );
            applePass.IconUrl = GetImageUrl( template.IconImageBinaryFileId );

            template.IconImageRetinaBinaryFileId = SaveImage( imgupIconRetina, template.IconImageRetinaBinaryFileId );
            applePass.IconRetinaUrl = GetImageUrl( template.IconImageRetinaBinaryFileId );

            template.LogoImageBinaryFileId = SaveImage( imgupLogo, template.LogoImageBinaryFileId );
            applePass.LogoUrl = GetImageUrl( template.LogoImageBinaryFileId );

            template.LogoImageRetinaBinaryFileId = SaveImage( imgupLogoRetina, template.LogoImageRetinaBinaryFileId );
            applePass.LogoRetinaUrl = GetImageUrl( template.LogoImageRetinaBinaryFileId );

            template.StripImageBinaryFileId = SaveImage( imgupStrip, template.StripImageBinaryFileId );
            applePass.StripImageUrl = GetImageUrl( template.StripImageBinaryFileId );

            template.StripImageRetinaBinaryFileId = SaveImage( imgupStripRetina, template.StripImageRetinaBinaryFileId );
            applePass.StripImageRetinaUrl = GetImageUrl( template.StripImageRetinaBinaryFileId );

            template.FooterImageBinaryFileId = SaveImage( imgupFooter, template.FooterImageBinaryFileId );
            applePass.FooterImageUrl = GetImageUrl( template.FooterImageBinaryFileId );

            template.FooterImageRetinaBinaryFileId = SaveImage( imgupFooterRetina, template.FooterImageRetinaBinaryFileId );
            applePass.FooterImageRetinaUrl = GetImageUrl( template.FooterImageRetinaBinaryFileId );

            template.BackgroundImageBinaryFileId = SaveImage( imgupBackground, template.BackgroundImageBinaryFileId );
            applePass.BackgroundImageUrl = GetImageUrl( template.BackgroundImageBinaryFileId );

            template.BackgroundImageRetinaBinaryFileId = SaveImage( imgupBackgroundRetina, template.BackgroundImageRetinaBinaryFileId );
            applePass.BackgroundImageRetinaUrl = GetImageUrl( template.BackgroundImageRetinaBinaryFileId );

            // Fields
            applePass.HeaderFields = HeaderFieldState;
            applePass.PrimaryFields = PrimaryFieldState;
            applePass.SecondaryFields = SecondaryFieldState;
            applePass.BackFields = BackFieldState;
            applePass.AuxiliaryFields = AuxiliaryFieldState;

            // Relevance
            applePass.RelevantLocations = RelevanceLocationsState.Take(10).ToList();
            applePass.RelevantBeacons = RelevanceBeaconsState.Take( 10 ).ToList();
            applePass.RelevantDate = dpRelevantDate.SelectedDate;
            applePass.ExpirationDate = dpExpirationDate.SelectedDate;

            if ( !template.IsValid )
            {
                // Controls will render the error messages                    
                return;
            }

            template.AppleTemplateData = applePass.ToJson();

            rockContext.SaveChanges();

            // Clean up images
            var binaryFileService = new BinaryFileService( rockContext );

            foreach( var orphanedBinaryFileId in _orphanedBinaryFileIds )
            {
                var binaryFile = binaryFileService.Get( orphanedBinaryFileId );

                if (binaryFile != null )
                {
                    binaryFileService.Delete( binaryFile );
                }
            }

            foreach( var newImageBinaryFileId in _newImageBinaryFileIds )
            {
                var binaryFile = binaryFileService.Get( newImageBinaryFileId );

                if ( binaryFile != null )
                {
                    binaryFile.IsTemporary = false;
                }
            }

            rockContext.SaveChanges();

            // Reload page
            var qryParams = new Dictionary<string, string>();
            qryParams["TemplateId"] = template.Id.ToString();

            NavigateToPage( RockPage.Guid, qryParams );
        }

        /// <summary>
        /// Handles the Click event of the btnCancelType control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void btnCancel_Click( object sender, EventArgs e )
        {
            if ( CurrentTemplateId == 0 )
            {
                // Cancelling on Add.  Return to Grid
                NavigateToParentPage();
            }
            else
            {
                // Cancelling on Edit.  Return to Details
                var template = new MinePassTemplateService( new RockContext() ).Get( CurrentTemplateId );
                ShowReadonlyDetails( template );
            }
        }

        /// <summary>
        /// Handles the Click event of the btnTab controls.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnTab_Click( object sender, EventArgs e )
        {
            ResetNavTabs();

            var control = ( Control ) sender;
            switch ( control.ID )
            {
                case "btnTabImages":
                    {
                        tabImages.AddCssClass( "active" );
                        pnlTabImages.Visible = true;
                        break;
                    }
                case "btnTabFields":
                    {
                        tabFields.AddCssClass( "active" );
                        pnlTabFields.Visible = true;
                        break;
                    }
                case "btnTabBarcode":
                    {
                        tabBarcode.AddCssClass( "active" );
                        pnlTabBarcode.Visible = true;
                        break;
                    }
                case "btnTabRelevance":
                    {
                        tabRelevance.AddCssClass( "active" );
                        pnlTabRelevance.Visible = true;
                        break;
                    }
                default:
                    {
                        tabGeneral.AddCssClass( "active" );
                        pnlTabGeneral.Visible = true;
                        break;
                    }
            }
        }

        /// <summary>
        /// Handles the SelectedIndexChanged event of the rblPassType control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void rblPassType_SelectedIndexChanged( object sender, EventArgs e )
        {
            // Show / hide the transit type picker
            var rblPassType = ( RockRadioButtonList ) sender;

            var selectedPass = rblPassType.SelectedValueAsEnum<MinePassAppleStyle>( MinePassAppleStyle.Generic );

            rblTransitType.Visible = selectedPass == MinePassAppleStyle.BoardingPass;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Gets the image URL.
        /// </summary>
        /// <param name="binaryFileId">The binary file identifier.</param>
        /// <returns></returns>
        private string GetImageUrl( int? binaryFileId )
        {
            if ( !binaryFileId.HasValue )
            {
                return string.Empty;
            }

            var globalAttributes = GlobalAttributesCache.Get();
            var publicApplicationRoot = globalAttributes.GetValue( "PublicApplicationRoot" );

            return string.Format( "{0}{1}GetImage.ashx?Id={2}", publicApplicationRoot.TrimEnd('/'), ResolveRockUrl( "~/"), binaryFileId.Value );
        }

        /// <summary>
        /// Saves the image.
        /// </summary>
        /// <param name="imgControl">The img control.</param>
        /// <param name="currentValue">The current value.</param>
        /// <returns></returns>
        private int? SaveImage( Rock.Web.UI.Controls.ImageUploader imgControl, int? currentValue )
        {
            int? orphanedImageId = null;
            var newImageId = imgControl.BinaryFileId;

            if ( currentValue.HasValue &&
                !currentValue.Equals( newImageId ) )
            {
                orphanedImageId = currentValue;
            }

            // Delete any ophaned images
            if ( orphanedImageId.HasValue )
            {
                _orphanedBinaryFileIds.Add( orphanedImageId.Value );
            }

            // Mark new images as not temporary
            if ( newImageId.HasValue )
            {
                _newImageBinaryFileIds.Add( newImageId.Value );
            }

            return newImageId;
        }

        /// <summary>
        /// Resets the nav tabs.
        /// </summary>
        private void ResetNavTabs()
        {
            tabGeneral.RemoveCssClass( "active" );
            tabImages.RemoveCssClass( "active" );
            tabBarcode.RemoveCssClass( "active" );
            tabFields.RemoveCssClass( "active" );
            tabRelevance.RemoveCssClass( "active" );

            pnlTabGeneral.Visible = false;
            pnlTabImages.Visible = false;
            pnlTabBarcode.Visible = false;
            pnlTabFields.Visible = false;
            pnlTabRelevance.Visible = false;
        }

        /// <summary>
        /// Binds all field grids.
        /// </summary>
        private void BindFields()
        {
            gHeaderFields.DataSource = this.HeaderFieldState;
            gHeaderFields.DataBind();

            gPrimaryFields.DataSource = this.PrimaryFieldState;
            gPrimaryFields.DataBind();

            gSecondaryFields.DataSource = this.SecondaryFieldState;
            gSecondaryFields.DataBind();

            gBackFields.DataSource = this.BackFieldState;
            gBackFields.DataBind();

            gAuxiliaryFields.DataSource = this.AuxiliaryFieldState;
            gAuxiliaryFields.DataBind();
        }

        /// <summary>
        /// Sets the edit mode.
        /// </summary>
        /// <param name="editable">if set to <c>true</c> [editable].</param>
        private void SetEditMode( bool editable )
        {
            pnlEditDetails.Visible = editable;
            fieldsetViewSummary.Visible = !editable;

            this.HideSecondaryBlocks( editable );
        }

        /// <summary>
        /// Shows the detail.
        /// </summary>
        private void ShowDetail()
        {
            MinePassTemplate template = null;

            if ( !CurrentTemplateId.Equals( 0 ) )
            {
                template = new MinePassTemplateService( new RockContext() ).Get( CurrentTemplateId );
            }

            if ( template == null )
            {
                template = new MinePassTemplate { Id = 0 };
            }

            bool readOnly = false;

            if ( !UserCanEdit )
            {
                readOnly = true;
                nbEditModeMessage.Text = Rock.Constants.EditModeMessage.ReadOnlyEditActionNotAllowed( MinePassTemplate.FriendlyTypeName );
            }

            if ( readOnly )
            {
                btnEdit.Visible = false;
                ShowReadonlyDetails( template );
            }
            else
            {
                btnEdit.Visible = true;
                if ( template.Id > 0 )
                {
                    ShowReadonlyDetails( template );
                }
                else
                {
                    ShowEditDetails( template );
                }
            }

            hlblIsActive.Visible = !template.IsActive;
        }

        /// <summary>
        /// Shows the readonly details.
        /// </summary>
        /// <param name="template">The template.</param>
        private void ShowReadonlyDetails( MinePassTemplate template )
        {
            SetEditMode( false );
            hlblIsActive.Visible = !template.IsActive;

            lTitle.Text = template.Name;

            lDescription.Text = template.Description;
            lTemplateName.Text = template.Name;

            var applePass = ConvertToApplePass( template.AppleTemplateData );

            lPassType.Text = string.Format( "<div class='text-center pull-right'><i class='{0} fa-7x' style='opacity: .5;'></i> <br> {1}</div>", applePass.PassStyle.GetIconCssClass(), applePass.PassStyle.GetFriendlyName() );
        }

        /// <summary>
        /// Shows the edit details.
        /// </summary>
        /// <param name="template">The template.</param>
        private void ShowEditDetails( MinePassTemplate template )
        {
            SetEditMode( true );

            tbName.Text = template.Name;
            cbIsActive.Checked = template.IsActive;

            var applePass = ConvertToApplePass( template.AppleTemplateData );
                      
            rblPassType.BindToEnum<MinePassAppleStyle>();
            rblPassType.SelectedValue = ((int)applePass.PassStyle).ToString();

            rblTransitType.BindToEnum<MinePassAppleTransitType>();
            rblTransitType.SelectedValue = ( ( int ) applePass.TransitType ).ToString();

            rblTransitType.Visible = applePass.PassStyle == MinePassAppleStyle.BoardingPass;

            txtOrganizationName.Text = applePass.OrganizationName;
            txtPassDescription.Text = applePass.Description;
            txtLogoText.Text = applePass.LogoText;

            cpBackgroundColor.Text = applePass.BackgroundColor;
            cpForegroundColor.Text = applePass.ForegroundColor;
            cpLabelColor.Text = applePass.LabelColor;

            cbShowBarcode.Checked = applePass.ShowBarcode;
            ceBarcodeValue.Text = applePass.BarcodeValue;
            ceBarcodeAlternateText.Text = applePass.BarcodeAlternateText;
            txtBarcodeEncoding.Text = applePass.BarcodeEncoding;
            rblBarcodeType.BindToEnum<MinePassAppleBarcodeType>();
            rblBarcodeType.SelectedValue = ( ( int ) applePass.BarCodeType ).ToString();

            // Images
            ceThumbnailImageUrl.Text = applePass.ThumbnailImageUrl;
            ceThumbnailRetinaImageUrl.Text = applePass.ThumbnailImageRetinaUrl;

            imgupIcon.BinaryFileId = template.IconImageBinaryFileId;
            imgupIconRetina.BinaryFileId = template.IconImageRetinaBinaryFileId;

            imgupLogo.BinaryFileId = template.LogoImageBinaryFileId;
            imgupLogoRetina.BinaryFileId = template.LogoImageRetinaBinaryFileId;

            imgupStrip.BinaryFileId = template.StripImageBinaryFileId;
            imgupStripRetina.BinaryFileId = template.StripImageRetinaBinaryFileId;

            imgupFooter.BinaryFileId = template.FooterImageBinaryFileId;
            imgupFooterRetina.BinaryFileId = template.FooterImageRetinaBinaryFileId;

            imgupBackground.BinaryFileId = template.BackgroundImageBinaryFileId;
            imgupBackgroundRetina.BinaryFileId = template.BackgroundImageRetinaBinaryFileId;

            this.HeaderFieldState = applePass.HeaderFields;
            this.PrimaryFieldState = applePass.PrimaryFields;
            this.SecondaryFieldState = applePass.SecondaryFields;
            this.BackFieldState = applePass.BackFields;
            this.AuxiliaryFieldState = applePass.AuxiliaryFields;
            BindFields();

            this.RelevanceLocationsState = applePass.RelevantLocations;
            this.RelevanceBeaconsState = applePass.RelevantBeacons;
            BindLocations();
            BindBeacons();

            dpExpirationDate.SelectedDate = applePass.ExpirationDate;
            dpRelevantDate.SelectedDate = applePass.RelevantDate;

            if ( template.Id == 0 )
            {
                lTitle.Text = ActionTitle.Add( MinePassTemplate.FriendlyTypeName );
                template.IsActive = true;
                cbIsActive.Checked = true;

                var globalAttributes = GlobalAttributesCache.Get();
                txtOrganizationName.Text = globalAttributes.GetValue( "OrganizationName" );

                // hide the panel drawer that show created and last modified dates
                pdAuditDetails.Visible = false;
            }
            else
            {
                lTitle.Text = template.Name;
            }
        }

        /// <summary>
        /// Converts to apple pass.
        /// </summary>
        /// <param name="passData">The pass data.</param>
        /// <returns></returns>
        private MinePassApple ConvertToApplePass( string passData )
        {
            if ( passData.IsNotNullOrWhiteSpace() )
            {
                return JsonConvert.DeserializeObject<MinePassApple>( passData );
            }
            else
            {
                return new MinePassApple();
            }
        }

        #endregion
    }
}