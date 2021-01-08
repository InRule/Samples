# Starter pipeline
# Start with a minimal pipeline that you can customize to build and deploy your code.
# Add steps that build, run tests, deploy, and more:
# https://aka.ms/yaml

trigger:
- master

pool:
  vmImage: 'ubuntu-latest'

steps:
- script: echo Hello, world!
  displayName: 'Run a one-line script'

- script: |
    echo Add other tasks to build, test, and deploy your project.
    echo See https://aka.ms/yaml
  displayName: 'Run a multi-line script'

- script: |
    curl -X GET "https://trial-9kdexu-757-portalsvc.staging.inrulecloud.com/configuration/api/v1/runtimeoverrides" -H "accept: application/json" -H "Authorization: APIKEY hv+pzfZ6xvXC2t/L1Kocgg"
  displayName: 'get a list of current overrides'

- script: |
    curl -X POST "https://trial-9kdexu-757-portalsvc.staging.inrulecloud.com/configuration/api/v1/runtimeoverrides" -H  "accept: application/json" -H  "Authorization: APIKEY hv+pzfZ6xvXC2t/L1Kocgg" -H  "Content-Type: application/json" -d "{\"type\":\"DatabaseConnection\",\"property\":\"ConnectionString\",\"name\":\"testName\",\"value\":\"testValue\"}"
  displayName: 'set the testName connection string to testValue'

- script: |
    curl -X GET "https://trial-9kdexu-757-portalsvc.staging.inrulecloud.com/configuration/api/v1/runtimeoverrides" -H  "accept: application/json" -H  "Authorization: APIKEY hv+pzfZ6xvXC2t/L1Kocgg"
  displayName: 'get a list of overrides after setting'

- script: |
    curl -X DELETE "https://trial-9kdexu-757-portalsvc.staging.inrulecloud.com/configuration/api/v1/runtimeoverrides" -H  "accept: application/json" -H  "Authorization: APIKEY hv+pzfZ6xvXC2t/L1Kocgg" -H  "Content-Type: application/json" -d "{\"type\":\"DatabaseConnection\",\"property\":\"ConnectionString\",\"name\":\"testName\"}"
  displayName: 'delete the override we just created'

- script: |
    curl -X GET "https://trial-9kdexu-757-portalsvc.staging.inrulecloud.com/configuration/api/v1/runtimeoverrides" -H  "accept: application/json" -H  "Authorization: APIKEY hv+pzfZ6xvXC2t/L1Kocgg"
  displayName: 'get a list of overrides after deleting'