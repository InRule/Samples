# Functional Samples

This folder contains a variety of Rule Apps that each demonstrate a specific functionality that may be applied in a variety of applications.

## Large Lookup Data Table
One of the limitations of Collections is that their performance begins to deminish as they grow larger in size.  For instances where you may have a need to load a large amount of reference data at runtime (IE load in a large list of valid IDs from a database and verify that all records in a batch use a valid ID), you can use UDFs to access a dictionary of that data stored as a Context Property and see performance improvements by an order of magnitude.
