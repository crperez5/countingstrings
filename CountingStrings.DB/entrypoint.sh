sleep ${ENV_SLEEP_TIME} && /opt/mssql-tools/bin/sqlcmd -S localhost -U sa -P "${SA_PASSWORD}" -i start.sql & /opt/mssql/bin/sqlservr 