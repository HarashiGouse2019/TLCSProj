namespace TLCSProj.UI
{
    internal class Label
    {
        internal string Content = string.Empty;
  
        internal int Size {
            get
            {
                return (Content != string.Empty) ? Content.Length : 0;
            }
        }

        public static Label NewLine { get; internal set; } = new Label("\n");

        internal Label(string content)
        {
            Content = content;
        }
    }

    
}
