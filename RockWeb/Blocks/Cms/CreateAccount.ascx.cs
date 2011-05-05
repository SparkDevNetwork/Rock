using System;
using System.Collections.Generic;
using Facebook;
using Rock.Models.Cms;
using Rock.Repository.Cms;

namespace Rock.Web.Blocks.Cms
{
    public partial class CreateAccount : Rock.Cms.CmsBlock
    {
        private string _returnUrl;

        protected void Page_Init( object sender, EventArgs e )
        {
            fbSubmit.Click += fbSubmit_Click;
            string clientId = cuWizard.CreateUserStep.ContentTemplateContainer.FindControl( "UserName" ).ClientID;
            string jqScript = @"
    $(document).ready(function () {
        var usernameUnavailableRow = $('#usernameUnavailableRow');
        var availabilityMessage = $('#availabilityMessage');
        var usernameTextbox = $('#" + clientId + @"');

        usernameUnavailableRow.hide();

        usernameTextbox.blur(function () {
            if ($(this).val()) {
                $.getJSON('AspxServices/UsernameAvailable.aspx?' + escape($(this).val()), function (results) {
                    if (results.available) {
                        if (usernameUnavailableRow.is(':visible')) {
                            availabilityMessage.html('This username is available.');
                            availabilityMessage.addClass('usernameAvailable');
                            availabilityMessage.removeClass('usernameTaken');
                        }
                    }
                    else {
                        usernameUnavailableRow.show();
                        availabilityMessage.html('This username is already taken!');
                        availabilityMessage.addClass('usernameTaken');
                        availabilityMessage.removeClass('usernameAvailable');
                    }
                });
            }
        });
    });";
            Page.ClientScript.RegisterClientScriptBlock( this.GetType(), "usernameAvailable", jqScript, true );

        }

        protected void Page_Load(object sender, EventArgs e)
        {
            if (Request.QueryString["code"] != null)
            {
                ShowView(Request.QueryString["code"]);
            }

            _returnUrl = Request.QueryString["returnurl"];
        }

        protected void fbRegister_Click(object sender, EventArgs e)
        {
            var oAuthClient = new FacebookOAuthClient(FacebookApplication.Current) { RedirectUri = new Uri(GetCreateAccountUrl()) };

            // Get user permission. Basic permissions are all that's needed.
            var loginUri = oAuthClient.GetLoginUrl(new Dictionary<string, object> { { "state", _returnUrl } });
            Response.Redirect(loginUri.AbsoluteUri);
        }

        protected void fbSubmit_Click(object sender, EventArgs e)
        {
            var userRepository = new EntityUserRepository();
            var user = new User
                           {
                               Username = fbUserName.Text,
                               FacebookId = fbFacebookId.Value
                           };

            //userRepository.Add(user);
            userRepository.Save(user, null);
        }

        private void ShowView(string code)
        {
            FacebookOAuthResult oAuthResult;

            if (FacebookOAuthResult.TryParse(Request.Url, out oAuthResult))
            {
                if (oAuthResult.IsSuccess)
                {
                    var oAuthClient = new FacebookOAuthClient(FacebookApplication.Current) { RedirectUri = new Uri(GetCreateAccountUrl()) };
                    dynamic tokenResult = oAuthClient.ExchangeCodeForAccessToken(code);
                    string accessToken = tokenResult.access_token;

                    FacebookClient fbClient = new FacebookClient(accessToken);
                    dynamic me = fbClient.Get("me");
                    fbFacebookId.Value = me.id;
                }
            }

            // TODO: Show user feedback indicating facebook access was not successful
        }

        private string GetCreateAccountUrl()
        {
            return new Uri(string.Format("~/page/{0}{1}", PageInstance.Id, Request.QueryString)).AbsoluteUri;
        }
   }
}