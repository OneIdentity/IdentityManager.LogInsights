using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using LogfileMetaAnalyser.Helpers;

namespace LogfileMetaAnalyser.Datastore
{
    public class SqlObject : DatastoreBaseDataRange
    {
        public string tablename;
        public string loggerid;
        public Dictionary<string, string> attributes = new Dictionary<string, string>(StringComparer.InvariantCultureIgnoreCase);
        public bool insertCmdWasCalled = false;

        private int _OrderSequenceId = -1;
        public int OrderSequenceId
        {
            get
            {
                if (_OrderSequenceId < 0)
                {
                    string strId = "0";
                    _OrderSequenceId = 0;

                    if (attributes.ContainsKey("SequenceNumber"))
                        strId = attributes["SequenceNumber"];
                    else if (attributes.ContainsKey("SequenceId"))
                        strId = attributes["SequenceId"];
                    else if (attributes.ContainsKey("OrderID"))
                        strId = attributes["OrderID"];
                    else if (attributes.ContainsKey("SortOrder"))
                        strId = attributes["SortOrder"];
                    else
                        return _OrderSequenceId;

                    if (!int.TryParse(strId, out _OrderSequenceId))
                        _OrderSequenceId = 0;
                }

                return _OrderSequenceId;
            }
        }

        public SqlObject(string tablename)
        {
            if (string.IsNullOrEmpty(tablename))
                throw new ArgumentNullException("table name");

            this.tablename = tablename.ToLowerInvariant();
        }

        public override string ToString()
        {
            return $"{tablename} ({GetPkValue()}) with {attributes.Count} attributes";
        }

        public void PutDataFromInsert(string attributeNameList, string attributeValueList)
        {
            try
            {
                attributes = SqlHelper.GetValuePairsFromInsertCmd(attributeNameList, attributeValueList);
            }
            catch
            {
                return;
            }

            uuid = null;
            uuid = GetPkValue();

            insertCmdWasCalled = true;
        }

        public void PutDataFromUpdate(string attributenameListWithValues)
        {
            var dictTemp = SqlHelper.GetValuePairsFromUpdateCmd(attributenameListWithValues);

            foreach (var kp in dictTemp)
                attributes.AddOrUpdate(kp.Key, kp.Value);
        }

        public string GetPkValue()
        {
            if (!string.IsNullOrEmpty(uuid))
                return uuid;

            if (attributes.Keys.Contains($"UID_{tablename}", StringComparer.InvariantCultureIgnoreCase))
                return attributes[$"UID_{tablename}"];

            return null;
        }

        public string Get(string attributeName, string defaultIfNotAvailable = "")
        {
            if (attributes.ContainsKey(attributeName))
                return attributes[attributeName];
            else
                return defaultIfNotAvailable;
        }

        public List<string> GetValueableAttributeNames(bool ordered = true)
        {
            var data = attributes.Keys.Where(k => !k.StartsWith("uid", StringComparison.InvariantCultureIgnoreCase) &&
                                                  !k.EndsWith("id", StringComparison.InvariantCultureIgnoreCase));

            if (ordered)
                return data.OrderBy(n => n).ToList();
            else
                return data.ToList();
        }

        public List<string> GetValueableAttributeValues()
        {
            List<string> res = new List<string>();

            var vipKeys = GetValueableAttributeNames(true);
            foreach (string k in vipKeys)
                res.Add(attributes[k]);

            return res;
        }
    }


}
