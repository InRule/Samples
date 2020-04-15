# Functional Samples

This folder contains a variety of Rule Apps that each demonstrate a specific functionality that may be applied in a variety of applications.

## Large Lookup Data Table
One of the limitations of Collections is that their performance begins to diminish as they grow larger in size.  For instances where you may have a need to load a large amount of reference data at runtime (IE load in a large list of valid IDs from a database and verify that all records in a batch use a valid ID), you can use UDFs to access a dictionary of that data stored as a Context Property and see performance improvements by an order of magnitude.

To run this Rule App, load the State from the json file and execute the Run* explicit RuleSets to see the difference in performance. 

## OAuth Token Sample

This Rule App was created in concert with a blog post to demonstrate two functionalities around making complex REST requests:

- Retrieve an OAuth Token during Rule Application execution (from Azure AD in this case)

- Execute another Rule Application using irServer REST Rule Execution Service with App Service Authentication enabled (and connected to Azure AD) using that Token as the Authorization header value

Executing another Rule Application via irServer during Rule Execution is not a common pattern, but it does come up occasionally, and it presents a great opportunity to demonstrate the usage of the Token retrieved as part of the first demonstrated functionality.

This Rule Application is intended to be used as a reference, since it relies on quite a few items in the infrastructure.  If you do want to run this Rule Application directly, you'll need to configure a number of items specific to your environment.

To run Demo_GetToken:
- Populate TennantID, Resource, ClientId, and ClientSecret in the Rule RootEntity.Demo_GetToken.SetDefaultCredentialInfoSample

To run Demo_RunRules:
- Update the Root URL in the End Point named IrServerRestEndpoint 
- Have default irCatalog credentials stored in your irServer instance
- Update the MultiplicationProblem entity and associated constructor, mapping, and logging logic to match the schema of a Rule Application that exists in your Catalog
- Update the RootEntity.RunRulesInternal.ExecuteRunRulesRequest (or ApplyRunRulesRequest) with appropriate information about a Rule Application that exists in your catalog

