﻿<%@ Page Language="C#" MasterPageFile="Site.Master" AutoEventWireup="true" Inherits="Rock.Web.UI.RockPage" %>
<asp:Content ID="ctMain" ContentPlaceHolderID="main" runat="server">
        <!-- Page Title -->
        <section id="secPageTitle" class="page-header fullwidth" runat="server">
            <div class="page-title">
                <h1 class="title"><Rock:PageIcon ID="PageIcon" runat="server" /> <Rock:PageTitle ID="PageTitle" runat="server" /></h1>
                <Rock:PageBreadCrumbs ID="PageBreadCrumbs" runat="server" />
                <Rock:PageDescription ID="PageDescription" runat="server" />
            </div>
            <Rock:Zone Name="Context" CssClass="zone-context" runat="server" />
        </section>

        <!-- Start Content Area -->

        <section id="page-content">

            <!-- Ajax Error -->
            <div class="alert alert-danger ajax-error no-index" style="display:none">
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

            <div class="row">
                <div class="col-md-6">
                    <Rock:Zone Name="Section E" runat="server" />
                </div>
                <div class="col-md-6">
                    <Rock:Zone Name="Section F" runat="server" />
                </div>
            </div>
        </section>

        <!-- End Content Area -->



</asp:Content>

