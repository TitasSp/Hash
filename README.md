# Hash funkcijos testavimo ataskaita

## 1. Testiniai failai

Testai buvo atlikti su failais `test/` aplanke:

* `a.txt` – vienas simbolis
* `b.txt` – vienas simbolis
* `empty.txt` – tuščias failas
* `random1.txt` – 1000 atsitiktinių simbolių
* `random2.txt` – 1000 atsitiktinių simbolių, besiskiriančių tik vienu simboliu
* `konstitucija.txt` – ilgesnis tekstas, naudojamas našumo testams

## 2. Išvedimo dydis

Hash rezultatas visada buvo **64 simbolių**, nepriklausomai nuo įvedimo.
(naudojant `output_size = 32` baitai → 64 HEX simboliai)

```
a.txt: 64 chars (expected 64)
b.txt: 64 chars (expected 64)
empty.txt: 64 chars (expected 64)
random1.txt: 64 chars (expected 64)
random2.txt: 64 chars (expected 64)
```

## 3. Deterministiškumas

Tas pats failas su tuo pačiu **globaliu salt** visada duoda tą patį hash.

```
Hashes equal? True
```

## 4. Efektyvumas

Testuota su `konstitucija.txt`, hash skaičiuotas keliems eilučių kiekiams.

| Eilučių skaičius | Vidutinis laikas (ms) |
| ---------------- | --------------------- |
| 1                | 0                     |
| 2                | 0                     |
| 4                | 0                     |
| 8                | 0                     |
| 16               | 0                     |
| 32               | 0                     |
| 64               | 0                     |
| 128              | 0.8                   |
| 256              | 2                     |

## 5. Kolizijų paieška

Iš 100 000 atsitiktinių porų kolizijų nerasta.

| String ilgis | Kolizijų skaičius / 100 000 |
| ------------ | --------------------------- |
| 10           | 0                           |
| 100          | 0                           |
| 500          | 0                           |
| 1000         | 0                           |

## 6. Lavinos efektas

Sugeneruota 100 000 porų, kurios skiriasi tik vienu simboliu.
Matuotas bitų ir hex skirtumas:

```
Bits difference: avg 50.13% (min 92, max 163)
Hex difference: avg 94.00% (min 50, max 64)
```

## 7. Negrįžtamumas (su salt)

Tas pats input (`password123`), bet kiekvieną kartą naudojant skirtingą salt, duoda visiškai kitokius hash:

```
Hash with salt 1: 436704529477985A142D08ED8BD2DB09F3C0F4643DCD8BDDC4D8892AA9F15E10
Hash with salt 2: 287DF419AA080228F7C12B658DF48A1FAC201FDFB75F35EEB4A52D65F2FFE71B
Hash with salt 3: 1030066072D2744538CE856D100AF61FACA81C26371D6BDA13D063C14BD62B24
```

## 8. Išvados

* **Stiprybės:**

  * Deterministiškas (su tuo pačiu salt visada gaunamas tas pats rezultatas).
  * Rezultato dydis pastovus, nepriklausomai nuo įvedimo ilgio.
  * Su salt galima pasiekti, kad tas pats tekstas duotų skirtingus hash’us.
  * Aiškus lavinos efektas (nedidelis pokytis inpute ženkliai keičia hash).
  * Testuose nerasta kolizijų (100 000 porų).

* **Trūkumai:**

  * Tai nėra kriptografiškai patikrintas algoritmas.
  * Kolizijų nebuvimas mažame bandymų kiekyje negarantuoja jų nebuvimo realybėje.
  * Netinka slaptažodžių saugojimui realiose sistemose.
  * Našumas mažėja didėjant failo dydžiui.

---
