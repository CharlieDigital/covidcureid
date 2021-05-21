<template>
    <q-page class="items-center justify-evenly q-px-md q-mb">
        <!--// A Short Intro //-->
        <p class="text-subtitle1 q-my-md text-center">
            An open source application which extracts data from the COVID specific case data from the
            <a href="https://cure.ncats.io/explore" target="_blank">FDA CURE ID</a> web application
            and surfaces the data to allow searching by a patient"s age and gender.  This application
            is neither endorsed by nor associated with the FDA.
        </p>

        <!--// Main Search Inputs //-->
        <h4 class="q-my-md">Find Cases</h4>

        <q-slide-transition>
            <q-banner
                class="bg-amber-2 text-orange-8"
                rounded
                v-show="!warningDismissed"                >
                <template v-slot:action>
                    <q-btn
                        flat
                        label="I understand"
                        size="lg"
                        icon="mdi-check"
                        @click="warningDismissed = true"/>
                </template>
                <template v-slot:avatar>
                    <q-icon name="mdi-information" color="orange-8" />
                </template>
                <p class="text-subtitle1">
                    <strong>Disclaimer</strong>: this website is not meant to be used in place of professional medical evaluation and treatment.  The contents
                    of this website are purely informational and the information has not been verified by the FDA.  Consult your physican if you believe
                    you are showing symptoms of COVID.
                </p>
                <p class="text-subtitle1">covidcureid.com is a third party application and is not associated with the FDA or the CURE ID application; it is not endorsed by the FDA.</p>
                <p class="text-subtitle1">
                    Click the <strong>I UNDERSTAND</strong> button to continue.
                </p>
            </q-banner>
        </q-slide-transition>

        <!--// Container for the form and chart //-->
        <q-slide-transition>
            <div v-show="warningDismissed">
                <q-form ref="criteria" class="row q-pa-md q-col-gutter-md">
                    <q-input
                        class="col-md-6 col-12"
                        type="text"
                        label="Age (0-100)"
                        v-model="age"
                        :rules="[
                            val => val !== null && val !== '' || 'Please enter an age',
                            val => val >= 0 && val <=100 || 'Please enter a value 0-100'
                        ]"
                        no-error-icon>

                    </q-input>

                    <q-select
                        class="col-md-6 col-12"
                        label="Gender"
                        :options="genderOptions"
                        v-model="gender"
                        dropdown-icon="mdi-menu-down"
                        :rules="[
                            val => val !== null || 'Please select a gender'
                        ]"
                        no-error-icon>

                    </q-select>
                </q-form>

                <div class="row">
                    <q-btn
                        class="col-md-4 offset-md-4 col-12"
                        icon="mdi-magnify"
                        size="lg"
                        color="teal-6"
                        rounded
                        @click="retrieveCases"
                        :disable="gender === '' || age === ''">
                        Find Cases
                    </q-btn>
                </div>

                <div ref="chartContainer">
                    <apexchart
                        height="100%"
                        type="bar"
                        :options="chartOptions"
                        :series="chartSeries"
                        v-if="showChart">
                    </apexchart>
                </div>
            </div>
        </q-slide-transition>

        <!--// About Cards //-->
        <h4 class="q-my-md">About</h4>

        <div class="row">
            <q-card flat class="col-md-4 col-sm-6 col-xs-12">
                <q-card-section>
                    <div class="text-h6">CURE ID</div>
                </q-card-section>

                <q-separator inset />

                <q-card-section>
                    <p>The <a href="https://cure.ncats.io/explore" target="_blank">CURE ID</a> app from the FDA is linked to a database that contains <a href="https://www.fda.gov/drugs/science-and-research-drugs/cure-id-app-lets-clinicians-report-novel-uses-existing-drugs" target="_blank">"novel uses of existing drugs" in the treatment of diseases</a>.</p>
                    <p>It is a very interesting database that provides real-world, first-hand case reports with regards to emerging approaches to treating a variety of diseases.</p>
                    <p>Unfortunately, the information is difficult to search and use in any practical way.</p>
                    <p>This application extracts the information from the CURE ID application using REST APIs and feeds the resulting data files into an Azure CosmosDB.</p>
                    <p>The CURE ID database includes a number of other facets which can be incorporated for search including co-morbidities (see the Github repo README for ideas).</p>
                </q-card-section>
            </q-card>

            <q-card flat class="col-md-4 col-sm-6 col-xs-12">
                <q-card-section>
                    <div class="text-h6">Tech Stack</div>
                </q-card-section>

                <q-separator inset />

                <q-card-section>
                    <p>This project uses the following technologies</p>
                    <q-chip icon="mdi-open-in-new" clickable @click="openURL('https://azure.microsoft.com/en-us/services/app-service/static/')">Azure Static Web Apps</q-chip>
                    <q-chip icon="mdi-open-in-new" clickable @click="openURL('https://azure.microsoft.com/en-us/services/cosmos-db/')">Azure CosmosDB</q-chip>
                    <q-chip icon="mdi-open-in-new" clickable @click="openURL('https://azure.microsoft.com/en-us/services/functions/')">Azure Functions</q-chip>
                    <q-chip icon="mdi-open-in-new" clickable @click="openURL('https://vuejs.org/')">Vue.js</q-chip>
                    <q-chip icon="mdi-open-in-new" clickable @click="openURL('https://quasar.dev/')">Quasar</q-chip>
                    <q-chip icon="mdi-open-in-new" clickable @click="openURL('https://apexcharts.com/')">Apex Charts</q-chip>
                    <q-chip>C#</q-chip>
                    <q-chip>TypeScript</q-chip>
                    <q-chip>JavaScript</q-chip>
                    <p class="q-my-md">If you are interested in contributing or forking the code and building your own version, check out the Github repo for more information.</p>
                </q-card-section>

                <q-card-actions>
                    <q-btn rounded icon="mdi-github" class="full-width" color="teal-6" @click="openURL('https://github.com/CharlieDigital/covidcureid')">Veiw on Github</q-btn>
                </q-card-actions>
            </q-card>

            <q-card flat class="col-md-4 col-sm-6 col-xs-12">
                <q-card-section>
                    <div class="text-h6">This Project</div>
                </q-card-section>

                <q-separator inset />

                <q-card-section>
                    <p>This open source project was inspired by the outbreak of COVID in India in April of 2021.</p>
                    <p>While there are many challenges in India at the moment, the goal of this project is to surface the information in the CURE ID database so that physicians can see real-world data on which treatments had positive outcomes and which did not given the available treatments on hand by cross-cutting the CURE ID database.</p>
                    <p>One shortcoming is that the CURE ID database is not heavily trafficked and contains only ~1000 case reports for COVID.</p>
                    <p>This application is <strong>read only</strong>; in other words, it has no ability to submit new cases to CURE ID.  If you are a physician, you must create an account via CURE ID to submit new cases.</p>
                </q-card-section>
            </q-card>

            <q-card flat class="col-12">
                <q-separator inset />

                <q-card-section style="text-align: center" class="text-h2">
                    <q-img
                        src="/logos/logo-azure.png"
                        style="height: 64px; max-width: 64px;"></q-img>
                        +
                    <q-img
                        src="/logos/logo-vue.png"
                        style="height: 64px; max-width: 64px;"></q-img>
                        +
                    <q-img
                        src="/logos/logo-quasar.png"
                        style="height: 64px; max-width: 64px;"></q-img>
                        +
                    <q-img
                        src="/logos/logo-apex.png"
                        style="height: 64px; max-width: 64px;"></q-img>
                </q-card-section>

                <q-card-actions align="center">
                    <q-chip clickable @click="openURL('https://charliedigital.com/')">
                        <q-avatar>
                            <img src="/images/charles-headshot-color.png"/>
                        </q-avatar>
                        Charles Chen
                    </q-chip>

                    <q-chip icon="mdi-linkedin" clickable @click="openURL('https://www.linkedin.com/in/charlescchen/')">
                        LinkedIn
                    </q-chip>
                </q-card-actions>
            </q-card>
        </div>

        <regimen-dialog/>
    </q-page>
</template>

<script lang="ts">
import { defineComponent, ref } from '@vue/composition-api';
import { openURL, QForm } from 'quasar'
import axios from 'axios'
import { rootStore } from 'src/store'

// Custom components
import { Option, Drug, Record } from 'components/model'
import RegimenDialog from 'components/RegimenDialog.vue'

export default defineComponent({
    name: 'PageIndex',

    components: {
        RegimenDialog
    },

    setup(props, { root }) {
        const warningDismissed = ref(false)
        const genderOptions = ref([
            { label: 'Female', value: 'female' },
            { label: 'Male', value: 'male' }
        ])
        const gender = ref<Option>()
        const age = ref('')
        const criteria = ref<QForm>()
        const chartContainer = ref<HTMLDivElement>()

        let drugs = new Array<Drug>() // This will hold the drug IDs

        const chartOptions = ref({

        })

        const chartSeries = ref([{
            data: new Array<number>()
        }])

        const showChart = ref(false)

        async function retrieveCases() {
            // Makes an API call to retrieve the cases.
            const valid = await criteria.value?.validate()

            if (!valid) {
                return
            }

            console.log(process.env.API_ENDPOINT)

            const ageValue = age.value
            const genderValue = gender.value?.value

            // Retrieve the data from the API endpoint.  The configuration is in the quasar.conf.js file
            // In Github, this should be added as a secret and will be injected at build.
            const response = await axios.get<Array<Record>>(`${process.env.API_ENDPOINT}/api/query/drug/by/${ageValue}/${genderValue || 'male'}`)

            const data = response.data;

            if (chartContainer.value) {
                chartContainer.value.style.height = `${data.length * 20}px`
            }

            // Sort by the total number of cases.
            data.sort((r1, r2) => {
                const r1Sum = r1.improved + r1.deteriorated + r1.undetermined
                const r2Sum = r2.improved + r2.deteriorated + r2.undetermined

                if (r1Sum < r2Sum) return 1
                if (r1Sum > r2Sum) return -1
                return 0
            })

            const improved = {
                name: 'Improved',
                data: data.map(r => r.improved)
            }

            const deteriorated = {
                name: 'Deteriorated',
                data: data.map(r => r.deteriorated)
            }

            const undetermined = {
                name: 'Undetermined',
                data: data.map(r => r.undetermined)
            }

            drugs = data.map(r => { return { drugId: r.drugId, drugName: r.drugName} })

            chartOptions.value = {
                chart: {
                    type: 'bar',
                    stacked: true,
                    events: {
                        dataPointSelection: async function(event: any, chartContext: any, config: any) {
                            // config has config.dataPointIndex which is the drug.
                            await root.$router.push({
                                name: 'RegimenDialog' ,
                                params: {
                                    age: ageValue,
                                    gender: genderValue || 'male',
                                    drugId: drugs[config.dataPointIndex].drugId.toString()
                                }
                            })

                            await rootStore.dispatch('app/setDrug', drugs[config.dataPointIndex])
                        }
                    }
                },
                plotOptions: {
                    bar: {
                        horizontal: true,
                        hideOverflowingLabels: true,
                        barHeight: '80%'
                    }
                },
                colors: ['#0db818', '#dddddd', '#b80d0d'],
                dataLabels: {
                    enabled: false
                },
                xaxis: {
                    categories: data.map(r => r.drugName)
                }
            }

            chartSeries.value = [
                improved, undetermined, deteriorated
            ]

            showChart.value = true
        }

        return {
            criteria,
            openURL,
            genderOptions,
            gender,
            age,
            warningDismissed,
            retrieveCases,
            chartContainer,
            showChart,
            chartOptions,
            chartSeries
        }
    }
});
</script>
