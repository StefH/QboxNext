import { Resolution } from '../enums';

export class QboxDataQuery {
  public from: string;

  public to: string;

  public resolution: Resolution;

  public adjustHours: boolean;

  public constructor(init?: Partial<QboxDataQuery>) {
    Object.assign(this, init);
  }
}
