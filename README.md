# Final_project_cosc519
Final Project for COSC 519 (J)

# **Courtyard Reflections: A VR Experience on Academic Stress and Support**

*A COSC 519 / IMTC 505 Final Project*

---

## **Overview**

**Courtyard Reflections** is an immersive VR experience that simulates the gradual escalation of academic stress in a realistic university setting. Set in the UBC Okanagan courtyard, users begin their day with a calm social interaction, encounter increasing academic responsibilities, and eventually reach a point of cognitive overload before transitioning into a guided phase of support and awareness.

The project uses narrative-driven design, stylized NPCs, environmental reactivity, and expressive choice-making to externalize internal emotional states. The experience follows the **Kishotenketsu narrative structure** and applies theory from **narrative studies, human–computer interaction, and stress psychology**.

This repository contains the full Unity project, narrative scripts, and implementation documentation.

---

## **Demo and Presentation**

### ▶ **Project Demo Video**

[Watch on YouTube](https://youtu.be/KvppszXMVfc)

### 📘 **Demo Presentation Slides**

[View Presentation](https://docs.google.com/presentation/d/1XcXwcH48-MGRW0NkAljYYibK1S0jH6Q0O4Gb1UXwhXo/edit?usp=sharing)

--- 

## **Core Features**

### **Narrative System**

* Multi-act storyline following the Kishotenketsu structure
* Branching expressive choices that influence tone, not structural outcomes

### **Stress and Overload Simulation**

* Dynamic vignette, chromatic aberration, grain, and heartbeat intensity
* Internal monologue system reflecting cognitive load

### **NPC Interaction**

* Stylized, color-coded NPC silhouettes designed to avoid uncanny valley effects
* Dialogue-driven emotional framing aligned with stress psychology
* Interaction triggers based on XR proximity and gaze

### **Task Management System**

* Phone-based task list with capacity constraints
* Forced prioritization to model academic and TA workload accumulation
* Real-time stress effects tied to scheduling events

### **Resolution and Support Phase**

* Grounding visuals and calm audio environments
* Awareness messaging aligned with mental health design practices
* Resource panels linking to UBC wellness services

---

## **Act Summary**

### **Act 0: Orientation and Social Baseline**  
The user is introduced to the UBCO courtyard with a calm voiceover that emphasizes comfort and control. A friend invites the user to a small evening event. The user can commit or express uncertainty, establishing an emotionally grounded starting point.

### **Act 1: The First Crack**  
The same friend returns with news that a lab report deadline has suddenly been moved to the next day. The user may respond with overwhelm or avoidance. Environmental cues darken slightly, and a new task appears on the phone, creating the first signs of academic strain.

### **Act 2: Rising Pressure**  
On the way to class, the professor informs the user that TA grading must be completed tonight. Regardless of how the user responds, the workload increases. The environment becomes more tense and the task list reaches maximum load.

### **Act 3: Overload**  
Notifications erupt uncontrollably, intrusive whispers intensify, and environmental distortion peaks. The user may express frantic urgency or shutdown, although both lead to cognitive overload. The sequence culminates in a moment of silence and internal acknowledgment of collapse.

### **Act 4: Awareness and Relief**  
The environment softens and a supportive voiceover reframes the experience as common and valid. A resource panel highlights UBC wellness services, and optional breathing guidance helps the user regulate. The act resolves the emotional arc with grounding and support

---

## **Project Structure**

```
CourtyardReflections/
│
├── Assets/
│   ├── Audio/              # Background music, dialogue clips, sound effects
│   ├── Prefabs/            # Some game object prefabs (interaction rings, paper, materials)
│   ├── Scenes/             # Main VR scene and testing environments
│   ├── Scripts/            # Core game logic, dialogue system, stress mechanics, task management
│   ├── UI/                 # Phone interface, notification system, dialogue interface
│   ├── Vamporium Language/ # Help paper texture
│   └── ZS_Assets/          # NPC models and custom scripts for anxiety manager by Zhehao Sun
│
├── Docs/
│   ├── FinalReport.pdf
│   ├── TechnicalDocumentation.pdf
│   └── PresentationSlides.pdf
│
└── README.md
```

---

## **Installation and Setup**

### **Requirements**

* Unity **2022.x** or later
* XR Interaction Toolkit
* OpenXR Plugin enabled
* VR headset (Meta Quest, Vive, Index, or PCVR configuration)

### **Setup Steps**

1. Clone the repository

   ```
   git clone <your-repo-link>
   ```
2. Open the project in Unity 2022.x
3. In Project Settings, confirm:

   * XR Plug-in Management is enabled
   * OpenXR is the active runtime
4. Load the scene:

   ```
   Scenes/updated_scene_a.unity
   ```
5. Press Play or build to headset

---

## **How to Play**

1. Start in a calm onboarding environment
2. Interact with NPCs by approaching them and selecting dialogue options
3. Manage tasks through the VR phone interface
4. Experience stress escalation through environmental effects
5. Reach the overload moment and proceed to the awareness phase
6. Explore resource panels and exit the experience grounded

---

## **Design Principles**

* **Expressive but controlled agency**: Choices reflect emotion rather than branch the storyline
* **Stylized NPCs**: Color-coded silhouettes avoid uncanny valley effects and support emotional clarity
* **Environmental storytelling**: Lighting, audio, and visual distortions communicate internal states
* **Psychologically informed pacing**: Stress escalating gradually before resolution and grounding

---

## **Team Members**

* **Sadia Ahmmed**
* **Zhehao Sun**
* **Aarav Gosalia**

Instructor: **Dr. Pourang Irani**

---

## **License**

MIT License

---

## **Acknowledgments**

* UBC Okanagan
* COSC 519 / IMTC 505 course staff
* Research foundations in interactive narrative, XR design, and mental-health HCI
