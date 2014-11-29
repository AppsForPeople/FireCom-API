using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FireCom.Models;

namespace FireCom.Helpers
{
    interface IDataStore
    {
        void RetrieveIncident();
        bool CheckIncidentExists(int wNumber);
        void StoreIncident(Incident ourIncident);
        void UpdateTimes(Incident ourIncident);
        void StoreLatLong(double latitude, double longitude, int wNumber);
        void StoreUnitTime(Dictionary<int, string> ourUnitTimes);
    }
}
