# covidcureid

## Overview

The idea for this application is to extract the information from the CURE ID application which is managed by the FDA.

The [CURE ID](https://cure.ncats.io/explore) application contains case report forms (CRFs) for treatments for COVID (and many other diseases) using so-called off-label usage of the drugs.

These are usages for which there may not yet be FDA approval but in some circumstances, the treatments may be the best or only option available.

## Code Organization

* The `server` directory contains the files for the server side of the application.  This is built using C#/.NET Core and Azure Functions.
  * `server/CovidCureIdApp/DataProcessor.cs` contains the Functions for ingest of the JSON files
  * `server/CovidCureIdApp/DataProvider.cs` contains the Functions that provide a REST API via `HttpTrigger`s
  * `server/CovidCureIdApp/DataAccess` contains the data access code (repository pattern described below).
  * `server/CovidCureIdApp/Model` contains the domain models.
* The `web` directory contains the front-end UI side of the application.  This is built using Vue and Quasar.
  * `web/src/pages/Index.vue` contains the Vue Single File Component (SFC) that has the main chart view.
  * `web/src/components/RegimenDialog.vue` contains the SFC that displays the side panel when a drug is clicked on.
  * `web/src/components/model.ts` contains the TypeScript models for the front-end.

## Getting Started

If you intend to deploy into Azure and you do not already have an Azure account, you will need to create one and grab the [CLI](https://docs.microsoft.com/en-us/cli/azure/install-azure-cli).  Many of the operations will be performed using the CLI.

We will need the following resources:

1. **CosmosDB** - This is where we will store our data.  You will need to grab the [Azure CosmosDB Emulator](https://docs.microsoft.com/en-us/azure/cosmos-db/local-emulator) for local testing.
2. **Storage** - This is where we will push the raw files AND where we will keep the static application files from `web`. You will also need to grab the [Azure Storage Emulator](https://docs.microsoft.com/en-us/azure/storage/common/storage-use-emulator) for local testing.  The emulator has been deprecated for [Azurite](https://docs.microsoft.com/en-us/azure/storage/common/storage-use-azurite)
3. **Functions** - When the files are pushed into Storage, this will trigger a Function to process the file and move the data into CosmosDB.  See the next step.

However, this codebase is designed to work entirely locally without the need for an Azure account.

The code was written with excellent and freely available **Visual Studio Code**.  For working with Vue, I recommend installing the [Vetur extension](https://marketplace.visualstudio.com/items?itemName=octref.vetur).

## Setup

*These steps cover the initial project setup and are not required for development since they will have already been performed.  The purpose is to provide the background in case you want to create it from scratch.  See **Development** below for the development flow*

The application requires [Azure Functions Core Tools](https://docs.microsoft.com/en-us/azure/azure-functions/functions-run-local?tabs=windows%2Ccsharp%2Cbash).

1. From the `server` directory in a command line, run `func init CovidCureIdApp` to initialize the Azure Functions application.
2. From the `CovidCureIdApp` directory, run `dotnet add package Microsoft.Azure.WebJobs.Extensions.Storage`
3. From the `CovidCureIdApp` directory, run `dotnet add package Microsoft.Azure.WebJobs.Extensions.Storage.Queues --prerelease`
4. From the `CovidCureIdApp` directory, run `dotnet add package Microsoft.Azure.WebJobs.Extensions.CosmosDB`
5. From the `CovidCureIdApp` directory, run `dotnet add package Microsoft.Azure.Cosmos`
6. From the `CovidCureIdApp` directory, run `dotnet add package Microsoft.Azure.Functions.Extensions`
7. Create a directory `Functions` under `CovidCureIdApp` and then run `func new` to create new Functions

To create the locally emulated blob storage, use the following:

```
az storage container create -n covidcureid-raw-files --connection-string "UseDevelopmentStorage=true"
```

This is where we will import our raw data files which will trigger the Function `ProcessDataFile` to parse the file.

Next, we will initialize the local CosmosDB using the command (the keys are the standard local keys; replace with your account key when pushing into Azure):

```
az cosmosdb database create --db-name CovidCureId --key "C2y6yDjf5/R+ob0N8A7Cgv30VRDJIWEHLM+4QDU5DE2nQ9nDuVTqobD4b8mGGyPMbIZnqyMsEcaGQy67XIw/Jw==" --url-connection "https://localhost:8081"

az cosmosdb collection create --db-name CovidCureId --collection-name CaseFiles --key "C2y6yDjf5/R+ob0N8A7Cgv30VRDJIWEHLM+4QDU5DE2nQ9nDuVTqobD4b8mGGyPMbIZnqyMsEcaGQy67XIw/Jw==" --url-connection "https://localhost:8081" --partition-key-path /PartitionKey
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

This executes the script `server/data/fetch-data.js` which will execute the REST API calls to retrieve the raw underlying data files.

Once the JSON data files have been downloaded, they need to be uploaded into Azure Storage.  This will trigger a function to process each of the files and push data into Azure CosmosDB via a trigger.

The copy operation should be performed after starting the Function on the local emulator (or the package has been deployed):

```
cd server/CovidCureIdApp
func start
```

Then push the files into the storage endpoint:

```
az storage blob upload-batch --destination covidcureid-raw-files --source server/data/raw-files --pattern "02-*.json" --connection-string "UseDevelopmentStorage=true"
```

The script `server/data/load-data.js` will perform the following actions:

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

(Or alternatively start it from your app launcher (Windows Start Menu))

To start the server project, switch into the directory `server/CovidCureIdApp` and run the command `func start --build` to build the server application and start a local runtime.

To start the front-end, switch into `web` and run the following command

```
npm install
npm run dev
```

The `dev` script is defined in `web/package.json` and executes:

```
cross-env API_ENDPOINT=http://localhost:7071 GA_TOKEN=blank quasar dev
```

This uses the `cross-env` package to allow the command to specify local environment variables for the API endpoint (localhost) and Google Analytics token (blank) to inject at build.  When building via GitHub actions, these are specified as secrets (see below).

## API Testing

To test the API in `DataProvider`, you can use `curl`, Postman, or [Huachao Mao's REST Client](https://marketplace.visualstudio.com/items?itemName=humao.rest-client).  The latter is recommended as it allows you to create a `.http` file and execute it directly within VS Code.

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

![Data Flow](assets/flow-data-processing/?raw=true "Data Processing Flow")

## Index Outcome by Drug

The first step is to index the outcome of each drug.  Each case may reference multiple drugs, but we want to create a `DrugEntry` based on the specific drug whether used in combination or on its own

## Index Outcome by Regimen

The second step is then to index the outcome of a regimen of drugs.  Because the cases are duplicated across the files, we only want to enter this once per unique case.  If this were a large dataset, the design should use Azure Service Bus Queues to ensure once-only entry.  However, for a small dataset of a few hundred or thousand records, it is enough to use the `DISTINCT` constraint to process the data.

## Queued Write Operation

There are different ways to control the costs associated with the CosmosDB RUs on data import during a heavy read/write cycle.  One possibility is to scale up the RU/s momentarily to incur the charge for the import process and then scale it down to reduce the running costs.

The other alternative is to queue the write operations using either Azure Storage Queues or Azure Service Bus Queues.  For simplicity, we will use the [Azure Storage Queue Concurrency](https://docs.microsoft.com/en-us/azure/azure-functions/functions-bindings-storage-queue-trigger?tabs=csharp#concurrency) controls to effectively throttle throughput to CosmosDB by queuing up the write operations.

For extremely large datasets, this may not work very well due to the limitations of the Storage Queue and the fact that if multiple Function runtimes start, then the write load and throughput will consequently increase beyond our control.  But it is good enough for this scale of data.

# Deployment

The section below outlines the deployment of the application to Azure.

## Serverless API Provisioning

TODO

## Serverless Static Web App Provisioning

TODO

## Github Secrets for Actions

The deployment actions are configured in the `.github/workflows` folder in two files which are both configured for manual deployment by default:

1. `build-deploy-api.yml` - this action deploys the Functions backend API
2. `build-deploy-web.yml` - this action deploys the Static Web App front end

Both require the following secrets to be configured:

|Token|Description|
|--|--|
|`API_ENDPOINT`|The URI of the Functions API which is injected into the Static Web App during build|
|`AZURE_CREDENTIALS`|The credentials configured for publishing to the Static Web App endpoint post build|
|`AZURE_FUNC_PUBLISH_PROFILE`|The XML formatted publishing profile which contains the credentials and configuration for publishing the Functions app.|
|`GA_TOKEN`|An optional Google Analytics token to inject to the Static Web App during build|

See this writeup for more info on the GitHub Actions deployment and configuration of secrets: https://charliedigital.com/2021/05/24/building-covidcureid-com/

# Code Patterns

## Functions Dependency Injection

The file `AppStarup.cs` uses [Functions Dependency Injection](https://docs.microsoft.com/en-us/azure/azure-functions/functions-dotnet-dependency-injection) to initialize singleton instances of the Cosmos client and data access components.  Note that the Functions are *non-static* as this allows us to take advantage of the injected repository classes.

## Repository Pattern

This project uses a simple [Repository Data Access Pattern](https://docs.microsoft.com/en-us/dotnet/architecture/microservices/microservice-ddd-cqrs-patterns/infrastructure-persistence-layer-design) to encapsulate access to the underlying CosmosDB instance and abstracts the interaction with the `CosmosClient`.  The original code was sourced from a Microsoft sample which has now been

Microsoft has many patterns available here: https://github.com/Azure/azure-cosmos-dotnet-v3/tree/master/Microsoft.Azure.Cosmos.Samples/Usage

And [this article by Joonas Westin that describes the various mechanisms of querying data in CosmosDB](https://joonasw.net/view/exploring-cosmos-db-sdk-v3).

There are many libraries available via nuget that can be used as well to speed up development:

|Nuget Package|GitHub Repo|
|--|--|
|[DataStore](https://www.nuget.org/packages/DataStore.Providers.CosmosDb/15.5.0-alpha)|https://github.com/anavarro9731/datastore|
|[CosmosDbRepository](https://www.nuget.org/packages/CosmosDbRepository/)|https://github.com/JohnLTaylor/CosmosDbRepository|

In general, the repository pattern works well with CosmosDB.

## Note on `JOIN` in CosmosDB

It is important to understand the purpose of the `JOIN` statement in CosmosDB as used here:

```sql
SELECT
    c.Id,
    c.RegimenName,
    c.CountryTreated,
    c.RegimenId,
    c.OutcomeComputed,
    c.Unusual,
    c.AdditionalInfo,
    c.AdverseEvents
FROM CaseFiles c
JOIN r IN c.RegimenDrugs
WHERE @age >= c.AgeLowerBound
    AND @age <= c.AgeUpperBound
    AND LOWER(c.Gender) = @gender
    AND r.CureId = @drugId
```

In CosmosDB, the `JOIN` *only operates across a single document*.  What happens in this cases, it that it is creating a product of two parts of *the same document* to "reshape" the result.  CosmosDB does **not** support `JOIN` operations *between* different documents.

In this example above, the `CaseFile` is being `JOIN`ed to the `CaseFile.RegimenDrugs` property to create one "row" for each drug.

See: https://docs.microsoft.com/en-us/azure/cosmos-db/sql/sql-query-join

# Areas for Improvement

There are many areas for additional development to consider:

* Search across additional parameters embedded in the data including co-morbidities and race
* Faceted search instead of database-driven search
* Pre-aggregating the information to improve performance

# References

* General
  * https://cure.ncats.io/explore
* `System.Text.Json`
  * https://docs.microsoft.com/en-us/dotnet/standard/serialization/system-text-json-how-to
* Azure Functions
  * https://docs.microsoft.com/en-us/azure/azure-functions/functions-run-local
  * https://docs.microsoft.com/en-us/azure/azure-functions/functions-bindings-cosmosdb-v2-output
  * https://docs.microsoft.com/en-us/azure/azure-functions/functions-triggers-bindings
  * https://github.com/Azure/azure-functions-host/wiki/host.json-(v2)
* Azure Static Web Sites
  * https://docs.microsoft.com/en-us/azure/static-web-apps/github-actions-workflow
  * https://docs.microsoft.com/en-us/azure/static-web-apps/application-settings
* Vue.js
  * https://vuejs.org/
* Quasar
  * https://quasar.dev/
