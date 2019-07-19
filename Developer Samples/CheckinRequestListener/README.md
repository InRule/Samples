This assembly injects a an Endpoint Behavior into the WCF pipeline that allows custom code to be executed upon checkin of a Rule Application into a Catalog. In this sample, the assembly will trigger an Azure DevOps pipeline to queue a build upon checkin of a Rule App that matches a configured name and/or label.

To deploy, in the irCatalog service configuration file (Program Files (x86)\InRule\irServer\RepositoryService\IisService\Web.config), you will need to make 4 changes:

In the configuration file, add the following AppSettings (or reference a separate config using configSource="InRule_app.config"):
```html
	<appSettings>
		<add key="AzureDevOpsPipelineOrg" value="OrganizationName" />
		<add key="AzureDevOpsPipelineProject" value="ProjectName" />
		<add key="AzureDevOpsPipelineID" value="2" />
		<add key="AzureDevOpsAuthToken" value="xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx" />
		<add key="AutoPromoteRuleAppsNamed" value="MultiplicationApp|AdditionalRuleAppNameHere" />
		<add key="AutoPromoteRuleAppsLabeled" value="LIVE|Production" />
	</appSettings>
```

Just before the /system.serviceModel tag, add a new section for the extension with the following:
```html
	<extensions>
		<behaviorExtensions>
			<add name="ruleAppCheckinBehavior" type="CheckinRequestListener.RuleApplicationCheckinBehavior, CheckinRequestListener" />
		</behaviorExtensions>
	</extensions>
```

Add a new endpoint behavior to the system.serviceModel section after the /serviceBehaviors tag and before the /behaviors tag with the following:
```html
	<endpointBehaviors>
		<behavior name="RuleAppCheckin">
			<ruleAppCheckinBehavior />
		</behavior>
	</endpointBehaviors>
```

Find the system.serviceModel.Services.Service entry (there should only be one).  For each Endpoint entries inside that Service that are used for Catalog purposes (there should be only one or two), add the following property:
```html
behaviorConfiguration="RuleAppCheckin"
```
