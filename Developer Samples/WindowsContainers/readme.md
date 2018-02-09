# InRule Samples

## Sample Title

# InRule Docker images

## Image descriptions

* [inrule-server](inrule-server/) image is used as a base image for the other InRule server- based container components. It exists to ensure that the necessary WCF and IIS components are present and available in the target container.

* The [inrule-catalog](inrule-catalog/) container image encapsulates the InRule irCatalog service, providing source control facilities to rules authors and runtime services alike.

* Runtime rule execution is handled by [inrule-runtime](inrule-runtime/). This is the irServer Rule Execution Service, and offers both a REST- and a SOAP- based endpoint for executing rules against data passed into it.

* Catalog management is provided by [inrule-catalog-manager](inrule-catalog-manager/). RuleApplications can be viewed, labeled, and promoted. Users can be created and modified along with permissions.

* Catalog database persistence is provided by SQL Server Express and comes from [microsoft/mssql-server-windows-express](https://github.com/Microsoft/sql-server-samples/tree/master/samples/manage/windows-containers/mssql-server-2016-express-sp1-windows)

### Image repository and registry

Images can be built using the respective dockerfiles in this repository. Because the images produced embed the InRule License key into them, you should refrain from making images publically available.

### Building the images

To build all of the sample images using the included build script, first clone the repository into a working directory. Next, run the [build.ps1](/build.ps1) script in an elevated Powershell prompt, passing in the name of the tag you'd like to use:

`.\build.ps1 -tag '5.0.24'`

If you want to also have the images tagged as 'latest', pass `-SetLatestTag` to the script:

`.\build.ps1 -tag '5.0.24' -setLatestTag`

Unless otherwise specified, InRule assets (e.g., irCatalog, irServer components) will be copied into their respective directories by the build script. The default installation location of `C:\Program Files (x86)\InRule\irServer` is used for this, but can be overridden by providing a value for the `defaultInRuleInstallFolder` parameter:

`.\build.ps1 -tag '5.0.26 -defaultInRuleInstallFolder 'z:\inrule\'`

<!-- For instructions on building a set of images using Compose, see the section below on **Using Docker Compose to provision a rule execution environment** -->
Please see the instructions for each respective image for information on how to build the individual images.

## Installation and configuration

All of the images in this repository are Windows-based. Depending on whether you are using Windows 10 or Windows Server,
you can find instructions on how to get started with Containers [here](https://msdn.microsoft.com/en-us/virtualization/windowscontainers/quick_start/quick_start).
Once you've followed all of the steps in a respective setup guide and verified things are working properly, you'll be ready to use the InRule images!

> Note: make sure to switch your running Dockerd from Linux to Windows containers!
<!-- 
## Using Docker Compose to provision a rule execution environment

### About the compose file

The `docker-compose.yml` file in the root of this repository describes the resources, services, that comprise a "vanilla" rule execution environment.
It contains the following components (container alias are in parenthesis):

* Rules execution service (rex)
* Catalog service (cat)
* SQL Express Db (db)

### Setting up the container environment

There are a lot of variables involved in configuring a rule exuection environment. Wherever possible, sensible defaults have been applied to reduce the number and locations where these variables are needed.
The file `docker-compose.yml` contains information about the containers to run and also environmental information that is specific to a given instance of a container. To set environment
values, create a new file in the same directory as `docker-compose.yml` named `.env`. Each line of this file should contain a single variable name in the format `NAME=VALUE`. For more information on this and other environment variables, see [the Docker Compose docs](https://docs.docker.com/compose/compose-file/#/envfile).

Names and values are case-sensitive.

### Sample `.env` file

```data
TAG=4.6.33
sa_password=abcd1234
CatalogUser=sa
CatalogPassword=abcd1234
CatalogDbName="InRuleCatalog_v4_6_33"
irCatalogDir=C:\\Program Files (x86)\\InRule\\irServer\\RepositoryService\\IisService\\
irRuntimeDir=C:\\Program Files (x86)\\InRule\\irServer\\RuleEngineService\\IisService\\
attach_dbs="[{'dbName':'InRuleCatalog_v4_6_33', 'dbFiles':['C:\\data\\InRuleCatalog_v4_6_33.mdf', 'C:\\data\\InRuleCatalog_v4_6_33_log.ldf' ]}]"
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
  * Example: ```attach_dbs="[{'dbName':'InRuleCatalog', 'dbFiles':['C:\\data\\InRuleCatalog.mdf', 'C:\\data\\InRuleCatalog_log.ldf' ]}]"```

### Usage

#### Start the environment

```cmd
docker-compose up
```

#### Stopping the environment

```cmd
docker-compose down
```

-->

### Accessing the rule execution service from outside the container environment

At the moment, host-to-container does not support name resolution in Docker for Windows, so it is necessary to interact with containers via IP address.
Below is an example of how to obtain the IP address of a runtime execution service in a container named `docker_rex_1`. Looking at the `IPAddress` entry, we see that our service
has been assigned an IP address of `172.26.212.72`. I can verify that my service is up and running by navigating to `http://172.26.212.72/Service.svc`

```powershell
PS:\> docker inspect docker_rex_1
[
    {
        "Id": "fc0777f47b0aed6d7b57889a49d2a717e991c9e78c09665f177c55904843095c",
        "Created": "2016-12-16T16:39:58.7513763Z",
        "Path": "cmd",
        "Args": [
            "/S",
            "/C",
            "powershell .\\Set-RuntimeConfig.ps1 -catalogServiceUri $env:CatalogUri -Verbose;"
        ],
        ...(snip)...
        },
        "NetworkSettings": {
            "Bridge": "",
            "SandboxID": "a1d025c10af3bd9f60986dd3499278cfb20ca12e2cb8e93d2a362b08c0e7491a",
            "HairpinMode": false,
            "LinkLocalIPv6Address": "",
            "LinkLocalIPv6PrefixLen": 0,
            "Ports": {
                "443/tcp": null,
                "80/tcp": [
                    {
                        "HostIp": "0.0.0.0",
                        "HostPort": "60987"
                    }
                ]
            },
            "Networks": {
                "nat": {
                    "IPAMConfig": null,
                    "Links": [
                        "docker_cat_1:cat",
                        "docker_cat_1:cat_1",
                        "docker_cat_1:docker_cat_1"
                    ],
                    "Aliases": [
                        "fc0777f47b0a",
                        "rex"
                    ],
                    "NetworkID": "f2bfa95a59dbb4b90c9e1b8fd7e5367adc6610c3c08cdd7036a351338ce451b8",
                    "EndpointID": "b652b50fb8d1e38a72b9c2d64d4d401c1b7f5fcffc5c570b3fa4637466799c05",
                    "Gateway": "172.26.208.1",
                    "IPAddress": "172.26.212.72",
                    "IPPrefixLen": 16,
                    "IPv6Gateway": "",
                    "GlobalIPv6Address": "",
                    "GlobalIPv6PrefixLen": 0,
                    "MacAddress": "00:15:5d:c6:83:f5"
                }
            }
        }
    }
]
```