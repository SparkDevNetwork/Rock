<%@ Page Language="C#" MasterPageFile="Site.Master" AutoEventWireup="true" Inherits="Rock.Web.UI.RockPage" %>

<asp:Content ID="ctMain" ContentPlaceHolderID="main" runat="server">

    
    <%--<script src="https://code.getmdl.io/1.1.3/material.min.js"></script>--%>

    <script src="/Themes/NewPointeLive/Scripts/firebase-chat.js"></script>

    <link rel="stylesheet" href="https://fonts.googleapis.com/icon?family=Material+Icons">
    <link rel="stylesheet" href="/Themes/NewPointeLive/Styles/material-orange.css" />
    

    <link rel="stylesheet" href="/Themes/NewPointeLive/Styles/firebase-chat.css" />

    <div class="container-fluid">
        <Rock:Zone Name="Full-Width Feature" runat="server"></Rock:Zone>
    </div>

    <main class="container">

        <!-- Start Content Area -->


        <!-- Ajax Error -->
        <div class="alert alert-danger ajax-error" style="display: none">
            <p><strong>Error</strong></p>
            <span class="ajax-error-message"></span>
        </div>

        <div class="row">
            <div class="col-md-12">
                <Rock:Zone Name="Feature" runat="server" />
            </div>
        </div>

        <div class="row">
            <div class="col-md-6">
                <Rock:Zone Name="Main" runat="server" />
            </div>
            <div class="col-md-6 live-tabs">
                <ul class="nav nav-tabs" role="tablist">
                    <li class="active"><a href="#livechat" data-toggle="tab"><i class="fa fa-comments-o"></i></a></li>
                    <li><a href="#notes" data-toggle="tab"><i class="fa fa-sticky-note-o"></i></a></li>
                    <li><a href="#calendar" data-toggle="tab"><i class="fa fa-calendar"></i></a></li>
                </ul>

                <div class="tab-content">
                    <div class="tab-pane vert-tab active" id="livechat">
                        <Rock:Zone Name="Live Chat" runat="server" />
                    </div>
                    <div class="tab-pane vert-tab" id="notes">
                        <Rock:Zone Name="Notes" runat="server" />
                    </div>
                    <div class="tab-pane vert-tab active" id="calendar">
                        <Rock:Zone Name="Calendar" runat="server" />
                    </div>
                </div>
                <Rock:Zone Name="Sidebar 1" runat="server" />
            </div>
        </div>

        <!-- End Content Area -->


    </main>
    <script src="https://code.getmdl.io/1.1.3/material.min.js"></script>
    <script src="https://www.gstatic.com/firebasejs/live/3.0/firebase.js"></script>

    <script>
        // Initialize Firebase
        var config = {
            apiKey: "AIzaSyDZjSNCYvua5eFMmdtB70-h5JEZdrvD1mw",
            authDomain: "newpointe-chat-app.firebaseapp.com",
            databaseURL: "https://newpointe-chat-app.firebaseio.com",
            storageBucket: "newpointe-chat-app.appspot.com",
        };
        firebase.initializeApp(config);
    </script>
</asp:Content>
