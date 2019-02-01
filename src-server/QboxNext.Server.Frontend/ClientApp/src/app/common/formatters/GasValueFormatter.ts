import { IValueFormatter } from './IValueFormatter';

export class GasValueFormatter implements IValueFormatter {

    public format(value: number, showZero: boolean = true): string {
        if (value === 0 && !showZero) {
            return `-`;
        }

        if (value < 10) {
            return `${value} L`;
        }

        if (value < 100) {
            return `${value} L`;
        }

        if (value < 1000) {
            return `${value} L`;
        }

        return `${(value / 1000).toFixed(1)} M&sup3;`;
    }
}
