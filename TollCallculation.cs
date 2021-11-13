using System;
using CommercialRegistration;
using ConsumerVehicleRegistration;
using LiveryRegistration;

namespace TollCallculation
{
    /// <summary>
    /// Callculate cost of ride.
    /// </summary>
    public class TollCallculation
    {
        private const decimal CARPRICE = 2.00m;
        private const decimal TAXIPRICE = 3.50m;
        private const decimal BUSPRICE = 5.00m;
        private const decimal TRUCKPRICE = 10.00m;
        private readonly DateTime time;
        private readonly object vehicle;
        private bool inbound;

        /// <summary>
        /// Initializes a new instance of the <see cref="TollCallculation"/> class.
        /// </summary>
        /// <param name="vehicle">The vehicle.</param>
        /// <param name="inbound">if set to <c>true</c> [inbound].</param>
        /// <param name="time">The time.</param>
        /// <exception cref="ArgumentNullException">vehicle.</exception>
        public TollCallculation(object vehicle, bool inbound, DateTime time)
        {
            this.vehicle = vehicle ?? throw new ArgumentNullException(nameof(vehicle));
            this.inbound = inbound;
            this.time = time;
        }

        /// <summary>
        /// Calculates this instance.
        /// </summary>
        /// <returns>Cost.</returns>
        public decimal Calculate() => this.CalculateToll() * this.PeakTimePremium();

        private decimal CalculateToll() => this.vehicle switch
        {
            Car c => c.Passangers switch
            {
                0 => CARPRICE + 0.5m,
                1 => CARPRICE,
                2 => CARPRICE - 0.5m,
                _ => CARPRICE - 1m,
            },

            Taxi t => t.Fares switch
            {
                0 => TAXIPRICE + 1.00m,
                1 => TAXIPRICE,
                2 => TAXIPRICE - 0.50m,
                _ => TAXIPRICE - 1.00m,
            },

            Bus b when ((double)b.Riders / (double)b.Capacity) < 0.50d => BUSPRICE + 2.00m,
            Bus b when ((double)b.Riders / (double)b.Capacity) > 0.90d => BUSPRICE - 1.00m,

            DeliveryTruck t when t.GrossWeightClass > 5000 => TRUCKPRICE + 5.00m,
            DeliveryTruck t when t.GrossWeightClass < 3000 => TRUCKPRICE - 2.00m,
            DeliveryTruck => TRUCKPRICE,
            { } => throw new ArgumentException("Not a known vehicle type", nameof(this.vehicle)),
            null => throw new ArgumentNullException(nameof(this.vehicle)),
        };

        private bool IsWeekDay() =>
            this.time.DayOfWeek switch
            {
                DayOfWeek.Saturday => false,
                DayOfWeek.Sunday => false,
                _ => true,
            };

        private TimeBand GetTimeBand() =>
            this.time.Hour switch
            {
                < 6 or > 19 => TimeBand.Overnight,
                < 10 => TimeBand.MorningRush,
                < 16 => TimeBand.Daytime,
                _ => TimeBand.EveningRush,
            };

        private decimal PeakTimePremium() =>
            (this.IsWeekDay(), this.GetTimeBand(), this.inbound) switch
        {
            (true, TimeBand.Overnight, _) => 0.75m,
            (true, TimeBand.Daytime, _) => 1.5m,
            (true, TimeBand.MorningRush, true) => 2.0m,
            (true, TimeBand.EveningRush, false) => 2.0m,
            (_, _, _) => 1.0m,
        };
    }
}
