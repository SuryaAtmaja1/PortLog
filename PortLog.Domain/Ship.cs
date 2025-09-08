namespace PortLog.Domain
{
  public class Ship
  {
    // Attributes
    private int _uuid;
    private int _registerId;
    private int _companyId;
    private string _name;
    private string _type;
    private int _captainId;
    private ShipStatus _status;

    //Getter and Setters
    public int Uuid
    {
      get { return _uuid; }
    }
    public int RegisterId
    {
      get { return _registerId; }
      set { _registerId = value; }
    }
    public int CompanyId
    {
      get { return _companyId; }
      set { _companyId = value; }
    }
    public string Name
    {
      get { return _name; }
      set { _name = value; }
    }
    public string Type
    {
      get { return _type; }
      set { _type = value; }
    }
    public int CaptainId
    {
      get { return _captainId; }
      set { _captainId = value; }
    }
    public ShipStatus Status
    {
      get { return _status; }
      set { _status = value; }
    }
    //Methods
    public void UpdateInfo(Ship data)
    {

    }
    public void addLog(TelemetryLog log)
    {

    }
    public void getLogs(DateRange  range)
    {

    }
    public void getCurrentLocation()
    {

    }
  }
}