Decision Services
====

Decision Services is the newest member of InRule®'s Decision Platform. It provides an improved, simpler model for authoring and executing rules, as well as an integration model that is standards-based that does not require deep knowledge of our SDK.

A decision in Decision Services is an entry point into rule exection. It consists of a set of inputs, rules that execute against those inputs, and a set of ouputs that reflect that result. Those inputs, outputs, and rules are author-defined, ultimately providing rule authors flexibility of shaping the input and output signature using during runtime execution.

Once authored is complete, Decisions can be tested in irVerify and subsequently published to a Decision Runtime for remote execution via a RESTful API. That Decision Runtime provides an OpenAPI (formerly known as Swagger) document which is a programming language-agnostic description on how to execute the published decisions, including each decision's required input and output result.

# Prerequisites

Before you get started, you'll need to make sure you have the following:

* For authoring and testing Decisions, [irAuthor 5.5.0](https://support.inrule.com/downloads.aspx) or greater.

* For publishing and executing Decisions, you will need TODO.

# Getting Started

- [Authoring a Decision](author-decision.md)
- [Testing a Decision](test-decison.md)
- [Publishing a Decision](publish-decision.md)
- [Executing a Published Decision](execute-decision.md)