FROM microsoft/mssql-server-linux
ARG SA_PASSWORD
ENV SA_PASSWORD ${SA_PASSWORD:-1991836789cP.1}
ENV ACCEPT_EULA=Y
COPY entrypoint.sh entrypoint.sh
COPY SqlCmdStartup.sh SqlCmdStartup.sh
COPY SqlCmdScript.sql SqlCmdScript.sql
RUN chmod +x ./SqlCmdStartup.sh
CMD /bin/bash ./entrypoint.sh $SA_PASSWORD