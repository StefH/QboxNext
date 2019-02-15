
export class QboxVersionInfo {

    public version: string;

    public constructor(init?: Partial<QboxVersionInfo>) {
        Object.assign(this, init);
    }
}
