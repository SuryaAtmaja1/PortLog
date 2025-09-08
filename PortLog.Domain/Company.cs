using System.Data.Common;
using System.Xml.Schema;

namespace PortLog.Domain
{
    class Company
    {
        private int _uuid;
        private string _name;
        private string _address;
        private string _email;
        private string _contracts;
        private int _totalFleet;
        private DateTime _createdAt;
        private DateTime _updatedAt;

        // Getters and Setters
        public int Uuid
        {
            get { return _uuid; }
        }

        public string Name
        {
            get { return _name; }
            set { _name = value; }
        }
        public string Address
        {
            get { return _address; }
            set { _address = value; }
        }
        public string Email
        {
            get { return _email; }
            set { _email = value; }
        }
        public string Contracts
        {
            get { return _contracts; }
            set { _contracts = value; }
        }
        public int TotalFleet
        {
            get { return _totalFleet; }
        }

        public DateTime CreatedAt
        {
            get { return _createdAt; }
            set { _createdAt = value; }
        }

        public DateTime UpdatedAt
        {
            get { return _updatedAt; }
            set { _updatedAt = value; }
        }

        //Method
        public void Addship(Ship ship)
        {
            if (ship == null) return;
            _totalFleet++;
            _updatedAt = DateTime.UtcNow;
        }
        public void Removeship(uuid shipid)
        {
            if (shipid == null) return;
            _totalFleet--;
            _updatedAt = DateTime.UtcNow;
        }
        public void UpdateInfo(Company data)
        {
            if (data == null) return;

            _name = data.Name;
            _address = data.Address;
            _email = data.Email;
            _contracts = data.Contracts;
            _totalFleet = data.TotalFleet;
            _updatedAt = DateTime.UtcNow;
        }

        public Ship getFleet()
        {
            return new Ship();
        }

        public Insight GetInsight(DateRange range)
        {
            return new Insight();
        }
    }
}