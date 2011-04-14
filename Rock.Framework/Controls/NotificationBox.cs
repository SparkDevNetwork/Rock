using System;  
using System.Collections.Generic;  
using System.ComponentModel;  
using System.Linq;  
using System.Text;  
using System.Web;  
using System.Web.UI;  
using System.Web.UI.WebControls;


namespace Rock.Controls  
{  
     [DefaultProperty("Text")]
     [ToolboxData( "<{0}:NotificationBox runat=server></{0}:NotificationBox>" )]  
     public class NotificationBox : Literal  
     {
         private string _Title;
         public string Title
         {
             get { return _Title; }
             set { _Title = value; }
         }

         private NotificationBoxType _NotificationBoxType;
         public NotificationBoxType NotificationBoxType
         {
             get { return _NotificationBoxType; }
             set { _NotificationBoxType = value; }
         }  

         protected override void Render(HtmlTextWriter writer)  
         {
             writer.Write( "<div class=\"notification-box " + _NotificationBoxType.ToString().ToLower() + "\">" + Environment.NewLine );
             writer.Write( "    <div class=\"text\">" + Environment.NewLine );

             if ( _Title != null && _Title != string.Empty )
                 writer.Write( "         <strong>" + _Title + "</strong>" );
             
             writer.Write( this.Text );

             writer.Write( "     </div>" + Environment.NewLine );
             writer.Write( "</div>" + Environment.NewLine );
         } 
    }

     public enum NotificationBoxType
     {
         Information,
         Warning,
         Error,
         Tip,
         Success
     };


}  