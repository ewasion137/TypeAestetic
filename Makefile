.PHONY: all clean run

all:
	dotnet publish src/TypeAestetic.csproj -c Release -r win-x64 --self-contained -p:PublishReadyToRun=true -p:PublishTrimmed=false -o build/

run:
	dotnet run --project src/TypeAestetic.csproj

clean:
	@if exist build rmdir /s /q build
	@if exist src\bin rmdir /s /q src\bin
	@if exist src\obj rmdir /s /q src\obj