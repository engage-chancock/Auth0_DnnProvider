<%@ Control Language="C#" AutoEventWireup="false" Inherits="GS.Auth0.Settings, GS.Auth0" Codebehind="Settings.ascx.cs" %>
<%@ Register TagPrefix="dnn" Namespace="DotNetNuke.UI.WebControls" Assembly="DotNetNuke" %>
<dnn:propertyeditorcontrol id="SettingsEditor" runat="Server" 
    helpstyle-cssclass="dnnFormHelpContent dnnClear" 
    labelstyle-cssclass="SubHead" 
    editmode="Edit"
    SortMode="SortOrderAttribute"
    />
<span style="padding-left:4rem;" id="exportUsersLabel"><%= Localization.GetString("ExportToken", this.LocalResourceFile) %></span>
<asp:TextBox runat="server" ID="exportUsersToken" ></asp:TextBox>
<asp:Button runat="server" ID="exportUsersButton" OnClick="exportUsersButton_Click" Text="Export DNN Users" ></asp:Button>
<asp:Label runat="server" id="exportUsersResult" Text=""/>