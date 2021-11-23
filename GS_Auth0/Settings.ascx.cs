/*
' Copyright (c) 2019 Glanton
'  All rights reserved.
' 
' THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED
' TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL
' THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF
' CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER
' DEALINGS IN THE SOFTWARE.
' 
*/

using System;
using DotNetNuke.Services.Exceptions;
using DotNetNuke.Entities.Modules;
using DotNetNuke.Services.Authentication;
using DotNetNuke.Common.Utilities;
using DotNetNuke.Services.Authentication.OAuth;
using DotNetNuke.UI.WebControls;
using GS.Auth0.Components;
using System.Threading.Tasks;
using System.Web.UI.WebControls;
using DotNetNuke.Services.Localization;

namespace GS.Auth0
{
    /// <summary>
    /// Class Settings.
    /// Implements the <see cref="DotNetNuke.Services.Authentication.AuthenticationSettingsBase" />
    /// </summary>
    /// <seealso cref="DotNetNuke.Services.Authentication.AuthenticationSettingsBase" />
    public partial class Settings : AuthenticationSettingsBase
    {
        /// <summary>
        /// The settings editor
        /// </summary>
        protected PropertyEditorControl SettingsEditor;

        /// <summary>
        /// Gets the name of the authentication system application.
        /// </summary>
        /// <value>The name of the authentication system application.</value>
        protected string AuthSystemApplicationName
        {
            get { return Constants.PROVIDER_NAME; }
        }

        /// <summary>
        /// Updates the settings.
        /// </summary>
        public override void UpdateSettings()
        {
            if (SettingsEditor.IsValid && SettingsEditor.IsDirty)
            {
                var config = (Auth0ConfigBase)SettingsEditor.DataSource;                
                Auth0ConfigBase.UpdateConfig(config);
            }
        }

        /// <summary>
        /// Handles the <see cref="E:Load" /> event.
        /// </summary>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            Auth0ConfigBase config = Auth0ConfigBase.GetConfig(AuthSystemApplicationName, PortalId);
            SettingsEditor.DataSource = config;
            SettingsEditor.DataBind();
        }

        /// <summary>
        /// Handles the Click event of the exportUsersButton control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void exportUsersButton_Click(object sender, EventArgs e)
        {
            if (exportUsersToken.Text.Length == 0)
            {
                exportUsersResult.Text = "Invalid token";
                return;
            }
            exportUsersResult.Text = "Export In Progress";
            var exporter = new DnnUserExporter();
            var result = exporter.ExportUsers(PortalId, exportUsersToken.Text, out int successCount, out int userCount);
            switch (result)
            {
                case (UserExportStatus.Success):
                    exportUsersResult.Text = Localization.GetString("ExportUsersFeedback.Success", LocalResourceFile);
                    break;
                case (UserExportStatus.Mixed):
                    var localizedStr = Localization.GetString("ExportUsersFeedback.Mixed", LocalResourceFile);
                    exportUsersResult.Text = string.Format(localizedStr, successCount, userCount);
                    break;
                case (UserExportStatus.Failure):
                    exportUsersResult.Text = Localization.GetString("ExportUsersFeedback.Failure", LocalResourceFile);
                    break;
            }
        }
    }

}

