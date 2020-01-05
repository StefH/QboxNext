import { Prices } from '../constants';

export class PriceService {

    public getElectricityPrice(year: number): number {
        return this.getFromMap(Prices.ElectricityMap, year);
    }

    public getGasPrice(year: number): number {
        return this.getFromMap(Prices.GasMap, year);
    }

    private getFromMap(map: Map<number, number>, year: number): number {
        const value: number | undefined = map.get(year);
        if (value) {
            return value;
        }

        const arrayFromMap = Array.from(map);
        const firstItemInMap = arrayFromMap[0];
        const lastItemInMap = arrayFromMap[map.size - 1];

        return year < firstItemInMap[0] ? firstItemInMap[1] : lastItemInMap[1];
    }
}
