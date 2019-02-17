sleep 120 && /opt/mssql-tools/bin/sqlcmd -S localhost -U sa -P ${SA_PASSWORD} -d master -i SqlCmdScript.sql
