<%@ Control Language="C#" AutoEventWireup="true" CodeFile="TemplateList.ascx.cs" Inherits="RockWeb.Blocks.Communication.TemplateList" %>

<asp:UpdatePanel ID="upnlContent" runat="server">
    <ContentTemplate>

        <Rock:ModalAlert ID="maGridWarning" runat="server" />

        <div class="grid">
            <Rock:GridFilter ID="rFilter" runat="server">
                <Rock:PersonPicker ID="ppCreatedBy" runat="server" Label="Created By" Help="The person who created the template." />
                <Rock:ComponentPicker ID="cpChannel" runat="server" ContainerType="Rock.Communication.ChannelContainer, Rock" Label="Channel" />
            </Rock:GridFilter>

            <Rock:Grid ID="gCommunication" runat="server" AllowSorting="true" TooltipField="Description" OnRowSelected="gCommunication_RowSelected">
                <Columns>
                    <asp:BoundField DataField="Name" SortExpression="Subject" HeaderText="Name" />
                    <asp:BoundField DataField="Subject" SortExpression="Subject" HeaderText="Subject" />
                    <asp:BoundField DataField="ChannelEntityType.FriendlyName" SortExpression="ChannelEntityType.FriendlyName" HeaderText="Channel" />
                    <asp:BoundField DataField="CreatedByPersonAlias.Person.FullName" SortExpression="CreatedByPersonAlias.Person.FullName" HeaderText="Created By" />
                    <Rock:SecurityField TitleField="Name" />
                    <Rock:DeleteField OnClick="gCommunication_Delete" />
                </Columns>
            </Rock:Grid>
        </div>

    </ContentTemplate>
</asp:UpdatePanel>
