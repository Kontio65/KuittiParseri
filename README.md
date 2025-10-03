
# Kuittiparseri v1.0
Copyrights Juha A Virtanen 2025
Hämeenlinna
Finland

**Kuittiparseri** on C# / WPF‑sovellus, joka lukee ja jäsentää palkkakuitteja (PDF- ja TXT‑muodossa) ja kokoaa niistä selkeän yhteenvedon. Ohjelma on suunniteltu erityisesti tilanteisiin, joissa palkkalaskelmien muoto vaihtelee, mutta niistä halutaan kerätä yhtenäistä dataa esimerkiksi seurantaa tai raportointia varten.

## Ominaisuudet

- 📂 **Tiedostojen käsittely**
  - Tukee yksittäisen tiedoston avaamista
  - Tukee koko kansion (ja alikansioiden) rekursiivista läpikäyntiä
  - Hyväksyy sekä PDF‑ että TXT‑tiedostot

- 🔎 **Parserointi**
  - Tunnistaa palkkajakson ja maksupäivän useista eri muodoista:
    - Palkkajakso ja maksupäivä samalla rivillä (esim. `21.02.2022 - 06.03.2022 25.03.2022`)
    - Palkkajakso ja maksupäivä eri riveillä, myös ylimääräisen tekstin välissä
  - Poimii palkkakoodit ja niihin liittyvät tiedot (esim. `1700 Työajan lyhennysvapaa`, `10025 Pekkanen`, `61062 ...`)
  - Tukee useita eri palkkalaskelmamalleja

- 📊 **Käyttöliittymä**
  - DataGrid näyttää selkeästi:
    - Tiedoston nimen
    - Palkkajakson
    - Maksupäivän
    - Palkkakoodin
    - Selitteen
    - Määrän (tunnit)
  - ProgressBar näyttää käsittelyn etenemisen

- 📝 **Debug‑ikkuna**
  - Näyttää parserin löytämät tiedot jokaisesta tiedostosta
  - Värikoodaus: vihreä = löytyi, punainen = ei löytynyt
  - Mahdollisuus kopioida debug‑loki leikepöydälle tai tallentaa tiedostoon

- ⚙️ **Lisätoiminnot**
  - Taulukon tyhjennys yhdellä napilla
  - Tietoa‑valikko ohjelman versiosta
  - Helppo sulkeminen valikosta

## Hyödyt

- Auttaa **seuraamaan palkkakuitteja** ja varmistamaan, että kaikki tunnit ja maksut on huomioitu.  
- Vähentää manuaalista työtä, kun tiedot kerätään automaattisesti eri muotoisista kuiteista.  
- Soveltuu sekä henkilökohtaiseen käyttöön että pienten yritysten palkanlaskennan tarkistukseen.  



