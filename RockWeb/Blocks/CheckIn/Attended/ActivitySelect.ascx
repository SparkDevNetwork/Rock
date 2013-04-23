<%@ Control Language="C#" AutoEventWireup="true" CodeFile="ActivitySelect.ascx.cs" Inherits="RockWeb.Blocks.CheckIn.Attended.ActivitySelect" %>

<asp:UpdatePanel ID="upContent" runat="server">
<ContentTemplate>

    <Rock:ModalAlert ID="maWarning" runat="server" />

    <div class="row-fluid attended-checkin-header">
        <div class="span3 attended-checkin-actions">
            <!--<asp:LinkButton ID="lbBack" CssClass="btn btn-primary btn-large btn-block btn-checkin-select" runat="server" OnClick="lbBack_Click" Text="BACK"/>-->
            <asp:LinkButton ID="lbBack2" CssClass="btn btn-primary btn-large" runat="server" OnClick="lbBack_Click" Text="BACK"/>
        </div>

        <div class="span6">
            <%--<legend>Search Result</legend>--%>
            <asp:Label ID="lblPersonName" runat="server"></asp:Label>
        </div>

        <div class="span3 attended-checkin-actions">
            <asp:LinkButton ID="lbNext" CssClass="btn btn-primary btn-large last" runat="server" OnClick="lbNext_Click" Text="NEXT"/>
        </div>
    </div>
                
    <div class="row-fluid attended-checkin-body">

        <div class="span3" style="background-color:#ffffff;">
            <div class="checkin-body-container">
                <asp:Repeater ID="rMinistry" runat="server" OnItemCommand="rMinistry_ItemCommand">
                    <ItemTemplate>
                        <asp:LinkButton ID="lbSelectMinistry" runat="server" CommandArgument='<%# Eval("Ministry.Id") %>' CssClass="btn btn-primary btn-large btn-block btn-checkin-select" ><%# Eval("Caption") %><span class="checkin-sub-title"><%# Eval("SubCaption") %></span></asp:LinkButton>
                    </ItemTemplate>
                </asp:Repeater>
            </div>
        </div>

        <div class="span3" style="background-color:#ff0000;">
            <div class="checkin-body-container">
                <asp:Repeater ID="rTime" runat="server" OnItemCommand="rTime_ItemCommand">
                    <ItemTemplate>
                        <asp:LinkButton ID="lbSelectTime" runat="server" Text='<%# Container.DataItem.ToString() %>' CommandArgument='<%# Eval("Time.Id") %>' CssClass="btn btn-primary btn-large btn-block btn-checkin-select" />
                    </ItemTemplate>
                </asp:Repeater>
            </div>
        </div>

        <div class="span3" style="background-color:#00ff00;">
            <div class="checkin-body-container">
                <asp:Repeater ID="rActivity" runat="server" OnItemCommand="rActivity_ItemCommand">
                    <ItemTemplate>
                        <asp:LinkButton ID="lbSelectActivity" runat="server" Text='<%# Container.DataItem.ToString() %>' CommandArgument='<%# Eval("Activity.Id") %>' CssClass="btn btn-primary btn-large btn-block btn-checkin-select" />
                    </ItemTemplate>
                </asp:Repeater>
            </div>
        </div>

        <div class="span3" style="background-color:#0000ff;">
<%--        Haven't completely decided what we're going to do with this column yet. I say that we list the choices that the user makes and allow them to make changes. Maybe if the user
            selects a block here, it highlights the current selections allowing them to edit? And then have a "X" button attached to each block allowing them to delete? Or is that getting too
            complicated for what we need?--%>
            <div class="checkin-body-container">
                <%--<h1>TEST</h1>--%>
<%--                <asp:LinkButton ID="lbAddFamily" runat="server" CssClass="btn btn-primary btn-large btn-block btn-checkin-select" OnClick="lbAddFamily_Click"></asp:LinkButton>
                <asp:LinkButton ID="lbAddPerson" runat="server" CssClass="btn btn-primary btn-large btn-block btn-checkin-select" OnClick="lbAddPerson_Click"></asp:LinkButton>
                <asp:LinkButton ID="lbAddVisitor" runat="server" CssClass="btn btn-primary btn-large btn-block btn-checkin-select" OnClick="lbAddVisitor_Click"></asp:LinkButton>                --%>
            </div>
        </div>

    </div>   

</ContentTemplate>
</asp:UpdatePanel>
