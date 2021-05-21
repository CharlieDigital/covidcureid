<template>
    <q-dialog
        position="right"
        full-height
        :maximized="$q.screen.lt.md"
        persistent
        :value="regimenDialogOpen">
        <q-card
            class="column full-height full-width no-wrap"
            :style="$q.screen.lt.md ? `min-width: ${$q.screen.width}px` : 'min-width: 800px'">
            <q-toolbar class="bg-teal text-white">
                <q-toolbar-title>Regimen Listing</q-toolbar-title>

                <q-btn flat round icon="mdi-close" @click="$router.back()"></q-btn>
            </q-toolbar>

            <q-card-section class="col col-shrink q-px-none q-py-xs q-ml-md">
                <h5 class="q-my-xs">{{ drug.drugName }}</h5>
                <div class="caption">{{ `Age ${age}, ${gender}; ${totalCount} records.` }} <strong>{{ `${ Math.ceil(totalImproved / totalCount * 100) }% improved` }}</strong></div>
                <q-card-actions align="center">
                    <q-btn-toggle
                        size='md'
                        dense
                        flat
                        v-model="filter"
                        toggle-color="teal"
                        :options="[
                            { label: 'All', value: 'All' },
                            { label: 'Improved', value: 'Improved' },
                            { label: 'Deteriorated', value: 'Deteriorated' },
                        ]">
                    </q-btn-toggle>
                </q-card-actions>
            </q-card-section>

            <q-separator/>

            <q-card-section class="col column q-px-none">
                <q-scroll-area style='height: 100%;' :thumb-style="{width: '5px', borderRadius: '2px', right: '1px'}" ref="chat">
                    <div
                        v-for="regimen in regimens"
                        :key="regimen.regimenId">
                        <transition
                            appear
                            enter-active-class="animated slideInRight"
                            leave-active-class="animated slideOutLeft">
                            <q-card
                                v-show="filter === 'All' || regimen.outcomeComputed === filter"
                                class="q-ma-sm"
                                :class="`case-${regimen.outcomeComputed.toLowerCase()}`"
                                bordered flat>
                                <q-card-section class="q-pa-sm">
                                    <q-card-section class="q-pa-none">
                                        <div class="text-h6">{{ regimen.regimenName.replace(/\+/g, ', ') }}</div>
                                    </q-card-section>
                                    <q-slide-transition>
                                        <q-card-section class="q-pa-md" v-show="openNote === regimen.regimenId">
                                            <div class="text-subtitle2">Unusual</div>
                                            <div class="text-body2">{{ regimen.unusual }}</div>

                                            <div class="text-subtitle2 q-mt-sm">Adverse Events</div>
                                            <div class="text-body2">{{ regimen.adverseEvents}}</div>

                                            <div class="text-subtitle2 q-mt-sm">Additional Notes</div>
                                            <div class="text-body2">{{ regimen.additionalInfo }}</div>
                                        </q-card-section>
                                    </q-slide-transition>
                                    <q-card-actions class="q-pa-none">
                                        <div class="text-subtitle1"><strong>{{ regimen.outcomeComputed}}</strong> - {{ regimen.countryTreated === '' ? 'N/A' : regimen.countryTreated }}</div>
                                        <q-space/>
                                        <q-btn
                                            icon="mdi-notebook-outline"
                                            fab-mini
                                            flat
                                            color="grey"
                                            size="xs"
                                            @click="openNote = regimen.regimenId"/>
                                        <q-btn
                                            icon="mdi-open-in-new"
                                            fab-mini
                                            flat
                                            color="grey"
                                            size="xs"
                                            @click="openURL(`https://cure.ncats.io/explore/cases/630/case-details/${regimen.regimenId}`)"/>
                                    </q-card-actions>
                                </q-card-section>
                            </q-card>
                        </transition>
                    </div>
                </q-scroll-area>
            </q-card-section>

            <q-separator/>

            <q-card-actions class="bg-white">
                <q-space/>
                <q-btn flat icon="mdi-check-circle-outline" label="Done" color="teal" @click="$router.back()" class="full-width"/>
            </q-card-actions>
        </q-card>
    </q-dialog>
</template>

<script lang="ts">
import axios from 'axios'
import { openURL } from 'quasar'
import { defineComponent, computed, watch, ref } from '@vue/composition-api'
import { router } from 'src/router'
import { rootStore } from 'src/store'
import { Regimen } from './model'

export default defineComponent({
    name: 'RegimenDialog',

    setup() {
        const regimenDialogOpen = computed(() => rootStore.state.app.regimenDialogOpen)
        const drug = computed(() => rootStore.state.app.drug || { drugId: 0, drugName: ''})
        const openNote = ref(0)
        const regimens = ref<Array<Regimen>>([])
        const age = ref('0')
        const gender = ref('male')
        const totalCount = ref(0)
        const totalImproved = ref(0)
        const filter = ref('All')

        watch(regimenDialogOpen, () => {
            filter.value = 'All'
        })

        watch(drug, async (newValue) => {
            // Drug changed; retrieve the value via API call.
            age.value = router.currentRoute.params.age
            gender.value = router.currentRoute.params.gender

            // Retrieve the data from the API endpoint.  The configuration is in the quasar.conf.js file
            // In Github, this should be added as a secret and will be injected at build.
            const response = await axios.get<Array<Regimen>>(`${process.env.API_ENDPOINT}/api/query/regimen/by/${age.value}/${gender.value}/${newValue.drugId}`)

            regimens.value = response.data

            totalCount.value = response.data.length

            let improved = 0;

            response.data.forEach(r => { if(r.outcomeComputed === 'Improved') improved++ })

            totalImproved.value = improved;
        })

        return {
            regimenDialogOpen,
            drug,
            age,
            gender,
            totalCount,
            totalImproved,
            openNote,
            regimens,
            openURL,
            filter
        }
    }
})
</script>

<style type="text/css">
    .case-improved { border-left: 4px solid #0db818 }
    .case-improved .text-h6 { color: #0db818 }

    .case-deteriorated { border-left: 4px solid #b80d0d }
    .case-deteriorated .text-h6 { color: #b80d0d }

    .case-undetermined { border-left: 4px solid #aaa }
</style>