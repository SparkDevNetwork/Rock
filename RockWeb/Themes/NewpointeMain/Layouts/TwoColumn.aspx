<%@ Page Language="C#" MasterPageFile="Site.Master" AutoEventWireup="true" Inherits="Rock.Web.UI.RockPage" %>

<asp:Content ID="ctMain" ContentPlaceHolderID="main" runat="server">

    <main class="container">
                
        <!-- Start Content Area -->
        

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
            <div class="col-md-6">
                <Rock:Zone Name="Left 1" runat="server" />
            </div>
            <div class="col-md-6">
                <Rock:Zone Name="Right 1" runat="server" />
            </div>
            </div>
                 <div class="row">
            <div class="col-md-6">
                <Rock:Zone Name="Left 2" runat="server" />
            </div>
            <div class="col-md-6">
                <Rock:Zone Name="Right 2" runat="server" />
            </div>
            </div>
         <div class="row">
            <div class="col-md-6">
                <Rock:Zone Name="Left 3" runat="server" />
            </div>
            <div class="col-md-6">
                <Rock:Zone Name="Right 3" runat="server" />
            </div>
        </div>

        <div class="row">
            <div class="col-md-12">
                <Rock:Zone Name="Section A" runat="server" />
            </div>
        </div>

        <div class="row ">
            <div class="col-md-4 col-sm-6  ">
                <Rock:Zone Name="Section C" runat="server" />
            </div>

            <!-- Add the extra clearfix for only the required viewport 
            <div class="clearfix visible-sm-block"></div>-->

            <div class="col-md-4 col-sm-6  ">
                <Rock:Zone Name="Section D" runat="server" />
            </div>
             <div class="col-md-4 col-sm-6  ">
                <Rock:Zone Name="Section E" runat="server" />
            </div>
        </div>

        <div class="row">
            <div class="col-md-12">
                <Rock:Zone Name="Section F" runat="server" />
            </div>
        </div>

        <!-- End Content Area -->

    </main>

</asp:Content>
