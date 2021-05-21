import { Drug } from 'src/components/model';
import { ActionTree } from 'vuex';
import { StateInterface } from '../index';
import { AppStateInterface } from './state';

const actions: ActionTree<AppStateInterface, StateInterface> = {
    toggleRegimenDialog(context) {
        context.commit('TOGGLE_REGIMEN_DIALOG')
    },

    setDrug(context, drug: Drug) {
        context.commit('SET_DRUG', drug)
    }
};

export default actions;
