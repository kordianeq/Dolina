# Dolina - Projekt gry w Unity

## Przegląd projektu

**Dolina** to projekt gry FPS (First-Person Shooter) w silniku Unity. Jest to gra akcji z elementami przygodowymi, zawierająca system walki, zbierania przedmiotów, zarządzania zasobami gracza, sztuczną inteligencję wrogów i różne tryby gry.

## Architektura projektu

### 1. System zarządzania grą
- **GameManager.cs** - Centralny manager gry, singleton zarządzający całym stanem gry, referencje do gracza, UI, broni i kamer
- **PlayerState** - Enumeracja definiująca stany gry: Normal, Locked, Butelki, Kolejka
- **UiMenager.cs** - Zarządzanie interfejsem użytkownika

### 2. System gracza
#### Ruch i kontrola
- **PlayerMovement.cs** - System ruchu gracza z obsługą skoków, skoków kopnięć, sprintu
- **CameraControll.cs** - Kontrola kamery FPS
- **LookController.cs** - System patrzenia i obracania się gracza
- **AnimationController.cs** - Zarządzanie animacjami gracza

#### Statystyki i zdolności
- **PlayerStats.cs** - Statystyki gracza (HP, świętość, liczba przedmiotów do rzucania)
- **HandPowers.cs** - System mocy rąk
- **Meele.cs** - System ataków wręcz

### 3. System broni
- **GunSystem (GunLogic.cs)** - Zaawansowany system broni z obsługą:
  - Odsyłu
  - Rozrzutu
  - Zasięgu
  - Różnych typów amunicji (Revolver, Shotgun, Sniper)
  - Scoping
  - Ricochetu (odbicia pocisków)
  - Systemu zależnego od świętości
- **WeaponSwap.cs** - Zmiana broni
- **CustomReload.cs** - System przeładowania
- **Projectile.cs** - System pocisków
- **UnlockWeapon.cs** - Odblokowywanie broni

### 4. System zbierania przedmiotów
- **pickUp.cs** - Obsługa zbierania przedmiotów z podziałem na typy:
  - **Health** - Uzdrawianie gracza
  - **Ammo** - Amunicja do broni (różne typy)
  - **Throwable** - Przedmioty do rzucania
  - **PowerUp** - Power-upy

### 5. System wrogów i AI
- **EnemyBrain.cs** - Mózg przeciwnika z systemem stanów (State Machine)
- **MachineCore.cs** - Bazowa klasa dla systemów stanów
- **NpcBrainMaster.cs** - Zarządzanie NPC
- **Iidmgeable.cs** - Interfejs dla obiektów podatnych na obrażenia
- **PrefebScatterer.cs** - Rozrzucanie prefabrykantów wrogów

#### Systemy obrażeń
- **DamagePlayer.cs** - System zadawania obrażeń graczowi
- **DamagePlayerOnArea.cs** - Zadawanie obrażeń na obszarze
- **EnemyHurtBox.cs** - Pudełko obrażeń dla wrogów
- **HurtBox.cs** - Pudełko obrażeń ogólne
- **DmgMannager.cs** - Manager obrażeń
- **DmgWobble.cs** - Efekt wizualny przy obrażeniach

### 6. Tryby gry
- **ButelkiGameMode/** - Tryb gry "Butelki":
  - `SpawnButelki.cs` - Spawnowanie butelek
  - `ButelkiStats.cs` - Statystyki trybu
  - `GoUp.cs` - Mechanikami wznoszenia się
  
- **KolejkaGameMode/** - Tryb gry "Kolejka":
  - `Tory.cs` - Tory dla trybu Kolejka
  - `Folower.cs` - Obserwowanie toru

### 7. Systemy pomocnicze
- **Teleport.cs** - System teleportacji
- **Chceckpoint.cs** - Punkty kontrolne
- **SaveObject.cs** - System zapisywania
- **QuestManager.cs** - Manager zadań
- **NewQuest.cs** - Nowe zadania
- **SwietoscDependent.cs** - System zależny od świętości
- **RotateSpriteIn3d.cs** - Rotacja sprite'ów w 3D
- **ThrowingTutorial.cs** - Poradnik rzucania
- **PanelFader.cs** - Efekt zanikania paneli

### 8. Efekty i wizualizacja
- **SmokeVFX/** - Efekty cząsteczek dymu
- **FlatKit/** - Materiały w stylu flat
- **ToonShaderStuff/** - Shadery w stylu toon:
  - `CustomPostProcessRenderFeature.cs`
  - `CustomPostProcessPass.cs`
  - `DotsBloomTutorial.cs`

### 9. Fizyka i kolizje
- **KickBox.cs** - Pudełko dla kopnięć
- **RigMiscCore.cs** - Zarządzanie fizyką
- **VisualRigMannager.cs** - Manager wizualnej fizyką
- **BodyPartMannager.cs** - Zarządzanie częściami ciała

## Struktura folderów

```
Assets/
├── Scripts/              # Główne skrypty gry
│   ├── Ai/             # Systemy AI i wrogów
│   ├── ButelkiGameMode/ # Tryb Butelki
│   ├── KolejkaGameMode/ # Tryb Kolejka
│   ├── Misc/           # Różne narzędzia
│   └── Ui/             # Skrypty UI
├── Prefabs/            # Prefabrykaty GameObjectów
├── Scenes/             # Sceny gry
├── Models/             # Modele 3D
├── Materials/          # Materiały
├── Animations/         # Animacje
├── SFX/                # Efekty dźwiękowe
├── Sprites/            # Sprite'y
├── Resources/          # Zasoby
├── ToonShaderStuff/    # Shadery Toon
├── FlatKit/            # Materiały Flat
├── Thirdparty/         # Biblioteki trzecich stron
└── ...

```

## Kluczowe systemy i koncepty

### GameManager - Singleton Pattern
Centralny punkt kontroli, zarządzający:
- Referencjami do gracza, kamer, broni
- Stanem gry
- Pauzy gier
- Ładowaniem scen

### State Machine dla AI
Przeciwnicy używają systemu stanów do zarządzania zachowaniem:
- Śledzenie
- Ataka
- Patrole
- Unikanie

### System obrażeń
- Interfejs `IDamagable` dla obiektów podatnych na obrażenia
- Pudełka obrażeń (`HurtBox`, `EnemyHurtBox`)
- Manager obrażeń obsługujący logikę

### Świętość (Swietosc)
System afektujący zdolności gracza i broni, z wizualnym odzwierciedleniem

### System broni
- Każda broń ma swoje statystyki
- Obsługuje różne typy amunicji
- System scoping
- System ricochetu

## Technologie i zależności

- **Unity Engine** - Silnik gry
- **Physics** - Dynamika i kolizje
- **Animation** - System animacji
- **UI (TextMesh Pro)** - Interfejs użytkownika
- **Post Processing** - Efekty post-procesu
- **Universal Render Pipeline (URP)** - Potok renderowania
- **Burst** - Kompilacja JIT dla wydajności
- **Collections** - Zoptymalizowane kolekcje
- **AI Navigation** - System nawigacji AI

## Status projektu

Projekt zawiera:
- ✅ System ruchu i kontroli gracza
- ✅ Zaawansowany system broni
- ✅ System zbierania przedmiotów
- ✅ AI i system wrogów
- ✅ Wiele trybów gry
- ✅ System statystyk i umiejętności
- ✅ Interfejs użytkownika
- ✅ Efekty wizualne (Toon, Post-processing)

## Notatki dla twórców

- **Wiele skryptów zawiera polskie zmienne i komentarze** - konsystencja stylu
- **System oparty na Singleton'ach** - GameManager jako centralna kontrola
- **Rozszerzalny system AI** - State Machine umożliwia łatwe dodawanie nowych zachowań
- **Modularny system broni** - łatwa dodawanie nowych broni
- **Dużo prefabrykatów** - przygotowane obiekty do użycia w scenach

---

*Ostatnia aktualizacja: 12 grudnia 2025*
