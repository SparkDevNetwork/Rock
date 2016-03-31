<%@ Control Language="C#" AutoEventWireup="true" CodeFile="InstagramBlock.ascx.cs" Inherits="RockWeb.Plugins.com_centralaz.Widgets.InstagramBlock" %>
<asp:UpdatePanel ID="upnlContent" runat="server">
    <ContentTemplate>
        <div class="instawrapper">   
        </div>
    </ContentTemplate>
</asp:UpdatePanel>

<script>
    var userid = '<%= GetAttributeValue( "InstagramUserID" )%>';
    var clientid = '<%= GetAttributeValue( "InstagramClientID" )%>';
    var gridsize = '<%= GetAttributeValue( "PictureGridSize" )%>';
    var imageres = '<%= GetAttributeValue( "ImageResolution" )%>';
    var minimageheight = '<%= GetAttributeValue( "MinimumImageHeight" )%>' + "px";
 
    fetchMedia(clientid, userid, function (response) {
        var index = 0;
        var div = document.querySelector('.instawrapper')
        switch (gridsize) {
            case "medium":
                for (var row = 0; row < 3; row++) {
                    // create the row div
                    var rowdiv = document.createElement('div');
                    rowdiv.className = "row";

                    // loop over three to build three columns
                    for (var col = 0; col < 3; col++) {

                        //create a single div column that contains image and link
                        var coldiv = document.createElement('div'),
                            anchor = document.createElement('a'),
                            bckgrndimg = document.createElement('bg-image');

                        //get item from the array
                        var item = response.data[index];

                        anchor.href = item.link;
                        anchor.target = '_blank';
                        switch (imageres) {
                            case "thumbnail":
                                bckgrndimg.src = item.images.thumbnail.url;
                                break;
                            case "low_resolution":
                                bckgrndimg.src = item.images.low_resolution.url;
                                break;
                            case "standard_resolution":
                                bckgrndimg.src = item.images.standard_resolution.url;
                                break;
                        }

                        coldiv.className = "col-xs-4";
                        coldiv.style.paddingLeft = 0;
                        coldiv.style.paddingRight = 0;
                        coldiv.style.backgroundImage = "url(" + bckgrndimg.src + ")";
                        coldiv.style.backgroundPosition = "50% 50%";
                        coldiv.style.backgroundSize = "cover";
                        coldiv.style.backgroundRepeat = "no-repeat";
                        coldiv.style.minHeight = minimageheight;

                        anchor.appendChild(coldiv);
                        rowdiv.appendChild(anchor);
                       
                        index++;
                    }
                    // end the row div
                    div.appendChild(rowdiv);
                }
                break;

            case "small":
                for (var row = 0; row < 2; row++) {
                    // create the row div
                    var rowdiv = document.createElement('div');
                    rowdiv.className = "row";

                    // loop over three to build three columns
                    for (var col = 0; col < 2; col++) {

                        //create a single div column that contains image and link
                        var coldiv = document.createElement('div'),
                            anchor = document.createElement('a'),
                            bckgrndimg = document.createElement('bg-image');

                        //get item from the array
                        var item = response.data[index];

                        anchor.href = item.link;
                        anchor.target = '_blank';
                        switch (imageres) {
                            case "thumbnail":
                                bckgrndimg.src = item.images.thumbnail.url;
                                break;
                            case "low_resolution":
                                bckgrndimg.src = item.images.low_resolution.url;
                                break;
                            case "standard_resolution":
                                bckgrndimg.src = item.images.standard_resolution.url;
                                break;
                        }

                        coldiv.className = "col-xs-6";
                        coldiv.style.paddingLeft = 0;
                        coldiv.style.paddingRight = 0;
                        coldiv.style.backgroundImage = "url(" + bckgrndimg.src + ")";
                        coldiv.style.backgroundPosition = "50% 50%";
                        coldiv.style.backgroundSize = "cover";
                        coldiv.style.backgroundRepeat = "no-repeat";
                        coldiv.style.minHeight = minimageheight;

                        anchor.appendChild(coldiv);
                        rowdiv.appendChild(anchor);

                        index++;
                    }
                    // end the row div
                    div.appendChild(rowdiv);
                }               
                break;

            case "single":
                for (var row = 0; row < 1; row++) {
                    // create the row div
                    var rowdiv = document.createElement('div');
                    rowdiv.className = "row";

                    // loop over three to build three columns
                    for (var col = 0; col < 1; col++) {

                        //create a single div column that contains image and link
                        var coldiv = document.createElement('div'),
                            anchor = document.createElement('a'),
                            bckgrndimg = document.createElement('bg-image');

                        //get item from the array
                        var item = response.data[index];

                        anchor.href = item.link;
                        anchor.target = '_blank';
                        switch (imageres) {
                            case "thumbnail":
                                bckgrndimg.src = item.images.thumbnail.url;
                                break;
                            case "low_resolution":
                                bckgrndimg.src = item.images.low_resolution.url;
                                break;
                            case "standard_resolution":
                                bckgrndimg.src = item.images.standard_resolution.url;
                                break;
                        }

                        coldiv.className = "col-xs-12";
                        coldiv.style.paddingLeft = 0;
                        coldiv.style.paddingRight = 0;
                        coldiv.style.backgroundImage = "url(" + bckgrndimg.src + ")";
                        coldiv.style.backgroundPosition = "50% 50%";
                        coldiv.style.backgroundSize = "cover";
                        coldiv.style.backgroundRepeat = "no-repeat";
                        coldiv.style.minHeight = minimageheight;

                        anchor.appendChild(coldiv);
                        rowdiv.appendChild(anchor);

                        index++;
                    }
                    // end the row div
                    div.appendChild(rowdiv);
                }              
                break;
        }

    });

    function fetchMedia(clientId, userId, callback) {
        var jsonUrl = 'https://api.instagram.com/v1/users/' + userId + '/media/recent/?client_id=' + clientId + '&callback=onMediaFetched',
            script = document.createElement('script');

        window.onMediaFetched = callback;
        script.src = jsonUrl;
        document.head.appendChild(script);

        setTimeout(function () {
            document.head.removeChild(script);
            window.onMediaFetched = null;
        }, 1000);
    }
</script>