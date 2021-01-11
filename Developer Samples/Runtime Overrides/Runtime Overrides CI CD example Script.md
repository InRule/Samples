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
    curl -X GET "<-- your GET URL -->" -H "accept: application/json" -H "Authorization: APIKEY <--your API key-->"
  displayName: 'get a list of current overrides'

- script: |
    curl -X POST "<-- your POST URL -->" -H  "accept: application/json" -H  "Authorization: APIKEY <--your API key-->" -H  "Content-Type: application/json" -d "{\"type\":\"DatabaseConnection\",\"property\":\"ConnectionString\",\"name\":\"testName\",\"value\":\"testValue\"}"
  displayName: 'set the testName connection string to testValue'

- script: |
    curl -X GET "<-- your GET URL -->" -H  "accept: application/json" -H  "Authorization: APIKEY <--your API key-->"
  displayName: 'get a list of overrides after setting'

- script: |
    curl -X DELETE "<-- your DELETE URL -->" -H  "accept: application/json" -H  "Authorization: APIKEY <--your API key-->" -H  "Content-Type: application/json" -d "{\"type\":\"DatabaseConnection\",\"property\":\"ConnectionString\",\"name\":\"testName\"}"
  displayName: 'delete the override we just created'

- script: |
    curl -X GET "<-- your GET URL -->" -H  "accept: application/json" -H  "Authorization: APIKEY <--your API key-->"
  displayName: 'get a list of overrides after deleting'
