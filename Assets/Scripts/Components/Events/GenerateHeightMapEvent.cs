namespace Components.Events
{
    public struct GenerateHeightMapEvent
    {
        public bool OnGPU { get; set; }
        public bool PrintTimers { get; set; }
    }
}