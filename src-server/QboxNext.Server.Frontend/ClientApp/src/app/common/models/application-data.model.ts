export class ApplicationData {
    public gasSelectedFromDate: Date;

    public gasSelectedToDate: Date;

    public gasSelectedResolutionId: string;

    public electricitySelectedFromDate: Date;

    public electricitySelectedToDate: Date;

    public electricitySelectedResolutionId: string;

    public check181: boolean;

    public check182: boolean;

    public check281: boolean;

    public check282: boolean;

    public checknet: boolean;

    public checkall: boolean;

    public constructor(init?: Partial<ApplicationData>) {
        Object.assign(this, init);
    }
}
