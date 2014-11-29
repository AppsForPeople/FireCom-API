using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Linq;
using System.Web;
using FireCom.Models;

namespace FireCom.Helpers
{
    public class DbStore : IDataStore
    {
        private readonly SqlConnection _sqlConnection;
        private int _wNumber;
        public SqlConnection Conn { get; set; }

        public DbStore()
        {
            _sqlConnection = new SqlConnection(ConfigurationManager.ConnectionStrings["DefaultConnection"].ToString());
        }

        public void UpdateTimes(Incident ourIncident)
        {
            //[DispatchTime],[EnRouteTime],[OnSceneTime],[CallEntryTime],

            try
            {
                if (_sqlConnection.State == ConnectionState.Closed)
                {
                    _sqlConnection.Open();
                }

                /* to be repeated? */

                // build our SQL Query
                string ourSql = "UPDATE [Incident] SET ";

                if (ourIncident.DispatchTime != "--:--:--")
                {
                    ourSql += "[DispatchTime] = @DispatchTime";
                }
                if (ourIncident.EnRouteTime != "--:--:--")
                {
                    ourSql += ",[EnRouteTime] = @EnRouteTime ";
                }
                if (ourIncident.OnSceneTime != "--:--:--")
                {
                    ourSql += ",[OnSceneTime] = @OnSceneTime ";
                }
                if (ourIncident.CallEntryTime != "--:--:--")
                {
                    ourSql += ",[CallEntryTime] = @CallEntryTime ";
                }

                ourSql += "WHERE [WNumber] = @WNumber";

                var cmd = new SqlCommand(ourSql)
                {
                    Connection = _sqlConnection
                };

                if (ourIncident.DispatchTime != "--:--:--")
                {
                    cmd.Parameters.AddWithValue("@DispatchTime", DateTime.Parse(ourIncident.DispatchTime));
                }
                if (ourIncident.EnRouteTime != "--:--:--")
                {
                    cmd.Parameters.AddWithValue("@EnRouteTime", DateTime.Parse(ourIncident.EnRouteTime));
                }
                if (ourIncident.OnSceneTime != "--:--:--")
                {
                    cmd.Parameters.AddWithValue("@OnSceneTime", DateTime.Parse(ourIncident.OnSceneTime));
                }
                if (ourIncident.CallEntryTime != "--:--:--")
                {
                    cmd.Parameters.AddWithValue("@CallEntryTime", DateTime.Parse(ourIncident.CallEntryTime));
                }

                cmd.Parameters.AddWithValue("@WNumber", ourIncident.WNumber);

                cmd.ExecuteScalar();

                _sqlConnection.Close();
            }
            catch (Exception storeInicidentSqlEx)
            {
                Debug.WriteLine(storeInicidentSqlEx.ToString());
                _sqlConnection.Close();
            }
        }

        public void StoreIncident(Incident ourIncident)
        {
            _wNumber = Int32.Parse(ourIncident.WNumber);

            if (CheckIncidentExists(_wNumber) == false)
            {
                try
                {
                    if (_sqlConnection.State == ConnectionState.Closed)
                    {
                        _sqlConnection.Open();
                    }

                    //TODO: Fix this empty date problem. Maybe initially insert the call then update afterward

                    var cmd = new SqlCommand("INSERT INTO [Incident] ([WNumber],[County],[CallType],[Units],[Address],[Agency],[Station],[Lat],[Long],[IsActive]) VALUES (@WNumber, @County, @CallType, @Units, @Address,@Agency,@Station,NULL,NULL,@IsActive);")
                        {
                            Connection = _sqlConnection
                        };

                    cmd.Parameters.AddWithValue("@WNumber", ourIncident.WNumber);
                    cmd.Parameters.AddWithValue("@County", ourIncident.County);
                    cmd.Parameters.AddWithValue("@CallType", ourIncident.CallType);
                    cmd.Parameters.AddWithValue("@Units", ourIncident.UnitList);
                    cmd.Parameters.AddWithValue("@Address", ourIncident.Address);
                    cmd.Parameters.AddWithValue("@Agency", ourIncident.Agency);
                    cmd.Parameters.AddWithValue("@Station", ourIncident.Station);
                    cmd.Parameters.AddWithValue("@IsActive", true);

                    cmd.ExecuteScalar();
                    _sqlConnection.Close();
                }
                catch (Exception storeInicidentSqlEx)
                {
                    Debug.WriteLine(storeInicidentSqlEx.ToString());
                    _sqlConnection.Close();
                }

                Debug.WriteLine("Added New" + _wNumber);
            }
            else
            {
                //TODO: complete update function
                // update some stuff.
                UpdateTimes(ourIncident);
            }
        }

        public void RetrieveIncident()
        {
        }

        public bool CheckIncidentExists(int wNumber)
        {
            int lookupResult = 0;

            try
            {
                if (_sqlConnection.State == ConnectionState.Closed)
                {
                    _sqlConnection.Open();
                }

                var command = new SqlCommand("SELECT COUNT([WNumber]) Total FROM [Incident] WHERE [WNumber] = @WNumber")
                {
                    Connection = _sqlConnection
                };

                command.Parameters.AddWithValue("@WNumber", wNumber);

                SqlDataReader reader = command.ExecuteReader();

                while (reader.Read())
                {
                    lookupResult = Int32.Parse(reader["Total"].ToString().Trim());
                }
            }
            catch (Exception checkIncidentSqlEx)
            {
                Debug.WriteLine(checkIncidentSqlEx.ToString());
                _sqlConnection.Close();
                lookupResult = 0;
            }

            _sqlConnection.Close();

            return lookupResult > 0;
        }

        public void StoreLatLong(double latitude, double longitude, int wNumber)
        {
            // because we are pulling lat/long for both WA and CC we want to check if the incident exists
            if (CheckIncidentExists(wNumber) != true) return;
            try
            {
                if (_sqlConnection.State == ConnectionState.Closed)
                {
                    _sqlConnection.Open();
                }

                var cmd =
                    new SqlCommand("UPDATE [Incident] SET [Lat] = @lat, [Long] = @long WHERE [WNumber] = @WNumber")
                    {
                        Connection = _sqlConnection
                    };

                cmd.Parameters.AddWithValue("@WNumber", wNumber);
                cmd.Parameters.AddWithValue("@lat", latitude);
                cmd.Parameters.AddWithValue("@long", longitude);

                cmd.ExecuteScalar();
                _sqlConnection.Close();
            }
            catch (Exception storeInicidentSqlEx)
            {
                Debug.WriteLine(storeInicidentSqlEx.ToString());
                _sqlConnection.Close();
            }
        }

        public void StoreUnitTime(Dictionary<int, string> ourUnitTimes)
        {
            // prepare data for storage

            int wNumber = Int32.Parse(ourUnitTimes[0]);
        }
    }
}