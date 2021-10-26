using System;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson.Serialization.IdGenerators;

namespace LoginServer.DataBase.Entities
{
    public class Account
    {
        [BsonId(IdGenerator = typeof(StringObjectIdGenerator))]
        [BsonElement("_id")]
        public string AccountId { set; get; }
        [BsonElement("account")]
        public string AccountName { set; get; }
        [BsonElement("password")]
        public string Password { set; get; }
        [BsonElement("email")]
        public string Email { set; get; }
        
        [BsonElement("nikeName")]
        public string NikeName { set; get; }
        [BsonDateTimeOptions(Kind =  DateTimeKind.Utc)]
        public DateTime CreateTime { set; get; }
    }
}