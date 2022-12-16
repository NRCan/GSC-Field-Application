using System.Collections.Generic;

namespace GSCFieldApp.Models
{
    public class SemanticDataGroup
    {
        public string Name { get; set; }

        public List<SemanticData> Items { get; set; }
    }

    public class SemanticData
    {
        public SemanticData(string title, string subtitle)
        {
            Title = title;
            Subtitle = subtitle;
        }

        public string Title { get; private set; }
        public string Subtitle { get; private set; }
    }
}
