using System;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson.Serialization.IdGenerators;

namespace LoginServer.DataBase.Entities
{
    public class Session
    {
        [BsonId(IdGenerator = typeof(ObjectIdGenerator))]
        public ObjectId Id { set; get; }
        
        [BsonElement("uid")]
        public string AccountId { set; get; }
        
        
        [BsonElement("token")]
        public string Token { set; get; }
        
        [BsonElement("expire_time")]
        [BsonDateTimeOptions(Kind = DateTimeKind.Utc)]
        public DateTime ExpireTime { set; get; }
        
    }
}