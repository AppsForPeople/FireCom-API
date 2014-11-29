using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Web.Http;
using System.Web.UI.WebControls;
using FireCom.Helpers;

namespace FireCom.Controllers
{
    public class FetchController : ApiController
    {
        // GET: api/Fetch
        /*
        public IEnumerable<string> Get()
        {
            return new string[] { "value1", "value2" };
        }
        */

        // GET: api/Fetch/5

        public HttpResponseMessage Get(string id)
        {
            //TODO: make split between WA/CC

            var ourFireComHandler = new FireComHandler();

            if (id != null)
            {
                switch (id)
                {
                    // the only reason we split the counties here is load balancing. 
                    // I want to be sure each call doesn't time out so we'll only scrape
                    // one county at a time, and put them in the same db.

                    case "ccom":
                        // fetch Washington County Calls
                        ourFireComHandler.FirstScrape("ccom");
                        break;

                    case "wccca":
                        // fetch Clackamas County Calls
                        ourFireComHandler.FirstScrape("wccca");
                        break;

                }   // end switch

            } // end if
            HttpResponseMessage response = Request.CreateResponse(HttpStatusCode.OK, "value");
            response.Content = new StringContent("Success", Encoding.Unicode);


            return response;


        }// end Get Method

    }   // end class

}   // end namespace
