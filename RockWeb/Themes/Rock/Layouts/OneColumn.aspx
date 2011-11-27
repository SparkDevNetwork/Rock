<%@ Page Title="" ValidateRequest="false" Language="C#" MasterPageFile="~/Themes/Rock/Layouts/Site.Master"
    AutoEventWireup="true" CodeFile="OneColumn.aspx.cs" Inherits="Rock.Themes.Rock.Layouts.OneColumn" %>

<asp:Content ID="ctMain" ContentPlaceHolderID="main" runat="server">
            <div id="page-wrapper">
                <header class="group">
                    <h1>Rock ChMS</h1>
                    <section class="user">
                        <asp:Literal ID="lUserName" runat="server"></asp:Literal> <asp:LoginStatus ID="LoginStatus1" runat="server" LogoutAction="Redirect" LogoutPageUrl="~/page/1" />
                    </section>

                    <asp:PlaceHolder ID="phHeader" runat="server"></asp:PlaceHolder>

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
                                                <img src="/Themes/Default/Assets/Mock-Images/staff-directory.png" />Lorem ispsum dolor sit amet, consectur adipiscing elit. Quisque 
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
                                                <img src="/Themes/Default/Assets/Mock-Images/staff-directory.png" />Lorem ispsum dolor sit amet, consectur adipiscing elit. Quisque 
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
                
                <div id="column-content">
                    <h1>Rock ChMS</h1>
                    <asp:PlaceHolder ID="Content" runat="server"></asp:PlaceHolder>
                </div>

                <footer class="group">
                    <asp:PlaceHolder ID="Footer" runat="server"></asp:PlaceHolder>
                </footer>
                
            </div>
</asp:Content>

