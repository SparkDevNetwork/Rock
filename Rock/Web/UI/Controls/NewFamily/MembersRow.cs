//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;

using Rock.Model;
using Rock.Web.Cache;

namespace Rock.Web.UI.Controls
{
    /// <summary>
    /// Displays a bootstrap badge
    /// </summary>
    public class NewFamilyMembersRow : CompositeControl
    {
        /// <summary>
        /// The Family role key
        /// </summary>
        public static string FAMILY_ROLE_KEY = "NewFamilyMembersRow_FamilyRoles";

        private RockRadioButtonList _rblRole;
        private DropDownList _ddlTitle;
        private RockTextBox _tbFirstName;
        private RockTextBox _tbNickName;
        private RockTextBox _tbLastName;
        private RockRadioButtonList _rblGender;
        private DatePicker _dpBirthdate;
        private DropDownList _ddlStatus;
        private RockDropDownList _ddlGrade;

        private LinkButton _lbDelete;

        /// <summary>
        /// Gets or sets the person GUID.
        /// </summary>
        /// <value>
        /// The person GUID.
        /// </value>
        public Guid? PersonGuid
        {
            get
            {
                if ( ViewState["PersonGuid"] != null )
                {
                    return (Guid)ViewState["PersonGuid"];
                }
                else
                {
                    return Guid.Empty;
                }
            }
            set { ViewState["PersonGuid"] = value; }
        }

        /// <summary>
        /// Gets or sets the role id.
        /// </summary>
        /// <value>
        /// The role id.
        /// </value>
        public int? RoleId
        {
            get { return _rblRole.SelectedValueAsInt(); }
            set { SetListValue( _rblRole, value ); }
        }

        /// <summary>
        /// Gets or sets the title value id.
        /// </summary>
        /// <value>
        /// The title value id.
        /// </value>
        public int? TitleValueId
        {
            get { return _ddlTitle.SelectedValueAsInt(); }
            set { SetListValue( _ddlTitle, value ); }
        }

        /// <summary>
        /// Gets or sets the first name.
        /// </summary>
        /// <value>
        /// The first name.
        /// </value>
        public string FirstName
        {
            get { return _tbFirstName.Text; }
            set { _tbFirstName.Text = value; }
        }

        /// <summary>
        /// Gets or sets the name of the nick.
        /// </summary>
        /// <value>
        /// The name of the nick.
        /// </value>
        public string NickName
        {
            get { return _tbNickName.Text; }
            set { _tbNickName.Text = value; }
        }

        /// <summary>
        /// Gets or sets a value indicating whether [show nick name].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [show nick name]; otherwise, <c>false</c>.
        /// </value>
        internal bool ShowNickName
        {
            get { return ViewState["ShowNickName"] as bool? ?? false; }
            set { ViewState["ShowNickName"] = value; }
        }

        /// <summary>
        /// Gets or sets the last name.
        /// </summary>
        /// <value>
        /// The last name.
        /// </value>
        public string LastName
        {
            get { return _tbLastName.Text; }
            set { _tbLastName.Text = value; }
        }

        /// <summary>
        /// Gets or sets the birth date.
        /// </summary>
        /// <value>
        /// The birth date.
        /// </value>
        public DateTime? BirthDate
        {
            get { return _dpBirthdate.SelectedDate; }
            set { _dpBirthdate.SelectedDate = value; }
        }

        /// <summary>
        /// Gets or sets the gender.
        /// </summary>
        /// <value>
        /// The gender.
        /// </value>
        public Gender Gender
        {
            get 
            {
                if ( string.IsNullOrWhiteSpace( _rblGender.SelectedValue ) )
                {
                    return Gender.Unknown;
                }
                return _rblGender.SelectedValueAsEnum<Gender>(); 
            }

            set 
            {
                string selectedValue = value.ConvertToInt().ToString();
                if ( selectedValue == "0" )
                {
                    selectedValue = string.Empty;
                }
                SetListValue( _rblGender, selectedValue );
            }
        }

        /// <summary>
        /// Gets or sets the status value id.
        /// </summary>
        /// <value>
        /// The status value id.
        /// </value>
        public int? StatusValueId
        {
            get { return _ddlStatus.SelectedValueAsInt(); }
            set { SetListValue( _ddlStatus, value ); }
        }

        /// <summary>
        /// Gets or sets the grade.
        /// </summary>
        /// <value>
        /// The grade.
        /// </value>
        public int? Grade
        {
            get { return _ddlGrade.SelectedValueAsInt(); }
            set { SetListValue( _ddlGrade, value ); }
        }

        /// <summary>
        /// Gets the family roles.
        /// </summary>
        /// <value>
        /// The family roles.
        /// </value>
        public Dictionary<int, string> FamilyRoles
        {
            get
            {
                if (HttpContext.Current.Items.Contains(FAMILY_ROLE_KEY))
                {
                    return HttpContext.Current.Items[FAMILY_ROLE_KEY] as Dictionary<int, string>;
                }
                else
                {
                    var familyRoles = new Dictionary<int, string>();
                    var familyGroupType = GroupTypeCache.Read( Rock.SystemGuid.GroupType.GROUPTYPE_FAMILY.AsGuid() );
                    if (familyGroupType != null)
                    {
                        new GroupTypeRoleService().GetByGroupTypeId(familyGroupType.Id)
                            .ToList().ForEach( r => familyRoles.Add(r.Id, r.Name));
                    }
                    HttpContext.Current.Items[FAMILY_ROLE_KEY] = familyRoles;
                    return familyRoles;
                }
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether [require gender].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [require gender]; otherwise, <c>false</c>.
        /// </value>
        public bool RequireGender
        {
            get 
            {
                return _rblGender.Required;
            }
            set 
            {
                _rblGender.Required = value;
                BindGender();
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether [show grade].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [show grade]; otherwise, <c>false</c>.
        /// </value>
        public bool ShowGrade
        {
            get { return ViewState["ShowGrade"] as bool? ?? false; }
            set { ViewState["ShowGrade"] = value; }
        }

        /// <summary>
        /// Gets or sets a value indicating whether [require grade].
        /// </summary>
        /// <value>
        /// <c>true</c> if [require grade]; otherwise, <c>false</c>.
        /// </value>
        public bool RequireGrade
        {
            get 
            {
                EnsureChildControls();
                return _ddlGrade.Required;
            }
            set
            {
                EnsureChildControls();
                _ddlGrade.Required = value;
            }
        }

        public string ValidationGroup
        {
            get
            {
                return _tbFirstName.ValidationGroup;
            }
            set
            {
                EnsureChildControls();
                _rblRole.ValidationGroup = value;
                _tbFirstName.ValidationGroup = value;
                _tbNickName.ValidationGroup = value;
                _tbLastName.ValidationGroup = value;
                _rblGender.ValidationGroup = value;
                _dpBirthdate.ValidationGroup = value;
                _ddlGrade.ValidationGroup = value;
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="NewFamilyMembersRow" /> class.
        /// </summary>
        public NewFamilyMembersRow()
            : base()
        {
            _rblRole = new RockRadioButtonList();
            _ddlTitle = new DropDownList();
            _tbFirstName = new RockTextBox();
            _tbNickName = new RockTextBox();
            _tbLastName = new RockTextBox();
            _rblGender = new RockRadioButtonList();
            _dpBirthdate = new DatePicker();
            _ddlStatus = new DropDownList();
            _ddlGrade = new RockDropDownList();
            _lbDelete = new LinkButton();
        }

        /// <summary>
        /// Called by the ASP.NET page framework to notify server controls that use composition-based implementation to create any child controls they contain in preparation for posting back or rendering.
        /// </summary>
        protected override void CreateChildControls()
        {
            base.CreateChildControls();
            Controls.Clear();

            _rblRole.ID = "_rblRole";
            _ddlTitle.ID = "_ddlTitle";
            _tbFirstName.ID = "_tbFirstName";
            _tbNickName.ID = "_tbNickName";
            _tbLastName.ID = "_tbLastName";
            _rblGender.ID = "_rblGender";
            _dpBirthdate.ID = "_dtBirthdate";
            _ddlStatus.ID = "_ddlStatus";
            _ddlGrade.ID = "_ddlGrade";
            _lbDelete.ID = "_lbDelete";

            Controls.Add( _rblRole );
            Controls.Add( _ddlTitle );
            Controls.Add( _tbFirstName );
            Controls.Add( _tbNickName );
            Controls.Add( _tbLastName );
            Controls.Add( _rblGender );
            Controls.Add( _dpBirthdate );
            Controls.Add( _ddlStatus );
            Controls.Add( _ddlGrade );
            Controls.Add( _lbDelete );

            _rblRole.RepeatDirection = RepeatDirection.Vertical;
            _rblRole.AutoPostBack = true;
            _rblRole.SelectedIndexChanged += rblRole_SelectedIndexChanged;
            _rblRole.Required = true;
            _rblRole.RequiredErrorMessage = "Role is required for all members";
            _rblRole.DataTextField = "Value";
            _rblRole.DataValueField = "Key";
            _rblRole.DataSource = FamilyRoles;
            _rblRole.DataBind();

            _ddlTitle.CssClass = "form-control";
            BindListToDefinedType( _ddlTitle, Rock.SystemGuid.DefinedType.PERSON_TITLE, true );

            _tbFirstName.CssClass = "form-control";
            _tbFirstName.Required = true;
            _tbFirstName.RequiredErrorMessage = "First Name is required for all family members";

            _tbNickName.CssClass = "form-control";

            _tbLastName.CssClass = "form-control";
            _tbLastName.Required = true;
            _tbLastName.RequiredErrorMessage = "Last Name is required for all family members";

            _rblGender.RepeatDirection = RepeatDirection.Vertical;
            _rblGender.RequiredErrorMessage = "Gender is required for all family members";
            BindGender();

            _ddlStatus.CssClass = "form-control";
            BindListToDefinedType( _ddlStatus, Rock.SystemGuid.DefinedType.PERSON_STATUS );

            _dpBirthdate.Required = false;

            _ddlGrade.CssClass = "form-control";
            _ddlGrade.RequiredErrorMessage = "Grade is required for all children";
            _ddlGrade.Items.Clear();
            _ddlGrade.Items.Add( new ListItem( "", "" ) );
            _ddlGrade.Items.Add( new ListItem( "K", "0" ) );
            _ddlGrade.Items.Add( new ListItem( "1st", "1" ) );
            _ddlGrade.Items.Add( new ListItem( "2nd", "2" ) );
            _ddlGrade.Items.Add( new ListItem( "3rd", "3" ) );
            _ddlGrade.Items.Add( new ListItem( "4th", "4" ) );
            _ddlGrade.Items.Add( new ListItem( "5th", "5" ) );
            _ddlGrade.Items.Add( new ListItem( "6th", "6" ) );
            _ddlGrade.Items.Add( new ListItem( "7th", "7" ) );
            _ddlGrade.Items.Add( new ListItem( "8th", "8" ) );
            _ddlGrade.Items.Add( new ListItem( "9th", "9" ) );
            _ddlGrade.Items.Add( new ListItem( "10th", "10" ) );
            _ddlGrade.Items.Add( new ListItem( "11th", "11" ) );
            _ddlGrade.Items.Add( new ListItem( "12th", "12" ) );

            var iDelete = new HtmlGenericControl( "i" );
            _lbDelete.Controls.Add( iDelete );
            iDelete.AddCssClass( "fa fa-times" );

            _lbDelete.CssClass = "btn btn-xs btn-danger";
            _lbDelete.Click += lbDelete_Click;
            _lbDelete.CausesValidation = false;
        }

        /// <summary>
        /// Outputs server control content to a provided <see cref="T:System.Web.UI.HtmlTextWriter" /> object and stores tracing information about the control if tracing is enabled.
        /// </summary>
        /// <param name="writer">The <see cref="T:System.Web.UI.HtmlTextWriter" /> object that receives the control content.</param>
        public override void RenderControl( HtmlTextWriter writer )
        {
            if ( this.Visible )
            {
                writer.AddAttribute( "rowid", ID );
                writer.RenderBeginTag( HtmlTextWriterTag.Tr );

                writer.RenderBeginTag( HtmlTextWriterTag.Td );
                writer.AddAttribute( HtmlTextWriterAttribute.Class, "form-group" + ( _rblRole.IsValid ? "" : " has-error" ) );
                writer.RenderBeginTag( HtmlTextWriterTag.Div );
                _rblRole.RenderControl( writer );
                writer.RenderEndTag();
                writer.RenderEndTag();

                writer.RenderBeginTag( HtmlTextWriterTag.Td );
                _ddlTitle.RenderControl( writer );
                writer.RenderEndTag();

                writer.RenderBeginTag( HtmlTextWriterTag.Td );
                writer.AddAttribute( HtmlTextWriterAttribute.Class, "form-group" + ( _tbFirstName.IsValid ? "" : " has-error" ) );
                writer.RenderBeginTag( HtmlTextWriterTag.Div );
                _tbFirstName.RenderControl( writer );
                writer.RenderEndTag();
                writer.RenderEndTag();

                if ( ShowNickName )
                {
                    writer.RenderBeginTag( HtmlTextWriterTag.Td );
                    _tbNickName.RenderControl( writer );
                    writer.RenderEndTag();
                }

                writer.RenderBeginTag( HtmlTextWriterTag.Td );
                writer.AddAttribute( HtmlTextWriterAttribute.Class, "form-group" + ( _tbLastName.IsValid ? "" : " has-error" ) );
                writer.RenderBeginTag( HtmlTextWriterTag.Div );
                _tbLastName.RenderControl( writer );
                writer.RenderEndTag();
                writer.RenderEndTag();

                writer.RenderBeginTag( HtmlTextWriterTag.Td );
                writer.AddAttribute( HtmlTextWriterAttribute.Class, "form-group" + ( _rblGender.IsValid ? "" : " has-error" ) );
                writer.RenderBeginTag( HtmlTextWriterTag.Div );
                _rblGender.RenderControl( writer );
                writer.RenderEndTag();
                writer.RenderEndTag();

                writer.RenderBeginTag( HtmlTextWriterTag.Td );
                _dpBirthdate.RenderControl( writer );
                writer.RenderEndTag();

                writer.RenderBeginTag( HtmlTextWriterTag.Td );
                _ddlStatus.RenderControl( writer );
                writer.RenderEndTag();

                writer.RenderBeginTag( HtmlTextWriterTag.Td );
                if ( ShowGrade )
                {
                    _ddlGrade.RenderControl( writer );
                }
                writer.RenderEndTag();

                writer.RenderBeginTag( HtmlTextWriterTag.Td );
                _lbDelete.RenderControl( writer );
                writer.RenderEndTag();

                writer.RenderEndTag();
            }
        }

        /// <summary>
        /// Binds the gender.
        /// </summary>
        private void BindGender()
        {
            string selectedValue = _rblGender.SelectedValue;

            _rblGender.Items.Clear();
            _rblGender.Items.Add( new ListItem( "M", "Male" ) );
            _rblGender.Items.Add( new ListItem( "F", "Female" ) );
            if ( !RequireGender )
            {
                _rblGender.Items.Add( new ListItem( "Unknown", "" ) );
            }

            _rblGender.SelectedValue = selectedValue;
        }

        /// <summary>
        /// Binds the type of the list to defined.
        /// </summary>
        /// <param name="listControl">The list control.</param>
        /// <param name="definedTypeGuid">The defined type GUID.</param>
        /// <param name="insertBlankOption">if set to <c>true</c> [insert blank option].</param>
        protected void BindListToDefinedType( ListControl listControl, string definedTypeGuid, bool insertBlankOption = false )
        {
            var definedType = DefinedTypeCache.Read( new Guid( definedTypeGuid ) );
            listControl.BindToDefinedType( definedType, insertBlankOption );
        }

        /// <summary>
        /// Sets the list value.
        /// </summary>
        /// <param name="listControl">The list control.</param>
        /// <param name="value">The value.</param>
        private void SetListValue(ListControl listControl, int? value)
        {
            foreach(ListItem item in listControl.Items)
            {
                item.Selected = (value.HasValue && item.Value == value.Value.ToString());
            }
        }

        /// <summary>
        /// Sets the list value.
        /// </summary>
        /// <param name="listControl">The list control.</param>
        /// <param name="value">The value.</param>
        private void SetListValue( ListControl listControl, string value )
        {
            foreach ( ListItem item in listControl.Items )
            {
                item.Selected = ( item.Value == value );
            }
        }

        /// <summary>
        /// Handles the SelectedIndexChanged event of the rblRole control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        void rblRole_SelectedIndexChanged( object sender, EventArgs e )
        {
            if ( RoleUpdated != null )
            {
                RoleUpdated( this, e );
            }
        }


        /// <summary>
        /// Handles the Click event of the lbDelete control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        void lbDelete_Click( object sender, EventArgs e )
        {
            if ( DeleteClick != null )
            {
                DeleteClick( this, e );
            }
        }

        /// <summary>
        /// Occurs when [role updated].
        /// </summary>
        public event EventHandler RoleUpdated;

        /// <summary>
        /// Occurs when [delete click].
        /// </summary>
        public event EventHandler DeleteClick;

    }

}