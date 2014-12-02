using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Configuration;
using FireCom.Helpers;
using FireCom.Models;
using HtmlAgilityPack;

namespace FireCom.Controllers
{
    public class Scraper : FireComHandler
    {
        public int CallCounter { get; set; }
        private string _output;

        public void FirstFetch(string county)
        {

            try
            {

                // create an HtmlWeb Object
                var webGet = new HtmlWeb();

                // load WCCCA page
                var document = webGet.Load("http://www.wccca.com/PITSv2/");

                // get everything in the incidents div   
                //HtmlNode navNode = document.GetElementbyId(County + "-incidents");

                HtmlNode navNode = document.GetElementbyId(county + "-incidents");
                
                //string strXPath = navNode.XPath;

                // get raw call data
                string[] rawcalls = navNode.InnerText.Split(new string[] {"\n", "\r\n"},
                    StringSplitOptions.RemoveEmptyEntries);

                int counter = 0;
                var ourIncident = new Incident { County = county == "wccca" ? 1 : 2 };
                
                
                foreach (string s in rawcalls)
                {
                    int thiscounter = rawcalls.Count();
                    

                    // trim out trailing spaces and blank lines
                    string ourline = s.Trim();

                    // in case we missed some blank lines
                    if (ourline.Length <= 0) continue;

                    // delineates calls 
                    if (ourline.StartsWith("Call Type:"))
                    {
                        counter = 0;
                        // This is the known start of a call 
                        // we may need to flush and / or write here
                    }
                    else
                    {

                        // a switch here seemed like the best way. I'm not in love with it, but it works
                        switch (counter)
                        {

                            case 1:
                                ourIncident.CallType = ourline.ToUpper();

                                break;

                            case 3:
                                ourIncident.Address = ourline.ToUpper();
                                break;

                            case 4:
                                ourIncident.WNumber = ourline;

                                break;

                            case 6:
                                ourIncident.CallEntryTime = ourline;
                                break;

                            case 7:
                                ourIncident.DispatchTime = ourline;
                                break;

                            case 8:
                                ourIncident.EnRouteTime = ourline;
                                break;

                            case 9:
                                ourIncident.OnSceneTime = ourline;
                                break;

                            case 10:
                                ourIncident.Agency = ourline.Replace("/", "");
                                break;

                            case 11:
                                // last element
                                ourIncident.UnitList = ourline.Substring(6, ourline.Length - 6);

                                // get station name from beginning of unit list
                                ourIncident.Station = ourline.Substring(0, 4);

                                //TODO: Remove after construction
                                
                                SaveCall(ourIncident);
                                //TODO: Find a better place for this
                                
                                GetUnitTimes(Int32.Parse(ourIncident.WNumber));
                                //GetUnitTimes(Int32.Parse(ourIncident.WNumber));
                                break;
                        }
                    }
                    // increment master counter
                    
                    counter++;
                }
            }
            catch (Exception firstfetchException)
            {
                Debug.WriteLine(firstfetchException.ToString());
            }
        }

        private void GetUnitTimes(int wNumber)
        {
            try
            {
                var webGet = new HtmlWeb();
                DbStore ourDbStore = new DbStore();
                // load wccca page
                var document = webGet.Load("http://www.wccca.com/PITSv2/units.aspx?cn=" + wNumber);

                // Get all tables in the document
                HtmlNodeCollection tables = document.DocumentNode.SelectNodes("//table");

                // Iterate all rows in the first table
                HtmlNodeCollection rows = tables[0].SelectNodes(".//tr");
                //HtmlNode ourNode = tables[0].SelectSingleNode("//td").InnerText;

                for (int i = 0; i < rows.Count; ++i)
                {
                    // build a dictionary with all our values 
                    Dictionary<int, string> unitTimes = new Dictionary<int, string>();

                    UnitTime ourUnitTime = new UnitTime();

                    // Iterate all columns in this row
                    HtmlNodeCollection cols = rows[i].SelectNodes(".//tr");

                    

                    for (int j = 0; j < cols.Count; ++j)
                    {
                        unitTimes.Add(j, cols[j].InnerText);
                        ourDbStore.StoreUnitTime(unitTimes);
                    }

                    // write it to record
                    
                    //unitTimes.Clear();

                }


            }
            catch (Exception getUnitTimesException)
            {
                Debug.WriteLine(getUnitTimesException.ToString());
            }


        }

        public void GetLatLong()
        {
            var output = new System.Text.StringBuilder();

            var webGet = new HtmlWeb { UseCookies = true };

            // load wccca page
            var document = webGet.Load("http://www.wccca.com/PITSv2/?__VIEWSTATE=");

            try
            {
                IEnumerable<HtmlNode> pageJavascript = document.DocumentNode.Descendants("script").Where(x => x.Attributes.Contains("type"));

                foreach (var section in pageJavascript)
                {
                    string[] results = section.InnerText.Split(';');
                    int counter = 0;

                    foreach (var stuff in results)
                    {
                        if (stuff.Contains("LoadMarker"))
                        {
                            string ourString = stuff;
                            const string cdataGarbage = @"//<![CDATA[";

                            ourString = ourString.Replace(cdataGarbage, "").TrimEnd('\r', '\n').TrimStart('\r', '\n');

                            if (ourString.Length > 1)
                            {
                                // extract the Latitude from a known position in the string
                                string latitude = ourString.Substring(22, 16);
                                // extract the Longitude from a known position in the string
                                string longitude = ourString.Substring(51, 16);
                                // the "lefttovers" are where the WCCCA number is buried. Call descriptions vary the length
                                string leftovers = ourString.Substring(71, ourString.Length - 71);
                                // leftovers are separated by commas
                                string[] leftoverSplit = leftovers.Split(',');
                                // we know where the WCCCA Number is supposed to be, grab it and clean it up
                                string tempWNumber = leftoverSplit[1].Replace("'", "").Replace(" ", "");
                                // store data
                                storeLatLong(latitude, longitude, tempWNumber);
                            }

                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.ToString());
            }
        }

        private static void storeLatLong(string latitude, string longitude, string wNumber)
        {
            var ourDbStore = new DbStore();
            
            try
            {
                double tempLatitude = Convert.ToDouble(MakeCleanCoord(latitude, "lat"));
                double tempLongitude = Convert.ToDouble(MakeCleanCoord(longitude, "long"));
                int tempWnumber = Convert.ToInt32(wNumber);

                ourDbStore.StoreLatLong(tempLatitude, tempLongitude, tempWnumber);
            }
            catch (Exception storeLatLongException)
            {
                Debug.WriteLine(storeLatLongException.ToString());
            }
        }

        private static string MakeCleanCoord(string input, string coordtype)
        {
            var digitsOnly = new Regex(@"[^\d]");

            string newValue = digitsOnly.Replace(input, "");

            switch(coordtype)
            {
                case "lat":
                    newValue = newValue.Insert(2, ".");
                    break;
                case "long":
                    newValue = newValue.Insert(0, "-");
                    newValue = newValue.Insert(4, ".");
                    break;
            }

            return newValue;
        }


    }
}