import VueApexCharts from 'vue3-apexcharts'
import { boot } from 'quasar/wrappers'

export default boot(({ app }) => {
    // eslint-disable-next-line @typescript-eslint/no-unsafe-member-access
    app.use(VueApexCharts)
    app.component('apexchart', VueApexCharts)
})
