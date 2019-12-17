using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MySQLLoadBalancer
{
    public class DbLoadBalancer
    {
        private readonly List<LoadBalancedConnectionString> _loadBalancedConnectionStrings;
        private readonly int _timeToIgnoreFailedDbInMin = 5;
        private readonly string _logFullPath = string.Empty;

        /// <summary>
        /// Initializes a new instance of the <see cref="DbLoadBalancer"/> class.
        /// Accepts the candidates connection string and uses a round robin algorithm to distribute the load amound candidate DBs.
        /// </summary>
        /// <param name="connectionStrings">one or more host name (DB instances)</param>
        /// <param name="logFullPath">full path of the log file, if no path is passed through then logging will be disabled</param>
        public DbLoadBalancer(IEnumerable<string> connectionStrings, string logFullPath = "")
        {
            _loadBalancedConnectionStrings = new List<LoadBalancedConnectionString>();
            _logFullPath = logFullPath;

            foreach (string connectionString in connectionStrings)
            {
                _loadBalancedConnectionStrings.Add(new LoadBalancedConnectionString(connectionString));
            }
        }

        public DbConnection GetConnection()
        {
            string message = string.Empty;
            string lastException = string.Empty;

            foreach (var candidate in _loadBalancedConnectionStrings.OrderBy(c => c.LastTimeConnectionWasUsed))
            {
                if (!candidate.IsDbAlive && DateTime.Compare(candidate.LastTimeConnectionWasUsed, DateTime.Now) > 0)
                {
                    // The DB instance was not alive and we are going to ignore it for 5 min (_timeToIgnoreFailedDbInMin)
                    continue;
                }

                try
                {
                    MySqlConnection mySqlConnection = new MySqlConnection(candidate.ConnectionString);
                    mySqlConnection.Open();
                    candidate.LastTimeConnectionWasUsed = DateTime.Now;
                    candidate.IsDbAlive = true;
                    Log($"{DateTime.Now}: using: {candidate.ConnectionString}");
                    return mySqlConnection;
                }
                catch (Exception ex)
                {
                    candidate.LastTimeConnectionWasUsed = DateTime.Now.AddMinutes(_timeToIgnoreFailedDbInMin);    // don't use this connection for the next 5 mins
                    candidate.IsDbAlive = false;
                    message += $"{DateTime.Now}: Failed to connect to DB using: {candidate.ConnectionString}\n";
                    lastException = ex.Message;
                    Log($"{DateTime.Now.ToString()}: Failed to connect to: {candidate.ConnectionString}, marking this connection as dead for {_timeToIgnoreFailedDbInMin} min");
                }
            }

            Log($"{DateTime.Now}: All connections are dead");
            throw new Exception($"Cannot connect to any of te DB instances {message}\n Exception: {lastException}");
        }

        private void Log(string message)
        {
            if (!string.IsNullOrEmpty(_logFullPath))
            {
                using (StreamWriter writer = File.AppendText(_logFullPath))
                {
                    writer.WriteLine(message);
                }
            }
        }
    }
}
