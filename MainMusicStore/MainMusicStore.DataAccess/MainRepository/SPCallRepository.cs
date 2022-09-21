using Dapper;
using MainMusicStore.Data;
using MainMusicStore.DataAccess.IMainRepository;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MainMusicStore.DataAccess.MainRepository
{
    public class SPCallRepository : ISPCallRepository
    {
        private readonly ApplicationDbContext _db;
        private static string connextionString = "";

        public SPCallRepository(ApplicationDbContext db)
        {
            _db = db;
            connextionString = db.Database.GetDbConnection().ConnectionString;
        }

        public void Dispose()
        {
            _db.Dispose();
        }

        public void Execute(string procedureName, DynamicParameters parameters = null)
        {
            using (SqlConnection sqlCon = new SqlConnection(connextionString))
            {
                sqlCon.Open();
                sqlCon.Execute(procedureName, parameters);
            }
        }

        public IEnumerable<T> List<T>(string procedureName, DynamicParameters parameters = null)
        {
            using (SqlConnection sqlCon = new SqlConnection(connextionString))
            {
                sqlCon.Open();
                return sqlCon.Query<T>(procedureName, parameters);
            }
        }

        public Tuple<IEnumerable<T1>, IEnumerable<T2>> List<T1, T2>(string procedureName, DynamicParameters parameters = null)
        {
            using (SqlConnection sqlCon = new SqlConnection(connextionString))
            {
                sqlCon.Open();
                var result = SqlMapper.QueryMultiple(sqlCon, procedureName, parameters);
                var item1 = result.Read<T1>().ToList();
                var item2 = result.Read<T2>().ToList();
                if (item1 != null && item2 != null)
                {
                    return new Tuple<IEnumerable<T1>, IEnumerable<T2>>(item1, item2);
                }
            }
            return new Tuple<IEnumerable<T1>, IEnumerable<T2>>(new List<T1>(), new List<T2>());
        }

        public T OneRecord<T>(string procedureName, DynamicParameters parameters = null)
        {
            using (SqlConnection sqlCon = new SqlConnection(connextionString))
            {
                sqlCon.Open();
                var value = sqlCon.Query<T>(procedureName, parameters);
                return (T)Convert.ChangeType(value.FirstOrDefault(), typeof(T));
            }
        }

        public T Single<T>(string procedureName, DynamicParameters parameters = null)
        {
            using (SqlConnection sqlCon = new SqlConnection(connextionString))
            {
                sqlCon.Open();
                return (T)Convert.ChangeType(sqlCon.ExecuteScalar<T>(procedureName, parameters), typeof(T));
            }
        }
    }
}
