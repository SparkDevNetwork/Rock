<%@ Control Language="C#" AutoEventWireup="true" CodeFile="Person.ascx.cs" Inherits="RockWeb.Blocks.CheckIn.Manager.Person" %>

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
                    <ul class="list-unstyled list-horizontal">
                        <asp:Repeater ID="rptrPhones" runat="server">
                            <ItemTemplate>
                                <li><a class="btn btn-default" href='tel:<%# Eval("Number") %>' ><i class="fa fa-phone-square"></i> <%# Eval("NumberFormatted") %> <small>(<%# Eval("NumberTypeValue.Value") %>)</small></a></li>                            </ItemTemplate>
                        </asp:Repeater>
                    </ul>
                </Rock:RockControlWrapper>

                <Rock:RockLiteral ID="lEmail" runat="server" Label="Email" />
            </div>
        </div>

        <Rock:RockControlWrapper ID="rcwFamily" runat="server" Label="Family" CssClass="list-unstyled">
            <ul class="list-unstyled list-horizontal">
                <asp:Repeater ID="rptrFamily" runat="server" OnItemDataBound="rptrFamily_ItemDataBound">
                    <ItemTemplate>
                        <li><a class="btn btn-action" href='<%# Eval("Url") %>' ><asp:Literal ID="lFamilyIcon" runat="server" /> <%# Eval("FullName") %> <small><%# Eval("Note") %></small></a> </li>
                    </ItemTemplate>
                </asp:Repeater>
            </ul>
        </Rock:RockControlWrapper>
        

        <Rock:RockControlWrapper ID="rcwCheckinHistory" runat="server" Label="Checkin History">
            <Rock:Grid ID="gHistory" runat="server" DisplayType="Light" AllowPaging="false" CssClass="table-condensed">
                <Columns>
                    <Rock:RockBoundField DataField="Date" HeaderText="Date" DataFormatString="{0:MM/dd/yy}" />
                    <Rock:RockBoundField DataField="Group" HeaderText="Group"  />
                    <Rock:RockBoundField DataField="Location" HeaderText="Location" HtmlEncode="false" />
                    <Rock:RockTemplateField HeaderText="Schedule">
                        <ItemTemplate>
                            <%# Eval("Schedule") %> <asp:Literal id="lActive" runat="server"></asp:Literal>
                        </ItemTemplate>
                    </Rock:RockTemplateField>
                </Columns>
            </Rock:Grid>
        </Rock:RockControlWrapper>


    </ContentTemplate>
</asp:UpdatePanel>
