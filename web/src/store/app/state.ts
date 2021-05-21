import { Drug } from 'src/components/model'

export interface AppStateInterface {
    regimenDialogOpen: boolean
    drug: Drug | null
}

function state(): AppStateInterface {
    return {
        regimenDialogOpen: false,
        drug: null
    };
}

export default state;
