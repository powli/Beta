 FROM microsoft/dotnet-framework:4.6.2
 WORKDIR /app
 COPY Beta/bin/Release .
 ENTRYPOINT ["Beta.exe"]