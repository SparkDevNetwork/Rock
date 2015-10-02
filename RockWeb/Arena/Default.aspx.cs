using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

public partial class Arena_Default : System.Web.UI.Page
{
    private List<RedirectRule> _redirectRules = new List<RedirectRule>();
    
    
    protected void Page_Load(object sender, EventArgs e)
    {
        // add the rules
        _redirectRules.Add( new RedirectRule( "/Arena/default.aspx?page=17369", "/home", false ) );
        _redirectRules.Add( new RedirectRule( "/Arena/default.aspx?page=18646", "/watch", false ) );
        _redirectRules.Add( new RedirectRule( "/Arena/Mobile.aspx?page=18108&channel=19", "/m/watch", false ) );
        _redirectRules.Add( new RedirectRule( "/Arena/default.aspx?page=17529&item=", "/watch/message?MessageId=", true ) );

        // get the current request
        var requestPath = Request.Url.PathAndQuery;

        foreach ( var rule in _redirectRules )
        {
            if ( requestPath.StartsWith( rule.OldAddress ) )
            {
                string newAddress = "";
                
                if ( rule.ReplaceString )
                {
                    
                    newAddress = Request.Url.OriginalString.Replace( rule.OldAddress, rule.NewAddress );
                }
                else
                {
                    newAddress = Request.Url.GetLeftPart(UriPartial.Authority) + rule.NewAddress;
                }

                HttpContext.Current.Response.Status = "301 Moved Permanently";
                HttpContext.Current.Response.AddHeader( "Location", newAddress );
            }
        }
    }

    public class RedirectRule
    {
        public string OldAddress { get; set; }
        public string NewAddress { get; set; }
        public bool ReplaceString { get; set; }

        public RedirectRule( string oldAddress, string newAddress, bool ReplaceString )
        {
            this.OldAddress = oldAddress;
            this.NewAddress = newAddress;
            this.ReplaceString = ReplaceString;
        }
    }
}