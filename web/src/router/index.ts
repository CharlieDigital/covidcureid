import { route } from 'quasar/wrappers';
import VueRouter from 'vue-router';
import { Store } from 'vuex';
import { rootStore, StateInterface } from '../store';
import routes from './routes';

/*
 * If not building with SSR mode, you can
 * directly export the Router instantiation
 */

let router: VueRouter

export default route<Store<StateInterface>>(function ({ Vue }) {
    Vue.use(VueRouter);

    const Router = new VueRouter({
        scrollBehavior: () => ({ x: 0, y: 0 }),
        routes,

        // Leave these as is and change from quasar.conf.js instead!
        // quasar.conf.js -> build -> vueRouterMode
        // quasar.conf.js -> build -> publicPath
        mode: process.env.VUE_ROUTER_MODE,
        base: process.env.VUE_ROUTER_BASE
    });

    Router.beforeEach(async (to, from, next) => {
        if (to.path.indexOf('regimen') > 0 || from.path.indexOf('regimen') > 0) {
            await rootStore.dispatch('app/toggleRegimenDialog')
        }

        next()
    })

    router = Router

    return Router
})

export { router }
