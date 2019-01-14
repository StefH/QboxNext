using System;

namespace QboxNext.Qserver.Core.Model
{
    /// <summary>
    /// Counter Sensor Mapping data holder
    /// Used during the dump of the qbox message so we can return the Ecospace 
    /// to the pool quickly. See MiniPoco.
    /// </summary>
    public class CounterSensorMappingPoco
    {
        public DateTime PeriodeBegin { get; set; }
        public decimal Formule { get; set; }
    }
}