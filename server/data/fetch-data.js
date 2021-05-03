/**
 * This file is used to retrieve the data files from CURE ID via REST API calls.
 */

 (async() => {
    const axios = require('axios')
    const fs = require('fs')

    console.log('Fetching data from CURE ID...')

    // Start by grabbing the root level listing from the CURE ID database
    const treatmentsResponse = await axios({
        url: 'https://cure-api2.ncats.io/v1/drugs?disease=630&no_page',
        method: 'GET'
    })

    const treatmentsResult = treatmentsResponse.data

    // This gives us the list of all treatments.
    fs.writeFile(
        'raw-files/01-covid-treatment-listing.json',
        JSON.stringify(treatmentsResult),
        function(err) {
            console.log(err)
        }
    )

    // And then for each treatment, we retrive the list of possible combinations.
    for(const treatment of treatmentsResult) {
        if (!(treatment.id === 11388 || treatment.id === 11364)) {
            continue
        }

        console.log(`  Fetching case data for: ${treatment.name} (${treatment.id})`)

        const treatmentResponse = await axios({
            url: `https://cure-api2.ncats.io/v1/reports?disease=630&drugs_id=${treatment.id}&outcome_computed=&no_page=`,
            method: 'GET'
        })

        const caseResults = treatmentResponse.data

        fs.writeFile(
            `raw-files/02-${treatment.id}-${treatment.name.replace('/', '--')}.json`,
            JSON.stringify(caseResults),
            function(err) {
                console.log(err)
            }
        )

        console.log(`    Saved ${caseResults.length} records`)
    }
})()