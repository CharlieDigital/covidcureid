import { GetterTree } from 'vuex';
import { StateInterface } from '../index';
import { AppStateInterface } from './state';

const getters: GetterTree<AppStateInterface, StateInterface> = {
    someGetter(/* context */) {
        // your code
    }
};

export default getters;
