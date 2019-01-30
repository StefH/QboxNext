import { HttpStatusCodes } from '../constants';

export class ErrorMessage {

    public exceptionType: string;

    public message: string;

    public statusCode: HttpStatusCodes = HttpStatusCodes.OK;

    public constructor(init?: Partial<ErrorMessage>) {
        Object.assign(this, init);
    }
}
