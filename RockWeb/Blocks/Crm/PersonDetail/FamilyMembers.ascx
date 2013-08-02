<%@ Control Language="C#" AutoEventWireup="true" CodeFile="FamilyMembers.ascx.cs" Inherits="RockWeb.Blocks.Crm.PersonDetail.FamilyMembers" %>

<asp:UpdatePanel ID="upFamilyMembers" runat="server">
    <ContentTemplate>

        <asp:Repeater ID="rptrFamilies" runat="server">
            <ItemTemplate>

                    <div class="actions" style="display: none;">
                        <asp:HyperLink ID="hlEditFamily" runat="server" CssClass="edit btn btn-mini"><i class="icon-pencil"></i> Edit Family</asp:HyperLink>
                    </div>

                    <div class="row-fluid">
                
                        <div class="span8 members clearfix">

                            <header><%# Eval("Name") %> <span class="light">Family</span></header>

                            <ul class="clearfix">

                                <asp:Repeater ID="rptrMembers" runat="server">
                                    <ItemTemplate>
                                        <li>
                                            <a href='<%# Eval("PersonId") %>'>
                                                <asp:Image ID="imgPerson" runat="server" />
                                                <div class="member">
                                                    <h4><%# Eval("Person.FirstName") %></h4>
                                                    <small class="age"><%# Eval("Person.Age")  %></small>
                                                    <small><%# Eval("GroupRole.Name")  %></small>
                                                </div>
                                            </a>
                                        </li>
                                    </ItemTemplate>
                                </asp:Repeater>

                            </ul>

                        </div>

                        <div class="span4 addresses clearfix">

                            <ul>

                                <asp:Repeater ID="rptrAddresses" runat="server">
                                    <ItemTemplate>
                                        <li class="address clearfix">
                                            <h4><%# FormatAddressType(Eval("LocationTypeValue.Name")) %></h4>
                                            <a id="aMap" runat="server" title="Map This Address" class="map" target="_blank" >
                                                <i class="icon-map-marker"></i>
                                            </a>
                                            <div class="address">
                                                <%# Eval("Location.Street1") %><br />
                                                <asp:PlaceHolder ID="phStreet2" runat="server" />
                                                <span><%# Eval("Location.City") %>, <%# Eval("Location.State") %> <%# Eval("Location.Zip") %></span>
                                            </div>
                                            <div class="actions" style="display: none;">
                                                <asp:LinkButton ID="lbGeocode" runat="server">
                                                    <i class="icon-globe"></i>
                                                </asp:LinkButton>
                                                <asp:LinkButton ID="lbStandardize" runat="server">
                                                    <i class="icon-magic"></i>
                                                </asp:LinkButton>
                                                <a title="Address Standardized" href="../Blocks/Crm/PersonDetail/#">
                                                </a>
                                            </div>
                                        </li>
                                    </ItemTemplate>
                                </asp:Repeater>

                            </ul>

                        </div>

                    </div>

            </ItemTemplate>
        </asp:Repeater>

    </ContentTemplate>
</asp:UpdatePanel>
