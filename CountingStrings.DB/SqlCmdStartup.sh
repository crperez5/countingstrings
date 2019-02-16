sleep 120 && /opt/mssql-tools/bin/sqlcmd -S localhost -U sa -P $1 -d master -i SqlCmdScript.sql
