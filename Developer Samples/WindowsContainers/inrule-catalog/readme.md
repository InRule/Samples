# InRule Samples

## README for the irCatalog Docker file

### Important notes on building the image

* The DOCKERFILE requires a build argument specifying the specific tag to use when acquiring assets from the [InRule assets repos]( https://github.com/InRule/AzureAppServices/releases/download)

* See [inrule-server](../inrule-server/) documentation for information on licensing

#### Environment Variables

* Specify the irCatalog database connection string with the  `inrule:repository:service:connectionString` environment variable

Note that the catalog database must already be present with schema before the catalog service will be fully operational.

### (optional) Building the database

The [microsoft/mssql-server-windows-express](https://hub.docker.com/r/microsoft/mssql-server-windows-express/) image can be used to quickly stand up an InRuleCatalog database instance. For convenience purposes, a pre-built image pre-populated with the irCatalog database schema is available for selected versions of irCatalog (v5.1.1+) on [Docker Hub](https://hub.docker.com/r/inrule/inrule-catalog-db/). Note that this image is currently (as of v5.1.1) unsupported.

#### Docker run command to start a SQL DB container

```docker run -d -p:1433:1433 -e ACCEPT_EULA=Y -v c:\inrule-catalog-db\:c:\data\ -e sa_password=<SA_PASSWORD> microsoft/mssql-server-windows-express```

The `-v` option tells Docker to mount the contents of the given host directory - `c:\inrule-catalog-db\` - in the container as `c:\data\`. This faciliates transfer of database between containers. Connect to the SQL instance with any client such as SQL Server Management Studio and run the Catalog DB creation scripts (default location: `C:\Program Files (x86)\InRule\irServer\RepositoryService\DbBuildScripts`) to create the catalog database

#### Mount an existing database into a new container

```docker run... -e attach_dbs="[{'dbName':'InRuleCatalog', 'dbFiles': ['C:\\data\\InRuleCatalog.mdf', 'C:\\data\\InRuleCatalog.ldf']}]"... microsoft/mssql-server-windows-express```

### Building the image

You can build this image from source using a command similar to the following example:

`docker build -t inrule/inrule-catalog:5.0.26 .`

### Running the image

Place the `InRuleLicense.xml` file in a location where the IIS process inside the docker container will be able to read it (e.g. not under a user's home directory)

`docker run -d --name cat -e CatalogUser=sa -e CatalogPassword=<SA_PASSWORD> -e CatalogDbHost=<DB_HOST> -v '<HOST_LICENSE_DIRECTORY>:C:\ProgramData\InRule\SharedLicenses:ro' inrule/inrule-catalog:latest`

When running this image, you'll need to supply all required and any optional environment parameters.
