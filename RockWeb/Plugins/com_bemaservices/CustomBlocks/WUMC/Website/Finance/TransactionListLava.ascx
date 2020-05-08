<%@ Control Language="C#" AutoEventWireup="true" CodeFile="TransactionListLava.ascx.cs" Inherits="RockWeb.Plugins.com_bemadev.Finance.TransactionListLava" %>

<style>
    .entry-date {
        position:absolute;
        color: #872634;
        height: 54px;
        text-align: center;
        padding-top: 7px;
        font-size: 15px;
        line-height: 1;     
    }

    .entry-date span {
        display: block;
        font-size: 25px;
        margin-top: 5px;
    }

    .gift-text {
        display:inline-block;
        font-size:16px;
        padding-left: 40px;
        margin-top:6px;
        width:100%;
    }

    .gift-text span {
      
    }

    .gift-box {
        min-height:60px;
        border-bottom:1px solid #ccc;
        margin-bottom:10px; 
        padding-bottom:10px;
    }

    .big-dropdown {
        width:100%;
        padding:12px;
        font-size:20px;
        margin-bottom:12px;
        color:#fff;
        background-color:#b7b1ac;
        border-radius:4px;
        -webkit-appearance: none;
        -moz-appearance: none;
        appearance: none;
    }

/* Targetting Webkit browsers only. FF will show the dropdown arrow with so much padding. */
@media screen and (-webkit-min-device-pixel-ratio:0) {
    select {padding-right:18px}
}

label {position:relative;
       width:100%;}
label:after {
    content:"\f107";   
    font-family: "FontAwesome";
    font-size: 20px;
    right:20px; top:12px;
    color:#fff;
    padding:0 0 2px;
    position:absolute;
    pointer-events:none;
}
label:before {
    content:'';
    right:4px; top:0px;
    width:23px; height:18px;
    position:absolute;
    pointer-events:none;
    display:block;
}
</style>

<asp:UpdatePanel ID="upnlContent" runat="server">
    <ContentTemplate>
        <div class="panel panel-block">
            <div class="panel-body">
                <div class="accounts-default">
                    <label>
                    <asp:DropDownList ID="ddlTimeFrame" runat="server" OnSelectedIndexChanged="ddlTimeFrame_SelectedIndexChanged" AutoPostBack="true" CssClass="big-dropdown">
                        <asp:ListItem Text="Last 3 Months" Value="1"></asp:ListItem>
                        <asp:ListItem Text="Last Month" Value="2"></asp:ListItem>
                        <asp:ListItem Text="Current Year" Value="3"  Selected="True" ></asp:ListItem>
                        <asp:ListItem Text="Last Year" Value="4"></asp:ListItem>
                    </asp:DropDownList>
                    </label>

                    <asp:Panel ID="pnlContent" runat="server">
                      
                        <asp:Literal ID="lLavaOutput" runat="server" />
                    </asp:Panel>

                    <asp:Literal ID="lDebug" runat="server" />

                </div>
            </div>
        </div>
    </ContentTemplate>
</asp:UpdatePanel>






