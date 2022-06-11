using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using SupremacyData.Runtime;
using UnityEditor;
using UnityEngine;

namespace SupremacyData.Editor
{
    public class FactionsImporter
    {
        private const string Filename = "factions.csv";

        private static readonly string[] Headers =
            {   
                "id",
                "vote_price",
                "contract_reward",
                "label",
                "guild_id",
                "deleted_at",
                "updated_at",
                "created_at",
                "primary_color",
                "secondary_color",
                "background_color",
                "logo_url",
                "background_url",
                "description",
            };

        private readonly string dataPath;
        private readonly ILogInterface logger;
        
        public FactionsImporter(ILogInterface logger, string staticDataDir)
        {
            this.logger = logger;
            dataPath = Path.Join(staticDataDir, Filename);
        }

        public bool ValidateFile()
        {
            if (!File.Exists(dataPath)) return false;
            StreamReader stream;
            try
            {
                stream = File.OpenText(dataPath);
            }
            catch (Exception e)
            {
                logger.LogError($"Factions data - Could not open data file at {dataPath}");
                logger.LogError(e.ToString());
                logger.LogError(e.StackTrace);
                return false;
            }

            string headerLine;
            try
            {
                headerLine = stream.ReadLine();
            }
            catch (Exception e)
            {
                stream.Close();
                logger.LogError($"Factions data - Could not read file at {dataPath}");
                logger.LogError(e.ToString());
                logger.LogError(e.StackTrace);
                return false;                
            } 

            stream.Close();

            if (headerLine == null)
            {
                stream.Close();
                logger.LogError($"Factions data - Could not find header at {dataPath}");
                return false;                     
            }
            
            string[] headers = headerLine.Split(",");
            if (headers.Length != Headers.Length)
            {
                logger.LogError($"Factions data - header invalid in {dataPath}");
                return false;
            }

            List<string> invalidHeaders = new();
            
            for (int index = 0; index < Headers.Length; index++)
            {
                if (!string.Equals(headers[index], Headers[index], StringComparison.InvariantCultureIgnoreCase))
                {
                    invalidHeaders.Add(headers[index]);
                }
            }

            if (invalidHeaders.Count <= 0) return true;

            logger.LogError($"Factions data - Unknown headers {string.Join(",",invalidHeaders)} in {dataPath}");
            return false;
        }
        
        public async Task Update(Runtime.Data data)
        {
            if (!File.Exists(dataPath))
            {
                logger.LogError($"Could not file faction file {dataPath}");
                return;
            }

            string[] lines;
            
            try
            {
                lines = await File.ReadAllLinesAsync(dataPath);
            }
            catch (Exception e)
            {
                logger.LogError($"Factions data - Could not read data file at {dataPath}");
                logger.LogError(e.ToString());
                logger.LogError(e.StackTrace);
                return;
            }

            for (int index = 1; index < lines.Length; index++)
            {
                var line = lines[index];
                var fields = line.Split(",");
                if (!Guid.TryParse(fields[0], out Guid id))
                {
                    logger.LogError($"Factions data - Could not parse ID on line {index} from data file at {dataPath}");
                    continue;
                }

                data.factions ??= new List<Faction>();
                
                var faction = data.factions.Find(x => x.Id == id);
                if (faction == null)
                {
                    faction = ScriptableObject.CreateInstance<Faction>();
                    AssetDatabase.AddObjectToAsset(faction, data);
                    faction.id = id;
                    data.factions.Add(faction);
                }
                
                faction.humanName = fields[3];
                faction.name = $"Faction - {fields[3]}";
            }
        }
    }
}