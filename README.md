# covidcureid

## Overview

The idea for this application is to extract the information from the CURE ID application which is managed by the FDA.

The [CURE ID](https://cure.ncats.io/explore) application contains case report forms (CRFs) for treatments for COVID (and many other diseases) using so-called off-label usage of the drugs.

These are usages for which there may not yet be FDA approval but in some circumstances, the treatments may be the best option available.

## Code Organization

The `server` directory contains the files for the server side of the application.  This is built using C# and Azure Functions.

The `web` directory contains the front-end UI side of the application.  This is built using Vue and Quasar.

## Getting Started

If you do not already have an Azure account, you will need to create one and grab the [CLI](https://docs.microsoft.com/en-us/cli/azure/install-azure-cli).  Many of the operations will be performed using the CLI.

We will need the following resources:

1. **CosmosDB** - This is where we will store our data.  You will need to grab the [Azure CosmosDB Emulator](https://docs.microsoft.com/en-us/azure/cosmos-db/local-emulator) for local testing.
2. **Storage** - This is where we will push the raw files AND where we will keep the static application files from `web`. You will also need to grab the [Azure Storage Emulator](https://docs.microsoft.com/en-us/azure/storage/common/storage-use-emulator) for local testing.
3. **Functions** - When the files are pushed into Storage, this will trigger a Function to process the file and move the data into CosmosDB.  See the next step.

## Setup

*These steps cover the initial project setup and are not required for development since they will have already been performed.  See **Development** below for the development flow*

The application requires [Azure Functions Core Tools](https://docs.microsoft.com/en-us/azure/azure-functions/functions-run-local?tabs=windows%2Ccsharp%2Cbash).

1. From the `server` directory in a command line, run `func init CovidCureIdApp` to initialize the Azure Functions application.
2. From the `CovidCureIdApp` directory, run `dotnet add package Microsoft.Azure.WebJobs.Extensions.Storage`
3. From the `CovidCureIdApp` directory, run `dotnet add package Microsoft.Azure.WebJobs.Extensions.Storage.Queues --prerelease`
4. From the `CovidCureIdApp` directory, run `dotnet add package Microsoft.Azure.WebJobs.Extensions.CosmosDB`
5. Create a directory `Functions` under `CovidCureIdApp` and then run `func new` to create new Functions

To create the locally emulated blob storage, use the following:

```
az storage container create -n covidcureid-raw-files --connection-string "UseDevelopmentStorage=true"
```

This is where we will import our raw data files which will trigger the Function `ProcessDataFile` to parse the file.

Next, we will initialize the local CosmosDB using the command:

```
az cosmosdb database create --db-name CovidCureId --key "C2y6yDjf5/R+ob0N8A7Cgv30VRDJIWEHLM+4QDU5DE2nQ9nDuVTqobD4b8mGGyPMbIZnqyMsEcaGQy67XIw/Jw==" --url-connection "https://localhost:8081"

az cosmosdb collection create --db-name CovidCureId --collection-name CaseFiles --key "C2y6yDjf5/R+ob0N8A7Cgv30VRDJIWEHLM+4QDU5DE2nQ9nDuVTqobD4b8mGGyPMbIZnqyMsEcaGQy67XIw/Jw==" --url-connection "https://localhost:8081" --partition-key-path /cureId
```

Then we provision the Azure Storage Queues which are used for queueing and throttling the write throughput to Cosmos on data import:

```
az storage queue create --name covidcureid-queue-regimen --connection-string "UseDevelopmentStorage=true"

az storage queue create --name covidcureid-queue-drug --connection-string "UseDevelopmentStorage=true"
```

## Downloading/Refreshing Data Files

Use the following command to grab the latest set of data files

```
cd server/data
npm run fetch
```

This executes the script `fetch-data.js` which will execute the REST API calls to retrieve the raw underlying data files.

Once the data files have been downloaded, they need to be uploaded into Azure Storage.  This will trigger a function to process each of the files and push data into Azure CosmosDB via a trigger.

The copy operation should be performed after starting the Function on the local emulator (or the package has been deployed):

```
az storage blob upload-batch --destination covidcureid-raw-files --source server/data/raw-files --pattern "02-*.json" --connection-string "UseDevelopmentStorage=true"
```

The script `load-data.js` will perform the following actions:

1. Delete the Azure Storage container `covidcureid-raw-files` which contains the raw JSON files from CURE ID
2. Delete the Azure CosmosDB database
3. Delete the Azure Storage queues
4. Create the Azure Storage container `covidcureid-raw-files`
5. Create the Azure Storage queue `covidcureid-queue-drug`
6. Create the Azure Storage queue `covidcureid-queue-regimen`
7. Create the Azure CosmosDB database `CovidCureId`
8. Create the Azure CosmosDB collection `CaseFiles`

It can be used to effectively reset the environment.

**NOTE:** The Function runtime should be started before invoking the script as this will trigger the execution of the Function handler to load the data from the JSON files into CosmosDB

## Development

Before starting the server runtime, you will need to start:

1. Azure Storage Emulator: `C:\Program Files (x86)\Microsoft SDKs\Azure\Storage Emulator>AzureStorageEmulator.exe start`
2. Azure CosmosDB Emulator: `C:\Program Files\Azure Cosmos DB Emulator\Microsoft.Azure.Cosmos.Emulator.exe`

To start the server project, switch into the directory `server/CovidCureIdApp` and run the command `func start --build` to build the server application and start a local runtime.

# Data Processing Logic

The data processing logic flow is described below.

Each case record can be repeated across multiple files.  For example, if a case references 3 drugs, it is repeated in the file of each of those drugs.

So when processing each file, we only need to tabulate a single record for a case, even if there are multiple drugs because the processing of the other files will create their own records for the case.  For example, if a patient identified by case 1234 receives both Paracematol and Nadroparin, each of those files will have a record for the case.  Each of those files is processed separately and creates an entry for the same individual so that we can see which drugs had a positive effect overall.

But we also want to store the combined outcomes.  For example, a patient that receives Paracematol + Nadroparin versus a patient that receives Paracematol + Nadroparin + Moxifloxacin versus a patient that only receives Paracematol.  However, because each case is repeated, we need to account for the potential parallel processing of these files to prevent duplicate entry.

Without a transactional mechanism on the database itself, we need to ensure that we only create one unique entry for each case.

To do so, Azure Service Bus Queues with session IDs can be used to queue the entries to ensure once-only entry.  The session ID will be of the case ID combined with the IDs of the drugs in ascending order.

*An alternate approach is to simply use a `DISTINCT` constraint on the query and allow the duplicates to be created as this simplifies the architecture at the expense of higher cost queries (for a small dataset like this, it is not an issue and the better design choice, but definitely the queue based mechanism is better for a larger dataset)*

We want to create two types of data entries for each case:

1. **Drug Entry** - the outcome of every case that references this drug whether it is used alone or combined with another drug
2. **Regimen Entry** - the outcome of a specific regimen of drugs associated with a case

## Index Outcome by Drug

The first step is to index the outcome of each drug.  Each case may reference multiple drugs, but we want to create a `DrugEntry` based on the specific drug whether used in combination or on its own

## Index Outcome by Regimen

The second step is then to index the outcome of a regimen of drugs.  Because the cases are duplicated across the files, we only want to enter this once per unique case.  If this were a large dataset, the design should use Azure Service Bus Queues to ensure once-only entry.  However, for a small dataset of a few hundred or thousand records, it is enough to use the `DISTINCT` constraint to process the data.

## Queued Write Operation

There are different ways to control the costs associated with the CosmosDB RUs on data import during a heavy read/write cycle.  One possibility is to scale up the RU/s momentarily to incur the charge for the import process and then scale it down to reduce the running costs.

The other alternative is to queue the write operations using either Azure Storage Queues or Azure Service Bus Queues.  For simplicity, we will use the [Azure Storage Queue Concurrency](https://docs.microsoft.com/en-us/azure/azure-functions/functions-bindings-storage-queue-trigger?tabs=csharp#concurrency) controls to effectively throttle throughput to CosmosDB by queuing up the write operations.

For extremely large datasets, this may not work very well due to the limitations of the Storage Queue and the fact that if multiple Function runtimes start, then the write load and throughput will consequently increase beyond our control.  But it is good enough for this scale of data.

# References

* https://cure.ncats.io/explore
* https://docs.microsoft.com/en-us/azure/azure-functions/functions-run-local
* https://docs.microsoft.com/en-us/azure/azure-functions/functions-bindings-cosmosdb-v2-output
* https://docs.microsoft.com/en-us/dotnet/standard/serialization/system-text-json-how-to
* https://docs.microsoft.com/en-us/azure/azure-functions/functions-triggers-bindings