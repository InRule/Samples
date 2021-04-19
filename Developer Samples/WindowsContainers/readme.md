# InRule Samples

## InRule Docker images

### ***Important note regarding licensing***

Some of the images in this repository require a valid InRule license file to be supplied on the host machine. Machine-specific (including evaluation licenses) will result in runtime errors when used for these purposes; please request and use an azure-appropriate license file. Azure license files can be obtained from your InRule Support Admin, or by contacting [support@inrule.com](mailto:support@inrule.com) with your customer information. See the individual image descriptions and README's for information on how to make this license file available to containers running on a host machine.

#### Troubleshooting license-related exceptions

##### Exception

"An unexpected server exception has occurred: The license for [PRODUCT NAME] is not valid. The machine's name ([CONTAINER HOSTNAME]) does not match the licensed machine name."

##### Explanation and resolution

This exception is thrown when attempting to mount a machine-specific license file into a container. Contact Support or your InRule Support Admin to obtain a valid Azure license key file.

## Image descriptions

* [inrule-server](inrule-server/) image is used as a base image for the other InRule server- based container components. It exists to ensure that the necessary WCF and IIS components are present and available in the target container.

* The [inrule-catalog](inrule-catalog/) container image encapsulates the InRule irCatalog service, providing source control facilities to rules authors and runtime services alike.

* Runtime rule execution is handled by [inrule-runtime](inrule-runtime/). This is the irServer Rule Execution Service, and offers both a REST- and a SOAP- based endpoint for executing rules against data passed into it.

* Catalog management is provided by [inrule-catalog-manager](inrule-catalog-manager/). RuleApplications can be viewed, labeled, and promoted. Users can be created and modified along with permissions.

* Most flavors of SQL are supported (SQL Server, MySQL, Oracle, etc.). Ensure that you allow port 1433 access between your container network and the database server.

### Image repository and registry

Images can be built using the respective dockerfiles in this repository. Take care to protect your license key file by avoiding pushing images containing a license key file to public repositories

### Building the images

Here are the steps to building these images from their base assets. To build the sample images using the included build script:

1. Clone the repository into a working directory

2. Run the [Invoke-ContainerBuild.ps1](/Invoke-ContainerBuild.ps1) script, passing in the name of the tag you'd like to use:

`.\Invoke-ContainerBuild.ps1 -inruleReleaseTag v5.1.1 -Registry mycustomcr.azurecr.io/inrule -imageTags myawesomeTag1, myTag2`

Image will be built and tagged with a suffix containing the base OS build and the version of InRule, ex:

> inrule/inrule-catalog:myTag2-v5.5.1-ltsc2019

## Installation and configuration

All of the images in this repository are Windows-based. Depending on whether you are using Windows 10 or Windows Server,
you can find instructions on how to get started with Containers [here](https://msdn.microsoft.com/en-us/virtualization/windowscontainers/quick_start/quick_start).

> ⚠ Note: Windows containers do not support the mounting of individual files as volumes, only directories

Once you've followed all of the steps in a respective setup guide and verified things are working properly, you'll be ready to use the InRule images!

> 👉 Note: make sure to switch your running Docker from Linux to Windows containers!

## Configuring SSL

The container images support communication over HTTPS. To configure, you'll need a PFX certificate file and its' corresponding password.

1. Define a volume mount that places the PFX file into a *subfolder* of the container's application folder (`$env:ContainerDir`, e.g., `C:\inrule-catalog\PFX`)

2. Populate the `PfxPassword` environment variable of the container with the password to the PFX file

3. Map SSL traffic to container port 443 using `-p 443:443` (or equivalent)

## Setting logging levels

Containers are configured to output their logs to stdout using Microsoft's [LogMonitor](https://github.com/microsoft/windows-container-tools/tree/master/LogMonitor). This includes IIS (wc3svc), the Windows Application and System Event Logs, and irSDK logging. The LogMonitor executable and its' `logmonitorconfig.json` are located in the container's `C:\LogMonitor` folder.

irSDK logging levels can be set via the irLogLevel environment variable. Possible values: `Debug`, `Info`, `Warn`, `Error`, and `Fatal`. Higher levels of verbosity incur greater runtime performance impact.

## Using Docker Compose to provision a rule execution environment

### About the compose file

The `docker-compose.yml` file in the root of this repository describes the resources, services, that comprise a "vanilla" rule execution environment.
It contains the following components (container alias are in parenthesis):

* Rule execution service (rex)
* Catalog service (cat)
* SQL Express Db (db) _Note: Microsoft no longer maintains the Windows Container version of this image. Consider using the Linux flavor of this container instead. You will need to manually create the database and then run the [catalog-db-builder](/inrule-catalog-db-builder/) container image (or utility).

### Setting up the container environment

There are a lot of variables involved in configuring a rule execution environment. Wherever possible, sensible defaults have been applied to reduce the number and locations where these variables are needed.
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
docker-compose -f .\docker-compose.yml -f .\docker-compose.override.yml up
```

#### Stopping the environment

```cmd
docker-compose -f .\docker-compose.yml -f .\docker-compose.override.yml down
```