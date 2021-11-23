<%@ Control Language="C#" AutoEventWireup="false" Inherits="GS.Auth0.Settings, GS.Auth0" Codebehind="Settings.ascx.cs" %>
<%@ Register TagPrefix="dnn" Namespace="DotNetNuke.UI.WebControls" Assembly="DotNetNuke" %>
<%@ Register TagPrefix="dnnLabel" TagName="Label" Src="~/controls/LabelControl.ascx" %>
<dnn:propertyeditorcontrol id="SettingsEditor" runat="Server" 
    helpstyle-cssclass="dnnFormHelpContent dnnClear" 
    labelstyle-cssclass="SubHead" 
    editmode="Edit"
    SortMode="SortOrderAttribute"
    />
<div class="exportArea">
    <fieldset>
        <div class="dnnFormItem">
            <dnnLabel:Label runat="server" id="ExportUsersLabel" ResourceKey="ExportUsersLabel"/>
            <asp:TextBox runat="server" ID="exportUsersToken" Width="150" ></asp:TextBox>
            <asp:Button runat="server" ID="exportUsersButton" OnClick="exportUsersButton_Click" Text="Export DNN Users" ></asp:Button>
            <asp:Label runat="server" id="exportUsersResult" Text=""/>
        </div>
    </fieldset>
</div>