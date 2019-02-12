import { Moment } from 'moment';
import * as moment from 'moment';

import { Resolution } from '../enums';

export class TimeRangeHelper {
    public getNextDate(resolution: Resolution, currentDate: Date): Date {
        return this.getNewDate(resolution, currentDate, 1);
    }

    public getPreviousDate(resolution: Resolution, currentDate: Date): Date {
        return this.getNewDate(resolution, currentDate, -1);
    }

    private getNewDate(resolution: Resolution, currentDate: Date, value: number): Date {
        const currentAsMoment = moment(currentDate);

        switch (resolution) {
            case Resolution.QuarterOfHour:
            case Resolution.Hour:
                return currentAsMoment.add(value, 'day').toDate();
            case Resolution.Day:
                return currentAsMoment.add(value, 'month').toDate();
            case Resolution.Month:
                return currentAsMoment.add(value, 'year').toDate();
            default:
                throw new Error('Invalid resolution');
        }
    }

    public getToDate(resolution: Resolution, from: Date): { fromDate: Moment, toDate: Moment } {
        const fromDate = moment(from);
        const toDate = moment(from);

        switch (resolution) {
            case Resolution.QuarterOfHour:
            case Resolution.Hour: toDate.add(1, 'day');
                break;
            case Resolution.Day: toDate.add(1, 'month');
                break;
            case Resolution.Month: toDate.add(1, 'year');
                break;
        }

        return {
            fromDate,
            toDate
        };
    }
}
