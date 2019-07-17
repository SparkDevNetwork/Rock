using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web.UI;
using System.Web.UI.WebControls;

using Newtonsoft.Json;

using Rock;
using Rock.Attribute;
using Rock.Blocks.Types.Mobile;
using Rock.Data;
using Rock.Model;
using Rock.Web;
using Rock.Web.Cache;

namespace RockWeb.Blocks.Mobile
{
    public partial class ContentChannelListSettings : System.Web.UI.UserControl, IRockCustomSettingsUserControl
    {
        #region Private members
        List<PropertyInfoSummary>  _contentChannelProperties = null;
        int _contentChannelId = 0;
        #endregion


        #region Properties
        private List<FieldSetting> FieldSettings
        {
            get
            {
                if ( _fieldSettings == null )
                {
                    return new List<FieldSetting>();
                }

                return _fieldSettings;
            }
            set
            {
                _fieldSettings = value;
            }
        }
        private List<FieldSetting> _fieldSettings = new List<FieldSetting>();
        #endregion


        #region Events
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );

            gIncludedAttributes.DataKeyNames = new string[] { "Key" };
            gIncludedAttributes.Actions.AddClick += gIncludedAttributes_AddClick;
            gIncludedAttributes.GridRebind += gIncludedAttributes_GridRebind;
            gIncludedAttributes.ShowHeaderWhenEmpty = true;
            gIncludedAttributes.Actions.ShowAdd = true;
        }

        protected void Page_Load( object sender, EventArgs e )
        {
            if ( !Page.IsPostBack )
            {
                _contentChannelProperties = new List<PropertyInfoSummary>();

                var contentChannelProperties = typeof( ContentChannelItem ).GetProperties().OrderBy( p => p.Name );
                foreach( var property in contentChannelProperties )
                {
                    _contentChannelProperties.Add( new PropertyInfoSummary { Name = property.Name, Type = property.PropertyType.ToString() } );
                }

                LoadForm();
            }

            //base.OnLoad( e );
        }

        protected override void LoadViewState( object savedState )
        {
            base.LoadViewState( savedState );

            var fieldSettingJson = ViewState["FieldSettings"].ToString();

            if ( fieldSettingJson.IsNotNullOrWhiteSpace() )
            {
                _fieldSettings = JsonConvert.DeserializeObject<List<FieldSetting>>( fieldSettingJson );
            }


            var propertyInfoJson = ViewState["PropertyInfo"].ToString();

            if ( propertyInfoJson.IsNotNullOrWhiteSpace() )
            {
                _contentChannelProperties = JsonConvert.DeserializeObject<List<PropertyInfoSummary>>( propertyInfoJson );
            }
        }

        protected override object SaveViewState()
        {
            ViewState["FieldSettings"] = this.FieldSettings.ToJson();
            ViewState["PropertyInfo"] = _contentChannelProperties.ToJson();
            return base.SaveViewState();
        }
        #endregion

        private void LoadForm()
        {
            rblFieldSource.BindToEnum<FieldSource>();
            rblAttributeFormatType.BindToEnum<AttributeFormat>();
            rblFieldFormat.BindToEnum<FieldFormat>();

            var rockContext = new RockContext();

            var contentChannels = new ContentChannelService( rockContext ).Queryable().AsNoTracking()
                                    .Select( c => new
                                        {
                                            c.Id,
                                            Value = c.Name
                                        } )
                                    .OrderBy( c => c.Value )
                                    .ToList();

            ddlContentChannel.DataSource = contentChannels;
            ddlContentChannel.DataTextField = "Value";
            ddlContentChannel.DataValueField = "Id";
            ddlContentChannel.DataBind();

            ddlContentChannelProperties.DataSource = _contentChannelProperties;
            ddlContentChannelProperties.DataValueField = "Name";
            ddlContentChannelProperties.DataTextField = "Name";
            ddlContentChannelProperties.DataBind();

            ddlContentChannel.SelectedValue = _contentChannelId.ToString();

            LoadAttributeListForContentChannel();

            BindGrid();
        }
        

        #region Grid Events
        private void BindGrid()
        {
            gIncludedAttributes.DataSource = this.FieldSettings;
            gIncludedAttributes.DataBind();
        }

        private void gIncludedAttributes_GridRebind( object sender, Rock.Web.UI.Controls.GridRebindEventArgs e )
        {
            BindGrid();
        }

        private void gIncludedAttributes_AddClick( object sender, EventArgs e )
        {
            pnlDataEdit.Visible = true;
            gIncludedAttributes.Visible = false;

            // Clear fields
            rblFieldSource.SetValue( FieldSource.Property.ConvertToInt().ToString() );
            rblAttributeFormatType.SetValue( AttributeFormat.FriendlyValue.ConvertToInt().ToString() );
            tbKey.Text = string.Empty;
            hfOriginalKey.Value = "new_key";
            rblAttributeFormatType.SetValue( AttributeFormat.FriendlyValue.ConvertToInt().ToString() );
            rblFieldFormat.SetValue( FieldFormat.String.ConvertToInt().ToString() );
            ceLavaExpression.Text = "{{ item | Attribute:'AttributeKey' }}";

            SetPropertyTypePanels();
        }

        protected void gIncludedAttributesEdit_Click( object sender, Rock.Web.UI.Controls.RowEventArgs e )
        {
            var settingKey = e.RowKeyValue.ToString();

            var setting = this.FieldSettings.Where( s => s.Key == settingKey ).FirstOrDefault();

            if ( setting == null )
            {
                return;
            }

            pnlDataEdit.Visible = true;
            gIncludedAttributes.Visible = false;

            // Set edit values
            rblFieldSource.SetValue( setting.FieldSource.ConvertToInt().ToString() );
            tbKey.Text = setting.Key;
            hfOriginalKey.Value = setting.Key;
            ceLavaExpression.Text = setting.Value;


            if ( setting.FieldSource == FieldSource.Property )
            {
                ddlContentChannelProperties.SelectedValue = setting.FieldName;
            }
            else if (setting.FieldSource == FieldSource.Attribute )
            {
                rblAttributeFormatType.SetValue( setting.AttributeFormat.ConvertToInt().ToString() );
            }
            else
            {
                ceLavaExpression.Text = setting.Value;
                rblAttributeFormatType.SetValue( setting.AttributeFormat.ConvertToInt().ToString() );
                rblFieldFormat.SetValue( setting.FieldFormat.ConvertToInt().ToString() );
            }

            SetPropertyTypePanels();
        }

        protected void gIncludedAttributesDelete_Click( object sender, Rock.Web.UI.Controls.RowEventArgs e )
        {
            var settingKey = e.RowKeyValue.ToString();

            var setting = this.FieldSettings.Where( s => s.Key == settingKey ).FirstOrDefault();

            _fieldSettings.Remove( setting );

            BindGrid();
        }

        protected void lbApply_Click( object sender, EventArgs e )
        {
            pnlDataEdit.Visible = false;
            gIncludedAttributes.Visible = true;

            var settings = this.FieldSettings;

            var setting = settings.Where( s => s.Key == hfOriginalKey.Value ).FirstOrDefault();


            if ( setting == null )
            {
                setting = new FieldSetting();
                settings.Add( setting );
            }

            setting.Key = tbKey.Text;
            setting.FieldSource = rblFieldSource.SelectedValueAsEnum<FieldSource>();

            if ( setting.FieldSource == FieldSource.Property )
            {
                var propertyName = ddlContentChannelProperties.SelectedValue;

                setting.Value = string.Format( "{{{{ item.{0} }}}}", propertyName );
                setting.FieldName = propertyName;
                setting.Key = propertyName;

                var property = _contentChannelProperties.Where( p => p.Name == propertyName).FirstOrDefault();

                if ( property.Type.Contains( ".Int") )
                {
                    setting.FieldFormat = FieldFormat.Number;
                }
                else if (property.Type.Contains( "DateTime" ) )
                {
                    setting.FieldFormat = FieldFormat.Date;
                }
                else
                {
                    setting.FieldFormat = FieldFormat.String;
                }
            }
            else if ( setting.FieldSource == FieldSource.Attribute )
            {
                var attributeKey = ddlContentChannelAttributes.SelectedValue;
                var attributeFormatType = rblAttributeFormatType.SelectedValueAsEnum<AttributeFormat>();

                setting.Key = attributeKey;

                if ( attributeFormatType == AttributeFormat.FriendlyValue )
                {
                    setting.Value = string.Format( "{{{{ item | Attribute:'{0}' }}}}", attributeKey );
                }
                else
                {
                    setting.Value = string.Format( "{{{{ item | Attribute:'{0}','RawValue' }}}}", attributeKey );
                }

                setting.FieldFormat = FieldFormat.String;
                setting.FieldName = attributeKey;
                setting.AttributeFormat = attributeFormatType;
            }
            else
            {
                setting.FieldFormat = rblFieldFormat.SelectedValueAsEnum<FieldFormat>();
                setting.FieldName = string.Empty;
                setting.Value = ceLavaExpression.Text;
            }

            this.FieldSettings = settings;

            BindGrid();
        }

        protected void lbCancel_Click( object sender, EventArgs e )
        {
            pnlDataEdit.Visible = false;
            gIncludedAttributes.Visible = true;
        }

        protected void rblFieldSource_SelectedIndexChanged( object sender, EventArgs e )
        {
            SetPropertyTypePanels();
        }

        protected void ddlContentChannel_SelectedIndexChanged( object sender, EventArgs e )
        {
            LoadAttributeListForContentChannel();
        }

        #endregion


        #region Private Methods

        private void LoadAttributeListForContentChannel()
        {
            var contentChannelId = ddlContentChannel.SelectedValue;
            var contentChannelEntityTypeId = EntityTypeCache.Get( Rock.SystemGuid.EntityType.CONTENT_CHANNEL_ITEM ).Id;

            var attributes = AttributeCache.All()
                                .Where( a => a.EntityTypeId == contentChannelEntityTypeId
                                        && a.EntityTypeQualifierColumn == "ContentChannelId"
                                        && a.EntityTypeQualifierValue == contentChannelId )
                                .Select( a => new {
                                                    a.Key,
                                                    a.Name } )
                                .ToList();

            ddlContentChannelAttributes.DataSource = attributes;
            ddlContentChannelAttributes.DataValueField = "Key";
            ddlContentChannelAttributes.DataTextField = "Name";
            ddlContentChannelAttributes.DataBind();
        }

        private void SetPropertyTypePanels()
        {
            var selectedItem = rblFieldSource.SelectedValueAsEnum<FieldSource>();

            pnlAttributes.Visible = false;
            pnlProperties.Visible = false;
            pnlLavaExpression.Visible = false;

            if ( selectedItem == FieldSource.Property )
            {
                pnlProperties.Visible = true;
            }
            else if ( selectedItem == FieldSource.Attribute )
            {
                pnlAttributes.Visible = true;
            }
            else
            {
                pnlLavaExpression.Visible = true;
            }
        }

        #endregion

        #region IRockCustomSettingsUserControl implementation

        public void ReadSettingsFromEntity( IHasAttributes attributeEntity )
        {
            var fieldSettings = attributeEntity.GetAttributeValue( Rock.Blocks.Types.Mobile.ContentChannelItemList.AttributeKeys.FieldSettings );

            if ( fieldSettings.IsNotNullOrWhiteSpace() )
            {
                this.FieldSettings = JsonConvert.DeserializeObject<List<FieldSetting>>( fieldSettings );
            }

            _contentChannelId = attributeEntity.GetAttributeValue( Rock.Blocks.Types.Mobile.ContentChannelItemList.AttributeKeys.ContentChannel ).AsInteger();
            nbPageSize.Text = attributeEntity.GetAttributeValue( Rock.Blocks.Types.Mobile.ContentChannelItemList.AttributeKeys.PageSize );
            cbIncludeFollowing.Checked = attributeEntity.GetAttributeValue( Rock.Blocks.Types.Mobile.ContentChannelItemList.AttributeKeys.IncludeFollowing ).AsBoolean();
            var pageCache = PageCache.Get( attributeEntity.GetAttributeValue( Rock.Blocks.Types.Mobile.ContentChannelItemList.AttributeKeys.DetailPage ).AsGuid() );
            ppDetailPage.SetValue( pageCache != null ? ( int? ) pageCache.Id : null );
        }

        public void WriteSettingsToEntity( IHasAttributes attributeEntity )
        {
            attributeEntity.SetAttributeValue( Rock.Blocks.Types.Mobile.ContentChannelItemList.AttributeKeys.FieldSettings, this.FieldSettings.ToJson() );
            attributeEntity.SetAttributeValue( Rock.Blocks.Types.Mobile.ContentChannelItemList.AttributeKeys.ContentChannel, ddlContentChannel.SelectedValue );
            attributeEntity.SetAttributeValue( Rock.Blocks.Types.Mobile.ContentChannelItemList.AttributeKeys.PageSize, nbPageSize.Text );
            attributeEntity.SetAttributeValue( Rock.Blocks.Types.Mobile.ContentChannelItemList.AttributeKeys.IncludeFollowing, cbIncludeFollowing.Checked.ToString() );

            string detailPage = string.Empty;
            if ( ppDetailPage.SelectedValueAsId().HasValue )
            {
                detailPage = PageCache.Get( ppDetailPage.SelectedValueAsId().Value ).Guid.ToString();
            }
            attributeEntity.SetAttributeValue( Rock.Blocks.Types.Mobile.ContentChannelItemList.AttributeKeys.DetailPage, detailPage );
        }

        #endregion
    }

    /// <summary>
    /// POCO to model info about the properties of the content channel item model
    /// </summary>
    public class PropertyInfoSummary
    {
        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        /// <value>
        /// The name.
        /// </value>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the type.
        /// </summary>
        /// <value>
        /// The type.
        /// </value>
        public string Type { get; set; }
    }
}