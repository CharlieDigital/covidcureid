// Type interfaces
export interface Option {
    label: string
    value: string
}

export interface Drug {
    drugName: string
    drugId: number
}

export interface Record extends Drug {
    improved: number
    undetermined: number
    deteriorated: number
}

export interface Regimen {
    id: string
    regimenId: number
    regimenName: string
    countryTreated: string
    outcomeComputed: string
    unusual: string
    adverseEvents: string
    additionalInfo: string
}