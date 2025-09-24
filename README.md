
# StormHash — Testavimo ataskaita

## Santrauka

**StormHash** — tai blokinis, kelių raundų maišymo (mixing) algoritmas, grąžinantis 512-bitų (128 heksų simbolių) rezultatą.
Jis apjungia:

* Blokų apdorojimą (64 baitai)
* Kelis maišymo raundus su rotacijomis, daugybėmis, XOR
* Galutinį stiprų maišymą (final mix)
* Ilgio/padding’o įtraukimą

Algoritmas nėra kopija jokio standarto — jis sukurtas kaip originali, „from-scratch“ demonstracinė hash funkcija.

Toliau pateikiama idėja pseudo-kodu, atliktas eksperimentinis tyrimas ir faktiniai rezultatai.

---

## 1. Idėja (žingsnis po žingsnio — PSEUDO-KODAS)

```text
FUNCTION ComputeHash(input: bytes) -> hexstring
    IF input == null THEN input = empty

    // 1) Inicializacija
    hash[0..7] <- INITIAL_HASH (8 × 64-bit konstantos)
    inputLength <- len(input) as 64-bit
    hash[0] ^= inputLength
    hash[7] ^= (inputLength << 32)

    // 2) Blokinis apdorojimas (64 baitai)
    FOR each 64-byte chunk of input:
        words[0..7] <- chunk as 8 × 64-bit mažieji žodžiai (little-endian)
        FOR round FROM 0 TO 3:
            MixingRound(hash, words, round)
        END FOR
    END FOR

    // 3) Paskutinis užpildytas blokas (padding + len in bits)
    IF remainingBytes > 0 OR input.size == 0:
        finalChunk <- 64-byte array (copy remaining, add 0x80 pad, write bitLength at end)
        ProcessChunk(finalChunk)

    // 4) Final mixing (kelios raundų iteracijos)
    FinalMix(hash)

    // 5) Išvestis: sujungti 8 × 64-bit į 128 heksų simbolių eilutę
    RETURN HexEncode(hash[0..7])
END FUNCTION
```

```text
FUNCTION MixingRound(hash, words, round)
    temp[0..7] <- copy(hash)
    FOR i FROM 0 TO 7:
        mixed <- words[i] processed differently per round:
            CASE 0: mixed = rotl(mixed ^ PRIME1, 31) * PRIME2
            CASE 1: mixed = rotl(mixed + PRIME3, 17) ^ PRIME4
            CASE 2: mixed = (mixed * PRIME5) ^ rotl(mixed, 23)
            CASE 3: mixed = rotl(mixed ^ PRIME1, 13) + PRIME2
        temp[i] = hash[i] ^ mixed ^ rotl(hash[next], 7) ^ rotl(hash[prev], 25)
        temp[i] = rotl(temp[i], 11) * PRIME1
    END FOR

    // kryžminis maišymas į hash
    FOR i FROM 0 TO 7:
        hash[i] = temp[i] ^ temp[(i+3)%8] ^ temp[(i+5)%8]
    END FOR
END FUNCTION
```

```text
FUNCTION FinalMix(hash)
    FOR round FROM 0 TO 4:
        FOR i FROM 0 TO 7:
            hash[i] ^= hash[(i+1)%8]
            hash[i] = rotl(hash[i], 19) * PRIME1
            hash[i] ^= hash[i] >> 17
            hash[i] *= PRIME3
            hash[i] ^= hash[i] >> 13
            hash[i] *= PRIME5
            hash[i] ^= hash[i] >> 16
        END FOR

        // papildomas cross-mix per raundus
        IF round < 4:
            tmp = hash[0]
            FOR i FROM 0 TO 6:
                hash[i] ^= hash[i+1]
            END FOR
            hash[7] ^= tmp
        END IF
    END FOR
END FUNCTION
```

### Pastabos apie dizainą

* Blokinis apdorojimas ir padding leidžia saugiai maišyti arbitrarius ilgius.
* Skirtingi raundai naudoja skirtingas aritmetines/bitines operacijas, kad padidintų nelinijiškumą.
* Naudojamos 64-bit konstantos (pirminiai/deriniai) kaip round constants.
* FinalMix dar labiau sujungia visą būseną, siekiant puikaus lavinos efekto.

---

## 2. Atliktas eksperimentinis tyrimas (metodika)

* **Output size:** pastovus hash ilgis (128 heksų simbolių).
* **Determinism:** tas pats failas/hash išsaugomas 10 kartų — identiški rezultatai.
* **Performance:** pritaikyta `konstitucija.txt` (76029 simbolių) ir kartota su 1×,2×,4×,...64× dydžiais.
* **Collision testing:** 100,000 atsitiktinių string’ų porų kiekvienam ilgiui.
* **Avalanche effect:** 100,000 porų, skiriasi TIK vienu simboliu.
* **Irreversibility / salt demo & dictionary simulation:** su salt gaunami skirtingi hash’ai; imituota žodyno ataka.

---

## 3. Gauti rezultatai (išvestiniai iš testų)

### 3.1 Testing Output Size

| File             | Input size  | Hash length | Hash (truncated) |
| ---------------- | ----------- | ----------- | ---------------- |
| empty.txt        | 0 chars     | 128 chars   | 616b44b3...      |
| a.txt            | 3 chars     | 128 chars   | 71ab1ea9...      |
| b.txt            | 3 chars     | 128 chars   | 4675df6f...      |
| random1.txt      | 1231 chars  | 128 chars   | 43b296b3...      |
| random2.txt      | 1231 chars  | 128 chars   | 18e04dba...      |
| konstitucija.txt | 76029 chars | 128 chars   | 883b2205...      |

✓ All hashes have consistent 128-character length

### 3.2 Testing Determinism

```
Hash 1:  883b2205...0e76077c88
Hash 2:  883b2205...0e76077c88
Hash 10: 883b2205...0e76077c88
All 10 hashes identical: ✓ YES
```

### 3.3 Performance Testing

| Multiplier | Size (chars) | Avg time (ms) |
| ---------- | ------------ | ------------- |
| 1x         | 76029        | 3.86          |
| 2x         | 152059       | 4.24          |
| 4x         | 304119       | 9.66          |
| 8x         | 608239       | 18.88         |
| 16x        | 1216479      | 41.14         |
| 32x        | 2432959      | 81.88         |
| 64x        | 4865919      | 146.90        |

### 3.4 Collision Testing

| Length | Collisions / 100,000 | Time (ms) |
| ------ | -------------------- | --------- |
| 10     | 0                    | 614       |
| 100    | 0                    | 764       |
| 500    | 0                    | 2056      |
| 1000   | 0                    | 3673      |

→ Kolizijų neaptikta per šiuos eksperimentus.

### 3.5 Avalanche Effect

* **Bit-level differences:** Min: 0.00%, Max: 60.94%, Avg: 49.92%, Std Dev: 2.95%
* **Hex-level differences:** Min: 0.00%, Max: 100.00%, Avg: 93.61%, Std Dev: 4.22%

✓ Avalanche effect is excellent (close to ideal 50%)

### 3.6 Irreversibility Demonstration (salt + dictionary demo)

```
Using salt: StormHash_Salt_638943149391327240
Input: 'password123'
Salted: 'password123StormHash_Salt_638943149391327240'
→ Demonstrates one-way property: hash reveals no information about original input

Dictionary attack simulation:
Target hash: df4b72f3...536b0
✓ Found match: 'password'
```

> Pastaba: „Found match: 'password'“ reiškė, kad be salting’o žodyno ataka gali rasti paprastus slaptažodžius; su salt rezultatai skiriasi.

---

## 4. Išvados ir rekomendacijos

### Pagrindinės išvados

* StormHash demonstruoja labai gerą lavinos efektą (\~49.9% bitų pokytis) ir aukštą hex-simbolių pokytį (\~93.6%).
* Nėra aptiktų kolizijų intervale, kurį testavome (100k porų įvairiems ilgiams).
* Algoritmas yra deterministinis: tas pats įvesties duoda tą patį hash.
* Našumas pakankamai geras; augimas su duomenų dydžiu priimtinas (praktiškai linijinis).

### Stiprybės

* 512-bitų išvestis (didelė entropija)
* Stiprūs maišymo/mechanizmo elementai: daug raundų, permutacijos, multiplikacijos su didelėmis konstantomis, cross-mixing
* Tinka testams, moksliniams eksperimentams ir demonstracijoms

### Ribojimai ir saugumo pastabos

* Nėra oficialaus kriptografinio įvertinimo.
* Nerekomenduojama naudoti vietoje standartinių hash funkcijų kritinėse sistemose be atitinkamo auditavimo.
* Demonstracinė žodyno ataka parodė, kad salting yra privalomas slaptažodžių apdorojimui.
* Tolesni bandymai: collision search, diferencialinė analizė, statistinė entropijos analizė, GPU/ASIC testavimas.

