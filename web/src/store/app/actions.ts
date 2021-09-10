import { ActionTree } from 'vuex';
import { StateInterface } from '../index';
import { AppStateInterface } from './state';
import { Drug } from 'src/components/model';

const actions: ActionTree<AppStateInterface, StateInterface> = {
    setDrug(context, drug: Drug) {
        context.commit('SET_DRUG', drug)
    }
};

export default actions;
