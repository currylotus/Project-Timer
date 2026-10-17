# Project Timer

Kleines Overlay für Windows: Projekte anlegen, pro Projekt eine Zeit messen, Gesamtzeit im Blick behalten. Kein Konto, kein Server, keine Telemetrie — die Daten liegen als JSON in `%AppData%\ProjectTimer\projects.json`.

## Features

- Freischaltbares, immer-im-Vordergrund-Overlay (Acrylic/Mica auf Windows 11, Fallback auf Windows 10)
- Mehrere Projekte, ein Projekt läuft zur Zeit
- Driftfreie Zeitmessung über `System.Diagnostics.Stopwatch`, Pause friert exakt ein
- Centisekunden-Anzeige, laufende Karte und Statuspunkt wechseln die Farbe
- Automatische Speicherung, läuft beim nächsten Start weiter
- Import via `build.bat publish` als einzelne EXE

## Anforderungen

- Windows 10 1809 oder neuer
- .NET 10 Desktop Runtime (bzw. .NET 10 SDK zum Bauen)

## Bauen

```
build.bat            Release-Build
build.bat run        Build und starten
build.bat clean      Artefakte entfernen
build.bat publish    Einzel-EXE nach dist\
build.bat help       Übersicht
```

Ohne Batch:

```
dotnet build -c Release
```

## Starten

```
bin\Release\net10.0-windows\ProjectTimer.exe
```

Das Fenster spawnt oben rechts, lässt sich frei ziehen und wird nicht in der Taskleiste angezeigt.

## Bedienung

| Aktion | Weg |
| --- | --- |
| Projekt anlegen | Namen eintippen, Enter oder `+` |
| Starten / pausieren | `▶` auf der Karte, Karte selbst anklicken |
| Löschen | `✕` auf der Karte |

## Daten

| Pfad | Inhalt |
| --- | --- |
| `%AppData%\ProjectTimer\projects.json` | Projekte, Zeiten, Laufzustand |

Die Datei wird alle fünf Sekunden sowie bei jeder Änderung geschrieben. Zum Zurücksetzen einfach den Ordner löschen.

## Aufbau

| Datei | Aufgabe |
| --- | --- |
| `MainWindow.xaml` | Oberfläche, Styles, Item-Template |
| `MainWindow.xaml.cs` | Ereignisse, Animationen, Zustandsabgleich |
| `Core/Project.cs` | Projektmodell mit Benachrichtigungen |
| `Core/TimerService.cs` | Uhr, Tick, Start/Pause/Reset |
| `Core/Store.cs` | Lesen und Schreiben der JSON-Datei |
| `Core/Backdrop.cs` | DWM-Attribute: dunkler Modus, Ecken, Acrylic |
| `Core/Format.cs` | Formatierung von Zeiten |
| `App.xaml` | Farbpalette und Standardschrift |