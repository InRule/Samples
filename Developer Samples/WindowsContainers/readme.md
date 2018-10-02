# InRule Samples

## InRule Docker images

## Image descriptions

* [inrule-server](inrule-server/) image is used as a base image for the other InRule server- based container components. It exists to ensure that the necessary WCF and IIS components are present and available in the target container.

* The [inrule-catalog](inrule-catalog/) container image encapsulates the InRule irCatalog service, providing source control facilities to rules authors and runtime services alike.

* Runtime rule execution is handled by [inrule-runtime](inrule-runtime/). This is the irServer Rule Execution Service, and offers both a REST- and a SOAP- based endpoint for executing rules against data passed into it.

* Catalog management is provided by [inrule-catalog-manager](inrule-catalog-manager/). RuleApplications can be viewed, labeled, and promoted. Users can be created and modified along with permissions.

* Although most flavors of SQL are supported (SQL Server, MySQL, Oracle, etc.) irCatalog database persistence is provided by SQL Server Express and comes from [microsoft/mssql-server-windows-express](https://github.com/Microsoft/sql-server-samples/tree/master/samples/manage/windows-containers/mssql-server-2016-express-sp1-windows)

### Image repository and registry

Images can be built using the respective dockerfiles in this repository. Take care to protect your license key file by avoiding pushing images containing a license key file to public repositories

### Building the images

Here are the steps to building these images from their base assets. To build the sample images using the included build script:

1. Clone the repository into a working directory

2. Run the [build.ps1](/build.ps1) script, passing in the name of the tag you'd like to use:

`.\build.ps1 -tag 'v5.1.1'`

If you want to also have the images tagged as 'latest', pass `-SetLatestTag` to the script:

`.\build.ps1 -tag 'v5.1.1' -setLatestTag`

To skip building the `inrule-server` base image, pass the `skipServerBuild` switch to the build script.

For instructions on building a set of images using Compose, see the section below on **Using Docker Compose to provision a rule execution environment**

Please see the instructions for each respective image for information on how to build the individual images.

## Installation and configuration

All of the images in this repository are Windows-based. Depending on whether you are using Windows 10 or Windows Server,
you can find instructions on how to get started with Containers [here](https://msdn.microsoft.com/en-us/virtualization/windowscontainers/quick_start/quick_start).
Once you've followed all of the steps in a respective setup guide and verified things are working properly, you'll be ready to use the InRule images!

> Note: make sure to switch your running Dockerd from Linux to Windows containers!

## Using Docker Compose to provision a rule execution environment

### About the compose file

The `docker-compose.yml` file in the root of this repository describes the resources, services, that comprise a "vanilla" rule execution environment.
It contains the following components (container alias are in parenthesis):

* Rules execution service (rex)
* Catalog service (cat)
* SQL Express Db (db)

### Setting up the container environment

There are a lot of variables involved in configuring a rule exuection environment. Wherever possible, sensible defaults have been applied to reduce the number and locations where these variables are needed.
The file `docker-compose.yml` contains information about the containers to run and also environmental information that is specific to a given instance of a container. To override these defaults or specify additional settings, create  a new file in the same directory as `docker-compose.yml` named `docker-compose.override.yml`. Any values found in the .override file will take precedence over values in the base docker-compose file.

<!-- 
### Sample `docker-compose.override.yml` file

```data
version: '2.4'

services:
    rex:
        ports:
            - "80:8020"
            - "443:8021"
    cat:
        environment:
            - inrule:repository:service:connectionString "tcp:1433,https://mydb.cloudsomething.net;User Id=sa;Password=12345;Initial Catalog=InRuleCatalog"
```

Environment-specific variables are listed below each with a short description:

* TAG
  * The image tag to use. This should correspond to either `latest` or to a specific version of InRule (e.g., `5.0.16`).
* sa_password
  * Used by the db container to set the SA account password. The value of this is shared with the `catalogPassword` variable (TODO: consolidate variables)
  * Will be set as the **sa** SQL account's password when the **db** container is started. If an existing database is being attached, that databases' sa password will be reset to the provided value.
* CatalogUser
  * The name of the SQL login that the catalog service will use to connect to the catalog database.
* CatalogPassword
  * Same value as `sa_password`, but used by the `cat` container to connect to the Db.
* irCatalogDir (build-time)
  * Specifies the path on the container host to use for copying files needed by the irCatalog web service image.
  * This is one way to run an older version of InRule while still using the latest Docker image.
* irRuntimeDir (build-time)
  * Specifies the path to use when building the rule execution service image.
  * Behaves identically to **irCatalogDir**
* attach_dbs
  * Takes advantage of a host-shared volume mounted at `c:\inrule-catalog-db` on the host, and at `c:\data\` in the container.
  * Copy .MDF and .LDF files into the host's directory
  and they'll be available to the container.
  * A JSON string containing any pre-existing databases to attach to the Db container on startup. Typically, you will specify paths to an already-existing InRuleCatalog Db
  * Example: ```attach_dbs="[{'dbName':'InRuleCatalog', 'dbFiles':['C:\\data\\InRuleCatalog.mdf', 'C:\\data\\InRuleCatalog_log.ldf' ]}]"``` -->

### Usage

#### Start the environment

```cmd
docker-compose up
```

#### Stopping the environment

```cmd
docker-compose down
```