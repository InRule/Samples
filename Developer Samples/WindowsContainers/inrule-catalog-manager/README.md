# InRule Samples

## README for Web Catalog Manager DOCKERFILE

## Important notes on building the image

* A valid InRule license file is required prior to building the image
* The default location for these assets in a typical installation is 'C:\Program Files (x86)\InRule\irServer\CatalogManagerWeb'

## About this image

* The Web Catalog Manager provides an web-based admin UI for the irCatalog service.
* Upon startup, the container will configure the service to point at the catalog service URI specified at run-time.

## Running the image

Once started, you can open a web browser and navigate to the IP address of the container to access the Web Catalog Manager. Valid credentials for the irCatalog instance being accessed are required.

### Using a linked catalog (default)

```cmd

docker run inrule/inrule-catalog-manager:latest

```

This will start a container running the Web Catalog Manager site using the default value for the Catalog URI (defaults to **cat**).

### Specifying an irCatalog URL

```cmd

docker run -d --env CatalogUri=https://acme-catalog.cloudapp.net/Service.svc inrule/inrule-catalog-manager:latest

```

### Access Catalog Manager with HTTPS

```cmd
docker run -d -p 443 -v '<HOST_PFX_FOLDER>:C:\inrule-catalog-manager\pfx:ro' --env PfxPassword=<PFX_PASSWORD> inrule/inrule-catalog-manager:latest
```
