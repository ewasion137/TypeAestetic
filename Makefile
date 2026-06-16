.PHONY: all clean run

all:
	dotnet publish src/TypeAestetic.csproj -c Release -r win-x64 --self-contained -p:PublishReadyToRun=true -p:PublishTrimmed=false -o build/

run:
	dotnet run --project src/TypeAestetic.csproj

clean:
	rm -rf build/ src/bin/ src/obj/