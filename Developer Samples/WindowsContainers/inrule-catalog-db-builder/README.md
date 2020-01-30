# InRule Catalog Database Builder

This image runs the InRule.Catalog.Service.Database utility included with [InRule Azure AppServices Releases](https://github.com/InRule/AzureAppServices/releases). 

## Instructions for use

1. Ensure that the destination host is accessible from the container host and is running SQL Server 2008+
2. Create an empty database instance on target server (e.g., `CREATE DATABASE InRuleCatalog`)
3. Craft a standard connection string ex: `Server=tcp:192.168.0.4,1433;Initial Catalog=InRuleCatalog;User ID=sa;Password=<a password>`
4. Run the image, passing the connection string as the `ConnectionString` environment variable:
`docker run --rm --env ConnectionString='Server=tcp:192.168.0.4,1433;User ID=sa;Password=asdf1234;Initial Catalog=InRuleCatalog' inrule/catalog-db-builder:latest`