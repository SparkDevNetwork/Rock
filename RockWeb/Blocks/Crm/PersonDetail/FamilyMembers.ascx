<%@ Control Language="C#" AutoEventWireup="true" CodeFile="FamilyMembers.ascx.cs" Inherits="RockWeb.Blocks.Crm.PersonDetail.FamilyMembers" %>

<asp:UpdatePanel ID="upFamilyMembers" runat="server">
    <ContentTemplate>

        <asp:Repeater ID="rptrFamilies" runat="server">
            <ItemTemplate>

                <div class="persondetails-family rollover-container">

                    <div class="actions rollover-item">
                        <asp:HyperLink ID="hlEditFamily" runat="server" CssClass="edit btn btn-link btn-xs"><i class="fa fa-pencil"></i> Edit Family</asp:HyperLink>
                    </div>

                    <div class="row">
                
                        <div class="col-md-8 clearfix">

                            <header class="title"><span class="first-word"><%# Eval("Name") %></span> Family</header>

                            <ul class="groupmembers">

                                <asp:Repeater ID="rptrMembers" runat="server">
                                    <ItemTemplate>
                                        <li class="member">
                                            <a href='<%# FormatPersonLink(Eval("PersonId").ToString()) %>'>
                                                <asp:Image ID="imgPerson" runat="server" />
                                                <div>
                                                    <h4><%# Eval("Person.FirstName") %></h4>
                                                    <small class="age"><%# Eval("Person.Age")  %></small>
                                                </div>
                                            </a>
                                        </li>
                                    </ItemTemplate>
                                </asp:Repeater>

                            </ul>

                        </div>

                        <div class="col-md-4 addresses clearfix">

                            <ul>

                                <asp:Repeater ID="rptrAddresses" runat="server">
                                    <ItemTemplate>
                                        <li class="address rollover-container clearfix">
                                            <h4><%# FormatAddressType(Eval("LocationTypeValue.Name")) %></h4>
                                            <a id="aMap" runat="server" title="Map This Address" class="map" target="_blank">
                                                <i class="fa fa-map-marker"></i>
                                            </a>
                                            <div class="address">
                                                <%# Eval("Location.Street1") %><br />
                                                <asp:PlaceHolder ID="phStreet2" runat="server" />
                                                <span><%# Eval("Location.City") %>, <%# Eval("Location.State") %> <%# Eval("Location.Zip") %></span>
                                            </div>
                                            <div class="actions rollover-item">
                                                <asp:LinkButton ID="lbGeocode" runat="server">
                                                    <i class="fa fa-globe"></i>
                                                </asp:LinkButton>
                                                <asp:LinkButton ID="lbStandardize" runat="server">
                                                    <i class="fa fa-magic"></i>
                                                </asp:LinkButton>
                                                <a title="Address Standardized" href="../Blocks/Crm/PersonDetail/#"></a>
                                            </div>
                                        </li>
                                    </ItemTemplate>
                                </asp:Repeater>

                            </ul>

                        </div>

                    </div>

                </div>

            </ItemTemplate>
        </asp:Repeater>

    </ContentTemplate>
</asp:UpdatePanel>
