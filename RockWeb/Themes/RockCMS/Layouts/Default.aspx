<%@ Page Title="" ValidateRequest="false" Language="C#" MasterPageFile="~/Themes/RockCMS/Layouts/Site.Master"
    AutoEventWireup="true" CodeFile="Default.aspx.cs" Inherits="RockWeb.Themes.RockCMS.Layouts.Default" %>

<asp:Content ID="ctMain" ContentPlaceHolderID="main" runat="server">
            <div id="page-wrapper">
                <header class="group">
                    <h1>Rock ChMS</h1>
                    <section class="user">
                        <asp:Literal ID="lUserName" runat="server"></asp:Literal> <asp:LoginStatus ID="LoginStatus1" runat="server" LogoutAction="Redirect" LogoutPageUrl="~/page/1" />
                    </section>

                    <asp:PlaceHolder ID="phHeader" runat="server"></asp:PlaceHolder>
                    <asp:PlaceHolder ID="Menu" runat="server"></asp:PlaceHolder>

                </header>
                
                <div id="main-content" class="group">
                    <div class="content-left column">
                        <asp:PlaceHolder ID="ContentLeft" runat="server"></asp:PlaceHolder>                        
                    </div>

                    <div class="content-right column">
                        <asp:PlaceHolder ID="ContentRight" runat="server"></asp:PlaceHolder>                        
                    </div>

                    <div class="content-center column">
                        <asp:PlaceHolder ID="Content" runat="server"></asp:PlaceHolder>
                    </div>
                </div>
                
                <div id="upper-band">
                    <asp:PlaceHolder ID="UpperBand" runat="server"></asp:PlaceHolder>
                </div>

                <div id="lower-band">
                    <asp:PlaceHolder ID="LowerBand" runat="server"></asp:PlaceHolder>
                </div>
                

                <div id="lower-content" class="group">
                    <div class="content-left column">
                        <asp:PlaceHolder ID="LowerContentLeft" runat="server"></asp:PlaceHolder>
                    </div>

                    <div class="content-right column">
                        <asp:PlaceHolder ID="LowerContentRight" runat="server"></asp:PlaceHolder>
                    </div>

                    <div class="content-center column">
                        <asp:PlaceHolder ID="LowerContent" runat="server"></asp:PlaceHolder>
                    </div>
                    <!-- May need to use http://www.cssnewbie.com/equalheights-jquery-plugin/ to get columns of equal height -->
                </div>
                
                <footer class="group">
                    <asp:PlaceHolder ID="Footer" runat="server"></asp:PlaceHolder>
                    <div id="footer-base" class="group">
                        <p id="footer-confidential">The information and data contained in this system is the property
                            of <insert organization name>/  Thos granted access are reminded to deny requests for giving
                            out addresses, emails, phone numbers, etc. or to use the information for anything other than
                            this organization's activities.</p>
                        <p id="footer-license">Rock ChMS is an open-source project by the <a href="">Spark Development 
                            Network</a> <a href="http://creativecommons.org/licenses/by-nc-sa/3.0/"><img id="imgCC" runat="server" height="15" width="80" /></a> </p>
                    </div>
                </footer>
                 
            </div>
</asp:Content>

