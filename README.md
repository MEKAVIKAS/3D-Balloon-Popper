# 🎈 3D Balloon Popper

A complete, polished, playable 3D Balloon Popper game developed in **Unity 6** using the **Universal Render Pipeline (URP)** and **C#**.

---

## 🎮 Game Overview & Features

- **Dynamic Balloon Spawning:** Colorful balloons (Red, Blue, Yellow, Green, Purple) spawn randomly across the bottom of the screen.
- **Varied Float Speeds:** Balloons rise at randomized velocities.
- **Mouse-Click Popping Mechanic:** Mouse-click balloon popping using Unity's `OnMouseDown` method and Collider components.
- **Particle & Visual Feedback:** Dynamic popping particle effects that inherit the color of the popped balloon.
- **Score & Countdown Timer:** Real-time HUD showing current score and a 60-second countdown timer.
- **Game State Management:** Centralized `GameManager` handling round start, active gameplay, game over cleanup, and restart.
- **Main Menu & Game Over Screen:** Clean UI built with TextMeshPro, complete with instructions, score review, and restart / main menu transitions.

---

## 🛠️ Technical Specifications

- **Engine Version:** Unity 6 (`6000.6.7f1`)
- **Render Pipeline:** Universal Render Pipeline (URP)
- **Programming Language:** C#
- **Input Handling:** Both Input Systems enabled (`activeInputHandler: 2`, supporting `OnMouseDown` collision events alongside uGUI / Input System)
- **Target Platform:** Desktop macOS (Apple Silicon) / Cross-platform
- **UI Framework:** Unity UI (uGUI) & TextMeshPro

---

## 📁 Repository Structure

```
Assets/
├── Editor/          # Automation setup scripts (BalloonPopperSetup.cs)
├── Materials/       # URP materials for balloons, background, and particles
├── Particles/       # Pop effect particle prefab
├── Prefabs/         # Balloon prefab with colliders and controllers
├── Scenes/          # MainMenu.unity and GameScene.unity
├── Scripts/         # C# game logic scripts (GameManager, BalloonController, BalloonSpawner, UIManager, etc.)
└── Settings/        # URP configuration and volume profiles
Packages/            # Unity package dependencies and manifest
ProjectSettings/     # Project configuration, physics, graphics, and tag settings
```

> **Note:** Cache and build folders (`Library/`, `Temp/`, `Logs/`, `UserSettings/`) are excluded via `.gitignore` to maintain a clean and lightweight repository.

---

## 🚀 How to Run the Project

1. Clone or download this repository.
2. Open **Unity Hub**.
3. Click **Add** -> **Add project from disk** and select the repository root folder (`3D-Balloon-Popper`).
4. Ensure the Unity Editor version matches **Unity 6** (e.g. `6000.6.7f1`).
5. Open the project.
6. In the Project window, navigate to `Assets/Scenes/` and double-click `MainMenu.unity`.
7. Press the **Play** button (`▶`) at the top of the Unity Editor to play!

---

## 🎯 Gameplay Controls

- **Left Mouse Click:** Click on floating balloons to pop them and increase score.
- **UI Buttons:** Click on START GAME, RESTART, and MAIN MENU buttons to navigate.
