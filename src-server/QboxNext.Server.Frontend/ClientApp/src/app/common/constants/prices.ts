export class Prices {
    // kWh
    public static readonly ElectricityMap: Map<number, number> = new Map([
        [2015, 0.202045], // Greenchoice (Hoog = 0.21406, Laag = 0.19003)
        [2016, 0.1828], // Qurrent (Hoog = 0.1754 ?, Laag = 0.1902 ?)
        [2017, 0.18691], // Pure Energy (Hoog = 0.19526 ?, Laag = 0.17856 ?)
        [2018, 0.229808], // Powerpeers
        [2019, 0.231471], // Powerpeers
        [2020, 0.24051] // Powerpeers
    ]);

    // m³
    public static readonly GasMap: Map<number, number> = new Map([
        [2015, 0.65736], // Greenchoice
        [2016, 0.6026], // Qurrent
        [2017, 0.59663], // Pure Energy
        [2018, 0.71], // Powerpeers  (0.71 ?)
        [2019, 0.713626], // Powerpeers
        [2020, 0.792325] // Powerpeers
    ]);
}
