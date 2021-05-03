# covidcureid

## Overview

The idea for this application is to extract the information from the CURE ID application which is managed by the FDA.

The CURE ID application contains case report forms (CRFs) for treatments for COVID (and many other diseases) using so-called off-label usage of the drugs.

These are usages for which there may not yet be FDA approval but in some circumstances, the treatments may be the best option available.

## Organization

The `server` directory contains the files for the server side of the application.  This is built using C# and Azure Functions.

The `web` directory contains the front-end UI side of the application.  This is built using Vue and Quasar.

## Getting Started

If you do not already have an Azure account, you will need to create one and grab the CLI.  Many of the operations will be performed using the CLI.

We will need the following resources:

1. CosmosDB - This is where we will store our data
2. Storage - This is where we will push the raw files AND where we will keep the static application files from `web`
3. Functions - When the files are pushed into Storage, this will trigger a Function to process the file and move the data into CosmosDB

## Downloading/Refreshing Data Files

Use the following command to grab the latest set of data files

```
cd server/data
npm run fetch
```

This executes the script `fetch-data.js` which will execute the REST API calls to retrieve the raw underlying data files.

