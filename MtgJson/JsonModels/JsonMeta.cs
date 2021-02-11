using System;

namespace MtgJson.JsonModels
{
    public class JsonMeta
    {
        public DateTime Date { get; set; } = DateTime.MinValue;

        public string Version { get; set; } = "";
    }
}
