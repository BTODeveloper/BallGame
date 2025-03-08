# Match-3 Physics Game

A physics-based Match-3 game developed as part of the Casual Friday home assignment.

## Technical Overview

### Architecture

This project follows a modular, component-based architecture with clear separation of concerns:

- **Core Systems**: Base classes and interfaces for scene management, initialization patterns, and game flow
- **Manager Pattern**: Each major system is controlled by a dedicated manager
- **Event-Driven Communication**: Components communicate via events rather than direct references where appropriate
- **Async Operations**: Using UniTask for asynchronous operations like scene loading and addressable assets

### Scene Initialization

- Each scene has a dedicated **GameplaySceneInitializer** as an entry point
- Scene initializers handle the ordered initialization of scene-specific managers
- **GlobalManagers** persists across scenes for cross-scene references
- The singleton-based approach was chosen as a lightweight solution without dependency injection frameworks (like ZenJect or VContainer)

### Technical Features

- **Addressable Asset System**: All game assets are loaded via Addressables for efficient memory management
- **Object Pooling**: Ball and particle systems use object pooling for performance
- **Physics-Based Gameplay**: Uses Unity physics for ball movement and collision
- **Safe Area Handling**: UI adapts to different device notches and safe areas
- **Adaptive Difficulty**: Three difficulty levels affecting gameplay parameters

### Design Patterns Used

- **Object Pool Pattern**: For efficient object reuse with balls and particles
- **Observer Pattern**: For event-based communication between components
- **Singleton Pattern (limited)**: Only for global access to critical systems via GlobalManagers
- **Command Pattern**: For input handling and user interactions
- **Component Pattern**: For modular behavior implementation

## Ball System Architecture

The game uses a modular, data-driven approach for ball configuration and management:

### Data Structure
- **BallRootType**: Enum defining the fundamental ball types (Regular, Special)
- **BallColorType**: Enum for visual variations within each type
- **BallGroupConfiguration**: ScriptableObject that defines a ball type and its properties:
  - Reference to the ball prefab
  - Detection radius for matching
  - List of visual data variations
  - Pool settings configuration

- **BallAppearanceData**: ScriptableObject for visual variants of each ball type:
  - Color type
  - Sprite reference
  - Particle effects on match

### Runtime Components
- **BallSpawnerController**: Manages spawning, pooling and placement of balls
- **Ball**: Individual ball behavior and interactions
- **BallRuntimeDataWrapper**: Runtime state container passed during initialization

This separation between configuration data and runtime components allows for:
- Designer-friendly tweaking without code changes
- Easy addition of new ball types and colors
- Efficient asset management via Addressables
- Performance optimization through object pooling

## Development Decisions

- Used DOTween for UI animations to create smooth, polished transitions
- Implemented a custom popup system with queuing for proper UI flow
- Used ScriptableObjects for configuration to support designer-friendly tweaking
- Followed clean architecture principles to maintain separation of concerns
- Optimized for mobile with appropriate object pooling and asset management
