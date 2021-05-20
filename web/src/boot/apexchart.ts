import VueApexCharts from 'vue-apexcharts'
import { boot } from 'quasar/wrappers'

export default boot(({ Vue }) => {
    // eslint-disable-next-line @typescript-eslint/no-unsafe-member-access
    Vue.use(VueApexCharts)
    Vue.component('apexchart', VueApexCharts)
})
