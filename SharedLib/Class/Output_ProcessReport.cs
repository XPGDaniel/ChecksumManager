namespace SharedLib.Class
{
    public class Output_ProcessReport
    {public Output_ProcessReport()
        {
            Healthy = Damaged = Missing = Total = 0;
        }
        public int Healthy { get; set; }
        public int Damaged { get; set; }
        public int Missing { get; set; }
        public int Total { get; set; }
    }
}
