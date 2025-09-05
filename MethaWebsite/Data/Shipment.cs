namespace MethaWebsite.Data
{
    public class Shipment
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string? OrderId { get; set; }
        public DateTime ArrivalDate { get; set; }
        public DateTime ShipmentDate { get; set; }
        public string? CurrentLocation { get; set; }
        public List<string>? PreviousLocations { get; set; }
    }
}
