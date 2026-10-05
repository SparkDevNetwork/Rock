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
// Chat's web push worker. The browser runs it outside every page, so a push reaches the person
// with no chat tab open. It is registered with this folder as its scope, never the site root, so
// it can never replace a worker a church's own theme registers there; it needs no control of the
// chat page, since it reaches every window of the site through clients.matchAll.
//
// It shows only what the push carries: the platform writes every word of the notification, so a
// released copy of this file never has to be changed to change what a person sees.
(function () {
    "use strict";

    /** How long a focused window has to say a channel is on screen, in ms; an estimate. */
    var onScreenWaitMs = 250;

    /**
     * The chat page this worker opens on a tap, from the query string it was registered with. Only
     * a path on this site is kept, so a tap can never open a page somewhere else.
     */
    var chatPage = (function () {
        try {
            var page = new URL(new URL(self.location.href).searchParams.get("page") || "/", self.location.origin);
            return page.origin === self.location.origin ? page.pathname : "/";
        }
        catch (e) {
            return "/";
        }
    })();

    self.addEventListener("install", function () {
        self.skipWaiting();
    });

    self.addEventListener("activate", function (event) {
        event.waitUntil(self.clients.claim());
    });

    /**
     * Asks one window whether a channel is on screen. Resolves false when it does not answer in
     * time, so a page that is busy or not chat costs at most the wait and the banner still shows.
     */
    function askIsOpen(windowClient, channelId) {
        return new Promise(function (resolve) {
            var channel = new MessageChannel();
            var timer = setTimeout(function () {
                resolve(false);
            }, onScreenWaitMs);

            channel.port1.onmessage = function (event) {
                clearTimeout(timer);
                resolve(!!(event.data && event.data.open === true));
            };

            windowClient.postMessage({ type: "chat.push.is-open", channel_id: channelId }, [channel.port2]);
        });
    }

    /**
     * Whether the message is already on screen in a window the person is looking at. Only a
     * message that appears in its channel can be; a reply kept to its thread always shows.
     */
    async function isOnScreen(data) {
        if (data.in_channel !== "true") {
            return false;
        }

        var windows = await self.clients.matchAll({ type: "window", includeUncontrolled: true });
        var focused = windows.filter(function (w) {
            return w.focused;
        });

        if (focused.length === 0) {
            return false;
        }

        var answers = await Promise.all(focused.map(function (w) {
            return askIsOpen(w, data.channel_id);
        }));

        return answers.indexOf(true) !== -1;
    }

    async function onPush(event) {
        var payload = {};

        try {
            payload = event.data ? event.data.json() : {};
        }
        catch (e) {
            // A push that is not ours or not JSON still has to show something, or Chrome shows
            // its own notice in its place.
            payload = {};
        }

        var notification = payload.notification || {};
        var data = payload.data || {};

        // Only a mention push carries the count, and it is the person's whole unread mention count,
        // so it replaces the badge; any other push leaves the badge as it is.
        if (data.badge !== undefined && self.navigator && typeof self.navigator.setAppBadge === "function") {
            try {
                await self.navigator.setAppBadge(Number(data.badge));
            }
            catch (e) {
                // The badge is decoration; a browser that refuses it still shows the push.
            }
        }

        if (await isOnScreen(data)) {
            return;
        }

        var options = {
            body: notification.body || "",
            // the message id, so an edit's mention push replaces the first banner
            tag: data.message_id,
            data: data
        };

        if (data.face) {
            options.icon = data.face;
        }

        await self.registration.showNotification(notification.title || "", options);
    }

    async function onClick(event) {
        event.notification.close();

        var data = event.notification.data || {};
        var windows = await self.clients.matchAll({ type: "window", includeUncontrolled: true });
        var chatWindow = windows.find(function (w) {
            return new URL(w.url, self.location.href).pathname === chatPage;
        });

        // A worker cannot navigate a window it does not control, so an open chat window is
        // focused and told which channel to open.
        if (chatWindow) {
            await chatWindow.focus();
            chatWindow.postMessage({ type: "chat.push.open", channel_id: data.channel_id });
            return;
        }

        await self.clients.openWindow(chatPage + "?ChannelGuid=" + encodeURIComponent(data.channel_id || ""));
    }

    self.addEventListener("push", function (event) {
        event.waitUntil(onPush(event));
    });

    self.addEventListener("notificationclick", function (event) {
        event.waitUntil(onClick(event));
    });
})();
