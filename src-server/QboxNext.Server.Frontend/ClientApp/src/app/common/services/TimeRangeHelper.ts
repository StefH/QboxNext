import * as moment from 'moment';

export class TimeRangeHelper {
    public getToDate(resolution: string, from: Date): Date {
        const fromMoment = moment(from);

        switch (resolution) {
            case 'QuarterOfHour':
            case 'Hour': return fromMoment.add(1, 'day').startOf('day').toDate();
            case 'Day': return fromMoment.add(1, 'month').startOf('day').toDate();
            case 'Month': return fromMoment.add(1, 'year').startOf('day').toDate();
            case 'Year': return fromMoment.add(1, 'year').startOf('day').toDate();
        }
    }
}
