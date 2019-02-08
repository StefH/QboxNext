import { Moment } from 'moment';
import * as moment from 'moment';

export class TimeRangeHelper {
    public getToDate(resolution: string, from: Date): { fromDate: Moment, toDate: Moment } {
        const fromDate = moment(from);
        const toDate = moment(from);

        switch (resolution) {
            case 'QuarterOfHour':
            case 'Hour': toDate.add(1, 'day');
                break;
            case 'Day': toDate.add(1, 'month');
                break;
            case 'Month': toDate.add(1, 'year');
                break;
        }

        return {
            fromDate,
            toDate
        };
    }
}
