using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Analytics.Models;
using AnalyticsApi.Models;
using ChoETL;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;
using Newtonsoft.Json;

namespace AnalyticsApi.Services
{
    public class AnalyticsService
    {
        private readonly IMongoCollection<DTOModel> _analytics2;
        private ILogger<AnalyticsService> logger;
        public AnalyticsService(ILogger<AnalyticsService> logger, IAnalyticsDatabaseSettings settings)
        {
            this.logger = logger;
            var client = new MongoClient(settings.ConnectionString);
            var database = client.GetDatabase(settings.DatabaseName);

            
            _analytics2 = database.GetCollection<DTOModel>(settings.AnalyticsCollectionName);
        }

       
        public List<DTOModel> Get()
        {
            var list = _analytics2.Find(jsonData => true).ToList();
            return list;
        }

        public List<DTOModel> ConvertAndMergeModel(string filename)
        {
            
            var location = Directory.GetCurrentDirectory();

            string csv = location + "\\Upload\\files\\" + filename;
           

            Dictionary<string, List<KeyValuePair<string, string>>> outputDictionary =
                    new Dictionary<string, List<KeyValuePair<string, string>>>();

            outputDictionary = BuildModel(outputDictionary, csv);



            List<DTOModel> dtoMdls = new List<DTOModel>();
           // CultureInfo culture = new CultureInfo("fr-FR");
           
            foreach (var unit in outputDictionary)
            {
                
                DTOModel dtoMdl = new DTOModel();
                dtoMdl.DateTime =Convert.ToDateTime(unit.Key);
                dtoMdl.child = new List<ChildClass>();
                foreach (var keyValue in unit.Value)
                {
                    ChildClass child = new ChildClass();
                    child.PropertyName = keyValue.Key;
                    child.Value = int.Parse(keyValue.Value);
                    dtoMdl.child.Add(child);
                }

                var puranData = Get(dtoMdl.DateTime);
                if (puranData != null)
                {
                    foreach (var child in dtoMdl.child)
                    {
                        puranData.child.Add(child);
                    }
                    UpdateDTO(puranData.DateTime, puranData);
                }
                else
                {
                    _analytics2.InsertOne(dtoMdl);
                }
                
            }


       

            return dtoMdls;
        }
        Dictionary<string, List<KeyValuePair<string, string>>>
          BuildModel(Dictionary<string, List<KeyValuePair<string, string>>> outputDictionary, string path)
        {
            StringBuilder jsonOutput = new StringBuilder();
            using (var csvReader = new ChoCSVReader(path).WithFirstLineHeader())
            {
                using (var JsonWriter = new ChoJSONWriter(new StringWriter(jsonOutput)))
                {
                    JsonWriter.Write(csvReader);
                }

            }

            var json = jsonOutput.ToString();


            using (var reader = new JsonTextReader(new StringReader(json)))
            {
                string currentKey = "";
                string propertyName = "";
                bool isDateTimeMonth = false;
                while (reader.Read())
                {


                    if (reader.TokenType == JsonToken.PropertyName)
                    {
                        if (reader.Value.ToString().Equals("DateTime"))
                        {
                            propertyName = reader.Value.ToString();
                            isDateTimeMonth = true;
                        }

                        propertyName = reader.Value.ToString();
                    }
                    else if (reader.TokenType == JsonToken.String)
                    {
                        if (isDateTimeMonth == true)
                        {
                            currentKey = reader.Value.ToString();
                            if (!outputDictionary.ContainsKey(currentKey))
                            {
                                outputDictionary[currentKey] = new List<KeyValuePair<string, string>>();
                            }
                            isDateTimeMonth = false;
                        }
                        else
                        {

                            outputDictionary[currentKey]
                                .Add(new KeyValuePair<string, string>(propertyName, reader.Value.ToString()));
                        }

                    }

                }


                return outputDictionary;
            }
        }


        
        public DTOModel Get(DateTime datetime) => _analytics2
            .Find<DTOModel>(data => data.DateTime == datetime).FirstOrDefault();


        //public DTOModel GetDataByRange(DateTime startdatetime, DateTime enddatetime) => database._analytics2
        //   .Where<DTOModel>(x => x.DateTime >= startdatetime && x.DateTime <= enddatetime).ToList();
        public List<DTOModel> GetDataByRange(DateTime? startdate, DateTime? enddate)
        {
            var rangeData = _analytics2.Find(x => x.DateTime >= startdate && x.DateTime <= enddate).ToList();

            return rangeData;
        }


        public void UpdateDTO(DateTime datetime, DTOModel dtoModel) => _analytics2.ReplaceOne(a => a.DateTime == datetime,
            dtoModel);


       
        public void Remove(DateTime datetime) =>
            _analytics2.DeleteOne(dtomodel => dtomodel.DateTime == datetime);


    }
}