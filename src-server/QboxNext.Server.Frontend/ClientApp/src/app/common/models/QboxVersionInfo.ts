
export class QboxVersionInfo {

    public copyright: string;

    public informationalVersion: string;

    public constructor(init?: Partial<QboxVersionInfo>) {
        Object.assign(this, init);
    }
}
