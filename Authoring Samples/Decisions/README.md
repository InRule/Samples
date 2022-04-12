Decisions
====

Decisions are the newest members of InRule®'s Decision Platform. They provide a different, decision-centric model for authoring and executing rules, as well as an integration model that does not require deep knowledge of our SDK.

A Decision is an entry point into rule execution. It consists of a set of inputs, rules that execute against those inputs, and a set of outputs that reflect the result. Those inputs, outputs, and rules are author-defined, ultimately providing rule authors flexibility of shaping the input and output signature used during runtime execution.

Once authoring is complete, Decisions can be tested in irVerify and subsequently consumed by the Rule Execution Service for remote execution via a RESTful API.

# Prerequisites

Before you get started, you'll need to make sure you have the following:

* For authoring and testing Decisions, [irAuthor 5.5.0](https://support.inrule.com/downloads.aspx) or greater.

# Getting Started

- [Authoring a Decision](author-decision.md)
- [Testing a Decision](test-decision.md)
- [Executing a Decision](execute-decision.md)

# Notes

InRule v5.5.0 includes additional classes in irSDK® to support Decisions. To make upgrading as painless as possible, the new data structures are persisted to files or irCatalog® in Attributes of the RuleApplicationDef.

This removes the need to running a feature version upgrade on irCatalog to use these new features.

Additionally the restriction on unique Entity Rule Set names has been removed, so long as they don't collide with existing Independent Rule Sets. This allows Rule Sets with the same names to exist under both Entities and Decisions.

# Limitations

* The majority of Rule Application authoring scenarios should inter-operate between versions 5.3.1-5.4.3 and 5.5.0 of irSDK/irAuthor/irCatalog. However, there are [several edge cases](known-issues.md#mix-version-use-of-irsdkirauthorircatalog) that are not supported at this time.
