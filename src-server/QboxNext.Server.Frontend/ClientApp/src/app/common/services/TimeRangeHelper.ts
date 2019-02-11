import { Moment } from 'moment';
import * as moment from 'moment';

import { Resolution } from '../enums';

export class TimeRangeHelper {
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
