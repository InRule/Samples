# InRule Samples

## User Defined Functions

Occasionally, you may need some processing that is more specialized than the standard InRule feature set. Often, this need can be met by writing your own User Defined Function.

The `UDF Examples.ruleappx` file contains a selection of sample User Defined Functions.

### String Library

| Function | Description |
|-------|------------|
| GetElapsedTimeFromSeconds | Returns elapsed time from a supplied number of seconds. |
| GetListItemByIndex | Returns the selected item from a comma separated list of text splitting on the comma. 0 based indexing. Passing in "4" gets the 5th word from the list. |
| GetListItemCount | Returns the count of items from a comma separated list of text splitting on the comma. |
| GetMatchText | Returns the matching text based on the supplied regular expression pattern. |
| GetNumberAfterMatchText | Returns the first number after the matched text. |
| GetNumberInText | Returns the first instance of a number in the supplied text. |
| GetTextAfter | Returns the text after  the first found match for the supplied match text. |
| GetTextAfterLast | Returns the text after the last found match for the supplied match text. |
| GetTextBefore | Returns the text before the first found match for the supplied match text. |
| GetTextBeforeLast | Returns the text before the last found match for the supplied match text. |
| GetTextBetween | Returns the text between the supplied start and end text. |
| GetTextBetweenInclusive | Returns the text between the supplied start and end text, including the start and end text. |
| GetTextBetweenMatchedDelimiters | Returns the text between the supplied start and end delimiters. If inclusive = true, it returns the text along with the start and end delimiters. |
| IsFoundInList | Looks for text in a comma separated list of values and returns true if found and false if not found. |
| IsNumeric | Determines if supplied text is numeric. If numeric, returns true, else false. |
| ParseDateString | Returns a datetime from a string that can be resolved to a date. |
| ReplaceTextBetweenMatchedDelimiters | Replaces the text between the supplied start and end delimiters. If inclusive = true, the delimiters are also replaced. |
| StripLeadingText | Returns the supplied text with the supplied leading text removed. |
| LoadXmlToEntity | Loads the supplied XML into the current entity. |
| CountWords | Returns the count of the instances of a given word, e.g. "abc" in a supplied text block. |
| DoStringsMatch | Determines if supplied strings match based on the supplied boolean "CaseInsensitive" parameter. If the strings match, returns true, else false. |

### Rule Library

| Function | Description |
|-------|------------|
| AreAllRulesTrueByRuleSetName | Returns true if all rules within the provided rule set have evaluated to true. |
| AreAllRulesTrueInRuleSet | Returns true if all rules within the current rule set evaluated to true. |
| GetParentLanguageRule | Navigates up to see if any parent in the tree is a language rule, stopping at the rule set. Returns the name of the language rule, if found. |
| GetParentRule | Returns the rule name of the parent rule. |
| GetRuleCondtion | Returns the rule condition expression of the provided rule. |
| GetRuleEval | Returns the boolean evaluation value of the provided rule. |
| GetRuleName | Returns the rule name for the currently executing rule. |
| GetRuleNames | Returns the rule names in a comma separated list for the currently executing rule set. |
| GetRuleSetName | Returns the rule set name for the currently executing rule set. |
| GetRuleSetsWithCategory | Returns the rule set names in a comma separated list for the rule sets tagged with the supplied categoryName. |
| IsLanguageRule | Returns true if the supplied rule is a language rule type. |
| IsRuleSet | Returns true if the supplied rule element is a rule set type. |
| GetCurrentRuleVersion | Returns the current active version of the supplied rule in the supplied rule set. |
| ExecuteRuleSetbyRuleSetName | Executes the supplied Explicit Entity Rule Set. |
| ExecuteRuleSetbyRuleSetNameWithParameters | Executes the supplied Explicit Entity Rule Set with parameters. |
| ExecuteIndependentRuleSetbyRuleSetName | Executes the supplied Independent Rule Set. |
| ExecuteIndependentRuleSetbyRuleSetNameWithParameters | Executes the supplied Independent Rule Set with parameters. |
| ExecuteFireNotificationRuleSet | Executes a rule set containing only a Fire Notification. Built to allow notifications to be in-lined within UDFs. Rule set parameter includes the message text. |
| ActivateRuleSetbyRuleSetName | Activates the supplied Rule Set. |

### Schema Library

| Function | Description |
|-------|------------|
| IsFieldValid | Returns true if the field is valid, and false if invalid. |
| SetFieldValue | Sets the value of the supplied field with the supplied value. |
| GetFieldValue | Returns the value of the supplied field. |
| GetFieldCountFromDef | Returns the count of fields on the current entity. |
| AllEntityFieldsAreValid | Returns true if all of the fields on the entity are valid, and false if any are invalid. |
| GetEntityRuleSetNames | Returns a comma separated list of rule set names within the current entity. |
| GetEntityFieldValues | Returns a list of field names and values for all fields within the current entity. |
| SetCollectionMemberValues | Sets the value of the supplied collection field for the supplied member index with the supplied value. Returns string "success" if successful and "Could not update value" if unsuccessful. |
| GetFieldProperty | Returns the value of the supplied property from the supplied field. |
| SetFieldProperty | Sets the value of the supplied property on the supplied field. If doesn't exist, creates the property. |
| ListProperties | Returns a list of properties and their values on the supplied field. |
| GetValueFromValueList | Returns the "value" column from the supplied value list and display value. |
| LoopThroughCollection | Returns a list of values for the supplied field on the supplied collection. |
| GetEntityDisplayName | Returns the entity display name for the supplied entity. |
| GetFieldAttributeValue | Returns the value of the supplied attribute key on the supplied field. |
| CreateDictionary | Creates and stores a dictionary with the supplied name for use in lookups. |
| LoadDictionary | Gets supplied dictionary and adds supplied key and value pair to the lookup dictionary. |
| GetDictionaryValue | Returns the value for the supplied dictionary key. |
| CreateEntityandAssignValues | Creates a new entity for the supplied entity name and sets the supplied value for the supplied field. |
| IsDaylightSavingTime | Returns true if the supplied date falls within Daylight Savings Time. |

### I/O Library

| Function | Description |
|-------|------------|
| GetValidPath | Returns the supplied file path with invalid characters stripped out and replaced with "_". |
| GetValidFileName | Returns the supplied file name with invalid characters stripped out and replaced with "_". |
| LoadFileToText | Returns a string with the text from the supplied file. |

### Function Library

| Function | Description |
|-------|------------|
| CallExternalFunction | Calls a specific external function. This function adds the supplied numbers together and returns the result. |
