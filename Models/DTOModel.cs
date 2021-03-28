using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;

namespace AnalyticsApi.Models
{
    [BsonIgnoreExtraElements]

    public class DTOModel
    {
        [BsonDateTimeOptions(Kind = DateTimeKind.Local)]
        public DateTime DateTime { get; set; }
        public List<ChildClass> child { get; set; }
    }
}
 