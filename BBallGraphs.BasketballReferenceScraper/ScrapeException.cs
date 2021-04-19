using System;
using System.Runtime.Serialization;

namespace BBallGraphs.BasketballReferenceScraper
{
    [Serializable]
    public class ScrapeException : Exception
    {
        public ScrapeException(string message, string url)
            : base($"{message}{Environment.NewLine}Url: {url}")
            => Url = url;

        public string Url { get; }

        #region ISerializable

        protected ScrapeException(SerializationInfo info, StreamingContext context)
            : base(info, context)
            => Url = info.GetString(nameof(Url));

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue(nameof(Url), Url);
            base.GetObjectData(info, context);
        }

        #endregion ISerializable
    }
}
