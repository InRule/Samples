Integrate with Power Automate (previously Microsoft Flow)
====

You can find information on how to create a customer connector for Power Automate from Microsoft [here](https://docs.microsoft.com/en-us/connectors/custom-connectors/define-openapi-definition).

Before creating a customer connector you will need:
* **Client id**: The client id of the application you have registered with the service.
* **Client secret**: The client secret of the application you have registered with the service.
* **Refresh Url**: The API endpoint to refresh the access token once it has expired. (This is the same as the Token URL below)

The following values will be automatically generated for you from the **Execution API for Power Automate** definition file: 
* **Authorization Url**: The API authorization endpoint to authenticate with the service.
* **Token Url**: The API endpoint to get the access token after the authorization has been done.
