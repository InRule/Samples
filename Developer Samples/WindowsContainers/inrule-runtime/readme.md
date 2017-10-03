# README for the inrule-runtime DOCKER image

## Important notes on building the image

* You must have a valid license for irCatalog.
* Prior to building the image, copy the irServer RuleEngineService IisService assets (default is usually `C:\Program Files (x86)\InRule\irServer\RuleEngineService\IisService`) into this repository's `/irServer/` directory.
* See [inrule-server](/inrule-server/) documentation for information on licensing

## About this image

* To use file system-based Rule Applications, you can either copy ruleapps directly into the `C:\RuleApps\` folder or (recommended) map a volume to a folder on the host computer to serve as a shared source

* The default settings specify a host name of `cat` for the catalog service; override the `CatalogUri` environment variable to use a different catalog service or if your linked container is named differently.

## Building the image

### Docker build

#### Using defaults

```docker build -t inrule-runtime .```

#### Specifying an alternative path for source artifacts

```docker build --build-arg irRuntimeDir=c:\users\jsmith\downloads\irServer -t inrule-runtime .```

## Running the image

Once the container starts it will be listening on the designated ports (80 by default). Requests can be sent either to the SOAP service or the REST service using these endpoints:

SOAP: `http://<container name or ip>/Service.svc`
REST: `http://<container name or ip>/HttpService.svc`

### Basic usage connecting to external catalog

```cmd

docker run -d -p 80:80 --env CatalogUri='https://contoso-catalog.cloudapp.net/Service.svc' inrule-runtime:latest

```

### Basic usage connecting to a linked container catalog

```cmd

docker run -d --link cat --env CatalogUri='https://cat/Service.svc' inrule-runtime:latest

```

Using a volume mount for file-based ruleapps and a container link to a container running irCatalog:

```cmd

docker run -d --rm --name=rex -v c:\inrule-ruleapps\:c:\RuleApps\ -P --link=cat inrule-runtime:latest

```

### Obtaining the IP address of the container

In order to connect to the service, you'll need the IP address of the container. Get it using a command similar to the following:

    `docker inspect -f '{{range .NetworkSettings.Networks}}{{.IPAddress}}{{end}}' rex`