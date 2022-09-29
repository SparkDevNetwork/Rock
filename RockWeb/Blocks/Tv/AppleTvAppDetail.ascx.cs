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
using Newtonsoft.Json;
using Rock;
using Rock.Data;
using Rock.Model;
using Rock.Tv.Classes;
using Rock.Web;
using Rock.Web.Cache;
using Rock.Web.UI;

namespace RockWeb.Blocks.Tv
{
    [DisplayName( "Apple TV Application Detail" )]
    [Category( "TV > TV Apps" )]
    [Description( "Allows a person to edit an Apple TV application." )]

    #region Block Attributes

    #endregion Block Attributes
    [Rock.SystemGuid.BlockTypeGuid( "49F3D87E-BD8D-43D4-8217-340F3DFF4562" )]
    public partial class AppleTvAppDetail : Rock.Web.UI.RockBlock
    {

        #region Fields

        // Temporary v14 patch, we do this better in v14.1.
        private string _defaultApplicationJs = @"// <copyright>
// Copyright by the Spark Development Network
//
// Licensed under the Rock Community License (the ""License"");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.rockrms.com/license
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an ""AS IS"" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
// </copyright>
//

// Uncomment out the line below to send the console to C#
//console.log = (message) => appCommands.log(message);

// ----------------------------------------------------------------------------------
// Utility Screens
// These screens are used in various places inside of the application.
// ----------------------------------------------------------------------------------
let loadingTemplate = '<document><loadingTemplate><activityIndicator><text>Loading</text></activityIndicator></loadingTemplate></document>';

// ----------------------------------------------------------------------------------
// NavigationStack
// This class is used by the RockTvApp to store the navigation stack and provide
// actions to change the pages while attaching main event handler.
// ----------------------------------------------------------------------------------
class NavigationStack {

    // Constructor
    constructor() {
        this.handler = new TvEventHandler(); // Creates the global event handler
    }

    pushDocument(doc, useHandler = true) {
        this._applyHandler(doc, useHandler);
        navigationDocument.pushDocument(doc);
    }

    replaceCurrentDocument(newDoc, useHandler = true) {

        // We need to replace the current doc so get that
        var previousDoc = getActiveDocument();

        // Check that the previous not null (the navigation stack could have been cleared)
        if (previousDoc == undefined) {
            this.pushDocument(newDoc, useHandler);
            return;
        }

        this._applyHandler(newDoc, useHandler);
        navigationDocument.replaceDocument(newDoc, previousDoc);
    }

    replaceDocument(newDoc, previousDoc, useHandler = true) {
        this._applyHandler(newDoc, useHandler);
        navigationDocument.replaceDocument(newDoc, previousDoc);
    }

    popDocument() {
        navigationDocument.popDocument();
    }

    clear() {
        navigationDocument.clear();
    }

    presentModal(doc, useHandler = true) {
        this._applyHandler(doc, useHandler);
        navigationDocument.presentModal(doc);
    }

    dismissModal() {
        navigationDocument.dismissModal();
    }

    // Gets the document from the server
    async getDocumentFromUrl(url, mergeFields, cacheControl, suppressInteraction = false) {
        // Get the content of the new page
        let content = await appCommands.loadPageCacheControlSuppressInteraction(url, cacheControl, suppressInteraction);

        // If page content not available then create an alert
        if (!content) {
            var alert = RockTvApp.createAlertDocument(""Error"", `Was not able to load the page. \n Page: ${RockTvApp.AppState.HomepageGuid}`);
            return alert;
        }

        // Render in merge fields
        if (mergeFields) {
            console.log(""[INFO] Parsing template \n"" + content);
            content = content.replace(/\{\s*(\w+)\s*}/g, (_, v) => (typeof mergeFields[v] != 'undefined') ? mergeFields[v] : '');
        }

        // Parse the resulting string content to an XML document
        let templateParser = new DOMParser();

        let doc = null;
        try {
            doc = templateParser.parseFromString(content, ""application/xml"");
        }
        catch (err) {
            console.log(""[ERROR] An error occurred parsing the document."", err, content);
            var alert = RockTvApp.createAlertDocument(""Error"", `An error occurred parsing the content provided.`);
            return alert;
        }

        if (doc) {
            return doc;
        }

        // The document was not able to be parsed so show alert
        var alert = RockTvApp.createAlertDocument(""Error"", `An invalid page was returned. \n Page: ${RockTvApp.AppState.HomepageGuid}`);
        return alert;
    }

    // Gets' the loading document from the application's loading template
    getLoadingDocument() {
        if (this._loadingTemplate) {
            return this._loadingTemplate;
        }

        let templateParser = new DOMParser();
        this._loadingTemplate = templateParser.parseFromString(loadingTemplate, ""application/xml"");
        return this._loadingTemplate;
    }

    // Adds global handler if requested
    _applyHandler(doc, useHandler) {
        if (useHandler) {
            doc.addEventListener(""select"", this.handler.handleEvent);
        }
    }
}

// ----------------------------------------------------------------------------------
// Login Handler
// This class abstracts the login logic into it's own handler.
// ----------------------------------------------------------------------------------
class LoginHandler {

    constructor(timeout, interval, authCode, timeoutPageGuid, successPageGuid, clearNavigationStackOnSuccess) {
        this.endTime = Number(new Date()) + (timeout * 1000);
        this.interval = interval * 1000;
        this.interationCount = 0;
        this.authCode = authCode;
        this.timeoutPageGuid = timeoutPageGuid;
        this.successPageGuid = successPageGuid;
        this.clearNavigationStackOnSuccess = clearNavigationStackOnSuccess;
    }

    // This is called on every 'tick' of the clock to determine if the individual has logged in yet.
    async checkCondition() {
        var currentTime = Number(new Date());
        this.interationCount++;

        // Check that the end time is not zero. This can be the case if the page has been unloaded and there is
        // one more tick of the clock
        if (this.endTime == 0) {
            return;
        }

        // Check the server to see if the authenication has occurred
        var isAuthComplete = await appCommands.checkForAuthentication(this.authCode);

        if (isAuthComplete) {
            console.log('[INFO] Login Success');

            if (this.clearNavigationStackOnSuccess) {
                RockTvApp.NavigationStack.clear();
            }

            var doc = await RockTvApp.NavigationStack.getDocumentFromUrl(this.successPageGuid, null, null, true);
            RockTvApp.NavigationStack.replaceCurrentDocument(doc);
        }
        else if (currentTime < this.endTime) {
            console.log(`[INFO] Checking for Authorization Complete | Interval: ${this.interval}ms - Interation: ${this.interationCount}`);

            // Wait for check duration and call again
            setTimeout(this.checkCondition.bind(this), this.interval);
        }
        // Didn't match and too much time, timeout!
        else {
            console.log('[INFO] Login Timed out');

            var doc = await RockTvApp.NavigationStack.getDocumentFromUrl(this.timeoutPageGuid, null, null, true);
            RockTvApp.NavigationStack.replaceCurrentDocument(doc);
        }
    }

    // Starts the clock ticking.
    startPolling() {
        // Start the polling process. The method call below is the initial check, which will then call itself (with delay) until the 
        // auth occurs or hits the timeout duration.
        console.log(""[INFO] Request to start polling sent."");
        this.checkCondition();
    }

    // Stops the clock
    stopPolling() {
        this.endTime = 0;
        console.log(""[INFO] Stopping the polling timer."");
    }

}

// ----------------------------------------------------------------------------------
// TvEventHandler
// The main event handler for the app. This processes all of the commands that come in
// using the appropriate utility classes.
// ----------------------------------------------------------------------------------
class TvEventHandler {
    constructor() {
        this.handleEvent = this.handleEvent.bind(this);
        this._loadingTemplate = null;
        this._lastMediaReportedTime = null;
        this.isProcessing = false;
    }

    async handleEvent(event) {
        if (this.isProcessing) {
            return;
        }

        this.isProcessing = true;
        try {
            var element = event.target;

            // Get command type
            var commandType = element.getAttribute(""rockCommand"");

            // Several commands can be specified as a comma separated string.
            var commands = commandType.split("","").map(function (item) {
                return item.trim();
            });

            console.log(""[INFO] Processing Command"", commands, element);

            // Process each command
            for (const command of commands) {
                switch (command) {

                    case ""pushPage"":
                    case ""replacePage"":
                    case ""presentModal"":
                        {
                            await this.processNavigationEvent(command, element);
                            break;
                        }
                    case ""popPage"":
                        {
                            RockTvApp.NavigationStack.popDocument();
                            break;
                        }
                    case ""clearNavigationStack"":
                        {
                            RockTvApp.NavigationStack.clear();
                            break;
                        }
                    case ""dismissModal"":
                        {
                            RockTvApp.NavigationStack.dismissModal();
                            break;
                        }
                    case ""playVideo"":
                        {
                            this.processPlayVideo(element);
                            break;
                        }
                    case ""playAudio"":
                        {
                            this.processPlayAudio(element);
                            break;
                        }
                    case ""setContext"":
                        {
                            let key = element.getAttribute(""rockContextKey"");
                            let value = element.getAttribute(""rockContextValue"");

                            appCommands.setContextValue(key, value);
                            break;
                        }
                    case ""clearContext"":
                        {
                            let key = element.getAttribute(""rockContextKey"");

                            appCommands.clearContext(key);
                            break;
                        }
                    case ""showDemo"":
                        {
                            if (RockTvApp.AppState.DemoModeEnabled) {
                                var demoDocument = RockTvApp.createDemoDocument();
                                RockTvApp.NavigationStack.pushDocument(demoDocument);
                            }
                            else {
                                var alert = RockTvApp.createAlertDocument(""Demo Mode"", ""Demo is not supported on this application."");
                                RockTvApp.NavigationStack.pushDocument(alert);
                            }

                            break;
                        }
                    case ""updateDemo"":
                        {
                            this.updateDemoSettings();
                            break;
                        }
                    case ""clearDemo"":
                        {
                            this.clearDemoSettings();
                            break;
                        }
                    case ""login"":
                        {
                            await this.processLogin(element);
                            break;
                        }
                    case ""logout"":
                        {
                            await this.processLogout(element);
                            break;
                        }
                    default:
                        {
                            console.log(`[WARNING] Unrecognized command encountered. The command ""${commandType}"" was not found.`, event);
                            break;
                        }
                }
            }
        }
        finally {
            this.isProcessing = false;
        }
    }

    // processesLoginCommand
    async processLogin(element) {

        // Get configuration
        let loginPageGuid = element.getAttribute(""rockLoginPageGuid"") ? element.getAttribute(""rockLoginPageGuid"") : '';
        let loginTimeoutPageGuid = element.getAttribute(""rockLoginTimeoutPageGuid"") ? element.getAttribute(""rockLoginTimeoutPageGuid"") : '';
        let loginSuccessPageGuid = element.getAttribute(""rockLoginSuccessPageGuid"") ? element.getAttribute(""rockLoginSuccessPageGuid"") : '';

        let loginTimeoutDuration = element.getAttribute(""rockLoginTimeoutDuration"") ? element.getAttribute(""rockLoginTimeoutDuration"") : 600;
        let loginCheckDuration = element.getAttribute(""rockLoginCheckDuration"") ? element.getAttribute(""rockLoginCheckDuration"") : 5;
        let loginClearNavigationStack = element.getAttribute(""rockLoginClearNavigationStack"") ? element.getAttribute(""rockLoginClearNavigationStack"") : true;

        // Make call to get accessToken
        var authResponse = await appCommands.getLoginAuthCode();
        console.log(""[INFO] Authenication Session Started"", authResponse);

        // start long polling
        RockTvApp.LoginHandler = new LoginHandler(loginTimeoutDuration, loginCheckDuration, authResponse.authCode, loginTimeoutPageGuid, loginSuccessPageGuid, loginClearNavigationStack);

        // Add spaces bettwen letters for better readability
        authResponse.authCode = authResponse.authCode.split('').join(' ');

        // Show login screen
        var doc = await RockTvApp.NavigationStack.getDocumentFromUrl(loginPageGuid, authResponse, ""Private"");

        doc.addEventListener(""disappear"", function (_) {
            RockTvApp.LoginHandler.stopPolling();
            RockTvApp.LoginHandler = null;
            console.log(""[INFO] Canceling the login process: individual has unloaded the page."");
        });

        RockTvApp.NavigationStack.pushDocument(doc);

        RockTvApp.LoginHandler.startPolling();
    }

    // Logs the individual out
    async processLogout(element) {

        // Get configuration
        let logoutPageGuid = element.getAttribute(""rockLogoutPageGuid"") ? element.getAttribute(""rockLogoutPageGuid"") : '';
        let logoutClearNavigationStack = element.getAttribute(""rockLogoutClearNavigationStack"") ? element.getAttribute(""rockLogoutClearNavigationStack"") : true;

        // Call the logout command in C#
        appCommands.logout();

        // Clear navigation stack if needed
        if (logoutClearNavigationStack) {
            RockTvApp.NavigationStack.clear();
        }

        // Load the logout page
        var doc = await RockTvApp.NavigationStack.getDocumentFromUrl(logoutPageGuid);
        RockTvApp.NavigationStack.pushDocument(doc);

        console.log(""[INFO] Logged out individual."");
    }

    // Updates the demo settings from the demo screen
    async updateDemoSettings() {

        var doc = navigationDocument.documents[navigationDocument.documents.length - 1];
        var textField = doc.getElementById(""demoLookupInput"");

        var demoKey = textField.getFeature(""Keyboard"").text;

        await appCommands.loadDemoPacket(demoKey);

        var alert = RockTvApp.createAlertDocument(""Demo Settings Updated"", ""Demo settings have been saved. Please restart the application to re-load the settings."");

        // Clear the navigation stack
        RockTvApp.NavigationStack.clear();

        // Show the restart app screen
        RockTvApp.NavigationStack.pushDocument(alert, false);
    }

    // Clears the demo settings from the demo screen
    clearDemoSettings(element) {
        let newHomepage = appCommands.clearDemoSettings();

        var alert = RockTvApp.createAlertDocument(""Demo Settings Cleared"", ""Demo settings have been cleared. Please restart the application to re-load the application."");

        // Clear the navigation stack
        RockTvApp.NavigationStack.clear();

        // Show the restart app screen
        RockTvApp.NavigationStack.pushDocument(alert);
    }

    // Handles the playing of video by reading configuration off of the element
    processPlayVideo(element) {
        this.processPlayMedia('video', element);
    }

    // Handles the playing of audio by reading configuration off of the element
    processPlayAudio(element) {
        this.processPlayMedia('audio', element);
    }

    // Guts of playing media files. The 'type' is a string with the values of video|audio. This prevents to duplication of code for just on string difference.
    processPlayMedia(type, element) {

        // Get configuration from attributes with defaults
        let url = '';

        if (type == 'audio') {
            url = element.getAttribute(""rockAudioUrl"");
        }
        else {
            url = element.getAttribute(""rockVideoUrl"");
        }

        // URL decode the url
        url = decodeURIComponent(url);

        console.log(`[INFO] Media URL: ${url}`);

        let mediaElementGuid = element.getAttribute(""rockVideoMediaElementGuid"") ? element.getAttribute(""rockVideoMediaElementGuid"") : '';
        let relatedEntityTypeId = element.getAttribute(""rockVideoRelatedEntityTypeId"") ? element.getAttribute(""rockVideoRelatedEntityTypeId"") : '';
        let relatedEntityId = element.getAttribute(""rockVideoRelatedEntityId"") ? element.getAttribute(""rockVideoRelatedEntityId"") : '';
        let interactionGuid = element.getAttribute(""rockInteractionGuid"") ? element.getAttribute(""rockInteractionGuid"") : '';
        let watchMap = element.getAttribute(""rockWatchMap"") ? element.getAttribute(""rockWatchMap"") : '';

        let enableResume = element.getAttribute(""rockVideoEnableResume"") ? element.getAttribute(""rockVideoEnableResume"") : true;
        let title = element.getAttribute(""rockVideoTitle"");
        let subtitle = element.getAttribute(""rockVideoSubtitle"");
        let description = element.getAttribute(""rockVideoDescription"");
        let artworkImageURL = element.getAttribute(""rockVideoArtworkImageURL"");

        // Play video
        let player = new Player();

        let media = new MediaItem(type, url);

        if (isNotEmptyOrSpaces(title)) {
            media.title = title;
        }

        if (isNotEmptyOrSpaces(description)) {
            media.description = description;
        }

        if (isNotEmptyOrSpaces(subtitle)) {
            media.subtitle = subtitle;
        }

        if (isNotEmptyOrSpaces(artworkImageURL)) {
            media.artworkImageURL = artworkImageURL;
        }

        // Set current media in app state (used for media interaction processing)
        var resumeLocation = mediaHelper.setCurrentMediaMediaElementGuidWatchMapInteractionGuidRelatedEntityTypeIdRelatedEntityId(url, mediaElementGuid, watchMap, interactionGuid, relatedEntityTypeId, relatedEntityId);

        // Handle resume logic
        if (enableResume) {
            media.resumeTime = resumeLocation;
            console.log(`[INFO] Media Elapsed: Will start playing media ${url} at: ${resumeLocation}`, element);
        }

        player.playlist = new Playlist();
        player.playlist.push(media);

        // Wire up events
        player.addEventListener(""stateDidChange"", this.handleMediaStateChange);
        player.addEventListener(""timeDidChange"", this.handleMediaTimeChange, { interval: 1 });

        // Reset media timer counter
        this._lastMediaReportedTime = null;

        player.play();
    }

    // Handles when the media play time changes (called each second)
    handleMediaTimeChange(s) {

        // Get current time
        let location = Math.round(s.time);

        // Only report a time change if it's different than the last one. This can be less the one second on start or if there is buffering, etc.
        if (location != this._lastMediaReportedTime) {
            mediaHelper.markLocationPlayedDuration(location, s.target.currentMediaItemDuration);
            this._lastMediaReportedTime = location;

            console.log(`[INFO] Media Playing: Maked the location ${location} played.`, s);
        }
    }

    // Handles when the media state changes
    handleMediaStateChange(s) {

        console.log(`[INFO] Media state changed to ${s.state}`, s);

        if (s.state == 'paused' || s.state == 'end') {
            // Get URL and resume location
            let url = s.target.currentMediaItem.url;
            let location = Math.round(s.elapsedTime);

            // Write state to elapsed time table
            console.log(`[INFO] Media Elapsed: Writing resume location of ${url} to ${location}`, s);
            mediaHelper.mediaStoppedLocation(url, location); // mediaHelper is a bridge to C#
        }

        // TODO - delete item from resume table when on duration == elapsedtime
    }

    // Handles navigation events by reading configuration off of the element
    async processNavigationEvent(command, element) {

        // Get configuration from attributes with defaults
        let url = element.getAttribute(""rockPageGuid"");
        let cacheControl = element.getAttribute(""rockPageCacheControl"") ? element.getAttribute(""rockPageCacheControl"") : false;
        let showLoading = element.getAttribute(""rockPageShowLoading"") ? element.getAttribute(""rockPageShowLoading"") : false;
        let suppressInteraction = element.getAttribute(""rockPageSuppressInteraction"") ? element.getAttribute(""rockPageSuppressInteraction"") : false;
        let mergeFields = element.getAttribute(""rockMergeFields"");

        switch (command) {
            case ""pushPage"":
                {
                    if (showLoading) {
                        var loadingDoc = RockTvApp.NavigationStack.getLoadingDocument();
                        RockTvApp.NavigationStack.pushDocument(loadingDoc, false);

                        var doc = await RockTvApp.NavigationStack.getDocumentFromUrl(url, mergeFields, cacheControl, suppressInteraction);
                        RockTvApp.NavigationStack.replaceDocument(doc, loadingDoc);
                        break;
                    }

                    var doc = await RockTvApp.NavigationStack.getDocumentFromUrl(url, mergeFields, cacheControl, suppressInteraction);
                    RockTvApp.NavigationStack.pushDocument(doc);
                    break;
                }
            case ""replacePage"":
                {
                    if (showLoading) {
                        var loadingDoc = RockTvApp.NavigationStack.getLoadingDocument();
                        RockTvApp.NavigationStack.pushDocument(loadingDoc, false);

                        var doc = await RockTvApp.NavigationStack.getDocumentFromUrl(url, mergeFields, cacheControl, suppressInteraction);

                        // Pop the loading screen now that the document is loaded
                        RockTvApp.NavigationStack.pushDocument();

                        // Now replace the current page with the new page
                        RockTvApp.NavigationStack.replaceCurrentDocument(doc);
                    }

                    var doc = await RockTvApp.NavigationStack.getDocumentFromUrl(url, mergeFields, cacheControl, suppressInteraction);
                    RockTvApp.NavigationStack.replaceCurrentDocument(doc);
                    break;
                }
            case ""presentModal"":
                {
                    var doc = await RockTvApp.NavigationStack.getDocumentFromUrl(url, mergeFields, cacheControl, suppressInteraction);
                    RockTvApp.NavigationStack.presentModal(doc, showLoading);
                    break;
                }
        }
    }
}


// ----------------------------------------------------------------------------------
// Startup Logic
// ----------------------------------------------------------------------------------
RockTvApp.NavigationStack = new NavigationStack();
RockTvApp.LoginHandler = null;

RockTvApp.onLaunch = async function (options) {

    // Get homepage
    var homepage = await RockTvApp.NavigationStack.getDocumentFromUrl(RockTvApp.AppState.HomepageGuid, null, ""Personal:600"", false); // TODO should make cache a setting 

    if (homepage) {
        // Present the homepage
        RockTvApp.NavigationStack.pushDocument(homepage);
    }
    else {
        // Show an error message
        var alert = RockTvApp.createAlertDocument(""Error"", `An error occurred loading hompage. \n Page: ${RockTvApp.AppState.HomepageGuid}`);
        RockTvApp.NavigationStack.pushDocument(alert);
    }
}";

        #endregion 
        #region Attribute Keys

        private static class AttributeKey
        {
            //public const string ShowEmailAddress = "ShowEmailAddress";
        }

        #endregion Attribute Keys

        #region PageParameterKeys

        private static class PageParameterKey
        {
            public const string SiteId = "SiteId";
        }

        #endregion PageParameterKeys

        #region Base Control Methods

        // Overrides of the base RockBlock methods (i.e. OnInit, OnLoad)

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );

            // This event gets fired after block settings are updated. It's nice to repaint the screen if these settings would alter it.
            this.BlockUpdated += Block_BlockUpdated;
            this.AddConfigurationUpdateTrigger( upnlContent );
        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Load" /> event.
        /// </summary>
        /// <param name="e">The <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnLoad( EventArgs e )
        {
            base.OnLoad( e );

            // Temporary v14 patch, we do this better in v14.1.
            txtApiKey.Required = true;

            if ( !Page.IsPostBack )
            {
                ShowView();
            }
        }

        #endregion

        #region Events

        /// <summary>
        /// Returns breadcrumbs specific to the block that should be added to navigation
        /// based on the current page reference.  This function is called during the page's
        /// oninit to load any initial breadcrumbs.
        /// </summary>
        /// <param name="pageReference">The <see cref="Rock.Web.PageReference" />.</param>
        /// <returns>
        /// A <see cref="System.Collections.Generic.List{BreadCrumb}" /> of block related <see cref="Rock.Web.UI.BreadCrumb">BreadCrumbs</see>.
        /// </returns>
        public override List<BreadCrumb> GetBreadCrumbs( PageReference pageReference )
        {
            var breadCrumbs = new List<BreadCrumb>();

            int? applicationId = PageParameter( pageReference, PageParameterKey.SiteId ).AsIntegerOrNull();

            if ( applicationId != null )
            {
                var site = SiteCache.Get( applicationId.Value );

                if ( site != null )
                {
                    breadCrumbs.Add( new BreadCrumb( site.Name, pageReference ) );
                }
                else
                {
                    breadCrumbs.Add( new BreadCrumb( "New Application", pageReference ) );
                }
            }
            else
            {
                // don't show a breadcrumb if we don't have a pageparam to work with
            }

            return breadCrumbs;
        }

        /// <summary>
        /// Handles the BlockUpdated event of the control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void Block_BlockUpdated( object sender, EventArgs e )
        {
            ShowView();
        }

        /// <summary>
        /// Handles the Click event of the btnSave control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnSave_Click( object sender, EventArgs e )
        {
            var applicationId = PageParameter( PageParameterKey.SiteId ).AsInteger();

            var rockContext = new RockContext();
            var siteService = new SiteService( rockContext );
            var site = siteService.Get( applicationId );

            var additionalSettings = new AppleTvApplicationSettings();
            var isNewSite = false;

            // Site is new so create one
            if ( site == null )
            {
                site = new Site();
                siteService.Add( site );
                isNewSite = true;
            }
            else
            {
                additionalSettings = JsonConvert.DeserializeObject<AppleTvApplicationSettings>( site.AdditionalSettings );
            }

            site.Name = tbApplicationName.Text;
            site.Description = tbDescription.Text;
            site.IsActive = cbIsActive.Checked;
            site.SiteType = SiteType.Tv;

            additionalSettings.ApplicationScript = ceApplicationJavaScript.Text;
            additionalSettings.ApplicationStyles = ceApplicationStyles.Text;
            additionalSettings.TvApplicationType = TvApplicationType.AppleTv;

            // Login page
            site.LoginPageId = ppLoginPage.PageId;
            site.LoginPageRouteId = ppLoginPage.PageRouteId;

            // Create/Modify API Key
            additionalSettings.ApiKeyId =  SaveApiKey( additionalSettings.ApiKeyId, txtApiKey.Text, string.Format( "tv_application_{0}", site.Id ), rockContext );
            site.AdditionalSettings = additionalSettings.ToJson();

            rockContext.SaveChanges();

            // Create interaction channel for this site
            var interactionChannelService = new InteractionChannelService( rockContext );
            int channelMediumWebsiteValueId = DefinedValueCache.Get( Rock.SystemGuid.DefinedValue.INTERACTIONCHANNELTYPE_WEBSITE.AsGuid() ).Id;
            var interactionChannelForSite = interactionChannelService.Queryable()
                .Where( a => a.ChannelTypeMediumValueId == channelMediumWebsiteValueId && a.ChannelEntityId == site.Id ).FirstOrDefault();

            if ( interactionChannelForSite == null )
            {
                interactionChannelForSite = new InteractionChannel();
                interactionChannelForSite.ChannelTypeMediumValueId = channelMediumWebsiteValueId;
                interactionChannelForSite.ChannelEntityId = site.Id;
                interactionChannelService.Add( interactionChannelForSite );
            }

            interactionChannelForSite.Name = site.Name;
            interactionChannelForSite.RetentionDuration = nbPageViewRetentionPeriodDays.Text.AsIntegerOrNull();
            interactionChannelForSite.ComponentEntityTypeId = EntityTypeCache.Get<Rock.Model.Page>().Id;

            rockContext.SaveChanges();

            // If this is a new site then we also need to add a layout record and a 'default page'
            if ( isNewSite )
            {
                var layoutService = new LayoutService( rockContext );

                var layout = new Layout
                {
                    Name = "Homepage",
                    FileName = "Homepage.xaml",
                    Description = string.Empty,
                    SiteId = site.Id
                };

                layoutService.Add( layout );
                rockContext.SaveChanges();

                var pageService = new PageService( rockContext );
                var page = new Rock.Model.Page{
                    InternalName = "Start Screen",
                    BrowserTitle = "Start Screen",
                    PageTitle = "Start Screen",
                    DisplayInNavWhen = DisplayInNavWhen.WhenAllowed,
                    Description = string.Empty,
                    LayoutId = layout.Id,
                    Order = 0
                };

                pageService.Add( page );
                rockContext.SaveChanges();

                site.DefaultPageId = page.Id;
                rockContext.SaveChanges();
            }

            // If the save was successful, reload the page using the new record Id.
            var qryParams = new Dictionary<string, string>();
            qryParams[PageParameterKey.SiteId] = site.Id.ToString();

            NavigateToPage( RockPage.Guid, qryParams );
        }

        /// <summary>
        /// Handles the Click event of the btnCancel control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnCancel_Click( object sender, EventArgs e )
        {
            ShowView();
        }

        /// <summary>
        /// Handles the Click event of the btnEdit control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnEdit_Click( object sender, EventArgs e )
        {
            ShowEdit();
        }

        /// <summary>
        /// Handles the CheckedChanged event of the cbEnablePageViews control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void cbEnablePageViews_CheckedChanged( object sender, EventArgs e )
        {
            nbPageViewRetentionPeriodDays.Visible = cbEnablePageViews.Checked;
        }

        #endregion

        #region Methods

        private void ShowView()
        {
            pnlEdit.Visible = false;
            pnlView.Visible = true;

            // Show the page list block
            this.HideSecondaryBlocks( false );

            var applicationId = PageParameter( PageParameterKey.SiteId ).AsInteger();

            // We're being instructed to build a new site.
            if (applicationId == 0 )
            {
                ShowEdit();
                return;
            }

            var rockContext = new RockContext();
            var site = new SiteService( rockContext ).Get( applicationId );

            if ( site == null )
            {
                nbMessages.Text = "Could not find the application.";
            }

            pdAuditDetails.SetEntity( site, ResolveRockUrl( "~" ) );

            hlblInactive.Visible = !site?.IsActive ?? true;

            DescriptionList viewContent = new DescriptionList();

            viewContent.Add( "Description", site.Description );
            viewContent.Add( "Enable Page Views", site.EnablePageViews.ToString() );

            // Display the page view retention duration
            if ( site.EnablePageViews )
            {
                int channelMediumWebsiteValueId = DefinedValueCache.Get( Rock.SystemGuid.DefinedValue.INTERACTIONCHANNELTYPE_WEBSITE.AsGuid() ).Id;
                var retentionDuration = new InteractionChannelService( new RockContext() ).Queryable()
                        .Where( c => c.ChannelTypeMediumValueId == channelMediumWebsiteValueId && c.ChannelEntityId == site.Id )
                        .Select( c => c.RetentionDuration )
                        .FirstOrDefault();

                if (retentionDuration.HasValue)
                {
                    viewContent.Add( "Page View Retention", retentionDuration.Value.ToString() + " days" );
                }
            }

            // Get API key
            var additionalSettings = JsonConvert.DeserializeObject<AppleTvApplicationSettings>( site.AdditionalSettings );
            var apiKeyLogin = new UserLoginService( rockContext ).Get( additionalSettings.ApiKeyId ?? 0 );

            viewContent.Add( "API Key", apiKeyLogin != null ? apiKeyLogin.ApiKey : string.Empty );


            // Print the content to the screen
            lViewContent.Text = viewContent.Html;

            lBlockTitle.Text = site.Name;
        }

        /// <summary>
        /// Shows the details.
        /// </summary>
        private void ShowEdit()
        {
            pnlView.Visible = false;
            pnlEdit.Visible = true;
            pdAuditDetails.Visible = false;

            // Hide the page list block
            this.HideSecondaryBlocks( true );

            var applicationId = PageParameter( PageParameterKey.SiteId ).AsInteger();

            var rockContext = new RockContext();
            var site = new SiteService( rockContext ).Get( applicationId );

            if ( site != null )
            {
                hlblInactive.Visible = !site?.IsActive ?? true;
                lBlockTitle.Text = site.Name;

                tbApplicationName.Text = site.Name;
                tbDescription.Text = site.Description;

                cbIsActive.Checked = site.IsActive;

                var additionalSettings = JsonConvert.DeserializeObject<AppleTvApplicationSettings>( site.AdditionalSettings );

                ceApplicationJavaScript.Text = additionalSettings.ApplicationScript;
                ceApplicationStyles.Text = additionalSettings.ApplicationStyles;

                cbEnablePageViews.Checked = site.EnablePageViews;

                // Login Page
                if ( site.LoginPageRoute != null )
                {
                    ppLoginPage.SetValue( site.LoginPageRoute );
                }
                else
                {
                    ppLoginPage.SetValue( site.LoginPage );
                }

                // Set the API key
                var apiKeyLogin = new UserLoginService( rockContext ).Get( additionalSettings.ApiKeyId ?? 0 );
                txtApiKey.Text = apiKeyLogin != null ? apiKeyLogin.ApiKey : GenerateApiKey();

                // Get page view retention
                int channelMediumWebsiteValueId = DefinedValueCache.Get( Rock.SystemGuid.DefinedValue.INTERACTIONCHANNELTYPE_WEBSITE.AsGuid() ).Id;
                var retentionDuration = new InteractionChannelService( new RockContext() ).Queryable()
                        .Where( c => c.ChannelTypeMediumValueId == channelMediumWebsiteValueId && c.ChannelEntityId == site.Id )
                        .Select( c => c.RetentionDuration )
                        .FirstOrDefault();

                if ( retentionDuration.HasValue )
                {
                    nbPageViewRetentionPeriodDays.Text = retentionDuration.Value.ToString();
                }
                
                nbPageViewRetentionPeriodDays.Visible = site.EnablePageViews;
            }
            else
            {
                // Temporary v14 patch, we do this better in v14.1.
                ceApplicationJavaScript.Text = _defaultApplicationJs;
            }
        }

        #endregion


        #region Private Methods
        /// <summary>
        /// Generates the API key.
        /// </summary>
        /// <returns></returns>
        private string GenerateApiKey()
        {
            // Generate a unique random 12 digit api key
            return Rock.Utility.KeyHelper.GenerateKey( ( RockContext rockContext, string key ) => new UserLoginService( rockContext ).Queryable().Any( a => a.ApiKey == key ) );
        }

        /// <summary>
        /// Saves the API key.
        /// </summary>
        /// <param name="restLoginId">The rest login identifier.</param>
        /// <param name="apiKey">The API key.</param>
        /// <param name="userName">Name of the user.</param>
        /// <param name="rockContext">The rock context.</param>
        /// <returns></returns>
        private int SaveApiKey( int? restLoginId, string apiKey, string userName, RockContext rockContext )
        {
            var userLoginService = new UserLoginService( rockContext );
            var personService = new PersonService( rockContext );
            UserLogin userLogin = null;
            Person restPerson = null;

            // the key gets saved in the api key field of a user login (which you have to create if needed)
            var entityType = new EntityTypeService( rockContext )
                .Get( "Rock.Security.Authentication.Database" );

            if ( restLoginId.HasValue )
            {
                userLogin = userLoginService.Get( restLoginId.Value );
                restPerson = userLogin.Person;
            }
            else
            {
                restPerson = new Person();
                personService.Add( restPerson );
            }

            // the rest user name gets saved as the last name on a person
            restPerson.LastName = tbApplicationName.Text;
            restPerson.RecordTypeValueId = DefinedValueCache.Get( Rock.SystemGuid.DefinedValue.PERSON_RECORD_TYPE_RESTUSER.AsGuid() ).Id;
            restPerson.RecordStatusValueId = DefinedValueCache.Get( Rock.SystemGuid.DefinedValue.PERSON_RECORD_STATUS_ACTIVE.AsGuid() ).Id;

            rockContext.SaveChanges();

            if ( userLogin == null )
            {
                userLogin = new UserLogin();
                userLoginService.Add( userLogin );
            }

            userLogin.UserName = userName;
            userLogin.IsConfirmed = true;
            userLogin.ApiKey = apiKey;
            userLogin.EntityTypeId = entityType.Id;
            userLogin.PersonId = restPerson.Id;

            rockContext.SaveChanges();

            return userLogin.Id;
        }
        #endregion

    }
}