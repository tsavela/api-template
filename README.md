# Introduction 
Template for "dotnet new" for creating a .NET Core API. Features:

* Following ports & adapters / hexagonal architectural pattern.
* ApplicationInsights and Elastic (through fluentd) support.
* AppMetrics to write application metrics to ApplicationInsights.
* API authentication using Azure AD tokens.


# Installation
```
dotnet new --install <path to the template folder>
```

# Usage
```
mkdir <project folder>
cd <project folder>
dotnet new hexagonal-api --servicePort <TCP port to listen on> --serviceName <friendly service name> --kubernetesServiceName <k8s service name>
```