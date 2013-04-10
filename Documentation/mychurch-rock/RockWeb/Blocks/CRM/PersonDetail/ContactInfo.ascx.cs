//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//

using System;
using System.Linq;
using System.Web.UI;
using System.Web.UI.HtmlControls;

using Rock;
using Rock.Model;

namespace RockWeb.Blocks.Crm.PersonDetail
{
    public partial class ContactInfo : Rock.Web.UI.PersonBlock
    {
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );

            var service = new PhoneNumberService();
            foreach ( dynamic item in service.Queryable()
                .Where( n => n.PersonId == Person.Id )
                .OrderBy( n => n.NumberTypeValue.Order )
                .Select( n => new
                {
                    Number = n.Number,
                    Unlisted = n.IsUnlisted,
                    Type = n.NumberTypeValue.Name
                }
                ) )
            {
                var li = new HtmlGenericControl( "li" );
                ulPhoneNumbers.Controls.Add( li );

                var anchor = new HtmlGenericControl( "a" );
                li.Controls.Add( anchor );
                anchor.Attributes.Add("href", "#");
                anchor.AddCssClass( "highlight" );

                var icon = new HtmlGenericControl( "i" );
                anchor.Controls.Add( icon );
                icon.AddCssClass( "icon-phone" );

                if ( item.Unlisted )
                {
                    var span = new HtmlGenericControl( "span" );
                    anchor.Controls.Add( span );
                    span.AddCssClass( "phone-unlisted" );
                    span.Attributes.Add( "data-value", PhoneNumber.FormattedNumber( item.Number ) );
                    span.InnerText = "Unlisted";
                }
                else
                {
                    anchor.Controls.Add( new LiteralControl( PhoneNumber.FormattedNumber( item.Number ) ) );
                }

                anchor.Controls.Add( new LiteralControl( " " ) );

                var small = new HtmlGenericControl( "small" );
                anchor.Controls.Add( small );
                small.InnerText = item.Type;
            }

            if ( !String.IsNullOrWhiteSpace( Person.Email ) )
            {
                hlEmail.Text = Person.Email;
                hlEmail.NavigateUrl = "mailto:" + Person.Email;
            }

        }
    }
}