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
        DropDownList ddlTitle;
        TextBox tbFirstName;
        TextBox tbNickName;
        TextBox tbLastName;
        RadioButtonList rblGender;
        DatePicker dpBirthdate;
        DropDownList ddlStatus;
        DropDownList ddlGrade;

        LinkButton lbDelete;

        public int? RoleId
        {
            get { return rblRole.SelectedValueAsInt(); }
            set { SetListValue( rblRole, value ); }
        }

        public int? TitleValueId
        {
            get { return ddlTitle.SelectedValueAsInt(); }
            set { SetListValue( ddlTitle, value ); }
        }

        public string FirstName
        {
            get { return tbFirstName.Text; }
            set { tbFirstName.Text = value; }
        }

        public string NickName
        {
            get { return tbNickName.Text; }
            set { tbNickName.Text = value; }
        }

        public string LastName
        {
            get { return tbLastName.Text; }
            set { tbLastName.Text = value; }
        }

        public DateTime? BirthDate
        {
            get { return dpBirthdate.SelectedDate; }
            set { dpBirthdate.SelectedDate = value; }
        }

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
            set { SetListValue( rblGender, value.ConvertToString() ); }
        }

        public int? StatusValueId
        {
            get { return ddlStatus.SelectedValueAsInt(); }
            set { SetListValue( ddlStatus, value ); }
        }

        public int? Grade
        {
            get { return ddlGrade.SelectedValueAsInt(); }
            set { SetListValue( ddlGrade, value ); }
        }

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

        public NewFamilyMembersRow()
            : base()
        {
            rblRole = new RadioButtonList();
            ddlTitle = new DropDownList();
            tbFirstName = new TextBox();
            tbNickName = new TextBox();
            tbLastName = new TextBox();
            rblGender = new RadioButtonList();
            dpBirthdate = new DatePicker();
            ddlStatus = new DropDownList();
            ddlGrade = new DropDownList();
            lbDelete = new LinkButton();

            rblRole.DataTextField = "Value";
            rblRole.DataValueField = "Key";
            rblRole.DataSource = FamilyRoles;
            rblRole.DataBind();

            BindListToDefinedType( ddlTitle, Rock.SystemGuid.DefinedType.PERSON_TITLE );

            rblGender.Items.Clear();
            rblGender.Items.Add( new ListItem( "Male", "Male" ) );
            rblGender.Items.Add( new ListItem( "Female", "Female" ) );

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

        protected override void CreateChildControls()
        {
            base.CreateChildControls();
            Controls.Clear();

            rblRole.ID = this.ID + "_rblRole";
            ddlTitle.ID = this.ID + "_ddlTitle";
            tbFirstName.ID = this.ID + "_tbFirstName";
            tbNickName.ID = this.ID + "_tbNickName";
            tbLastName.ID = this.ID + "_tbLastName";
            rblGender.ID = this.ID + "_rblGender";
            dpBirthdate.ID = this.ID + "_dtBirthdate";
            ddlStatus.ID = this.ID + "_ddlStatus";
            ddlGrade.ID = this.ID + "_ddlGrade";
            lbDelete.ID = this.ID + "_lbDelete";

            Controls.Add( rblRole );
            Controls.Add( ddlTitle );
            Controls.Add( tbFirstName );
            Controls.Add( tbNickName );
            Controls.Add( tbLastName );
            Controls.Add( rblGender );
            Controls.Add( dpBirthdate );
            Controls.Add( ddlStatus );
            Controls.Add( ddlGrade );
            Controls.Add( lbDelete );

            rblRole.RepeatDirection = RepeatDirection.Horizontal;
            ddlTitle.CssClass = "input-small";
            tbFirstName.CssClass = "input-small";
            tbNickName.CssClass = "input-small";
            tbLastName.CssClass = "input-small";
            rblGender.RepeatDirection = RepeatDirection.Horizontal;
            ddlStatus.CssClass = "input-medium";
            ddlGrade.CssClass = "input-mini";

            lbDelete.CssClass = "btn btn-mini btn-danger";
            lbDelete.Click += lbDelete_Click;
            lbDelete.CausesValidation = false;

            var iDelete = new HtmlGenericControl( "i" );
            lbDelete.Controls.Add( iDelete );
            iDelete.AddCssClass( "icon-remove" );
        }

        public override void RenderControl( HtmlTextWriter writer )
        {
            writer.RenderBeginTag( HtmlTextWriterTag.Tr );

            writer.RenderBeginTag( HtmlTextWriterTag.Td );
            rblRole.RenderControl( writer );
            writer.RenderEndTag();

            writer.RenderBeginTag( HtmlTextWriterTag.Td );
            ddlTitle.RenderControl( writer );
            writer.RenderEndTag();

            writer.RenderBeginTag( HtmlTextWriterTag.Td );
            tbFirstName.RenderControl( writer );
            writer.RenderEndTag();

            writer.RenderBeginTag( HtmlTextWriterTag.Td );
            tbNickName.RenderControl( writer );
            writer.RenderEndTag();

            writer.RenderBeginTag( HtmlTextWriterTag.Td );
            tbLastName.RenderControl( writer );
            writer.RenderEndTag();

            writer.RenderBeginTag( HtmlTextWriterTag.Td );
            rblGender.RenderControl( writer );
            writer.RenderEndTag();

            writer.RenderBeginTag( HtmlTextWriterTag.Td );
            dpBirthdate.RenderControl( writer );
            writer.RenderEndTag();

            writer.RenderBeginTag( HtmlTextWriterTag.Td );
            ddlStatus.RenderControl( writer );
            writer.RenderEndTag();

            writer.RenderBeginTag( HtmlTextWriterTag.Td );
            ddlGrade.RenderControl( writer );
            writer.RenderEndTag();

            writer.RenderBeginTag( HtmlTextWriterTag.Td );
            lbDelete.RenderControl( writer );
            writer.RenderEndTag();

            writer.RenderEndTag();
        }

        protected void BindListToDefinedType( ListControl listControl, string definedTypeGuid, bool insertBlankOption = false )
        {
            var definedType = DefinedTypeCache.Read( new Guid( definedTypeGuid ) );
            listControl.BindToDefinedType( definedType, insertBlankOption );
        }

        private void SetListValue(ListControl listControl, int? value)
        {
            foreach(ListItem item in listControl.Items)
            {
                item.Selected = (value.HasValue && item.Value == value.Value.ToString());
            }
        }
        private void SetListValue( ListControl listControl, string value )
        {
            foreach ( ListItem item in listControl.Items )
            {
                item.Selected = ( item.Value == value );
            }
        }

        void lbDelete_Click( object sender, EventArgs e )
        {
            if ( DeleteClick != null )
            {
                DeleteClick( this, e );
            }
        }

        /// <summary>
        /// Occurs when [delete click].
        /// </summary>
        public event EventHandler DeleteClick;

    }

}