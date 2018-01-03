# InRule Samples

## Sample Title

# README for the inrule-server DOCKER image

## Notes regarding licensing

You must have a valid license for InRule to use these images. By using these images you agree to the InRule EULA.
Contact support@inrule.com to check for and obtain the appropriate license file.

## Environment

Windows Server Core Base OS image

## About this image

This image serves as a base image for other inrule server-based components. It ensures that the necessary pre-requisites have been fulfilled (e.g., IIS is installed, WCF is configured, etc) and that InRule event log sources are created.

## Usage

### Docker build

```docker build -t inrule-server .```



### Docker run

This image should not need to be run



## Further reading

* [About Windows Containers](https://msdn.microsoft.com/en-us/virtualization/windowscontainers/about/index)
* [Containers with Windows 10](https://docs.microsoft.com/en-us/virtualization/windowscontainers/quick-start/quick-start-windows-10)
* [Container Samples](https://msdn.microsoft.com/en-us/virtualization/windowscontainers/samples/)