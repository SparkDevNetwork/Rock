<%@ Control Language="C#" AutoEventWireup="true" CodeFile="FamilySelect.ascx.cs" Inherits="RockWeb.Blocks.CheckIn.Attended.FamilySelect" %>

<asp:UpdatePanel ID="upContent" runat="server">
<ContentTemplate>

    <Rock:ModalAlert ID="maWarning" runat="server" />

    <div class="row-fluid attended-checkin-header">
        <div class="span3 attended-checkin-actions">
            <asp:LinkButton ID="lbBack" CssClass="btn btn-large btn-primary" runat="server" OnClick="lbBack_Click" Text="Back"/>
        </div>

        <div class="span6">
            <h1 id="familyTitle" runat="server">Search Results</h1>
        </div>

        <div class="span3 attended-checkin-actions">
            <asp:LinkButton ID="lbNext" CssClass="btn btn-large last btn-primary" runat="server" OnClick="lbNext_Click" Text="Next"/>
        </div>
    </div>
                
    <div class="row-fluid attended-checkin-body">
        
<%--        <div id="familyDiv" class="span3 family-div" runat="server">
            <div class="attended-checkin-body-container">
                <asp:Repeater ID="rFamily" runat="server" OnItemCommand="rFamily_ItemCommand">
                    <ItemTemplate>
                        <asp:LinkButton ID="lbSelectFamily" runat="server" CommandArgument='<%# Eval("Group.Id") %>' CssClass="btn btn-primary btn-large btn-block btn-checkin-select" ><%# Eval("Caption") %><br /><span class="checkin-sub-title"><%# Eval("SubCaption") %></span></asp:LinkButton>
                    </ItemTemplate>
                </asp:Repeater>
            </div>
        </div>--%>

        <div id="familyDiv" class="span3 family-div" runat="server">
            <div class="attended-checkin-body-container">
                <asp:ListView ID="lvFamily" runat="server" OnPagePropertiesChanging="lvFamily_PagePropertiesChanging" OnItemCommand="lvFamily_ItemCommand">
                    <ItemTemplate>
                        <asp:LinkButton ID="lSelectFamily" runat="server" CommandArgument='<%# Eval("Group.Id") %>' CssClass="btn btn-primary btn-large btn-block btn-checkin-select" ><%# Eval("Caption") %><br /><span class="checkin-sub-title"><%# Eval("SubCaption") %></span></asp:LinkButton>
                    </ItemTemplate>
                </asp:ListView>
                <asp:DataPager ID="Pager" runat="server" PageSize="4" PagedControlID="lvFamily">
                    <Fields>
                        <asp:NextPreviousPagerField ButtonType="Button" ButtonCssClass="btn btn-primary" />
                    </Fields>
                </asp:DataPager>
            </div>
        </div>

        <div id="personDiv" class="span3 person-div" runat="server">
            <div class="attended-checkin-body-container">
                <asp:Repeater ID="rPerson" runat="server" OnItemCommand="rPerson_ItemCommand">
                    <ItemTemplate>
                        <asp:LinkButton ID="lbSelectPerson" runat="server" Text='<%# Container.DataItem.ToString() %>' CommandArgument='<%# Eval("Person.Id") %>' CssClass="btn btn-primary btn-large btn-block btn-checkin-select" />
                    </ItemTemplate>
                </asp:Repeater>
            </div>
        </div>

        <div id="emptyDiv" class="span3 empty-div" runat="server">
<%--            <div class="attended-checkin-body-container">
                <asp:ListView ID="lvFamily" runat="server" OnPagePropertiesChanging="lvFamily_PagePropertiesChanging" OnItemCommand="lvFamily_ItemCommand">
                    <ItemTemplate>
                        <asp:LinkButton ID="lSelectFamily" runat="server" CommandArgument='<%# Eval("Group.Id") %>' CssClass="btn btn-primary btn-large btn-block btn-checkin-select" ><%# Eval("Caption") %><br /><span class="checkin-sub-title"><%# Eval("SubCaption") %></span></asp:LinkButton>
                    </ItemTemplate>
                </asp:ListView>
                <asp:DataPager ID="Pager" runat="server" PageSize="5" PagedControlID="lvFamily">
                    <Fields>
                        <asp:NextPreviousPagerField ButtonType="Button" ButtonCssClass="btn btn-primary" />
                    </Fields>
                </asp:DataPager>
            </div>--%>
        </div>

        <div id="nothingFoundMessage" class="span9 nothing-found-message" runat="server">
            <div class="span12">
            <p>
                <h1>Aww man!</h1> Too bad you didn't find what you were looking for. Go ahead and add someone using one of the buttons on the right or click the "Back" button and try again!
            </p>
            </div>
        </div>

        <div class="span3 add-someone">
            <div class="attended-checkin-body-container last">
                <asp:LinkButton ID="lbAddPerson" runat="server" CssClass="btn btn-primary btn-large btn-block btn-checkin-select" OnClick="lbAddPerson_Click" Text="Add Person"></asp:LinkButton>
                <asp:LinkButton ID="lbAddVisitor" runat="server" CssClass="btn btn-primary btn-large btn-block btn-checkin-select" OnClick="lbAddVisitor_Click" Text="Add Visitor"></asp:LinkButton>                
                <asp:LinkButton ID="lbAddFamily" runat="server" CssClass="btn btn-primary btn-large btn-block btn-checkin-select" OnClick="lbAddFamily_Click" Text="Add Family"></asp:LinkButton>
            </div>
        </div>
    </div>

</ContentTemplate>
</asp:UpdatePanel>
