<%@ Control Language="C#" AutoEventWireup="true" CodeFile="SampleReactBlock.ascx.cs" Inherits="RockWeb.Blocks.Examples.SampleReactBlock" %>

<h1>Experimental Feature</h1>
<p>
    The use of React with Rock is still in an experimental state. Use this block, and React.js with Rock, with the understanding
    that this could change / break at any release. No official JavaScript front-end framework has been selected for use in Rock
    core. 
</p>
<asp:Literal ID="PageContent" runat="server"></asp:Literal>