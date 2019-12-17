using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MySQLLoadBalancer
{
    public class LoadBalancedConnectionString
    {
        public LoadBalancedConnectionString(string connectionString)
        {
            ConnectionString = connectionString;
            LastTimeConnectionWasUsed = DateTime.Now;
        }

        public string ConnectionString { get; set; }

        public DateTime LastTimeConnectionWasUsed { get; set; }
    }
}
