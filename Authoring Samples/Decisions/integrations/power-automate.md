Integrate with Power Automate (previously Microsoft Flow)
====

The Decision Runtime Service for Decisions Services provides an OpenAPI 2.0 document specific for Power Automate. That document can be found at a url similar to:

`https://myorg-decisions.inrulecloud.com/api/decisions/powerautomate.json`

With that document (either by using the URL or saving it locally) you can create a custom connector that can be used in Flows to execute Decisions.

You can find information on how to create a customer connector for Power Automate from Microsoft [here](https://docs.microsoft.com/en-us/connectors/custom-connectors/define-openapi-definition#import-the-openapi-definition-for-microsoft-flow-and-powerapps).

When you get to the step of "[Review authentication Type](https://docs.microsoft.com/en-us/connectors/custom-connectors/define-openapi-definition#import-the-openapi-definition-for-microsoft-flow-and-powerapps)", you can use the following settings (some will be populated automatically):

* **Authentication Type**: `OAuth 2.0`

* **OAuth 2.0**
  * **Identity Provider**: `Generic Oauth 2`
  * **Client id**: `CLIENT_ID_GOES_HERE`<br />
    This will be provided to you when granted access to a Decision Runtime Service environment. Please contact [InRule Support](mailto:support@inrule.com) to request access.
  * **Client secret**: `CLIENT_SECRET_GOES_HERE`<br />
    This will be provided to you when granted access to a Decision Runtime Service environment. Please contact [InRule Support](mailto:support@inrule.com) to request access.
  * **Authorization URL**: `https://auth.inrulecloud.com/authorize`<br />
    The API authorization endpoint to authenticate with the service.
  * **Token URL**: `https://auth.inrulecloud.com/oauth/token`<br />
     The API endpoint to get the access token after the authorization has been done.
  * **Refresh URL**: `https://auth.inrulecloud.com/oauth/token`<br />
    The API endpoint to refresh the access token once it has expired. (This is the same as the Token URL)
  * **Scope**: `offline`
    Requires `offline` in order to provide a refresh token.

Once those settings are in place, you should be able to create the connector and use that custom connector in your [logic apps](https://docs.microsoft.com/en-us/connectors/custom-connectors/use-custom-connector-logic-apps), [flows](https://docs.microsoft.com/en-us/connectors/custom-connectors/use-custom-connector-flow), or [PowerApps apps](https://docs.microsoft.com/en-us/connectors/custom-connectors/use-custom-connector-powerapps). Please refer to the [Microsoft documentation](https://docs.microsoft.com/en-us/connectors/custom-connectors/) for more information on custom connectors and Power Automate. 
