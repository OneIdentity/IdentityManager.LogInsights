using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;


namespace LogfileMetaAnalyser
{
    public class ExportProfiles
    {
        //pairs of profile name and profile settings
        public Dictionary<string, ExportSettings> profiles_predef = new Dictionary<string, ExportSettings>(StringComparer.InvariantCultureIgnoreCase);
        public Dictionary<string, ExportSettings> profiles_custom = new Dictionary<string, ExportSettings>(StringComparer.InvariantCultureIgnoreCase);

        private static string jsonFilename_predef = "exportprofiles_pre.json";
        private static string jsonFilename_custom = "exportprofiles_user.json";

        private Datastore.DatastoreStructure ds;

        private static readonly JsonSerializerOptions _jsonOptions = new(JsonSerializerDefaults.General)
        {
            PropertyNameCaseInsensitive = true,
            IncludeFields = true, //include public fields even without an explicit getter/setter
            WriteIndented = true, //write pretty formatted text
        };



        //constructor
        public ExportProfiles(Datastore.DatastoreStructure datastore)
        {
            ds = datastore;

            LoadFromFile();
        }

        public void LoadFromFile()
        {
            LoadFromFile(jsonFilename_predef, out profiles_predef);
            LoadFromFile(jsonFilename_custom, out profiles_custom);
        }

        private void LoadFromFile(string fn, out Dictionary<string, ExportSettings> profileLst)
        {
            profileLst = new Dictionary<string, ExportSettings>();
            ExportProfilesDump[] data = null;

            if (!System.IO.File.Exists(fn))
                return;
            
            string text = System.IO.File.ReadAllText(fn);

            data = JsonSerializer.Deserialize<ExportProfilesDump[]>(text, _jsonOptions);

            if (data != null)            
                foreach (var elem in data)
                {
                    ExportSettings expSetting = ExportSettings.FromJson(elem.ProfileContentJson);
                    expSetting.PutDatastoreRef(this.ds);
                    profileLst.Add(elem.ProfileName, expSetting);
                }            
        }

        public void SaveToFile(bool storePredefinedProfilesToo = true)
        {
            //write custom profiles in its json text file             
            var data = profiles_custom.Select(d => new ExportProfilesDump()
            {
                ProfileName = d.Key,
                ProfileContentJson = d.Value.AsJson()
            }).ToArray();

            string jsonText = JsonSerializer.Serialize(data, _jsonOptions);
            System.IO.File.WriteAllText(jsonFilename_custom, jsonText);


            //write predefined profiles in its json text file 
            if (!storePredefinedProfilesToo)
                return;

            data = profiles_predef.Select(d => new ExportProfilesDump()
                                                        {
                                                            ProfileName = d.Key,
                                                            ProfileContentJson = d.Value.AsJson()
                                                        })
                                        .ToArray();

            jsonText = JsonSerializer.Serialize(data, _jsonOptions);
            System.IO.File.WriteAllText(jsonFilename_predef, jsonText);
        }
     

        public void AddOrUpdateProfile(string profilename, ExportSettings settings, bool allowPredefine = false)
        {
            if (profiles_custom.ContainsKey(profilename))
                profiles_custom[profilename] = settings;
            else if (profiles_predef.ContainsKey(profilename) && allowPredefine)
                profiles_predef[profilename] = settings;
            else
                profiles_custom.Add(profilename, settings);

            SaveToFile();
        }

        public string DeleteProfile(string profilename, bool allowPredefine=false)
        {            
            if (profiles_custom.ContainsKey(profilename))
            {
                try
                {
                    profiles_custom.Remove(profilename);
                    SaveToFile(false);
                }
                catch (Exception E)
                {
                    return E.Message;
                }

                return "";
            }

            if (profiles_predef.ContainsKey(profilename))
            {
                if (!allowPredefine)
                    return "Deleting a predefined profile is not allowed!";

                try
                {
                    profiles_predef.Remove(profilename);
                    SaveToFile();
                }
                catch (Exception E)
                {
                    return E.Message;
                }

                return "";
            }

            return $"profile name {profilename} not found!";
        }
        
        private class ExportProfilesDump
        {
            public string ProfileName { get; set; }
            public string ProfileContentJson { get; set; }
        }


    }
    
}
