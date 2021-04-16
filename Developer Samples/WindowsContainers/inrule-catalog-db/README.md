# InRule Catalog Database Image

This image provides a blank irCatalog database, initialized with default credentials and ready to be hooked up to a cat service.

Usage:

`docker run --name irdb --env sa_password=<a strong password> -p 1433:1433 inrule/inrule-catalog-db:latest`
