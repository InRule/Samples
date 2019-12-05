Decision Services
====

Decision Services is the newest member of InRule®'s Decision Platform. It provides an improved decision-centric model for authoring and executing rules, as well as an integration model that is standards-based that does not require deep knowledge of our SDK.

A Decision in Decision Services is an entry point into rule exection. It consists of a set of inputs, rules that execute against those inputs, and a set of ouputs that reflect the result. Those inputs, outputs, and rules are author-defined, ultimately providing rule authors flexibility of shaping the input and output signature used during runtime execution.

Once authored is complete, Decisions can be tested in irVerify and subsequently published to a Decision Runtime for remote execution via a RESTful API. The Decision Runtime provides an OpenAPI (formerly known as Swagger) document which is a programming language-agnostic description on how to execute the published Decisions, including each Decision's required input and output result.

# Prerequisites

Before you get started, you'll need to make sure you have the following:

* For authoring and testing Decisions, [irAuthor 5.5.0](https://support.inrule.com/downloads.aspx) or greater.

* For publishing and executing Decisions, access to a [Decision Runtime Service](#decision-runtime-service) using [Centralized Authentication](#centralized-authentication). Please contact [InRule Support](mailto:support@inrule.com) to request access.

### Decision Runtime Service

The Decision Runtime Service is responsible for storing and executing published Decisions. It also provides several OpenAPI documents based on the [OpenAPI specification](https://www.openapis.org/) (both [2.0](https://github.com/OAI/OpenAPI-Specification/blob/master/versions/2.0.md) and [3.0](https://github.com/OAI/OpenAPI-Specification/blob/master/versions/3.0.2.md)) as well as a [SwaggerUI](https://swagger.io/tools/swagger-ui/) interface. This allows integration with 3rd party tools like [Postman](https://www.getpostman.com/) or generating client code based on your published decisions.

### Centralized Authentication

Instead of relying on an irCatalog instance for authentication, InRule's hosted Decision Runtime Service integrates with its Centralized Authentication solution (running on the [Auth0](https://auth0.com/) platform). This authentication solution provides customers the ability to authenticate using external authentication providers including Azure Active Directory or any OpenID Connect compatible provider. 

# Getting Started

- [Authoring a Decision](author-decision.md)
- [Testing a Decision](test-decision.md)
- [Publishing a Decision](publish-decision.md)
- [Executing a published Decision](execute-decision.md)

# Integrations

- [Power Automate (previously Microsoft Flow)](integrations/power-automate.md)

# Notes

InRule v5.5.0 includes additional classes in irSDK® to support Decisions. To make upgrading as painless as possible, the new data structures are persisted to files or irCatalog® in Attributes of the RuleApplicationDef.

This removes the need to running a feature version upgrade on irCatalog to use these new features.

Additionally the restriction on unique Entity Rule Set names has been removed, so long as they don't collide with existing Independent Rule Sets. This allows Rul eSets with the same names to exist under both Entities and Decisions.

# Limitations

* The majority of Rule Application authoring scenarios should inter-operate between versions 5.3.1-5.4.3 and 5.5.0 of irSDK/irAuthor/irCatalog. Howevever, there are [several edge cases](known-issues.md#mix-version-use-of-irsdkirauthorircatalog) that are not supported at this time.
