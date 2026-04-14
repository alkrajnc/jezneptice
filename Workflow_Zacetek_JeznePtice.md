# 🐦 Jezne Ptice — Workflow za Začetek Razvoja

**Skupina:** Jezne Ptice  
**Člani:** Matej Dolinšek, Aljaž Krajnc, Jure Vidmar, Tilen Urbanc  
**Okolje:** Unity 2D | GitHub | Notion | Discord

---

## 1. GitHub Setup (naredite to najprej — vsi skupaj)

### .gitignore za Unity
V root repozitorija **mora** biti pravilni `.gitignore`, sicer se boste stepli z merganjem Unity meta datotek.

Pojdite na [gitignore.io](https://www.toptal.com/developers/gitignore/api/unity) ali dodajte ročno:

```
# Unity
[Ll]ibrary/
[Tt]emp/
[Oo]bj/
[Bb]uild/
[Bb]uilds/
[Ll]ogs/
[Uu]ser[Ss]ettings/
*.csproj
*.unityproj
*.sln
*.suo
*.tmp
*.user
*.userprefs
*.pidb
*.booproj
*.svd
*.pdb
*.mdb
*.opendb
*.VC.db
.DS_Store
```

> ⚠️ Brez tega bo vsak push/pull povzročil konflikte v `/Library/` mapi.

### Branch strategija (priporočeno)
```
main          ← samo delujoče verzije (merge samo ko je build stable)
develop       ← aktivni razvoj, sem mergate feature branche
feature/fraca
feature/fizika-trkov
feature/logika-nivojev
feature/grafika-scena
feature/ui-meniji
```

**Vsak dela na svojem feature branchu, merge v `develop` ko je stvar dokončana.**

---

## 2. Struktura Map v Unity Projektu

Ustvarite to strukturo v `/Assets/`:

```
Assets/
├── _Scenes/
│   ├── MainMenu.unity
│   ├── LevelSelect.unity
│   └── GameScene.unity
├── Scripts/
│   ├── Core/
│   │   ├── GameManager.cs
│   │   └── LevelManager.cs
│   ├── Physics/
│   │   ├── SlingshotController.cs
│   │   └── BirdProjectile.cs
│   ├── Enemies/
│   │   └── PigController.cs
│   ├── Blocks/
│   │   └── BlockDamage.cs
│   ├── UI/
│   │   └── HUD.cs
│   └── Data/
│       └── LevelData.cs
├── Prefabs/
│   ├── Birds/
│   ├── Pigs/
│   ├── Blocks/
│   └── UI/
├── Sprites/
│   ├── Birds/
│   ├── Pigs/
│   ├── Blocks/
│   ├── Environment/
│   └── UI/
├── Audio/
│   ├── Music/
│   └── SFX/
└── Data/
    └── Levels/           ← JSON datoteke nivojev
```

> Naredite to strukturo **en član** in jo pushne na `develop`. Potem vsi pullajo.

---

## 3. Sprint 1 — Razdelitev dela (1-2 tedna)

Cilj prvega sprinta je **delujoč core gameplay loop**: ptica leti iz frače in zadane blok.

### Matej ali Aljaž — Frača & Fizika
- [ ] Ustvari `SlingshotController.cs`
  - Drag z miško (LineRenderer za gumico)
  - Izračun sile in kota iz razdalje vleka
  - Izstrel ptice z `Rigidbody2D.AddForce()`
- [ ] Dodaj `TrajectoryPreview.cs` — prikaži prekinjeno pot s `Physics2D.Simulate()`
- [ ] Nastavi `Rigidbody2D`, `CircleCollider2D` na Bird prefabu

### Jure — Logika igre (osnova)
- [ ] `GameManager.cs` — stanje igre (Playing, LevelComplete, GameOver)
- [ ] `LevelData.cs` — C# klasa + JSON format za nivo
  ```json
  {
    "levelId": 1,
    "birds": ["red", "red", "yellow"],
    "blocks": [{"type": "wood", "x": 5, "y": 0.5}],
    "pigs": [{"x": 5, "y": 1}]
  }
  ```
- [ ] `LevelManager.cs` — naloži JSON, spawna objekte

### Tilen — Grafika & Scena
- [ ] Naredi osnovno GameScene: nebo, trava, podlaga (po barvni paleti iz poročila)
- [ ] Naredi placeholder spriteove za ptičo, prašiča, bloke (les, kamen, led — barve iz poročila)
- [ ] Nastavi kamero — `Cinemachine` ali ročno sledenje ptici med letom

### Skupaj ob koncu sprinta:
- [ ] Spoj vse skupaj v GameScene
- [ ] Test: ali ptica leti, zadane blok, prašič "umre"

---

## 4. Notion Kanban — Predlagana struktura

Stolpci:
| **Backlog** | **Ta teden** | **V delu** | **Review** | **Dokončano** |
|---|---|---|---|---|

Oznake (tags):
- 🔴 Fizika
- 🔵 Logika
- 🟢 Grafika
- 🟡 UI/Zvok

Vsaka kartica naj vsebuje: opis, odgovorno osebo, branch ime.

---

## 5. Vrstni red sistemov (celoten projekt)

```
Sprint 1:  Frača + fizika izstrela + osnovna scena
Sprint 2:  Sistem trkov + rušenje blokov + prašiči + točkovanje
Sprint 3:  JSON nivoji + level select + shranjevanje napredka
Sprint 4:  Posebne moči ptičev + parallax ozadje + efekti
Sprint 5:  UI (meniji, HUD, zvezde) + zvok + polish
Sprint 6:  Testiranje, bugfixi, build
```

---

## 6. Pravila za Git (da se ne stepete)

1. **Nikoli ne commitaj direktno v `main` ali `develop`** — vedno feature branch
2. Preden začneš delati: `git pull origin develop`
3. Commit sporočila naj bodo opisna: `feat: dodaj trajektorijo frače` ne `update`
4. Ko končaš feature: odpri **Pull Request** v `develop`, en član pregleda
5. **Unity Scenes** — delajte vsak svojo sceno, merge scenov je paklenski

---

## 7. Takojšnji naslednji koraki (danes/jutri)

1. ✅ En član preveri/doda `.gitignore` za Unity in pushe
2. ✅ En član naredi mapo strukturo v Assets in pushe na `develop`
3. ✅ Vsi ostali naredijo `git pull` in odprejo Unity
4. ✅ Vsakemu se dodeli feature branch iz zgornje razdelitve
5. ✅ V Notion se dodajo kartice za Sprint 1 naloge
6. ✅ Discord kanal `#git-updates` za obvestila pri pushih

---

*Ustvarjeno na podlagi poročil Naloga 2 & Naloga 3 — Jezne Ptice*
