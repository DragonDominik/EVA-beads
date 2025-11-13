# Üldözős játék – Windows Forms és WPF verziók

Ez a repozitórium két megvalósítást tartalmaz ugyanarra a játékra: **Windows Forms** és **WPF**.  

## Játék leírása

- **Cél:** A játékosnak két üldözőt kell aknára csalnia anélkül, hogy ő maga aknára lépne.  
- **Pálya:** n × n-es rács (választható méret: 11×11, 15×15, 21×21).  
- **Kezdő pozíciók:**  
  - Játékos: felső sor közepén  
  - Üldözők: alsó két sarok  
- **Ellenfelek mozgása:**  
  - Időközönként közelítenek a játékos felé.  
  - Függőleges távolság nagyobb → függőleges lépés  
  - Egyébként vízszintes lépés  
  - Aknára lépve eltűnnek, de az akna megmarad.  
- **Játékos mozgása:** egységenként vízszintesen vagy függőlegesen.  

## Játékmenet

- **Győzelem:** minden üldöző aknára lép  
- **Vereség:** a játékost elkapják, vagy aknára lép  
- **Funkciók:**  
  - Új játék indítása pályaméret kiválasztásával  
  - Játék szüneteltetése  
  - Mentés és betöltés szünet alatt  
  - Játékidő folyamatos kijelzése  
  - Automatikus vége felismerés és eredmény kijelzése  

## Projekt struktúra

- `WinForms/` – Windows Forms implementáció  
- `WPF/` – WPF implementáció  
