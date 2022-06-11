using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace SupremacyData.Editor.Importers
{
    public abstract class Base
    {
        protected abstract string Filename { get; }
        protected abstract string[] Headers { get; }
        
        public abstract string ImporterName { get; }

        protected readonly string dataPath;
        protected readonly ILogInterface logger;

        public Base(ILogInterface logger, string staticDataDir)
        {
            this.logger = logger;
            // ReSharper disable once VirtualMemberCallInConstructor
            // we know what we're doing here - subclasses should use a const to feed the filename, so this should work.
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
                logger.LogError($"{ImporterName} data - Could not open data file at {dataPath}");
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
                logger.LogError($"{ImporterName} data - Could not read file at {dataPath}");
                logger.LogError(e.ToString());
                logger.LogError(e.StackTrace);
                return false;                
            } 

            stream.Close();

            if (headerLine == null)
            {
                stream.Close();
                logger.LogError($"{ImporterName} data - Could not find header at {dataPath}");
                return false;                     
            }
            
            string[] headers = headerLine.Split(",");
            if (headers.Length != Headers.Length)
            {
                logger.LogError($"{ImporterName} data - header invalid in {dataPath}");
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

            logger.LogError($"{ImporterName} data - Unknown headers {string.Join(",",invalidHeaders)} in {dataPath}");
            return false;            
        }

        public async Task Update(Runtime.Data data)
        {
            if (!File.Exists(dataPath))
            {
                logger.LogError($"Could not file faction file {dataPath}");
                return;
            }

            StreamReader reader;
            try
            {
                reader = File.OpenText(dataPath);
            }
            catch (Exception e)
            {
                logger.LogError($"{ImporterName} data - Could not read data file at {dataPath}");
                logger.LogError(e.ToString());
                logger.LogError(e.StackTrace);
                return;
            }

            int index = 0;

            SetupForImport(data);
            
            while (!reader.EndOfStream)
            {
                String line;
                try
                {
                    line = await reader.ReadLineAsync();
                }
                catch (Exception e)
                {
                    logger.LogError($"{ImporterName} data - Could not read line {index} from file at {dataPath}");
                    logger.LogError(e.ToString());
                    logger.LogError(e.StackTrace);
                    reader.Close();
                    return;                    
                }

                if (index == 0)
                {
                    index++;
                    continue;
                }
                
                var fields = line.Split(",");
                
                ProcessRecord(data, index, fields);
                
                index++;
            }

            reader.Close();
        }

        protected Guid ParseGuid(int index, string guid, string idName)
        {
            if (Guid.TryParse(guid, out var id)) return id;

            logger.LogError($"{ImporterName} data - Could not parse {idName} on line {index} from data file at {dataPath}");
            return Guid.Empty;
        }
        
        protected abstract void SetupForImport(Runtime.Data data);

        protected abstract void ProcessRecord(Runtime.Data data, int index, string[] fields);
    }
}