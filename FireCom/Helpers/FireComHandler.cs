using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Web;
using FireCom.Controllers;
using FireCom.Models;

namespace FireCom.Helpers
{
    public class FireComHandler
    {
        // The Firecom object is the one object to rule them all. 
        // will contain common elements
        // calls to scraper
        // db writer
        // timer / profiler

        public void FirstScrape(string county)
        {
            var ourScraper = new Scraper();
            
            ourScraper.FirstFetch(county);
            ourScraper.GetLatLong();
        }

        // save call data
        protected void SaveCall(Incident ourIncident)
        {
            var ourDbStore = new DbStore();

            ourDbStore.StoreIncident(ourIncident);
            ourDbStore.UpdateTimes(ourIncident);
        }

        // store lat and long for call
        protected void StoreLatLong(string latitude, string longitude, string wNumber)
        {
            var ourDbStore = new DbStore();

            try
            {
                double tempLatitude = Convert.ToDouble(latitude);
                double tempLongitude = Convert.ToDouble(longitude);
                int tempWnumber = Convert.ToInt32(wNumber);

                ourDbStore.StoreLatLong(tempLatitude, tempLongitude, tempWnumber);
            }
            catch (Exception storeLatLongException)
            {
                Debug.WriteLine(storeLatLongException.ToString());
            }
        }

    }
}