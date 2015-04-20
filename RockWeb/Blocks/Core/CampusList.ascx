<%@ Control Language="C#" AutoEventWireup="true" CodeFile="CampusList.ascx.cs" Inherits="RockWeb.Blocks.Core.Campuses" %>

<asp:UpdatePanel ID="upCampusList" runat="server">
    <ContentTemplate>
        <Rock:ModalAlert ID="mdGridWarning" runat="server" />
        
        <div class="panel panel-block">
            <div class="panel-heading">
                <h1 class="panel-title"><i class="fa fa-building-o"></i> Campus List</h1>
            </div>
            <div class="panel-body">

                <div class="grid grid-panel">
                    <Rock:Grid ID="gCampuses" runat="server" AllowSorting="true" OnRowSelected="gCampuses_Edit">
                        <Columns>
                            <Rock:RockBoundField DataField="Name" HeaderText="Name" SortExpression="Name" />
                            <Rock:RockBoundField DataField="ShortCode" HeaderText="Short Code" SortExpression="ShortCode" />
                            <Rock:RockBoundField DataField="PhoneNumber" HeaderText="Phone Number" SortExpression="PhoneNumber" />
                            <Rock:RockBoundField DataField="LeaderPersonAlias.Person.FullName" HeaderText="Campus Leader" SortExpression="LeaderPersonAlias.Person.FullName" />
                            <Rock:BoolField DataField="IsActive" HeaderText="Active" />
                            <Rock:DeleteField OnClick="gCampuses_Delete" />
                        </Columns>
                    </Rock:Grid>
                </div>

            </div>
        </div>


        

    </ContentTemplate>
</asp:UpdatePanel>

