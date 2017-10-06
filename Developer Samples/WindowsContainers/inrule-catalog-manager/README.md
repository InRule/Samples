# README for Web Catalog Manager DOCKERFILE

## Important notes on building the image

* A valid InRule license file is required prior to building the image
* The default location for these assets in a typical installation is 'C:\Program Files (x86)\InRule\irServer\CatalogManagerWeb'

## About this image

* The Web Catalog Manager provides an web-based admin UI for the irCatalog service.
* Upon startup, the container will configure the service to point at the catalog service URI specified at run-time.

## Building the image

### Docker build

The DOCKERFILE defines a build-time argument, `catManDir` which defaults to the [CatalogManagerWeb](CatalogManagerWeb/) child folder.
Typically, there's no need to modify this value in your builds.

#### Using defaults

```cmd

docker build -t server/inrule-catalog-manager:5.0.14 .

```

#### Specifying an alternative path for source artifacts

```cmd

docker build --build-arg catManDir=c:\users\jsmith\downloads\irServer -t server/inrule-catalog-manager:5.0.12 .

```

## Running the image

Once started, you can open a web browser and navigate to the IP address of the container to access the Web Catalog Manager. Valid credentials for the irCatalog instance being accessed are required.

### Using a linked catalog (default)

```cmd

docker run server/inrule-catalog-manager:latest

```

This will start a container running the Web Catalog Manager site using the default value for the Catalog URI (defaults to **cat**).

### Specifying an irCatalog URL

```cmd

docker run -d --env CatalogUri=https://acme-catalog.cloudapp.net/Service.svc server/inrule-catalog-manager:latest

```