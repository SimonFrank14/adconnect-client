# Active Directory Connect Client

[![Build Status](https://dev.azure.com/schulit/Active%20Directory%20Connect%20Client/_apis/build/status/SchulIT.adconnect-client?branchName=master)](https://dev.azure.com/schulit/Active%20Directory%20Connect%20Client/_build/latest?definitionId=10&branchName=master)
![GitHub](https://img.shields.io/github/license/schulit/adconnect-client?style=flat-square)
![.NET 8](https://img.shields.io/badge/.NET%208-brightgreen?style=flat-square)

Mithilfe des Active Directory Connect Clients werden Benutzer aus dem Active Directory im Identity Provider 
vorab provisioniert. Dies ermöglicht es, Benutzer zu bearbeiten bevor sie sich initial anmelden. 

## Installation

Das aktuellste Installationspaket lässt sich [hier](https://github.com/schulit/adconnect-client/releases) herunterladen. Der Client benötigt keine
weiteren Abhängigkeiten (.NET 8 wird mit der Software mitgeliefert). 

Das Tool ist auf allen Windows-Betriebssystemen lauffähig, auf denen auch .NET 8 lauffähig ist. 
Die Installation auf einem Domaincontroller wird grundsätzlich unterstützt, jedoch nicht empfohlen. 

## Kompatibilität

Version 2 benötigt mindestens Version 1.4.0 des Single-Sign-Ons.

## Einrichtung

Nach dem ersten Start muss der Client konfiguriert werden. Siehe [Handbuch](https://adconnect-client.readthedocs.org).

## Upgrade

Das Aktualisieren des Clients ist unkompliziert. Einfach die neueste Version des Tools installieren.

## Probleme?

Dann bitte in den [Issue schauen](https://github.com/schulit/adconnect-client/issues), ob das Problem bereits bekannt ist. Falls nicht, kann dort ein Issue geöffnet werden. Support via E-Mail wird nicht angeboten.

## Lizenz

Der Quelltext steht (abgesehen von den [Icons](ICONS_LICENSE.md)) unter der [MIT License](LICENSE.md).