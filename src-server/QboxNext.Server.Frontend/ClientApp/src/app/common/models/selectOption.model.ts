
export abstract class SelectOptionModel {

    public key: string;

    public name: string;

    public value: number;

    public constructor(init?: Partial<SelectOptionModel>) {
        Object.assign(this, init);
    }
}
