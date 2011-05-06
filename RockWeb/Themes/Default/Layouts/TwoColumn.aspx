<%@ Page Title="" ValidateRequest="false" Language="C#" MasterPageFile="~/Themes/Default/Layouts/Site.Master" AutoEventWireup="true" CodeFile="TwoColumn.aspx.cs" Inherits="Rock.Themes.Default.Layouts.TwoColumn" %>
<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="main" runat="server">
    <div class="content">
    <header id="page-header" class="group">
        
            <h1>Spark ChMS</h1>
            <section class="user">
                <asp:Literal ID="lUserName" runat="server"></asp:Literal> <asp:LoginStatus runat="server" LogoutAction="Redirect" LogoutPageUrl="~/page/1" />
            </section>
            <section class="search">
                <p>Search</p> 
                <div class="search-combo group">
                    <input type="text" /> 
                </div>
            </section>
            
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

            <asp:PlaceHolder ID="Heading" runat="server"></asp:PlaceHolder>
        
    </header>

    <div id="page-content" class="group">
        <asp:Panel ID="FirstColumn" runat="server" class="first-column"></asp:Panel>
        <asp:Panel ID="SecondColumn" runat="server" class="second-column"></asp:Panel>
    </div>

    <footer class="group">
        <asp:PlaceHolder ID="Footer" runat="server"></asp:PlaceHolder>

        <div id="footer-base" class="group">
            <p id="footer-confidential">The information and data contained in this system is the property
            of <insert organization name>/  Thos granted access are reminded to deny requests for giving
            out addresses, emails, phone numbers, etc. or to use the information for anything other than
            this organization's activities.</p>
            <p id="footer-license">Spark ChMS is an open-source project by the <a href="">Spark Development 
            Network</a> <a href="http://creativecommons.org/licenses/by-nc-sa/3.0/"><img src="../../Themes/Default/Assets/Images/cc-license.png" height="15" width="80" /></a> </p>
        </div>
    </footer>
    </div>
    <script>
        $('input').placeholder();
    </script>

</asp:Content>
