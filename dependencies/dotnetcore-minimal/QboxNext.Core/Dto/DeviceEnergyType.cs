namespace QboxNext.Core.Dto
{
    public enum DeviceEnergyType
    {
        Consumption = 0,    // Bruto verbruik
        Generation = 1,     // Opwek
        Net = 2,            // Netto verbruik/teruglevering
        Gas = 5,
        NetHigh = 7,        // Netto verbruik/teruglevering hoog tarief (SAM: is this still needed?)
        NetLow = 8          // Netto verbruik/teruglevering laag tarief
    }
}
