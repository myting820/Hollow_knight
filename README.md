# Hollow Knight (Unity & C#)

## Overview
This project is a high-complexity **2D action platformer** developed using the **Unity Engine** and **C#**. It serves as a validation of **Object-Oriented Programming (OOP)** principles and **Finite State Machine (FSM)** architectures in a dynamic game environment. The system features a precise physics-driven character controller, diverse enemy Al, and a robust global state management system.

🎬 **[Watch Gameplay Demo](https://youtu.be/KWVxwBmTp5U)**

---

## Key Features

### Advanced Player Kinematics
* **Physics-Driven Controller:** Utilizes `Rigidbody2D` and force-based movement for a weighted, responsive feel.
* **Double Jump & Dash:** Implements state-based secondary jumping and **Coroutine-managed dash** with temporary invincibility frames (i-frames).
* **Recoil Mechanics (Pogo):** Features a downward strike mechanic that triggers recoil feedback upon colliding with enemies, enabling advanced traversal.
* **Dynamic Knockback:** Calculates repulsion vectors to prevent "stunlock" and ensure gameplay fairness.

### Intelligent Enemy AI (FSM)
* **Boss: Dark Fairy:** A multi-stage encounter driven by a **Finite State Machine**. Features complex behavior including teleportation, magic orb casting, and anti-close-quarters logic.
* **Enemy Variety:**
    * **Ground Patroller:** Edge detection via Raycasting.
    * **Self-Destructing Crawler:** Proximity-based state switching (Chase → Detonate).
    * **Ranged Flying Unit:** Altitude-locking logic with vector-based projectile tracking.

### System Architecture
* **Global State Management:** Utilizes the **Singleton Pattern** and `DontDestroyOnLoad` for persistent game states (respawn points, HP/Mana) across scenes.
* **Robust Game Loop:** Implements a "Liveness-First" strategy to prevent logical anomalies during state transitions.
* **Object Lifecycle Management:** Integrated an **asynchronous reset mechanism** using Coroutines to resolve state synchronization conflicts during player respawns.

---

## Tech Stack
* **Engine:** Unity 6 (2023 LTS)
* **Language:** C#
* **Design Patterns:** Singleton Pattern, Finite State Machine (FSM), Observer-like registry.
* **APIs Used:** Physics2D (Raycast, OverlapCircle), Coroutines, Rigidbody Dynamics.

---

## Project Structure
* `GameManager.cs`: Central nervous system handling respawns and global flags.
* `PlayerController.cs`: Physics-based kinematics and input distribution.
* `DarkFairyBoss.cs`: FSM logic and Coroutine-based action sequences.
* `EnemyAI.cs`: Polymorphic behaviors for various hostile units.
