export class ApplicationData {
    public gasSelectedFromDate: Date;

    public gasSelectedToDate: Date;

    public gasSelectedResolutionId: string;

    public constructor(init?: Partial<ApplicationData>) {
        Object.assign(this, init);
    }
}
