<%@ Page Language="C#" MasterPageFile="Site.Master" AutoEventWireup="true" Inherits="Rock.Web.UI.RockPage" %>

<asp:Content ID="ctMain" ContentPlaceHolderID="main" runat="server">

    
    <%--<script src="https://code.getmdl.io/1.1.3/material.min.js"></script>--%>

    <script src="/Themes/NewPointeLive/Scripts/firebase-chat-private.js"></script>

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
