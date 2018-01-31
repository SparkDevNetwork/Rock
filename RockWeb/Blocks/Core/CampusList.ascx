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
                    <Rock:Grid ID="gCampuses" runat="server"  OnRowSelected="gCampuses_Edit">
                        <Columns>
                            <Rock:ReorderField />
                            <Rock:RockBoundField DataField="Name" HeaderText="Name" />
                            <Rock:RockBoundField DataField="ShortCode" HeaderText="Short Code"  />
                            <Rock:RockBoundField DataField="PhoneNumber" HeaderText="Phone Number" />
                            <Rock:RockBoundField DataField="LeaderPersonAlias.Person.FullName" HeaderText="Campus Leader" />
                            <Rock:BoolField DataField="IsActive" HeaderText="Active" />
                        </Columns>
                    </Rock:Grid>
                </div>

            </div>
        </div>


        

    </ContentTemplate>
</asp:UpdatePanel>

