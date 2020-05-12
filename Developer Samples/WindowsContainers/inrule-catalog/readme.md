# InRule Samples

## README for the irCatalog Docker file

### Important notes on building the image

* The DOCKERFILE requires a build argument specifying the specific tag to use when acquiring assets from the [InRule assets repos]( https://github.com/InRule/AzureAppServices/releases/download)

* See [inrule-server](../inrule-server/) documentation for information on licensing

#### Environment Variables

* Specify the irCatalog database connection string with the  `inrule:repository:service:connectionString` environment variable

Note that the catalog database must already be present with schema before the catalog service will be fully operational.

### (optional)  use pre-built catalog DB image

The [microsoft/mssql-server-windows-express](https://hub.docker.com/r/microsoft/mssql-server-windows-express/) image can be used to quickly stand up an InRuleCatalog database instance. For convenience purposes, a pre-built image pre-populated with the irCatalog database schema is available for selected versions of irCatalog (v5.1.1+) on [Docker Hub](https://hub.docker.com/r/inrule/inrule-catalog-db/).

> ⚠ SQL in Windows Containers is not supported for production scenarios

Microsoft has discontinued support and release of Windows container versions of SQL Server in favor of the [mssql linux](https://hub.docker.com/_/microsoft-mssql-server) images, so future support for pre-built Windows SQL DB images is not planned.

### Running the image

Place the `InRuleLicense.xml` file in a location where the IIS process inside the docker container will be able to read it (e.g. not under a user's home directory)

```cmd
docker run -d -p 443 -p 80 --name cat --env irLogLevel=Info --env inrule:repository:service:connectionString='Server=tcp:irdb,1433;Initial Catalog=InRuleCatalog;User ID=sa;Password=<SA_PASSWORD>' -v '<HOST_LICENSE_DIRECTORY>:C:\ProgramData\InRule\SharedLicenses:ro' -v '<HOST_PFX_DIRECTORY>:C:\inrule-catalog\pfx:ro' --env PfxPassword=<Password_for_cert> inrule/inrule-catalog:latest
```

When running this image, you'll need to supply all required and any optional environment parameters.
