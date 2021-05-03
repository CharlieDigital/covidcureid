# covidcureid

## Overview

The idea for this application is to extract the information from the CURE ID application which is managed by the FDA.

The CURE ID application contains case report forms (CRFs) for treatments for COVID (and many other diseases) using so-called off-label usage of the drugs.

These are usages for which there may not yet be FDA approval but in some circumstances, the treatments may be the best option available.

## Organization

The `server` directory contains the files for the server side of the application.  This is built using C# and Azure Functions.

The `web` directory contains the front-end UI side of the application.  This is built using Vue and Quasar.

## Getting Started

If you do not already have an Azure account, you will need to create one and grab the [CLI](https://docs.microsoft.com/en-us/cli/azure/install-azure-cli).  Many of the operations will be performed using the CLI.

We will need the following resources:

1. CosmosDB - This is where we will store our data.  You will need to grab the [Azure CosmosDB Emulator](https://docs.microsoft.com/en-us/azure/cosmos-db/local-emulator) for local testing.
2. Storage - This is where we will push the raw files AND where we will keep the static application files from `web`. You will also need to grab the [Azure Storage Emulator](https://docs.microsoft.com/en-us/azure/storage/common/storage-use-emulator) for local testing.
3. Functions - When the files are pushed into Storage, this will trigger a Function to process the file and move the data into CosmosDB.  See the next step.

## Setup

The application requires [Azure Functions Core Tools](https://docs.microsoft.com/en-us/azure/azure-functions/functions-run-local?tabs=windows%2Ccsharp%2Cbash).

1. From the `server` directory in a command line, run `func init CovidCureIdApp` to initialize the Azure Functions application.
2. From the `CovidCureIdApp` directory, run `dotnet add package Microsoft.Azure.WebJobs.Extensions.Storage`
3. From the `CovidCureIdApp` directory, run `dotnet add package Microsoft.Azure.WebJobs.Extensions.CosmosDB`
4. Create a directory `Functions` under `CovidCureIdApp` and then run `func new` to create a new Function

To create the locally emulated blob storage, use the following:

```
az storage container create -n covidcureid-raw-files --connection-string "UseDevelopmentStorage=true"
```

This is where we will import our raw data files which will trigger the Function `ProcessDataFile` to parse the file.

## Downloading/Refreshing Data Files

Use the following command to grab the latest set of data files

```
cd server/data
npm run fetch
```

This executes the script `fetch-data.js` which will execute the REST API calls to retrieve the raw underlying data files.

## Development

To start the server project, switch into the directory `server/CovidCureIdApp` and run the command `func start --build`
