using System;
using System.Text;

namespace LogInsights.Datastore
{
    public class SyncStepDetailQueryObject : DatastoreBaseDataRange
    {
        public bool isSuccessful;
        public int queryResult_NumberOfObjectsBeforeScope = -1;
        public int queryResult_NumberOfObjectsAfterScope = -1;
        public int queryResult_NumberOfObjectsFailed = 0;
        public int reloadObjectCountRequested = -1;
        public string options = "";
        public string schemaClassName = "";
        public string queryObjectDisplay = "";
        public SystemConnBelongsTo connectionSide;
        public QueryByEnum queryType; 
        public SystemConnType systemConnType;

        public SyncStepDetailQueryObject() { }

        public bool IsSuspicious
        {
            get
            {
                return                    
                    !isSuccessful
                    ||
                    isDataComplete &&
                    (
                        (queryResult_NumberOfObjectsBeforeScope > 0 && queryResult_NumberOfObjectsAfterScope == 0)
                        ||
                        (reloadObjectCountRequested > 0 && queryResult_NumberOfObjectsAfterScope == 0)
                        ||
                        (queryResult_NumberOfObjectsFailed > 0)
                        ||
                        (queryType == QueryByEnum.ReloadObject && durationSec > 3)
                        ||
                        (queryType == QueryByEnum.QueryByKey && durationSec > 10)
                        ||
                        (queryType == QueryByEnum.QueryByFilter && durationSec > 60 * 5)
                        ||
                        (queryType == QueryByEnum.QueryByExample && durationSec > 60 * 5)
                    ) ;
            }
        }

        public string GetAdditionalInformation()
        {
            StringBuilder sb = new StringBuilder();

            //1.) object, when available
            if (!string.IsNullOrEmpty(queryObjectDisplay))
                sb.Append($"Object: {queryObjectDisplay}; ");

            //2.) load counts
            //print information only when they are of interesst (e.g. greater than 0, count1 <> count2, ...)

            if (queryType == QueryByEnum.ReloadObject)
            {
                sb.Append($"Cnt objects to reload: {reloadObjectCountRequested}; ");
                if (queryResult_NumberOfObjectsFailed > 0)
                    sb.Append($"Cnt objects failed: {queryResult_NumberOfObjectsFailed}; ");

                sb.Append($"Cnt objects loaded: {queryResult_NumberOfObjectsAfterScope}; ");
            }
            else
            {
                if (queryResult_NumberOfObjectsFailed > 0)
                    sb.Append($"Cnt objects failed: {queryResult_NumberOfObjectsFailed}; ");

                if (queryResult_NumberOfObjectsBeforeScope >= 0 && queryResult_NumberOfObjectsAfterScope != queryResult_NumberOfObjectsBeforeScope)
                {
                    if (queryResult_NumberOfObjectsBeforeScope >= 0)
                        sb.Append($"Cnt objects loaded (before scope): {queryResult_NumberOfObjectsBeforeScope}; ");
                    if (queryResult_NumberOfObjectsAfterScope >= 0)
                        sb.Append($"Cnt objects loaded (after scope): {queryResult_NumberOfObjectsAfterScope}; ");
                }
                else
                {
                    if (queryResult_NumberOfObjectsAfterScope >= 0)
                        sb.Append($"Cnt objects loaded: {queryResult_NumberOfObjectsAfterScope}; ");
                }
            }

            //3.) options, if available
            if (!string.IsNullOrEmpty(options) && options.Length>5)
                sb.Append($"Options: {options}; ");

            //result
            return sb.ToString().Replace('\n', ' ').Replace('\r', ' ');
        }

        public override string ToString()
        {
            return string.Format("{5}-{6} by {9} [{7}, {10}]: obj='{8}'; class '{0}'; is {1}successful; {2} objects before scope; {3} objects after scope; options: {4}",
                                    schemaClassName,
                                    isSuccessful ? "" : "not ",
                                    queryResult_NumberOfObjectsBeforeScope,
                                    queryResult_NumberOfObjectsAfterScope,
                                    options,
                                    dtTimestampStart,
                                    dtTimestampEnd,
                                    queryType,
                                    queryObjectDisplay,
                                    systemConnType,
                                    IsSuspicious ? "isSuspicious" : ""
                                 );
        }
    }

    public enum QueryByEnum
    {
        UnknownQueryType,
        QueryByKey,  //PK=Value
        QueryByExample, //ObjectGuid=GuidVal AND DistinguishedName=DNVal AND Lastname=NameVal
        QueryByFilter, //Class definition: ObjectClass=Value
        ReloadObject  //almost the same as QueryByKey, but here an object was already found, changed and is now reloaded
    }
}
