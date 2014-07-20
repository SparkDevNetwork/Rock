<%@ Control Language="C#" AutoEventWireup="true" CodeFile="Person.ascx.cs" Inherits="RockWeb.Blocks.Utility.Stark" %>

<asp:UpdatePanel ID="upnlContent" runat="server">
    <ContentTemplate>

        <h4><asp:Literal ID="lName" runat="server"></asp:Literal></h4>

        <div class="row">
            <div class="col-sm-3">
                <div class="photoframe">
                    <asp:Literal ID="lPhoto" runat="server" />
                </div>
            </div>
            <div class="col-sm-9">
                <Rock:RockControlWrapper ID="rcwPhone" runat="server" Label="Phone(s)">
                    <ul>
                        <asp:Repeater ID="rptrPhones" runat="server">
                            <ItemTemplate>
                                <li><a href='tel:<%# Eval("Number") %>' ><%# Eval("NumberFormatted") %></a> <small>(<%# Eval("NumberTypeValue.Name") %>)</small></li>
                            </ItemTemplate>
                        </asp:Repeater>
                    </ul>
                </Rock:RockControlWrapper>

                <Rock:RockLiteral ID="lEmail" runat="server" Label="Email" />
            </div>
        </div>

        <Rock:RockControlWrapper ID="rcwFamily" runat="server" Label="Family" CssClass="list-unstyled">
            <asp:Repeater ID="rptrFamily" runat="server" OnItemDataBound="rptrFamily_ItemDataBound">
                <ItemTemplate>
                    <li><a class="btn btn-default" href='<%# Eval("Url") %>' ><asp:Literal ID="lFamilyIcon" runat="server" /> <%# Eval("FullName") %> <small><%# Eval("Note") %></small></a> </li>
                </ItemTemplate>
            </asp:Repeater>
        </Rock:RockControlWrapper>
        

        <Rock:RockControlWrapper ID="rcwCheckinHistory" runat="server" Label="Checkin History">
            <Rock:Grid ID="gHistory" runat="server" DisplayType="Light" AllowPaging="false" CssClass="table-condensed">
                <Columns>
                    <asp:BoundField DataField="Date" HeaderText="Date" DataFormatString="{0:MM/dd/yy}" />
                    <asp:BoundField DataField="Group" HeaderText="Group"  />
                    <asp:BoundField DataField="Location" HeaderText="Location" HtmlEncode="false" />
                    <asp:BoundField DataField="Schedule" HeaderText="Schedule" />
                </Columns>
            </Rock:Grid>
        </Rock:RockControlWrapper>


    </ContentTemplate>
</asp:UpdatePanel>
