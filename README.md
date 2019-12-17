# MySQLConnectionManagerForNet
Load Balanced Connection Manager for .Net, MySQL Connector for .Net does not work with multi-host connection string

The purpose of this project is to load balance requests among multiple hosts.

Most DbConnectors works with multi-host connection string, for example if we refer to MySQL documentation, it explains that, we can add multiple hosts to connection string, for example:

    Server=host1, host2, host3; Database=myDataBase; Uid=myUsername; Pwd=myPassword
   
The above connection string is valid, and the requests should be load balanaced among the 3 hosts. Unfortunately there are several open bugs with MySQL Connector .Net and multi host connection string does not work.

This project is intended to peoplw who use .NET with MySQL connector for .Net and uses a simple round robin algorithm to distribute the load among multiple hosts
