using System;
using System.Collections.Generic;
using BlazingPizza.ComponentsLibrary.Map;

namespace BlazingPizza
{
    public class OrderWithStatus
    {
        public readonly static TimeSpan PreparationDuration = TimeSpan.FromSeconds(10);
        public readonly static TimeSpan DeliveryDuration = TimeSpan.FromMinutes(1); // Unrealistic, but more interesting to watch

        // Set from DB
        public Order Order { get; set; } = null!;

        // Set from Order
        public string StatusText { get; set; } = null!;

        public bool IsDelivered => StatusText == "Delivered";

        public List<Marker> MapMarkers { get; set; } = null!;

        // Nuevo campo para el tiempo restante de preparación (1 minuto total)
        public TimeSpan RemainingPreparationTime { get; set; } = TimeSpan.FromMinutes(1);

        public static OrderWithStatus FromOrder(Order order)
        {
            ArgumentNullException.ThrowIfNull(order.DeliveryLocation);

            // Calcular el tiempo restante de preparación (1 minuto total)
            var remainingPreparationTime = PreparationDuration + DeliveryDuration - (DateTime.Now - order.CreatedTime);

            // Si el tiempo restante es menor que cero, establecerlo en cero
            if (remainingPreparationTime < TimeSpan.Zero)
            {
                remainingPreparationTime = TimeSpan.Zero;
            }

            // Calcular los marcadores del mapa y el estado del pedido
            string statusText;
            List<Marker> mapMarkers;

            if (DateTime.Now < order.CreatedTime + PreparationDuration)
            {
                // Si la hora actual es antes de la hora de despacho, el pedido se está preparando
                statusText = "Preparing";
                mapMarkers = new List<Marker>
                {
                    ToMapMarker("You", order.DeliveryLocation, showPopup: true)
                };
            }
            else if (DateTime.Now < order.CreatedTime + PreparationDuration + DeliveryDuration)
            {
                // Si la hora actual está dentro del tiempo de entrega, el pedido está en camino
                statusText = "Out for delivery";

                var startPosition = ComputeStartPosition(order);
                var proportionOfDeliveryCompleted = Math.Min(1, (DateTime.Now - order.CreatedTime - PreparationDuration).TotalMilliseconds / DeliveryDuration.TotalMilliseconds);
                var driverPosition = LatLong.Interpolate(startPosition, order.DeliveryLocation, proportionOfDeliveryCompleted);
                mapMarkers = new List<Marker>
                {
                    ToMapMarker("You", order.DeliveryLocation),
                    ToMapMarker("Driver", driverPosition, showPopup: true),
                };
            }
            else
            {
                // Si la hora actual es después del tiempo de entrega, el pedido ha sido entregado
                statusText = "Delivered";
                mapMarkers = new List<Marker>
                {
                    ToMapMarker("Delivery location", order.DeliveryLocation, showPopup: true),
                };
            }

            // Crear y devolver una nueva instancia de OrderWithStatus con los datos calculados
            return new OrderWithStatus
            {
                Order = order,
                StatusText = statusText,
                MapMarkers = mapMarkers,
                RemainingPreparationTime = remainingPreparationTime
            };
        }

        // Método para calcular la posición inicial del conductor (para simular)
        private static LatLong ComputeStartPosition(Order order)
        {
            ArgumentNullException.ThrowIfNull(order.DeliveryLocation);
            // Generar una posición aleatoria pero determinista basada en el ID del pedido
            var rng = new Random(order.OrderId);
            var distance = 0.01 + rng.NextDouble() * 0.02;
            var angle = rng.NextDouble() * Math.PI * 2;
            var offset = (distance * Math.Cos(angle), distance * Math.Sin(angle));
            return new LatLong(order.DeliveryLocation.Latitude + offset.Item1, order.DeliveryLocation.Longitude + offset.Item2);
        }

        // Método auxiliar para crear un marcador en el mapa
        static Marker ToMapMarker(string description, LatLong coords, bool showPopup = false)
            => new Marker { Description = description, X = coords.Longitude, Y = coords.Latitude, ShowPopup = showPopup };
    }
}
