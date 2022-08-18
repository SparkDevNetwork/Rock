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
            var alert = RockTvApp.createAlertDocument("Error", `Was not able to load the page. \n Page: ${RockTvApp.AppState.HomepageGuid}`);
            return alert;
        }

        // Render in merge fields
        if (mergeFields) {
            console.log("[INFO] Parsing template \n" + content);
            content = content.replace(/\{\s*(\w+)\s*}/g, (_, v) => (typeof mergeFields[v] != 'undefined') ? mergeFields[v] : '');
        }

        // Parse the resulting string content to an XML document
        let templateParser = new DOMParser();

        let doc = null;
        try {
            doc = templateParser.parseFromString(content, "application/xml");
        }
        catch (err) {
            console.log("[ERROR] An error occurred parsing the document.", err, content);
            var alert = RockTvApp.createAlertDocument("Error", `An error occurred parsing the content provided.`);
            return alert;
        }

        if (doc) {
            return doc;
        }

        // The document was not able to be parsed so show alert
        var alert = RockTvApp.createAlertDocument("Error", `An invalid page was returned. \n Page: ${RockTvApp.AppState.HomepageGuid}`);
        return alert;
    }

    // Gets' the loading document from the application's loading template
    getLoadingDocument() {
        if (this._loadingTemplate) {
            return this._loadingTemplate;
        }

        let templateParser = new DOMParser();
        this._loadingTemplate = templateParser.parseFromString(loadingTemplate, "application/xml");
        return this._loadingTemplate;
    }

    // Adds global handler if requested
    _applyHandler(doc, useHandler) {
        if (useHandler) {
            doc.addEventListener("select", this.handler.handleEvent);
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
        console.log("RBF: Point 1");
        // Check that the end time is not zero. This can be the case if the page has been unloaded and there is
        // one more tick of the clock
        if (this.endTime == 0) {
            return;
        }
        console.log("RBF: Point 2", this.authCode);
        // Check the server to see if the authenication has occurred
        var isAuthComplete = await appCommands.checkForAuthentication(this.authCode);
        console.log("RBF: Point 3", isAuthComplete);
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
        console.log("[INFO] Request to start polling sent.");
        this.checkCondition();
    }

    // Stops the clock
    stopPolling() {
        this.endTime = 0;
        console.log("[INFO] Stopping the polling timer.");
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
    }

    handleEvent(event) {
        var element = event.target;

        // Get command type
        var commandType = element.getAttribute("rockCommand");

        // Several commands can be specified as a comma separated string.
        var commands = commandType.split(",").map(function (item) {
            return item.trim();
        });

        console.log("[INFO] Processing Command", commands, element);

        // Process each command
        for (const command of commands) {

            switch (command) {

                case "pushPage":
                case "replacePage":
                case "presentModal":
                    {
                        this.processNavigationEvent(command, element);
                        break;
                    }
                case "popPage":
                    {
                        RockTvApp.NavigationStack.popDocument();
                        break;
                    }
                case "clearNavigationStack":
                    {
                        RockTvApp.NavigationStack.clear();
                        break;
                    }
                case "dismissModal":
                    {
                        RockTvApp.NavigationStack.dismissModal();
                        break;
                    }
                case "playVideo":
                    {
                        this.processPlayVideo(element);
                        break;
                    }
                case "playAudio":
                    {
                        this.processPlayAudio(element);
                        break;
                    }
                case "setContext":
                    {
                        let key = element.getAttribute("rockContextKey");
                        let value = element.getAttribute("rockContextValue");

                        appCommands.setContextValue(key, value);
                        break;
                    }
                case "clearContext":
                    {
                        let key = element.getAttribute("rockContextKey");

                        appCommands.clearContext(key);
                        break;
                    }
                case "showDemo":
                    {
                        if (RockTvApp.AppState.DemoModeEnabled) {
                            var demoDocument = RockTvApp.createDemoDocument();
                            RockTvApp.NavigationStack.pushDocument(demoDocument);
                        }
                        else {
                            var alert = RockTvApp.createAlertDocument("Demo Mode", "Demo is not supported on this application.");
                            RockTvApp.NavigationStack.pushDocument(alert);
                        }

                        break;
                    }
                case "updateDemo":
                    {
                        this.updateDemoSettings();
                        break;
                    }
                case "clearDemo":
                    {
                        this.clearDemoSettings();
                        break;
                    }
                case "login":
                    {
                        this.processLogin(element);
                        break;
                    }
                case "logout":
                    {
                        this.processLogout(element);
                        break;
                    }
                default:
                    {
                        console.log(`[WARNING] Unrecognized command encountered. The command "${commandType}" was not found.`, event);
                        break;
                    }
            }
        }
    }

    // processesLoginCommand
    async processLogin(element) {

        // Get configuration
        let loginPageGuid = element.getAttribute("rockLoginPageGuid") ? element.getAttribute("rockLoginPageGuid") : '';
        let loginTimeoutPageGuid = element.getAttribute("rockLoginTimeoutPageGuid") ? element.getAttribute("rockLoginTimeoutPageGuid") : '';
        let loginSuccessPageGuid = element.getAttribute("rockLoginSuccessPageGuid") ? element.getAttribute("rockLoginSuccessPageGuid") : '';

        let loginTimeoutDuration = element.getAttribute("rockLoginTimeoutDuration") ? element.getAttribute("rockLoginTimeoutDuration") : 600;
        let loginCheckDuration = element.getAttribute("rockLoginCheckDuration") ? element.getAttribute("rockLoginCheckDuration") : 5;
        let loginClearNavigationStack = element.getAttribute("rockLoginClearNavigationStack") ? element.getAttribute("rockLoginClearNavigationStack") : true;

        // Make call to get accessToken
        var authResponse = await appCommands.getLoginAuthCode();
        console.log("[INFO] Authenication Session Started", authResponse);

        // start long polling
        RockTvApp.LoginHandler = new LoginHandler(loginTimeoutDuration, loginCheckDuration, authResponse.authCode, loginTimeoutPageGuid, loginSuccessPageGuid, loginClearNavigationStack);
        RockTvApp.LoginHandler.startPolling();

        // Add spaces bettwen letters for better readability
        authResponse.authCode = authResponse.authCode.split('').join(' ');

        // Show login screen
        var doc = await RockTvApp.NavigationStack.getDocumentFromUrl(loginPageGuid, authResponse, "Private");

        // Add a call back on unload so we can stop the polling if needed
        doc.addEventListener("unload", this.cancelLogin);

        RockTvApp.NavigationStack.pushDocument(doc);
    }

    // Cancels the login polling process
    cancelLogin() {
        RockTvApp.LoginHandler.stopPolling();
        RockTvApp.LoginHandler = null;

        console.log("[INFO] Canceling the login process has the individual has unload the page.");
    }

    // Logs the individual out
    async processLogout(element) {

        // Get configuration
        let logoutPageGuid = element.getAttribute("rockLogoutPageGuid") ? element.getAttribute("rockLogoutPageGuid") : '';
        let logoutClearNavigationStack = element.getAttribute("rockLogoutClearNavigationStack") ? element.getAttribute("rockLogoutClearNavigationStack") : true;

        // Call the logout command in C#
        appCommands.logout();

        // Clear navigation stack if needed
        if (logoutClearNavigationStack) {
            RockTvApp.NavigationStack.clear();
        }

        // Load the logout page
        var doc = await RockTvApp.NavigationStack.getDocumentFromUrl(logoutPageGuid);
        RockTvApp.NavigationStack.pushDocument(doc);

        console.log("[INFO] Logged out individual.");
    }

    // Updates the demo settings from the demo screen
    async updateDemoSettings() {

        var doc = navigationDocument.documents[navigationDocument.documents.length - 1];
        var textField = doc.getElementById("demoLookupInput");

        var demoKey = textField.getFeature("Keyboard").text;

        await appCommands.loadDemoPacket(demoKey);

        var alert = RockTvApp.createAlertDocument("Demo Settings Updated", "Demo settings have been saved. Please restart the application to re-load the settings.");

        // Clear the navigation stack
        RockTvApp.NavigationStack.clear();

        // Show the restart app screen
        RockTvApp.NavigationStack.pushDocument(alert, false);
    }

    // Clears the demo settings from the demo screen
    clearDemoSettings(element) {
        let newHomepage = appCommands.clearDemoSettings();

        var alert = RockTvApp.createAlertDocument("Demo Settings Cleared", "Demo settings have been cleared. Please restart the application to re-load the application.");

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
            url = element.getAttribute("rockAudioUrl");
        }
        else {
            url = element.getAttribute("rockVideoUrl");
        }

        // URL decode the url
        url = decodeURIComponent(url);

        console.log(`[INFO] Media URL: ${url}`);

        let mediaElementGuid = element.getAttribute("rockVideoMediaElementGuid") ? element.getAttribute("rockVideoMediaElementGuid") : '';
        let relatedEntityTypeId = element.getAttribute("rockVideoRelatedEntityTypeId") ? element.getAttribute("rockVideoRelatedEntityTypeId") : '';
        let relatedEntityId = element.getAttribute("rockVideoRelatedEntityId") ? element.getAttribute("rockVideoRelatedEntityId") : '';
        let interactionGuid = element.getAttribute("rockInteractionGuid") ? element.getAttribute("rockInteractionGuid") : '';
        let watchMap = element.getAttribute("rockWatchMap") ? element.getAttribute("rockWatchMap") : '';

        let enableResume = element.getAttribute("rockVideoEnableResume") ? element.getAttribute("rockVideoEnableResume") : true;
        let title = element.getAttribute("rockVideoTitle");
        let subtitle = element.getAttribute("rockVideoSubtitle");
        let description = element.getAttribute("rockVideoDescription");
        let artworkImageURL = element.getAttribute("rockVideoArtworkImageURL");

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
        player.addEventListener("stateDidChange", this.handleMediaStateChange);
        player.addEventListener("timeDidChange", this.handleMediaTimeChange, { interval: 1 });

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
        let url = element.getAttribute("rockPageGuid");
        let cacheControl = element.getAttribute("rockPageCacheControl") ? element.getAttribute("rockPageCacheControl") : false;
        let showLoading = element.getAttribute("rockPageShowLoading") ? element.getAttribute("rockPageShowLoading") : false;
        let suppressInteraction = element.getAttribute("rockPageSuppressInteraction") ? element.getAttribute("rockPageSuppressInteraction") : false;
        let mergeFields = element.getAttribute("rockMergeFields");

        switch (command) {
            case "pushPage":
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
            case "replacePage":
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
            case "presentModal":
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
    var homepage = await RockTvApp.NavigationStack.getDocumentFromUrl(RockTvApp.AppState.HomepageGuid, null, "Personal:600", false); // TODO should make cache a setting 

    if (homepage) {
        // Present the homepage
        RockTvApp.NavigationStack.pushDocument(homepage);
    }
    else {
        // Show an error message
        var alert = RockTvApp.createAlertDocument("Error", `An error occurred loading hompage. \n Page: ${RockTvApp.AppState.HomepageGuid}`);
        RockTvApp.NavigationStack.pushDocument(alert);
    }
}
