export class LoginModel {

    public accessToken: string;

    public expiresAt: number;

    public serialNumber: string;

    public getAuthenticationHeader(): string {
        return `Bearer ${this.accessToken}`;
    }

    public isExpired(): boolean {
        return new Date().getTime() > this.expiresAt;
    }

    public constructor(init?: Partial<LoginModel>) {
        Object.assign(this, init);
    }
}
