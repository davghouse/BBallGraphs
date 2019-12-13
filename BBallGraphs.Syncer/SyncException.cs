using System;
using System.Runtime.Serialization;

namespace BBallGraphs.Syncer
{
    [Serializable]
    public class SyncException : Exception
    {
        public SyncException(string message, string url = null)
            : base(message + (url != null ? $"{Environment.NewLine}Url: {url}" : null))
            => Url = url;

        public string Url { get; }

        #region ISerializable

        protected SyncException(SerializationInfo info, StreamingContext context)
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
