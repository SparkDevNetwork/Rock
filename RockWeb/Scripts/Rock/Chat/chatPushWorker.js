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
            // A path that starts with two slashes, however the page spelled it (/.//host, /..//host, this-site//host),
            // opens as another host, so it is refused like another origin.
            var isOnSite = page.origin === self.location.origin && page.pathname.indexOf("//") !== 0;
            return isOnSite ? page.pathname : "/";
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
     * Asks one window whether a channel is on screen, or with a root, whether that thread is.
     * Resolves false when it does not answer in time, so a page that is busy or not chat costs at
     * most the wait and the banner still shows.
     */
    function askIsOpen(windowClient, channelId, rootId) {
        return new Promise(function (resolve) {
            var channel = new MessageChannel();

            // The worker's end is closed once the ask is settled either way, so a worker woken by
            // many pushes holds no open ports and a late answer goes nowhere.
            var timer = setTimeout(function () {
                channel.port1.close();
                resolve(false);
            }, onScreenWaitMs);

            channel.port1.onmessage = function (event) {
                clearTimeout(timer);
                channel.port1.close();
                resolve(!!(event.data && event.data.open === true));
            };

            var ask = { type: "chat.push.is-open", channel_id: channelId };

            // A root's ask stays exactly as it was, so a page that predates threads still answers it.
            if (rootId) {
                ask.root_id = rootId;
            }

            windowClient.postMessage(ask, [channel.port2]);
        });
    }

    /**
     * Whether the message is already on screen in a window the person is looking at: a root or a
     * reply shown in the channel when its channel is, and any reply when its thread's drawer is.
     */
    async function isOnScreen(data) {
        var windows = await self.clients.matchAll({ type: "window", includeUncontrolled: true });
        var focused = windows.filter(function (w) {
            return w.focused;
        });

        if (focused.length === 0) {
            return false;
        }

        // A reply kept to its thread is seen only in the drawer, so only its thread is asked about;
        // one shown in the channel is seen in either, so both are asked at once and share the wait.
        var isReply = !!data.parent_id;
        var isInChannel = !isReply || data.in_channel === "true";
        var asks = [];

        focused.forEach(function (w) {
            if (isInChannel) {
                asks.push(askIsOpen(w, data.channel_id));
            }

            if (isReply) {
                asks.push(askIsOpen(w, data.channel_id, data.parent_id));
            }
        });

        var answers = await Promise.all(asks);

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
            // The push's own tag when it carries one, else the message id, so an edit's mention push
            // replaces the first banner. The platform chooses, so a new kind of push needs no new worker.
            tag: data.tag || data.message_id,
            data: data
        };

        // A replaced banner is silent unless asked otherwise; a push that is news each time asks.
        if (data.renotify === "true") {
            options.renotify = true;
        }

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

        // Any push about a reply, a reaction to one included, opens the reply in its thread; the
        // parent is always a root, since a reply answers only a root.
        var rootId = data.parent_id || "";
        var open = { type: "chat.push.open", channel_id: data.channel_id };

        if (rootId) {
            open.root_id = rootId;
            open.message_id = data.message_id;
        }

        // A worker cannot navigate a window it does not control, so an open chat window is
        // focused and told what to open.
        if (chatWindow) {
            await chatWindow.focus();
            chatWindow.postMessage(open);
            return;
        }

        var url = chatPage + "?ChannelGuid=" + encodeURIComponent(data.channel_id || "");

        if (rootId) {
            url += "&ThreadRoot=" + encodeURIComponent(rootId) + "&Message=" + encodeURIComponent(data.message_id || "");
        }

        await self.clients.openWindow(url);
    }

    self.addEventListener("push", function (event) {
        event.waitUntil(onPush(event));
    });

    self.addEventListener("notificationclick", function (event) {
        event.waitUntil(onClick(event));
    });
})();
