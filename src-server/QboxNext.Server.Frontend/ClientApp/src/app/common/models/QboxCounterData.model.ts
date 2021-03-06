import { QboxDataQuery } from './QboxDataQuery';

export class QboxCounterData {
    public labelText: string;

    public labelValue: string;

    public drillDownQuery: QboxDataQuery;

    public delta0181: number;

    public delta0182: number;

    public delta0281: number;

    public delta0282: number;

    public delta2421: number;

    public net: number;

    public costs: number;

    public constructor(init?: Partial<QboxCounterData>) {
        Object.assign(this, init);
    }
}
