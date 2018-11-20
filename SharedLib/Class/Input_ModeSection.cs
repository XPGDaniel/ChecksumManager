namespace SharedLib.Class
{
    public class Input_ModeSection
    {
        public Input_ModeSection()
        {
            Mode = Mode.Verify_Checksums;
            ProcessingPower = ProcessingPower.Single;
            Optional_Parameter = "";
        }
        public Mode Mode { get; set; }
        public ProcessingPower ProcessingPower { get; set; }
        public string Optional_Parameter { get; set; }
    }
}
