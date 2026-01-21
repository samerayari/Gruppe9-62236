Projektets formål

Formålet med projektet er at udvikle en Minimum Viable Product (MVP), der:

- Automatiserer sortering af objekter (plast og metal)
- Reducerer manuelt og fysisk belastende arbejde
- Demonstrerer samspillet mellem OT (robot & sensorer) og IT (GUI & database)
- Understøtter Industry 4.0-principper som sporbarhed, standardisering og digital styring


Systemoversigt

Systemet består af følgende hovedkomponenter:

- UR-industrirobot styret via URScript
- To fotoelektriske sensorer (PE_350mm og PE_100mm)
- C#-applikation med grafisk brugergrænseflade (GUI)
- SQLite-database via Entity Framework Core
- Rollebaseret login-system (User / Admin)


Sorteringslogik:
- Begge sensorer aktive → Plast
- Kun PE_350mm aktiv → Metal

Beslutningen træffes i softwaren og sendes til robotten, som udfører den korrekte pick-and-place-sekvens.


GUI funktioner

Almindelig bruger:

- Starte robotprogram
- Power on / Brake release
- Stop / Emergency stop
- Overvåge systemstatus

Administrator:
- Oprette og administrere brugere
- Se databaseindhold og systemlog
- Fuld adgang til alle faner

GUI’en viser kun funktioner, der matcher brugerens rolle.


Sikkerhed

- Login kræves før adgang til systemet
- Rollebaseret adgang (least privilege)
- Adgangskoder hashes og saltes
- Bruger- og logdata gemmes i database
- Loginforsøg (succes/fejl) logges

Systemet er designet som et OT-system med fokus på kontrolleret adgang.


Database

Databasen bruges til:

- Brugeroplysninger (Account)
- Roller (admin / bruger)
- Systemlog og login-events

Adgang til databasen sker udelukkende via AppDbContext og services. GUI’en kommunikerer ikke direkte med databasen.


Test & demonstration

Systemet er testet gennem gentagne sorteringsforløb for både plast og metal.

Testen verificerer:

- Korrekt sensorregistrering
- Korrekt beslutningslogik
- Korrekte robotbevægelser
- Stabil og gentagelig drift

Resultaterne kan ses i eksamensvideoen.


Læring

Projektet har givet erfaring med:

- Regelbaseret robotstyring
- Sensor, software og robot integration
- GUI-design til industrielle systemer
- Databasesikkerhed og login-systemer
- Menneske og robot samarbejde
