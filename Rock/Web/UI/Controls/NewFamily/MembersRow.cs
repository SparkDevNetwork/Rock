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
        public static string FAMILY_ROLE_KEY = "NewFamilyMembersRow_FamilyRoles";

        RadioButtonList rblRole;
        RequiredFieldValidator rfvRole;
        DropDownList ddlTitle;
        TextBox tbFirstName;
        RequiredFieldValidator rfvFirstName;
        TextBox tbNickName;
        TextBox tbLastName;
        RequiredFieldValidator rfvLastName;
        RadioButtonList rblGender;
        RequiredFieldValidator rfvGender;
        DatePicker dpBirthdate;
        DropDownList ddlStatus;
        DropDownList ddlGrade;

        LinkButton lbDelete;

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
            get { return rblRole.SelectedValueAsInt(); }
            set { SetListValue( rblRole, value ); }
        }

        /// <summary>
        /// Gets or sets the title value id.
        /// </summary>
        /// <value>
        /// The title value id.
        /// </value>
        public int? TitleValueId
        {
            get { return ddlTitle.SelectedValueAsInt(); }
            set { SetListValue( ddlTitle, value ); }
        }

        /// <summary>
        /// Gets or sets the first name.
        /// </summary>
        /// <value>
        /// The first name.
        /// </value>
        public string FirstName
        {
            get { return tbFirstName.Text; }
            set { tbFirstName.Text = value; }
        }

        /// <summary>
        /// Gets or sets the name of the nick.
        /// </summary>
        /// <value>
        /// The name of the nick.
        /// </value>
        public string NickName
        {
            get { return tbNickName.Text; }
            set { tbNickName.Text = value; }
        }

        /// <summary>
        /// Gets or sets the last name.
        /// </summary>
        /// <value>
        /// The last name.
        /// </value>
        public string LastName
        {
            get { return tbLastName.Text; }
            set { tbLastName.Text = value; }
        }

        /// <summary>
        /// Gets or sets the birth date.
        /// </summary>
        /// <value>
        /// The birth date.
        /// </value>
        public DateTime? BirthDate
        {
            get { return dpBirthdate.SelectedDate; }
            set { dpBirthdate.SelectedDate = value; }
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
                if ( string.IsNullOrWhiteSpace( rblGender.SelectedValue ) )
                {
                    return Gender.Unknown;
                }
                return rblGender.SelectedValueAsEnum<Gender>(); 
            }

            set 
            {
                string selectedValue = value.ConvertToInt().ToString();
                if ( selectedValue == "0" )
                {
                    selectedValue = string.Empty;
                }
                SetListValue( rblGender, selectedValue );
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
            get { return ddlStatus.SelectedValueAsInt(); }
            set { SetListValue( ddlStatus, value ); }
        }

        /// <summary>
        /// Gets or sets the grade.
        /// </summary>
        /// <value>
        /// The grade.
        /// </value>
        public int? Grade
        {
            get { return ddlGrade.SelectedValueAsInt(); }
            set { SetListValue( ddlGrade, value ); }
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
                    new GroupTypeService()
                        .Get( new Guid( Rock.SystemGuid.GroupType.GROUPTYPE_FAMILY ) )
                        .Roles.ToList().ForEach( r => familyRoles.Add(r.Id, r.Name));
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
            get { return ViewState["RequireGender"] as bool? ?? false; }
            set 
            { 
                ViewState["RequireGender"] = value;
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
            get { return ViewState["RequireGrade"] as bool? ?? false; }
            set { ViewState["RequireGrade"] = value; }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="NewFamilyMembersRow" /> class.
        /// </summary>
        public NewFamilyMembersRow()
            : base()
        {
            rblRole = new RadioButtonList();
            rfvRole = new RequiredFieldValidator();
            ddlTitle = new DropDownList();
            tbFirstName = new TextBox();
            rfvFirstName = new RequiredFieldValidator();
            tbNickName = new TextBox();
            tbLastName = new TextBox();
            rfvLastName = new RequiredFieldValidator();
            rblGender = new RadioButtonList();
            rfvGender = new RequiredFieldValidator();
            dpBirthdate = new DatePicker();
            ddlStatus = new DropDownList();
            ddlGrade = new DropDownList();
            lbDelete = new LinkButton();

            rblRole.DataTextField = "Value";
            rblRole.DataValueField = "Key";
            rblRole.DataSource = FamilyRoles;
            rblRole.DataBind();

            rfvRole = new RequiredFieldValidator();
            rfvRole.Display = ValidatorDisplay.Dynamic;
            rfvRole.ErrorMessage = "Role is required for all members";
            rfvRole.CssClass = "validation-error help-inline";
            rfvRole.Enabled = true;

            BindListToDefinedType( ddlTitle, Rock.SystemGuid.DefinedType.PERSON_TITLE, true );

            rfvFirstName = new RequiredFieldValidator();
            rfvFirstName.Display = ValidatorDisplay.Dynamic;
            rfvFirstName.ErrorMessage = "First Name is required for all family members";
            rfvFirstName.CssClass = "validation-error help-inline";
            rfvFirstName.Enabled = true;

            rfvLastName = new RequiredFieldValidator();
            rfvLastName.Display = ValidatorDisplay.Dynamic;
            rfvLastName.ErrorMessage = "Last Name is required for all family members";
            rfvLastName.CssClass = "validation-error help-inline";
            rfvLastName.Enabled = true;

            BindGender();

            rfvGender = new RequiredFieldValidator();
            rfvGender.Display = ValidatorDisplay.Dynamic;
            rfvGender.ErrorMessage = "Gender is required for all family members";
            rfvGender.CssClass = "validation-error help-inline";

            BindListToDefinedType( ddlStatus, Rock.SystemGuid.DefinedType.PERSON_STATUS );

            ddlGrade.Items.Clear();
            ddlGrade.Items.Add( new ListItem( "", "" ) );
            ddlGrade.Items.Add( new ListItem( "K", "0" ) );
            ddlGrade.Items.Add( new ListItem( "1st", "1" ) );
            ddlGrade.Items.Add( new ListItem( "2nd", "2" ) );
            ddlGrade.Items.Add( new ListItem( "3rd", "3" ) );
            ddlGrade.Items.Add( new ListItem( "4th", "4" ) );
            ddlGrade.Items.Add( new ListItem( "5th", "5" ) );
            ddlGrade.Items.Add( new ListItem( "6th", "6" ) );
            ddlGrade.Items.Add( new ListItem( "7th", "7" ) );
            ddlGrade.Items.Add( new ListItem( "8th", "8" ) );
            ddlGrade.Items.Add( new ListItem( "9th", "9" ) );
            ddlGrade.Items.Add( new ListItem( "10th", "10" ) );
            ddlGrade.Items.Add( new ListItem( "11th", "11" ) );
            ddlGrade.Items.Add( new ListItem( "12th", "12" ) );

        }

        /// <summary>
        /// Called by the ASP.NET page framework to notify server controls that use composition-based implementation to create any child controls they contain in preparation for posting back or rendering.
        /// </summary>
        protected override void CreateChildControls()
        {
            base.CreateChildControls();
            Controls.Clear();

            rblRole.ID = this.ID + "_rblRole";
            rfvRole.ID = this.ID + "_rfvRole";
            ddlTitle.ID = this.ID + "_ddlTitle";
            tbFirstName.ID = this.ID + "_tbFirstName";
            rfvFirstName.ID = this.ID + "_rfvFirstName";
            tbNickName.ID = this.ID + "_tbNickName";
            tbLastName.ID = this.ID + "_tbLastName";
            rfvLastName.ID = this.ID + "_rfvLastName";
            rblGender.ID = this.ID + "_rblGender";
            rfvGender.ID = this.ID + "rfvGender";
            dpBirthdate.ID = this.ID + "_dtBirthdate";
            ddlStatus.ID = this.ID + "_ddlStatus";
            ddlGrade.ID = this.ID + "_ddlGrade";
            lbDelete.ID = this.ID + "_lbDelete";

            Controls.Add( rblRole );
            Controls.Add( rfvRole );
            Controls.Add( ddlTitle );
            Controls.Add( tbFirstName );
            Controls.Add( rfvFirstName );
            Controls.Add( tbNickName );
            Controls.Add( tbLastName );
            Controls.Add( rfvLastName );
            Controls.Add( rblGender );
            Controls.Add( rfvGender );
            Controls.Add( dpBirthdate );
            Controls.Add( ddlStatus );
            Controls.Add( ddlGrade );
            Controls.Add( lbDelete );

            rblRole.RepeatDirection = RepeatDirection.Vertical;
            rblRole.AutoPostBack = true;
            rblRole.SelectedIndexChanged += rblRole_SelectedIndexChanged;
            rfvRole.ControlToValidate = rblRole.ID;

            ddlTitle.CssClass = "form-control";

            tbFirstName.CssClass = "form-control";
            rfvFirstName.ControlToValidate = tbFirstName.ID;

            tbNickName.CssClass = "form-control";

            tbLastName.CssClass = "form-control";
            rfvLastName.ControlToValidate = tbLastName.ID;

            rblGender.RepeatDirection = RepeatDirection.Vertical;
            rfvGender.ControlToValidate = rblGender.ID;

            ddlStatus.CssClass = "form-control";

            ddlGrade.CssClass = "form-control";

            lbDelete.CssClass = "btn btn-xs btn-danger";
            lbDelete.Click += lbDelete_Click;
            lbDelete.CausesValidation = false;

            var iDelete = new HtmlGenericControl( "i" );
            lbDelete.Controls.Add( iDelete );
            iDelete.AddCssClass( "icon-remove" );
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
                writer.AddAttribute( HtmlTextWriterAttribute.Class, "form-group" + ( rfvRole.IsValid ? "" : " error" ) );
                writer.RenderBeginTag( HtmlTextWriterTag.Div );
                rblRole.RenderControl( writer );
                rfvRole.RenderControl( writer );
                writer.RenderEndTag();
                writer.RenderEndTag();

                writer.RenderBeginTag( HtmlTextWriterTag.Td );
                ddlTitle.RenderControl( writer );
                writer.RenderEndTag();

                writer.RenderBeginTag( HtmlTextWriterTag.Td );
                writer.AddAttribute( HtmlTextWriterAttribute.Class, "form-group" + ( rfvFirstName.IsValid ? "" : " error" ) );
                writer.RenderBeginTag( HtmlTextWriterTag.Div );
                tbFirstName.RenderControl( writer );
                rfvFirstName.RenderControl( writer );
                writer.RenderEndTag();
                writer.RenderEndTag();

                writer.RenderBeginTag( HtmlTextWriterTag.Td );
                tbNickName.RenderControl( writer );
                writer.RenderEndTag();

                writer.RenderBeginTag( HtmlTextWriterTag.Td );
                writer.AddAttribute( HtmlTextWriterAttribute.Class, "form-group" + ( rfvLastName .IsValid ? "" : " error" ) );
                writer.RenderBeginTag( HtmlTextWriterTag.Div );
                tbLastName.RenderControl( writer );
                rfvLastName.RenderControl( writer );
                writer.RenderEndTag();
                writer.RenderEndTag();

                writer.RenderBeginTag( HtmlTextWriterTag.Td );
                writer.AddAttribute( HtmlTextWriterAttribute.Class, "form-group" + ( rfvGender.IsValid ? "" : " error" ) );
                writer.RenderBeginTag( HtmlTextWriterTag.Div );
                rfvGender.Enabled = RequireGender;
                rblGender.RenderControl( writer );
                if ( rfvGender.Enabled )
                {
                    rfvGender.RenderControl( writer );
                }
                writer.RenderEndTag();
                writer.RenderEndTag();

                writer.RenderBeginTag( HtmlTextWriterTag.Td );
                dpBirthdate.RenderControl( writer );
                writer.RenderEndTag();

                writer.RenderBeginTag( HtmlTextWriterTag.Td );
                ddlStatus.RenderControl( writer );
                writer.RenderEndTag();

                writer.RenderBeginTag( HtmlTextWriterTag.Td );
                if ( ShowGrade )
                {
                    ddlGrade.RenderControl( writer );
                }
                writer.RenderEndTag();

                writer.RenderBeginTag( HtmlTextWriterTag.Td );
                lbDelete.RenderControl( writer );
                writer.RenderEndTag();

                writer.RenderEndTag();
            }
        }

        /// <summary>
        /// Binds the gender.
        /// </summary>
        private void BindGender()
        {
            string selectedValue = rblGender.SelectedValue;

            rblGender.Items.Clear();
            rblGender.Items.Add( new ListItem( "M", "Male" ) );
            rblGender.Items.Add( new ListItem( "F", "Female" ) );
            if ( !RequireGender )
            {
                rblGender.Items.Add( new ListItem( "Unknown", "" ) );
            }

            rblGender.SelectedValue = selectedValue;
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