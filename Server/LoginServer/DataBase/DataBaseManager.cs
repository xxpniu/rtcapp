using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using LoginServer.DataBase.Entities;
using MongoDB.Driver;
using RTC.ServerUtility;
using RTC.XNet;

namespace LoginServer.DataBase
{
    public  class DataBaseManager:XSingleton<DataBaseManager>
    {
        
        private const string AccountEntity = "db_account";

        private const string SessionEntity = "db_session";
        
        private MongoClient _client;
        private IMongoDatabase _dataBase;
        private IMongoCollection<Account> _account;
        private IMongoCollection<Session> _session;


        private async Task<Session> CreateSessionAsync(string accountId, DateTime expireDate)
        {
            var entity = new Session
            {
                AccountId = accountId,
                ExpireTime =  expireDate,
                Token = Md5Tool.GetMd5Hash( Guid.NewGuid().ToString())
            };

            var filter = Builders<Session>.Filter.Eq(t => t.AccountId , accountId);

            await _session.DeleteManyAsync(filter);

            await _session.InsertOneAsync(entity);
            
            return entity;
        }


        public async Task<bool> CheckSessionAsync(string accountId, string token,
            CancellationToken cancellationToken = default)
        {
            var bf = Builders<Session>.Filter;
            var filter = bf.Eq(t => t.AccountId, accountId);
            var query = await _session.FindAsync(filter, cancellationToken: cancellationToken);
            var session = await query.FirstOrDefaultAsync(cancellationToken: cancellationToken);
            return session?.Token == token;
        }

        public async Task InitAsync(string connectString, string db, CancellationToken token = default)
        {
            _client = new MongoClient(connectString);
            _dataBase = _client.GetDatabase(db);
            _account = _dataBase.GetCollection<Account>(AccountEntity);
            _session = _dataBase.GetCollection<Session>(SessionEntity);
            await Task.CompletedTask;
        }

        public async Task<(Account account, Session session)> CreateAccountAsync(string account, string pwd, string nikeName, string email, CancellationToken token = default)
        {
            var entity = new Account
            {
                AccountName = account,
                Email = email,
                Password = pwd,
                NikeName = nikeName,
                CreateTime = DateTime.UtcNow
            };


            var filter = Builders<Account>.Filter.Eq(t => t.AccountName, account);
            var query = await _account.FindAsync(filter, cancellationToken: token);
            if (await query.AnyAsync(cancellationToken:token ))
                throw new Exception("account is existed");
            
            await _account.InsertOneAsync(entity, cancellationToken: token);

            var session = await CreateSessionAsync(entity.AccountId, DateTime.UtcNow.AddDays(30));

            return (entity,session);
        }


        public async Task<Session> LoginAsync(string account, string pwd, CancellationToken token = default)
        {
            var filter = Builders<Account>.Filter.Eq(t => t.AccountName, account) 
                         & Builders<Account>.Filter.Eq(t=>t.Password, pwd);
            var query = await _account.FindAsync(filter, cancellationToken: token);
            
      
            var entity = await query.SingleOrDefaultAsync(cancellationToken: token);

            if (entity == null)
            {
                return null;
            }

            return await CreateSessionAsync(entity.AccountId, DateTime.UtcNow.AddDays(30));
        }
    }
}