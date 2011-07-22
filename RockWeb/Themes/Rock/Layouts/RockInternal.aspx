<%@ Page Title="" ValidateRequest="false" Language="C#" MasterPageFile="~/Themes/Rock/Layouts/Site.Master"
    AutoEventWireup="true" CodeFile="RockInternal.aspx.cs" Inherits="Rock.Themes.Rock.Layouts.RockInternal" %>

<asp:Content ID="ctMain" ContentPlaceHolderID="main" runat="server">
            <div id="page-wrapper">
                <header class="group">
                    <h1>Rock ChMS</h1>
                    <section class="user">
                        <asp:Literal ID="lUserName" runat="server"></asp:Literal> <asp:LoginStatus ID="LoginStatus1" runat="server" LogoutAction="Redirect" LogoutPageUrl="~/page/1" />
                    </section>

                    <asp:PlaceHolder ID="Header" runat="server"></asp:PlaceHolder>

                    <nav>
                        <ul>
                            <li>
                                <a href="#">Intranet</a>
                                <div class="nav-menu group">
                                    <div class="width-1column">
                                        <section>
                                            <header>
                                                <strong>Office Information</strong>
                                                Lorem ispsum dolor sit amet, consectur adipiscing elit. Quisque tincdunt, arcu.
                                            </header>
                                            <ul class="group">
                                                <li><a href="">Holidays</a></li>
                                                <li><a href="">Templates & Logos</a></li>
                                                <li><a href="">Weddings, Funerals, and Outside Use of Facilities</a></li>
                                                <li><a href="">Staff FAQ</a></li>
                                                <li><a href="">Shipping Info</a></li>
                                                <li><a href="">VIP Volunteers</a></li>
                                                <li><a href="">Personal Ministry Referral List</a></li>
                                            </ul>
                                        </section>

                                        <section>
                                            <header>
                                                <strong>Staff Directory</strong>
                                                <img src='/RockWeb/Themes/Rock/Assets/Mock-Images/staff-directory.png' />Lorem ispsum dolor sit amet, consectur adipiscing elit. Quisque 
                                                tincdunt, arcu.
                                            </header>
                                        </section>

                                        <section>
                                            <header>
                                                <strong>HR Resources</strong>
                                            </header>
                                            <ul class="group">
                                                <li><a href="">Holidays</a></li>
                                                <li><a href="">Templates & Logos</a></li>
                                                <li><a href="">Weddings, Funerals, and Outside Use of Facilities</a></li>
                                                <li><a href="">Staff FAQ</a></li>
                                                <li><a href="">Shipping Info</a></li>
                                                <li><a href="">VIP Volunteers</a></li>
                                                <li><a href="">Personal Ministry Referral List</a></li>
                                            </ul>
                                        </section>
                                    </div>
                                </div>
                            </li>
                            <li>
                                <a href="#">Metrics</a>
                                <div class="nav-menu">
                                    <div class="width-1column">
                                        <section>
                                            <header>
                                                <strong>Office Information</strong>
                                                Lorem ispsum dolor sit amet, consectur adipiscing elit. Quisque tincdunt, arcu.
                                            </header>
                                            <ul class="group">
                                                <li><a href="">Holidays</a></li>
                                                <li><a href="">Templates & Logos</a></li>
                                                <li><a href="">Weddings, Funerals, and Outside Use of Facilities</a></li>
                                                <li><a href="">Staff FAQ</a></li>
                                                <li><a href="">Shipping Info</a></li>
                                                <li><a href="">VIP Volunteers</a></li>
                                                <li><a href="">Personal Ministry Referral List</a></li>
                                            </ul>
                                        </section>

                                        <section>
                                            <header>
                                                <strong>Staff Directory</strong>
                                                <img src='/RockWeb/Themes/Rock/Assets/Mock-Images/staff-directory.png' />Lorem ispsum dolor sit amet, consectur adipiscing elit. Quisque 
                                                tincdunt, arcu.
                                            </header>
                                        </section>

                                        <section>
                                            <header>
                                                <strong>HR Resources</strong>
                                            </header>
                                            <ul class="group">
                                                <li><a href="">Holidays</a></li>
                                                <li><a href="">Templates & Logos</a></li>
                                                <li><a href="">Weddings, Funerals, and Outside Use of Facilities</a></li>
                                                <li><a href="">Staff FAQ</a></li>
                                                <li><a href="">Shipping Info</a></li>
                                                <li><a href="">VIP Volunteers</a></li>
                                                <li><a href="">Personal Ministry Referral List</a></li>
                                            </ul>
                                        </section>
                                    </div>
                                </div>
                            </li>
                            <li><a href="#">Communication</a></li>
                            <li><a href="#">Promotion</a></li>
                            <li><a href="#">Finance</a></li>
                            <li><a href="#">Administration</a></li>
                        </ul>
                    </nav>

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
                            Network</a> <a href="http://creativecommons.org/licenses/by-nc-sa/3.0/"><img src='RockWeb/Themes/Rock/Assets/Images/cc-license.png' height="15" width="80" /></a> </p>
                    </div>
                </footer>
                 
            </div>
</asp:Content>

