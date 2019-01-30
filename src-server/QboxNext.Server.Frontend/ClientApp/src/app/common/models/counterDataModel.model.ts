
export abstract class CounterDataValue {
    public label: string;

    public measureTime: Date;

    public delta0181: number;

    public delta0182: number;

    public delta0281: number;

    public delta0282: number;

    public delta2421: number;

    public net: number;

    public constructor(init?: Partial<CounterDataValue>) {
        Object.assign(this, init);
    }
}
