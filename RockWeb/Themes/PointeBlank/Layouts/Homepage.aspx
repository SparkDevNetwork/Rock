<%@ Page Language="C#" MasterPageFile="Site.Master" AutoEventWireup="true" Inherits="Rock.Web.UI.RockPage" %>



<asp:Content ID="ctFeature" ContentPlaceHolderID="feature" runat="server">

    <header id="header-full">
        <div class="wrap-primary">
            <div class="container">
                <Rock:Zone Name="Feature" runat="server" />
            </div>
        </div>
    </header>


    <div class="container">
        <section id="latestepisode" class="margin-bottom">
            <div class="row">
                <Rock:Zone Name="LatestEpisode" runat="server" />
            </div>
        </section>

        <section id="about" class="margin-bottom" style="min-height:600px;">
            <div class="row">
                <div class="col-md-12">
                    <Rock:Zone Name="About" runat="server" />
                </div>
            </div>
        </section>

        <section id="archive" class="margin-bottom" style="min-height:600px;">
            <div class="row">
                <Rock:Zone Name="Other" runat="server" />
            </div>
        </section>
    </div>

</asp:Content>

<asp:Content ID="ctMain" ContentPlaceHolderID="main" runat="server">



    <main class="container">



        <!-- Start Content Area -->

        <!-- Ajax Error -->
        <div class="alert alert-danger ajax-error" style="display: none">
            <p><strong>Error</strong></p>
            <span class="ajax-error-message"></span>
        </div>

        <div class="row">
            <div class="col-md-12">
                <Rock:Zone Name="Sub Feature" runat="server" />
            </div>
        </div>

        <div class="row">
            <div class="col-md-12">
                <Rock:Zone Name="Section A" runat="server" />
            </div>
        </div>

        <div class="row">
            <div class="col-md-4">
                <Rock:Zone Name="Section B" runat="server" />
            </div>
            <div class="col-md-4">
                <Rock:Zone Name="Section C" runat="server" />
            </div>
            <div class="col-md-4">
                <Rock:Zone Name="Section D" runat="server" />
            </div>
        </div>

        <!-- End Content Area -->

    </main>

</asp:Content>

