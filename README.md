# Indexer - Team project 9@KISI'2023

Indexer is an application for manual image labeling.

This application was worked on during Team Project course at Gdańsk University of Technology.

## Building

**.NET SDK 7.0 required**

This project can be built with the command:
```
dotnet build
```
Alternatively, you can open the solution file (`PG_INF_PG_9KISI2023.sln`) in Visual Studio 2022
and run the build from there.

After the build is finished, its output can be found in `artifacts/bin/Indexer/Debug` directory.

## Building for deployment

This project can be built for deployment with the command:
```
dotnet publish -c Release
```
After the build is finished, the application file is available at `artifacts/publish/Indexer/Release/Indexer.exe`.

## License

Distributed under the MIT License. See `LICENSE` for more information.

---

> Jakub Kuczys &nbsp;&middot;&nbsp;
> GitHub [@Jackenmen](https://github.com/Jackenmen)<br>
> Mikołaj Morozowski<br>
> Mateusz Kozak<br>
> Mikołaj Nadzieja<br>
> Dawid Łydka
