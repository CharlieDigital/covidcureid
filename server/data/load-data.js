/**
 * This script is used to load the data files into Azure Storage and CosmosDB
 * after they have been fetched.
 *
 * The Function should be running before executing as the trigger will not fire
 * otherwise.
 */

const cmd = require('node-cmd')
const path = require('path')

const rawDataFileDir = path.resolve(__dirname, 'raw-files')

console.log(`Loading data files from ${rawDataFileDir}`)

// Function to execute a command and log the output
function execute(command) {
    console.log(`EXECUTING COMMAND: ${command}\r\n\r\n`)

    let output = cmd.runSync(command);

    console.log(`
        Sync Err:${output.err}

        Sync stderr:  ${output.stderr}

        Sync Data: ${output.data}
    `);
}

// Delete the Azure Storage container to purge the files.
execute('az storage container delete --name covidcureid-raw-files --connection-string "UseDevelopmentStorage=true"')

// Delete the CosmosDB database
execute('az cosmosdb collection delete --db-name CovidCureId --collection-name CaseFiles --yes --key "C2y6yDjf5/R+ob0N8A7Cgv30VRDJIWEHLM+4QDU5DE2nQ9nDuVTqobD4b8mGGyPMbIZnqyMsEcaGQy67XIw/Jw==" --url-connection "https://localhost:8081"')
execute('az cosmosdb database delete --db-name CovidCureId --yes --key "C2y6yDjf5/R+ob0N8A7Cgv30VRDJIWEHLM+4QDU5DE2nQ9nDuVTqobD4b8mGGyPMbIZnqyMsEcaGQy67XIw/Jw==" --url-connection "https://localhost:8081"')

// Delete the Azure Storage queues
execute('az storage queue delete --name covidcureid-queue-drug --connection-string "UseDevelopmentStorage=true"')
execute('az storage queue delete --name covidcureid-queue-regimen --connection-string "UseDevelopmentStorage=true"')

// Create the Azure Storage container
execute('az storage container create --name covidcureid-raw-files --connection-string "UseDevelopmentStorage=true"')

// Create the Azure Storage queues
execute('az storage queue create --name covidcureid-queue-drug --connection-string "UseDevelopmentStorage=true"')
execute('az storage queue create --name covidcureid-queue-regimen --connection-string "UseDevelopmentStorage=true"')

// Create the CosmosDB database
execute('az cosmosdb database create --db-name CovidCureId --key "C2y6yDjf5/R+ob0N8A7Cgv30VRDJIWEHLM+4QDU5DE2nQ9nDuVTqobD4b8mGGyPMbIZnqyMsEcaGQy67XIw/Jw==" --url-connection "https://localhost:8081"')

// Create the CosmosDB collection
execute('az cosmosdb collection create --db-name CovidCureId --collection-name CaseFiles --key "C2y6yDjf5/R+ob0N8A7Cgv30VRDJIWEHLM+4QDU5DE2nQ9nDuVTqobD4b8mGGyPMbIZnqyMsEcaGQy67XIw/Jw==" --url-connection "https://localhost:8081" --partition-key-path /PartitionKey')

// Move the files into Azure Storage
execute(`az storage blob upload-batch --destination covidcureid-raw-files --source "${rawDataFileDir}" --pattern "02-*.json" --connection-string "UseDevelopmentStorage=true"`)