import { Drug } from 'src/components/model';
import { MutationTree } from 'vuex';
import { AppStateInterface } from './state';

const mutation: MutationTree<AppStateInterface> = {
    SET_DRUG(state: AppStateInterface, drug: Drug) {
        state.drug = drug
    }
};

export default mutation;
