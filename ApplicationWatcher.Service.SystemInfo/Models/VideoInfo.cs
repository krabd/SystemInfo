namespace ApplicationWatcher.Service.SystemInfo.Models
{
    public class VideoInfo
    {
        public string AdapterCompatibility { get; set; }

        public string Caption { get; set; }

        public int ResolutionWidth { get; set; }

        public int ResolutionHeight { get; set; }

        public int RefreshRate { get; set; }

        public int BitsPerPixel { get; set; }
    }
}
