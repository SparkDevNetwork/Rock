<%@ Control Language="C#" AutoEventWireup="true" CodeFile="MediaBlock.ascx.cs" Inherits="RockWeb.Plugins.com_centralaz.Widgets.MediaBlock" %>
<script>

    (function (root, factory) {
        if (typeof define === 'function' && define.amd) {
            // AMD. Register as an anonymous module.
            define([], factory);
        } else if (typeof exports === 'object') {
            // Node. Does not work with strict CommonJS, but
            // only CommonJS-like environments that support module.exports,
            // like Node.
            module.exports = factory();
        } else {
            // Browser globals.
            factory();
        }
    }(this, function () {
        var domNode = '';
        var maxTweets = 20;
        var queue = [];
        var inProgress = false;
        var supportsClassName = true;
        var lang = 'en';
        var script = null;

        function getElementsByClassName(node, classname) {
            var a = [];
            var regex = new RegExp('(^| )' + classname + '( |$)');
            var elems = node.getElementsByTagName('*');
            for (var i = 0, j = elems.length; i < j; i++) {
                if (regex.test(elems[i].className)) {
                    a.push(elems[i]);
                }
            }
            return a;
        }

        function extractImageUrl(image_data) {
            if (image_data !== undefined) {
                var data_src = image_data.innerHTML.match(/data-srcset="([A-z0-9%_\.-]+)/i)[0];
                return decodeURIComponent(data_src).split('"')[1];
            }
        }

        //Grabs the Twitter Images
        var twitterFetcher = {
            fetch: function (config) {
                config.maxTweets = <%= Int32.Parse(GetAttributeValue( "MaximumNumberOfTweets" )) %>;

                if (inProgress) {
                    queue.push(config);
                } else {
                    inProgress = true;
                    domNode = config.domId;
                    maxTweets = config.maxTweets;

                    var head = document.getElementsByTagName('head')[0];
                    if (script !== null) {
                        head.removeChild(script);
                    }
                    script = document.createElement('script');
                    script.type = 'text/javascript';
                    script.src = 'https://cdn.syndication.twimg.com/widgets/timelines/' +
                        config.id + '?&lang=' + (config.lang || lang) +
                        '&callback=twitterFetcher.callback&' +
                        'suppress_response_codes=true&rnd=' + Math.random();
                    head.appendChild(script);
                }
            },
            callback: function (data) {
                var div = document.createElement('div');
                div.innerHTML = data.body;
                if (typeof (div.getElementsByClassName) === 'undefined') {
                    supportsClassName = false;
                }

                var tweets = [];
                var images = [];
                var rts = [];
                var tids = [];
                var x = 0;

                if (supportsClassName) {
                    var tmp = div.getElementsByClassName('tweet');
                    while (x < tmp.length) {
                        if (tmp[x].getElementsByClassName('retweet-credit').length > 0) {
                            rts.push(true);
                        } else {
                            rts.push(false);
                        }
                        if (!rts[x]) {
                            tweets.push(tmp[x].getElementsByClassName('e-entry-title')[0]);
                            tids.push(tmp[x].getAttribute('data-tweet-id'));
                            if (tmp[x].getElementsByClassName('inline-media')[0] !== undefined) {
                                images.push(tmp[x].getElementsByClassName('inline-media')[0]);
                            } else {
                                images.push(undefined);
                            }
                        }
                        x++;
                    }
                } else {
                    var tmp = getElementsByClassName(div, 'tweet');
                    while (x < tmp.length) {
                        tweets.push(getElementsByClassName(tmp[x], 'e-entry-title')[0]);
                        tids.push(tmp[x].getAttribute('data-tweet-id'));
                        if (getElementsByClassName(tmp[x], 'inline-media')[0] !== undefined) {
                            images.push(getElementsByClassName(tmp[x], 'inline-media')[0]);
                        } else {
                            images.push(undefined);
                        }
                        x++;
                    }
                }

                if (tweets.length > maxTweets) {
                    tweets.splice(maxTweets, (tweets.length - maxTweets));
                    images.splice(maxTweets, (images.length - maxTweets));
                }

                var arrayTweets = [];
                var x = tweets.length;
                var n = 0;
                while (n < x) {
                    if (images[n] !== undefined) {
                        var tweetInfo = {
                            Caption: tweets[n].textContent,
                            Link: "https://twitter.com/"+"<%= GetAttributeValue( "TwitterUsername" )%>"+"/status/" + tids[n],
                            ImageLink: extractImageUrl(images[n])
                        }
                        arrayTweets.push(tweetInfo);
                    }

                    n++;
                }
                getInstagramImages(arrayTweets);
                inProgress = false;

                if (queue.length > 0) {
                    twitterFetcher.fetch(queue[0]);
                    queue.splice(0, 1);
                }
            }
        };

        // Authenticates Instagram
        function authenticateInstagram() {
            // This is my private client id. You'll need to create one for Central
            // and change it here. You will probably need to make this into a
            // block setting.
            var clientId ='<%= GetAttributeValue( "InstagramClientId" )%>',

            // This is the "user id" for centralchristianaz in instagram.com. It's
            // not immediately obvious how to get at this data, since they hide
            // it so well. I was able to get it with this URL:
            // https://api.instagram.com/v1/users/search?q=[USERNAME]&client_id=[CLIENT ID]
                userId = '<%= GetAttributeValue( "InstagramUserId" )%>';

            return $.ajax({
                url: 'https://api.instagram.com/v1/users/' + userId + '/media/recent/?client_id=' + clientId,
                dataType: 'jsonp'
            });

        }

        // Gets the instagram images
        function getInstagramImages(twitterImages) {
            var promise = authenticateInstagram();
            promise.done(function (response) {
                var arrayInstas = [];
                $.each(response.data, function (index, item) {
                    if (item.caption == null) {
                        var InstagramInfo = {
                            Link: item.link,
                            ImageLink: item.images.low_resolution.url,
                            Caption: ""
                        }
                        arrayInstas.push(InstagramInfo);
                    }
                    else {
                        var InstagramInfo = {
                            Link: item.link,
                            ImageLink: item.images.low_resolution.url,
                            Caption: item.caption.text || ""
                        }
                        arrayInstas.push(InstagramInfo);
                    }

                });
                mergeImageSources(twitterImages, arrayInstas);
            });
        }

        //Merge the two image arrays
        function mergeImageSources(twitterImages, instaImages) {
            var allImages = twitterImages.concat(instaImages);
            handleImages(allImages);
        }

        //Output to html
        function handleImages(allImages) {

            var x = <%= GetAttributeValue( "NumberOfImages" ) %>;
            var n = 0;
            var pixels = '<%= GetAttributeValue( "PixelSize" )%>';
            var element = document.getElementById(domNode);
            var html = '<ul>';
            while (n < x) {
                html += '<li style="width: '+pixels+'; height: '+pixels+';" ><a class="thumb" href="' + allImages[n].Link + '" style="background-image: url(' + allImages[n].ImageLink + ');"><div class="image-caption">' + allImages[n].Caption + '</div></a></li>';
                n++;
            }
            html += '</ul>';
            element.innerHTML = html;
        }

        // It must be a global variable because it will be called by JSONP.
        window.twitterFetcher = twitterFetcher;

        return twitterFetcher;
    }));

    Sys.Application.add_load(function () {
        var config1 = {
            "id": '<%= GetAttributeValue( "TwitterWidgetId" )%>',
            "domId": 'divPhotoGrid',
        };
        twitterFetcher.fetch(config1);

    });
</script>
<style>
    .image-caption {
        padding: 25px;
        width: 100%;
        background: rgba(0,0,0,0.4);
        position: absolute;
        bottom: 0px;
        padding-left: 0px;
        color: white;
        font-family: Gotham;
        font: bold;
        margin-left: 5px;
        font-size: large;
    }

    .thumb {
        width: 380px;
        height: 380px;
        background-position: center center;
        background-size: cover;
    }
</style>
<asp:UpdatePanel ID="upnlContent" runat="server">
    <ContentTemplate>

        <div id="divPhotoGrid" class="ri-grid" >
        </div>

    </ContentTemplate>
</asp:UpdatePanel>

