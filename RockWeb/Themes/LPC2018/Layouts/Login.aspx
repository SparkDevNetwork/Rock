<%@ Page Language="C#" MasterPageFile="Site.Master" AutoEventWireup="true" Inherits="Rock.Web.UI.RockPage" %>

<asp:Content ID="ctMain" ContentPlaceHolderID="main" runat="server">

    <style>
        input[type=password], input[type=text] {
            height: 100%;
        }

        label{
            display: none;
        }

        .rock-text-box{
            display: inline-block;
            width: 49.5%;
        }

        @media (max-width: 768px){
            .rock-text-box{
                width: 100%;
            }
        }

        @media (min-width: 992px){
            .section-parallax {
                padding-top: 50px;
                padding-bottom: 230px;
            }
        }

        @media (max-width: 991px){
            .card-static{
                margin-top: 50px !important;
            }
        }

        legend{
            color: #0b1d2d!important;
            font-size: 26px;
            position: relative;
            padding-bottom: 16px;
            font-weight: 500;
            margin: 0;
            text-transform: uppercase;
        }

        .new-btn:after{
            z-index: 900;
            color: white;
            margin-top: 10px;
        }

        .new-btn:hover:after{
            margin-left: -25px;
        }

        .pagetitle{
            display: none;
        }

        input.text, input.title, input[type=email], input[type=password], input[type=tel], input[type=text], select .form-control, textarea{
          border: 1px solid #bbb;
          margin: 0;
          width: 100%;
          padding: 16px;
          border-width: 0;
          color: #999;
          font-size: 14px;
          -webkit-appearance: none;
          background-color: #f9f9f9;
          border-radius: 5px 5px 5px 5px;
          overflow: hidden;
          box-shadow: none !important;
          -webkit-box-shadow: none !important;
          outline: none !important;
        }

        input.text:focus, input.title:focus, input[type=email]:focus, input[type=password]:focus, input[type=tel]:focus, input[type=text]:focus, select:focus, textarea:focus {
          color: #3e3e3e;
        }

        .card-static{
            max-width: 75% !important;
            width: 75%;
            margin-top: -180px !important;
        }

    </style>
    
    <script>
        $(document).ready(function () {
            $('input[id$="UserName"]').attr("placeholder", "Username");
            $('input[id$="Password"]').attr("placeholder", "Password");

            $('input[id$="btnLogin"]').wrap('<span class="new-btn login-wrap"></span>');
            $('.login-wrap').wrap('<div style="margin-bottom: 10px; text-align: right;"></div>');
            $('input[id$="btnHelp"]').wrap('<span class="new-btn" style="margin-bottom: 10px; float: right;"></span>');
            $('input[id$="btnNewAccount"]').wrap('<span class="new-btn" style="margin-bottom: 10px; float: right;  margin-left: 10px;"></span>');
            
        });
    </script>

    <main class="container">

        <!-- Start Content Area -->

        <!-- Page Title -->
        <Rock:PageIcon ID="PageIcon" runat="server" /> <h1 class="pagetitle"><Rock:PageTitle ID="PageTitle" runat="server" /></h1>
        <Rock:PageBreadCrumbs ID="PageBreadCrumbs" runat="server" />

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
            <div class="col-md-6">
                <Rock:Zone Name="Sidebar 1" runat="server" />
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