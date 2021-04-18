using System;
using System.Runtime.Serialization;

namespace BBallGraphs.BasketballReferenceScraper
{
    [Serializable]
    public class ScrapeException : Exception
    {
        public ScrapeException(string message, string url, string pageSource)
            : base($"{message}{Environment.NewLine}Url: {url}{Environment.NewLine}Page source: {pageSource}")
        {
            Url = url;
            PageSource = pageSource;
        }

        public string Url { get; }
        public string PageSource { get; }

        #region ISerializable

        protected ScrapeException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            Url = info.GetString(nameof(Url));
            PageSource = info.GetString(nameof(PageSource));
        }

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue(nameof(Url), Url);
            info.AddValue(nameof(PageSource), PageSource);
            base.GetObjectData(info, context);
        }

        #endregion ISerializable
    }
}
