# MySQLConnectionManagerForNet

The purpose of this project is to load balance requests among multiple hosts.

Most DbConnectors works with multi-host connection string, for example if we refer to MySQL documentation, it explains that, we can add multiple hosts to connection string, for example:

    Server=host1, host2, host3; Database=myDataBase; Uid=myUsername; Pwd=myPassword
   
The above connection string is valid, and the requests should be load balanaced among the 3 hosts. Unfortunately there are several open bugs with MySQL Connector for .Net and multi-host connection string does not work: [81650](https://bugs.mysql.com/bug.php?id=81650), [88962](https://bugs.mysql.com/bug.php?id=88962)

This project is for developers who use .NET with MySQL connector for .Net. It allows you to use a multi-host connection string and it uses a simple a round robin algorithm to distribute the requests among all hosts. If it detect that one of the hosts are down, it would ignore it for 5 mins, and then reties the host. Your connection would still work if you have at least one running DB instance.

*Note: this project does not handle the synchronization among DB instances, it is simply a load balancer which distributes the load among DB instances*

# How to use

You can define a multi-host connection string as below:

    Server=host1, host2, host3; Database=myDataBase; Uid=myUsername; Pwd=myPassword
    
Then instanciate your `DbContext` like:

````
[DbConfigurationType(typeof(MySqlEFConfiguration))]
public class MyDbContext : DbContext
{
    // instead of passing the connection string name to base constructor, use the following base construcotr  
    // public B2bDbContext() : base("DataDB")

    // MySQLConnectionManager uses round robin algorithm to choose the least recently used host and establishes a connection to that node
    // passing true for contextOwnConnection ensures that connection is terminated once MyDbContext is disposed
    public MyDbContext() : base(MySQLConnectionManager.GetDeConnection("DataDB"), true/*contextOwnConnection*/)
    {
    }

    public virtual DbSet<MyEntity1> MyEntity1 { get; set; }

    public virtual DbSet<MyEntity2> MyEntity2 { get; set; }
        
    protected override void OnModelCreating(DbModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
    }
}
````
