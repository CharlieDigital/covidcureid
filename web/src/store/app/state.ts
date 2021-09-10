import { Drug } from 'src/components/model'

export interface AppStateInterface {
    drug: Drug | null
}

function state(): AppStateInterface {
    return {
        drug: null
    };
}

export default state;
