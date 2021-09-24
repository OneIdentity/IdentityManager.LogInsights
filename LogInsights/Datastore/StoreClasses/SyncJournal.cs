using System;
using System.Collections.Generic;
using System.Linq;

using LogInsights.Helpers;

namespace LogInsights.Datastore
{
    public class DprJournal: SqlObject
    {
        public List<DprJournalSetupElems> journalSetupElems = new List<DprJournalSetupElems>();
        public List<DprJournalMessage> journalMessages = new List<DprJournalMessage>();
        public List<DprJournalFailure> journalFailures = new List<DprJournalFailure>();
        public List<DprJournalObject> journalObjects = new List<DprJournalObject>();

        
        public string belongsToProjectionUuid = "";

        public bool isAdHocProjection
        {
            get {
                bool isFullSync = Get("ProjectionContext") == "Full" && Get("UID_DPRProjectionStartInfo") != "";
                return !isFullSync;
            }
        }

        public DprJournal() : base("dprjournal")
        { }

        public List<KeyValuePair<string, string[]>> GetSetupElements(bool isForConnectedSystem, string[] columns)  //returns a list of rows; each row is a pair of key==PK, value==list of column values
        {
            List<KeyValuePair<string, string[]>> result = new List<KeyValuePair<string, string[]>>();

            string filter = isForConnectedSystem ? "ConnectedSystemConnection" : "MainConnection";

            foreach (var setup in journalSetupElems.Where(el => el.Get("OptionContext") == filter))
                result.AddRange(setup.GetSetupElements(columns));

            return result;
        }

        public List<KeyValuePair<string, string[]>> GetJournalObjects(string[] columns)  //returns a list of rows; each row is a pair of key==PK, value==list of column values
        {
            List<KeyValuePair<string, string[]>> result = new List<KeyValuePair<string, string[]>>();

            foreach (var obj in journalObjects.OrderBy(o => o.OrderSequenceId))
                result.Add(new KeyValuePair<string, string[]>(
                                obj.uuid,
                                columns.Select(col => obj.Get(col)).ToArray()
                            ));
            return result;
        }

        public List<KeyValuePair<string, string[]>> GetJournalObjectsProperties(string uuid_journalObject, bool showOldValues=true, bool showNewValues=true)  //returns a list of rows; each row is a pair of key==PK, value==list of column values
        {
            List<KeyValuePair<string, string[]>> result = new List<KeyValuePair<string, string[]>>();
            List<string> row = new List<string>();
            bool shortVal;

            foreach (DprJournalObject obj in journalObjects.Where(jo => jo.uuid == uuid_journalObject || uuid_journalObject == "").OrderBy(o => o.OrderSequenceId))
                foreach (DprJournalProperty prop in obj.dprJournalProperties)
                {
                    row.Clear();
                    row.Add(prop.Get("Propertyname"));
                    if (showOldValues)
                    {
                        shortVal = prop.Get("isoldvalueFull") != "1";
                        row.Add(shortVal ? prop.Get("OldValueShort") : prop.Get("OldValueFull"));  //the full property is not logged in classic SQL log for Insert commands as it is a clob
                    }

                    if (showNewValues)
                    {
                        shortVal = prop.Get("IsNewValueFull") != "1";
                        row.Add(shortVal ? prop.Get("NewValueShort") : prop.Get("NewValueFull"));  //the full property is not logged in classic SQL log for Insert commands as it is a clob
                    }

                    result.Add(new KeyValuePair<string, string[]>(prop.uuid, row.ToArray()));
                }

            return result;
        }


        public List<KeyValuePair<string, string[]>> GetJournalMessages()  //returns a list of rows; each row is a pair of key==PK, value==list of column values
        {
            List<KeyValuePair<string, string[]>> result = new List<KeyValuePair<string, string[]>>();
            List<string> row = new List<string>();

            foreach (var jmsg in journalMessages.OrderBy(mm => mm.OrderSequenceId))
            {
                row.Clear();
                row.Add(jmsg.dtTimestampStart.ToString("G"));
                row.Add(jmsg.Get("MessageContext"));
                row.Add(jmsg.Get("MessageType"));
                row.Add(StringHelper.TranslateLds(jmsg.Get("MessageString")));
                row.Add(jmsg.Get("MessageSource"));

                result.Add(new KeyValuePair<string, string[]>(
                                jmsg.uuid,
                                row.ToArray()
                            ));
            }
                
            return result;
        }

        public List<KeyValuePair<string, string[]>> GetJournalFailures(string[] columns)  //returns a list of rows; each row is a pair of key==PK, value==list of column values
        {
            List<KeyValuePair<string, string[]>> result = new List<KeyValuePair<string, string[]>>();
            List<string> row = new List<string>();

            foreach (var obj in journalFailures)
            {
                row.Clear();
                row.Add(obj.dtTimestampStart.ToString("G"));
                row.AddRange(columns.Select(col => StringHelper.TranslateLds(obj.Get(col))).ToArray());
                result.Add(new KeyValuePair<string, string[]>(
                                obj.uuid,
                                row.ToArray()
                            ));
            }
            return result;
        }

        public List<KeyValuePair<string, string[]>> GetJournalFailedObjAndProps(string uuid_dprJournalFailure)  //returns a list of rows; each row is a pair of key==PK, value==list of column values
        {
            List<KeyValuePair<string, string[]>> result = new List<KeyValuePair<string, string[]>>();

            if (string.IsNullOrEmpty(uuid_dprJournalFailure))
                return result;

            
            var failure = journalFailures
                            .Where(o => o.uuid == uuid_dprJournalFailure)
                            .FirstOrDefault();

            if (failure == null)
                return result;


            List<string> dprJournalObjectRow = new List<string>();
            string key;
            string[] dprJournalObjectCols = { "ObjectDisplay", "ObjectIdentifier", "Method", "SchemaTypeName", "SequenceNumber", "IsImport" };
            //Prop ==> GetJournalObjectsProperties(uuid_journalObject)


            foreach (var failedObj in failure.dprJournalObjects)
            {
                dprJournalObjectRow.Clear();
                dprJournalObjectRow.Add(failedObj.dtTimestampStart.ToString("G"));
                dprJournalObjectRow.AddRange(dprJournalObjectCols.Select(col => StringHelper.TranslateLds(failedObj.Get(col))).ToArray());

                var propResult = GetJournalObjectsProperties(failedObj.uuid);

                foreach(var prop in propResult)
                {
                    key = $"{prop.Key}@{failedObj.uuid}";
                    var finalRow = dprJournalObjectRow.Select(i => i).Union(prop.Value);

                    result.Add(new KeyValuePair<string, string[]>(key, finalRow.ToArray()));
                }
            }

            return result;
        }

    } //DprJournal


    public class DprJournalSetupElems: SqlObject
    {
        public List<DprJournalSetupElems> childElems = new List<DprJournalSetupElems>();
        public bool isTop;

        public DprJournalSetupElems() : base("dprjournalsetup")
        { }

        public List<KeyValuePair<string, string[]>> GetSetupElements(string[] columns)
        {
            List<KeyValuePair<string, string[]>> result = new List<KeyValuePair<string, string[]>>();

            result.Add(new KeyValuePair<string, string[]>(
                this.uuid,
                columns.Select(c => Get(c)).ToArray()
             ));

            foreach (var childElem in childElems)
                result.AddRange(childElem.GetSetupElements(columns));

            return result;
        }
    }


    public class DprJournalObject: SqlObject
    {
        public List<DprJournalProperty> dprJournalProperties = new List<DprJournalProperty>(); 

        public DprJournalObject() : base("dprjournalobject")
        { }     
    }


    public class DprJournalProperty : SqlObject
    {
        public DprJournalProperty() : base("dprjournalproperty")
        { }
    }


    public class DprJournalFailure: SqlObject
    {
        public List<DprJournalObject> dprJournalObjects = new List<DprJournalObject>();

        public DprJournalFailure() : base("dprjournalfailure")
        { }
    }


    public class DprJournalMessage: SqlObject
    {
        public DprJournalMessage() : base("dprjournalmessage")
        { }
    }

}
