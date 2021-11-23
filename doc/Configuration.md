## Configuration
Configuration process is devided into two steps. First you need to configure connection on Auth0 side. After that coordinates generated on Auth0 (Domain, Client ID, Client Secret) needs to be applied in DNN website.
### Configuration at the Auth0 platform
First you need to have an account in Auth0 platform, where you can create new 'Regular Web Application'. This 'Application' will be some kind of proxy bridge between DNN and Identity Providers (Google, Facebook). Whole process is welly described on Auth0 doc, see here for reference [Register a Regular Web Application](https://auth0.com/docs/dashboard/guides/applications/register-app-regular-web).

Figure below presents typical 'Application'. Please copy paste following attributes: `Domain`, `Client ID`, `Client Secret`. See figure below for reference:
![alt text](https://raw.githubusercontent.com/BarryWaluszko/Auth0_DnnProvider/doc/doc/images/Auth0_Configure_02.png)

For **Application Type** select **Regular Web Application**.

Inside field **Allowed Callback URLs** enter DNN login url. See figure below for reference:
![alt text](https://raw.githubusercontent.com/BarryWaluszko/Auth0_DnnProvider/doc/doc/images/Auth0_Configure_03.png)

Inside filed **Allowed Logout URLs** enter DNN logoff url, usually it's something like http://MyDnnDomain.com/Logoff. See figure below for reference: 
![alt text](https://raw.githubusercontent.com/BarryWaluszko/Auth0_DnnProvider/doc/doc/images/Auth0_Configure_04.png)

### Configuration on the DNN website
Sign in to DNN as a Host user and go to the `Extensions` pane. Click on pencil icon where 'Auth0 Connector' is, see figure below for reference:
![alt text](https://raw.githubusercontent.com/BarryWaluszko/Auth0_DnnProvider/doc/doc/images/DNN_Configure_01.png)
Then select `Site Settings` tab. Fill the form according to figure below. Click `Update` button to keep the changes.
![alt text](https://raw.githubusercontent.com/BarryWaluszko/Auth0_DnnProvider/doc/doc/images/DNN_Configure_02.png)

Inspect the `web.config` file, check if under `<appSettings>` node is line from code below. It will enable OWIN in your DNN and marks 'Auth0 Provider' as an OWIN application. 
```
<add key="owin:AppStartup" value="GS_Auth0" />
```
![alt text](https://raw.githubusercontent.com/BarryWaluszko/Auth0_DnnProvider/doc/doc/images/DNN_Configure_03.png)

Toggle `Bypass DNN Login Page` if you would like clicks on the `Login` button to log in directly instead of going to the DNN login page.

### Exporting your users from DNN to Auth0
If you have users in DNN and would like to bulk export your users to DNN, you will need to setup your Management API on the Auth0 side.

Go to Auth0 in the **API** tab, go to the **Auth0 Management API** **API Explorer** and create a new test management application. After it is created, you will be presented with a text box where you can copy a code.
![managementapi](https://user-images.githubusercontent.com/90643304/142917722-d0723bfb-d2c5-4c69-9fde-1531e038b8af.PNG)

Now paste that token into the module settings and click the button to export. Be patient as it can take a couple minutes to complete depending on how many DNN users you have. Once complete, you will see some text to the right of the button indicating the results of the exports.

![export](https://user-images.githubusercontent.com/90643304/142923587-746665e1-8b58-4a33-8cb0-47e801f230cb.PNG)

Since this process takes some time, you may see an HTTP Timeout error depending on how your web.config is configured. If you get this problem, go to your web.config for your site and set your `executionTimeout` under `<httpRuntime>` to something around 300 as pictured below. If that doesn't work, increase the time some more, but 300 is 5 minutes so that should be plenty.

![timeout](https://user-images.githubusercontent.com/90643304/142924196-bda2475f-4aca-4b9e-a827-2cc8da017fdc.PNG)

### Defaulting new user DNN user usernames to Auth0 usernames
By default, when authenticating through Auth0, this module will look through the DNN users for a user with the same username as your Auth0 userId. This can be problematic
 if you already have DNN users and want them to be able to seamlessly login through Auth0 and keep their old DNN account when they log in. In order to mitigate this problem, you can create a rule in your Auth0 dashboard to send the Auth0 `preferred_username` which the module will then automatically use instead of the `user_id` for DNN user creation. In order to do so, go to your **Auth Pipeline -> Rules** section of Auth0. Click the `create` button to create a new rule and choose `Empty Rule`. Then paste the following code in the script box:
```
function (user, context, callback) {
			  user.preferred_username = user.username; //map username to OIDC standard claim
			  return callback(null, user, context);
			}
```

It should look like this.

![rule](https://user-images.githubusercontent.com/90643304/142925014-9707c81b-6114-415f-88c0-bbd2bda3bd04.PNG)

Save changes for the rule and you should be able to login to DNN afterwards with Auth0 authentication without the module creating a new DNN account on your first login.


