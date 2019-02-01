import { IValueFormatter } from './IValueFormatter';

export class ElectricityValueFormatter implements IValueFormatter {

    public format(value: number, showZero: boolean = true): string {
        if (value === 0 && !showZero) {
            return `-`;
        }

        if (Math.abs(value) < 10000) {
            return `${value} W`;
        }

        if (Math.abs(value) < 100000) {
            return `${(value / 1000).toFixed(1)} KW`;
        }

        return `${(value / 1000).toFixed(0)} KW`;
    }
}
