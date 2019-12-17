using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MySQLLoadBalancer
{

    public static class MySQLConnectionManager
    {
        private const string _server = "server";

        private static Dictionary<string, DbLoadBalancer> _dbLoadBalancers = new Dictionary<string, DbLoadBalancer>();

        static MySQLConnectionManager()
        {
            foreach (ConnectionStringSettings multiHostConnectionString in ConfigurationManager.ConnectionStrings)
            {
                DbConnectionStringBuilder dbConnectionStringBuilder = new DbConnectionStringBuilder();
                var singleHostConnectionStrings = SplitMultiHostConnectionString(multiHostConnectionString.ConnectionString);

                if (singleHostConnectionStrings.Count > 0)
                {
                    _dbLoadBalancers.Add(multiHostConnectionString.Name, new DbLoadBalancer(singleHostConnectionStrings));
                }
            }
        }

        public static DbConnection GetDeConnection(string connectionStringName)
        {
            if (_dbLoadBalancers.TryGetValue(connectionStringName, out DbLoadBalancer dbLoadBalancer))
            {
                return dbLoadBalancer.GetConnection();
            }

            throw new Exception($"Could not find any connection string with name = {connectionStringName}");
        }

        private static List<string> SplitMultiHostConnectionString(string connectionString)
        {
            List<string> singleHostConnectionString = new List<string>();

            DbConnectionStringBuilder dbConnectionStringBuilder = new DbConnectionStringBuilder()
            {
                ConnectionString = connectionString
            };

            if (dbConnectionStringBuilder.ContainsKey(_server))
            {
                string allServers = dbConnectionStringBuilder[_server] as string;
                string[] allServersArray = allServers.Split(',');

                foreach (string server in allServersArray)
                {
                    DbConnectionStringBuilder builder = new DbConnectionStringBuilder();
                    builder.ConnectionString = dbConnectionStringBuilder.ConnectionString;
                    builder[_server] = server.Trim();
                    singleHostConnectionString.Add(builder.ConnectionString);
                }
            }

            return singleHostConnectionString;
        }
    }
}
