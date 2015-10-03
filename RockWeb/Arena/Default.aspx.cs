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
        _redirectRules.Add( new RedirectRule( "/Arena/default.aspx?portal=5", "/", false ) );
        _redirectRules.Add( new RedirectRule( "/Arena/default.aspx?page=17623", "/about", false ) );
        _redirectRules.Add( new RedirectRule( "/Arena/default.aspx?page=18386", "/anthem", false ) );
        _redirectRules.Add( new RedirectRule( "/Arena/default.aspx?page=17018", "/apps", false ) );
        _redirectRules.Add( new RedirectRule( "/Arena/default.aspx?page=19221", "/avondale", false ) );
        _redirectRules.Add( new RedirectRule( "/Arena/default.aspx?page=17655", "/baptism", false ) );
        _redirectRules.Add( new RedirectRule( "/Arena/default.aspx?page=17811", "/care", false ) );
        _redirectRules.Add( new RedirectRule( "/Arena/default.aspx?page=17663", "/careers", false ) );
        _redirectRules.Add( new RedirectRule( "/Arena/default.aspx?page=17668", "/contact", false ) );
        _redirectRules.Add( new RedirectRule( "/Arena/default.aspx?page=17595", "/dashboard", false ) );
        _redirectRules.Add( new RedirectRule( "/Arena/default.aspx?page=17900", "/eastvalley", false ) );
        _redirectRules.Add( new RedirectRule( "/Arena/default.aspx?page=19675", "/ebooks/htdad", false ) );
        _redirectRules.Add( new RedirectRule( "/Arena/default.aspx?page=17705", "/events", false ) );
        _redirectRules.Add( new RedirectRule( "/Arena/default.aspx?page=17397", "/events", false ) );
        _redirectRules.Add( new RedirectRule( "/Arena/default.aspx?page=17393&eventId=24364", "/events", false ) );
        _redirectRules.Add( new RedirectRule( "/Arena/default.aspx?page=17806", "/finance", false ) );
        _redirectRules.Add( new RedirectRule( "/Arena/default.aspx?page=12699", "/ForgotUserName", false ) );
        _redirectRules.Add( new RedirectRule( "/Arena/default.aspx?page=17659", "/foundations", false ) );
        _redirectRules.Add( new RedirectRule( "/Arena/default.aspx?page=18485", "/give", false ) );
        _redirectRules.Add( new RedirectRule( "/Arena/default.aspx?page=18562", "/give/history", false ) );
        _redirectRules.Add( new RedirectRule( "/Arena/default.aspx?page=18566", "/give/manage", false ) );
        _redirectRules.Add( new RedirectRule( "/Arena/default.aspx?page=18560", "/give/now", false ) );
        _redirectRules.Add( new RedirectRule( "/Arena/default.aspx?page=18559", "/give/now", false ) );
        _redirectRules.Add( new RedirectRule( "/Arena/default.aspx?page=18012", "/haf", false ) );
        _redirectRules.Add( new RedirectRule( "/Arena/default.aspx?page=17369", "/home", false ) );
        _redirectRules.Add( new RedirectRule( "/arena/default.aspx?page=17369", "/home", false ) );
        _redirectRules.Add( new RedirectRule( "/Arena/default.aspx?page=19622", "/kids", false ) );
        _redirectRules.Add( new RedirectRule( "/Arena/default.aspx?page=17631", "/leadership", false ) );
        _redirectRules.Add( new RedirectRule( "/Arena/default.aspx?page=17641", "/leadershipinstitute", false ) );
        _redirectRules.Add( new RedirectRule( "/Arena/default.aspx?page=17628", "/locations", false ) );
        _redirectRules.Add( new RedirectRule( "/Arena/default.aspx?page=719", "/login", false ) );
        _redirectRules.Add( new RedirectRule( "/Arena/default.aspx?page=719&requestUrl=/Arena/default.aspx?page=14964", "/login", false ) );
        _redirectRules.Add( new RedirectRule( "/Arena/default.aspx?page=19495", "/lyn", false ) );
        _redirectRules.Add( new RedirectRule( "/Arena/Mobile.aspx?page=18095", "/m/events", false ) );
        _redirectRules.Add( new RedirectRule( "/Arena/mobile.aspx?page=14347&from=MobileSite", "/m/give", false ) );
        _redirectRules.Add( new RedirectRule( "/Arena/Mobile.aspx?page=18108&channel=1", "/m/watch", false ) );
        _redirectRules.Add( new RedirectRule( "/Arena/default.aspx?page=14347&portal=5&from=MobileApp&version=1", "/ma-give", false ) );
        _redirectRules.Add( new RedirectRule( "/Arena/default.aspx?page=17642", "/missions", false ) );
        _redirectRules.Add( new RedirectRule( "/Arena/default.aspx?page=17636", "/neighborhoods", false ) );
        _redirectRules.Add( new RedirectRule( "/Arena/default.aspx?page=17400", "/nextsteps", false ) );
        _redirectRules.Add( new RedirectRule( "/Arena/default.aspx?page=16120", "/page/501", false ) );
        _redirectRules.Add( new RedirectRule( "/Arena/default.aspx?page=17640", "/page/827", false ) );
        _redirectRules.Add( new RedirectRule( "/Arena/default.aspx?page=17676", "/peoria", false ) );
        _redirectRules.Add( new RedirectRule( "/Arena/default.aspx?page=15168", "/prayer/team", false ) );
        _redirectRules.Add( new RedirectRule( "/Arena/default.aspx?page=18767", "/program", false ) );
        _redirectRules.Add( new RedirectRule( "/arena/default.aspx?page=17677", "/scottsdale", false ) );
        _redirectRules.Add( new RedirectRule( "/Arena/default.aspx?page=17660", "/serve", false ) );
        _redirectRules.Add( new RedirectRule( "/Arena/default.aspx?page=18128", "/specialneeds", false ) );
        _redirectRules.Add( new RedirectRule( "/Arena/default.aspx?page=19341", "/spks", false ) );
        _redirectRules.Add( new RedirectRule( "/Arena/default.aspx?page=19123", "/students", false ) );
        _redirectRules.Add( new RedirectRule( "/Arena/default.aspx?page=17647", "/students/highschool", false ) );
        _redirectRules.Add( new RedirectRule( "/arena/default.aspx?page=17683", "/surprise", false ) );
        _redirectRules.Add( new RedirectRule( "/Arena/default.aspx?page=17683", "/surprise", false ) );
        _redirectRules.Add( new RedirectRule( "/Arena/default.aspx?page=18646", "/watch", false ) );
        _redirectRules.Add( new RedirectRule( "/Arena/default.aspx?page=17429&channel=1", "/watch", false ) );
        _redirectRules.Add( new RedirectRule( "/Arena/default.aspx?page=18646&p=", "/watch?PageNum=", true ) );
        _redirectRules.Add( new RedirectRule( "/Arena/default.aspx?page=17529&item=", "/watch/message?MessageId=", true ) );
        _redirectRules.Add( new RedirectRule( "/Arena/default.aspx?page=18604&topic=", "/watch/series?SeriesId=", true ) );
        _redirectRules.Add( new RedirectRule( "/Arena/default.aspx?page=18735", "/youngadults", false ) );

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
