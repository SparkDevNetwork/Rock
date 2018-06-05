<%@ Page Language="C#" MasterPageFile="Site.Master" AutoEventWireup="true" Inherits="Rock.Web.UI.RockPage" %>

<asp:Content ID="ctMain" ContentPlaceHolderID="main" runat="server">

<style>

    header {
        display: none;
    }

    .navbar-static-side {
        display: none !important;
    }

    body.navbar-side-close #content-wrapper {
        left: 0;
        background-color: #ffffff;
    }

    #content-wrapper #page-content {
        overflow-y: visible;
        padding: 40px 60px;
        margin-right: 50%;
    }

    @media (max-width: 767px) {
        #content-wrapper #page-content {
            padding: 15px;
            margin-right: 0;
        }
    }

    .panel {
        box-shadow: none;
        border-color: transparent !important;
    }

    .newspring-wordmark {
        transform: rotate(-90deg);
        bottom: 90px;
        right: -30px;
    }

    .newspring-wordmark h4 {
        margin-bottom: 0;
        font-size: 24px;
        font-weight: 700;
        color: #fff;
        text-shadow: 0 0 10px rgba(0,0,0,.45);
    }

    footer {
        display: none;
    }

</style>

        <!-- Start Content Area -->

        <!-- Page Title -->
        <section id="page-title" class="hidden">
            <h1 class="title"><Rock:PageIcon ID="PageIcon" runat="server" /> <Rock:PageTitle ID="PageTitle" runat="server" /></h1>
            <Rock:PageBreadCrumbs ID="PageBreadCrumbs" runat="server" />
            <Rock:PageDescription ID="PageDescription" runat="server" />
        </section>
        
        <section id="page-content">

            <!-- Ajax Error -->
            <div class="alert alert-danger ajax-error" style="display:none">
                <p><strong>Error</strong></p>
                <span class="ajax-error-message"></span>
            </div>

            <div class="row">
                <div class="col-md-12">
                    <Rock:Zone Name="Feature" runat="server" />
                </div>
            </div>

            <div class="row">
                <div class="col-md-12">
                    <Rock:Zone Name="Main" runat="server" />
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
        </section>

        <section id="side-image" class="hidden-xs fixed top-zero bottom-zero right-zero left-split background-cover" style="background-image: url('https://s3.amazonaws.com/ns.images/newspring/fpo/newspring.generic.1x2.jpg');">
            <div class="newspring-wordmark fixed">
                <h4 class="soft-half flush text-light-primary uppercase watermark visuallyhidden@handheld">NewSpring</h4>
            </div>
        </section>

        <!-- End Content Area -->



</asp:Content>

