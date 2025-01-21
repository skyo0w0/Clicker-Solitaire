
# **Unity Clicker&Solitaire Demo**

This project is a demo featuring two distinct mini-games: a **card-based solitaire-like game** and a **clicker game**. It demonstrates flexible game architecture, allowing seamless switching between games, and utilizes modern Unity tools such as **DoTween**, **UniTask**, and **Addressables**.

---

## **Features**

### **Card Game Mechanics**
- Stacking rules with validation for card placement.
- Smooth card animations, including flips, movements, and undo actions using **DoTween**.
- Pyramid layout and dynamic deck arrangement.
- Efficient asset loading with **Addressables** for cards and other resources.

### **Clicker Game**
- Responsive UI with animations for click interactions.
- Particle effects and scaling animations triggered during clicks.
- Persistent score saving between sessions.

### **Flexible Game Switching**
- Simple and intuitive system to switch between the card game and the clicker game without restarting the application.
- Seamless transitions and independent state management for each game.

---

## **Technology Stack**
- **DoTween**: Smooth animations and transitions.
- **UniTask**: Asynchronous operations for improved performance and readability.
- **Addressables**: Dynamic asset management for scalability and performance optimization.

---

## **How to Run**
1. Clone the repository:
   ```bash
   git clone <repository_url>
   ```
2. Open the project in Unity (tested on Unity 2021.3 or later).
3. Play the game directly from the Unity Editor or build it for your desired platform.

---

