<%@ Control Language="C#" AutoEventWireup="true" CodeFile="TemplateList.ascx.cs" Inherits="RockWeb.Blocks.Communication.TemplateList" %>

<asp:UpdatePanel ID="upnlContent" runat="server">
    <ContentTemplate>

        <Rock:ModalAlert ID="maGridWarning" runat="server" />

        <div class="panel panel-block">
            <div class="panel-heading">
                <h1 class="panel-title"><i class="fa fa-comment-o"></i>&nbsp;Communication Template List</h1>
            </div>
            <div class="panel-body">

                <div class="grid grid-panel">
                    <Rock:GridFilter ID="rFilter" runat="server" OnClearFilterClick="rFilter_ClearFilterClick">
                        <Rock:PersonPicker ID="ppCreatedBy" runat="server" Label="Created By" Help="The person who created the template." EnableSelfSelection="true" />
                        <Rock:CategoryPicker ID="cpCategory" runat="server" Label="Category" Required="false" EntityTypeName="Rock.Model.CommunicationTemplate" />
                        <Rock:RockDropDownList ID="ddlSupports" runat="server" Label="Supports">
                            <asp:ListItem></asp:ListItem>
                            <asp:ListItem Text="Simple Email Template" Value="Simple Email Template"></asp:ListItem>
                            <asp:ListItem Text="Email Wizard" Value="Email Wizard"></asp:ListItem>
                        </Rock:RockDropDownList>
                        <Rock:RockDropDownList ID="ddlActiveFilter" runat="server" Label="Active Status">
                            <asp:ListItem></asp:ListItem>
                            <asp:ListItem Text="Active" Value="Active"></asp:ListItem>
                            <asp:ListItem Text="Inactive" Value="Inactive"></asp:ListItem>
                        </Rock:RockDropDownList>
                    </Rock:GridFilter>

                    <Rock:Grid ID="gCommunicationTemplates" runat="server" AllowSorting="true" TooltipField="Description" OnRowSelected="gCommunicationTemplates_RowSelected" OnRowDataBound="gCommunicationTemplates_RowDataBound" CssClass="js-grid-communicationtemplate-list">
                        <Columns>
                            <Rock:RockBoundField DataField="Name" SortExpression="Name" HeaderText="Name" />
                            <Rock:RockBoundField DataField="Subject" SortExpression="Subject" HeaderText="Subject" />
                            <Rock:RockBoundField DataField="Category" HeaderText="Category" SortExpression="Category.Name" />
                            <Rock:RockLiteralField ID="lSupports" HeaderText="Supports" />
                            <Rock:RockBoundField DataField="CreatedByPersonAlias.Person.FullName" SortExpression="CreatedByPersonAlias.Person.NickName, CreatedByPersonAlias.Person.LastName" HeaderText="Created By" />
                            <Rock:BoolField DataField="IsActive" SortExpression="IsActive" HeaderText="Active" />
                            <Rock:SecurityField TitleField="Name" />
                            <Rock:LinkButtonField HeaderText="Copy" CssClass="btn btn-default btn-sm btn-square fa fa-clone" OnClick="gCommunicationTemplates_Copy" HeaderStyle-HorizontalAlign="Center" ItemStyle-HorizontalAlign="Center" />
                            <Rock:DeleteField OnClick="gCommunicationTemplates_Delete" />
                        </Columns>
                    </Rock:Grid>
                </div>

            </div>
        </div>

    </ContentTemplate>
</asp:UpdatePanel>
