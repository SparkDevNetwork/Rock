<%@ Control Language="C#" AutoEventWireup="true" CodeFile="Person.ascx.cs" Inherits="RockWeb.Blocks.Utility.Stark" %>

<asp:UpdatePanel ID="upnlContent" runat="server">
    <ContentTemplate>

        <h4><asp:Literal ID="lName" runat="server"></asp:Literal></h4>

        <div class="row">
            <div class="col-xs-5 col-sm-3">
                <asp:Literal ID="lPhoto" runat="server"></asp:Literal>
            </div>
            <div class="col-xs-7 col-sm-9">
                <Rock:RockControlWrapper ID="rcwFamily" runat="server" Label="Family">
                    <asp:Repeater ID="rptrFamily" runat="server">
                        <ItemTemplate>
                            <li><a href='<%# Eval("Url") %>' ><%# Eval("FullName") %></a> <small><%# Eval("Note") %></small></li>
                        </ItemTemplate>
                    </asp:Repeater>
                </Rock:RockControlWrapper>
            </div>
        </div>

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

        <Rock:RockControlWrapper ID="rcwCheckinHistory" runat="server" Label="Checkin History">
            <Rock:Grid ID="gHistory" runat="server" DisplayType="Light" CssClass="table-condensed">
                <Columns>
                    <asp:BoundField DataField="Date" DataFormatString="{0:MM/dd/yy}" />
                    <asp:BoundField DataField="Group" />
                    <asp:BoundField DataField="Location" />
                    <asp:BoundField DataField="Schedule" />
                </Columns>
            </Rock:Grid>
        </Rock:RockControlWrapper>


    </ContentTemplate>
</asp:UpdatePanel>
