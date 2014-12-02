namespace FireCom.Models
{
    public struct Incident
    {
        //TODO: Get Unit Info
        //TODO: Some error handling!

        public int County { get; set; }
        public string CallType { get; set; }
        public string Address { get; set; }
        public string WNumber { get; set; }
        public string CallEntryTime { get; set; }
        public string DispatchTime { get; set; }
        public string EnRouteTime { get; set; }
        public string OnSceneTime { get; set; }
        public string Station { get; set; }
        public string Agency { get; set; }
        public string UnitList { get; set; }
        public bool IsActive { get; set; }

        internal void FlushData()
        {
            

        }
    }
}