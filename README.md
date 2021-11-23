# Auth0 Connector - DNN provider
A DNN provider utilizing Auth0 platform. Allows you to sign in to DNN using various accounts like: Google, Facebook, Twitter.

## Overview
The "Auth0 Connector" is a provider for DNN Platform that uses [Auth0 platform](https://auth0.com/) to authenticate users. Thanks to Auth0, that sits between DNN and the Identity Provider, users can easily utilize their Google, Facebook, etc, accounts in DNN website. At the login process new identity will be automatically created in DNN (username, displayname, email). Check [this Auth0 doc page](https://auth0.com/docs/connections) for list of all possible Identity Providers that you can connect.

"Auth0 Connector" is builded at the top of [OWIN standard](http://owin.org/)
    
## Table of Contents
* [Requirements](doc/Requirements.md)
* [Installation](doc/Installation.md)
* [Configuration](doc/Configuration.md)
    * [Configuration on the Auth0 platform](doc/Configuration.md#configuration-at-the-auth0-platform)
    * [Configuration on the DNN website](doc/Configuration.md#configuration-on-the-dnn-website)
    * [Exporting Your Users From DNN To Auth0](doc/Configuration.md#exporting-your-users-from-dnn-to-auth0)
    * [Configuring new DNN usernames to default to Auth0 usernames](doc/Configuration.md#defaulting-new-user-dnn-user-usernames-to-auth0-usernames)
* [Login flow](doc/LoginFlow.md)
* [Author](doc/Author.md)
* [License](doc/License.md)
