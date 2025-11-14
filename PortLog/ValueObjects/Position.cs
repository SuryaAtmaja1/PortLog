namespace PortLog.ValueObjects
{
    public class Position
    {
        public float Lat { get; }
        public float Lon { get; }
        public float Heading { get; }
        public float Speed { get; }

        public Position(float lat, float lon, float heading, float speed)
        {
            Lat = lat;
            Lon = lon;
            Heading = heading;
            Speed = speed;
        }
    }
}