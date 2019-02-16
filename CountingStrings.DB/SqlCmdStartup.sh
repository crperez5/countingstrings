sleep 120 && echo $SA_PASSWORD && echo ${SA_PASSWORD} && /opt/mssql-tools/bin/sqlcmd -S localhost -U sa -P ${SA_PASSWORD} -d master -i SqlCmdScript.sql
