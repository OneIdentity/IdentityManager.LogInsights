using System;

using LogInsights.Datastore;

namespace LogInsights
{
    public interface IExportSetting
    {
        void Prepare();

        string ExportAsJson();

        MessageMatchResult IsMessageMatch(TextMessage msg, object additionalData);

        Datastore.DataStore datastore { set; }
    }

    public enum MessageMatchResult
    {
        /// <summary>
        /// //the filter was not applied, means it could not decide whether to include or exclude the message from the exported result
        /// </summary>
        filterNotApplied = 0,

        /// <summary>
        /// exclude this message from the result export
        /// </summary>
        negative = 1,
            
        /// <summary>
        /// include this message in the result export
        /// </summary>
        positive = 2
        
    }

}
